﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OpenEngine.Core
{
    public static class ProcessExtensions
    {
        public static void Run(
            this Process proc,
            string command,
            string arguments,
            bool visible,
            string workingDir)
        {
            prepareProcess(proc, command, arguments, visible, workingDir);
            proc.Start();
			proc.WaitForExit();
        }

        public static void Query(
            this Process proc,
            string command,
            string arguments,
            bool visible,
            string workingDir,
			Action<string> onRecievedLine)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix &&
                Environment.OSVersion.Platform != PlatformID.MacOSX)
            {
                arguments = "/c " +
                    "^\"" + batchEscape(command) + "^\" " +
                    batchEscape(arguments);
                command = "cmd.exe";
            }
			
			var exit = false;
            prepareProcess(proc, command, arguments, visible, workingDir);
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
			proc.OutputDataReceived += (s, data) => {
					if (data.Data == null)
						exit = true;
					else
						onRecievedLine(data.Data);
				};
            proc.Exited += (sender, e) => exit = true;
            if (proc.Start())
            {
                proc.BeginOutputReadLine();
                while (!exit && !proc.HasExited)
					System.Threading.Thread.Sleep(10);
            }
        }

        private static string batchEscape(string text) {
            foreach (var str in new[] { "^"," ", "&", "(", ")", "[", "]", "{", "}", "=", ";", "!", "'", "+", ",", "`", "~", "\"" })
                text = text.Replace(str, "^" + str);
            return text;
        }

        private static void prepareProcess(
            Process proc,
            string command,
            string arguments,
            bool visible,
            string workingDir)
        {
            var info = new ProcessStartInfo(command, arguments);
            info.CreateNoWindow = !visible;
            if (!visible)
                info.WindowStyle = ProcessWindowStyle.Hidden;
            info.WorkingDirectory = workingDir;
            proc.StartInfo = info;
        }
    }
}
