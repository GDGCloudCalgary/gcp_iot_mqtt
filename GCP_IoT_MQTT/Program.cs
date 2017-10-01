
using Jose;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace GCP_IoT_MQTT
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime dtnow = DateTime.Now;
            long iat = ((DateTimeOffset)dtnow).ToUnixTimeSeconds();
            long exp = iat + 3600; //expires in 3600 seconds

            var claims = new Dictionary<string, object>()
            {
                {"iat", iat}
               ,{"exp", exp}
               ,{"aud","ieccloudiot"} //aud is gcp project
            };


            
            X509Certificate2 x509Certificate2 = new X509Certificate2(@"C:\temp.myCode\GCP_IoT_MQTT\GCP_IoT_MQTT\bin\Debug\netcoreapp2.0\ia.p12"
                , "123456789", X509KeyStorageFlags.Exportable);

            var privateKey = x509Certificate2.PrivateKey;
            string token = Jose.JWT.Encode(claims, privateKey, JwsAlgorithm.RS256); // JWT token
            
            X509Certificate x509_roots = new X509Certificate(@"C:\temp.myCode\GCP_IoT_MQTT\GCP_IoT_MQTT\bin\Debug\netcoreapp2.0\roots.pem");

            MqttClient client = new MqttClient("mqtt.googleapis.com",
                8883
                , true
                , x509_roots //caCert,CA certificate for secure connection
                , x509Certificate2 //ClientCert,Client certificate
                , MqttSslProtocols.TLSv1_2
                );

            string clientId = "projects/ieccloudiot/locations/us-central1/registries/devreg1/devices/dev1";
            client.Connect(clientId, null, token); //username is null, authentication via JWT


            //publish 100 messages
            for (int i = 0; i <= 100; i++)
            {
                Random rnd = new Random();
                string strValue = Convert.ToString(rnd.Next());
                strValue = strValue + "-netcore";
                client.Publish("/devices/dev1/events", Encoding.UTF8.GetBytes(strValue), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            }
            client.Disconnect();

        }
    }
}
