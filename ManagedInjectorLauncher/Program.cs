﻿// (c) Copyright Cory Plotts.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using ManagedInjector;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ManagedInjectorLauncher
{
    public static class Program
	{
		public static void Main(string[] args)
		{            
            Injector.LogMessage("Starting the injection process...", false);

			var windowHandle = (IntPtr)long.Parse(args[0]);
			var assemblyName = args[1];
			var className = args[2];
			var methodName = args[3];
		    var settingsFile = args[4];

		    var transportData = new TransportData
		                        {
                                    AssemblyName = assemblyName,
                                    ClassName = className,
                                    MethodName = methodName,
                                    SettingsFile = settingsFile
		                        };

			Injector.Launch(windowHandle, transportData);

            //check to see that it was injected, and if not, retry with the main window handle.
            var process = GetProcessFromWindowHandle(windowHandle);
            if (process != null && !CheckInjectedStatus(process) && process.MainWindowHandle != windowHandle)
            {
                Injector.LogMessage("Could not inject with current handle... retrying with MainWindowHandle", true);
                Injector.Launch(process.MainWindowHandle, transportData);
                CheckInjectedStatus(process);
            }
		}

        private static Process GetProcessFromWindowHandle(IntPtr windowHandle)
        {
            int processId;
            GetWindowThreadProcessId(windowHandle, out processId);
            if (processId == 0)
            {
                Injector.LogMessage(string.Format("could not get process for window handle {0}", windowHandle), true);
                return null;
            }

            var process = Process.GetProcessById(processId);
            if (process == null)
            {
                Injector.LogMessage(string.Format("could not get process for PID = {0}", processId), true);
                return null;
            }
            return process;
        }

        private static bool CheckInjectedStatus(Process process)
        {
            bool containsFile = false;
            process.Refresh();
            foreach (ProcessModule module in process.Modules)
            {
                if (module.FileName.Contains("ManagedInjector"))
                {
                    containsFile = true;
                }
            }
            if (containsFile)
            {
                Injector.LogMessage(string.Format("Successfully injected Snoop for process {0} (PID = {1})", process.ProcessName, process.Id), true);
            }
            else
            {
                Injector.LogMessage(string.Format("Failed to inject for process {0} (PID = {1})", process.ProcessName, process.Id), true);
            }

            return containsFile;
        }

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int processId);
	}
}
