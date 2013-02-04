using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Woodbase.DBUNet.CalendarGrabber.Enums;
using Woodbase.DBUNet.CalendarGrabber.Objects;

namespace Woodbase.DBUNet.CalendarGrabber.Interfaces
{
    public interface IDataConnector
    {
        void SaveToDB(int matchNo, DateTime matchDay, Objects.Location location, Role role, MatchDayShort matchDayShort,
                      char matchType, int round, string row, DrivingInfo drivingInfo, Team homeTeam, Team awayTeam);

        List<Game> CheckRemovedMatch(IEnumerable<int> matchNos);
        string[] GetEmails(string userName);
        List<int> GetSentGameId();
        int SetSentGames(IEnumerable<int> gameNos);
    }
}
