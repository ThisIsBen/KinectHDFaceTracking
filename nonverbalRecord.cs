using System;
using System.Text;
using System.IO;
using System.Net.Sockets;
namespace Microsoft.Samples.Kinect.HDFaceBasics
{
    public class nonverbalRecord
    {
        public string timeStamp { get; set; }
        public int smileIntensity { get; set; }
        public int nodIntensity { get; set; }
        public int eyeContactIntensity { get; set; }
        

        public static void initNonverbalRecordOBJ(nonverbalRecord[] nonverbalRecordOBJ, int nonverbalAryNum)
        {
            //init nonverbal recorder array
            for (int i = 0; i < nonverbalAryNum; i++)
            {
                nonverbalRecordOBJ[i] = new nonverbalRecord();

                nonverbalRecordOBJ[i].timeStamp = "-";
                nonverbalRecordOBJ[i].smileIntensity = -1;
                nonverbalRecordOBJ[i].nodIntensity = -1;
                nonverbalRecordOBJ[i].eyeContactIntensity = -1;
               

            }
            //////////////////////////////////
        }

        //send nonverbalRecord in the form of json to backend(array->json)
        public static void sendNonverbalRecord(nonverbalRecord[] nonverbalRecordOBJ)
        {

            //Turn it into json
            JSonHelper helper0 = new JSonHelper();
            string jsonResult = helper0.ConvertObjectToJSon(nonverbalRecordOBJ);
            //Console.WriteLine("jsonResult Converter test" + jsonResult);

            //tcp to send json to back end
            TcpClient tcpclnt = new TcpClient();

            tcpclnt.Connect("54.191.185.244", 8084); // use the ipaddress as in the server program

            Stream stm = tcpclnt.GetStream();

            //encode UTF8
            Byte[] nonverbal_data = Encoding.UTF8.GetBytes(jsonResult);



            stm.Write(nonverbal_data, 0, nonverbal_data.Length);

            tcpclnt.Close();


        }



    }


}
