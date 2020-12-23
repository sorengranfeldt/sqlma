using Microsoft.MetadirectoryServices;
using System;

namespace Granfeldt
{
    public partial class SQLManagementAgent : IDisposable, IMAExtensible2GetCapabilities, IMAExtensible2GetSchema, IMAExtensible2GetParameters, IMAExtensible2CallImport, IMAExtensible2CallExport
	{
		MACapabilities IMAExtensible2GetCapabilities.Capabilities
		{
			get
			{
				Tracer.Enter(nameof(IMAExtensible2GetCapabilities.Capabilities));
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
				Tracer.Exit(nameof(IMAExtensible2GetCapabilities.Capabilities));
				return cap;
			}
		}

	}

}
