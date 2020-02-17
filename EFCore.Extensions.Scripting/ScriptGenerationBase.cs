using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore.Extensions.Scripting
{
    public abstract class ScriptGenerationBase : IScriptGenerator
    {
        protected IDbContext _context = null;
        public DataModel Model { get; protected set; }

        public ScriptGenerationBase(IDbContext context)
        {
            if (context == null || context.MasterModel == null)
                throw new Exception("The context and model cannot be null.");

            _context = context;
            this.Model = new DataModel(context.MasterModel);
        }

        public abstract string GenerateCreateScript();
        public abstract string GenerateDiffScript(DataModel previousModel);
    }

    internal static class Extensions
    {
        public static string GetSchema(this EntityModel entity)
        {
            return entity.Schema ?? "dbo";
        }

        public static string GetDefaultText(this PropertyModel property)
        {
            switch (property.PropertyType)
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
