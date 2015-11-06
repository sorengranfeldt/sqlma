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
		Schema _schema = null;
		public Schema Schema
		{
			get
			{
				return _schema;
			}
			set
			{
				_schema = value;
			}
		}

		public SQLManagementAgent()
		{
			Tracer.IndentLevel = 0;
			Tracer.Enter("initialize");
			Tracer.Indent();
			try
			{
				System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
				FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
				string version = fvi.FileVersion;
				Tracer.TraceInformation("sqlma-version {0}", version);
				Tracer.TraceInformation("reading-registry-settings");

				Tracer.TraceInformation("adding-eventlog-listener-for name: {0}, source: {1}", EventLogName, EventLogSource);
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
					Tracer.TraceInformation("creating-eventlog-source '{0}'", EventLogSource);
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
				Tracer.Unindent();
				Tracer.Exit("initialize");
			}
		}
	}
}
