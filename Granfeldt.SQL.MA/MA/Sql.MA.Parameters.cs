﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.MetadirectoryServices;

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
			Tracer.Enter("getconfigparameters");
			try
			{
				List<ConfigParameterDefinition> configParametersDefinitions = new List<ConfigParameterDefinition>();
				switch (page)
				{
					case ConfigParameterPage.Connectivity:
						configParametersDefinitions.Add(ConfigParameterDefinition.CreateTextParameter(Configuration.Parameters.ConnectionString, Configuration.ConnectionString));
						configParametersDefinitions.Add(ConfigParameterDefinition.CreateDividerParameter());
						configParametersDefinitions.Add(ConfigParameterDefinition.CreateLabelParameter("Authentication (optional): These credentials are replace in the connection string."));
						configParametersDefinitions.Add(ConfigParameterDefinition.CreateStringParameter(Configuration.Parameters.Username, Configuration.UserName));
						configParametersDefinitions.Add(ConfigParameterDefinition.CreateEncryptedStringParameter(Configuration.Parameters.Password, ""));

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
						configParametersDefinitions.Add(ConfigParameterDefinition.CreateLabelParameter("Below you can specify SQL schema related XML configuration."));
						configParametersDefinitions.Add(ConfigParameterDefinition.CreateTextParameter(Configuration.Parameters.SchemaConfiguration, DefaultSchemaXml));
						break;
					case ConfigParameterPage.Global:
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
				Tracer.TraceError("getconfigparameters", ex);
				throw;
			}
			finally
			{
				Tracer.Exit("getconfigparameters");
			}
		}
		ParameterValidationResult IMAExtensible2GetParameters.ValidateConfigParameters(KeyedCollection<string, ConfigParameter> configParameters, ConfigParameterPage page)
		{
			Tracer.Enter("validateconfigparameters");
			try
			{
				if (page == ConfigParameterPage.Capabilities)
				{
					return new ParameterValidationResult(ParameterValidationResultCode.Success, "", "");
				}
				if (page == ConfigParameterPage.Connectivity)
				{
					//string schemaScriptFilename = Path.GetFullPath(configParameters[Constants.Parameters.SchemaScript].Value);
					//if (!File.Exists(schemaScriptFilename))
					//{
					//	return new ParameterValidationResult(ParameterValidationResultCode.Failure, string.Format("Can not find or access Schema script '{0}'. Please make sure that the FIM Synchronization Service service account can read and access this file.", schemaScriptFilename), Constants.Parameters.SchemaScript);
					//}
					return new ParameterValidationResult(ParameterValidationResultCode.Success, "", "");
				}
				if (page == ConfigParameterPage.Global)
				{
					//string importScriptFilename = Path.GetFullPath(configParameters[Constants.Parameters.ImportScript].Value);
					//if (!File.Exists(importScriptFilename))
					//{
					//	return new ParameterValidationResult(ParameterValidationResultCode.Failure, string.Format("Can not find or access Import script '{0}'. Please make sure that the FIM Synchronization Service service account can read and access this file.", importScriptFilename), Constants.Parameters.ImportScript);
					//}
					//string exportScriptFilename = Path.GetFullPath(configParameters[Constants.Parameters.ExportScript].Value);
					//if (!File.Exists(exportScriptFilename))
					//{
					//	return new ParameterValidationResult(ParameterValidationResultCode.Failure, string.Format("Can not find or access Export script '{0}'. Please make sure that the FIM Synchronization Service service account can read and access this file.", exportScriptFilename), Constants.Parameters.ExportScript);
					//}
					//string passwordManagementScriptFilename = Path.GetFullPath(configParameters[Constants.Parameters.PasswordManagementScript].Value);
					//if (!File.Exists(passwordManagementScriptFilename))
					//{
					//	return new ParameterValidationResult(ParameterValidationResultCode.Failure, string.Format("Can not find or access Password Management script '{0}'. Please make sure that the FIM Synchronization Service service account can read and access this file.", passwordManagementScriptFilename), Constants.Parameters.PasswordManagementScript);
					//}
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
				Tracer.TraceError("validateconfigparameters", ex);
				throw;
			}
			finally
			{
				Tracer.Exit("validateconfigparameters");
			}
			return new ParameterValidationResult(ParameterValidationResultCode.Success, "", "");
		}
		public void InitializeConfigParameters(System.Collections.ObjectModel.KeyedCollection<string, ConfigParameter> configParameters)
		{
			Tracer.Enter("initializeconfigparameters");
			try
			{
				//Configuration.Schema = DefaultSchemaXml.XmlDeserializeFromString<SchemaConfiguration>();

				if (configParameters != null)
				{
					foreach (ConfigParameter cp in configParameters)
					{
						Tracer.TraceInformation("{0}: '{1}'", cp.Name, cp.IsEncrypted ? "*** secret ***" : cp.Value);

						if (cp.Name.Equals(Configuration.Parameters.SchemaConfiguration)) Configuration.Schema = configParameters[cp.Name].Value.XmlDeserializeFromString<SchemaConfiguration>();

						if (cp.Name.Equals(Configuration.Parameters.Username)) Configuration.UserName = configParameters[cp.Name].Value;
						if (cp.Name.Equals(Configuration.Parameters.Password)) Configuration.Password = configParameters[cp.Name].SecureValue.ConvertToUnsecureString();

						if (cp.Name.Equals(Configuration.Parameters.ConnectionString)) Configuration.ConnectionString = configParameters[cp.Name].Value;

						if (cp.Name.Equals(Configuration.Parameters.TableNameSingleValue)) Configuration.TableNameSingle = configParameters[cp.Name].Value;
						if (cp.Name.Equals(Configuration.Parameters.TableNameMultiValue)) Configuration.TableNameMulti = configParameters[cp.Name].Value;

						if (cp.Name.Equals(Configuration.Parameters.ColumnAnchor)) Configuration.AnchorColumn = configParameters[cp.Name].Value;
						if (cp.Name.Equals(Configuration.Parameters.ColumnMVAnchorReference)) Configuration.BackReferenceColumn = configParameters[cp.Name].Value;

						if (cp.Name.Equals(Configuration.Parameters.ColumnOrValueObjectClass)) Configuration.ObjectClass = configParameters[cp.Name].Value;
						if (cp.Name.Equals(Configuration.Parameters.ColumnDelta)) Configuration.DeltaColumn = configParameters[cp.Name].Value;
						if (cp.Name.Equals(Configuration.Parameters.ColumnIsDeleted)) Configuration.DeletedColumn = configParameters[cp.Name].Value;
						if (cp.Name.Equals(Configuration.Parameters.ColumnDN)) Configuration.DNColumn = configParameters[cp.Name].Value;

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
						//if (cp.Name.Equals(Constants.Parameters.ImpersonationDomain)) impersonationUserDomain = configParameters[cp.Name].Value;
						//if (cp.Name.Equals(Constants.Parameters.ImpersonationUsername)) impersonationUsername = configParameters[cp.Name].Value;
						//if (cp.Name.Equals(Constants.Parameters.ImpersonationPassword)) impersonationUserPassword = configParameters[cp.Name].SecureValue.ConvertToUnsecureString();

						//if (cp.Name.Equals(Constants.Parameters.SchemaScript)) SchemaScript = configParameters[cp.Name].Value;
						//if (cp.Name.Equals(Constants.Parameters.ImportScript)) ImportScript = configParameters[cp.Name].Value;
						//if (cp.Name.Equals(Constants.Parameters.ExportScript)) ExportScript = configParameters[cp.Name].Value;
						//if (cp.Name.Equals(Constants.Parameters.PasswordManagementScript)) PasswordManagementScript = configParameters[cp.Name].Value;
						//if (cp.Name.Equals(Constants.Parameters.ExportSimpleObjects)) ExportSimpleObjects = configParameters[cp.Name].Value == "0" ? false : true;
						//if (cp.Name.Equals(Constants.Parameters.UsePagedImport)) UsePagedImport = configParameters[cp.Name].Value == "0" ? false : true;
					}
				}
			}
			catch (Exception ex)
			{
				Tracer.TraceError("initializeconfigparameters", ex);
				throw;
			}
			finally
			{
				Tracer.Exit("initializeconfigparameters");
			}
		}

	}

}
