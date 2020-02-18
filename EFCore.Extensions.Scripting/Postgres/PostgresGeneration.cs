using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Extensions.Scripting.Postgres
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

            //Create Schemas
            var allSchemas = this.Model.EntityList.Select(x => x.Schema).Distinct().ToList();
            foreach (var schema in allSchemas.Where(x => !string.IsNullOrEmpty(x)))
            {
                sb.AppendLine($"--CREATE SCHEMA [{schema}]");
                sb.AppendLine($"CREATE SCHEMA IF NOT EXISTS {schema};");
            }
            sb.AppendLine();

            //Create tables
            foreach (var entity in this.Model.EntityList)
            {
                var tableName = entity.GetDatabaseName();
                var schemaName = entity.Schema ?? "public";
                sb.AppendLine(GetSQLCreateTable(entity));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public override string GenerateDiffScript(DataModel previousModel, Versioning version)
        {
            if (previousModel == null)
                throw new Exception("The previous model cannot be null.");

            var sb = new StringBuilder();
            sb.AppendLine("--NOT IMPLEMENTED");
            sb.AppendLine("--Generated Upgrade For Version " + version.ToString());
            sb.AppendLine("--Generated on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            return sb.ToString();
        }

        private static string GetSQLCreateTable(EntityModel table)
        {
            try
            {
                var sb = new StringBuilder();
                var tableName = table.GetDatabaseName();

                sb.AppendLine($"--CREATE TABLE [{tableName}]");
                sb.AppendLine($"CREATE TABLE IF NOT EXISTS {table.GetSchema()}.\"{tableName}\" (");

                var firstLoop = true;
                foreach (var column in table.PropertyList)
                {
                    if (!firstLoop) sb.AppendLine(",");
                    else firstLoop = false;
                    sb.Append("\t" + AppendColumnDefinition(column));
                }

                //Emit PK
                foreach (var tableIndex in table.IndexList)
                {
                    var indexName = "PK_" + table.DatabaseName.ToUpper();
                    sb.AppendLine(",");
                    //var clustered = tableIndex.Clustered ? "CLUSTERED" : "NONCLUSTERED";
                    var clustered = string.Empty; //TEMP until figure out clustered
                    sb.AppendLine($"\tPRIMARY KEY {clustered}");
                    sb.AppendLine("\t" + "(");
                    sb.AppendLine("\t\t" + string.Join(", ", tableIndex.PropertyList.Select(x=> $"\"{x}\"")));
                    sb.AppendLine("\t" + ")");
                }

                if (!table.IndexList.Any())
                    sb.AppendLine();

                sb.AppendLine(");");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static string AppendColumnDefinition(PropertyModel column)
        {
            var sb = new StringBuilder();

            //Add column
            sb.Append("\"" + column.GetDatabaseName() + "\" " + column.PostgresDatabaseType());

            //Add Identity
            if (column.ValueGenerated)
            {
                if (column.DataType == "System.Guid")
                    sb.Append(" DEFAULT uuid_generate_v4()");
                else
                    sb.Append(" GENERATED ALWAYS AS IDENTITY");
            }

            //Add NULLable
            if (!column.Nullable) sb.Append(" NOT");
            sb.Append(" NULL");

            return sb.ToString();
        }
    }
}
