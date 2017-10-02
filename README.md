# Google Cloud IoT Core .net Sample
This folder contains .net core samples that demonstrate an overview of the Google Cloud IoT Core platform.

This sample publishes MQTT messages based on .net core to Google Cloud Core IoT API.

general help for setting up the project on gcp available in "gen certificates help.txt".


## Quickstart
1. before start, make sure you have pub/sub and core iot api is already enabled.

2. If you need install the gCloud CLI, just follow readme docx document to setup your IoT project in Google cloud.

3. Create a PubSub topic: ie. events

4. Create a device registry: ie. reg1

5. Register a device: ie. dev1.

6. Follow the instruction `gen certificates help.txt` to generate your signing keys.

7. Run the application:
  dotnet gcp_iot_sample.dll -p {projectId} -r {registryId} -d {deviceId} -a {cloudRegion}

## Example

C:\>dotnet gcp_iot_sample.dll -p projiotid -r reg1 -d dev1 -a us-central1

Results:

MessageId = 1 Published = True

MessageId = 2 Published = True

MessageId = 3 Published = True

...
