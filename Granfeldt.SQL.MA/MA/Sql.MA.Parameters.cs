﻿// december 3, 2017 :: søren granfeldt
//  - fixed issue with validation value in username parameter

using Microsoft.MetadirectoryServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Granfeldt
{
    public partial class SQLManagementAgent : IDisposable, IMAExtensible2GetCapabilities, IMAExtensible2GetSchema, IMAExtensible2GetParameters, IMAExtensible2CallImport, IMAExtensible2CallExport
    {
        public string DefaultSchemaXml = @"
<configuration>
 <objectclasses>
  <objectclass name='person'>
   <overrides>
    <attribute name='managerid' schematype='reference' />
    <attribute name='organizationalid' schematype='string' />
   </overrides>
   <excludes>
    <attribute name='export_password'/>
    <attribute name='grouptype'/>
    <attribute name='member'/>
    <attribute name='managedby'/>
   </excludes>
  </objectclass>

  <objectclass name='group'>
   <overrides>
    <attribute name='managedby' schematype='reference' />
   </overrides>
   <excludes>
    <attribute name='managerid'/>
    <attribute name='export_password'/>
    <attribute name='firstname'/>
    <attribute name='lastname'/>
    <attribute name='proxyaddresses'/>
    </excludes>
  </objectclass>
 </objectclasses>
</configuration>

";
        IList<ConfigParameterDefinition> IMAExtensible2GetParameters.GetConfigParameters(KeyedCollection<string, ConfigParameter> configParameters, ConfigParameterPage page)
        {
            Tracer.Enter(nameof(IMAExtensible2GetParameters.GetConfigParameters));
            try
            {
                List<ConfigParameterDefinition> configParametersDefinitions = new List<ConfigParameterDefinition>();
                switch (page)
                {
                    case ConfigParameterPage.Connectivity:
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateTextParameter(Configuration.Parameters.ConnectionString, Configuration.ConnectionString));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateDividerParameter());
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateLabelParameter("Authentication (optional): These credentials are replaced in the connection string."));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateDropDownParameter(Configuration.Parameters.TypeOfAuthentication, Configuration.Parameters.AuthenticationTypeSQL + "," + Configuration.Parameters.AuthenticationTypeWindows, false, Configuration.Parameters.AuthenticationTypeSQL));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.Username, "", Configuration.UserName));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateEncryptedStringParameter(Configuration.Parameters.Password, "", ""));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.Domain, "", Configuration.Domain));

                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.TableNameSingleValue, "", Configuration.TableNameSingle));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.TableNameMultiValue, "", ""));

                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateDropDownParameter(Configuration.Parameters.TypeOfObjectClass, "Column,Fixed", false, ObjectClassType.Column.ToString()));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.ColumnOrValueObjectClass, "", Configuration.ObjectClass));

                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.ColumnAnchor, "", Configuration.AnchorColumn));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.ColumnDN, "", Configuration.DNColumn));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.ColumnIsDeleted, "", ""));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.ColumnMVAnchorReference, "", ""));

                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateDropDownParameter(Configuration.Parameters.TypeOfDelta, "Rowversion,DateTime", false, DeltaColumnType.Rowversion.ToString()));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.ColumnDelta, "", ""));

                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateDividerParameter());
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.DateFormat, "", Configuration.DateFormat));

                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateDividerParameter());
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateLabelParameter("Below you can specify SQL schema related XML configuration."));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateTextParameter(Configuration.Parameters.SchemaConfiguration, DefaultSchemaXml));
                        break;
                    case ConfigParameterPage.Global:
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateLabelParameter("Optionally, you can specify the names of Stored Procedures to run before and after imports and exports. If a parameter is left blank, no action is taken for that step."));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateDividerParameter());
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateLabelParameter("Import Stored Procedures"));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.ImportCommandBefore, Configuration.ImportCommandBefore));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.ImportCommandAfter, Configuration.ImportCommandAfter));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateDividerParameter());
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateLabelParameter("Export Stored Procedures"));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.ExportCommandBefore, Configuration.ExportCommandBefore));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.ExportCommandAfter, Configuration.ExportCommandAfter));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.ExportObjectCommandBefore, Configuration.ExportObjectCommandBefore));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.ExportObjectCommandAfter, Configuration.ExportObjectCommandAfter));
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateDividerParameter());
                        //Regex validating minimal 30 and max 99999 seconds: "^([3-9][0-9]|[0-9][0-9][0-9]|[0-9][0-9][0-9][0-9]|[0-9][0-9][0-9][0-9][0-9])$"
                        configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.CommandTimeout, "^([3-9][0-9]|[0-9][0-9][0-9]|[0-9][0-9][0-9][0-9]|[0-9][0-9][0-9][0-9][0-9])$", Configuration.CommandTimeout));

                        break;
                    case ConfigParameterPage.Partition:
                        break;
                    case ConfigParameterPage.RunStep:
                        break;
                }
                return configParametersDefinitions;
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(IMAExtensible2GetParameters.GetConfigParameters), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(IMAExtensible2GetParameters.GetConfigParameters));
            }
        }
        ParameterValidationResult IMAExtensible2GetParameters.ValidateConfigParameters(KeyedCollection<string, ConfigParameter> configParameters, ConfigParameterPage page)
        {
            Tracer.Enter(nameof(IMAExtensible2GetParameters.ValidateConfigParameters));
            try
            {
                if (page == ConfigParameterPage.Capabilities)
                {
                    return new ParameterValidationResult(ParameterValidationResultCode.Success, "", "");
                }
                if (page == ConfigParameterPage.Connectivity)
                {
                    return new ParameterValidationResult(ParameterValidationResultCode.Success, "", "");
                }
                if (page == ConfigParameterPage.Global)
                {
                    return new ParameterValidationResult(ParameterValidationResultCode.Success, "", "");
                }
                if (page == ConfigParameterPage.Partition)
                {
                    return new ParameterValidationResult(ParameterValidationResultCode.Success, "", "");
                }
                if (page == ConfigParameterPage.RunStep)
                {
                    return new ParameterValidationResult(ParameterValidationResultCode.Success, "", "");
                }
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(IMAExtensible2GetParameters.ValidateConfigParameters), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(IMAExtensible2GetParameters.ValidateConfigParameters));
            }
            return new ParameterValidationResult(ParameterValidationResultCode.Success, "", "");
        }
        public void InitializeConfigParameters(System.Collections.ObjectModel.KeyedCollection<string, ConfigParameter> configParameters)
        {
            Tracer.Enter(nameof(InitializeConfigParameters));
            try
            {
                if (configParameters != null)
                {
                    foreach (ConfigParameter cp in configParameters)
                    {
                        Tracer.TraceInformation("{0}: '{1}'", cp.Name, cp.IsEncrypted ? "*** secret ***" : cp.Value);

                        if (cp.Name.Equals(Configuration.Parameters.SchemaConfiguration)) Configuration.Schema = configParameters[cp.Name].Value.XmlDeserializeFromString<SchemaConfiguration>();

                        if (cp.Name.Equals(Configuration.Parameters.TypeOfAuthentication)) Configuration.TypeOfAuthentication = configParameters[cp.Name].Value;

                        if (cp.Name.Equals(Configuration.Parameters.Username)) Configuration.UserName = configParameters[cp.Name].Value;
                        if (cp.Name.Equals(Configuration.Parameters.Password)) Configuration.Password = configParameters[cp.Name].SecureValue.ConvertToUnsecureString();
                        if (cp.Name.Equals(Configuration.Parameters.Domain)) Configuration.Domain = configParameters[cp.Name].Value;

                        if (cp.Name.Equals(Configuration.Parameters.ConnectionString)) Configuration.ConnectionString = configParameters[cp.Name].Value;

                        if (cp.Name.Equals(Configuration.Parameters.TableNameSingleValue)) Configuration.TableNameSingle = configParameters[cp.Name].Value;
                        if (cp.Name.Equals(Configuration.Parameters.TableNameMultiValue)) Configuration.TableNameMulti = configParameters[cp.Name].Value;

                        if (cp.Name.Equals(Configuration.Parameters.ColumnAnchor)) Configuration.AnchorColumn = configParameters[cp.Name].Value;
                        if (cp.Name.Equals(Configuration.Parameters.ColumnMVAnchorReference)) Configuration.BackReferenceColumn = configParameters[cp.Name].Value;

                        if (cp.Name.Equals(Configuration.Parameters.ColumnOrValueObjectClass)) Configuration.ObjectClass = configParameters[cp.Name].Value;
                        if (cp.Name.Equals(Configuration.Parameters.ColumnDelta)) Configuration.DeltaColumn = configParameters[cp.Name].Value;
                        if (cp.Name.Equals(Configuration.Parameters.ColumnIsDeleted)) Configuration.DeletedColumn = configParameters[cp.Name].Value;
                        if (cp.Name.Equals(Configuration.Parameters.ColumnDN)) Configuration.DNColumn = configParameters[cp.Name].Value;

                        if (cp.Name.Equals(Configuration.Parameters.ImportCommandBefore)) Configuration.ImportCommandBefore = configParameters[cp.Name].Value;
                        if (cp.Name.Equals(Configuration.Parameters.ImportCommandAfter)) Configuration.ImportCommandAfter = configParameters[cp.Name].Value;
                        if (cp.Name.Equals(Configuration.Parameters.ExportCommandBefore)) Configuration.ExportCommandBefore = configParameters[cp.Name].Value;
                        if (cp.Name.Equals(Configuration.Parameters.ExportCommandAfter)) Configuration.ExportCommandAfter = configParameters[cp.Name].Value;
                        if (cp.Name.Equals(Configuration.Parameters.ExportObjectCommandBefore)) Configuration.ExportObjectCommandBefore = configParameters[cp.Name].Value;
                        if (cp.Name.Equals(Configuration.Parameters.ExportObjectCommandAfter)) Configuration.ExportObjectCommandAfter = configParameters[cp.Name].Value;
                        if (cp.Name.Equals(Configuration.Parameters.CommandTimeout)) Configuration.CommandTimeout = configParameters[cp.Name].Value;

                        if (cp.Name.Equals(Configuration.Parameters.DateFormat)) Configuration.DateFormat = configParameters[cp.Name].Value;

                        if (cp.Name.Equals(Configuration.Parameters.TypeOfObjectClass))
                        {
                            switch (configParameters[cp.Name].Value)
                            {
                                case "Column":
                                    Configuration.ObjectClassType = ObjectClassType.Column;
                                    break;
                                case "Fixed":
                                    Configuration.ObjectClassType = ObjectClassType.Fixed;
                                    break;
                                default:
                                    Configuration.ObjectClassType = ObjectClassType.Column;
                                    break;
                            }
                        }

                        if (cp.Name.Equals(Configuration.Parameters.TypeOfDelta))
                        {
                            switch (configParameters[cp.Name].Value)
                            {
                                case "Rowversion":
                                    Configuration.DeltaColumnType = DeltaColumnType.Rowversion;
                                    break;
                                case "DateTime":
                                    Configuration.DeltaColumnType = DeltaColumnType.DateTime;
                                    break;
                                default:
                                    Configuration.DeltaColumnType = DeltaColumnType.Rowversion;
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(InitializeConfigParameters), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(InitializeConfigParameters));
            }
        }

    }

}
