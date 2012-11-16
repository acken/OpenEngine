using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenEngine.Core
{
    public class ScriptFailHandler
    {
        private string _path;

        public ScriptFailHandler(string path) {
            _path = path;
        }

        public string GetState(string scriptPath) {
            var failedHandle = getFailItem(scriptPath);
            if (File.Exists(failedHandle))
                return failedHandle;
            return null;
        }

        public void PassRun(string scriptPath) {
            var failedHandle = getFailItem(scriptPath);
            if (File.Exists(failedHandle))
                File.Delete(failedHandle);
        }

        public void FailRun(string scriptPath, string reason)
        {
            var failedHandle = getFailItem(scriptPath);
            if (File.Exists(failedHandle))
                File.Delete(failedHandle);
            File.WriteAllText(failedHandle, reason);
        }

        private string getFailItem(string scriptPath)
        {
            return Path.Combine(_path, Path.GetFileName(scriptPath) + ".failed_on_last_run");
        }
    }
}
