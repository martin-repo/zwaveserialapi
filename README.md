# C# Z-Wave Serial API

This API is for C#/.NET developers who want to create their own home automation tool.

Since this API doesn't support Z-Wave protocol security it should not be used for burglar alarms, door locks, etc.

## Install

[![Build status](https://dev.azure.com/martin-repo/ZWaveSerialApi/_apis/build/status/ZWaveSerialApi)](https://dev.azure.com/martin-repo/ZWaveSerialApi/_build/latest?definitionId=1)
[![Nuget](https://img.shields.io/nuget/vpre/ZWaveSerialApi.Devices?logo=nuget)](https://www.nuget.org/packages/ZWaveSerialApi.Devices)

## Planned (not yet in scope, no release date)

Use Z-Wave PC Controller software for now (installed via Silabs' [Simplicity Studio](https://www.silabs.com/developers/simplicity-studio)).
- Get/set device configuration parameters
- Include/exclude devices to network

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
