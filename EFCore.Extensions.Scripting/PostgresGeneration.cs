using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Extensions.Scripting
{
    public class PostgresGeneration : ScriptGenerationBase
    {
        public PostgresGeneration(IDbContext context)
            : base(context)
        {
        }

        public override string GenerateCreateScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine("--Idempotent Create Script");
            sb.AppendLine();

            //TODO: Create Schemas

            //Create tables
            foreach (var entity in this.Model.EntityList)
            {
                var tableName = entity.GetDatabaseName();
                var schemaName = entity.Schema ?? "public";

                sb.AppendLine($"--CREATE TABLE [{tableName}]");
                sb.AppendLine($"CREATE TABLE IF NOT EXISTS {schemaName}.\"{tableName}\" (");

                //Sort fields and generate
                var fieldList = entity.PropertyList.Where(x => x.PkIndex != null).OrderBy(x => x.PkIndex).ToList();
                fieldList.AddRange(entity.PropertyList.Where(x => x.PkIndex == null).OrderBy(x => x.GetDatabaseName()));
                foreach (var property in fieldList)
                {

                }

                sb.AppendLine(")");
                sb.AppendLine("GO");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public override string GenerateDiffScript(DataModel previousModel)
        {
            return null;
        }

        private string GetDatabaseType(PropertyModel column)
        {
            var size = " (MAX)";
            if (column.Length > 0)
                size = $" ({column.Length})";

            //https://severalnines.com/database-blog/migrating-mssql-postgresql-what-you-should-know
            switch (column.DataType)
            {
                case "System.String":
                    if (column.IsXml)
                        return "TEXT";
                    else if (column.IsBlob)
                        return "TEXT";
                    else if (column.IsUnicode)
                        return "VARCHAR" + column.Length;
                    else
                        return "VARCHAR" + column.Length;
                case "System.Int32":
                    return "INTEGER";
                case "System.Int64":
                    return "BIGINT";
                case "System.Int16":
                    return "SMALLINT";
                case "System.DateTime":
                    return "TIMESTAMP";
                case "System.Guid":
                    return "VARCHAR";
                case "System.Byte[]":
                    return "BYTEA" + column.Length;
                case "System.Boolean":
                    return "BOOLEAN";
                case "System.Char":
                    return "CHAR";
                case "System.Decimal":
                    return "DOUBLE PRECISION";
                case "System.Double":
                    return "DOUBLE PRECISION";
                case "System.Single":
                    return "DOUBLE PRECISION";
                default:
                    throw new Exception($"Unknown data type '{column.DataType}'.");
            }
        }

    }
}
