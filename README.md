# VR-Notifier

## Purpose
The projects purpose is creating a microservice for sending release notifications to discord. There are also some features for interaction with discord users present.
The API will also offer the possibility to query the releases.
The microservice integrates with the other VR* repositories.

## How to setup
You need at least Docker Engine Release 18.06.0 (due to docker-compose file format v3.7) and docker-compose installed.
Be sure to check out the .env file. The sensible parts of the service are configured via the environment. Normally the .env file would not be part of
the repository (I know), but as everything runs locally in Docker and is not accessible from outside, there is no problem with it.
The only environment variable *you have to provide yourself is the API-Key*!

1. Provide a value for the environment variable *VRNotifier_NotificationSettings__DiscordSettings__ApiKey* in .env.example and rename the file to .env
2. Execute *docker-compose up* 
    * builds the images, spins up the containers, reads the .env file and starts the containers

## How to use
VR-Notifier exposes its API on localhost:${VRNotifier_ApiPort}.
Under /resources there is a Postman-Collection with example requests which can be used to test the service. Just dont forget to update the port in accordance with ${VRNotifier_ApiPort}.
Also you need VRPersistence up and running at ${VRNotifier_VRPersistenceClientSettings__Endpoint} as this service communicates with it.
