using System;
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
			Action<string,bool> onRecievedLine)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix &&
                Environment.OSVersion.Platform != PlatformID.MacOSX)
            {
                arguments = "/c " +
                    "^\"" + batchEscape(command) + "^\" " +
                    batchEscape(arguments);
                command = "cmd.exe";
            }
			
			var endOfOutput = false;
            var endOfError = false;
            prepareProcess(proc, command, arguments, visible, workingDir);
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.ErrorDataReceived += (s, data) => {
                if (data.Data == null)
                    endOfError = true;
                else
                    onRecievedLine("Error: " + data.Data, true);
            };
			proc.OutputDataReceived += (s, data) => {
					if (data.Data == null)
                        endOfOutput = true;
					else
                        onRecievedLine(data.Data, false);
				};
            
            if (proc.Start())
            {
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                while (!endOfError || !endOfOutput)
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
