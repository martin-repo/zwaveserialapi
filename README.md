# C# Z-Wave Serial API

This API is for C#/.NET developers who want to create their own home automation tool. If you're looking for a fully featured solution you may want to use [Home Assistant](https://developers.home-assistant.io/docs/api/rest/), and connect to it via REST API and/or WebSocket API.

Since this API doesn't support Z-Wave protocol security it should not be used for home security, door locks, etc.

## Install

[![Nuget](https://img.shields.io/nuget/v/ZWaveSerialApi.Devices?style=for-the-badge)](https://www.nuget.org/packages/ZWaveSerialApi.Devices)

## Not planned (ie. will most likely not happen)
- Security (ie. S0 and S2 classed communication with nodes)
- Multi Channel
- Associations (device signalling other device, bypassing controller)
- Scenes

## Example

```cs
private static async Task DeviceExample()
{
    using var devices = new ZWaveDevices("COM3");

    var multiSensor = devices.GetDevices<AeotecMultiSensor6>().First();

    // Alternative:
    //var multiSensor = devices.GetDevice<AeotecMultiSensor6>("Kitchen");

    // Register for unsolicited home security notifications
    multiSensor.HomeSecurityMotionDetected += (sender, eventArgs) => { /* Motion detection started */ };
    multiSensor.HomeSecurityIdle += (sender, eventArgs) => { /* Motion detection stopped */ };

    // Get sensor values
    var temperatureReport = await multiSensor.GetTemperatureAsync(TemperatureScale.Celcius, CancellationToken.None);
    Console.WriteLine($"Temperature: {temperatureReport.Value}{temperatureReport.Unit}");

    var humidityReport = await multiSensor.GetHumidityAsync(HumidityScale.Percentage, CancellationToken.None);
    Console.WriteLine($"Humidity: {humidityReport.Value}{humidityReport.Unit}");
}
```
