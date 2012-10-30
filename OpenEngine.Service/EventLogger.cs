using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OpenEngine.Service
{
    class EventLogger
    {
        private const string SOURCE = "OpenEngine";
        private const string LOG = "Application";

        private static bool _sourceExists = false;

        public static void WriteInformation(string information)
        {
            write(information, EventLogEntryType.Information);
        }

        public static void WriteError(string error)
        {
            write(error, EventLogEntryType.Error);
        }

        public static void WriteWarning(string warning)
        {
            write(warning, EventLogEntryType.Warning);
        }

        private static void write(string text, EventLogEntryType type)
        {
            if (_sourceExists)
                initializeSource();
            EventLog.WriteEntry(SOURCE, text, type);
        }

        private static void initializeSource()
        {
            if (!EventLog.SourceExists(SOURCE))
                EventLog.CreateEventSource(SOURCE, LOG);
            _sourceExists = true;
        }
    }
}
