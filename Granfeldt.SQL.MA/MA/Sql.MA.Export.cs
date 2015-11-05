using Microsoft.MetadirectoryServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Granfeldt
{
	public partial class SQLManagementAgent : IDisposable, IMAExtensible2GetCapabilities, IMAExtensible2GetSchema, IMAExtensible2GetParameters, IMAExtensible2CallImport, IMAExtensible2CallExport
	{
		int IMAExtensible2CallExport.ExportDefaultPageSize
		{
			get
			{
				return 100;
			}
		}
		int IMAExtensible2CallExport.ExportMaxPageSize
		{
			get
			{
				return 500;
			}
		}
		void IMAExtensible2CallExport.OpenExportConnection(KeyedCollection<string, ConfigParameter> configParameters, Schema types, OpenExportConnectionRunStep exportRunStep)
		{
			OpenExportConnectionDetached(configParameters, types, exportRunStep);
		}
		PutExportEntriesResults IMAExtensible2CallExport.PutExportEntries(IList<CSEntryChange> csentries)
		{
			return PutExportEntriesDetached(csentries);
		}
		void IMAExtensible2CallExport.CloseExportConnection(CloseExportConnectionRunStep exportRunStep)
		{
			CloseExportConnectionDetached(exportRunStep);
		}
	}

}
