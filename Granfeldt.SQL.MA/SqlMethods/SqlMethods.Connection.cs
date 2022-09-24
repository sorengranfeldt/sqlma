using System;
using System.Data.SqlClient;

namespace Granfeldt
{

    public partial class SqlMethods : IDisposable
    {
        SqlConnection con;

        public void OpenConnection()
        {
            Tracer.Enter(nameof(OpenConnection));

            Configuration.ConnectionString = Configuration.ConnectionString.Replace("{username}", Configuration.UserName);
            Configuration.ConnectionString = Configuration.ConnectionString.Replace("{domain}", Configuration.Domain);
            string maskedConnectionString = Configuration.ConnectionString;
            maskedConnectionString = Configuration.ConnectionString.Replace("{password}", "***");
            Configuration.ConnectionString = Configuration.ConnectionString.Replace("{password}", Configuration.Password);

            Tracer.TraceInformation($"connection-string {maskedConnectionString}");

            SetupImpersonationToken();

            con = new SqlConnection(Configuration.ConnectionString);
            con.Open();
            if (con.State == System.Data.ConnectionState.Open)
            {
                Tracer.TraceInformation($"sql-server-version {con.ServerVersion}");
            }
            Tracer.Exit(nameof(OpenConnection));
        }
        public void CloseConnection()
        {
            Tracer.Enter(nameof(CloseConnection));
            Tracer.TraceInformation($"connection-state {con.State}");
            if (con.State != System.Data.ConnectionState.Closed)
            {
                con.Close();
                Tracer.TraceInformation("connection-closed");
            }
            else
            {
                Tracer.TraceInformation("connection-already-closed");
            }
            RevertImpersonation();
            Tracer.Exit(nameof(CloseConnection));
        }
    }
}