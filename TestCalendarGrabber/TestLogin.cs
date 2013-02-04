using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Woodbase.DBUNet.CalendarGrabber;
using Woodbase.DBUNet.CalendarGrabber.DataConnectors;
using Woodbase.DBUNet.CalendarGrabber.Enums;
using Woodbase.DBUNet.CalendarGrabber.Objects;

namespace TestCalendarGrabber
{
    [TestFixture]
    public class TestLogin
    {
        [Test]
        public void DoLogin()
        {
            var login = new Login("<username>", "<password>");
            Assert.IsNotNull(login.DoLogin());
        }

        [Test]
        public void GetMatchList()
        {
            var login = new Login("<username>", "<password>");
            var cal = new Calendar();
            var games = cal.GrabMatchList(login.DoLogin());
            var enumerable = games as Game[] ?? games.ToArray();
            var h = enumerable.First();
            Assert.AreEqual(9, enumerable.Count());
        }

        [Test]
        public void TryInformer()
        {
            var login = new Login("<username>", "<password>");
            var cal = new Calendar();
            var games = cal.GrabMatchList(login.DoLogin());
            var enumerable1 = games as List<Game> ?? games.ToList();
            var enumerable = games as Game[] ?? enumerable1.ToArray();

            var inform = new Informer(new MySqlConnector(""), login.UserName);
            inform.SendNotifications(enumerable1.ToList());

            var h = enumerable.First();
            Assert.AreEqual(9, enumerable.Count());
        }

        [Test]
        public void SimpleCall()
        {
            var cal = new Calendar();
            var list = cal.GetCalenders();
            Assert.IsNotNull(list);
        }

        [Test]
        public void SaveToDB()
        {

            var conn =
                new MySqlConnector(
                    "server=mysql13.unoeuro.com;user id=woodbase_dk; password=15Tuller; database=woodbase_dk_db; pooling=false");
            conn.SaveToDB(98000, DateTime.Now.AddDays(+4), new Location(){Name = "TEST"}, Role.Referee, MatchDayShort.Lør, 'N', 5, "Serie 4", new DrivingInfo(){Hours = 0, Minutes = 25}, new Team(){Name = "DIF"}, new Team(){Name="SIF"});
           // Assert.AreEqual(2, lId);
        }

        [Test]
        public void CheckRemovedMatches()
        {
            var conn =
                new MySqlConnector(
                    "server=mysql13.unoeuro.com;user id=woodbase_dk; password=15Tuller; database=woodbase_dk_db; pooling=false");
            conn.CheckRemovedMatch(new List<int>(98001));
        }
    }
}
