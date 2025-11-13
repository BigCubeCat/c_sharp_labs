global using Interface.Strategy;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Interface;
using Interface.Channel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Src;
using Src.Channels;
using Src.Channels.Items;
using Src.Philosophers;
using Src.Strategy;

namespace Src
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                ParseArgs(args, out string pathToConf, out bool helpOnly);

                if (helpOnly)
                    return;

                using IHost host = Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((hostContext, configuration) =>
                    {
                        configuration.AddJsonFile(pathToConf, optional: false, reloadOnChange: false);
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddSingleton<IChannel<PhilosopherToAnalyzerChannelItem>, PhilosopherToAnalyzerChannel>();
                        services.AddSingleton<IChannel<PhilosopherToPrinterChannelItem>, PhilosopherToPrinterChannel>();
                        services.AddSingleton<IStrategy, LeftRightStrategy>();
                        services.AddSingleton<ILogger<PhilosopherService>, Logger<PhilosopherService>>();
                        services.AddSingleton<ILogger<DeadlockAnalyzer>, Logger<DeadlockAnalyzer>>();
                        services.AddSingleton<ILogger<SimulationManager>, Logger<SimulationManager>>();
                        services.AddSingleton<CompletionCoordinator>();

                        services.AddTransient<IForksFactory<Fork>, ForksFactory<Fork>>();

                        services.AddHostedService<Aristotel>();
                        services.AddHostedService<Engels>();
                        services.AddHostedService<Kant>();
                        services.AddHostedService<Marks>();
                        services.AddHostedService<Platon>();

                        services.AddHostedService<DeadlockAnalyzer>();
                        services.AddHostedService<SimulationManager>();

                        var root = hostContext.Configuration;
                        services.Configure<PhilosopherConfiguration>(root.GetSection(nameof(PhilosopherConfiguration)));
                    })
                    .Build();

                await host.RunAsync();
            }
            catch (ApplicationException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Simulation was cancelled.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private static void ParseArgs(string[] args, out string pathToConf, out bool helpOnly)
        {
            pathToConf = "./philosophers.json";
            helpOnly = false;

            bool wasConfigPath = false;
            bool confPathFlag = false;

            foreach (var arg in args)
            {
                if (confPathFlag)
                {
                    if (wasConfigPath)
                        throw new ArgumentException("Double set path");

                    pathToConf = arg;
                    wasConfigPath = true;
                    confPathFlag = false;
                    continue;
                }

                switch (arg)
                {
                    case "-c":
                    case "--config_path":
                        confPathFlag = true;
                        break;

                    case "-h":
                    case "--help":
                        PrintHelp();
                        helpOnly = true;
                        break;
                }
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine(
                """
                This is lab1 of NSU C# course.

                *DESCRIPTION*
                In this lab, I solved the Dining Philosophers problem using multiple threads.

                *ARGUMENTS*
                -c or --config_path   - Relative or full path to config file. Current directory used by default.
                -h or --help          - Show this page.
                -t or --update_time   - Time between updates of the simulation state output (100-200 ms).
                -s or --simulation_time - Total simulation time in milliseconds.
                """
            );
        }
    }
}
