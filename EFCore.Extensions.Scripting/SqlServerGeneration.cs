using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Extensions.Scripting
{
    public class SqlServerGeneration : IScriptGenerator
    {
        protected IDbContext _context = null;

        public SqlServerGeneration(IDbContext context)
        {
            if (context == null || context.MasterModel == null)
                throw new Exception("The context and model cannot be null.");

            _context = context;
            this.Model = new DataModel(context.MasterModel);
        }

        public DataModel Model { get; protected set; }

        public virtual string Generate()
        {
            var sb = new StringBuilder();
            sb.AppendLine("--Idempotent Create Script");
            sb.AppendLine();

            //Create Schemas
            var allSchemas = this.Model.EntityList.Select(x => x.Schema).Distinct().ToList();
            foreach (var sch in allSchemas.Where(x => !string.IsNullOrEmpty(x)))
            {
                sb.AppendLine($"--CREATE SCHEMA [{sch}]");
                sb.AppendLine($"if not exists(select * from sys.schemas where [name] = '{sch}')");
                sb.AppendLine($"exec('create schema [{sch}];')");
            }
            sb.AppendLine();

            //Create tables
            foreach (var entity in this.Model.EntityList)
            {
                var tableName = entity.GetDatabaseName();
                var schemaName = entity.Schema ?? "dbo";

                sb.AppendLine($"--CREATE TABLE [{tableName}]");
                sb.AppendLine($"if not exists(select * from sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id where t.name = '{tableName}' and s.name = '{schemaName}')");
                sb.AppendLine($"CREATE TABLE [{schemaName}].[{tableName}] (");

                //Sort fields and generate
                var fieldList = entity.PropertyList.Where(x => x.PkIndex != null).OrderBy(x => x.PkIndex).ToList();
                fieldList.AddRange(entity.PropertyList.Where(x => x.PkIndex == null).OrderBy(x => x.GetDatabaseName()));
                foreach (var property in fieldList)
                {
                    var identityText = string.Empty;
                    if (property.ValueGenerated)
                        identityText = "IDENTITY(1,1)";
                    var nullText = "NULL";
                    if (!property.Nullable) nullText = "NOT NULL";
                    var defaultText = property.GetDefaultText();
                    if (!string.IsNullOrEmpty(defaultText))
                    {
                        var defaultName = $"DF_{entity.GetDatabaseName()}_{property.GetDatabaseName()}".ToUpper();
                        defaultText = $"CONSTRAINT [{defaultName}] {defaultText}";
                    }
                    sb.AppendLine($"[{property.GetDatabaseName()}] {GetDatabaseType(property)} {identityText} {nullText} {defaultText},");
                }

                var pkFields = entity.PropertyList.Where(x => x.PkIndex != null).OrderBy(x => x.PkIndex).ToList();
                if (pkFields.Any())
                {
                    var clustered = "CLUSTERED";
                    if (!entity.PrimaryKeyClustered) clustered = "NOT CLUSTERED";
                    sb.AppendLine($"CONSTRAINT [PK_{tableName.ToUpper()}] PRIMARY KEY {clustered}");
                    sb.AppendLine("    (");
                    var fList = new List<string>();
                    foreach (var property in pkFields)
                    {
                        fList.Add($"[{property.GetDatabaseName()}] ASC");
                    }
                    sb.AppendLine("        " + string.Join(", ", fList));
                    sb.AppendLine("    )");
                }

                sb.AppendLine(")");
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            GetSqlCreatePK(sb);
            AppendRemoveDefaults(sb);
            AppendIndexes(sb);
            AppendRelations(sb);
            AppendStaticData(sb);

            return sb.ToString();

        }

        private string GetSqlCreatePK(StringBuilder sb)
        {
            try
            {
                sb.AppendLine("--##SECTION BEGIN [RENAME PK]");
                sb.AppendLine();

                //Rename existing PK if they exist
                sb.AppendLine("--RENAME EXISTING PRIMARY KEYS IF NECESSARY");
                foreach (var entity in this.Model.EntityList.OrderBy(x => x.Name))
                {
                    var schemaName = entity.Schema ?? "dbo";
                    sb.AppendLine("DECLARE @pkfix" + entity.GetDatabaseName() + " varchar(500)");
                    sb.AppendLine("SET @pkfix" + entity.GetDatabaseName() + " = (SELECT top 1 i.name AS IndexName FROM sys.indexes AS i WHERE i.is_primary_key = 1 AND OBJECT_NAME(i.OBJECT_ID) = '" + entity.GetDatabaseName() + "')");
                    sb.AppendLine("if @pkfix" + entity.GetDatabaseName() + " <> '' and (BINARY_CHECKSUM(@pkfix" + entity.GetDatabaseName() + ") <> BINARY_CHECKSUM('PK_" + entity.GetDatabaseName().ToUpper() + "')) exec('sp_rename '''+@pkfix" + entity.GetDatabaseName() + "+''', ''PK_" + entity.GetDatabaseName().ToUpper() + "''')");
                }
                sb.AppendLine("GO");
                sb.AppendLine();

                sb.AppendLine("--##SECTION END [RENAME PK]");
                sb.AppendLine();

                sb.AppendLine("--##SECTION BEGIN [CREATE PK]");
                sb.AppendLine();

                //Create PK
                foreach (var entity in this.Model.EntityList.OrderBy(x => x.Name))
                {
                    sb.Append(GetSqlCreatePK(entity));
                    sb.AppendLine("GO");
                }

                sb.AppendLine("--##SECTION END [CREATE PK]");
                sb.AppendLine();

                sb.AppendLine("--##SECTION BEGIN [CREATE INDEXES]");
                sb.AppendLine();

                //Create Indexes
                foreach (var entity in this.Model.EntityList.OrderBy(x => x.Name))
                {
                    sb.Append(GetSqlUniqueIndexes(entity));
                }

                sb.AppendLine("--##SECTION END [CREATE INDEXES]");
                sb.AppendLine();

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string GetSqlUniqueIndexes(EntityModel entity)
        {
            var sb = new StringBuilder();
            foreach (var property in entity.PropertyList.Where(x => x.IsUnique))
            {
                sb.AppendLine($"--Unique index for column [{entity.Name}].[{property.GetDatabaseName()}]");
                var indexName = $"IX_UQ_{entity.Name}_{property.GetDatabaseName()}".ToUpper();
                sb.AppendLine($"if not exists(select * from INFORMATION_SCHEMA.TABLE_CONSTRAINTS where CONSTRAINT_NAME = '{indexName}')");
                sb.AppendLine($"ALTER TABLE [{entity.Name}] ADD CONSTRAINT [{indexName}] UNIQUE ([{property.GetDatabaseName()}]);");
                sb.AppendLine("GO");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private string GetSqlCreatePK(EntityModel entity)
        {
            try
            {
                var sb = new StringBuilder();
                if (entity.PropertyList.Any(x => x.PkIndex != null))
                {
                    var fieldSet = GetSQLPKFields(entity);
                    if (!string.IsNullOrEmpty(fieldSet))
                    {
                        var indexName = $"PK_{entity.GetDatabaseName().ToUpper()}";
                        sb.AppendLine($"--PRIMARY KEY FOR TABLE [{entity.GetDatabaseName()}]");
                        sb.AppendLine($"if not exists(select * from sys.objects where name = '{indexName}' and type = 'PK')");
                        sb.AppendLine($"ALTER TABLE [{entity.GetSchema()}].[{entity.GetDatabaseName()}] WITH NOCHECK ADD ");
                        sb.AppendLine($"CONSTRAINT [{indexName}] PRIMARY KEY " + (entity.PrimaryKeyClustered ? "CLUSTERED" : "NONCLUSTERED"));
                        sb.AppendLine("(");
                        sb.AppendLine("\t" + fieldSet);
                        sb.Append(")");
                        sb.AppendLine();
                    }
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string GetSQLPKFields(EntityModel entity)
        {
            try
            {
                var list = new List<string>();
                foreach (var indexColumn in entity.PropertyList.Where(x => x.PkIndex != null).OrderBy(x => x.PkIndex))
                    list.Add($"[{indexColumn.GetDatabaseName()}]");
                return string.Join(",", list);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void AppendRemoveDefaults(StringBuilder sb)
        {
            sb.AppendLine("--##SECTION BEGIN [REMOVE DEFAULTS]");
            sb.AppendLine();
            foreach (var entity in this.Model.EntityList.OrderBy(x => x.Name))
            {
                //Add Defaults
                var tempsb = new StringBuilder();
                foreach (var column in entity.PropertyList)
                {
                    var defaultText = GetSqlDropColumnDefault(entity, column);
                    if (!string.IsNullOrEmpty(defaultText)) tempsb.Append(defaultText);
                }

                if (tempsb.ToString() != string.Empty)
                {
                    sb.AppendLine($"--BEGIN DEFAULTS FOR TABLE [{entity.GetDatabaseName()}]");
                    sb.AppendLine("DECLARE @defaultName varchar(max)");
                    sb.Append(tempsb.ToString());
                    sb.AppendLine($"--END DEFAULTS FOR TABLE [{entity.GetDatabaseName()}]");
                    sb.AppendLine("GO");
                    sb.AppendLine();
                }

            }
            sb.AppendLine("--##SECTION END [REMOVE DEFAULTS]");
            sb.AppendLine();
        }

        private HashSet<string> _usedRelations = new HashSet<string>();
        private void AppendRelations(StringBuilder sb)
        {
            sb.AppendLine("--##SECTION BEGIN [ADD RELATIONS]");
            sb.AppendLine();

            foreach (var entity in this.Model.EntityList)
            {
                foreach (var relation in entity.NavigationList)
                {
                    if (!_usedRelations.Contains(relation.Name))
                    {
                        _usedRelations.Add(relation.Name);
                        var pkTable = this.Model.EntityList.First(x => x.Name == relation.PrincipalEntityName);
                        var fkTable = this.Model.EntityList.First(x => x.Name == relation.ForeignEntityName);
                        var sourceList = new List<string>();
                        var targetList = new List<string>();
                        foreach (var column in relation.PropertyList)
                        {
                            sourceList.Add("[" + column.Key + "]");
                            targetList.Add("[" + column.Value + "]");
                        }
                        sb.AppendLine($"--ADD FOREIGN KEY FOR [{pkTable.GetDatabaseName()}] => [{fkTable.GetDatabaseName()}]");
                        sb.AppendLine("if not exists(select * from sys.objects where name = '" + relation.Name + "' and type = 'F')");
                        sb.AppendLine($"ALTER TABLE [{fkTable.GetSchema()}].[{fkTable.GetDatabaseName()}]");
                        sb.AppendLine($"ADD CONSTRAINT [{relation.Name}] FOREIGN KEY ({string.Join(", ", targetList)}) REFERENCES [{pkTable.GetSchema()}].[{pkTable.GetDatabaseName()}] ({string.Join(", ", sourceList)});");
                        sb.AppendLine("GO");
                        sb.AppendLine();
                    }
                }
            }

            sb.AppendLine("--##SECTION END [ADD RELATIONS]");
            sb.AppendLine();
        }

        private void AppendIndexes(StringBuilder sb)
        {
            sb.AppendLine("--##SECTION BEGIN [ADD INDEXES]");
            sb.AppendLine();

            foreach (var entity in this.Model.EntityList)
            {
                foreach(var index in entity.IndexList)
                {
                    sb.AppendLine($"--CREATE INDEX FOR [{entity.GetDatabaseName()}]");
                    sb.AppendLine($"if not exists (select * from sys.indexes where [name] = '{index.Name.ToUpper()}')");
                    sb.AppendLine($"CREATE NONCLUSTERED INDEX [{index.Name.ToUpper()}]");
                    sb.Append($"ON [{entity.GetSchema()}].[{entity.GetDatabaseName()}] (");
                    var cList = new List<string>();
                    foreach(var columnName in index.PropertyList)
                    {
                        cList.Add($"[{columnName}]");
                    }
                    sb.Append(string.Join(", ", cList));
                    sb.AppendLine(");");
                    sb.AppendLine("GO");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("--##SECTION END [ADD INDEXES]");
            sb.AppendLine();
        }

        private void AppendStaticData(StringBuilder sb)
        {
            sb.AppendLine("--##SECTION BEGIN [STATIC DATA]");
            sb.AppendLine();

            foreach (var entity in this.Model.EntityList.Where(x => x.IsStaticDataMapping).ToList())
            {
                var fieldId = entity.PropertyList.First(x => x.PropertyType == PropertyTypeConstants.StaticDataId);
                var fieldName = entity.PropertyList.First(x => x.PropertyType == PropertyTypeConstants.StaticDataName);

                var enumName = $"{entity.Name}Constants";
                var enumObjects = _context.GetType().Assembly.GetTypes().Where(x => x.Name.Contains(enumName)).ToList();
                if (enumObjects.Count != 1)
                    throw new Exception($"There must be exactly one {enumName} enumeration defined.");

                //TODO: Verify that this is an identity field for 'IDENTITY_INSERT'

                sb.AppendLine($"--INSERT STATIC DATA FOR [{entity.GetDatabaseName()}]");
                sb.AppendLine($"SET IDENTITY_INSERT [{entity.GetSchema()}].[{entity.GetDatabaseName()}] ON");

                var enumType = enumObjects.First();
                var enumValues = Enum.GetValues(enumType);
                foreach(var ee in enumValues)
                {
                    sb.AppendLine($"if not exists (select * from [{entity.GetSchema()}].[{entity.GetDatabaseName()}] where [{fieldId.GetDatabaseName()}] = {(int)ee})");
                    sb.AppendLine($"insert into [{entity.GetSchema()}].[{entity.GetDatabaseName()}] ([{fieldId.GetDatabaseName()}], [{fieldName.GetDatabaseName()}]) values ({(int)ee}, '{ee.ToString()}')");
                }
                sb.AppendLine($"SET IDENTITY_INSERT [{entity.GetSchema()}].[{entity.GetDatabaseName()}] OFF");
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            sb.AppendLine("--##SECTION END [STATIC DATA]");
            sb.AppendLine();
        }

        private string GetSqlDropColumnDefault(EntityModel entity, PropertyModel column, bool upgradeScript = false)
        {
            return "";
            var sb = new StringBuilder();
            sb.AppendLine($"--DROP CONSTRAINT FOR '[{entity.GetDatabaseName()}].[{column.GetDatabaseName()}]'");
            if (upgradeScript)
                sb.AppendLine("DECLARE @defaultName varchar(max)");
            sb.AppendLine("SET @defaultName = (SELECT d.name FROM sys.columns c inner join sys.default_constraints d on c.column_id = d.parent_column_id and c.object_id = d.parent_object_id inner join sys.objects o on d.parent_object_id = o.object_id where o.name = '" + entity.GetDatabaseName() + "' and c.name = '" + column.GetDatabaseName() + "')");
            sb.AppendLine("if @defaultName IS NOT NULL");
            sb.AppendLine($"exec('ALTER TABLE [{entity.GetSchema()}].[{entity.GetDatabaseName()}] DROP CONSTRAINT ' + @defaultName)");
            if (upgradeScript)
                sb.AppendLine("GO");
            return sb.ToString();
        }

        private string GetDatabaseType(PropertyModel property)
        {
            var size = " (MAX)";
            if (property.Length > 0)
                size = $" ({property.Length})";

            switch (property.PropertyType)
            {
                case PropertyTypeConstants.AuditTimestamp:
                    return "[ROWVERSION]";
            }

            switch (property.DataType)
            {
                case "System.String":
                    if (property.IsXml)
                        return "[xml]";
                    else if (property.IsBlob)
                    {
                        if (property.IsUnicode)
                            return "[ntext]";
                        else
                            return "[text]";
                    }
                    else if (property.IsUnicode)
                        return "[nvarchar]" + size;
                    else
                        return "[varchar]" + size;
                case "System.Int32":
                    return "[int]";
                case "System.Int64":
                    return "[bigint]";
                case "System.Int16":
                    return "[smallint]";
                case "System.DateTime":
                    return "[DateTime2] (7)";
                case "System.Guid":
                    return "[UniqueIdentifier]";
                case "System.Byte[]":
                    return "[varbinary]" + size;
                case "System.Boolean":
                    return "[bit]";
                case "System.Char":
                    return "[char] (1)";
                case "System.Decimal":
                    return "[decimal] (18,2)";
                case "System.Double":
                    return "[float]";
                case "System.Single":
                    return "[real]";
                default:
                    throw new Exception($"Unknown data type '{property.DataType}'.");
            }
        }
    }

    internal static class Extensions
    {
        public static string GetSchema(this EntityModel entity)
        {
            return entity.Schema ?? "dbo";
        }

        public static string GetDefaultText(this PropertyModel property)
        {
            switch(property.PropertyType)
            {
                case PropertyTypeConstants.AuditCreatedDate:
                    return "DEFAULT (getdate())";
                case PropertyTypeConstants.AuditModifiedDate:
                    return "DEFAULT (getdate())";
                case PropertyTypeConstants.AuditTimestamp:
                    return "";
            }

            var defaultText = string.Empty;
            if (property.DefaultValue != null)
            {
                if (property.DefaultValue?.StartsWith("**") == true)
                {
                    var v = property.DefaultValue.Replace("**", string.Empty);
                    if (property.IsDate() && v == EFCore.Extensions.Attributes.DefaultValueSpecialAttribute.DefaultValueTypeConstants.CurrentTime.ToString())
                        defaultText = $"DEFAULT (GETDATE())";
                    else if (property.IsDate() && v == EFCore.Extensions.Attributes.DefaultValueSpecialAttribute.DefaultValueTypeConstants.CurrentTimeUTC.ToString())
                        defaultText = $"DEFAULT (GETUTCDATE())";
                    else if (property.IsString() && v == EFCore.Extensions.Attributes.DefaultValueSpecialAttribute.DefaultValueTypeConstants.AppName.ToString())
                        defaultText = $"DEFAULT (APP_NAME())";
                    else if (property.IsString() && v == EFCore.Extensions.Attributes.DefaultValueSpecialAttribute.DefaultValueTypeConstants.DbUser.ToString())
                        defaultText = $"DEFAULT (system_user)";
                    else
                        throw new Exception("Unknown default");
                }
                else if (property.IsBoolean())
                    defaultText = "DEFAULT (" + (property.DefaultValue?.ToLower() == "false" ? "0" : "1") + ")";
                else if (property.IsString())
                    defaultText = $"DEFAULT ('{property.DefaultValue}')";
                else if (property.IsNumeric())
                    defaultText = $"DEFAULT ({property.DefaultValue})";
                else
                    throw new Exception("Unknown default");
            }
            return defaultText;
        }

        public static bool IsBoolean(this PropertyModel property)
        {
            return property.DataType == "System.Boolean";
        }

        public static bool IsString(this PropertyModel property)
        {
            return property.DataType == "System.String";
        }

        public static bool IsInt(this PropertyModel property)
        {
            return property.DataType == "System.Int16" ||
                property.DataType == "System.Int32" ||
                property.DataType == "System.Int64";
        }

        public static bool IsNumeric(this PropertyModel property)
        {
            return property.DataType == "System.Int16" ||
                property.DataType == "System.Int32" ||
                property.DataType == "System.Int64" ||
                property.DataType == "System.Decimal" ||
                property.DataType == "System.Double" ||
                property.DataType == "System.Single";
        }

        public static bool IsDate(this PropertyModel property)
        {
            return property.DataType == "System.DateTime";
        }
    }

}
