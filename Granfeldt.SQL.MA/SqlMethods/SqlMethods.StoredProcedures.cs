using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Granfeldt
{
	public partial class SqlMethods : IDisposable
	{
		public void RunStoredProcedure(string query)
		{
			Tracer.Enter("runstoredprocedure");
			try
			{
				using (SqlCommand cmd = new SqlCommand(query, con))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					Tracer.TraceInformation("run-storedprocedure {0}", query);
					Tracer.TraceInformation("rows-affected {0:n0}", cmd.ExecuteNonQuery());
				}
			}
			catch (Exception ex)
			{
				Tracer.TraceError("runstoredprocedure", ex);
			}
			finally
			{
				Tracer.Exit("runstoredprocedure");
			}
		}
		public void RunStoredProcedure(string query, IEnumerable<SqlParameter> parameters)
		{
			Tracer.Enter("runstoredprocedure");
			try
			{
				using (SqlCommand cmd = new SqlCommand(query, con))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					foreach (SqlParameter param in parameters)
					{
						Tracer.TraceInformation("add-parameter name: {0}, value: '{1}'", param.ParameterName, param.SqlValue);
						cmd.Parameters.Add(param);
					}
					Tracer.TraceInformation("run-storedprocedure {0}", query);
					Tracer.TraceInformation("rows-affected {0:n0}", cmd.ExecuteNonQuery());
				}
			}
			catch (Exception ex)
			{
				Tracer.TraceError("runstoredprocedure", ex);
			}
			finally
			{
				Tracer.Exit("runstoredprocedure");
			}
		}
	}
}
