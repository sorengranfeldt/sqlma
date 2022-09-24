using Microsoft.MetadirectoryServices;
using System;
using System.Diagnostics;

namespace Granfeldt
{
    public partial class SQLManagementAgent : IDisposable, IMAExtensible2GetCapabilities, IMAExtensible2GetSchema, IMAExtensible2GetParameters, IMAExtensible2CallImport, IMAExtensible2CallExport
    {
        // New-EventLog -Source "SQL Management Agent" -LogName Application
        const string EventLogSource = "SQL Management Agent";
        const string EventLogName = "Application";

        SqlMethods methods = new SqlMethods();

        public Schema Schema { get; set; } = null;

        public SQLManagementAgent()
        {
            Tracer.Enter(nameof(SQLManagementAgent));
            try
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                string version = fvi.FileVersion;
                Tracer.TraceInformation($"sqlma-version {version}");
                Tracer.TraceInformation("reading-registry-settings");

                Tracer.TraceInformation($"adding-eventlog-listener-for name: {EventLogName}, source: {EventLogSource}");
                EventLog evl = new EventLog(EventLogName);
                evl.Log = EventLogName;
                evl.Source = EventLogSource;

                EventLogTraceListener eventLog = new EventLogTraceListener(EventLogSource);
                eventLog.EventLog = evl;
                EventTypeFilter filter = new EventTypeFilter(SourceLevels.Warning | SourceLevels.Error | SourceLevels.Critical);
                eventLog.TraceOutputOptions = TraceOptions.Callstack;
                eventLog.Filter = filter;
                Tracer.Trace.Listeners.Add(eventLog);
                if (!EventLog.SourceExists(EventLogSource))
                {
                    Tracer.TraceInformation($"creating-eventlog-source '{EventLogSource}'");
                    EventLog.CreateEventSource(EventLogSource, EventLogName);
                }
            }
            catch (Exception ex)
            {
                Tracer.TraceError("could-not-initialize", ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(SQLManagementAgent));
            }
        }
    }
}
