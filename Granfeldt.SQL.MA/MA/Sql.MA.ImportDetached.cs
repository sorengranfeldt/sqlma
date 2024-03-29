﻿// march 2, 2017, soren granfeldt
//	-added disposable of dataset and lists after import
// november 13, 2019, soren granfeldt
//	- added handling of datetime types to use specific date format (same as FIM Service) to make sure local date formats are not used
// november 11, 2023, soren granfeldt
//	- added import check for included objectclasses (only return included object types)

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
        OperationType _importOperationType = OperationType.Full;
        string _customData = null;
        int _pageSize;
        List<object> importAnchors = null;
        List<CSEntryChange> importCsEntryQueue = null;
        public OperationType ImportType
        {
            get
            {
                return _importOperationType;
            }
            set
            {
                _importOperationType = value;
            }
        }

        public string CustomData
        {
            get
            {
                return _customData;
            }
            set
            {
                _customData = value;
            }
        }
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = value;
            }
        }

        bool IsDateTimeType(Type type) => typeof(DateTime) == type || typeof(DateTimeOffset) == type;

        object GetSafeValue(DataRow dataRow, string attributeName, AttributeType attributeType, bool DeltaDate = false)
        {
            object value = null;
            try
            {
                if (dataRow.IsNull(attributeName))
                {
                    return null;
                }
                switch (attributeType)
                {
                    case AttributeType.Binary:
                        value = dataRow.Field<object>(attributeName);
                        if (value.GetType().Equals(typeof(System.Guid)))
                        {
                            value = ((Guid)value).ToByteArray();
                        }
                        return value;
                    case AttributeType.Reference: // reference is always string in FIM
                        value = dataRow.Field<object>(attributeName);
                        return value.ToString();
                    case AttributeType.String:
                        if (DeltaDate)
                        {
                            value = dataRow.Field<object>(attributeName);
                            return value;
                        }
                        else
                        {
                            value = dataRow.Field<object>(attributeName);
                            if (IsDateTimeType(value.GetType()))
                            {
                                DateTime dt;
                                if (DateTime.TryParse(value.ToString(), out dt))
                                {
                                    value = dt.ToString(Configuration.DateFormat);
                                    return value;
                                }
                                else
                                {
                                    Tracer.TraceWarning("unable-to-parse-value-to-date: value: {0}", value);
                                }
                            }
                            return value.ToString();
                        }
                    default:
                        value = dataRow.Field<object>(attributeName);
                        return value;
                }
            }
            catch (Exception e)
            {
                Tracer.TraceError($"{nameof(GetSafeValue)}, attribute: {attributeName}, target-type: {attributeType}, value: {value}, error: {e.Message}");
                throw;
            }
        }
        void SetCustomData(DataRow row)
        {
            object deltaValue = null;
            try
            {
                if (!Configuration.HasDeltaColumn)
                {
                    return;
                }
                else
                {
                    if (Configuration.DeltaColumnType == DeltaColumnType.Rowversion)
                    {
                        ulong rowversionCustomData = string.IsNullOrEmpty(CustomData) ? 0 : ulong.Parse(CustomData);
                        deltaValue = GetSafeValue(row, Configuration.DeltaColumn, AttributeType.Binary);
                        ulong newRowversionCustomData = BitConverter.ToUInt64(((Byte[])deltaValue).Reverse().ToArray(), 0);
                        if (rowversionCustomData < newRowversionCustomData)
                        {
                            Tracer.TraceInformation("change-customdata old: {0}, new: {1}", rowversionCustomData, newRowversionCustomData);
                            CustomData = newRowversionCustomData.ToString();
                        }
                    }
                    else
                    {
                        //TODO: Documentation: Remember to write UTC for delta dates
                        DateTime currentCustomData = string.IsNullOrEmpty(CustomData) ? DateTime.MinValue.ToUniversalTime() : DateTime.Parse(CustomData).ToUniversalTime();
                        DateTime newCustomData = ((DateTime)GetSafeValue(row, Configuration.DeltaColumn, AttributeType.String, true));
                        if (currentCustomData < newCustomData)
                        {
                            Tracer.TraceInformation("change-customdata old: {0}, new: {1}", currentCustomData.ToString(Configuration.DateFormat), newCustomData.ToString(Configuration.DateFormat));
                            CustomData = newCustomData.ToString(Configuration.DateFormat);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Tracer.TraceError($"{nameof(SetCustomData)}, custom-data: {CustomData}, delta-value: {deltaValue}, error: {e.Message}");
                throw;
            }
        }
        IEnumerable<CSEntryChange> DataSetToCsEntryChanges(DataSet records)
        {
            Tracer.Enter(nameof(DataSetToCsEntryChanges));
            try
            {
                string singleanchor = Configuration.AnchorColumn;
                string DNColumn = Configuration.DNColumn;
                string objectclasscolumnname = Configuration.ObjectClass;
                string multivalueanchorref = Configuration.BackReferenceColumn;
                bool handleSoftDeletion = Configuration.HasDeletedColumn;
                bool handleMultivalues = Configuration.HasMultivalueTable;

                CSEntryChange csentry = null;
                foreach (DataRow singleRow in records.Tables[Configuration.TableNameSingle].Rows)
                {
                    try
                    {
                        csentry = CSEntryChange.Create();
                        string Dn = GetSafeValue(singleRow, DNColumn, AttributeType.String).ToString();
                        string objectClass = Configuration.ObjectClassType == ObjectClassType.Column ? GetSafeValue(singleRow, objectclasscolumnname, AttributeType.String).ToString() : Configuration.ObjectClass;
                        if (!Schema.Types.Contains(objectClass))
                        {
                            Tracer.TraceInformation($"skip-record objectclass: {objectClass}, DN: {Dn}");
                            continue;
                        }
                        object anchorValue = GetSafeValue(singleRow, singleanchor, Schema.Types[objectClass].AnchorAttributes[0].DataType);
                        bool isDeleted = false; // assume false
                        if (handleSoftDeletion)
                        {
                            isDeleted = (bool?)GetSafeValue(singleRow, Configuration.DeletedColumn, AttributeType.Boolean) ?? false;
                        }

                        try
                        {
                            csentry.ObjectModificationType = isDeleted ? ObjectModificationType.Delete : ObjectModificationType.Add;
                            csentry.ObjectType = objectClass;
                            csentry.DN = Dn;
                            csentry.AnchorAttributes.Add(AnchorAttribute.Create(singleanchor, anchorValue));

                            Tracer.TraceInformation($"start-record id: {anchorValue}, objectclass: {objectClass}, DN: {Dn}");

                            SetCustomData(singleRow);

                            if (isDeleted)
                            {
                                Tracer.TraceInformation("returning-deleted-object");
                            }
                            else
                            {
                                foreach (SchemaAttribute attr in Schema.Types[objectClass].Attributes.Where(a => !a.IsMultiValued))
                                {
                                    if (attr.Name == "export_password")
                                    {
                                        Tracer.TraceInformation("{0}, {1}", attr.Name, attr.AllowedAttributeOperation);
                                        continue;
                                    }
                                    if (singleRow.IsNull(attr.Name))
                                    {
                                        continue;
                                    }
                                    Tracer.TraceInformation($"get-single-value {attr.Name}");
                                    object value = GetSafeValue(singleRow, attr.Name, attr.DataType);
                                    Tracer.TraceInformation("add-single-value name: {0}, value: {1}: '{2}'", attr.Name, attr.DataType, value);
                                    csentry.AttributeChanges.Add(AttributeChange.CreateAttributeAdd(attr.Name, value));
                                }

                                if (handleMultivalues)
                                {
                                    foreach (SchemaAttribute attr in Schema.Types[objectClass].Attributes.Where(a => a.IsMultiValued))
                                    {
                                        List<object> mvs = new List<object>();
                                        foreach (DataRow row in singleRow.GetChildRows(records.Relations[0]).Where(r => !r.IsNull(attr.Name)))
                                        {
                                            isDeleted = false;
                                            if (handleSoftDeletion)
                                            {
                                                isDeleted = (bool?)GetSafeValue(row, Configuration.DeletedColumn, AttributeType.Boolean) ?? false;
                                            }
                                            SetCustomData(row); // we get delta value even for deleted

                                            if (!isDeleted)
                                            {
                                                Tracer.TraceInformation($"get-multi-value {attr.Name}");
                                                object value = GetSafeValue(row, attr.Name, attr.DataType);
                                                Tracer.TraceInformation("add-multi-value name: {0}, value: {1}: '{2}'", attr.Name, attr.DataType, value);
                                                mvs.Add(value);
                                            }
                                        }
                                        if (mvs.Count > 0)
                                        {
                                            csentry.AttributeChanges.Add(AttributeChange.CreateAttributeAdd(attr.Name, mvs));
                                        }
                                        mvs.Clear();
                                        mvs = null;
                                    }
                                }
                            }
                        }
                        catch (Exception csEx)
                        {
                            csentry.ErrorCodeImport = MAImportError.ImportErrorCustomContinueRun;
                            csentry.ErrorName = "import-convert-error";
                            csentry.ErrorDetail = csEx.Message;
                            Tracer.TraceError("converting-to-csentry", csEx);
                        }
                        finally
                        {
                            Tracer.TraceInformation($"end-record id: {anchorValue}, objectclass: {objectClass}, DN: {Dn}");
                        }
                    }
                    catch (Exception iex)
                    {
                        Tracer.TraceError(nameof(DataSetToCsEntryChanges), iex);
                    }
                    yield return csentry;
                }
            }
            finally
            {
                Tracer.Exit(nameof(DataSetToCsEntryChanges));
            }
        }

        public OpenImportConnectionResults OpenImportConnectionDetached(KeyedCollection<string, ConfigParameter> configParameters, Schema types, OpenImportConnectionRunStep importRunStep)
        {
            Tracer.Enter(nameof(OpenImportConnectionDetached));
            OpenImportConnectionResults result = new OpenImportConnectionResults();
            try
            {
                if (importRunStep != null) // only use when attached to FIM
                {
                    InitializeConfigParameters(configParameters);
                    ImportType = importRunStep.ImportType;
                    CustomData = ImportType == OperationType.Delta ? importRunStep.CustomData : null;
                    PageSize = importRunStep.PageSize;
                }
                Tracer.TraceInformation($"import-type {ImportType}");
                Tracer.TraceInformation($"customdata {CustomData}");
                Tracer.TraceInformation($"pagesize {PageSize}");

                Schema = types;

                importCsEntryQueue = new List<CSEntryChange>();

                methods.OpenConnection();

                if (Configuration.RunBeforeImport)
                {
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("importtype", ImportType.ToString()));
                    parameters.Add(new SqlParameter("customdata", CustomData));
                    methods.RunStoredProcedure(Configuration.ImportCommandBefore, parameters, Convert.ToInt32(Configuration.CommandTimeout));
                    parameters.Clear();
                    parameters = null;
                }
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(OpenImportConnectionDetached), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(OpenImportConnectionDetached));
            }
            return result;
        }
        public GetImportEntriesResults GetImportEntriesDetached(GetImportEntriesRunStep importRunStep)
        {
            Tracer.Enter(nameof(GetImportEntriesDetached));
            GetImportEntriesResults results = new GetImportEntriesResults();
            try
            {
                results.MoreToImport = false; // default to no more to import to prevent loops
                if (importAnchors == null)
                {
                    importAnchors = new List<object>();
                    if (ImportType == OperationType.Full)
                    {
                        importAnchors = methods.GetAllAnchors(SqlMethods.ImportType.Full).ToList();
                        Tracer.TraceInformation("got-anchors {0:n0}", importAnchors.Count);
                    }
                    else
                    {
                        importAnchors = methods.GetAllAnchors(SqlMethods.ImportType.Delta, CustomData).ToList();
                    }

                }

                if (importAnchors.Count > 0)
                {
                    List<object> sqlbatch = importAnchors.Take(PageSize).ToList();
                    if (importAnchors.Count > PageSize)
                    {
                        importAnchors.RemoveRange(0, sqlbatch.Count);
                    }
                    else
                    {
                        importAnchors.Clear();
                    }
                    Tracer.TraceInformation("reading-objects {0:n0}", sqlbatch.Count);

                    DataSet records;
                    records = methods.ReadObjects(sqlbatch);
                    importCsEntryQueue.AddRange(DataSetToCsEntryChanges(records));
                    records.Clear();
                    records.Dispose();

                    sqlbatch.Clear();
                    sqlbatch = null;
                }

                // return this batch
                Tracer.TraceInformation("sql-object-left {0:n0}", importAnchors.Count);
                Tracer.TraceInformation("cs-objects-left {0:n0}", importCsEntryQueue.Count);

                List<CSEntryChange> batch = importCsEntryQueue.Take(PageSize).ToList();
                if (importCsEntryQueue.Count > PageSize)
                {
                    importCsEntryQueue.RemoveRange(0, batch.Count);
                }
                else
                {
                    importCsEntryQueue.Clear();
                }
                results.MoreToImport = (importAnchors != null && importAnchors.Count > 0) || (importCsEntryQueue.Count > 0);
                results.CustomData = CustomData == null ? "" : CustomData;
                results.CSEntries = batch;

                Tracer.TraceInformation($"more-to-import {results.MoreToImport}");
                Tracer.TraceInformation($"custom-data '{results.CustomData}'");
                Tracer.TraceInformation("csobjects-returned {0:n0}", results.CSEntries.Count);
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(GetImportEntriesDetached), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(GetImportEntriesDetached));
            }
            return results;
        }
        public CloseImportConnectionResults CloseImportConnectionDetached(CloseImportConnectionRunStep importRunStep)
        {
            Tracer.Enter(nameof(CloseImportConnectionDetached));
            CloseImportConnectionResults result = new CloseImportConnectionResults();
            try
            {
                if (Configuration.RunAfterImport)
                {
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("importtype", ImportType.ToString()));
                    parameters.Add(new SqlParameter("customdata", CustomData));
                    methods.RunStoredProcedure(Configuration.ImportCommandAfter, parameters, Convert.ToInt32(Configuration.CommandTimeout));
                    parameters.Clear();
                    parameters = null;
                }
                methods.CloseConnection();
                if (importAnchors != null)
                {
                    importAnchors.Clear();
                    importAnchors = null;
                }
                GC.Collect();
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(CloseImportConnectionDetached), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(CloseImportConnectionDetached));
            }
            return result;
        }
    }

}
