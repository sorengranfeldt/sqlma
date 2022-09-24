// november 13, 2019 | soren granfeldt
//  - removed and sorted usings
//  - added extra debug information

namespace Granfeldt
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public partial class SqlMethods : IDisposable
    {
        // schema
        public IEnumerable<string> GetObjectClasses()
        {
            Tracer.Enter(nameof(GetObjectClasses));
            try
            {
                string query = string.Format("select distinct {0} from {1}", Configuration.ObjectClass, Configuration.TableNameSingle);
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
                Tracer.Exit(nameof(GetObjectClasses));
            }
        }
        public IEnumerable<AttributeDefinition> GetSchema(string TableName)
        {
            Tracer.Enter(nameof(GetSchema));
            try
            {
                string query = $"select top 1 * from {TableName}";
                Tracer.TraceInformation($"run-query '{query}'");
                SqlCommand command = new SqlCommand(query, con);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int n = 0; n < reader.FieldCount; n++)
                        {
                            Tracer.TraceInformation($"return-column: name: {reader.GetName(n)}, type: {reader.GetDataTypeName(n)}");
                            yield return new AttributeDefinition(reader.GetName(n), reader.GetDataTypeName(n));
                        }
                    }
                }
            }
            finally
            {
                Tracer.Exit(nameof(GetSchema));
            }
        }

    }
}