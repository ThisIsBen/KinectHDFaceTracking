﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Microsoft.Samples.Kinect.HDFaceBasics
{
    public class CKIPSS
    {
        Socket _conn = null;
        string _serverIP = "140.109.19.104";
        int _serverPort = 1501;
        string _id = "hjoru";
        string _pw = "wy611167";


        public CKIPSS(string id, string pw)
        {
            _id = id;
            _pw = pw;
            setConnection();
        }

        public List<string> Send(string targetString, out bool isSuccess, out string errorMsg)
        {
            isSuccess = false;
            errorMsg = "";

            try
            {
                string xmlString = setXmlString(targetString);

                // Blocks until send returns.
                byte[] msg = Encoding.Default.GetBytes(xmlString);
                int size = msg.Length * 10;
                byte[] bytes = new byte[size];
                int i = _conn.Send(msg);

                // Get reply from the server.
                i = _conn.Receive(bytes);
                string recieve = Encoding.Default.GetString(bytes);
                string sucessMsg = "<processstatus code=\"0\">Success</processstatus>";

                if (!recieve.Contains(sucessMsg))
                {
                    string[] seperator = { "\">", "</processstatus>" };
                    string[] sepResult = recieve.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
                    errorMsg = sepResult[2];
                }
                else
                    isSuccess = true;

                List<string> result = parseXML(recieve);
                return result;
            }
            catch (SocketException exp)
            {
                errorMsg = exp.Message + " code:" + exp.ErrorCode;
            }


            return null;
        }

        private List<string> parseXML(string recieve)
        {
            List<string> result = new List<string>();
            string[] seperator = { "<sentence>", "</sentence>" };
            string[] sepResult = recieve.Split(seperator, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < sepResult.Length - 1; i++)
            {
                string s = sepResult[i].Trim();
                if (s != "")
                {
                    result.Add(s);
                }
            }
            return result;
        }

        private string setXmlString(string targetString)
        {
            List<string> content = new List<string>();

            content.Add("<?xml version=\"1.0\" ?>");
            content.Add("<wordsegmentation version=\"0.1\">");
            content.Add("<option showcategory=\"1\" />");
            content.Add("<authentication username=\"" + _id + "\" password=\"" + _pw + "\" />");
            string contentString = "<text>" + targetString + "</text>";
            content.Add(contentString);
            content.Add("</wordsegmentation>");

            string result = "";
            foreach (string s in content)
            {
                result += s;
            }

            return result;
        }

        private void setConnection()
        {
            if (_conn == null)
            {
                try
                {
                    IPAddress ip = IPAddress.Parse(_serverIP);
                    IPEndPoint ipep = new IPEndPoint(ip, _serverPort);

                    _conn = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _conn.Connect(ipep);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

    }
}
