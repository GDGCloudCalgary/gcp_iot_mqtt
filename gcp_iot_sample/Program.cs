
using Jose;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using GoogleCloudIoTSamples;
using System.Reflection;
using System.Diagnostics;

namespace GoogleCloudIoTSample
{
  


    public class IotSample
    {
        static ushort msgId;
        static string currentExcutionPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        private static void client_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
        {
            Console.WriteLine("MessageId = " + e.MessageId + " Published = " + e.IsPublished);
        }

        private static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            Console.WriteLine("Received = " + Encoding.UTF8.GetString(e.Message) + " on topic " + e.Topic);
        }


        static void Main(string[] args)
        {

            var options = new Options();

            if (CommandLine.Parser.Default.ParseArgumentsStrict(args, options))
            {
                if (options.Help)
                {
                    Console.WriteLine(CommandLine.Text.HelpText.AutoBuild(options));
                    return;
                }
                //Console.WriteLine(options.Help);
            }

            DateTime dtnow = DateTime.Now;
            long iat = ((DateTimeOffset)dtnow).ToUnixTimeSeconds();
            long exp = iat + 3600; //expires in 3600 seconds

            var claims = new Dictionary<string, object>()
            {
                {"iat", iat}
               ,{"exp", exp}
               ,{"aud",options.projectId} //aud is gcp project
            };

            X509Certificate2 x509Certificate2 = new X509Certificate2(currentExcutionPath + "\\" + "ia.p12"
                , "123456789"
                , X509KeyStorageFlags.Exportable);

            string token = Jose.JWT.Encode(claims
                , x509Certificate2.PrivateKey
                , JwsAlgorithm.RS256); // JWT token

            X509Certificate x509_roots = new X509Certificate(currentExcutionPath + "\\" + "roots.pem");

            MqttClient client = new MqttClient("mqtt.googleapis.com",
                8883
                , true
                , x509_roots //caCert,CA certificate for secure connection
                , x509Certificate2 //ClientCert,Client certificate
                , MqttSslProtocols.TLSv1_2
                );

            String clientId = "projects/" + options.projectId + "/locations/us-central1/registries/reg1/devices/dev1";

            // register to message received
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            client.Connect(clientId, null, token); //username is null, authentication via JWT

            if (client.IsConnected)
            {
                //publish 10 messages
                for (int i = 0; i <= 10; i++)
                {
                    Random rnd = new Random();
                    string strValue = dtnow + "," + Convert.ToString(rnd.Next());
                    strValue = strValue + "-n2";
                    msgId = client.Publish("/devices/dev1/events", Encoding.UTF8.GetBytes(strValue), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
                    client.MqttMsgPublished += client_MqttMsgPublished;
                    System.Threading.Thread.Sleep(500);
                }
                client.Disconnect(); //flush on disconnect
            }
            else
            {
                Console.WriteLine("Can not connect to project: {0}", options.projectId);
                Environment.Exit(0);
            }

        }

    }
}
