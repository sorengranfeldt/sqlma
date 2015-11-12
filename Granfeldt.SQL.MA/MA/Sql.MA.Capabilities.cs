using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.MetadirectoryServices;

namespace Granfeldt
{
	public partial class SQLManagementAgent : IDisposable, IMAExtensible2GetCapabilities, IMAExtensible2GetSchema, IMAExtensible2GetParameters, IMAExtensible2CallImport, IMAExtensible2CallExport
	{
		MACapabilities IMAExtensible2GetCapabilities.Capabilities
		{
			get
			{
				Tracer.Enter("getcapabilities");
				MACapabilities cap = new MACapabilities();
				cap.ExportPasswordInFirstPass = false;
				cap.ConcurrentOperation = true;
				cap.DeltaImport = true;
				cap.DistinguishedNameStyle = MADistinguishedNameStyle.Generic;
				cap.ExportType = MAExportType.AttributeUpdate;
				cap.FullExport = true;
				cap.ObjectConfirmation = MAObjectConfirmation.Normal;
				cap.ObjectRename = false;
				cap.NoReferenceValuesInFirstExport = false;
				Tracer.Exit("getcapabilities");
				return cap;
			}
		}

	}

}
