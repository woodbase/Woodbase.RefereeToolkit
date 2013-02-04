using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Calendar.v3;
using Google.Apis.Util;
using HtmlAgilityPack;
using Woodbase.DBUNet.CalendarGrabber.Enums;
using Woodbase.DBUNet.CalendarGrabber.Objects;

namespace Woodbase.DBUNet.CalendarGrabber
{
    public class Calendar
    {
        public IEnumerable<Game> GrabMatchList(CookieContainer cookies)
        {
            var formUrl = "http://dbunet.dbu.dk/public/refereeMatchProgram.aspx";
            var req = (HttpWebRequest)WebRequest.Create(formUrl);
            req.Method = "GET";
            req.CookieContainer = cookies;
            var resp = req.GetResponse() as HttpWebResponse;
            var loginPage = new HtmlDocument();
            loginPage.Load(resp.GetResponseStream());
            var matchTableHtml =
                loginPage.DocumentNode.SelectNodes("//table[@id='uc_refereeMatchProgram_dgMatchCategory1']//tr[@class!='DataGridHeaderStyle']");

            var matches = matchTableHtml.Select(x => new Game()
                                                        {
                                                            GameNo = GetGameNumber(x),
                                                            Round = GetRoundNumber(x),
                                                            GameDayShort = GetGameDayShort(x),
                                                            GameDay =
                                                                GetMatchDate(
                                                                    x.SelectSingleNode(".//td[position()=5]").InnerText.
                                                                        Trim()),
                                                            Row = GetRowInfo(x),
                                                            Location =
                                                                new Location()
                                                                    {
                                                                        Name = GetLocationName(x)
                                                                    },
                                                            HomeTeam =
                                                                new Team()
                                                                    {
                                                                        Name =
                                                                            GetTeamName(x, true)
                                                                    },
                                                            AwayTeam =
                                                                new Team()
                                                                    {
                                                                        Name =
                                                                            GetTeamName(x, false)
                                                                    },
                                                            Role =
                                                                (Role)
                                                                x.SelectSingleNode(".//td[position()=14]").InnerText.
                                                                    Trim().ToCharArray()[0],
                                                            DrivingInfo = new DrivingInfo(x.SelectSingleNode(".//td/a[position()=1]").GetAttributeValue("href", ""), cookies),
                                                            MatchType = (x.SelectSingleNode(".//td[position()=13]") == null || string.IsNullOrEmpty(x.SelectSingleNode(".//td[position()=13]").InnerText.Trim())) ? 'N' : x.SelectSingleNode(".//td[position()=13]").InnerText.
                                                                    Trim().ToCharArray()[0]
                                                        });

            return matches;
        }

        private string GetLocationName(HtmlNode htmlNode)
        {
            return Regex.Split(htmlNode.SelectSingleNode(".//td[position()=6]").
                                   InnerText.Trim(), "\r\n")[1].Trim().Replace("&nbsp;", "");
        }

        private string GetRowInfo(HtmlNode htmlNode)
        {
            return Regex.Split(htmlNode.SelectSingleNode(".//td[position()=6]").
                                   InnerText.Trim(), "\r\n")[0].Trim().Replace("&nbsp;", "");
        }

        private MatchDayShort GetGameDayShort(HtmlNode htmlNode)
        {
            return (MatchDayShort)
                Enum.Parse(typeof (MatchDayShort), htmlNode.SelectSingleNode(".//td[position()=4]").InnerText, true);
        }

        private int GetRoundNumber(HtmlNode htmlNode)
        {
            return string.IsNullOrEmpty(htmlNode.SelectSingleNode(".//td[position()=3]").InnerText)
                ? 0
                : int.Parse(
                    htmlNode.SelectSingleNode(".//td[position()=3]").InnerText);
        }

        private int GetGameNumber(HtmlNode htmlNode)
        {
            return int.Parse(htmlNode.SelectSingleNode(".//td/a[position()=1]").InnerText);
        }

        private string GetTeamName(HtmlNode htmlNode, bool homeTeam)
        {
            var teamsinfo = Regex.Split(htmlNode.SelectSingleNode(".//td[position()=7]").
                                   InnerText.Trim(), "\r\n");
            if (homeTeam)
            {
                return teamsinfo.First().Trim();
            }
            else
            {
                return teamsinfo.Last(x => x.Trim() != "&nbsp;").Trim();
            }
        }

        public CalendarListResource.GetRequest GetCalenders()
        {
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description);
            provider.ClientIdentifier = "36225438087.apps.googleusercontent.com";
            provider.ClientSecret = "t437LrazWglB2v_OeAqMaj-r";
            var auth = new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthorization);
            // Create the service. This will automatically call the previously registered authenticator.
            auth.LoadAccessToken();
            var service = new CalendarService(auth);
            //auth code
            // 4/USRsNMQpLCUgsr2cv1VZM0U-cx5t.ohfarZngF4kVOl05ti8ZT3YdL5qWcwI
            var calendar = service.CalendarList.Get("martin.skov.nielsen@gmail.com");
            var oAuth = calendar.Oauth_token;

            return calendar;
        }

        private DateTime GetMatchDate(string dateString)
        {
            var date = dateString.Substring(0, 10);
            var time = dateString.Substring(dateString.Length - 5);
            return Convert.ToDateTime(date + " " + time);
        }

        private static IAuthorizationState GetAuthorization(NativeApplicationClient arg)
        {
            // Get the auth URL:
            IAuthorizationState state = new AuthorizationState(new[] { CalendarService.Scopes.Calendar.GetStringValue() });
            state.Callback = new Uri(NativeApplicationClient.OutOfBandCallbackUrl);
            Uri authUri = arg.RequestUserAuthorization(state);

            // Request authorization from the user (by opening a browser window):
            Process.Start(authUri.ToString());
            string authCode = "4/USRsNMQpLCUgsr2cv1VZM0U-cx5t.ohfarZngF4kVOl05ti8ZT3YdL5qWcwI";// Console.ReadLine();
            //Console.WriteLine();

            // Retrieve the access token by using the authorization code:
            return arg.ProcessUserAuthorization(authCode, state);
        }

    }
}
