﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace OpenEngine.Core
{
    public class HttpReporter
    {
        private EventPolling _state;
        private HttpServer _server;
        private int _port;
        private int _refreshPeriod;
        private ScriptFailHandler _failHandler;


        public HttpReporter(EventPolling state, int port, int refreshPeriod, ScriptFailHandler failHandler) {
            _state = state;
            _failHandler = failHandler;
            _port = port;
            _refreshPeriod = refreshPeriod;
            _server = new HttpServer(Environment.ProcessorCount);
            _server.ProcessRequest += handleRequest;
        }

        public void Start() {
            _server.Start(_port);
        }

        public void Stop() {
            _server.Stop();
        }

        private void handleRequest(HttpListenerContext ctx) {
            if (ctx.Request.Url.AbsolutePath == "/favicon.ico")
                return;
            if (ctx.Request.Url.AbsolutePath == "/force-run") {
                _state.ForceRun();
                System.Threading.Thread.Sleep(500);
                ctx.Response.Redirect("/");
                ctx.Response.Close();
                return;
            }
            var response = ctx.Response;
            byte[] buffer = Encoding.UTF8.GetBytes(hyperText());
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            output.Write(buffer,0,buffer.Length);
            output.Close();
        }

        private string hyperText()
        {
            var info = getStateScriptOutput();
            var triggerState = getTriggerScriptState();
            var additionalInfo = "";
            if (info.Length > 0)
                additionalInfo = info.ToString().Replace(Environment.NewLine, "<br>").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;") + "<br>";

            string refreshScriptOnUpdate = String.Empty;
            string cssLeft = cssLeftInactive();
            if (_state.IsRunning)
            {
                refreshScriptOnUpdate = refreshScript();
                cssLeft = cssLeftActive();
            }



            return "<HTML>"+
            "<head><style>"+
            "body html { height: 100%; }"+
            "div#right{ height:100% }"+
            cssLeft +
            ".right { "+
            " width: 50%;"+
            " height: 500px;"+
            " border: 1px solid green; "+
            " color: white; "+
            " background-color: black;"+
            " -webkit-border-radius:5px; " +
            " -moz-border-radius:5px;" +
            " -border-radius:5px;" +
            " float: left;" +
            " padding: 20px;"+
            " margin: 30px; 40px; 10px; 10px;"+
            "}\n"+
            ".inScript { "+
            " margin-left: 40px; "+
            "}"+
            "</style></head>" +
            "<BODY>" + 
            refreshScriptOnUpdate +
                "<div class='left'>" +
                    "<div class='trigger_run_now'>"+
                        "<a href=\"/force-run\">Trigger run now</a>"+
                    "</div>" + 
                    "<div class='state'>"+
                        _state.GetState().Replace(Environment.NewLine, "<br/>") +
                    "</div>"+
                    "<div class='scripts'>"+
                        "<h1>Scripts</h1>"+
                    "</div>"+
                    "<div class='triggerState'>"+
                        triggerState +
                    "</div>"+
                "</div>"+
                "<div class='right'>"+
                    "<div class='summaryHeader'>"+
                        "<h1>Summary</h1>"+
                    "</div>" + 
                    "<div class='additionalInfo'>" +
                        additionalInfo +
                    "</div>"+
                    "<div class='outputHeader'>"+
                        "<strong>Output</strong><br/>"+
                    "</div>" +
                    "<div class='output'>" + 
                        _state.GetOutput().Replace(Environment.NewLine, "<br/>") +
                    "</div>" +
                    "</div>" +
                "</div>" +
            "</BODY></HTML>";
        }

        private string cssLeftInactive()
        {
            return ".left { " +
            " width: 30%; " +
            " height: 100%" +
            " border: 1px solid darkgrey;" +
            " float: left; " +
            " background-color: gainsboro;" +
            " -webkit-border-radius:5px; " +
            " -moz-border-radius:5px;" +
            " -border-radius:5px;" +
            " padding: 20px;" +
            " margin: 30px;" +
            "}\n";
        }

        private string cssLeftActive()
        {
            return ".left { " +
            " width: 30%; " +
            " height: 100%" +
            " border: 1px solid darkgrey;" +
            " float: left; " +
            " background-color: goldenrod;" +
            " -webkit-border-radius:5px; " +
            " -moz-border-radius:5px;" +
            " -border-radius:5px;" +
            " padding: 20px;" +
            " margin: 30px;" +
            "}\n";

        }

        private string refreshScript()
        {
            return "<script type='text/javascript'>"+
                "setTimeout(function(){ window.location.reload(1);}, "+_refreshPeriod.ToString()+");"+
            "</script>";
        }

        private string getTriggerScriptState()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "triggers");
            var info = new StringBuilder();
            if (Directory.Exists(path))
            {
                try
                {
                    foreach (var script in Directory.GetFiles(path)) {
                        if (_failHandler.GetState(script) == null)
                            info.Append("<font color=\"Green\">" + Path.GetFileName(script) + "</font><br>");
                        else
                            info.Append("<font color=\"Red\">" + Path.GetFileName(script) + "</font><br>");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }
            return info.ToString();
        }

        private StringBuilder getStateScriptOutput()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "states");
            var info = new StringBuilder();
            if (Directory.Exists(path))
            {
                try
                {
                    foreach (var script in Directory.GetFiles(path))
                    {
                        try
                        {
                            new Process().Query(
                                script,
                                "",
                                false,
                                Path.GetDirectoryName(script),
                                (s, error) => {
                                    if (error)
                                        info.Append("<font color=\"Red\">" + s + "</font><br>");
                                    else
                                        info.AppendLine(s);
                                });
                        }
                        catch (Exception ex)
                        {
                            Logger.Write(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }
            return info;
        }
    }

    public class HttpServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private readonly Thread[] _workers;
        private readonly ManualResetEvent _stop, _ready;
        private Queue<HttpListenerContext> _queue;

        public HttpServer(int maxThreads)
        {
            _workers = new Thread[maxThreads];
            _queue = new Queue<HttpListenerContext>();
            _stop = new ManualResetEvent(false);
            _ready = new ManualResetEvent(false);
            _listener = new HttpListener();
            _listenerThread = new Thread(HandleRequests);
        }

        public void Start(int port)
        {
            _listener.Prefixes.Add(String.Format(@"http://+:{0}/", port));
            _listener.Start();
            _listenerThread.Start();

            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new Thread(Worker);
                _workers[i].Start();
            }
        }

        public void Dispose()
        { Stop(); }

        public void Stop()
        {
            _stop.Set();
            _listenerThread.Join();
            foreach (Thread worker in _workers)
                worker.Join();
            _listener.Stop();
        }

        private void HandleRequests()
        {
            while (_listener.IsListening)
            {
                var context = _listener.BeginGetContext(ContextReady, null);

                if (0 == WaitHandle.WaitAny(new[] { _stop, context.AsyncWaitHandle }))
                    return;
            }
        }

        private void ContextReady(IAsyncResult ar)
        {
            try
            {
                lock (_queue)
                {
                    _queue.Enqueue(_listener.EndGetContext(ar));
                    _ready.Set();
                }
            }
            catch { return; }
        }

        private void Worker()
        {
            WaitHandle[] wait = new[] { _ready, _stop };
            while (0 == WaitHandle.WaitAny(wait))
            {
                HttpListenerContext context;
                lock (_queue)
                {
                    if (_queue.Count > 0)
                        context = _queue.Dequeue();
                    else
                    {
                        _ready.Reset();
                        continue;
                    }
                }

                try { ProcessRequest(context); }
                catch (Exception e) { 
                    Logger.Write(e);
                }
            }
        }

        public event Action<HttpListenerContext> ProcessRequest;
    }
}
