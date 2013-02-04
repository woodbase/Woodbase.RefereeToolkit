using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Woodbase.DBUNet.CalendarGrabber.Enums;
using Woodbase.DBUNet.CalendarGrabber.Interfaces;
using Woodbase.DBUNet.CalendarGrabber.Objects;

namespace Woodbase.DBUNet.CalendarGrabber.DataConnectors
{
    public class MySqlConnector : IDataConnector
    {
        public string ConnectionString { get; set; }

        public MySqlConnector(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void SaveToDB(int matchNo, DateTime matchDay, Location location, Role role, MatchDayShort matchDayShort, char matchType, int round, string row, DrivingInfo drivingInfo, Team homeTeam, Team awayTeam)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                var locationId = GetLocationId(location);
                var homeTeamId = GetTeamId(homeTeam);
                var awayTeamId = GetTeamId(awayTeam);
                var cmd = new MySqlCommand(
                    "INSERT INTO DbuGrabber_Game " +
                    "(GameNo, Round, GameDayShort, GameDay, Row, LocationId, HomeTeamId, AwayTeamId, Role, DrivingHours, DrivingMinutes, MatchType) " +
                    "VALUES " +
                    "(@GameNo, @Round, @GameDayShort, @GameDay, @Row, @LocationId, @HomeTeamId, @AwayTeamId, @Role, @DrivingHours, @DrivingMinutes, @MatchType)" +
                    " ON DUPLICATE KEY UPDATE Round = @Round, GameDayShort = @GameDayShort, GameDay = @GameDay, Row = @Row, LocationId = @LocationId, HomeTeamId= @HomeTeamId, AwayTeamId = @AwayTeamId, Role = @Role, DrivingHours = CASE WHEN (DrivingHours <> @DrivingHours) THEN @DrivingHours ELSE DrivingHours END, DrivingMinutes = CASE WHEN (DrivingMinutes <> @DrivingMinutes) THEN @DrivingMinutes ELSE DrivingMinutes END, MatchType= @MatchType, Sent = CASE WHEN (DrivingHours <> @DrivingHours OR DrivingMinutes <> @DrivingMinutes) THEN 0 ELSE Sent END", conn);
                cmd.Parameters.AddWithValue("@GameNo", matchNo);
                cmd.Parameters.AddWithValue("@Round", round);
                cmd.Parameters.AddWithValue("@GameDayShort", matchDayShort.ToString());
                cmd.Parameters.AddWithValue("@GameDay", matchDay);
                cmd.Parameters.AddWithValue("@Row", row);
                cmd.Parameters.AddWithValue("@LocationId", locationId);
                cmd.Parameters.AddWithValue("@HomeTeamId", homeTeamId);
                cmd.Parameters.AddWithValue("@AwayTeamId", awayTeamId);
                cmd.Parameters.AddWithValue("@Role", role.ToString());
                cmd.Parameters.AddWithValue("@DrivingHours", drivingInfo.Hours);
                cmd.Parameters.AddWithValue("@DrivingMinutes", drivingInfo.Minutes);
                cmd.Parameters.AddWithValue("@MatchType", matchType);
                cmd.ExecuteNonQuery();
            }
        }

