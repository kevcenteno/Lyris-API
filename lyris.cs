using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Web;

namespace Lyris
{
    public class LyrisService
    {
        private string lyrisURL = "https://www.elabs10.com/API/mailing_list.html";
        private string lyrisSiteID = "123456"; // Lyris Site ID here
        private string lyrisPassword = "password"; // Lyris API password here
        public string lyrisResponse;
        public string lyrisMessage;

        public void MailingListSignUp(int MLID, string email, Dictionary<int, string> demographics)
        {
            try
            {
                var dataset = new XElement("DATASET",
                    new XElement("SITE_ID", lyrisSiteID),
                    new XElement("MLID", MLID),
                    new XElement("DATA", lyrisPassword,
                        new XAttribute("type", "extra"),
                        new XAttribute("id", "password")
                        ),
                    new XElement("DATA", email,
                        new XAttribute("type", "email")
                        )
                    );
                foreach (KeyValuePair<int, string> pair in demographics)
                {
                    dataset.Add(
                        new XElement("DATA", pair.Value,
                            new XAttribute("type", "demographic"),
                            new XAttribute("id", pair.Key)
                            )
                        );
                }
                string emailState = getEmailState(MLID, email);

                // If the email doesn't exist in Lyris, create the record.
                if (emailState == "dne")
                {
                    var request = WebRequest.Create(
                    string.Format("{0}?type={1}&activity={2}&input={3}",
                        lyrisURL,
                        "record",
                        "add",
                        dataset.ToString()
                        )
                    );
                    processLyrisResponse(request);

                }
                // If the email exists in Lyris and is subscribed, update the record.
                else if (emailState == "isSubscribed")
                {
                    var request = WebRequest.Create(
                    string.Format("{0}?type={1}&activity={2}&input={3}",
                        lyrisURL,
                        "record",
                        "update",
                        dataset.ToString()
                        )
                    );
                    processLyrisResponse(request);
                }
                // If the email exists in Lyris, but is unsubscribed, resubscribe them
                else
                {
                    XElement updatedDataset1 =
                        new XElement("DATA", "active",
                        new XAttribute("type", "extra"),
                        new XAttribute("id", "state")
                        );
                    XElement updatedDataSet2 =
                        new XElement("DATA", "n",
                        new XAttribute("type", "extra"),
                        new XAttribute("id", "trashed")
                        );
                    dataset.Add(updatedDataset1, updatedDataSet2);

                    var request = WebRequest.Create(
                     string.Format("{0}?type={1}&activity={2}&input={3}",
                         lyrisURL,
                         "record",
                         "update",
                         dataset.ToString()
                         )
                     );

                    processLyrisResponse(request);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void MailingListUnsubscribe(int MLID, string email)
        {
            try
            {
                var dataset = new XElement("DATASET",
                    new XElement("SITE_ID", lyrisSiteID),
                    new XElement("MLID", MLID),
                    new XElement("DATA", lyrisPassword,
                        new XAttribute("type", "extra"),
                        new XAttribute("id", "password")
                        ),
                    new XElement("DATA", email,
                        new XAttribute("type", "email")
                        ),
                    new XElement("DATA", "unsubscribed",
                        new XAttribute("type", "extra"),
                        new XAttribute("id", "state")
                        )
                    );
                var request = WebRequest.Create(
                    string.Format("{0}?type={1}&activity={2}&input={3}",
                        lyrisURL,
                        "record",
                        "update",
                        dataset.ToString()
                        )
                    );
                request.Method = "POST";
                processLyrisResponse(request);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private string getEmailState(int MLID, string email)
        {
            try
            {
                var dataset = new XElement("DATASET",
                    new XElement("SITE_ID", lyrisSiteID),
                    new XElement("MLID", MLID),
                    new XElement("DATA", lyrisPassword,
                        new XAttribute("type", "extra"),
                        new XAttribute("id", "password")
                        ),
                    new XElement("DATA", email,
                        new XAttribute("type", "email")
                        )
                );

                var request = WebRequest.Create(
                    string.Format("{0}?type={1}&activity={2}&input={3}",
                        lyrisURL,
                        "record",
                        "query-data",
                        dataset.ToString()
                        )
                    );
                request.Method = "POST";

                var _lyrisResponse = string.Empty;
                var _lyrisMessage = string.Empty;

                using (var response = request.GetResponse())
                {
                    using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                    {
                        using (XmlReader lyrisXml = XmlReader.Create(new StringReader(reader.ReadToEnd())))
                        {

                            while (lyrisXml.Read())
                            {
                                if (lyrisXml.NodeType == XmlNodeType.Element)
                                {
                                    if (lyrisXml.Name == "TYPE")
                                    {
                                        _lyrisResponse = lyrisXml.ReadInnerXml();
                                    }

                                    if (lyrisXml.Name == "DATA" && lyrisXml.GetAttribute("id") == "state")
                                    {
                                        _lyrisMessage = lyrisXml.ReadInnerXml();
                                    }

                                }
                            }
                        }
                    }

                    response.Close();
                }

                if (_lyrisResponse == "error")
                {
                    return "dne";
                }

                else if (_lyrisResponse == "success" && _lyrisMessage == "active")
                {
                    return "isSubscribed";
                }

                else
                {
                    return "isUnsubscribed";
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void processLyrisResponse(WebRequest request)
        {
            using (var response = request.GetResponse())
            {
                using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                {

                    using (XmlReader lyrisXml = XmlReader.Create(new StringReader(reader.ReadToEnd())))
                    {

                        while (lyrisXml.Read())
                        {
                            if (lyrisXml.NodeType == XmlNodeType.Element)
                            {
                                if (lyrisXml.Name == "TYPE")
                                {
                                    lyrisResponse = lyrisXml.ReadInnerXml();
                                }

                                if (lyrisXml.Name == "DATA")
                                {
                                    lyrisMessage = lyrisXml.ReadInnerXml();
                                }
                            }
                        }
                    }
                }

                response.Close();
            }
        }
    }
}