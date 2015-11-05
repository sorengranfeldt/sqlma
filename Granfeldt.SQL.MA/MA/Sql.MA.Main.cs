using Microsoft.MetadirectoryServices;
using System;

namespace Granfeldt
{
	public partial class SQLManagementAgent : IDisposable, IMAExtensible2GetCapabilities, IMAExtensible2GetSchema, IMAExtensible2GetParameters, IMAExtensible2CallImport, IMAExtensible2CallExport
    {
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
	}
}
