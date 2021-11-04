// -------------------------------------------------------------------------------------------------
// <copyright file="GitHubSnippets.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace DeveloperTest
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;
    using ZWaveSerialApi.Devices;
    using ZWaveSerialApi.Devices.Brands.Aeotec;
    using ZWaveSerialApi.Utilities;

    internal class GitHubSnippets
    {
        public async Task CustomDeviceType()
        {
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
        }

        public async Task GettingStarted()
        {
            using var network = new ZWaveNetwork("COM3");
            await network.ConnectAsync();

            var multiSensor = network.GetDevices<AeotecMultiSensor6>().First();

            // Register for unsolicited home security notifications
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
        }

        public async Task Location()
        {
            using var network = new ZWaveNetwork("COM3");
            await network.ConnectAsync();

            // Set location
            var unknownMultiSensor = network.GetDevices<AeotecMultiSensor6>().First();
            unknownMultiSensor.Location = "Kitchen";

            // Get by location
            var kitchenMultiSensor = network.GetDevice<AeotecMultiSensor6>("Kitchen");
        }

        public async Task Settings()
        {
            using var network = new ZWaveNetwork("COM3");
            await network.LoadAsync("ZWaveNetwork.json");
            await network.ConnectAsync();

            // ... program logic ...

            await network.SaveAsync("ZWaveNetwork.json");
        }

        public async Task UpdateParameter()
        {
            using var network = new ZWaveNetwork("COM3");
            await network.ConnectAsync();

            var multiSensor = network.GetDevices<AeotecMultiSensor6>().First();

            // Get current value
            var timeout = await multiSensor.Parameters.MotionTimeout.GetAsync();
            Console.WriteLine($"MotionTimeout: {timeout.TotalSeconds}");

            // Set new value
            await multiSensor.Parameters.MotionTimeout.SetAsync(TimeSpan.FromMinutes(1));
        }

        public async Task ValueConvert()
        {
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
        }

        public async Task WakeUp()
        {
            using var network = new ZWaveNetwork("COM3");
            await network.ConnectAsync();

            var multiSensor = network.GetDevices<AeotecMultiSensor6>().First();
            multiSensor.WakeUpNotification += (_, _) =>
            {
                // ... regular processing when device wakes up ...
            };

            async void MultiSensorSetupAsync(object? sender, EventArgs eventArgs)
            {
                // ... one-time setup of device when it wakes up ...
                multiSensor.WakeUpNotification -= MultiSensorSetupAsync;
            }

            multiSensor.WakeUpNotification += MultiSensorSetupAsync;
        }
    }
}