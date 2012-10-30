using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace OpenEngine.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            try
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
			    { 
				    new EngineService() 
			    };
                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception ex)
            {
                EventLogger.WriteError(string.Format("{0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace));
            }
        }
    }
}
