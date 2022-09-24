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
        public enum ImportType
        {
            Full,
            Delta
        }

        public DataSet ReadObjects(List<object> anchors)
        {
            Tracer.Enter(nameof(ReadObjects));
            DataSet ds = new DataSet();
            try
            {
                string singleanchor = Configuration.AnchorColumn;
                string multivalueanchorref = Configuration.BackReferenceColumn;
                DataTable single = new DataTable(Configuration.TableNameSingle);
                DataTable multi = Configuration.HasMultivalueTable ? new DataTable(Configuration.TableNameMulti) : null;
                ds.Tables.Add(single);

                StringBuilder anchorList = new StringBuilder();
                for (int i = 0; i < anchors.Count; i++)
                {
                    if (i > 0) anchorList.Append(",");
                    anchorList.AppendFormat("'{0}'", anchors[i]);
                }

                StringBuilder query = new StringBuilder();
                query.AppendFormat("select * from {0} where [{1}] in ({2});", Configuration.TableNameSingle, singleanchor, anchorList);
                if (Configuration.HasMultivalueTable)
                {
                    query.AppendFormat("select * from {0} where [{1}] in ({2});", Configuration.TableNameMulti, multivalueanchorref, anchorList);
                    ds.Tables.Add(multi);
                }
                Tracer.TraceInformation($"run-query '{query.ToString()}'");
                using (SqlCommand cmd = new SqlCommand(query.ToString(), con))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (Configuration.HasMultivalueTable)
                        {
                            ds.Load(dr, LoadOption.OverwriteChanges, single, multi);
                            DataRelation relCustOrder = new DataRelation("mv", single.Columns[singleanchor], multi.Columns[multivalueanchorref]);
                            ds.Relations.Add(relCustOrder);
                        }
                        else
                        {
                            ds.Load(dr, LoadOption.OverwriteChanges, single);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.TraceError($"{nameof(ReadObjects)} {0}", ex.Message);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(ReadObjects));
            }
            return ds;
        }
        public IEnumerable<object> GetAllAnchors(ImportType importType, string customData = null)
        {
            Tracer.Enter(nameof(GetAllAnchors));
            List<object> results = new List<object>();
            try
            {
                List<string> queries = new List<string>();

                if (importType == ImportType.Full)
                {
                    if (Configuration.HasDeletedColumn)
                    {
                        queries.Add(string.Format("select [{0}] from {1} where ([{2}] is null) or ([{2}] = 0)", Configuration.AnchorColumn, Configuration.TableNameSingle, Configuration.DeletedColumn));
                    }
                    else
                    {
                        queries.Add(string.Format("select [{0}] from {1}", Configuration.AnchorColumn, Configuration.TableNameSingle));
                    }
                }
                else
                {
                    //TODO: should use SqlParameters to handle sql injections, but still fighting how to pass timestamps
                    string deltaWaterMark = null;
                    if (importType == ImportType.Delta)
                    {
                        if (Configuration.DeltaColumnType == DeltaColumnType.Rowversion)
                        {
                            customData = string.IsNullOrEmpty(customData) ? "00" : customData;
                            deltaWaterMark = string.Concat("0x", ulong.Parse(customData).ToString("X16"));
                        }
                        else
                        {
                            customData = string.IsNullOrEmpty(customData) ? DateTime.MinValue.ToString(Configuration.DateFormat) : customData;
                            deltaWaterMark = string.Concat("'", customData.TrimEnd('Z'), "'");
                        }
                    }
                    queries.Add(string.Format("select [{0}] from {1} where ([{2}] > {3})", Configuration.AnchorColumn, Configuration.TableNameSingle, Configuration.DeltaColumn, deltaWaterMark));
                    if (Configuration.HasMultivalueTable)
                    {
                        queries.Add(string.Format("select [{0}] from {1} where ([{2}] > {3})", Configuration.BackReferenceColumn, Configuration.TableNameMulti, Configuration.DeltaColumn, deltaWaterMark));
                    }
                }

                foreach (string query in queries)
                {
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        Tracer.TraceInformation($"run-query '{query}'");
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(reader.GetProviderSpecificValue(0));
                            }
                        }
                    }
                }
                Tracer.TraceInformation("no-of-anchors {0:n0}", results.Count);
                results = results.Distinct().ToList();
                Tracer.TraceInformation("no-of-distinct-anchors {0:n0}", results.Count);
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(GetAllAnchors), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(GetAllAnchors));
            }
            return results;
        }

    }
}
