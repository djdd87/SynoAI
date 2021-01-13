# SynoAI
A Synology Surveillance Station notification system utilising DeepStack AI

## Example appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },

  "Url": "http://192.168.0.0:5000",
  "User": "SynologyUser",
  "Password": "SynologyPassword",

  "AI": {
    "Type": "DeepStack",
    "Url": "http://10.0.0.10:83",
    "MinSizeX": 100,
    "MinSizeY": 100
  },

  "Notifier": {
    "Type": "Pushbullet",
    "ApiKey": "0.123456789"
  },

  "Cameras": [
    {
      "Name": "Driveway",
      "Types": [ "Person", "Car" ],
      "Threshold": 45
    },
    {
      "Name": "Back Door",
      "Types": [ "Person" ],
      "Threshold": 30
    }
  ]
}
```
