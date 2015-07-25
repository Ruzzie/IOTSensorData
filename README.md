# IOTSensorData

A simple store API for IOT. Inspired by dweet.io but the need for control over the data. This API can be used to store dynamic data from your IOT devices and retrieve that data.

## Getting started

- Checkout code
- Add connectionstring to mongo (including database name) in web.config and app.config of integration test project 
- Add connectionstring to redis in web.config and app.config of integration test project

```xml
  <appSettings>
	<add key="mongodbconnectionstring" value="xxx" />
	<add key="redisconnectionstring" value="xxx" />
  </appSettings>
```

## Technologies used
* Asp.Net WebApi
* Swagger
* MongoDb
* Redis

### Todo
- [x] Store dynamic data structure
- [x] Caching of latest data
- [x] Retrieving latest data
- [x] Improve serialization performance of dynamic type for caching (good enough for now)
- [ ] Create an overview page of Items
- [ ] Export data for a timeperiod for an item
- [ ] Create pipeline for data processing
- [ ] Pub / Sub with socket.io for custom visualization purposes
- [ ] Security strategy
- [ ] Low payload protocol and library for IoT devices
