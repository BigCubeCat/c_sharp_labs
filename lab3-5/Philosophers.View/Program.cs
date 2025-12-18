﻿using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Philosophers.DB;
using Philosophers.Services.DB.Service;
using Interface.DTO;
using Interface;

try
{
    ParseArgs(out string pathToConf, out bool helpOnly);

    if (helpOnly)
    {
        return;
    }

    using JsonDocument document = JsonDocument.Parse(File.ReadAllText(pathToConf));
    var root = document.RootElement;

    var connectionString = root.GetProperty("ConnectionStrings").GetProperty("PostgresConnection").GetString();
    if (connectionString == null)
    {
        Console.WriteLine("Connection string is null");
        return;
    }

    var dbService = SimulationDatabaseProcessor.Create(connectionString);
    
    // Основной цикл работы с пользователем
    await RunUserInterface(dbService);
}
catch (KeyNotFoundException e)
{
    Console.WriteLine($"Can not found key exception: {e.Message} from {e.Source}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}

static void ParseArgs(out string pathToConf, out bool helpOnly)
{
    pathToConf = "./philosophers.json";
    helpOnly = false;

    bool wasConfigPath = false;

    bool confPathFlag = false;

    var args = Environment.GetCommandLineArgs();

    foreach (var arg in args)
    {
        if (confPathFlag)
        {
            if (wasConfigPath)
                throw new ArgumentException("Double set path");

            pathToConf = arg;
            wasConfigPath = true;
            confPathFlag = false;
        }

        if (arg == "-c" || arg == "--config_path")
            confPathFlag = true;
        else if (arg == "-h" || arg == "--help")
        {
            Console.Write(
                """
                This is lab3-5 of NSU C# course.

                *DESCRIPTION*
                In this lab, I solved the Dining Philosophers problem using .Net Generic Host and EntityFramework.

                *ARGUMENTS*
                -c or --config_path - relative or full path to config file. Current directory used by default.
                -h or --help - see this page
                """
            );

            helpOnly = true;
        }
    }
}

static async Task RunUserInterface(SimulationDatabaseProcessor dbService)
{
    while (true)
    {
        Console.WriteLine("\n=== PHILOSOPHERS DATABASE MANAGER ===");
        Console.WriteLine("1. Get all running info");
        Console.WriteLine("2. Get running info by ID");
        Console.WriteLine("3. Get running info by step");
        Console.WriteLine("4. Get running info by simulation state");
        Console.WriteLine("5. Exit");
        Console.Write("Select option (1-5): ");

        string choice = Console.ReadLine()?.Trim();

        switch (choice)
        {
            case "1":
                await GetAllRunningInfo(dbService);
                break;
            case "2":
                await GetRunningInfoById(dbService);
                break;
            case "3":
                await GetRunningInfoByStep(dbService);
                break;
            case "4":
                await GetRunningInfoByState(dbService);
                break;
            case "5":
                Console.WriteLine("Goodbye!");
                return;
            default:
                Console.WriteLine("Invalid option. Please try again.");
                break;
        }
    }
}

static async Task GetAllRunningInfo(SimulationDatabaseProcessor dbService)
{
    try
    {
        Console.WriteLine("\n--- Getting all running info ---");
        var runningInfos = await dbService.GetAllRunningInfosAsync(CancellationToken.None);
        
        if (runningInfos.Count == 0)
        {
            Console.WriteLine("No running info found.");
            return;
        }

        Console.WriteLine($"Found {runningInfos.Count} records:");
        foreach (var info in runningInfos)
        {
            PrintRunningInfo(info);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting all running info: {ex.Message}");
    }
}

static async Task GetRunningInfoById(SimulationDatabaseProcessor dbService)
{
    try
    {
        Console.WriteLine("\n--- Getting running info by ID ---");
        Console.Write("Enter run ID: ");
        
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            var runningInfo = await dbService.GetRunningInfoByIdAsync(id, CancellationToken.None);
            PrintRunningInfo(runningInfo);
        }
        else
        {
            Console.WriteLine("Invalid ID format.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting running info by ID: {ex.Message}");
    }
}

static async Task GetRunningInfoByStep(SimulationDatabaseProcessor dbService)
{
    try
    {
        Console.WriteLine("\n--- Getting running info by step ---");
        Console.Write("Enter step number: ");
        
        if (int.TryParse(Console.ReadLine(), out int step))
        {
            var runningInfos = await dbService.GetRunningInfoByStepAsync(step, CancellationToken.None);
            
            if (runningInfos.Count == 0)
            {
                Console.WriteLine($"No running info found for step {step}.");
                return;
            }

            Console.WriteLine($"Found {runningInfos.Count} records for step {step}:");
            foreach (var info in runningInfos)
            {
                PrintRunningInfo(info);
            }
        }
        else
        {
            Console.WriteLine("Invalid step format.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting running info by step: {ex.Message}");
    }
}

static async Task GetRunningInfoByState(SimulationDatabaseProcessor dbService)
{
    try
    {
        Console.WriteLine("\n--- Getting running info by simulation state ---");
        Console.WriteLine("Available states:");
        Console.WriteLine("0 - Running");
        Console.WriteLine("1 - FinishSuccess");
        Console.WriteLine("2 - FinishError");
        Console.WriteLine("3 - Deadlock");
        Console.Write("Select state (0-4): ");
        
        if (int.TryParse(Console.ReadLine(), out int stateValue) && 
            Enum.IsDefined(typeof(SimulationStates), stateValue))
        {
            var state = (SimulationStates)stateValue;
            var runningInfos = await dbService.GetRunningInfoBySimulationStateAsync(state, CancellationToken.None);
            
            if (runningInfos.Count == 0)
            {
                Console.WriteLine($"No running info found for state {state}.");
                return;
            }

            Console.WriteLine($"Found {runningInfos.Count} records for state {state}:");
            foreach (var info in runningInfos)
            {
                PrintRunningInfo(info);
            }
        }
        else
        {
            Console.WriteLine("Invalid state value.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting running info by state: {ex.Message}");
    }
}

static void PrintRunningInfo(RunningInfoDto info)
{
    Console.WriteLine($"\n--- Run ID: {info.Id} ---");
    Console.WriteLine($"Step: {info.Step}");
    Console.WriteLine($"Duration: {info.Duration}");
    Console.WriteLine($"Simulation State: {info.SimulationState}");
    Console.WriteLine($"Philosophers Count: {info.Philosophers.Count}");
    
    for (int i = 0; i < info.Philosophers.Count; i++)
    {
        var philosopher = info.Philosophers[i];
        Console.WriteLine($"  Philosopher {i + 1}:");
        Console.WriteLine($"    State: {philosopher.PhilosopherState}");
        Console.WriteLine($"    Left Fork: {philosopher.LeftFork.ForkState}");
        Console.WriteLine($"    Right Fork: {philosopher.RightFork.ForkState}");
    }
    Console.WriteLine("---");
}