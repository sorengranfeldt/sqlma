using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.MetadirectoryServices;

namespace Granfeldt
{
	public class AttributeDefinition
	{
		public string Name;
		public AttributeType AttributeType;
		public AttributeOperation AttributeOperation;

		public AttributeDefinition(string Name, string DataTypeName)
		{
			this.Name = Name;
			AttributeOperation = AttributeOperation.ImportExport;
			switch (DataTypeName)
			{
				case "int": this.AttributeType = AttributeType.Integer; break;
				case "bigint": this.AttributeType = AttributeType.Integer; break;
				case "tinyint": this.AttributeType = AttributeType.Integer; break;
				case "smallint": this.AttributeType = AttributeType.Integer; break;

				case "bit": this.AttributeType = AttributeType.Boolean; break;

				case "binary": this.AttributeType = AttributeType.Binary; break;
				case "varbinary": this.AttributeType = AttributeType.Binary; break;
				case "image": this.AttributeType = AttributeType.Binary; break;
				case "uniqueidentifier": this.AttributeType = AttributeType.Binary; break;

				case "date": this.AttributeType = AttributeType.String; break;
				case "datetime": this.AttributeType = AttributeType.String; break;
				case "smalldatetime": this.AttributeType = AttributeType.String; break;
				case "datetime2": this.AttributeType = AttributeType.String; break;
				case "datetimeoffset": this.AttributeType = AttributeType.String; break;
				case "time": this.AttributeType = AttributeType.String; break;
				case "timestamp": this.AttributeType = AttributeType.String; AttributeOperation = AttributeOperation.ImportOnly; break;
				case "nvarchar": this.AttributeType = AttributeType.String; break;
				case "char": this.AttributeType = AttributeType.String; break;
				case "varchar": this.AttributeType = AttributeType.String; break;
				case "text": this.AttributeType = AttributeType.String; break;
				case "nchar": this.AttributeType = AttributeType.String; break;
				case "ntext": this.AttributeType = AttributeType.String; break;
				case "decimal": this.AttributeType = AttributeType.String; break;
				case "float": this.AttributeType = AttributeType.String; break;
				case "money": this.AttributeType = AttributeType.String; break;
				case "smallmoney": this.AttributeType = AttributeType.String; break;
				case "real": this.AttributeType = AttributeType.String; break;
				case "xml": this.AttributeType = AttributeType.String; break;
				default:
					Tracer.TraceWarning("non-supported-type name: {0}, sql-type: {1}", Name, DataTypeName);
					this.AttributeType = AttributeType.String;
					break;
			}
			Tracer.TraceInformation("name: {0}, sql-type: {1}, selected-schematype: {2}", Name, DataTypeName, AttributeType);
		}
	}

}