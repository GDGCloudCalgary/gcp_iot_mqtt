
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
            long exp = iat + 3600; // Token expires in 3600 seconds

            var claims = new Dictionary<string, object>()
            {
                {"iat", iat} // Issued at
               ,{"exp", exp} // expiration time
               ,{"aud",options.projectId} // audience is gcp project
            };

            X509Certificate x509_roots = new X509Certificate(currentExcutionPath + "\\" + "roots.pem");

            X509Certificate2 x509Certificate2 = new X509Certificate2(currentExcutionPath + "\\" + "ia.p12"
                , "123456789" // Your token password, note: hardcoding password isn't a good security practice
                , X509KeyStorageFlags.Exportable);

            string token = Jose.JWT.Encode(claims
                , x509Certificate2.PrivateKey
                , JwsAlgorithm.RS256); // Using RSA Signature with SHA-256 asymmetric algorithm


            MqttClient client = new MqttClient("mqtt.googleapis.com", // Google Cloud mqtt API host
                8883 // ssl mqtt port
                , true // secure = true
                , x509_roots //caCert,CA certificate for secure connection, the CA certificate used to sign the broker certificate you’ll connect to
                , x509Certificate2 //ClientCert,Client certificate
                , MqttSslProtocols.TLSv1_2
                );

            // Subscribe to be notified about received messages,
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            // event handler for published messages
            client.MqttMsgPublished += client_MqttMsgPublished;

            // Building clientId as required by google mqtt 
            String clientId = "projects/" + options.projectId + "/locations/us-central1/registries/reg1/devices/dev1";
            
            // Username is null, authentication via JWT
            client.Connect(clientId, null, token); 

            if (client.IsConnected)
            {
                //publish 10 messages
                for (int i = 0; i <= 10; i++)
                {

                    Random rnd = new Random(); // just a random number for sample date
                    string strValue = dtnow + "," + Convert.ToString(rnd.Next());
                    strValue = strValue + "-anything";

                    byte[] bMessage = Encoding.UTF8.GetBytes(strValue);

                    // This call returns immediately the id assigned to the message that will be sent shortly after. 
                    // The library works in an asynchronous way with an internal queue and an internal thread for publishing messages.
                    // An error means that the client made more attempts to send the message but it couldn’t reach the broker
                    // of course this is true only for QoS level 1 and 2 where an acknowledge sequence from broker is expected
                    msgId = client.Publish("/devices/dev1/events"
                        , bMessage
                        , MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE
                        , false); //publish a retained message if set to true

                    // 1000ms between each publish
                    System.Threading.Thread.Sleep(1000);
                }
                client.Disconnect(); // Flush on disconnect
            }
            else
            {
                Console.WriteLine("Can not connect to project: {0}", options.projectId);
                Environment.Exit(0);
            }
        }
    }
}
