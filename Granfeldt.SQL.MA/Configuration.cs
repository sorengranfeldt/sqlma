using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace Granfeldt
{
	static class StringHandling
	{
		/// <summary>
		/// Converts a SecureString to a normal string
		/// </summary>
		/// <param name="securePassword">The encrypted string to be converted</param>
		/// <returns></returns>
		public static string ConvertToUnsecureString(this SecureString securePassword)
		{
			if (securePassword == null)
				throw new ArgumentNullException("securePassword");

			IntPtr unmanagedString = IntPtr.Zero;
			try
			{
				unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
				return Marshal.PtrToStringUni(unmanagedString);
			}
			finally
			{
				Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
			}
		}
	}

	public enum ObjectClassType
	{
		Column,
		Fixed
	}
	public enum DeltaColumnType
	{
		Rowversion,
		DateTime
	}
	public static class Configuration
	{
		public static string ConnectionString = "data source=192.168.0.225;initial catalog=test;persist security info=true;user id={username};password={password};multipleactiveresultsets=true";
		public static string UserName = "sa";
		public static string Password = "";
		public static string Domain = "";

		public static string TableNameSingle = "object";
		public static string TableNameMulti = "objectmv";

        public static string AnchorColumn = "_id";
		public static string ObjectClass = "_objectclass";
		public static string DeletedColumn = "_isdeleted";
        public static string DeltaColumn = "_rowversion";
        public static string DNColumn = "_id";
		public static string BackReferenceColumn = "_refid";

		public static string ImportCommandBefore = "";
		public static string ImportCommandAfter = "";
		public static string ExportCommandBefore = "";
		public static string ExportCommandAfter = "";
		public static string ExportObjectCommandBefore = "";
		public static string ExportObjectCommandAfter = "";

		public static ObjectClassType ObjectClassType = ObjectClassType.Column;
		public static DeltaColumnType DeltaColumnType = DeltaColumnType.Rowversion;

		public static SchemaConfiguration Schema = new SchemaConfiguration();

		public static bool RunBeforeImport
		{
			get
			{
				return !string.IsNullOrEmpty(ImportCommandBefore);
			}
		}
		public static bool RunAfterImport
		{
			get
			{
				return !string.IsNullOrEmpty(ImportCommandAfter);
			}
		}
		public static bool RunBeforeExport
		{
			get
			{
				return !string.IsNullOrEmpty(ExportCommandBefore);
			}
		}
		public static bool RunAfterExport
		{
			get
			{
				return !string.IsNullOrEmpty(ExportCommandAfter);
			}
		}
		public static bool RunBeforeObjectExport
		{
			get
			{
				return !string.IsNullOrEmpty(ExportObjectCommandBefore);
			}
		}
		public static bool RunAfterObjectExport
		{
			get
			{
				return !string.IsNullOrEmpty(ExportObjectCommandAfter);
			}
		}

		public static bool HasMultivalueTable
		{
			get
			{
				return !string.IsNullOrEmpty(TableNameMulti.Trim());
			}
		}
		public static bool HasDeletedColumn
		{
			get
			{
				return !string.IsNullOrEmpty(DeletedColumn.Trim());
			}
		}
		public static bool HasDeltaColumn
		{
			get
			{
				return !string.IsNullOrEmpty(DeltaColumn.Trim());
			}
		}

		public static class Parameters
		{
			public const string ConnectionString = "Connection string";
			public const string Username = "Username";
			public const string Password = "Password";
			public const string SchemaConfiguration = "Schema Configuration";

			public const string TableNameSingleValue = "Tablename";
			public const string TableNameMultiValue = "Tablename (multivalue)";

			public const string ColumnAnchor = "Anchor column name";
			public const string ColumnDN = "DN column name";
			public const string ColumnIsDeleted = "IsDeleted column name";
			public const string ColumnMVAnchorReference = "Multivalue anchor reference column";

			public const string TypeOfDelta = "Delta column type";
			public const string ColumnDelta = "Delta column name";

			public const string TypeOfObjectClass = "Object class";
			public const string ColumnOrValueObjectClass = "Object class (name or column)";

			public const string ImportCommandBefore = "Run before import";
			public const string ImportCommandAfter = "Run after import";
			public const string ExportCommandBefore = "Run before export";
			public const string ExportCommandAfter = "Run after export";
			public const string ExportObjectCommandBefore = "Run before export object";
			public const string ExportObjectCommandAfter = "Run after export object";
		}
		public static IEnumerable<string> ReservedColumnNames
		{
			get
			{
				// we don't return DNColumnd, since we need that
				yield return AnchorColumn;
				if (ObjectClassType == ObjectClassType.Column)
				{
					yield return ObjectClass;
				}
				yield return DeletedColumn;
				if (DeltaColumnType == DeltaColumnType.Rowversion)
				{
					yield return DeltaColumn;
				}
				yield return BackReferenceColumn;
				yield return "export_password";
			}
		}
		public static string SortableDateFormat
		{
			get
			{
				return "yyyy-MM-dd HH:mm:ss.fff";
			}
		}
	}
}
