using DomainLayer.Model;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using OnionConsumeWebAPI.ApiService;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace OnionConsumeWebAPI.Models
{
    public class MongoHelper
    {

        public string GetRequestCacheKey(SimpleAvailabilityRequestModel FlightSCriteria,string flightClass)
        {
            StringBuilder key = new StringBuilder();
            //string key = string.Empty;

            //Adult
            if (FlightSCriteria.passengercount != null)
            {
                key.Append(FlightSCriteria.passengercount.adultcount);
                key.Append(FlightSCriteria.passengercount.childcount);
                key.Append(FlightSCriteria.passengercount.infantcount);
            }
            else
            {
                key.Append(FlightSCriteria.adultcount);
                key.Append(FlightSCriteria.childcount);
                key.Append(FlightSCriteria.infantcount);
            }



            //if (FlightSCriteria.ChildrenAges != null)
            //{
            //    if (FlightSCriteria.Children > 0)
            //        foreach (string str in FlightSCriteria.ChildrenAges)
            //            key.Append(str);
            //}

            if (FlightSCriteria.endDate != null)
            {
                key.Append(1);
            }
            else
            {
                key.Append(0);
            }

            key.Append(Convert.ToDateTime(FlightSCriteria.beginDate).ToString("ddMMyyyy"));  //
            key.Append(Convert.ToDateTime(FlightSCriteria.endDate).ToString("ddMMyyyy"));  //
            if (FlightSCriteria.origin.Contains("-"))
            {
                key.Append(FlightSCriteria.origin.ToString().Split("-")[1].Trim());
               
            }
            else
            {
				key.Append(FlightSCriteria.origin);
				
			}

            if (FlightSCriteria.destination.Contains("-"))
            {

                key.Append(FlightSCriteria.destination.ToString().Split("-")[1].Trim());
            }
            else
            {

                key.Append(FlightSCriteria.destination);

            }
            key.Append(flightClass);
            // key.Append(FlightSCriteria.DirectFlights.ToString());
            //    if (!string.IsNullOrEmpty(FlightSCriteria.Carrier))
            //        key.Append(FlightSCriteria.Carrier.ToString());

            //if (!string.IsNullOrEmpty(FlightSCriteria.CabinClass.ToString()))
            //    key.Append(FlightSCriteria.CabinClass.ToString());
            //key.Append(currcode);


            //key = key.TrimEnd('~');
            return key.ToString();
        }

        public string CreateCommonObject(object apiRsp)
        {
            string str = string.Empty;

            using (MemoryStream memStream = new MemoryStream())
            {
                XmlSerializer serializer;
                serializer = new XmlSerializer(typeof(List<SimpleAvailibilityaAddResponce>));
                using (StreamWriter streamWriter = new StreamWriter(memStream))
                {
                    serializer.Serialize(streamWriter, apiRsp);
                    memStream.Position = 0;
                    using (StreamReader streamReader = new StreamReader(memStream))
                    {
                        XmlDocument serializedXML = new XmlDocument();
                        serializedXML.Load(streamReader);
                        str = serializedXML.OuterXml;
                    }
                }
            }

            return str;
        }

        public List<SimpleAvailibilityaAddResponce> deserializecommonobject(string str)
        {
            List<SimpleAvailibilityaAddResponce> obj = new List<SimpleAvailibilityaAddResponce>();
            try
            {
                XmlSerializer serializerFF = new XmlSerializer(typeof(List<SimpleAvailibilityaAddResponce>));
                obj = (List<SimpleAvailibilityaAddResponce>)serializerFF.Deserialize(new StringReader(str));

            }
            catch { }
            return obj;
        }



        public string Zip(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text)) return text;
                else
                {
                    //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(text);
                    //MemoryStream ms = new MemoryStream();
                    //using (System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress, true))
                    //{
                    //    zip.Write(buffer, 0, buffer.Length);
                    //}

                    //ms.Position = 0;
                    //MemoryStream outStream = new MemoryStream();

                    //byte[] compressed = new byte[ms.Length];
                    //ms.Read(compressed, 0, compressed.Length);

                    //byte[] gzBuffer = new byte[compressed.Length + 4];
                    //System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
                    //System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
                    //return Convert.ToBase64String(gzBuffer);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(text);

                    using (var msi = new MemoryStream(bytes))
                    //using (var mso = new MemoryStream())
                    {
                        var mso = new MemoryStream();
                        using (var gs = new System.IO.Compression.GZipStream(mso, System.IO.Compression.CompressionMode.Compress))
                        {
                            //msi.CopyTo(gs);
                            CopyTo(msi, gs);
                        }

                        return Convert.ToBase64String(mso.ToArray());
                    }

                }
            }
            catch
            {
                return text;
            }
        }

        public string UnZip(string compressedText)
        {
            try
            {
                if (string.IsNullOrEmpty(compressedText)) return compressedText;
                else
                {
                    //byte[] gzBuffer = Convert.FromBase64String(compressedText);
                    //using (MemoryStream ms = new MemoryStream())
                    //{
                    //    int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                    //    ms.Write(gzBuffer, 4, gzBuffer.Length - 4);
                    //    byte[] buffer = new byte[msgLength];
                    //    ms.Position = 0;
                    //    using (System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress))
                    //    {
                    //        zip.Read(buffer, 0, buffer.Length);
                    //    }
                    //    return System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    //}
                    byte[] gzBuffer = Convert.FromBase64String(compressedText);
                    using (var msi = new MemoryStream(gzBuffer))
                    using (var mso = new MemoryStream())
                    {
                        using (var gs = new System.IO.Compression.GZipStream(msi, System.IO.Compression.CompressionMode.Decompress))
                        {
                            //gs.CopyTo(mso);
                            CopyTo(gs, mso);
                        }

                        return System.Text.Encoding.UTF8.GetString(mso.ToArray());
                    }

                }
            }
            catch
            {
                return compressedText;
            }
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                //dest.WriteAsync(bytes, 0, cnt).GetAwaiter();
                dest.Write(bytes, 0, cnt);
            }
        }


        public string Get8Digits()
        {
            var bytes = new byte[4];
            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            uint random = BitConverter.ToUInt32(bytes, 0) % 1000000000;
            return String.Format("{0:D8}", random);
        }

        public string GetIp()
        {

            IHttpContextAccessor httpContextAccessorInstance = new HttpContextAccessor();

            IPAddress iPAddress = new IPAddress(httpContextAccessorInstance);
            return iPAddress.GetVisitorIPAddress();

            //string ip = "192.168.1.199";
            //         return ip;
        }

        public string DeviceName()
        {

            IHttpContextAccessor httpContextAccessorInstance = new HttpContextAccessor();
            var userAgent = httpContextAccessorInstance.HttpContext.Request.Headers["User-Agent"].ToString();
            var isMobile = IsMobileDevice(userAgent);

            if (isMobile)
            {
                return "Mobile";
            }
            else
            {
                return "Desktop";
            }



            //string ip = "192.168.1.199";
            //         return ip;
        }

        private bool IsMobileDevice(string userAgent)
        {
            // Simple mobile regex pattern to match common mobile user agents
            var mobileRegex = new Regex(@"(android|iphone|ipod|blackberry|iemobile|mobile|opera mini)", RegexOptions.IgnoreCase);
            return mobileRegex.IsMatch(userAgent);
        }




    }


    //public static class Extentions
    //{
    //    public static bool IsMobile(string userAgent)
    //    {
    //        if (string.IsNullOrEmpty(userAgent))
    //            return false;
    //        //tablet
    //        if (Regex.IsMatch(userAgent, "(tablet|ipad|playbook|silk)|(android(?!.*mobile))", RegexOptions.IgnoreCase))
    //            return true;
    //        //mobile
    //        const string mobileRegex =
    //            "blackberry|iphone|mobile|windows ce|opera mini|htc|sony|palm|symbianos|ipad|ipod|blackberry|bada|kindle|symbian|sonyericsson|android|samsung|nokia|wap|motor";

    //        if (Regex.IsMatch(userAgent, mobileRegex, RegexOptions.IgnoreCase)) return true;
    //        //not mobile 
    //        return false;
    //    }
    // }

    //  var isMobile = Extentions.IsMobile( Request.Headers["user-agent"].ToString());
}
