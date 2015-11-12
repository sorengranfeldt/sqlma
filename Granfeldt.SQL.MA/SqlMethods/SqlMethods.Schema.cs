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

	public partial class SqlMethods : IDisposable
	{
		// schema
		public IEnumerable<string> GetObjectClasses()
		{
			Tracer.Enter("getobjectclasses");
			try
			{
				string query = string.Format("select distinct {0} from [{1}]", Configuration.ObjectClass, Configuration.TableNameSingle);
				Tracer.TraceInformation("run-query '{0}'", query);
				SqlCommand command = new SqlCommand(query, con);
				using (SqlDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						Tracer.TraceInformation("name: {0}, value: {1}", reader.GetName(0), reader[0]);
						yield return reader[0] as string;
					}
				}
			}
			finally
			{
				Tracer.Exit("getobjectclasses");
			}
		}
		public IEnumerable<AttributeDefinition> GetSchema(string TableName)
		{
			Tracer.Enter("getschema");
			try
			{
				string query = string.Format("select top 1 * from [{0}]", TableName);
				Tracer.TraceInformation("run-query '{0}'", query);
				SqlCommand command = new SqlCommand(query, con);
				using (SqlDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						for (int n = 0; n < reader.FieldCount; n++)
						{
							yield return new AttributeDefinition(reader.GetName(n), reader.GetDataTypeName(n));
						}
					}
				}
			}
			finally
			{
				Tracer.Exit("getschema");
			}
		}

	}
}