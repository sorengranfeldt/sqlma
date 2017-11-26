using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.MetadirectoryServices;
using System.Data;
using System.Data.SqlClient;

namespace Granfeldt
{
	[TestClass]
	public class SqlTests
	{
		SqlMethods methods = new SqlMethods();

		[TestMethod]
		public void GetObjectClasses()
		{
			methods.OpenConnection();
			foreach (string str in methods.GetObjectClasses())
			{
				Tracer.TraceInformation("got-objectclass {0}", str);
			}
			methods.Dispose();
		}

		[TestMethod]
		public void GetSingleValueColumns()
		{
			methods.OpenConnection();
			foreach (AttributeDefinition ad in methods.GetSchema(Configuration.TableNameSingle))
			{
			}
			methods.Dispose();
		}

		[TestMethod]
		public void GetMultiValueColumns()
		{
			methods.OpenConnection();
			foreach (AttributeDefinition ad in methods.GetSchema(Configuration.TableNameMulti))
			{
			}
			methods.Dispose();
		}

		[TestMethod]
		public void GetMASchema()
		{

			using (SQLManagementAgent ma = new SQLManagementAgent())
			{
				Configuration.Schema = ma.DefaultSchemaXml.XmlDeserializeFromString<SchemaConfiguration>();
				Schema schema = ma.GetSchemaDetached();
			}
		}

		[TestMethod]
		public void GetFullImport()
		{
			using (SQLManagementAgent ma = new SQLManagementAgent())
			{
				Configuration.Schema = ma.DefaultSchemaXml.XmlDeserializeFromString<SchemaConfiguration>();
				Schema schema = ma.GetSchemaDetached();

				ma.Schema = schema;
				ma.ImportType = OperationType.Full;

				OpenImportConnectionRunStep dummyOpenImportRunStep = new OpenImportConnectionRunStep();

				// fake runstep data
				ma.ImportType = OperationType.Full;
				ma.CustomData = "";
				ma.PageSize = 100;

				System.Collections.ObjectModel.KeyedCollection<string, ConfigParameter> configParams = null;
				ma.OpenImportConnectionDetached(configParams, schema, null);

				GetImportEntriesRunStep rs = new GetImportEntriesRunStep();

				GetImportEntriesResults rest = new GetImportEntriesResults();
				rest.MoreToImport = true;
				while (rest.MoreToImport)
				{
                    rest = ma.GetImportEntriesDetached(rs);
				}
				
				CloseImportConnectionRunStep dummyCloseImportRunStep = null;
				ma.CloseImportConnectionDetached(dummyCloseImportRunStep);
			}
		}
		[TestMethod]
		public void GetDeltaImport()
		{
			using (SQLManagementAgent ma = new SQLManagementAgent())
			{
				Configuration.Schema = ma.DefaultSchemaXml.XmlDeserializeFromString<SchemaConfiguration>();
				Schema schema = ma.GetSchemaDetached();

				// first get full data to get deltawatermark
				ma.Schema = schema;
				ma.ImportType = OperationType.Delta;
				ma.CustomData = "140180";
				ma.PageSize = 1;

				System.Collections.ObjectModel.KeyedCollection<string, ConfigParameter> configParams = null;
				ma.OpenImportConnectionDetached(configParams, schema, null);

				GetImportEntriesRunStep rs = new GetImportEntriesRunStep();

				GetImportEntriesResults rest = new GetImportEntriesResults();
				rest.MoreToImport = true;
				while (rest.MoreToImport)
				{
					rest = ma.GetImportEntriesDetached(rs);
				}

				CloseImportConnectionRunStep dummyCloseImportRunStep = null;
				ma.CloseImportConnectionDetached(dummyCloseImportRunStep);
			}
		}

	}
}
