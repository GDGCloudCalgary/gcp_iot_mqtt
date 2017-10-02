using CommandLine;

namespace GoogleCloudIoTSamples
{
    class Options
    {
        [Option('p', "GoogleCloudProjectId", HelpText = "Google Cloud IoT project ID.", Required = true)]
        public string projectId { get; set; }

        [Option('h', "help", HelpText = "Usage: dotnet gcp_iot_sample.dll -p projiotid", Required = false, DefaultValue = false)]
        public bool Help { get; set; }

    }
}
