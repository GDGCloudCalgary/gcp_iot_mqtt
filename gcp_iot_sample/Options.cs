using CommandLine;

namespace GoogleCloudIoTSamples
{
    class Options
    {
        [Option('p', "GoogleCloudProjectId", HelpText = "Google Cloud IoT project ID.", Required = true)]
        public string projectId { get; set; }

        [Option('r', "RegistryId", HelpText = "Google Cloud IoT Registry ID.", Required = true)]
        public string registryId { get; set; }

        [Option('d', "DeviceId", HelpText = "Google Cloud IoT Device ID.", Required = true)]
        public string deviceId { get; set; }

        [Option('a', "cloudRegion", HelpText = "Google Cloud IoT Project Cloud Region.", Required = true)]
        public string cloudRegion { get; set; }

        [Option('t', "topic", HelpText = "Google Cloud pub/sub topic.", Required = true)]
        public string topic { get; set; }

        [Option('h', "help", HelpText = "Usage: dotnet gcp_iot_sample.dll -p projiotid", Required = false, DefaultValue = false)]
        public bool Help { get; set; }

    }
}
