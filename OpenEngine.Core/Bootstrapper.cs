using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace OpenEngine.Core
{
    public class Bootstrapper
    {
        private EventPolling _poller;
        private HttpReporter _reporter;

        public void Start() {
            _poller = new EventPolling(getDelay());
            _poller.Start();
            _reporter = new HttpReporter(_poller, getPort());
            _reporter.Start();
        }

        public void Stop() {
            _poller.Stop();
            _reporter.Stop();
        }

        private int getDelay() {
            try {
                return int.Parse(getExtension("delay"));
            } catch {
                // Defaults to once every hour
                return 3600000;
            }
        }

        private int getPort() {
            try {
                return int.Parse(getExtension("port"));
            } catch {
                return 8888;
            }
        }

        private string getExtension(string prefix) {
            var files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), prefix + ".*");
            if (files.Length == 0)
                return null;
            return Path.GetExtension(files[0]).Replace(".", "");
        }
    }
}
