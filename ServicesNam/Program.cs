using System;
using System.Collections.Generic;
using System.Management;
using System.ServiceProcess;

namespace ServicesNam
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"

                                 _____                 _               ______         
                                /  ___|               (_)              | ___ \        
                                \ `--.  ___ _ ____   ___  ___ ___  ___ | |_/ / __ ___ 
                                 `--. \/ _ \ '__\ \ / / |/ __/ _ \/ __||  __/ '__/ __|
                                /\__/ /  __/ |   \ V /| | (_|  __/\__ \| |  | | | (__ 
                                \____/ \___|_|    \_/ |_|\___\___||___/\_|  |_|  \___|
                                                      
                                                      

  +-------------------------------------------------------------------------------------------------------------------+
            ");
            Console.ResetColor();
            Console.WriteLine($"    {Environment.UserName} Services in {Environment.OSVersion}\n");
            List<ServiceController> stoppedServices = new List<ServiceController>();
            List<ServiceController> runningServices = new List<ServiceController>();
            List<ServiceController> pendingServices = new List<ServiceController>();
            ServiceController[] services = ServiceController.GetServices();

            foreach (ServiceController service in services)
            {
                try
                {
                    switch (service.Status)
                    {
                        case ServiceControllerStatus.Running:
                            runningServices.Add(service);
                            break;

                        case ServiceControllerStatus.Stopped:
                            stoppedServices.Add(service);
                            break;

                        case ServiceControllerStatus.StartPending:
                        case ServiceControllerStatus.StopPending:
                        case ServiceControllerStatus.ContinuePending:
                        case ServiceControllerStatus.PausePending:
                        case ServiceControllerStatus.Paused:
                            pendingServices.Add(service);
                            break;

                        default:
                            pendingServices.Add(service);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"{service.ServiceName} - [Error: {ex.Message}]");
                }
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n    Stopped Services:\n");
            foreach (ServiceController service in stoppedServices)
            {
                Console.WriteLine($"    {service.ServiceName} - [Stopped]");
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n    Running Services:\n");
            foreach (ServiceController service in runningServices)
            {
                Console.WriteLine($"    {service.ServiceName} - [Running]");
                DateTime startTime = GetServiceStartTime(service.ServiceName);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"    {service.ServiceName} - [Last Started: {startTime.ToShortDateString()} {startTime.ToShortTimeString()}]");
                Console.ForegroundColor = ConsoleColor.Green;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (ServiceController service in pendingServices)
            {
                Console.WriteLine($"    {service.ServiceName} - [{service.Status}]");
            }

            Console.ResetColor();
            Console.WriteLine("\n    Press any key to exit...");
            Console.ReadKey();
        }

        static DateTime GetServiceStartTime(string serviceName)
        {
            string query = $"SELECT ProcessId FROM Win32_Service WHERE Name = '{serviceName}'";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    uint processId = (uint)obj["ProcessId"];
                    string processQuery = $"SELECT CreationDate FROM Win32_Process WHERE ProcessId = {processId}";
                    using (ManagementObjectSearcher processSearcher = new ManagementObjectSearcher(processQuery))
                    {
                        foreach (ManagementObject processObj in processSearcher.Get())
                        {
                            string creationDateStr = processObj["CreationDate"].ToString();
                            return ManagementDateTimeConverter.ToDateTime(creationDateStr);
                        }
                    }
                }
            }
            return DateTime.MinValue;
        }
    }
}