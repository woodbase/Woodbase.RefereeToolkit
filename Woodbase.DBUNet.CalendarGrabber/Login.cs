using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace Woodbase.DBUNet.CalendarGrabber
{
    public class Login
    {
        public string UserName { get; private set; }
        public string Password { get; set; }

        public Login(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        public CookieContainer DoLogin()
        {
            const string formUrl = "http://dbunet.dbu.dk";
            var req = (HttpWebRequest)WebRequest.Create(formUrl);
            req.Method = "GET";

            var resp = req.GetResponse() as HttpWebResponse;
            var loginPage = new HtmlDocument();
            loginPage.Load(resp.GetResponseStream());
            
            var viewstate = loginPage.DocumentNode.SelectSingleNode("//input[@name='__VIEWSTATE']").GetAttributeValue("value","---");

            string proxy = null;
            
            var formParams = string.Format("?__VIEWSTATE={0}&_ctl2:sys_txtUsername={1}&_ctl2:sys_txtPassword={2}&_ctl2:cbRememberMe=true&_ctl2:ibtnSignin=Continue&_ctl2:ibtnSignin.x=33&_ctl2:ibtnSignin.y=3&TopMenu_ClientState=", viewstate, UserName, Password);
            req = (HttpWebRequest)WebRequest.Create(formUrl);
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            req.KeepAlive = true;
            req.AllowAutoRedirect = false;
            req.Proxy = new WebProxy(proxy, true); // ignore for local addresses
            req.CookieContainer = new CookieContainer(); // enable cookies
            byte[] bytes = Encoding.ASCII.GetBytes(formParams);
            req.ContentLength = bytes.Length;
            using (var os = req.GetRequestStream())
            {
                os.Write(bytes, 0, bytes.Length);
            }
            resp = (HttpWebResponse)req.GetResponse();

            return req.CookieContainer;
        }
    }
}
