[![Build status](https://dev.azure.com/martin-repo/ZWaveSerialApi/_apis/build/status/ZWaveSerialApi)](https://dev.azure.com/martin-repo/ZWaveSerialApi/_build/latest?definitionId=1)
[![Nuget](https://img.shields.io/nuget/vpre/ZWaveSerialApi.Devices?logo=nuget)](https://www.nuget.org/packages/ZWaveSerialApi.Devices)

# C# Z-Wave Serial API

This API is for C#/.NET developers who want to create their own home automation tool.

Since this API doesn't support Z-Wave protocol security it should not be used for burglar alarms, door locks, etc.

## Install

Install [`ZWaveSerialApi.Devices`](https://www.nuget.org/packages/ZWaveSerialApi.Devices) nuget for a high-level, simple experience.

Install [`ZWaveSerialApi`](https://www.nuget.org/packages/ZWaveSerialApi) nuget for a low-level, "Z-Wave domain knowledge required" experience.

## Examples

### Getting started
```cs
using var devices = new ZWaveDevices("COM3");
await devices.ConnectAsync();

var multiSensor = devices.GetDevices<AeotecMultiSensor6>().First();

// Register for unsolicited home security notifications
multiSensor.HomeSecurityMotionDetected += (_, _) =>
{
    /* Motion detection started */
};
multiSensor.HomeSecurityIdle += (_, _) =>
{
    /* Motion detection stopped */
};

// Get sensor values
var temperatureReport = await multiSensor.GetTemperatureAsync(TemperatureScale.Celcius);
Console.WriteLine($"Temperature: {temperatureReport.Value}{temperatureReport.Unit}");

var humidityReport = await multiSensor.GetHumidityAsync(HumidityScale.Percentage);
Console.WriteLine($"Humidity: {humidityReport.Value}{humidityReport.Unit}");

Console.ReadKey();
```
### Network settings persistance
When connecting, the API will attempt to query all unknown devices. Devices that are sleeping will be added when they wake up.

Cut down startup time by persisting the network information.
```cs
using var devices = new ZWaveDevices("COM3");
await devices.LoadAsync("ZWaveDevices.settings");
await devices.ConnectAsync();

// ... program logic ...

await devices.SaveAsync("ZWaveDevices.settings");
```

### Using device location
When using multiple devices of the same type, it helps to assign a location to each device.
```cs
using var devices = new ZWaveDevices("COM3");
await devices.ConnectAsync();

// Set location
var unknownMultiSensor = devices.GetDevices<AeotecMultiSensor6>().First();
unknownMultiSensor.Location = "Kitchen";

// Get by location
var kitchenMultiSensor = devices.GetDevice<AeotecMultiSensor6>("Kitchen");
```

### Converting unsolicited values
Units for unsolicited values are defined in the configuration of each device. If there is a need to use alternative units they can be converted manually.
```cs
using var devices = new ZWaveDevices("COM3");
await devices.ConnectAsync();

static void OutputBothTemperatureUnits(MultilevelSensorReport temperatureReport)
{
    switch (temperatureReport.Scale)
    {
        case TemperatureScale.Celcius:
            var farenheitValue = temperatureReport.Value * 9 / 5 + 32;
            var (farenheitUnit, _) = AttributeHelper.GetUnit(TemperatureScale.Fahrenheit);
            Console.WriteLine($"Temperature = {temperatureReport.Value}{temperatureReport.Unit} / {farenheitValue}{farenheitUnit}");
            break;
        case TemperatureScale.Fahrenheit:
            var celciousValue = (temperatureReport.Value - 32) * 5 / 9;
            var (celciousUnit, _) = AttributeHelper.GetUnit(TemperatureScale.Fahrenheit);
            Console.WriteLine($"Temperature = {celciousValue}{celciousUnit} / {temperatureReport.Value}{temperatureReport.Unit}");
            break;
    }
}

var aerq = devices.GetDevices<AeotecAerqSensor>().First();
aerq.TemperatureReport += (_, eventArgs) => OutputBothTemperatureUnits(eventArgs.Report);

Console.ReadKey();
```

## Planned
- Get/set device configuration parameters
- Include/exclude devices to network

## Not planned
- Security (ie. S0 and S2 classed communication with nodes)
- Multi Channel
- Associations (device signalling other device, bypassing controller)
- Scenes
