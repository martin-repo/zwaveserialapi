# C# Z-Wave Serial API

This API is for C#/.NET developers who want to create their own home automation tool using a Z-Wave USB stick, such as the [Aeotec Z-Stick 7](https://aeotec.com/z-wave-usb-stick/z-stick-7.html).

Since this API doesn't support Z-Wave protocol security (S0 or S2) it should not be used for burglar alarms, door locks, etc.

## Requirements
- .NET 5.0
- Devices should already be included/added to the network. Use any other software for this purpose, eg. [Z-Wave PC Controller](https://www.silabs.com/developers/simplicity-studio) (part of Simplicity Studio).

## Examples

### Getting started
```cs
using var network = new ZWaveNetwork("COM3");
await network.ConnectAsync();

var multiSensor = network.GetDevices<AeotecMultiSensor6>().First();

// Register for unsolicited motion notifications
multiSensor.MotionDetected += (_, _) =>
{
    /* Motion detection started */
};
multiSensor.MotionIdle += (_, _) =>
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
### Device parameters
Read and write device configuration parameters.
```cs
var multiSensor = network.GetDevices<AeotecMultiSensor6>().First();

// Get current value
var timeout = await multiSensor.Parameters.MotionTimeout.GetAsync();
Console.WriteLine($"MotionTimeout: {timeout.TotalSeconds}");

// Set new value
await multiSensor.Parameters.MotionTimeout.SetAsync(TimeSpan.FromMinutes(1));
```
### Using device location
When using multiple devices of the same type, it helps to assign a location to each device.
```cs
// Set location
var unknownMultiSensor = network.GetDevices<AeotecMultiSensor6>().First();
unknownMultiSensor.Location = "Kitchen";

// Get by location
var kitchenMultiSensor = network.GetDevice<AeotecMultiSensor6>("Kitchen");
```
### Converting unsolicited values
Units for unsolicited values are defined in the configuration of each device. If there is a need to use alternative units they can be converted manually.
```cs
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
```
### Custom device types
[Create an issue](https://github.com/martin-repo/zwaveserialapi/issues) for missing devices. Until they are part of the API, you can create a custom type. See [CustomMultilevelSensor.cs](https://github.com/martin-repo/zwaveserialapi/blob/main/src/DeveloperTest/CustomMultilevelSensor.cs) source code and below example for how it's used.
```cs
using var network = new ZWaveNetwork("COM3");

// Get the device type of the unsupported device, either
// 1) Input the data manually, eg.:
//    var customDeviceType = new DeviceType(0x0086, 0x0002, 0x0064);
// *OR*
// 2) Get the data from a device already on the network, eg.:
var customDeviceType = network.GetUnsupportedDeviceTypes().First();

network.RegisterCustomDeviceType(customDeviceType, (client, deviceState) => new CustomMultilevelSensor(client, deviceState));

await network.ConnectAsync();

var multiSensor = network.GetDevices<CustomMultilevelSensor>().First();
var temperature = await multiSensor.GetTemperatureAsync(TemperatureScale.Celsius);
```
## Features not supported
- Z-Wave security (ie. S0 and S2 classed communication with nodes)
- Multi channel communication
- Association groups (device signalling other device, bypassing controller)
- Scenes
- Inclusion/exclusion of devices