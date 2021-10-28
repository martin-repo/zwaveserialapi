[![Build status](https://dev.azure.com/martin-repo/ZWaveSerialApi/_apis/build/status/ZWaveSerialApi)](https://dev.azure.com/martin-repo/ZWaveSerialApi/_build/latest?definitionId=1)
[![Nuget](https://img.shields.io/nuget/vpre/ZWaveSerialApi.Devices?logo=nuget)](https://www.nuget.org/packages/ZWaveSerialApi.Devices)

# C# Z-Wave Serial API

This API is for C#/.NET developers who want to create their own home automation tool using a Z-Wave USB stick, such as the [Aeotec Z-Stick 7](https://aeotec.com/z-wave-usb-stick/z-stick-7.html).

Since this API currently doesn't support Z-Wave protocol security it should not be used for burglar alarms, door locks, etc.

## Install

Install [`ZWaveSerialApi.Devices`](https://www.nuget.org/packages/ZWaveSerialApi.Devices) nuget for a high-level, simple experience. (Examples below)

Install [`ZWaveSerialApi`](https://www.nuget.org/packages/ZWaveSerialApi) nuget for a low-level, "Z-Wave domain knowledge required" experience.

## Examples

### Getting started
```cs
using var network = new ZWaveNetwork("COM3");
await network.ConnectAsync();

var multiSensor = network.GetDevices<AeotecMultiSensor6>().First();

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
var temperature = await multiSensor.GetTemperatureAsync(TemperatureScale.Celsius);
Console.WriteLine($"Temperature: {temperature.Value}{temperature.Unit}");

var humidity = await multiSensor.GetHumidityAsync(HumidityScale.Percentage);
Console.WriteLine($"Humidity: {humidity.Value}{humidity.Unit}");

Console.ReadKey();
```
### Network settings persistance
When connecting, the API will attempt to query all unknown devices. Devices that are sleeping will be added when they wake up.

Cut down startup time by persisting the network information.
```cs
using var network = new ZWaveNetwork("COM3");
await network.LoadAsync("ZWaveNetwork.json");
await network.ConnectAsync();

// ... program logic ...

await network.SaveAsync("ZWaveNetwork.json");
```

### Using device location
When using multiple devices of the same type, it helps to assign a location to each device.
```cs
using var network = new ZWaveNetwork("COM3");
await network.ConnectAsync();

// Set location
var unknownMultiSensor = network.GetDevices<AeotecMultiSensor6>().First();
unknownMultiSensor.Location = "Kitchen";

// Get by location
var kitchenMultiSensor = network.GetDevice<AeotecMultiSensor6>("Kitchen");
```

### Converting unsolicited values
Units for unsolicited values are defined in the configuration of each device. If there is a need to use alternative units they can be converted manually.
```cs
using var network = new ZWaveNetwork("COM3");
await network.ConnectAsync();

static void OutputBothTemperatureUnits(MultilevelSensorReport temperature)
{
    switch (temperature.Scale)
    {
        case TemperatureScale.Celsius:
            var farenheitValue = temperature.Value * 9 / 5 + 32;
            var (farenheitUnit, _) = AttributeHelper.GetUnit(TemperatureScale.Fahrenheit);
            Console.WriteLine($"Temperature = {temperature.Value}{temperature.Unit} / {farenheitValue}{farenheitUnit}");
            break;
        case TemperatureScale.Fahrenheit:
            var celsiusValue = (temperature.Value - 32) * 5 / 9;
            var (celsiusUnit, _) = AttributeHelper.GetUnit(TemperatureScale.Fahrenheit);
            Console.WriteLine($"Temperature = {celsiusValue}{celsiusUnit} / {temperature.Value}{temperature.Unit}");
            break;
    }
}

var aerq = network.GetDevices<AeotecAerqSensor>().First();
aerq.TemperatureReport += (_, eventArgs) => OutputBothTemperatureUnits(eventArgs.Report);

Console.ReadKey();
```

### Custom device types
[Create an issue](https://github.com/martin-repo/zwaveserialapi/issues) for missing devices. Until they are part of the API, you can create a custom type. See [CustomMultiSensor6.cs](https://github.com/martin-repo/zwaveserialapi/blob/main/src/DeveloperTest/CustomMultiSensor6.cs) source code and below example for how it's used.
```cs
using var network = new ZWaveNetwork("COM3");

var customDeviceType = new DeviceType(0x0086, 0x0002, 0x0064);
network.RegisterCustomDeviceType(customDeviceType, (client, deviceState) => new CustomMultiSensor6(client, deviceState));

await network.ConnectAsync();

var multiSensor = network.GetDevices<CustomMultiSensor6>().First();
var temperature = await multiSensor.GetTemperatureAsync(TemperatureScale.Celsius);
```

## Planned
- Get/set device configuration parameters
- Include/exclude devices to network

## Not planned
- Security (ie. S0 and S2 classed communication with nodes)
- Multi Channel
- Associations (device signalling other device, bypassing controller)
- Scenes