        //TODO: Add addtional team attributes on insert!
        private int GetTeamId(Team team)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                var cmd =
                    new MySqlCommand(
                        string.Format("SELECT TeamId FROM DbuGrabber_Team WHERE TeamName = '{0}'", team.Name), conn);
                var teamId = cmd.ExecuteScalar();
                if (teamId == null)
                {
                    cmd = new MySqlCommand(string.Format("INSERT INTO DbuGrabber_Team (TeamName) VALUES ('{0}'); SELECT last_insert_id();", team.Name), conn);
                    teamId = cmd.ExecuteScalar();
                }
                return int.Parse(teamId.ToString());
            }
        }

        private int GetLocationId(Location location)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                var cmd =
                    new MySqlCommand(
                        string.Format("SELECT LocationId FROM DbuGrabber_Location WHERE LocationName = '{0}'",
                                      location.Name), conn);
                var locationId = cmd.ExecuteScalar();
                if (locationId == null)
                {
                    cmd =
                        new MySqlCommand(
                            string.Format("INSERT INTO DbuGrabber_Location (LocationName) VALUES ('{0}'); select last_insert_id();",
                                          location.Name), conn);
                    locationId = cmd.ExecuteScalar();
                }
                return int.Parse(locationId.ToString());
            }
        }

        public List<Game> CheckRemovedMatch(IEnumerable<int> matchNos)
        {
            var result = new List<Game>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(string.Format(@"UPDATE DbuGrabber_Game SET Deleted = 1 WHERE GameNo NOT IN ({0}) AND (Deleted = 0 OR Deleted IS NULL) AND GameDay > NOW();
                                                            SELECT GameNo, Round, GameDayShort, GameDay, Row, HomeTeamId, AwayTeamId, Role, DrivingHours, DrivingMinutes, MatchType, loc.LocationId, loc.LocationName, home.TeamId, home.TeamName, away.TeamId, away.TeamName 
                                                            FROM DbuGrabber_Game game
                                                            JOIN DbuGrabber_Location loc ON loc.LocationId = game.LocationId
                                                            JOIN DbuGrabber_Team home ON game.HomeTeamId = home.TeamId
                                                            JOIN DbuGrabber_Team away ON game.AwayTeamId = away.TeamId WHERE GameNo NOT IN ({0}) AND game.Deleted = 1 AND GameDay > NOW();
                                                            ", string.Join(",", matchNos)), conn);
                var reader = cmd.ExecuteReader();

                if(reader.HasRows)
                while (reader.Read())
                {
                    result.Add(new Game()
                                   {
                                       GameNo = reader.GetInt32("GameNo"),
                                       Round = reader.GetInt32("Round"),
                                       HomeTeam = new Team() { Id = reader.GetInt32("home.TeamId"), Name = reader.GetString("home.TeamName") },
                                       AwayTeam = new Team() { Id = reader.GetInt32("away.TeamId"), Name = reader.GetString("away.TeamName") },
                                       GameDayShort = (MatchDayShort)Enum.Parse(typeof(MatchDayShort), reader.GetString("GameDayShort")),
                                       GameDay = reader.GetDateTime("GameDay"),
                                       Row = reader.GetString("Row"),
                                       Role = (Role)Enum.Parse(typeof(Role), reader.GetChar("Role").ToString()),
                                       DrivingInfo = new DrivingInfo(){Hours = reader.GetInt32("DrivingHours"), Minutes = reader.GetInt32("DrivingMinutes")},
                                       Location = new Location(){Id = reader.GetInt32("loc.LocationId"), Name = reader.GetString("loc.LocationName")},
                                       MatchType = reader.GetChar("MatchType")
                                   });
                }
            }
            return result;
        }

        public string[] GetEmails(string userName)
        {
            var emails = new List<string>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT Email FROM DbuGrabber_Subscribers WHERE RefUserName = @UserName", conn);
                cmd.Parameters.AddWithValue("@UserName", userName);
                var reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    emails.Add(reader.GetString(0));
                }
            }
            return emails.ToArray();
        }

        public List<int> GetSentGameId()
        {
            var gameNos = new List<int>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT GameNo FROM DbuGrabber_Game WHERE Sent = 1", conn);
                
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    gameNos.Add(reader.GetInt32(0));
                }
            }
            return gameNos.ToList();
        }

        public int SetSentGames(IEnumerable<int> gameNos)
        {
            var nos = gameNos as List<int> ?? gameNos.ToList();
            if (!nos.Any())
                return 0;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(string.Format("UPDATE DbuGrabber_Game SET Sent = 1 WHERE GameNo in ({0})", string.Join(",", nos)), conn);
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
