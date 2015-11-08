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
		public void AddRecord(object anchor, out object newId, string objectClass = null)
		{
			Tracer.Enter("addrecord");
			newId = anchor;
			try
			{
				string query;
				if (anchor == null)
				{
					query = string.Format("insert into [{0}] ([{1}]) values (@objectclass)", Configuration.TableNameSingle, Configuration.ObjectClass);
					if (Configuration.ObjectClassType == ObjectClassType.Fixed)
					{
						query = string.Format("insert into [{0}] default values", Configuration.TableNameSingle);
					}
				}
				else
				{
					query = string.Format("insert into [{0}] ([{1}], [{2}]) values (@anchor, @objectclass)", Configuration.TableNameSingle, Configuration.AnchorColumn, Configuration.ObjectClass);
					if (Configuration.ObjectClassType == ObjectClassType.Fixed)
					{
						query = string.Format("insert into [{0}] ([{1}]) values (@anchor)", Configuration.TableNameSingle, Configuration.AnchorColumn);
					}
				}
				query = string.Concat("; select scope_identity();", query);
				using (SqlCommand cmd = new SqlCommand(query, con))
				{
					cmd.Parameters.AddWithValue("@anchor", anchor);
					cmd.Parameters.AddWithValue("@objectclass", objectClass);
					Tracer.TraceInformation("run-query {0}", query);
					newId = (object)cmd.ExecuteScalar();
					if (newId == null)
					{
						newId = anchor;
					}
				}
			}
			catch (Exception ex)
			{
				Tracer.TraceError("addrecord", ex);
			}
			finally
			{
				Tracer.Exit("addrecord");
			}
		}
		public bool ExistRecord(object anchor)
		{
			Tracer.Enter("existrecord");
			try
			{
				string query = string.Format("select [{0}] from [{1}] where [{0}] = @anchor;", Configuration.AnchorColumn, Configuration.TableNameSingle, Configuration.AnchorColumn);
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
				Tracer.TraceError("existrecord", ex);
			}
			finally
			{
				Tracer.Exit("existrecord");
			}
			return false;
		}
		public void Undelete(object anchor)
		{
			Tracer.Enter("undelete");
			try
			{
				string query = string.Format("update [{0}] set [{1}] = null where [{2}] = @anchor", Configuration.TableNameSingle, Configuration.DeletedColumn, Configuration.AnchorColumn);
				using (SqlCommand cmd = new SqlCommand(query, con))
				{
					cmd.Parameters.AddWithValue("@anchor", anchor);
					Tracer.TraceInformation("run-query {0}", query);
					Tracer.TraceInformation("rows-affected {0:n0}", cmd.ExecuteNonQuery());
				}
			}
			catch (Exception ex)
			{
				Tracer.TraceError("undelete", ex);
			}
			finally
			{
				Tracer.Exit("undelete");
			}
		}
		public void DeleteRecord(object anchor, bool deleteMultiValues = false, bool softDelete = false)
		{
			Tracer.Enter("deleterecord");
			try
			{
				string query = null;

				if (softDelete)
				{
					query = string.Format("update [{0}] set [{1}] = 1 where [{2}] = @anchor;", Configuration.TableNameSingle, Configuration.DeletedColumn, Configuration.AnchorColumn);
					if (deleteMultiValues)
					{
						string.Concat(query, string.Format("update [{0}] set [{1}] = 1 where [{2}] = @anchor;", Configuration.TableNameMulti, Configuration.DeletedColumn, Configuration.BackReferenceColumn));
					}
				}
				else
				{
					query = string.Format("delete from [{0}] where [{1}] = @anchor;", Configuration.TableNameSingle, Configuration.AnchorColumn);
					if (deleteMultiValues)
					{
						string.Concat(query, string.Format("delete from [{0}] where [{1}] = @anchor;", Configuration.TableNameMulti, Configuration.BackReferenceColumn));
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
				Tracer.TraceError("addsinglevalue", ex);
			}
			finally
			{
				Tracer.Exit("addsinglevalue");
			}
		}

		public void AddSingleValue(object anchor, string attributeName, object value)
		{
			Tracer.Enter("addsinglevalue");
			try
			{
				string query = string.Format("update [{0}] set [{1}] = @value where ([{2}] = @anchor)", Configuration.TableNameSingle, attributeName, Configuration.AnchorColumn);
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
				Tracer.TraceError("addsinglevalue", ex);
			}
			finally
			{
				Tracer.Exit("addsinglevalue");
			}
		}
		public void DeleteSingleValue(object anchor, string attributeName)
		{
			Tracer.Enter("deletesinglevalue");
			try
			{
				string query = null;
				query = string.Format("update [{0}] set [{1}] = null where ([{2}] = @anchor)", Configuration.TableNameSingle, attributeName, Configuration.AnchorColumn);

				using (SqlCommand cmd = new SqlCommand(query, con))
				{
					cmd.Parameters.AddWithValue("@anchor", anchor);
					Tracer.TraceInformation("run-query {0}", query);
					Tracer.TraceInformation("rows-affected {0:n0}", cmd.ExecuteNonQuery());
				}
			}
			catch (Exception ex)
			{
				Tracer.TraceError("deletesinglevalue", ex);
			}
			finally
			{
				Tracer.Exit("deletesinglevalue");
			}
		}

		public void AddMultiValue(object anchor, string attributeName, object value)
		{
			Tracer.Enter("addmultivalue");
			try
			{
				string query = string.Format("insert into [{0}] ([{1}], [{2}]) values (@anchor, @value)", Configuration.TableNameMulti, Configuration.BackReferenceColumn, attributeName);
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
				Tracer.TraceError("addmultivalue", ex);
			}
			finally
			{
				Tracer.Exit("addmultivalue");
			}
		}
		public void DeleteMultiValue(object anchor, string attributeName, object value, bool softDelete)
		{
			Tracer.Enter("deletemultivalue");
			try
			{
				string query = null;
				if (softDelete)
				{
					query = string.Format("update [{0}] set [{1}] = 1 where ([{2}] = @anchor and [{3}] = @value)", Configuration.TableNameMulti, Configuration.DeletedColumn, Configuration.BackReferenceColumn, attributeName);
				}
				else
				{
					query = string.Format("delete from [{0}] where ([{1}] = @anchor and [{2}] = @value)", Configuration.TableNameMulti, Configuration.BackReferenceColumn, attributeName);
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
				Tracer.TraceError("deletemultivalue", ex);
			}
			finally
			{
				Tracer.Exit("deletemultivalue");
			}
		}
		public void RemoveAllMultiValues(object anchor, string attributeName, bool softDelete)
		{
			Tracer.Enter("removeallmultivalues");
			try
			{
				string query = null;
				if (softDelete)
				{
					query = string.Format("update [{0}] set [{1}] = 1 where ([{2}] = @anchor and [{3}] is not null and [{1}] <> 1)", Configuration.TableNameMulti, Configuration.DeletedColumn, Configuration.BackReferenceColumn, attributeName);
				}
				else
				{
					query = string.Format("delete from [{0}] where ([{1}] = @anchor and [{2}] is not null)", Configuration.TableNameMulti, Configuration.BackReferenceColumn, attributeName);
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
				Tracer.TraceError("removeallmultivalues", ex);
			}
			finally
			{
				Tracer.Exit("removeallmultivalues");
			}
		}
	}
}
