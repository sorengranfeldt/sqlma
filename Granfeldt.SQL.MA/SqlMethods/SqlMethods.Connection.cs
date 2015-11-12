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
		SqlConnection con;

		public void OpenConnection()
		{
			Tracer.Enter("openconnection");
			
			Configuration.ConnectionString = Configuration.ConnectionString.Replace("{username}", Configuration.UserName);
			Configuration.ConnectionString = Configuration.ConnectionString.Replace("{domain}", Configuration.Domain);
			string maskedConnectionString = Configuration.ConnectionString;
			maskedConnectionString = Configuration.ConnectionString.Replace("{password}", "***");
			Configuration.ConnectionString = Configuration.ConnectionString.Replace("{password}", Configuration.Password);

			Tracer.TraceInformation("connection-string {0}", maskedConnectionString);
			con = new SqlConnection(Configuration.ConnectionString);
			con.Open();
			if (con.State == System.Data.ConnectionState.Open)
			{
				Tracer.TraceInformation("sql-server-version {0}", con.ServerVersion);
			}
			Tracer.Exit("openconnection");
		}
		public void CloseConnection()
		{
			Tracer.Enter("closeconnection");
			Tracer.TraceInformation("connection-state {0}", con.State);
			if (con.State != System.Data.ConnectionState.Closed)
			{
				con.Close();
				Tracer.TraceInformation("connection-closed");
			}
			else
			{
				Tracer.TraceInformation("connection-already-closed");
			}
			Tracer.Exit("closeconnection");
		}
	}
}