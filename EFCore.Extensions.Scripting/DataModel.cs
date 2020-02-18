using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Extensions.Scripting
{
    public enum PropertyTypeConstants
    {
        Normal = 0,
        AuditCreatedBy = 1,
        AuditCreatedDate = 2,
        AuditModifiedBy = 3,
        AuditModifiedDate = 4,
        AuditTimestamp = 5,
        StaticDataId = 6,
        StaticDataName = 7,
        TenantId = 8,
        SoftDelete = 9,
    }

    public class DataModel
    {
        public DataModel() { }

        public DataModel(Microsoft.EntityFrameworkCore.Metadata.IMutableModel model)
            : this()
        {
            if (model == null)
                throw new Exception();

            try
            {
                var entities = model.GetEntityTypes().ToList();
                foreach (var entity in entities)
                {
                    var newEntity = new EntityModel { Name = entity.ShortName() };
                    this.EntityList.Add(newEntity);

                    newEntity.Schema = entity.GetSchema();
                    var pkIndex = 0;

                    var entityAnnotations = entity.GetAnnotations();
                    var pkClustered = (bool?)entityAnnotations.FirstOrDefault(x => x.Name == "PrimaryKeyClustered")?.Value ?? true;
                    newEntity.DatabaseName = (string)entityAnnotations.FirstOrDefault(x => x.Name == "Relational:TableName")?.Value;
                    newEntity.PrimaryKeyClustered = pkClustered;

                    foreach (var prop in entity.GetProperties())
                    {
                        var newProperty = new PropertyModel
                        {
                            CodeName = prop.Name,
                            Nullable = prop.IsNullable,
                            DataType = prop.ClrType.ToString(),
                        };
                        newEntity.PropertyList.Add(newProperty);

                        var anns = prop.GetAnnotations();
                        foreach (var a in anns)
                        {
                            if (a.Name == "MaxLength")
                                newProperty.Length = (int)a.Value;
                            else if (a.Name == "Relational:ColumnName")
                                newProperty.DatabaseName = (string)a.Value;
                            else if (a.Name == "SqlServer:ValueGenerationStrategy" && a.Value.ToString() == "IdentityColumn")
                                newProperty.ValueGenerated = true;
                            else if (a.Name == "IsUnicode")
                                newProperty.IsUnicode = (bool)a.Value;
                            else if (a.Name == "ModelId")
                                newProperty.ModelId = (string)a.Value;
                            else if (a.Name == "IsBlob")
                                newProperty.IsBlob = true;
                            else if (a.Name == "IsXml")
                                newProperty.IsXml = true;
                            else if (a.Name == "IsUnique")
                                newProperty.IsUnique = true;
                            else if (a.Name == "TypeMapping")
                            {
                                //TODO
                                //newProperty.CodeName = (a.Value as Microsoft.EntityFrameworkCore.Storage.IntTypeMapping).;
                            }
                            else if (a.Name == "Relational:DefaultValue")
                                newProperty.DefaultValue = a.Value.ToString();
                            else
                                Console.WriteLine($"Not found. {a.Name}, {a.Value}");
                        }

                        var customerAtts = prop.PropertyInfo.GetCustomAttributes(true);
                        foreach (var a in customerAtts)
                        {
                            if (a is EFCore.Extensions.Attributes.PrimaryKeyAttribute)
                                newProperty.PkIndex = pkIndex++;
                            else if (a is System.ComponentModel.DataAnnotations.KeyAttribute)
                                newProperty.PkIndex = pkIndex++;
                            else if (a is System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute)
                                newProperty.ValueGenerated = true;
                            else if (a is System.ComponentModel.DataAnnotations.StringLengthAttribute)
                                newProperty.Length = (a as System.ComponentModel.DataAnnotations.StringLengthAttribute).MaximumLength;
                            else if (a is System.ComponentModel.DataAnnotations.RequiredAttribute)
                                newProperty.Nullable = false;
                            else if (a is System.ComponentModel.DescriptionAttribute)
                                newProperty.Description = (a as System.ComponentModel.DescriptionAttribute).Description;
                            else if (a is Attributes.AuditCreatedByAttribute)
                                newProperty.PropertyType = PropertyTypeConstants.AuditCreatedBy;
                            else if (a is Attributes.AuditCreatedDateAttribute)
                                newProperty.PropertyType = PropertyTypeConstants.AuditCreatedDate;
                            else if (a is Attributes.AuditModifiedByAttribute)
                                newProperty.PropertyType = PropertyTypeConstants.AuditModifiedBy;
                            else if (a is Attributes.AuditModifiedDateAttribute)
                                newProperty.PropertyType = PropertyTypeConstants.AuditModifiedDate;
                            //else if (a is Attributes.AuditTimestampAttribute)
                            //    newProperty.PropertyType = PropertyTypeConstants.AuditTimestamp;
                            else if (a is Attributes.MaxLengthUnboundedAttribute)
                                newProperty.Length = 0;
                            else if (a is Attributes.NullableAttribute)
                                newProperty.Nullable = (a as Attributes.NullableAttribute).AllowNull;
                            else if (a is System.ComponentModel.DataAnnotations.Schema.ColumnAttribute)
                                newProperty.DatabaseName = (a as System.ComponentModel.DataAnnotations.Schema.ColumnAttribute).Name;
                            else if (a is System.ComponentModel.DataAnnotations.MaxLengthAttribute)
                                newProperty.Length = (a as System.ComponentModel.DataAnnotations.MaxLengthAttribute).Length;
                            else if (a is Attributes.BlobFieldAttribute)
                            { }
                            else if (a is Attributes.XmlFieldAttribute)
                            { }
                            else if (a is Attributes.UnicodeAttribute)
                            { }
                            else if (a is Attributes.StaticDataIdFieldAttribute)
                            {
                                newEntity.IsStaticDataMapping = true;
                                newProperty.PropertyType = PropertyTypeConstants.StaticDataId;
                            }
                            else if (a is Attributes.StaticDataNameFieldAttribute)
                                newProperty.PropertyType = PropertyTypeConstants.StaticDataName;
                            else if (a is Attributes.TenantIDFieldAttribute)
                                newProperty.PropertyType = PropertyTypeConstants.TenantId;
                            else if (a is Attributes.SoftDeleteFieldAttribute)
                                newProperty.PropertyType = PropertyTypeConstants.SoftDelete;
                            else if (a is Attributes.VersionFieldAttribute)
                            {
                                //TODO
                            }
                            else if (a is Attributes.UniqueFieldAttribute)
                            {
                                //TODO
                            }
                            else if (a is Attributes.IndexedAttribute)
                            {
                                //TODO
                            }
                            else if (a is System.ComponentModel.DataAnnotations.ConcurrencyCheckAttribute)
                                newProperty.PropertyType = PropertyTypeConstants.AuditTimestamp;
                            else if (a is EFCore.Extensions.Attributes.DefaultValueSpecialAttribute)
                            {
                                var v = (EFCore.Extensions.Attributes.DefaultValueSpecialAttribute.DefaultValueTypeConstants)(a as EFCore.Extensions.Attributes.DefaultValueSpecialAttribute).Value;
                                newProperty.DefaultValue = $"**{v.ToString()}";
                            }
                            else if (a is System.ComponentModel.DefaultValueAttribute)
                                newProperty.DefaultValue = (a as System.ComponentModel.DefaultValueAttribute).Value.ToString();
                            else
                                Console.WriteLine($"Not found. {a.ToString()}");
                        }
                    }

                    foreach (var nav in entity.GetNavigations())
                    {
                        var newNav = new NavigationModel();
                        newNav.Name = nav.ForeignKey.GetConstraintName().ToUpper();
                        newNav.IsRequired = nav.ForeignKey.IsRequired;
                        newNav.PrincipalEntityName = nav.ForeignKey.PrincipalEntityType.ShortName();
                        newNav.ForeignEntityName = nav.ForeignKey.DeclaringEntityType.ShortName();
                        newNav.DeleteBehavior = nav.ForeignKey.DeleteBehavior;
                        newEntity.NavigationList.Add(newNav);
                        for (var ii = 0; ii < nav.ForeignKey.Properties.Count; ii++)
                        {
                            newNav.PropertyList.Add(nav.ForeignKey.PrincipalKey.Properties[ii].Name, nav.ForeignKey.Properties[ii].Name);
                        }
                    }

                    foreach (var key in entity.GetKeys())
                    {
                        foreach (var prop in key.Properties)
                        {
                            var theProp = newEntity.PropertyList.First(x => x.CodeName == prop.Name);
                            if (theProp.PkIndex == null)
                            {
                                theProp.PkIndex = pkIndex++;
                            }
                        }
                    }

                    foreach (var index in entity.GetIndexes())
                    {
                        var newIndex = new IndexModel();
                        newEntity.IndexList.Add(newIndex);
                        newIndex.Name = index.GetName();
                        foreach (var prop in index.Properties)
                        {
                            newIndex.PropertyList.Add(newEntity.PropertyList.First(x => x.CodeName == prop.Name).CodeName);
                        }
                    }

                    foreach (var key in entity.GetForeignKeys())
                    {

                    }

                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public List<EntityModel> EntityList { get; set; } = new List<EntityModel>();
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"Entities={this.EntityList.Count}, Properties={this.EntityList.SelectMany(z => z.PropertyList).Count()}";
        }

    }

    public class EntityModel
    {
        public string Name { get; set; }
        public string DatabaseName { get; set; }
        public string Schema { get; set; }
        public bool PrimaryKeyClustered { get; set; } = true;
        public bool IsStaticDataMapping { get; set; }
        public List<PropertyModel> PropertyList { get; set; } = new List<PropertyModel>();
        public List<IndexModel> IndexList { get; set; } = new List<IndexModel>();
        public List<NavigationModel> NavigationList { get; set; } = new List<NavigationModel>();

        public string GetDatabaseName()
        {
            if (string.IsNullOrEmpty(this.DatabaseName)) return this.Name;
            return this.DatabaseName;
        }

        public override string ToString() => $"{this.Name}";
    }

    public class PropertyModel
    {
        public bool Nullable { get; set; }
        public string CodeName { get; set; }
        public string DatabaseName { get; set; }
        public string DataType { get; set; }
        public int? PkIndex { get; set; }
        public bool ValueGenerated { get; set; }
        public int Length { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }
        public bool IsUnicode { get; set; }
        public string ModelId { get; set; }
        public bool IsXml { get; set; }
        public bool IsUnique { get; set; }
        public bool IsBlob { get; set; }
        public PropertyTypeConstants PropertyType { get; set; } = PropertyTypeConstants.Normal;

        public string GetDatabaseName()
        {
            if (string.IsNullOrEmpty(this.DatabaseName)) return this.CodeName;
            return this.DatabaseName;
        }

        public override string ToString() => $"{this.CodeName}, {this.CodeName}";
    }

    public class IndexModel
    {
        public string Name { get; set; }
        public List<string> PropertyList { get; set; } = new List<string>();

        public override string ToString() => $"{this.Name}";
    }

    public class NavigationModel
    {
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public string PrincipalEntityName { get; set; }
        public string ForeignEntityName { get; set; }
        public Microsoft.EntityFrameworkCore.DeleteBehavior DeleteBehavior { get; set; } = Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict;

        /// <summary>
        /// Map principal key to FK
        /// </summary>
        public Dictionary<string, string> PropertyList { get; set; } = new Dictionary<string, string>();

        public override string ToString() => $"{this.Name}";
    }
}
