# SynoAI
A Synology Surveillance Station notification system utilising DeepStack AI, inspired by Christopher Adams' [sssAI](https://github.com/Christofo/sssAI) implementation.

The aim of the solution is to reduce the noise generated by Synology Surveillance Station's motion detection by routing all motion events via a [Deepstack](https://deepstack.cc/) docker image to look for particular objects, e.g. people.

While sssAI is a great solution, it is hamstrung by the Synology notification system to send motion alerts. Due to the delay between fetching the snapshot, processing the image using the AI and requesting the alert, it means that the image attached to the Synology notification is sometimes 5-10 seconds after the motion alert was originally triggered.

SynoAI aims to solve this problem by side-stepping the Synology notifications entirely by allowing other notification systems to be used.

## Buy Me A Coffee! :coffee:

I made this application mostly for myself in order to improve upon Christopher Adams' original idea and don't expect anything in return. However, if you find it useful and would like to buy me a coffee, feel free to do it at [__Buy me a coffee! :coffee:__](https://buymeacoff.ee/djdd87). This is entirely optional, but would be appreciated! Or even better, help supported this project by contributing changes such as expanding the supported notification systems (or even AIs).

## Table of Contents

* [Features](#features)
* [Config](#config)
* [Support AIs](#supported-ais)  
  * [Deepstack](#deepstack)
* [Notifications](#notifications)
  * [Pushbullet](#pushbullet)
  * [Webhook](#webhook)
  * [HomeAssistant](#homeassistant)
* [Caveats](#caveats)
* [Configuration](#configuration)
  * [1) Configure Deepstack](#1-configure-deepstack)
  * [2) Configure SynoAI](#2-configure-synoai)
  * [3) Create Action Rules](#3-create-action-rules)
  * [Summary](#summary)
* [Docker](#docker)
  * [Docker Configuration](#docker-configuration)
  * [Docker Compose](#docker-compose)
* [Example appsettings.json](#example-appsettingsjson)
* [Problems/Debugging](#problemsdebugging)
  * [Logging](#logging)
  * [Common Synology Error Codes](#common-synology-error-codes)

## Features
* Triggered via an Action Rule from Synology Surveillance Station
* Works using the camera name and requires no technical knowledge of the Surveillance Station API in order to retrieve the unique camera ID
* Uses an AI for object/person detection
* Produces an output image with highlighted objects using the original image at the point of motion detection
* Sends notification(s) at the point of notification with the processed image attached
* The AI does not need to run on the Synology box and can be run on an another server.

## Config

An example appsettings.json configuration file can be found [here](#example-appsettingsjson) and all configuration for notifications and AI can be found under their respective sections. The following are the top level configs for communication with Synology Surveillance Station:

* Url [required]: The URL and port of your NAS, e.g. http://{IP}:{Port}
* User [required]: The user that will be used to request API snapshots
* Password [required]: The password of the user above
* Cameras [required]: An array of camera objects
  * Name: [required]: The name of the camera on Surveillance Station
  * Types: [required]: An array of types that will trigger a notification when detected by the AI, e.g. ["Person", "Car"]
  * Threshold [required]: An integer denoting the required confidence of the AI to trigger the notification, e.g. 40 means that the AI must be 40% sure that the object detected was a person before SynoAI sends a notification.
* Notifiers [required]: See [notifications](#notifications)
* Delay [optiona] (Default: 5000): The period of time in milliseconds (ms) that must occur between the last motion detection of camera and the next time it'll be processed. i.e. if your delay is set to 5000 and your camera reports motion 4 seconds after it had already reported motion to SynoAI, then the check will be ignored. However, if the report from Surveillance Station is more than 5000ms, then the cameras image will be processed.
* DrawMode [optional] (Default: Matches): Whether to draw all predictions from the AI on the capture image:
  * Matches: Will draw boundary boxes over any object/person that matches the types defined on the cameras
  * All: Will draw boundary boxes over any object/person that the AI detected
  * Off: Will not draw boundary boxes (note - this will speed up time between detection and notification as SynoAI will not have to manipulate the image)
* Font [optiona] (Default: Tahoma): The font to use when labelling the boundary boxes on the output image
* FontSize [optiona] (Default: 12): The size of the font to use (in pixels) when labelling the boundary boxes on the output image
* TextOffsetX [optional] (Default: 2) : The number of pixels to offset the label from the left of the inside of the boundary image on the output image
* TextOffsetY [optional] (Default: 2) : The number of pixels to offset the label from the top of the inside of the boundary image on the output image.

## Supported AIs
* [Deepstack](https://deepstack.cc/)

In order to specify the AI to use, set the Type property against the AI section in the config:

```json
"AI": {
  "Type": "DeepStack"
}
```

### Deepstack

The Deepstack API is a free to use AI that can identify objects, faces and more. Currently SynoAI makes use of the object detection model, which allows detection of people, cars, bicycles, trucks and even giraffes! For a full list of supported types see the [Deepstack documentation](https://docs.deepstack.cc/object-detection/#classes).

```json
"AI": {
  "Type": "DeepStack",
  "Url": "http://10.0.0.10:83",
  "MinSizeX": 100,
  "MinSizeY": 100
}
```
* Url [required]: The URL of the AI to POST the image to
* MinSizeX [optional] (Default: 50): The minimum size in pixels that the object must be to trigger a change
* MinSizeY [optional] (Default: 50): The minimum size in pixels that the object must be to trigger a change.

## Notifications

Multiple notifications can be triggered when an object is detected. Each notification will have a defined "Type" and the sections below explain how each notification should be defined.

Supported notifications:

* Pushbullet
* Webhook

### Pushbullet
The [Pushbullet](https://www.pushbullet.com/) notification will send an image and a message containing a list of detected object types. An API key will need to be obtained from your Pushbullet account. Currently the notification will be sent to all devices that the API key belongs to.

```json
{
  "Type": "Pushbullet",
  "ApiKey": "0.123456789"
}
```
* ApiKey [required]: The API key for the Pushbullet service

### Webhook 
The webhook notification will POST an image to the specified URL with a specified field name.

```json
{
  "Url": "http://servername/resource",
  "Method": "POST",
  "Field": "image"
}
```
* Url [required]: The URL to send the image to
* Method [optional] (Default: ```POST```): The HTTP method to use, e.g. POST, PUT
* Field [optional] (Default: ```image```): The field name of the image in the POST data.

### HomeAssistant
Integration with HomeAssistant can be achieved using the [Push](https://www.home-assistant.io/integrations/push/) integration by following the instructions on that page by calling the HomeAssistant webhook using the SynoAI Webhook notification. 

## Caveats
* SynoAI still relies on Surveillance Station triggering the motion alerts
* Looking for an object, such as a car on a driveway, will continually trigger alerts if that object is in view of the camera when Surveillance Station detects movement, e.g. a tree blowing in the wind.

## Configuration
The configuration instructions below are primarily aimed at running SynoAI in a docker container on DSM (Synology's operating system). Docker will be required anyway as Deepstack is assumed to be setup inside a Docker container. It is entirely possible to run SynoAI on a webserver instead, or to install it on a Docker instance that's not running on your Synology NAS, however that is outside the scope of these instructions. Additionally, the configuration of the third party notification systems (e.g. generating a Pushbullet API Key) is outside the scope of these instructions and can be found on the respective applications help guides.

The top level steps are:
* Setup the Deepstack Docker image on DSM
* Setup the SynoAI image on DSM
* Add Action Rules to Synology Surveillance Station's motion alerts in order to trigger the SynoAI API.

### 1) Configure Deepstack
The following instructions explain how to set up the Deepstack image using the Docker app built into DSM. Before continuing, you'll need to obtain a *free* API key from [Deepstack](https://deepstack.css). 

* Download the deepquestai/deepstack image by either;
  * Searching the registry for deepquestai/deepstack
  * Choose the tag cpu-x6-beta, or noavx; this is dependent on the capabilities of your NAS.
* Run the image
* Enter a name for the image, e.g. deepstack
* Edit the advanced settings
* Enable auto restarts
* On the port settings tab;
  * Enter a port mapping to port 5000 from an available port on your NAS, e.g. 83
* On the Environment tab;
  * Set MODE: Low
  * Set VISION-DETECTION: True
* Accept the advnaced settings and then run the image
* Open a webbrowser and go to the Deepstack page by navigating to http://{YourIP}:{YourDeepstackPort}
* If you've set everything up successfully then you will be able to enter your API key in here and move onto the next step.
   
### 2) Configure SynoAI
The following instructions explain how to set up the SynoAI image using the Docker app built into DSM. For docker-compose, see the example file in the src, or in the documentation below.

* Create a folder called synoai (this will contain your Captures directory and appsettings.json)
* Put your appsettings.json file in the folder
* Create a folder called Captures 
* Open Docker in DSM
* Download the djdd87/synoai:latest image by either;
  * Searching the registry for djdd87/synoai
  * Going to the image tab and;
    * Add > Add from URL
    * Enter https://hub.docker.com/r/djdd87/synoai
* Run the image
* Enter a name for the image, e.g. synoai
* Edit the advanced settings
* Enable auto restarts
* On the volumes tab;
   * Add a file mapping from your appsettings.json to /app/appsettings.json
   * Add a folder mapping from your captures directory to /app/Captures (optional)
* On the port settings tab;
   * Enter a port mapping to port 80 from an available port on your NAS, e.g. 8080

### 3) Create Action Rules
The next step is to configure actions inside Surveillance Station that will call the SynoAI API. 

* Open up Surveillance Station
* Open Action Rules
* Create a new rule and enter;
  * Name: A name for the action e.g. Trigger SynoAI - Driveway
  * Rule type: Triggered (Default)
  * Action type: Interruptible (Default)
* Click next to open the Event tab and enter;
  * Event source: Camera
  * Device: Your camera, e.g. Driveway
  * Event: Motion Detected
* Click next to open the Action tab and enter;
  * Action device: Webhook
  * URL: http://{YourIP}:{YourPort}/Camera/{CameraName}, e.g. http://10.0.0.10:8080/Camera/Driveway, where
    * YourIP: Is the IP of your NAS, or the Docker server where SynoAI is deployed
    * YourPort: The port that the SynoAI image is listening on as you configured above.
    * CameraName: The name of the camera, e.g. Driveway
  * Username: Blank
  * Password: Blank
  * Method: GET
  * Times: 1
* Click test send and if everything is set up correctly, then you'll get a green tick
* Click next and the action will now be created.

### Summary

Congratulations, you should now have a trigger calling the SynoAI API for your camera every time Surveillance Station detects motion. In order to set up multiple cameras, just create a new Action Rule for each camera.

Note that SynoAI is still reliant on Surveillance Station detecting the motion, so this will need some tuning on your part. However, it's now possible to up the sensitivity and avoid false-positives as SynoAI will only notify you (via your preferred notification system/app) when an object is detected, e.g. a Person.

## Docker
SynoAI can be installed as a docker image, which is [available from DockerHub](https://hub.docker.com/r/djdd87/synoai).

### Docker Configuration
The image can be pulled using the Docker cli by calling:
```
docker pull djdd87/synoai:latest
```
To run the image a volume must be specified to map your appsettings.json file. Additionally a port needs mapping to port 80 in order to trigger the API. Optionally, the Captures directory can also be mapped to easily expose all the images output from SynoAI.

```
docker run 
  -v /path/appsettings.json:/app/appsettings.json 
  -v /path/captures:/app/Captures 
  -p 8080:80 djdd87/synoai:latest
```

### Docker-Compose
```yaml
version: '3.4'

services:
  synoai:
    image: djdd87/synoai:latest
    build:
      context: .
      dockerfile: ./Dockerfile
    ports:
      - "8080:80"
    volumes:
      - /docker/synoai/captures:/app/Captures
      - /docker/synoai/appsettings.json:/app/appsettings.json
```

## Example appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Warning"
    }
  },

  "Url": "http://10.0.0.10:5000",
  "User": "SynologyUser",
  "Password": "SynologyPassword",

  "AI": {
    "Type": "DeepStack",
    "Url": "http://10.0.0.10:83",
    "MinSizeX": 100,
    "MinSizeY": 100
  },

  "Notifiers": [
    {
      "Type": "Pushbullet",
      "ApiKey": "0.123456789"
    },
    {
      "Type": "Webhook",
      "Url": "http://server/images",
      "Method": "POST",
      "Field": "image"
    }
  ],

  "Cameras": [
    {
      "Name": "Driveway",
      "Types": [ "Person", "Car" ],
      "Threshold": 45
    },
    {
      "Name": "BackDoor",
      "Types": [ "Person" ],
      "Threshold": 30
    }
  ]
}
```

## Problems/Debugging

### Logging
If issues are encountered, to get more verbose information in the logs, change the logging to the following:

```json  
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft": "Warning",
    "Microsoft.Hosting.Lifetime": "Information"
  }
}
```
This will output the full information log and help identify where things are going wrong, as well as displaying the confidence percentages from Deepstack.

### Common Synology Error Codes
* 100: Unknown error
* 101: Invalid parameters
* 102: API does not exist
* 103: Method does not exist
* 104: This API version is not supported
* 105: Insufficient user privilege
  * If this occurs, check your username and password, or;
  * Try creating a specific user for Synology Surveillance Station
* 106: Connection time out
* 107: Multiple login detected
