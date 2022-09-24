using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Granfeldt
{
    public static class StringHelper
    {
        public static string XmlSerializeToString(this object objectInstance)
        {
            var serializer = new XmlSerializer(objectInstance.GetType());
            var sb = new StringBuilder();

            using (TextWriter writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, objectInstance);
            }

            return sb.ToString();
        }

        public static T XmlDeserializeFromString<T>(this string objectData)
        {
            return (T)XmlDeserializeFromString(objectData, typeof(T));
        }

        public static object XmlDeserializeFromString(this string objectData, Type type)
        {
            var serializer = new XmlSerializer(type);
            object result;

            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }

            return result;
        }
    }

    [XmlTypeAttribute(AnonymousType = true)]
    [XmlRootAttribute(ElementName = "configuration", Namespace = "", IsNullable = false)]
    public partial class SchemaConfiguration
    {
        [XmlArray(ElementName = "objectclasses")]
        [XmlArrayItemAttribute("objectclass", IsNullable = false)]
        public List<ObjectClass> ObjectClasses = new List<ObjectClass>();
    }

    [XmlTypeAttribute(AnonymousType = true)]
    public partial class ObjectClass
    {
        [XmlArray("overrides")]
        [XmlArrayItemAttribute("attribute", IsNullable = false)]
        public List<DatabaseColumn> Overrides = new List<DatabaseColumn>();

        [XmlArray("excludes")]
        [XmlArrayItem("attribute", IsNullable = false)]
        public List<DatabaseColumn> Excludes = new List<DatabaseColumn>();

        [XmlAttribute(AttributeName = "name")]
        public string Name;

        public IEnumerable<string> ExcludeList
        {
            get
            {
                foreach (DatabaseColumn dbc in Excludes)
                    yield return dbc.Name;
            }
        }
    }

    public partial class DatabaseColumn
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name;
        [XmlAttribute(AttributeName = "schematype")]
        public OverrideType SchemaType;
        [XmlAttribute(AttributeName = "type")]
        public ReservedType Type;
    }

    public enum OverrideType
    {
        Unknown,
        [XmlEnum(Name = "reference")]
        Reference,
        [XmlEnum(Name = "string")]
        String,
        [XmlEnum(Name = "binary")]
        Binary,
        [XmlEnum(Name = "boolean")]
        Boolean,
        [XmlEnum(Name = "integer")]
        Integer
    }

    public enum ReservedType
    {
        Unknown,
        [XmlEnum(Name = "anchor")]
        Anchor,
        [XmlEnum(Name = "objectclass")]
        ObjectClass,
        [XmlEnum(Name = "rowversion")]
        RowVersion,
        [XmlEnum(Name = "isdeleted")]
        IsDeleted,
        [XmlEnum(Name = "mvbackreference")]
        MVBackReference,
        [XmlEnum(Name = "dn")]
        DN
    }
}
