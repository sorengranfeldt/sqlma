using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.MetadirectoryServices;
using System.Data;
using System.Globalization;

namespace Granfeldt
{
	public partial class SQLManagementAgent : IDisposable, IMAExtensible2GetCapabilities, IMAExtensible2GetSchema, IMAExtensible2GetParameters, IMAExtensible2CallImport, IMAExtensible2CallExport
	{
		int IMAExtensible2CallImport.ImportDefaultPageSize
		{
			get
			{
				return 100;
			}
		}
		int IMAExtensible2CallImport.ImportMaxPageSize
		{
			get
			{
				return 500;
			}
		}
		OpenImportConnectionResults IMAExtensible2CallImport.OpenImportConnection(KeyedCollection<string, ConfigParameter> configParameters, Schema types, OpenImportConnectionRunStep importRunStep)
		{
			return OpenImportConnectionDetached(configParameters, types, importRunStep);
		}
		GetImportEntriesResults IMAExtensible2CallImport.GetImportEntries(GetImportEntriesRunStep importRunStep)
		{
			return GetImportEntriesDetached(importRunStep);
		}
		CloseImportConnectionResults IMAExtensible2CallImport.CloseImportConnection(CloseImportConnectionRunStep importRunStep)
		{
			return CloseImportConnectionDetached(importRunStep);
		}
	}

}
