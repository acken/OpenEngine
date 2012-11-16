using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using OpenEngine.Core;

namespace OpenEngine.Service
{
    public partial class EngineService : ServiceBase
    {
        private Bootstrapper _bootstrapper;

        public EngineService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _bootstrapper = new Bootstrapper(new EventLogger(), Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            _bootstrapper.Start();
        }

        protected override void OnStop()
        {
            _bootstrapper.Stop();
        }
    }
}
