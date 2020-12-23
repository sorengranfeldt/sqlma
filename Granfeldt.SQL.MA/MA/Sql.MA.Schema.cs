// november 13, 2019 | soren granfeldt
//  - fixed bug where watermark column could cause duplicate key in schema

namespace Granfeldt
{
    using Microsoft.MetadirectoryServices;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

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
                Tracer.TraceInformation($"setting-override-type-for name: {ad.Name}, schema-type: {ad.AttributeType}, override-type: {ov.SchemaType}");
            }
            return attrType;
        }
        public Schema GetSchemaDetached()
        {
            Tracer.Enter(nameof(GetSchemaDetached));
            Schema schema = Schema.Create();
            try
            {
                using (SqlMethods methods = new SqlMethods())
                {
                    methods.OpenConnection();
                    List<string> objectClasses = new List<string>();

                    Tracer.TraceInformation($"objectclass-type {Configuration.ObjectClassType}");
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
                        Tracer.TraceInformation($"start-object-class {obj}");
                        SchemaType schemaObj = SchemaType.Create(obj, true);

                        ObjectClass objectClass = Configuration.Schema.ObjectClasses.FirstOrDefault(c => c.Name.Equals(obj));
                        Tracer.TraceInformation($"found-schemaxml-information-for {obj}");

                        // single-values
                        Tracer.TraceInformation("start-detect-single-value-attributes");
                        List<string> excludeSv = Configuration.ReservedColumnNames.ToList();
                        foreach (AttributeDefinition ad in sva)
                        {
                            AttributeType attrType = ad.AttributeType;
                            if (objectClass != null)
                            {
                                if (objectClass.Excludes.Exists(x => x.Name.Equals(ad.Name)))
                                {
                                    Tracer.TraceInformation($"skipping-excluded-attribute {ad.Name}");
                                    continue;
                                }
                                attrType = GetAttributeOverride(objectClass, ad, attrType);
                            }
                            if (ad.Name.Equals(Configuration.AnchorColumn))
                            {
                                Tracer.TraceInformation($"adding-anchor name: {ad.Name}, type: {attrType} [{ad.AttributeOperation}]");
                                schemaObj.Attributes.Add(SchemaAttribute.CreateAnchorAttribute(ad.Name, attrType));
                                continue;
                            }
                            if (excludeSv.Contains(ad.Name))
                            {
                                Tracer.TraceInformation($"skipping-reserved-column {ad.Name}");
                                continue;
                            }
                            Tracer.TraceInformation($"add-singlevalue name: {ad.Name}, type: {attrType} [{ad.AttributeOperation}]");
                            schemaObj.Attributes.Add(SchemaAttribute.CreateSingleValuedAttribute(ad.Name, attrType, ad.AttributeOperation));
                        }
                        Tracer.TraceInformation("end-detect-single-value-attributes");

                        // multivalues
                        if (Configuration.HasMultivalueTable)
                        {
                            Tracer.TraceInformation("start-detect-multi-value-attributes");
                            foreach (AttributeDefinition ad in mva)
                            {
                                AttributeType attrType = ad.AttributeType;
                                if (objectClass != null)
                                {
                                    if (objectClass.Excludes.Exists(x => x.Name.Equals(ad.Name)))
                                    {
                                        Tracer.TraceInformation($"skipping-excluded-attribute {ad.Name}");
                                        continue;
                                    }
                                    attrType = GetAttributeOverride(objectClass, ad, attrType);
                                }
                                if (excludeSv.Contains(ad.Name))
                                {
                                    Tracer.TraceInformation($"skipping-reserved-column {ad.Name}");
                                    continue;
                                }
                                if (schemaObj.Attributes.Contains(ad.Name) || schemaObj.AnchorAttributes.Contains(ad.Name))
                                {
                                    Tracer.TraceInformation($"skipping-existing-column {ad.Name} (already defined in single-value-table)");
                                    continue;
                                }
                                Tracer.TraceInformation($"add-multivalue name: {ad.Name}, type: {attrType} [{ad.AttributeOperation}]");
                                schemaObj.Attributes.Add(SchemaAttribute.CreateMultiValuedAttribute(ad.Name, attrType, ad.AttributeOperation));
                            }
                            Tracer.TraceInformation("end-detect-multi-value-attributes");
                        }

                        schema.Types.Add(schemaObj);
                        Tracer.TraceInformation($"end-object-class {obj}");
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.TraceError(nameof(GetSchemaDetached), ex);
                throw;
            }
            finally
            {
                Tracer.Exit(nameof(GetSchemaDetached));
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
