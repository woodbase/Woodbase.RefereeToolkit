using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using Woodbase.DBUNet.CalendarGrabber.Enums;
using Woodbase.DBUNet.CalendarGrabber.Interfaces;
using Woodbase.DBUNet.CalendarGrabber.Objects;

namespace Woodbase.DBUNet.CalendarGrabber
{
    public class Informer
    {
        private readonly IDataConnector _connector;
        private readonly string _userName;

        public Informer(IDataConnector connector, string userName)
        {
            _connector = connector;
            _userName = userName;
        }

        public void SendNotifications(List<Game> matches)
        {
            var removedMatches = _connector.CheckRemovedMatch(matches.Select(x => x.GameNo).ToList());
            foreach (var match in matches)
            {
                _connector.SaveToDB(match.GameNo, match.GameDay, match.Location, match.Role, match.GameDayShort, match.MatchType, match.Round, match.Row, match.DrivingInfo, match.HomeTeam, match.AwayTeam);
            }
            SendEmail(_connector.GetEmails(_userName), matches, removedMatches);
        }



        private void SendEmail(IEnumerable<string> mailAddress, IEnumerable<Game> matches, List<Game> removedGames)
        {
            var alreadySent = _connector.GetSentGameId();
            var notSentGames = matches.Where(x => !alreadySent.Contains(x.GameNo)).Select(x => x).ToList();
            var attachment = CreateCalendarEvent(notSentGames);
            var msg = new MailMessage { From = new MailAddress("martin.skov.nielsen@gmail.com", "Martin Skov Nielsen") };
            var body = new StringBuilder();
            var games = notSentGames as List<Game> ?? notSentGames.ToList();

            if (games.Count > 0)
            {
                body.Append("<h2>Følgende kampe er registreret som nye:</h2>");
                var messagecontent = games.Select(x => "<p><table>" +
                                                       "<tr><td>Dato:</td><td>" + x.GameDay.ToString("dd-MM-yyyy") +
                                                       "</td></tr>" +
                                                       "<tr><td>Afgang:</td><td>" +
                                                       x.GameDay.AddMinutes((60 + (x.DrivingInfo.Hours * 60) +
                                                                             x.DrivingInfo.Minutes) *
                                                                            -1).ToString("HH:mm") + (x.DrivingInfo.Minutes == 0 ? " + kørsel" : "") + "</td></tr>" +
                                                       "<tr><td>Hjemkomst:</td><td>" +
                                                       x.GameDay.AddMinutes((120 + (x.DrivingInfo.Hours * 60) +
                                                                             x.DrivingInfo.Minutes)).ToString("HH:mm") + (x.DrivingInfo.Minutes == 0 ? " + kørsel" : "") +
                                                             "</td></tr>" +
                                                       "<tr><td>Sted:</td><td>" + x.Location.Name + "</td></tr>" +
                                                       "<tr><td>Række:</td><td>" + x.Row + "</td></tr>" +
                                                       "<tr><td>Hjemmehold:</td><td>" + x.HomeTeam.Name + "</td></tr>" +
                                                       "<tr><td>Udehold:</td><td>" + x.AwayTeam.Name + "</td></tr>" +
                                                       "</table></p><br/>");
                foreach (var part in messagecontent)
                {
                    body.Append(part);
                }
            }

            if (removedGames.Count > 0)
            {
                body.Append("<h2>Følgende kampe er udgået:</h2>");
                var removeContent = removedGames.Select(x => "<p><table>" +
                                                             "<tr><td>Dato:</td><td>" +
                                                             x.GameDay.ToString("dd-MMM-yyyy") +
                                                             "</td></tr>" +
                                                             "<tr><td>Afgang:</td><td>" +
                                                             x.GameDay.AddMinutes((60 + (x.DrivingInfo.Hours * 60) +
                                                                                   x.DrivingInfo.Minutes) * -1).ToString("HH:mm") +
                                                             "</td></tr>" +
                                                             "<tr><td>Sted:</td><td>" + x.Location.Name + "</td></tr>" +
                                                             "<tr><td>Række:</td><td>" + x.Row + "</td></tr>" +
                                                             "<tr><td>Hjemmehold:</td><td>" + x.HomeTeam.Name +
                                                             "</td></tr>" +
                                                             "<tr><td>Udehold:</td><td>" + x.AwayTeam.Name +
                                                             "</td></tr>" +
                                                             "</table></p><br/>");
                foreach (var part in removeContent)
                {
                    body.Append(part);
                }
            }

            if (body.Length <= 0) return;
            msg.Body = body.ToString();
            msg.IsBodyHtml = true;
            msg.Attachments.Add(new Attachment(attachment, "Kampprogram.ics"));
            msg.Subject = "Fodbolddommer - kampprogram";
            var smtp = new SmtpClient("smtp.gmail.com", 587) { EnableSsl = true, Credentials = new NetworkCredential("<gmail-account>", "<password>", "") };
            msg.To.Add(string.Join(",", mailAddress));
            smtp.Send(msg);

            var updatedRows = _connector.SetSentGames(games.Select(x => x.GameNo));
        }

        private MemoryStream CreateCalendarEvent(List<Game> matches)
        {
            var result = new StringBuilder();
            result.Append("BEGIN:VCALENDAR{0}" +
                                "VERSION:2.0{0}" +
                                "PRODID:-//hacksw/handcal//NONSGML v1.0//EN{0}"
                                +"METHOD:PUBLISH{0}");
            var template = "BEGIN:VEVENT{5}" +
                                "UID:uid1@example.com{5}" +
                                "DTSTAMP:{0}{5}" +
                                //"ORGANIZER;CN=John Doe:MAILTO:john.doe@example.com{5}" +
                                "DTSTART:{0}{5}" +
                                "DTEND:{1}{5}" +
                                "SUMMARY:{2}{5}" +
                                "DESCRIPTION:{4}{5}" +
                                "LOCATION:{3}{5}" +
                                "END:VEVENT{5}";
            foreach (var game in matches)
            {
                var start = game.GameDay.ToString("yyyyMMdd") + "T" +
                             game.GameDay.AddMinutes((60 + (game.DrivingInfo.Hours * 60) +
                                                      game.DrivingInfo.Minutes) * -1).ToString("HHmmss") + "Z";
                var end = game.GameDay.ToString("yyyyMMdd") + "T" +
                             game.GameDay.AddMinutes((120 + (game.DrivingInfo.Hours * 60) +
                                                      game.DrivingInfo.Minutes)).ToString("HHmmss") + "Z";
                var shortDesc = "Fodbolddommer (" + game.Row + ")";
                result.Append(string.Format(template, start, end, shortDesc, game.Location.Name, game.DrivingInfo.Minutes == 0 ? "Der er ikke medregnet køretid" : "", Environment.NewLine));
            }

            result.Append("END:VCALENDAR");

            return new MemoryStream(Encoding.ASCII.GetBytes(string.Format(result.ToString(), Environment.NewLine)));
        }
    }
}
