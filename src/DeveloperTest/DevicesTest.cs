// -------------------------------------------------------------------------------------------------
// <copyright file="DevicesTest.cs" company="Martin Karlsson">
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

    using ZWaveSerialApi;
    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;
    using ZWaveSerialApi.Devices;
    using ZWaveSerialApi.Devices.Brands.Aeotec;

    internal class DevicesTest
    {
        public async Task Test()
        {
            var assemblyFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

            var configuration = new ConfigurationBuilder().SetBasePath(assemblyFolderPath)
                                                          .AddJsonFile("appsettings.json")
                                                          .AddJsonFile(
                                                              $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "production"}.json",
                                                              true)
                                                          .Build();

            var logger = new LoggerConfiguration().ReadFrom.Configuration(configuration)
                                                  .CreateLogger()
                                                  .ForContext<DevicesTest>()
                                                  .ForContext(Constants.ClassName, GetType().Name);

            var networkFilePath = Path.Combine(assemblyFolderPath, "ZWaveNetwork.json");

            using var network = new ZWaveNetwork(logger, "COM3");

            if (File.Exists(networkFilePath))
            {
                await network.LoadAsync(networkFilePath);
            }

            await network.ConnectAsync();

            var multiSensor = network.GetDevices<AeotecMultiSensor6>().First();

            var temperatureReport = await multiSensor.GetTemperatureAsync(TemperatureScale.Celsius);
            logger.Information($"Temperature: {temperatureReport.Value}{temperatureReport.Unit}");

            multiSensor.MotionDetected += (_, _) =>
            {
                logger.Information("Motion detection started");
            };

            multiSensor.MotionIdle += (_, _) =>
            {
                logger.Information("Motion detection idle");
            };

            //var aerq = network.GetDevices<AeotecAerqSensor>().First();
            //aerq.TemperatureReport += (_, eventArgs) => logger.Information("AërQ temperature = " + eventArgs.Report.Value + eventArgs.Report.Unit);

            Console.ReadKey();

            await network.DisconnectAsync();
            await network.SaveAsync("ZWaveNetwork.json");
        }
    }
}