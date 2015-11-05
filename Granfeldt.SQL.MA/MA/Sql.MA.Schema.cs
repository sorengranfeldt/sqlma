using System;
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
		AttributeType GetAttributeOverride(ObjectClass objectClass, AttributeDefinition ad, AttributeType attrType)
		{
			DatabaseColumn ov = objectClass.Overrides.FirstOrDefault(x => x.Name.Equals(ad.Name));
			if (ov != null)
			{
				switch (ov.SchemaType)
				{
					case OverrideType.Binary:
						attrType = AttributeType.Binary;
						break;
					case OverrideType.Boolean:
						attrType = AttributeType.Boolean;
						break;
					case OverrideType.Integer:
						attrType = AttributeType.Integer;
						break;
					case OverrideType.Reference:
						attrType = AttributeType.Reference;
						break;
					case OverrideType.String:
						attrType = AttributeType.String;
						break;
					default:
						break;
				}
				Tracer.TraceInformation("setting-override-type-for name: {0}, schema-type: {1}, override-type: {2}", ad.Name, ad.AttributeType, ov.SchemaType);
			}
			return attrType;
		}
		public Schema GetSchemaDetached()
		{
			Tracer.IndentLevel = 0;
			Tracer.Enter("getschema");
			Schema schema = Schema.Create();
			try
			{
				using (SqlMethods methods = new SqlMethods())
				{
					methods.OpenConnection();
					List<string> objectClasses = new List<string>();

					Tracer.TraceInformation("objectclass-type {0}", Configuration.ObjectClassType);
					if (Configuration.ObjectClassType == ObjectClassType.Column)
					{
						objectClasses = methods.GetObjectClasses().ToList(); //since we are using yield, we need to call ToList() to get results
					}
					else
					{
						objectClasses.Add(Configuration.ObjectClass);
					}

					List<AttributeDefinition> sva = new List<AttributeDefinition>();
					sva = methods.GetSchema(Configuration.TableNameSingle).ToList(); //since we are using yield, we need to call ToList() to get results

					List<AttributeDefinition> mva = new List<AttributeDefinition>();
					if (Configuration.HasMultivalueTable)
					{
						mva = methods.GetSchema(Configuration.TableNameMulti).ToList(); //since we are using yield, we need to call ToList() to get results
					}

					foreach (string obj in objectClasses)
					{
						Tracer.TraceInformation("start-object-class {0}", obj);
						Tracer.Indent();
						SchemaType schemaObj = SchemaType.Create(obj, true);

						ObjectClass objectClass = Configuration.Schema.ObjectClasses.FirstOrDefault(c => c.Name.Equals(obj));
						Tracer.TraceInformation("found-schemaxml-information-for {0}", obj);

						// single-values
						List<string> excludeSv = Configuration.ReservedColumnNames.ToList();
						foreach (AttributeDefinition ad in sva)
						{
							AttributeType attrType = ad.AttributeType;
							if (objectClass != null)
							{
								if (objectClass.Excludes.Exists(x => x.Name.Equals(ad.Name)))
								{
									Tracer.TraceInformation("skipping-excluded-attribute {0}", ad.Name);
									continue;
								}
								attrType = GetAttributeOverride(objectClass, ad, attrType);
							}
							if (ad.Name.Equals(Configuration.AnchorColumn))
							{
								Tracer.TraceInformation("adding-anchor name: {0}, type: {1} [{2}]", ad.Name, attrType, ad.AttributeOperation);
								schemaObj.Attributes.Add(SchemaAttribute.CreateAnchorAttribute(ad.Name, attrType));
								continue;
							}
							if (excludeSv.Contains(ad.Name))
							{
								Tracer.TraceInformation("skipping-reserved-column {0}", ad.Name);
								continue;
							}
							Tracer.TraceInformation("add-singlevalue name: {0}, type: {1} [{2}]", ad.Name, attrType, ad.AttributeOperation);
							schemaObj.Attributes.Add(SchemaAttribute.CreateSingleValuedAttribute(ad.Name, attrType, ad.AttributeOperation));
						}

						// multivalues
						if (Configuration.HasMultivalueTable)
						{
							foreach (AttributeDefinition ad in mva)
							{
								AttributeType attrType = ad.AttributeType;
								if (objectClass != null)
								{
									if (objectClass.Excludes.Exists(x => x.Name.Equals(ad.Name)))
									{
										Tracer.TraceInformation("skipping-excluded-attribute {0}", ad.Name);
										continue;
									}
									attrType = GetAttributeOverride(objectClass, ad, attrType);
								}
								if (excludeSv.Contains(ad.Name))
								{
									Tracer.TraceInformation("skipping-reserved-column {0}", ad.Name);
									continue;
								}
								Tracer.TraceInformation("add-multivalue name: {0}, type: {1} [{2}]", ad.Name, attrType, ad.AttributeOperation);
								schemaObj.Attributes.Add(SchemaAttribute.CreateMultiValuedAttribute(ad.Name, attrType, ad.AttributeOperation));
							}
						}

						schema.Types.Add(schemaObj);
						Tracer.Unindent();
						Tracer.TraceInformation("end-object-class {0}", obj);
					}
				}
			}
			catch (Exception ex)
			{
				Tracer.TraceError("getschema", ex);
			}
			finally
			{
				Tracer.Exit("getschema");
			}
			return schema;
		}
		Schema IMAExtensible2GetSchema.GetSchema(KeyedCollection<string, ConfigParameter> configParameters)
		{
			InitializeConfigParameters(configParameters);
			return GetSchemaDetached();
		}
	}

}
