using System;
using System.Net;
using System.Web;
using HtmlAgilityPack;

namespace Woodbase.DBUNet.CalendarGrabber
{
    public class DrivingInfo
    {
        public DrivingInfo(string matchUrl, CookieContainer loginCookie)
        {
            var formUrl = matchUrl;
            var req = (HttpWebRequest)WebRequest.Create("http://dbunet.dbu.dk" + HttpUtility.HtmlDecode(formUrl));
            req.Method = "GET";
            req.CookieContainer = loginCookie;
            var resp = req.GetResponse() as HttpWebResponse;
            var detailsDocument = new HtmlDocument();
            detailsDocument.Load(resp.GetResponseStream());

            var directionLinkCell =
                detailsDocument.DocumentNode.SelectSingleNode(
                    "//div[@class='pageContent']//table//td[text()='Rutebeskrivelse']/following-sibling::td");
            if (directionLinkCell.SelectSingleNode("./a") == null) return;
            var drivingDirectionUrl = directionLinkCell.SelectSingleNode("./a").GetAttributeValue("href", "");
            if (!string.IsNullOrEmpty(drivingDirectionUrl))
            {
                req = (HttpWebRequest)WebRequest.Create("http://dbunet.dbu.dk" + HttpUtility.HtmlDecode(drivingDirectionUrl));
                req.Method = "GET";
                req.CookieContainer = loginCookie;
                resp = req.GetResponse() as HttpWebResponse;
                detailsDocument = new HtmlDocument();
                detailsDocument.Load(resp.GetResponseStream());

                var drivingTimeString = detailsDocument.DocumentNode.SelectSingleNode("//div[@class='pageContent']//table//td[text()='Ca. kørselstid:']/following-sibling::td").InnerText;
                var drivingStringArray = drivingTimeString.Split(' ');
                Hours = int.Parse(drivingStringArray[0]);
                Minutes = int.Parse(drivingStringArray[3]);
            }
        }

        public DrivingInfo()
        {
        }

        public int Hours { get; set; }
        public int Minutes { get; set; }
    }
}