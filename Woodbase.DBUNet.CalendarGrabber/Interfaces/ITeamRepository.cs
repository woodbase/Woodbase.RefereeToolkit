using System.Collections.Generic;
using Woodbase.DBUNet.CalendarGrabber.Objects;

namespace Woodbase.DBUNet.CalendarGrabber.Interfaces
{
    public interface ITeamRepository
    {
        List<Team> GetAllTeams();
        List<Team> SearchTeams(string query);
        Team GetTeam(int teamId);
    }
}