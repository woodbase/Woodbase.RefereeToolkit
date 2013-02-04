using System;
using Woodbase.DBUNet.CalendarGrabber.Enums;

namespace Woodbase.DBUNet.CalendarGrabber.Objects
{
    public class Game
    {
        public int GameNo { get; set; }
        public int Round { get; set; }
        public MatchDayShort GameDayShort { get; set; }
        public DateTime GameDay { get; set; }
        public string Row { get; set; }
        public Location Location { get; set; }
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public Role Role { get; set; }
        public DrivingInfo DrivingInfo { get; set; }
        public char MatchType { get; set; }
    }
}