// -------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace DeveloperTest
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Configuration;

    using Serilog;

    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;
    using ZWaveSerialApi.Devices;
    using ZWaveSerialApi.Devices.Brands.Aeotec;
    using ZWaveSerialApi.Utilities;

    internal class Program
    {
        private static async Task DeveloperTest()
        {
            var assemblyFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

            var configuration = new ConfigurationBuilder().SetBasePath(assemblyFolderPath)
                                                          .AddJsonFile("appsettings.json")
                                                          .AddJsonFile(
                                                              $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "production"}.json",
                                                              true)
                                                          .Build();

            var logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger().ForContext("ClassName", nameof(Program));

            var settingsFilePath = Path.Combine(assemblyFolderPath, "ZWaveDevices.settings");

            using var devices = new ZWaveDevices(logger, "COM3");

            if (File.Exists(settingsFilePath))
            {
                await devices.LoadAsync(settingsFilePath);
            }

            await devices.ConnectAsync();

            var multiSensor = devices.GetDevices<AeotecMultiSensor6>().First();
            var temperatureReport = await multiSensor.GetTemperatureAsync(TemperatureScale.Celcius);

            logger.Information($"Temperature: {temperatureReport.Value}{temperatureReport.Unit}");

            var aerq = devices.GetDevices<AeotecAerqSensor>().First();
            aerq.TemperatureReport += (_, eventArgs) => logger.Information("AërQ temperature = " + eventArgs.Report.Value + eventArgs.Report.Unit);

            Console.ReadKey();

            await devices.DisconnectAsync();
            await devices.SaveAsync("ZWaveDevices.settings");
        }

        private static async Task GitHubConvertExample()
        {
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
        }

        private static async Task GitHubExample()
        {
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
        }

        private static async Task GitHubLocationExample()
        {
            using var devices = new ZWaveDevices("COM3");
            await devices.ConnectAsync();

            // Set location
            var unknownMultiSensor = devices.GetDevices<AeotecMultiSensor6>().First();
            unknownMultiSensor.Location = "Kitchen";

            // Get by location
            var kitchenMultiSensor = devices.GetDevice<AeotecMultiSensor6>("Kitchen");
        }

        private static async Task GitHubSettingsExample()
        {
            using var devices = new ZWaveDevices("COM3");
            await devices.LoadAsync("ZWaveDevices.settings");
            await devices.ConnectAsync();

            // ... program logic ...

            await devices.SaveAsync("ZWaveDevices.settings");
        }

        private static async Task Main(string[] args)
        {
            await DeveloperTest();
        }
    }
}