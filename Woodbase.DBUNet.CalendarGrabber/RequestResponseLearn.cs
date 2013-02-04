using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Woodbase.DBUNet.CalendarGrabber
{
    public class RequestResponseLearn
    {
        public string RequestResponse()
        {
            CookieContainer cookies = new CookieContainer();
            var webRequest = (HttpWebRequest)WebRequest.Create("http://dbunet.dbu.dk");
            webRequest.CookieContainer = cookies;
            webRequest.AllowAutoRedirect = false;
            var webResponse = (HttpWebResponse)webRequest.GetResponse();


            if (webResponse.StatusCode == HttpStatusCode.Redirect)
            {
                string strNewLocation = webResponse.Headers[HttpResponseHeader.Location];
                webResponse.Close();

                // this is the login page url
                Uri loginUrl = new Uri(new Uri("http://dbunet.dbu.dk"), strNewLocation);
                webRequest = (HttpWebRequest)WebRequest.Create(loginUrl);
                webRequest.CookieContainer = cookies;
                webResponse = (HttpWebResponse)webRequest.GetResponse();
                string strLoginHtml;

                using (Stream stream = webResponse.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                        // get the login page html, we may need it later
                        strLoginHtml = reader.ReadToEnd();
                }
                webResponse.Close();

                webRequest = (HttpWebRequest)WebRequest.Create(loginUrl);
                webRequest.CookieContainer = cookies;

                // make a POST to the login url with valid username and password
                webRequest.Method = "POST";

                using (Stream stream = webRequest.GetRequestStream())
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write("Login$UserName=admin&Login$Password=b");
                    }
                }
            }

            // if everything works fine now this should contain the file we want
            webResponse = (HttpWebResponse)webRequest.GetResponse();

            // Ask the server for the file size and store it
            var fileSize = webResponse.ContentLength;

            // It will store the current number of bytes we retrieved from the server
            int bytesSize = 0;
            // A buffer for storing and writing the data retrieved from the server
            byte[] downBuffer = new byte[2048];
            
            //req.ContentType = "application/x-www-form-urlencoded";
            //req.Method = "POST";
            //req.KeepAlive = true;
            ////req.AllowAutoRedirect = false;

            //byte[] bytes = Encoding.ASCII.GetBytes("Login$UserName=admin&Login$Password=b");
            //req.ContentLength = bytes.Length;

            //using(var reqStream = req.GetRequestStream())
            //{
            //    reqStream.Write(bytes, 0, bytes.Length);
            //}

            //var response = (HttpWebResponse)req.GetResponse();
            
            return webResponse.StatusDescription;
        }
    }
}
