// august 8, 2016 | soren granfeldt
//	- changed string.Concat element order to fix buggy select scope_identity
// september 20, 2022 | soren granfeldt
//	- add throw to try/catch to bubble any errors up
// march 27, 2024 | soren granfeldt
//  - merged fix for bug where deletes are exported and softdeletes are used and the DeletedColumn is NULL instead of 0 then the delete is not processed.

using System;
using System.Data.SqlClient;


namespace Granfeldt
{
    public partial class SqlMethods : IDisposable
    {
        public void AddRecord(object anchor, out object newId, string objectClass = null)
        {
            Tracer.Enter(nameof(AddRecord));
            newId = anchor;
            try
            {
                string query;
                if (anchor == null)
                {
                    query = string.Format("insert into {0} ([{1}]) values (@objectclass)", Configuration.TableNameSingle, Configuration.ObjectClass);
                    if (Configuration.ObjectClassType == ObjectClassType.Fixed)
                    {
                        query = string.Format("insert into {0} default values", Configuration.TableNameSingle);
                    }
                }
                else
                {
                    query = string.Format("insert into {0} ([{1}], [{2}]) values (@anchor, @objectclass)", Configuration.TableNameSingle, Configuration.AnchorColumn, Configuration.ObjectClass);
                    if (Configuration.ObjectClassType == ObjectClassType.Fixed)
                    {
                        query = string.Format("insert into {0} ([{1}]) values (@anchor)", Configuration.TableNameSingle, Configuration.AnchorColumn);
                    }
                }
                query = string.Concat(query, "; select scope_identity();");
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@anchor", anchor);
                    cmd.Parameters.AddWithValue("@objectclass", objectClass);
                    Tracer.TraceInformation("run-query {0}", query);
                    newId = (object)cmd.ExecuteScalar();
                    if (newId == null)
                    {
                        Tracer.TraceInformation("no-new-anchor-returned (scope_identity)");
                        newId = anchor;
                    }
                    else
                    {
                        Tracer.TraceInformation("scope_identity-returned {0}", newId);
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(AddRecord), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(AddRecord));
            }
        }
        public bool ExistRecord(object anchor)
        {
            Tracer.Enter(nameof(ExistRecord));
            try
            {
                string query = string.Format("select [{0}] from {1} where [{0}] = @anchor;", Configuration.AnchorColumn, Configuration.TableNameSingle, Configuration.AnchorColumn);
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@anchor", anchor);
                    Tracer.TraceInformation("run-query {0}", query);
                    object affectedRecords = (object)cmd.ExecuteScalar();
                    Tracer.TraceInformation("exists {0}", affectedRecords != null);
                    return affectedRecords != null;
                }
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(ExistRecord), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(ExistRecord));
            }
            return false;
        }
        public void Undelete(object anchor)
        {
            Tracer.Enter(nameof(Undelete));
            try
            {
                string query = string.Format("update {0} set [{1}] = 0 where [{2}] = @anchor", Configuration.TableNameSingle, Configuration.DeletedColumn, Configuration.AnchorColumn);
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@anchor", anchor);
                    Tracer.TraceInformation("run-query {0}", query);
                    Tracer.TraceInformation("rows-affected {0:n0}", cmd.ExecuteNonQuery());
                }
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(Undelete), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(Undelete));
            }
        }
        public void DeleteRecord(object anchor, bool deleteMultiValues = false, bool softDelete = false)
        {
            Tracer.Enter(nameof(DeleteRecord));
            try
            {
                string query = null;

                if (softDelete)
                {
                    query = string.Format("update {0} set [{1}] = 1 where [{2}] = @anchor;", Configuration.TableNameSingle, Configuration.DeletedColumn, Configuration.AnchorColumn);
                    if (deleteMultiValues)
                    {
                        string.Concat(query, string.Format("update {0} set [{1}] = 1 where [{2}] = @anchor;", Configuration.TableNameMulti, Configuration.DeletedColumn, Configuration.BackReferenceColumn));
                    }
                }
                else
                {
                    query = string.Format("delete from {0} where [{1}] = @anchor;", Configuration.TableNameSingle, Configuration.AnchorColumn);
                    if (deleteMultiValues)
                    {
                        string.Concat(query, string.Format("delete from {0} where [{1}] = @anchor;", Configuration.TableNameMulti, Configuration.BackReferenceColumn));
                    }
                }
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@anchor", anchor);
                    Tracer.TraceInformation("run-query {0}", query);
                    Tracer.TraceInformation("rows-affected {0:n0}", cmd.ExecuteNonQuery());
                }
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(DeleteRecord), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(DeleteRecord));
            }
        }

        public void AddSingleValue(object anchor, string attributeName, object value)
        {
            Tracer.Enter(nameof(AddSingleValue));
            try
            {
                string query = string.Format("update {0} set [{1}] = @value where ([{2}] = @anchor)", Configuration.TableNameSingle, attributeName, Configuration.AnchorColumn);
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@anchor", anchor);
                    cmd.Parameters.AddWithValue("@value", value);
                    Tracer.TraceInformation("run-query {0}", query);
                    Tracer.TraceInformation("rows-affected {0:n0}", cmd.ExecuteNonQuery());
                }
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(AddSingleValue), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(AddSingleValue));
            }
        }
        public void DeleteSingleValue(object anchor, string attributeName)
        {
            Tracer.Enter(nameof(DeleteSingleValue));
            try
            {
                string query = null;
                query = string.Format("update {0} set [{1}] = null where ([{2}] = @anchor)", Configuration.TableNameSingle, attributeName, Configuration.AnchorColumn);

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@anchor", anchor);
                    Tracer.TraceInformation("run-query {0}", query);
                    Tracer.TraceInformation("rows-affected {0:n0}", cmd.ExecuteNonQuery());
                }
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(DeleteSingleValue), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(DeleteSingleValue));
            }
        }

        public void AddMultiValue(object anchor, string attributeName, object value)
        {
            Tracer.Enter(nameof(AddMultiValue));
            try
            {
                string query = string.Format("insert into {0} ([{1}], [{2}]) values (@anchor, @value)", Configuration.TableNameMulti, Configuration.BackReferenceColumn, attributeName);
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@anchor", anchor);
                    cmd.Parameters.AddWithValue("@value", value);
                    Tracer.TraceInformation("run-query {0}", query);
                    Tracer.TraceInformation("rows-affected {0:n0}", cmd.ExecuteNonQuery());
                }
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(AddMultiValue), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(AddMultiValue));
            }
        }
        public void DeleteMultiValue(object anchor, string attributeName, object value, bool softDelete)
        {
            Tracer.Enter(nameof(DeleteMultiValue));
            try
            {
                string query = null;
                if (softDelete)
                {
                    query = string.Format("update {0} set [{1}] = 1 where ([{2}] = @anchor and [{3}] = @value)", Configuration.TableNameMulti, Configuration.DeletedColumn, Configuration.BackReferenceColumn, attributeName);
                }
                else
                {
                    query = string.Format("delete from {0} where ([{1}] = @anchor and [{2}] = @value)", Configuration.TableNameMulti, Configuration.BackReferenceColumn, attributeName);
                }
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@anchor", anchor);
                    cmd.Parameters.AddWithValue("@value", value);
                    Tracer.TraceInformation("run-query {0}", query);
                    Tracer.TraceInformation("rows-affected {0:n0}", cmd.ExecuteNonQuery());
                }
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(DeleteMultiValue), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(DeleteMultiValue));
            }
        }
        public void RemoveAllMultiValues(object anchor, string attributeName, bool softDelete)
        {
            Tracer.Enter(nameof(RemoveAllMultiValues));
            try
            {
                string query = null;
                if (softDelete)
                {
                    query = string.Format("update {0} set [{1}] = 1 where ([{2}] = @anchor and [{3}] is not null and ([{1}] is null) or ([{1}] = 0))", Configuration.TableNameMulti, Configuration.DeletedColumn, Configuration.BackReferenceColumn, attributeName);
                }
                else
                {
                    query = string.Format("delete from {0} where ([{1}] = @anchor and [{2}] is not null)", Configuration.TableNameMulti, Configuration.BackReferenceColumn, attributeName);
                }
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@anchor", anchor);
                    Tracer.TraceInformation("run-query {0}", query);
                    Tracer.TraceInformation("rows-affected {0:n0}", cmd.ExecuteNonQuery());
                }
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(RemoveAllMultiValues), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(RemoveAllMultiValues));
            }
        }
    }
}
