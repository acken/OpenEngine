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
        private string _storage;

        public Bootstrapper(string storage) {
            initStorage(storage);
        }

        public Bootstrapper(ILogger logger, string storage) {
            Logger.SetLogger(logger);
            initStorage(storage);
        }

        private void initStorage(string storage)
        {
            _storage = storage;
            if (!Directory.Exists(_storage))
                throw new Exception("You need to specify an existing storage base location");
            _storage = Path.Combine(_storage, "OpenEngine");
            if (!Directory.Exists(_storage))
                Directory.CreateDirectory(_storage);
        }

        public void Start() {
            var failHandler = new ScriptFailHandler(_storage);
            _poller = new EventPolling(getDelay(), failHandler);
            _poller.Start();
            _reporter = new HttpReporter(_poller, getPort(), failHandler);
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
