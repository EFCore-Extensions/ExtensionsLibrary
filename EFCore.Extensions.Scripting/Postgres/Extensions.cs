using System;

namespace EFCore.Extensions.Scripting.Postgres
{
    internal static class Extensions
    {
        public static string GetSchema(this EntityModel entity)
        {
            return entity.Schema ?? "public";
        }

        public static string PostgresDatabaseType(this PropertyModel column)
        {
            var size = string.Empty;
            if (column.Length > 0)
                size = $" ({column.Length})";

            //https://severalnines.com/database-blog/migrating-mssql-postgresql-what-you-should-know
            switch (column.DataType)
            {
                case "System.String":
                    if (column.Length == 0)
                        return "TEXT";
                    else if (column.IsXml)
                        return "TEXT";
                    else if (column.IsBlob)
                        return "TEXT";
                    else if (column.IsUnicode)
                        return "VARCHAR" + size;
                    else
                        return "VARCHAR" + size;
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
                    return "BYTEA" + size;
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
