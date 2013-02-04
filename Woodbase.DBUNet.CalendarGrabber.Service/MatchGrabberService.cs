using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Woodbase.DBUNet.CalendarGrabber;
using Woodbase.DBUNet.CalendarGrabber.DataConnectors;
using Woodbase.DBUNet.CalendarGrabber.Enums;
using Woodbase.DBUNet.CalendarGrabber.Interfaces;
using Woodbase.DBUNet.CalendarGrabber.Objects;

namespace Woodbase.DBUNet.Service
{
    public partial class MatchGrabberService : ServiceBase
    {
        private readonly string _connectionString;
        private readonly string _userName;
        private readonly string _password;

        public MatchGrabberService(string connectionString, string userName, string password)
        {
            _connectionString = connectionString;
            _password = password;
            _userName = userName;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (args.Length == 0)
                try
                {
                    LoadGames(_userName, _password);
                }
                catch (Exception ex)
                {
                    throw new Exception(args.Length + " argument length. Using: " + _userName + " and " + _password);
                }
            else
                LoadGames(args[1], args[2]);

        }

        protected override void OnStop()
        {
        }

        public void LoadGames(string userName, string password)
        {
            var login = new Login(userName, password);
            var cal = new Calendar();
            var games = cal.GrabMatchList(login.DoLogin());

            var informer = new Informer(new MySqlConnector(_connectionString), userName);
            informer.SendNotifications(games.ToList());
        }
    }
}
