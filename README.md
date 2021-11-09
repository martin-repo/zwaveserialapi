[![Build status](https://dev.azure.com/martin-repo/ZWaveSerialApi/_apis/build/status/ZWaveSerialApi)](https://dev.azure.com/martin-repo/ZWaveSerialApi/_build/latest?definitionId=1)
[![Nuget](https://img.shields.io/nuget/vpre/ZWaveSerialApi.Devices?logo=nuget)](https://www.nuget.org/packages/ZWaveSerialApi.Devices)

## Install

Install [`ZWaveSerialApi.Devices`](https://www.nuget.org/packages/ZWaveSerialApi.Devices) from NuGet.org

# C# Z-Wave Serial API

This API is for C#/.NET developers who want to create their own home automation tool using a Z-Wave USB stick, such as the [Aeotec Z-Stick 7](https://aeotec.com/z-wave-usb-stick/z-stick-7.html).

Since this API doesn't support Z-Wave protocol security (S0 or S2) it should not be used for burglar alarms, door locks, etc.

## Requirements
- .NET 5.0

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
```
### Adding (including) devices
```cs
using var network = new ZWaveNetwork("COM3");
await network.ConnectAsync();

// Add/include device
var (success, device) = await network.AddDeviceAsync();

// Add/include device (optional callback when controller is ready)
(success, device) = await network.AddDeviceAsync(
                        () => Console.WriteLine("Initiate inclusion on device (ie. press button according to manual."));

// Add/include device (optional initialization for wake-up devices)
(success, device) = await network.AddDeviceAsync(
                        wakeUpInitializationFunc: async wakeUpDevice =>
                        {
                            const int IntervalHours = 2;
                            var wakeUpCapabilities = await wakeUpDevice.GetWakeUpIntervalCapabilitiesAsync();
                            var intervalSeconds = TimeSpan.FromHours(IntervalHours).TotalSeconds
                                                  - TimeSpan.FromHours(IntervalHours).TotalSeconds
                                                  % wakeUpCapabilities.IntervalStep.TotalSeconds;
                            await wakeUpDevice.SetWakeUpIntervalAsync(TimeSpan.FromHours(intervalSeconds));
                        });

// Add/include device (optional cancellation tokens)
// The cancellation token will stop the adding process immediately. This may leave the network in an unusable state which requires a reconnect.
// The abort token will stop the adding process nicely until a device is found. Once a device has been found then the token will do nothing.
var cancellationToken = new CancellationTokenSource().Token;
var abortRequestedToken = new CancellationTokenSource().Token;
(success, device) = await network.AddDeviceAsync(cancellationToken: cancellationToken, abortRequestedToken: abortRequestedToken);
```
### Removing (excluding) devices
```cs
using var network = new ZWaveNetwork("COM3");
await network.ConnectAsync();

// Remove/exclude device
await network.RemoveDeviceAsync();

// Remove/exclude device (optional callback when controller is ready)
await network.RemoveDeviceAsync(() => Console.WriteLine("Initiate exclusion on device (ie. press button according to manual."));

// Remove/exclude device (optional cancellation tokens)
// The cancellation token will stop the removal process immediately. This may leave the network in an unusable state which requires a reconnect.
// The abort token will stop the removal process nicely until a device is found. Once a device has been found then the token will do nothing.
var cancellationToken = new CancellationTokenSource().Token;
var abortRequestedToken = new CancellationTokenSource().Token;
await network.RemoveDeviceAsync(cancellationToken: cancellationToken, abortRequestedToken: abortRequestedToken);
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
### Wake up processing
Make calls to battery operated devices while they are awake, either every time or just once.
```cs
var multiSensor = network.GetDevices<AeotecMultiSensor6>().First();
multiSensor.WakeUpNotification += async (_, _) =>
{
    // ... processing every time device wakes up ...
};

async Task MultiSensorSetupAsync(object? sender, EventArgs eventArgs)
{
    // ... one-time setup of device when it wakes up ...

    multiSensor.WakeUpNotification -= MultiSensorSetupAsync;
}

multiSensor.WakeUpNotification += MultiSensorSetupAsync;
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
- MultiChannel/MultiCast
- Association groups (device signalling other device, bypassing controller)
- Scenes
