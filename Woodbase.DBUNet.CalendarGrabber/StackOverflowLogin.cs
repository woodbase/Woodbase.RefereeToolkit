using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Woodbase.DBUNet.CalendarGrabber
{
    public class StackOverflowLogin
    {
        public string Login()
        {
            WebClient wc = new WebClient();
            wc.Credentials = new NetworkCredential("woodbase", "RelluT.22");
            string url = "http://dbunet.dbu.dk/public/refereeMatchProgram.aspx";
            try
            {
                using (Stream stream = wc.OpenRead(new Uri(url)))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var str = reader.ReadToEnd();
                        return str;
                    }
                }
            }
            catch (WebException e)
            {
                return "";
                //Error handeling
            }
        }
    }
}