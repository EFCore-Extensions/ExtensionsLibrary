using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Datascan.EfModeling
{
    public class PostgresGeneration
    {
        protected DataModel _model = null;
        protected ModelBuilder _modelBuilder = null;

        public PostgresGeneration(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
                throw new Exception("Model cannot be null.");

            _modelBuilder = modelBuilder;
            _model = new DataModel(modelBuilder);
        }

        public virtual string Generate()
        {
            var sb = new StringBuilder();

            //Create tables
            foreach (var entity in _model.EntityList)
            {
                var tableName = entity.GetDatabaseName();
                var schemaName = entity.Schema ?? "public";

                sb.AppendLine($"--CREATE TABLE [{tableName}]");
                sb.AppendLine($"CREATE TABLE IF NOT EXISTS {schemaName}.\"{tableName}\" (");

                var firstLoop = true;
                foreach (var property in entity.PropertyList)
                {
                    if (!firstLoop) sb.AppendLine(",");
                    else firstLoop = false;
                    sb.Append("\t" + AppendColumnDefinition(property, allowDefault: true, allowIdentity: true));
                }

                sb.AppendLine(")");
                sb.AppendLine(")");
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            return sb.ToString();

        }

        private static string AppendColumnDefinition(PropertyModel column, bool allowDefault, bool allowIdentity)
        {
            return AppendColumnDefinition(column, allowDefault: allowDefault, allowIdentity: allowIdentity, forceNull: false, allowFormula: true, allowComputed: true);
        }

        private static string AppendColumnDefinition(PropertyModel column, bool allowDefault, bool allowIdentity, bool forceNull)
        {
            return AppendColumnDefinition(column, allowDefault: allowDefault, allowIdentity: allowIdentity, forceNull: forceNull, allowFormula: true, allowComputed: true);
        }

        private static string AppendColumnDefinition(PropertyModel column, bool allowDefault, bool allowIdentity, bool forceNull, bool allowFormula)
        {
            return AppendColumnDefinition(column, allowDefault: allowDefault, allowIdentity: allowIdentity, forceNull: forceNull, allowFormula: true, allowComputed: true);
        }

        private static string AppendColumnDefinition(PropertyModel column, bool allowDefault, bool allowIdentity, bool forceNull, bool allowFormula, bool allowComputed)
        {
            var sb = new StringBuilder();

            if (!allowComputed || !column.ComputedColumn)
            {
                //Add column
                sb.Append("\"" + column.DatabaseName + "\" " + column.PostgresDatabaseType());

                //Add Identity
                if (allowIdentity && (column.Identity == IdentityTypeConstants.Database))
                {
                    if (column.DataType == SqlDbType.UniqueIdentifier)
                        sb.Append(" DEFAULT uuid_generate_v4()");
                    else
                        sb.Append(" GENERATED ALWAYS AS IDENTITY");
                }

                //Add NULLable
                if (!forceNull && !column.Nullable) sb.Append(" NOT");
                sb.Append(" NULL");

                //Add default value
                var defaultValue = GetDefaultValueClause(column);
                if (allowDefault && defaultValue != null)
                    sb.Append(" " + GetDefaultValueClause(column));
            }
            else
            {
                //TODO: computed columns not supported
                sb.Append("COMPUTED COLUMNS NOT SUPPORTED!!!");
            }
            return sb.ToString();

        }

        public static string PostgresDatabaseType(this PropertyModel column)
        {
            //https://severalnines.com/database-blog/migrating-mssql-postgresql-what-you-should-know
            switch (column.DataType)
            {
                case SqlDbType.BigInt: return "BIGINT";
                case SqlDbType.Binary: return "BYTEA";
                case SqlDbType.Bit: return "BOOLEAN";
                case SqlDbType.Char: return "CHAR";
                case SqlDbType.DateTime: return "TIMESTAMP"; //precision 3
                case SqlDbType.Decimal: return "DOUBLE PRECISION";
                case SqlDbType.Float: return "DOUBLE PRECISION";
                case SqlDbType.Image: return "BYTEA";
                case SqlDbType.Int: return "INTEGER";
                case SqlDbType.Money: return "MONEY";
                case SqlDbType.NChar: return "CHAR";
                case SqlDbType.NText: return "TEXT";
                case SqlDbType.NVarChar:
                    if (column.Length == 0) return "TEXT";
                    else return "VARCHAR";
                case SqlDbType.Real: return "DOUBLE PRECISION";
                case SqlDbType.UniqueIdentifier: return "UUID";
                case SqlDbType.SmallDateTime: return "TIMESTAMP"; //precision 0
                case SqlDbType.SmallInt: return "SMALLINT";
                case SqlDbType.SmallMoney: return "MONEY";
                case SqlDbType.Text: return "TEXT";
                case SqlDbType.Timestamp: return "TIMESTAMP";
                case SqlDbType.TinyInt: return "SMALLINT";
                case SqlDbType.VarBinary: return "BYTEA";
                case SqlDbType.VarChar:
                    if (column.Length == 0) return "TEXT";
                    else return "VARCHAR";
                case SqlDbType.Variant: return "BYTEA";
                case SqlDbType.Xml: return "TEXT";
                case SqlDbType.Udt: throw new Exception("Udt not implemented");
                case SqlDbType.Structured: throw new Exception("Structured not implemented");
                case SqlDbType.Date: return "DATE";
                case SqlDbType.Time: return "TIME";
                case SqlDbType.DateTime2: return "TIMESTAMP"; //same precision as SQL
                case SqlDbType.DateTimeOffset: return "TIMESTAMP";
                default:
                    throw new Exception("Unknown data type");
            }

        }

    }
}
