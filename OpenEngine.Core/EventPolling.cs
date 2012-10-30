using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace OpenEngine.Core
{
    public class EventPolling
    {
        private int _frequency;
        private bool _stop = false;
        private string _state = "Idle";
        private StringBuilder _sb = new StringBuilder();
        private DateTime _nextRun = DateTime.Now;

        public EventPolling(int frequency) {
            _frequency = frequency;
        }

        public string GetState() {
            if (_state.Contains("{0}"))
                return string.Format(_state, _nextRun.ToLongTimeString());
            else
                return _state;
        }

        public string GetOutput() {
            return _sb.ToString();
        }

        public void ForceRun() {
            _nextRun = DateTime.Now;
        }

        public void Start() {
            new Thread(() => {
                while (!_stop) {
                    _sb = new StringBuilder();
                    var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "triggers");
                    if (Directory.Exists(path)) {
                        _state = "Running... (run stareted at {0})";
                        foreach (var script in Directory.GetFiles(path)) {
                            try
                            {
                                new Process().Query(script, "", false, Path.GetDirectoryName(script), (s) => _sb.AppendLine(DateTime.Now.ToLongTimeString() + " " + s));
                            }
                            catch (Exception ex)
                            {
                                Logger.Write(ex);
                            }
                        }
                    }
                    _nextRun  = DateTime.Now.AddMilliseconds(_frequency);
                    _state = "Run finished (next run starting at {0})";
                    while (DateTime.Now < _nextRun) {
                        Thread.Sleep(100);
                    }
                }
            }).Start();
        }

        public void Stop() {
            _stop = true;
        }
    }
}
