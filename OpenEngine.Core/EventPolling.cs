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
        private bool _force = false;
        private ScriptFailHandler _failHandler;

        public EventPolling(int frequency, ScriptFailHandler failHandler)
        {
            _frequency = frequency;
            _failHandler = failHandler;
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
            _force = true;
            _nextRun = DateTime.Now;
        }

        public void Start() {
            new Thread(() => {
                while (!_stop) {
                    _sb = new StringBuilder();
                    var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "triggers");
                    if (Directory.Exists(path)) {
                        var scripts = Directory.GetFiles(path);
                        var scriptSeparator = "";
                        _state = "Running... (run stareted at {0})";
                        foreach (var script in scripts) {
                            try
                            {
                                var hasFailed = false;
                                var scriptOutput = new StringBuilder();
                                _sb.AppendLine(scriptSeparator + "Running: " + Path.GetFileName(script));
                                var arguments = "";
                                var failHandle = _failHandler.GetState(script);
                                if (failHandle != null)
                                    arguments = "--fail-output-from-last-run \"" + failHandle + "\"";
                                if (_force)
                                    arguments += " --force";
                                new Process().Query(
                                    script,
                                    arguments,
                                    false,
                                    Path.GetDirectoryName(script),
                                    (s, error) => {
                                        _sb.AppendLine(DateTime.Now.ToLongTimeString() + " " + s);
                                        scriptOutput.AppendLine(DateTime.Now.ToLongTimeString() + " " + s);
                                        if (error)
                                            hasFailed = true;
                                    });
                                if (hasFailed)
                                    _failHandler.FailRun(script, scriptOutput.ToString());
                                else
                                    _failHandler.PassRun(script);
                            }
                            catch (Exception ex)
                            {
                                Logger.Write(ex);
                            }
                            scriptSeparator = Environment.NewLine;
                        }
                    }
                    _force = false;
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
