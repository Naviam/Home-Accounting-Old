// Diafaan SMS Server Scripting Connector skeleton script
//
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Text;
using System.IO;

namespace DiafaanMessageServer
{
    public class ConnectorScript : IScript
    {
        private IScriptHost host = null;

        public void OnLoad(IScriptHost host)
        {
            this.host = host;
            //
            // TODO: Add initialization code.
            //
        }

        public void OnUnload()
        {
            //
            // TODO: Add cleanup code, make sure to remove (Timer) event handlers here
            //
        }
        private void OnMessageReceived(string fromAddress, string toAddress, string message, string messageType,
                                       string messageId, string smsc, string pdu, string gateway,
                                       DateTime sendTime, DateTime receiveTime)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(@"http://localhost:54345/Sms/RecieveMessage");
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.Method = "POST";
                string queryString = "key=" + HttpUtility.UrlEncode("givemeaccesstotoyou") + "&message=" + HttpUtility.UrlEncode(message) + "&gateway=" + HttpUtility.UrlEncode(gateway) +
                     "&from=" + HttpUtility.UrlEncode(fromAddress) + "&to=" + HttpUtility.UrlEncode(toAddress);
                byte[] byteData = UTF8Encoding.UTF8.GetBytes(queryString);
                httpWebRequest.ContentLength = byteData.Length;
                using (Stream postStream = httpWebRequest.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }
                using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    string responseText = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
                    if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                    {
                        PostEventLog("Send ok:" + responseText, responseText, EventLog.Information);
                        //PostEventLog("Send error:"+DateTime.Now.ToString(), httpWebResponse.StatusCode.ToString(), EventLog.Error);
                    }
                    else
                    {
                        PostEventLog("Send error:" + httpWebResponse.StatusCode.ToString(), httpWebResponse.StatusCode.ToString(), EventLog.Error);
                    }
                }
            }
            catch (Exception e)
            {
                WebException exc = e as WebException;
                if (exc != null)
                {
                    string responseText = new StreamReader(exc.Response.GetResponseStream()).ReadToEnd();
                    PostEventLog("Send error:" + responseText, responseText, EventLog.Error);
                }
                else
                    PostEventLog(e.Message, e.ToString(), EventLog.Error);
                //PostSendResult("1", "", StatusCode.SendError, "Error: message rejected", "", e.Message, false);
            }			//
            // TODO: Add code to handle received SMS messages
            // e.g.
            // PostSendMessage(fromAddress, "", "We have received your message", "sms.text", "", "", "");
            //
        }

        private void OnSendMessageResult(string toAddress, string fromAddress, string message, string messageType,
                                         string messageId, string pdu, string gateway, int statusCode,
                                         string statusText, string errorCode, string errorText,
                                         DateTime sendTime, DateTime receiveTime, string userId, string userInfo)
        {
            //
            // TODO: Add code to handle the status result of a previously sent message
            //
        }
        private void OnSendMessageResultUpdate(string messageId, int statusCode, string statusText,
                                               string errorCode, string errorText, DateTime receiveTime)
        {
            //
            // TODO: Add code to handle a status result update of a previously sent message
            //
        }
        #region Connector Script helper functions
        private enum StatusCode { Sent = 200, Received = 201, SendError = 300 }
        private enum EventLog { Information = 0, Warning = 1, Error = 2 }
        private enum TraceLog { Information = 0, Send = 1, Receive = 2 }

        public void OnMessagePacket(Hashtable messagePacket)
        {
            if (GetPacketString(messagePacket, "PacketName", "") == "MessageIn")
            {
                OnMessageReceived(GetPacketString(messagePacket, "From", ""),
                                    GetPacketString(messagePacket, "To", ""),
                                    GetPacketString(messagePacket, "Message", ""),
                                    GetPacketString(messagePacket, "MessageType", ""),
                                    GetPacketString(messagePacket, "MessageId", ""),
                                    GetPacketString(messagePacket, "SMSC", ""),
                                    GetPacketString(messagePacket, "PDU", ""),
                                    GetPacketString(messagePacket, "Gateway", ""),
                                    GetPacketDateTime(messagePacket, "SendTime", DateTime.Now),
                                    GetPacketDateTime(messagePacket, "ReceiveTime", DateTime.Now));
            }
            else if (GetPacketString(messagePacket, "PacketName", "") == "MessageOutResult")
            {
                OnSendMessageResult(GetPacketString(messagePacket, "To", ""),
                                    GetPacketString(messagePacket, "From", ""),
                                        GetPacketString(messagePacket, "Message", ""),
                                        GetPacketString(messagePacket, "MessageType", ""),
                                        GetPacketString(messagePacket, "MessageId", ""),
                                        GetPacketString(messagePacket, "PDU", ""),
                                        GetPacketString(messagePacket, "Gateway", ""),
                                        GetPacketInteger(messagePacket, "StatusCode", -1),
                                        GetPacketString(messagePacket, "StatusText", ""),
                                        GetPacketString(messagePacket, "ErrorCode", ""),
                                        GetPacketString(messagePacket, "ErrorText", ""),
                                        GetPacketDateTime(messagePacket, "SendTime", DateTime.Now),
                                        GetPacketDateTime(messagePacket, "ReceiveTime", DateTime.MaxValue),
                                        GetPacketString(messagePacket, "UserId", ""),
                                        GetPacketString(messagePacket, "UserInfo", ""));
            }
            else if (GetPacketString(messagePacket, "PacketName", "") == "MessageOutResultUpdate")
            {
                OnSendMessageResultUpdate(GetPacketString(messagePacket, "MessageId", ""),
                                                    GetPacketInteger(messagePacket, "StatusCode", -1),
                                                    GetPacketString(messagePacket, "StatusText", ""),
                                                GetPacketString(messagePacket, "ErrorCode", ""),
                                                    GetPacketString(messagePacket, "ErrorText", ""),
                                                    GetPacketDateTime(messagePacket, "ReceiveTime", DateTime.MaxValue));
            }
        }
        private void PostSendMessage(string toAddress, string fromAddress, string messageText,
                                     string messageType, string gateway, string userId, string userInfo)
        {
            Hashtable messagePacket = new Hashtable();

            messagePacket.Add("PacketName", "MessageOut");
            messagePacket.Add("To", toAddress);
            messagePacket.Add("From", fromAddress);
            messagePacket.Add("Message", messageText);
            messagePacket.Add("Gateway", gateway);
            messagePacket.Add("UserId", userId);
            messagePacket.Add("UserInfo", userInfo);
            host.PostMessagePacket(messagePacket);
        }
        private void PostEventLog(string eventMessage, string eventDescription, EventLog eventType)
        {
            Hashtable messagePacket = new Hashtable();

            messagePacket.Add("PacketName", "EventLog");
            messagePacket.Add("EventType", ((int)eventType).ToString());
            messagePacket.Add("EventMessage", eventMessage);
            messagePacket.Add("EventDescription", eventDescription);
            host.PostMessagePacket(messagePacket);
        }
        private void PostTraceLog(string traceLogMessage, TraceLog logType)
        {
            Hashtable messagePacket = new Hashtable();

            messagePacket.Add("PacketName", "TraceLog");
            messagePacket.Add("TraceLogType", ((int)logType).ToString());
            messagePacket.Add("TraceLogMessage", traceLogMessage);
            host.PostMessagePacket(messagePacket);
        }
        private string GetPacketString(Hashtable hashtable, string key, string defaultValue)
        {
            try
            {
                return (string)hashtable[key];
            }
            catch { }
            return defaultValue;
        }

        private int GetPacketInteger(Hashtable hashtable, string key, int defaultValue)
        {
            try
            {
                return Convert.ToInt32((string)hashtable[key]);
            }
            catch { }
            return defaultValue;
        }

        private DateTime GetPacketDateTime(Hashtable hashtable, string key, DateTime defaultValue)
        {
            try
            {
                return new DateTime(Convert.ToInt64((string)hashtable[key]));
            }
            catch { }
            return defaultValue;
        }
        #endregion
    }
}
