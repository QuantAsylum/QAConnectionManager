using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;

namespace Com.QuantAsylum
{
    public static class QAConnectionManager
    {
        public enum Devices { QA100, QA300, QA400, QA505 };

        static List<string> InstallDirSearchPaths = new List<string>();

        const string QA100ExePath = @"QA100\QAScope.exe";
        const string QA300ExePath = @"QA300\QAPowerSupply.exe";
        const string QA400ExePath = @"QA400\QAAnalyzer.exe";
        const string QA505ExePath = @"QA505\QARFPowerMeter.exe";

        static QAConnectionManager()
        {
            // Note the trailing slash!
            InstallDirSearchPaths.Add(@"c:\program files\quantasylum\");
            InstallDirSearchPaths.Add(@"c:\program files (x86)\quantasylum\");
            InstallDirSearchPaths.Add(@"d:\program files\quantasylum\");
            InstallDirSearchPaths.Add(@"d:\program files (x86)\quantasylum\");
            InstallDirSearchPaths.Add(@"e:\program files\quantasylum\");
            InstallDirSearchPaths.Add(@"e:\program files (x86)\quantasylum\");
        }

        /// <summary>
        /// Adds a search path for the QuantAsylum root install directory
        /// </summary>
        /// <param name="path"></param>
        public static void AddSearchPath(string path)
        {
            // Get rid of any forward slashes and replace with backslash
            path = path.Replace('/', '\\');

            if (path[path.Length - 1] != '\\')
            {
                // Trailing backslash not present, so add it
                path = path + '\\';
            }

            InstallDirSearchPaths.Add(path);
        }

        /// <summary>
        /// Attempts to load assembly from the given name by iterating through all search paths. The 'name'
        /// should specify subdirectory and exe name, for example "QA100\QAScope.exe" (Note! No leading backslash!)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static Assembly TryToLoadAssembly(string name)
        {
            Assembly assembly;

            foreach (string dir in InstallDirSearchPaths)
            {
                if (Directory.Exists(dir))
                {
                    try
                    {
                        assembly = Assembly.LoadFrom(dir + name);

                        return assembly;
                    }
                    catch
                    {

                    }
                }
            }

            return null;
        }



        public static Object ConnectTo(Devices device)
        {
            Assembly assembly;
            Type type;

            try
            {
                switch (device)
                {
                    case Devices.QA100:
                        assembly = TryToLoadAssembly(QA100ExePath);
                        type = assembly.GetType("Com.QuantAsylum.QA100Interface");
                        return Activator.GetObject(type, "tcp://localhost:9100/QuantAsylumQA100Server");

                    case Devices.QA300:
                        assembly = TryToLoadAssembly(QA300ExePath);
                        type = assembly.GetType("Com.QuantAsylum.QA300Interface");
                        return Activator.GetObject(type, "tcp://localhost:9300/QuantAsylumQA300Server");

                    case Devices.QA400:
                        assembly = TryToLoadAssembly(QA400ExePath);
                        type = assembly.GetType("Com.QuantAsylum.QA400Interface");
                        return Activator.GetObject(type, "tcp://localhost:9400/QuantAsylumQA400Server");

                    case Devices.QA505:
                        assembly = TryToLoadAssembly(QA505ExePath);
                        type = assembly.GetType("Com.QuantAsylum.QA505Interface");
                        return Activator.GetObject(type, "tcp://localhost:9505/QuantAsylumQA505Server");

                    default:
                        throw new NotImplementedException("QAConnectionManager ConnectTo");
                }
            }
            catch
            {

            }

            return null;

        }

        public static bool IsAppRunning(Devices device)
        {
            return IsAppRunning(device, false);
        }

        public static bool IsAppRunning(Devices device, bool remindUser)
        {
            string procName = ""; ;

            switch (device)
            {
                case Devices.QA100:
                    procName = "QAScope";
                    break;
                case Devices.QA300:
                    procName = "QAPowerSupply";
                    break;
                case Devices.QA400:
                    procName = "QAAnalyzer";
                    break;
                case Devices.QA505:
                    procName = "QARFPowerMeter";
                    break;
                default:
                    throw new NotImplementedException("QAConnectionManager IsAppRunning");
            }

            if (Process.GetProcessesByName(procName).Length == 0)
            {
                // In here, the app is not running. Check if the user wants to us to anything else
                if (remindUser)
                    MessageBox.Show("The " + device.ToString() + " application is not running. Please start that application before attempting to control it remotely.");

                return false;
            }

            return true;
        }

        static bool TryToRunExe(string name)
        {
            foreach (string dir in InstallDirSearchPaths)
            {

                string pathToExe = dir + name;
                if (File.Exists(pathToExe))
                {
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.WorkingDirectory = Path.GetDirectoryName(pathToExe);
                        psi.FileName = pathToExe;
                        System.Diagnostics.Process.Start(psi);
                        Thread.Sleep(1000);
                        return true;
                    }
                    catch
                    {

                    }
                }
            }

            return false;
        }

        public static bool LaunchAppIfNotRunning(Devices device)
        {
            if (IsAppRunning(device))
                return true;

            switch (device)
            {
                case Devices.QA100:
                    return TryToRunExe(QA100ExePath);
                case Devices.QA300:
                    return TryToRunExe(QA300ExePath);
                case Devices.QA400:
                    return TryToRunExe(QA400ExePath);
                case Devices.QA505:
                    return TryToRunExe(QA505ExePath);
                default:
                    throw new NotImplementedException("QAConnectionManager LaunchAppIfNotRunning");
            }


        }
    }
}
