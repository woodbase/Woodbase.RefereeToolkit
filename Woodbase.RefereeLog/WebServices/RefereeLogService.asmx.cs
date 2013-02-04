using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Web;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using Woodbase.DBUNet.CalendarGrabber.Objects;

namespace Woodbase.RefereeLog.WebServices
{
    /// <summary>
    /// Summary description for RefereeLogService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class RefereeLogService : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public List<Team> LookupTeam()
        {
            
            var team = new List<Team> {new Team() {Id = 1, Name = "Dianalund"}, new Team() {Id = 2, Name = "SB&I"}};
            return team;
        }
    }
}
