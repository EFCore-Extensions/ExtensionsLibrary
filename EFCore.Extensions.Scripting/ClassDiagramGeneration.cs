using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace EFCore.Extensions.Scripting
{
    public class ClassDiagramGeneration : IScriptGenerator
    {
        protected IDbContext _context = null;

        public ClassDiagramGeneration(IDbContext context)
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
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<DirectedGraph xmlns=\"http://schemas.microsoft.com/vs/2009/dgml\">");

            //Create Schemas
            var allSchemas = this.Model.EntityList.Select(x => x.Schema).Distinct().ToList();

            //Create tables
            sb.AppendLine("  <Nodes>");
            foreach (var entity in this.Model.EntityList)
            {
                var tableName = entity.GetDatabaseName();
                var schemaName = entity.Schema ?? "dbo";

                sb.AppendLine($"    <Node Id=\"{tableName}\" Group=\"Expanded\" LayoutSettings=\"List\" Label=\"{tableName}\" />");

                foreach (var property in entity.PropertyList)
                {
                    var targetName = $"{tableName}_{property.CodeName}";
                    var dataType = property.DataType.Replace("System.", string.Empty);
                    if (property.Length > 0 && property.IsString())
                        dataType += $" ({property.Length})";
                    var nullType = property.Nullable ? "NULL" : "NOT NULL";
                    sb.AppendLine($"    <Node Id=\"{targetName}\" Label=\"{property.CodeName} : {dataType} {nullType}\" />");
                }
            }
            sb.AppendLine("  </Nodes>");

            sb.AppendLine("  <Links>");
            foreach (var entity in this.Model.EntityList)
            {
                var tableName = entity.GetDatabaseName();
                var schemaName = entity.Schema ?? "dbo";

                //Group all properties into table group
                foreach (var property in entity.PropertyList)
                {
                    var targetName = $"{tableName}_{property.CodeName}";
                    sb.AppendLine($"    <Link Source=\"{tableName}\" Target=\"{targetName}\" Category=\"Contains\" />");
                }

                foreach (var relation in entity.NavigationList)
                {
                    sb.AppendLine($"    <Link Source=\"{relation.PrincipalEntityName}\" Target=\"{relation.ForeignEntityName}\" Label=\"{relation.Name}\" />");
                }
            }
            sb.AppendLine("  </Links>");

            sb.AppendLine("</DirectedGraph>");

            return sb.ToString();

        }

    }
}
