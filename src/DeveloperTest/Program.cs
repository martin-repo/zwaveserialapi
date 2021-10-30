// -------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace DeveloperTest
{
    using System;
    using System.Threading.Tasks;

    internal class Program
    {
        private static async Task Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                if (eventArgs.ExceptionObject is Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }
            };

            await new DevicesTest().Test();
        }
    }
}