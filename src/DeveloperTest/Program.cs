// -------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace DeveloperTest
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    using Microsoft.Extensions.Configuration;

    using Serilog;
    using System.Linq;

    using ZWaveSerialApi;
    using ZWaveSerialApi.Devices;
    using ZWaveSerialApi.Devices.Brands.Aeotec;
    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;
    using System.Threading.Tasks;

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await DeviceExample();
        }

        private static async Task DeveloperTest()
        {
            var assemblyFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var configuration = new ConfigurationBuilder().SetBasePath(assemblyFolderPath)
                                                          .AddJsonFile("appsettings.json")
                                                          .AddJsonFile(
                                                              $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "production"}.json",
                                                              true)
                                                          .Build();

            var logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger().ForContext("ClassName", nameof(Program));

            using var devices = new ZWaveDevices("COM3", logger);
            var multiSensor = devices.GetDevices<AeotecMultiSensor6>().First();
            var temperatureReport = await multiSensor.GetTemperatureAsync(TemperatureScale.Celcius, CancellationToken.None);

            logger.Information($"Temperature: {temperatureReport.Value}{temperatureReport.Unit}");
        }

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
    }
}