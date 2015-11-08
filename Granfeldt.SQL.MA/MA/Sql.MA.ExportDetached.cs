using Microsoft.MetadirectoryServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Granfeldt
{
	public partial class SQLManagementAgent : IDisposable, IMAExtensible2GetCapabilities, IMAExtensible2GetSchema, IMAExtensible2GetParameters, IMAExtensible2CallImport, IMAExtensible2CallExport
	{
		OperationType _exportOperationType;
		int _exportBatchSize = 100;

		public int ExportBatchSize
		{
			get
			{
				return _exportBatchSize;
			}
			set
			{
				_exportBatchSize = value;
			}
		}
		public OperationType ExportType
		{
			get
			{
				return _exportOperationType;
			}
			set
			{
				_exportOperationType = value;
			}
		}

		string CSValueAsString(object Value, AttributeType DataType)
		{
			Tracer.TraceInformation("CSValueAsSting {0}, {1}", Value == null ? "(null)" : Value.GetType().ToString(), DataType);
			switch (DataType)
			{
				case AttributeType.Binary:
					return new Guid((Byte[])Value).ToString();
				default:
					return Value.ToString();
			}
		}
		public void OpenExportConnectionDetached(KeyedCollection<string, ConfigParameter> configParameters, Schema types, OpenExportConnectionRunStep exportRunStep)
		{
			Tracer.Enter("openexportconnectiondetached");
			try
			{
				InitializeConfigParameters(configParameters);

				Schema = types;

				ExportType = exportRunStep.ExportType;
				Tracer.TraceInformation("step-partition-id {0}", exportRunStep.StepPartition.Identifier);
				Tracer.TraceInformation("step-partition-dn {0}", exportRunStep.StepPartition.DN);
				Tracer.TraceInformation("step-partition-name {0}", exportRunStep.StepPartition.Name);
				Tracer.TraceInformation("export-type {0}", ExportType);
				ExportBatchSize = exportRunStep.BatchSize;
				Tracer.TraceInformation("export-batch-size {0:n0}", ExportBatchSize);

				methods.OpenConnection();

				if (Configuration.RunBeforeExport)
				{
					List<SqlParameter> parameters = new List<SqlParameter>();
					parameters.Add(new SqlParameter("exporttype", ExportType.ToString()));
					methods.RunStoredProcedure(Configuration.ExportCommandBefore, parameters);
				}
			}
			catch (Exception ex)
			{
				Tracer.TraceError("openexportconnection", ex);
				throw new TerminateRunException(ex.Message);
			}
			finally
			{
				Tracer.Exit("exit-openexportconnection");
			}
		}
		//TODO: Remember to return identity from SQL if possible
		public PutExportEntriesResults PutExportEntriesDetached(IList<CSEntryChange> csentries)
		{
			Tracer.Enter("putexportentriesdetached");
			PutExportEntriesResults results = new PutExportEntriesResults();
			try
			{
				bool handleSoftDeletion = Configuration.HasDeletedColumn;
				bool handleMultivalues = Configuration.HasMultivalueTable;

				foreach (CSEntryChange exportChange in csentries)
				{
					List<string> exportSqlCommands = new List<string>();
					List<AttributeChange> attrchanges = new List<AttributeChange>();

					string anchor = null;
					if (exportChange.AnchorAttributes.Count == 0)
					{
						Tracer.TraceInformation("no-anchor-present");
					}
					else
					{
						anchor = CSValueAsString(exportChange.AnchorAttributes[0].Value, exportChange.AnchorAttributes[0].DataType);
					}
					string objectClass = exportChange.ObjectType;

					if (Configuration.RunBeforeObjectExport)
					{
						List<SqlParameter> parameters = new List<SqlParameter>();
						parameters.Add(new SqlParameter("anchor", anchor));
						parameters.Add(new SqlParameter("action", exportChange.ObjectModificationType.ToString()));
						methods.RunStoredProcedure(Configuration.ExportObjectCommandBefore, parameters);
					}

					Tracer.TraceInformation("export-object {0}, cs-id: {1}, anchor: {2}, dn: {3} [{4}]", objectClass, exportChange.Identifier, anchor, exportChange.DN, exportChange.ObjectModificationType);
					try
					{
						// first try to handle a delete
						if (exportChange.ObjectModificationType == ObjectModificationType.Delete)
						{
							if (anchor == null)
							{
								throw new InvalidOperationException("cannot-delete-without-anchor");
							}

							Tracer.TraceInformation("deleting-record type: {1}, anchor: {0}", objectClass, anchor);
							methods.DeleteRecord(anchor, Configuration.HasMultivalueTable, handleSoftDeletion);
							results.CSEntryChangeResults.Add(CSEntryChangeResult.Create(exportChange.Identifier, attrchanges, MAExportError.Success));
							continue;
						}

						// if we get here its either an update or and add
						if (exportChange.ObjectModificationType == ObjectModificationType.Add)
						{
							Tracer.TraceInformation("adding-record type: {1}, anchor: {0}", objectClass, anchor);
							object newAnchor = anchor; // set it to incoming anchor
							if (anchor != null && methods.ExistRecord(anchor))
							{
								methods.Undelete(anchor);
							}
							else
							{
								methods.AddRecord(anchor, out newAnchor, objectClass);
							}
							attrchanges.Add(AttributeChange.CreateAttributeAdd(anchor, newAnchor));
						}

						//foreach (AttributeChange ac in exportChange.AttributeChanges)
						//{
						//	Tracer.TraceInformation("attribute-change {0}, {1}", ac.Name, ac.ModificationType);
						//}
						//foreach (string attributeChange in exportChange.ChangedAttributeNames)
						//{
						//	Tracer.TraceInformation("changed-attribute {0}", attributeChange);
						//}

						// updating attributes is common for add and update
						foreach (string attributeChange in exportChange.ChangedAttributeNames)
						{
							//Tracer.TraceInformation("attribute {0}", attributeChange);
							AttributeChange ac = exportChange.AttributeChanges[attributeChange];
							Tracer.TraceInformation("is-remove {0}", attributeChange == null);
							Tracer.TraceInformation("attribute-change {0}, {1}", ac.Name, ac.ModificationType);
							if (ac.IsMultiValued)
							{
								if (ac.ModificationType == AttributeModificationType.Delete)
								{
									methods.RemoveAllMultiValues(anchor, attributeChange, handleSoftDeletion);
									continue;
								}
								foreach (ValueChange vc in ac.ValueChanges)
								{
									switch (vc.ModificationType)
									{
										case ValueModificationType.Add:
											methods.AddMultiValue(anchor, attributeChange, vc.Value);
											break;
										case ValueModificationType.Delete:
											methods.DeleteMultiValue(anchor, attributeChange, vc.Value, handleSoftDeletion);
											break;
									}
								}
								continue;
							}
							if (ac.ModificationType == AttributeModificationType.Delete)
							{
								methods.DeleteSingleValue(anchor, attributeChange);
								continue;
							}
							foreach (ValueChange vc in ac.ValueChanges.Where(x => x.ModificationType == ValueModificationType.Add))
							{
								Tracer.TraceInformation("singlevalue-change {0}, {1}", vc.ModificationType, vc.Value);
								methods.AddSingleValue(anchor, attributeChange, vc.Value);
							}
						}
						results.CSEntryChangeResults.Add(CSEntryChangeResult.Create(exportChange.Identifier, attrchanges, MAExportError.Success));

						if (Configuration.RunAfterObjectExport)
						{
							List<SqlParameter> parameters = new List<SqlParameter>();
							parameters.Add(new SqlParameter("anchor", anchor));
							parameters.Add(new SqlParameter("action", exportChange.ObjectModificationType.ToString()));
							methods.RunStoredProcedure(Configuration.ExportObjectCommandAfter, parameters);
						}
					}
					catch (Exception exportEx)
					{
						Tracer.TraceError("putexportentriesdetached", exportEx);
						results.CSEntryChangeResults.Add(CSEntryChangeResult.Create(exportChange.Identifier, attrchanges, MAExportError.ExportErrorCustomContinueRun, "export-exception", exportEx.Message));
						throw;
					}
				}
			}
			catch (Exception ex)
			{
				Tracer.TraceError("putexportentriesdetached", ex);
				throw;
			}
			finally
			{
				Tracer.Exit("putexportentriesdetached");
			}
			return results;
		}
		public void CloseExportConnectionDetached(CloseExportConnectionRunStep exportRunStep)
		{
			Tracer.Enter("closeexportconnectiondetached");
			try
			{
				if (Configuration.RunAfterExport)
				{
					List<SqlParameter> parameters = new List<SqlParameter>();
					parameters.Add(new SqlParameter("exporttype", ExportType.ToString()));
					methods.RunStoredProcedure(Configuration.ExportCommandAfter, parameters);
				}
				methods.CloseConnection();
			}
			catch (Exception ex)
			{
				Tracer.TraceError("closeexportconnectiondetached", ex);
				throw;
			}
			finally
			{
				Tracer.Exit("closeexportconnection");
			}
		}
	}

}
