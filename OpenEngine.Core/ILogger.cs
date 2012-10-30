using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenEngine.Core
{
    public interface ILogger
    {
        void Write(Exception ex);
    }

    public static class Logger
    {
        private static ILogger _logger = new NullLogger();

        public static void SetLogger(ILogger logger) {
            _logger = logger;
        }

        public static void Write(Exception ex) {
            _logger.Write(ex);
        }
    }

    class NullLogger : ILogger
    {
        public void Write(Exception ex)
        {
        }
    }
}
