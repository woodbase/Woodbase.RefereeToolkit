using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Woodbase.DBUNet.CalendarGrabber.Interfaces;
using Woodbase.DBUNet.CalendarGrabber.Objects;

namespace Woodbase.DBUNet.CalendarGrabber.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        public List<Team> GetAllTeams()
        {
            throw new NotImplementedException();
        }

        public List<Team> SearchTeams(string query)
        {
            throw new NotImplementedException();
        }

        public Team GetTeam(int teamId)
        {
            throw new NotImplementedException();
        }
    }
}
