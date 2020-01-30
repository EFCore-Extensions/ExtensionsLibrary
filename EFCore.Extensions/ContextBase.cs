using System;
using System.Linq;
using System.Configuration;
using EFCore.Extensions.Attributes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace EFCore.Extensions
{
    public interface IDbContext
    {
        Microsoft.EntityFrameworkCore.Metadata.IMutableModel MasterModel { get; }
    }

    public partial class ContextBase : DbContext, IDbContext
    {
        protected string _connectionString = null;
        protected Random _rnd = new Random();

        /// <summary>
        /// Used for model cache management under certain conditions
        /// </summary>
        protected internal string ModelCacheKey { get; }

        public ContextBase()
            : base()
        {
            _connectionString = ConfigurationManager.ConnectionStrings[this.GetType().Name]?.ConnectionString;
            this.OnContextCreated();
        }

        public ContextBase(string connectionString)
            : base()
        {
            _connectionString = connectionString;
            this.OnContextCreated();
        }

        public ContextBase(IContextStartup startup)
            : base()
        {
            if (startup == null)
                throw new Exception("Startup cannot be null");

            if (startup is TenantContextStartup)
                this.ModelCacheKey = (startup as TenantContextStartup).TenantId;

            _connectionString = ConfigurationManager.ConnectionStrings[this.GetType().Name]?.ConnectionString;
            this.ContextStartup = startup;
            this.OnContextCreated();
        }

        public ContextBase(IContextStartup startup, string connectionString)
            : base()
        {
            if (startup == null)
                throw new Exception("Startup cannot be null");

            if (startup is TenantContextStartup)
                this.ModelCacheKey = (startup as TenantContextStartup).TenantId;

            _connectionString = connectionString;
            this.ContextStartup = startup;
            this.OnContextCreated();
        }

        public ContextBase(DbContextOptions<ContextBase> options)
            : base(options)
        {
            this.OnContextCreated();
        }

        partial void OnContextCreated();
        partial void OnBeforeSaveChanges(ref bool cancel);
        partial void OnAfterSaveChanges();

        public IContextStartup ContextStartup { get; protected set; } = new ContextStartup(null);

        /// <summary>
        /// This model is used for script generation. There is no need to expose it publically.
        /// </summary>
        protected static Microsoft.EntityFrameworkCore.Metadata.IMutableModel MasterModel { get; set; }

        IMutableModel IDbContext.MasterModel => MasterModel;

        public override int SaveChanges()
        {
            var cancel = false;
            this.OnBeforeSaveChanges(ref cancel);
            if (cancel) return 0;

            var markedTime = System.DateTime.Now;

            var tenantId = (this.ContextStartup as TenantContextStartup)?.TenantId;

            //Add list audits
            var addedList = this.ChangeTracker.Entries().Where(x => x.State == EntityState.Added);
            foreach (var item in addedList)
            {
                //Audit
                SetPropertyByAttribute(item.Entity, typeof(AuditCreatedByAttribute), this.ContextStartup.Modifer);
                SetPropertyByAttribute(item.Entity, typeof(AuditCreatedDateAttribute), markedTime);
                SetPropertyByAttribute(item.Entity, typeof(AuditModifiedByAttribute), this.ContextStartup.Modifer);
                SetPropertyByAttribute(item.Entity, typeof(AuditModifiedDateAttribute), markedTime);
                SetPropertyByAttribute(item.Entity, typeof(VersionFieldAttribute), 1);
                SetPropertyConcurrency(item.Entity, typeof(ConcurrencyCheckAttribute));

                //Only set the TenantID on create. It never changes.
                if (item.Entity is ITenantEntity)
                {
                    if (!SetPropertyByAttribute(item.Entity, typeof(TenantIDFieldAttribute), tenantId))
                        SetPropertyByName(item.Entity, "TenantId", tenantId);
                }
            }

            //Modify list audits
            var modifiedList = this.ChangeTracker.Entries().Where(x => x.State == EntityState.Modified);
            foreach (var item in modifiedList)
            {
                //Audit
                SetPropertyByAttribute(item.Entity, typeof(AuditModifiedByAttribute), this.ContextStartup.Modifer);
                SetPropertyByAttribute(item.Entity, typeof(AuditModifiedDateAttribute), markedTime);
                var versionValue = GetPropertyByAttribute<int>(item.Entity, typeof(VersionFieldAttribute));
                SetPropertyByAttribute(item.Entity, typeof(VersionFieldAttribute), ++versionValue);
                SetPropertyConcurrency(item.Entity, typeof(ConcurrencyCheckAttribute));
            }

            var count = base.SaveChanges();

            //Call this partial method in case the client wants to perform any custom changes
            this.OnAfterSaveChanges();

            return count;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            //This will create one model cache per key
            optionsBuilder.ReplaceService<Microsoft.EntityFrameworkCore.Infrastructure.IModelCacheKeyFactory, ModelCacheKeyFactory>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Load all configurations from the client
            foreach (var a in this.GetAssemblyHierarchy())
                modelBuilder.ApplyConfigurationsFromAssembly(a);

            //Perform all framework mappings
            this.CreateMappings(modelBuilder);

            //Set the static model as it is only loaded once
            MasterModel = modelBuilder.Model;
        }

        protected IReadOnlyList<System.Reflection.Assembly> GetAssemblyHierarchy()
        {
            var assemblyList = new List<System.Reflection.Assembly>();
            var contextType = this.GetType();
            do
            {
                assemblyList.Add(contextType.Assembly);
                contextType = contextType.BaseType;
            } while (!(contextType == typeof(ContextBase)));
            return assemblyList;
        }

        protected IReadOnlyList<System.Type> GetAllTypeHierarchy()
        {
            var assemblyList = GetAssemblyHierarchy();
            var allTableTypes = new List<System.Type>();
            foreach (var aa in assemblyList)
            {
                allTableTypes.AddRange(aa.GetTypes());
            }
            return allTableTypes;
        }

        protected IReadOnlyList<System.Type> GetEntityTypeHierarchy()
        {
            return GetAllTypeHierarchy().Where(mytype => !mytype.IsAbstract && mytype.GetInterfaces().Contains(typeof(IEntity))).ToList();
        }

        private void CreateMappings(ModelBuilder modelBuilder)
        {
            try
            {
                //TODO: Map non-default schemas
                //https://stackoverflow.com/questions/39499470/dynamically-changing-schema-in-entity-framework-core
                string schemaName = null;

                #region Map all tables
                foreach (var tableType in this.GetEntityTypeHierarchy())
                {
                    var tableAttributes = tableType.GetCustomAttributes(true);
                    var tableAttr = tableAttributes.Where(x => x.GetType() == typeof(System.ComponentModel.DataAnnotations.Schema.TableAttribute)).FirstOrDefault() as System.ComponentModel.DataAnnotations.Schema.TableAttribute;
                    var isTenant = tableType.GetInterfaces().Any(x => x == typeof(ITenantEntity));
                    var isSoftDelete = tableType.GetInterfaces().Any(x => x == typeof(ISoftDeleted));
                    var auditAddedImplicitLengths = 0;
                    var skipStringLengthChecks = 0;

                    //Verify that this object type is on the context
                    var modelEntity = modelBuilder.Model.FindEntityType(tableType.FullName);
                    if (modelEntity == null)
                        throw new Exception($"The entity {tableType.FullName} was not found.");

                    //Remove all convention based ValueGenerated settings as these will be set explicitly
                    foreach (var column in modelEntity.GetProperties())
                        modelBuilder.Entity(tableType.FullName).Property(column.Name).ValueGeneratedNever();

                    #region Get string length attributes
                    var stringLengths = tableType.Props(false).Where(x => !x.NotMapped() && x.GetAttr<StringLengthAttribute>() != null)
                        .Select(x => x.GetAttrWithProp<StringLengthAttribute>())
                        .ToList();
                    var stringUnboundedLengths = tableType.Props(false).Where(x => !x.NotMapped() && x.GetAttr<StringLengthUnboundedAttribute>() != null)
                        .Select(x => x.GetAttrWithProp<StringLengthUnboundedAttribute>())
                        .ToList();
                    #endregion

                    #region Handle the Tenant mappings
                    if (isTenant)
                    {
                        //Verify 
                        var startup = this.ContextStartup as TenantContextStartup;
                        if (startup == null)
                            throw new Exception("A tenant context must be created with a TenantContextStartup object.");

                        //map to Tenant table
                        var name = tableAttr?.Name ?? tableType.Name;
                        modelBuilder.Entity(tableType.FullName).ToTable(name, schemaName);
                        var userAttr = tableType.Props(false).FirstOrDefault(x => x.GetCustomAttributes(true).Any(z => z.GetType() == typeof(TenantIDFieldAttribute)));
                        if (userAttr == null)
                            userAttr = tableType.Props(false).FirstOrDefault(x => x.Name == "TenantId");

                        if (userAttr == null)
                        {
                            throw new Exception($"The entity {tableType.Name} is marked as Tenant but there is no TenantId property defined.");
                        }
                        else
                        {
                            //set the TenantId field
                            modelBuilder.Entity(tableType.FullName).Property(userAttr.Name).IsRequired();
                            if (!stringUnboundedLengths.Any(x => x.Item2.Name == userAttr.Name) && !stringUnboundedLengths.Any(x => x.Item2.Name == userAttr.Name))
                            {
                                modelBuilder.Entity(tableType.FullName).Property(userAttr.Name).HasMaxLength(50);
                                auditAddedImplicitLengths++;
                            }

                            //https://haacked.com/archive/2019/07/29/query-filter-by-interface/
                            ReflectionHelpers.SetEntityQueryFilter<ITenantEntity>(modelBuilder, tableType, p => p.TenantId == startup.TenantId);

                        }
                    }
                    #endregion

                    #region Soft Delete

                    if (isSoftDelete)
                    {
                        //map to Soft Delete table
                        var name = tableAttr?.Name ?? tableType.Name;
                        modelBuilder.Entity(tableType.FullName).ToTable(name, schemaName);
                        var userAttr = tableType.Props(false).FirstOrDefault(x => x.GetCustomAttributes(true).Any(z => z.GetType() == typeof(SoftDeleteFieldAttribute)));
                        if (userAttr == null)
                            userAttr = tableType.Props(false).FirstOrDefault(x => x.Name == "IsDeleted");

                        if (userAttr == null)
                        {
                            throw new Exception($"The entity {tableType.Name} is marked as SoftDelete but there is no IsDeleted property defined.");
                        }
                        else
                        {
                            //Set the IsDeleted field
                            modelBuilder.Entity(tableType.FullName).Property(userAttr.Name).IsRequired().HasDefaultValue(false);

                            //https://haacked.com/archive/2019/07/29/query-filter-by-interface/
                            ReflectionHelpers.SetEntityQueryFilter<ISoftDeleted>(modelBuilder, tableType, p => p.IsDeleted == false);
                        }
                    }

                    #endregion

                    #region If field is marked DatabaseGenerated then error if has public setter

                    var databaseGeneratedInfo = tableType.Props(false).Where(x => !x.NotMapped() && x.GetAttr<DatabaseGeneratedAttribute>() != null)
                            .Select(x => x.GetAttrWithProp<DatabaseGeneratedAttribute>())
                            .ToList();

                    //Check for non-public setters
                    foreach(var item in databaseGeneratedInfo)
                    {
                        if (item.Item2.SetMethod.IsPublic)
                        {
                            throw new Exception($"The property {tableType.Name}.{item.Item2.Name} is marked as database generated. The setter method must not be marked public.");
                        }
                        //Mark as DB generated
                        modelBuilder.Entity(tableType.FullName).Property(item.Item2.Name).ValueGeneratedOnAdd();
                    }

                    #endregion

                    #region Handled Static Data Tables

                    var staticDataAttr = tableAttributes.Where(x => x.GetType() == typeof(StaticDataAttribute)).FirstOrDefault() as StaticDataAttribute;
                    if (staticDataAttr != null)
                    {
                        var idProp = tableType.Props(false).FirstOrDefault(x => x.GetCustomAttributes(true).Any(z => z.GetType() == typeof(StaticDataIdFieldAttribute)));
                        var nameProp = tableType.Props(false).FirstOrDefault(x => x.GetCustomAttributes(true).Any(z => z.GetType() == typeof(StaticDataNameFieldAttribute)));
                        if (idProp == null || nameProp == null)
                            throw new Exception($"Static data tables must be marked with the {typeof(StaticDataAttribute).Name}, {typeof(StaticDataIdFieldAttribute).Name}, {typeof(StaticDataNameFieldAttribute).Name} set.");

                        var enumName = $"{tableType.Name}Constants";
                        var enumExists = this.GetAllTypeHierarchy().Where(x => x.IsEnum).Any(x => x.Name == enumName);
                        if (!enumExists)
                            throw new Exception($"The static data entity {tableType.Name} requires that a backing Enum named {enumName} exist.");

                        modelBuilder.Entity(tableType.FullName).Property(idProp.Name).ValueGeneratedOnAdd().IsRequired();
                        modelBuilder.Entity(tableType.FullName).Property(nameProp.Name).IsRequired();
                    }

                    #endregion

                    #region Versioning

                    var versioningInfos = tableType.Props(false).Where(x => !x.NotMapped() && x.GetAttr<VersionFieldAttribute>() != null)
                            .Select(x => x.GetAttrWithProp<VersionFieldAttribute>())
                            .ToList();

                    if (versioningInfos.Count > 1)
                        throw new Exception($"The entity {tableType.Name} has more than one versioning property.");

                    foreach (var item in versioningInfos)
                    {
                        if (item.Item2.PropertyType.Name != "Int32" && item.Item2.PropertyType.Name != "Int64")
                            throw new Exception($"The versioning property {tableType.Name}.{item.Item2.Name} must be of type 'int' or 'long'.");
                    }

                    #endregion

                    #region Unicode

                    var unicodeInfos = tableType.Props(false).Where(x => !x.NotMapped() && x.GetAttr<UnicodeAttribute>() != null)
                            .Select(x => x.GetAttrWithProp<UnicodeAttribute>())
                            .ToList();

                    foreach (var item in unicodeInfos)
                    {
                        modelBuilder.Entity(tableType.FullName).Property(item.Item2.Name).HasAnnotation("IsUnicode", item.Item1.IsUnicode);
                    }

                    #endregion

                    #region Blob

                    var blobInfos = tableType.Props(false).Where(x => !x.NotMapped() && x.GetAttr<BlobFieldAttribute>() != null)
                            .Select(x => x.GetAttrWithProp<BlobFieldAttribute>())
                            .ToList();

                    foreach (var item in blobInfos)
                    {
                        modelBuilder.Entity(tableType.FullName).Property(item.Item2.Name).HasAnnotation("IsBlob", true);
                        skipStringLengthChecks++;
                    }

                    #endregion

                    #region Blob

                    var concurrencyInfos = tableType.Props(false).Where(x => !x.NotMapped() && x.GetAttr<ConcurrencyCheckAttribute>() != null)
                            .Select(x => x.GetAttrWithProp<ConcurrencyCheckAttribute>())
                            .ToList();

                    foreach (var item in concurrencyInfos)
                    {
                        modelBuilder.Entity(tableType.FullName).Property(item.Item2.Name).IsRequired();
                    }

                    #endregion

                    #region XmlField

                    var xmlFieldInfos = tableType.Props(false).Where(x => !x.NotMapped() && x.GetAttr<XmlFieldAttribute>() != null)
                            .Select(x => x.GetAttrWithProp<XmlFieldAttribute>())
                            .ToList();

                    foreach (var item in xmlFieldInfos)
                    {
                        modelBuilder.Entity(tableType.FullName).Property(item.Item2.Name).HasAnnotation("IsXml", true);
                        skipStringLengthChecks++;
                    }

                    #endregion

                    #region XmlField

                    var uniqueInfos = tableType.Props(false).Where(x => !x.NotMapped() && x.GetAttr<UniqueFieldAttribute>() != null)
                            .Select(x => x.GetAttrWithProp<UniqueFieldAttribute>())
                            .ToList();

                    foreach (var item in uniqueInfos)
                    {
                        modelBuilder.Entity(tableType.FullName).Property(item.Item2.Name).HasAnnotation("IsUnique", true);
                    }

                    #endregion

                    #region Verify keys
                    var hasKey = false;
                    var keySet1 = tableType.Props().Where(x => !x.NotMapped() && x.GetAttr<KeyAttribute>() != null)
                        .Select(x => x.GetAttrWithProp<KeyAttribute>())
                        .ToList();
                    if (keySet1.Any())
                    {
                        if (keySet1.Count > 1)
                            throw new Exception($"Error on '{tableType.Name}'. Key attribute can only be defined once per entity.");

                        //This is a default Key attribute
                        hasKey = true;
                        var key = keySet1.First();
                        modelBuilder.Entity(tableType.FullName).Property(key.Item2.PropertyType, key.Item2.Name);
                        modelBuilder.Entity(tableType.FullName).HasKey(key.Item2.Name);
                    }
                    else
                    {
                        //Look for PrimaryKeyAttribute extension
                        var keySet2 = tableType.Props().Where(x => !x.NotMapped() && x.GetAttr<PrimaryKeyAttribute>() != null)
                            .Select(x => x.GetAttrWithProp<PrimaryKeyAttribute>())
                            .ToList();
                        if (keySet2.Any())
                        {
                            //Verify that all are Clustered or NOT but not mixed
                            if (keySet2.Any(x => x.Item1.Clustered) && keySet2.Any(x => !x.Item1.Clustered))
                                throw new Exception($"Error on {tableType.FullName}. The primary key must be marked as clustered or non-clustered but not both.");

                            //TODO: store clustered somewhere
                            var clustered = keySet2.Any(x => x.Item1.Clustered);

                            hasKey = true;
                            //Verify unique indexes
                            if (keySet2.Select(x => x.Item1.ColumnIndex).Distinct().Count() != keySet2.Count)
                                throw new Exception($"Error on '{tableType.Name}'. Each ColumnIndex property on the PrimaryKey attribute must be unique.");

                            //This is a multi-column PK so add in order
                            foreach (var oo in keySet2)
                                modelBuilder.Entity(tableType.FullName).Property(oo.Item2.PropertyType, oo.Item2.Name);
                            modelBuilder.Entity(tableType.FullName).HasKey(keySet2.OrderBy(x => x.Item1.ColumnIndex).Select(x => x.Item2.Name).ToArray());
                            modelBuilder.Entity(tableType.FullName).HasAnnotation("PrimaryKeyClustered", clustered);
                        }
                    }

                    //If has no key then verify this is intentional
                    if (!hasKey)
                    {
                        if (tableType.GetAttr<HasNoKeyAttribute>() == null)
                            throw new Exception($"The entity {tableType.Name} has no key. Either add a primary key or decorate the entity with the HasNoKeyAttribute attribute.");

                        //If intentional then define in model
                        modelBuilder.Entity(tableType.FullName).HasNoKey();
                    }
                    #endregion

                    #region Create indexes
                    var indexSets = tableType.Props().Where(x => !x.NotMapped() && x.GetAttr<IndexedAttribute>() != null)
                        .Select(x => x.GetAttrWithProp<IndexedAttribute>())
                        .ToList();

                    //Process single column indexes
                    foreach (var index in indexSets.Where(x => string.IsNullOrEmpty(x.Item2.Name)))
                    {
                        var q = modelBuilder.Entity(tableType.FullName).HasIndex(index.Item1.Name);
                        if (index.Item1.IsUnique) q.IsUnique();
                    }

                    //Process multi-column indexes
                    foreach (var indexSet in indexSets.Where(x => !string.IsNullOrEmpty(x.Item2.Name))
                        .GroupBy(x => x.Item1.Name)
                        .Select(x => new { x.Key, Values = x })
                        .ToList())
                    {
                        //Verify unique indexes
                        if (indexSet.Values.Select(x => x.Item1.ColumnIndex).Distinct().Count() != indexSet.Values.Count())
                            throw new Exception($"Error on '{tableType.Name}'. Each ColumnIndex property on the Indexed attribute must be unique.");

                        //This is a multi-column index so add in order
                        foreach (var oo in indexSet.Values)
                            modelBuilder.Entity(tableType.FullName).Property(oo.Item2.PropertyType, oo.Item2.Name);
                        modelBuilder.Entity(tableType.FullName).HasIndex(indexSet.Values.OrderBy(x => x.Item1.ColumnIndex).Select(x => x.Item2.Name).ToArray());
                    }
                    #endregion

                    #region Audit Fields
                    foreach (var prop in tableType.Props(false).Where(x => x.GetCustomAttributes(true).Any(z => z.GetType() != typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute))))
                    {
                        //Created By
                        var attr1 = prop.GetAttr<AuditCreatedByAttribute>();
                        if (attr1 != null)
                        {
                            modelBuilder.Entity(tableType.FullName).Property(prop.Name);
                            if (!stringUnboundedLengths.Any(x => x.Item2.Name == prop.Name) && !stringUnboundedLengths.Any(x => x.Item2.Name == prop.Name))
                            {
                                //Only set string length if it is undefined
                                if (!prop.CustomAttributes.Any(x => x.AttributeType == typeof(StringLengthAttribute) || x.AttributeType == typeof(StringLengthUnboundedAttribute)))
                                {
                                    modelBuilder.Entity(tableType.FullName).Property(prop.Name).HasMaxLength(50);
                                    auditAddedImplicitLengths++;
                                }
                            }
                        }

                        //Created Date
                        var attr2 = prop.GetAttr<AuditCreatedDateAttribute>();
                        if (attr2 != null)
                        {
                            modelBuilder.Entity(tableType.FullName).Property(prop.Name).IsRequired();
                        }

                        //Modified By
                        var attr3 = prop.GetAttr<AuditModifiedByAttribute>();
                        if (attr3 != null)
                        {
                            modelBuilder.Entity(tableType.FullName).Property(prop.Name);
                            if (!stringUnboundedLengths.Any(x => x.Item2.Name == prop.Name) && !stringUnboundedLengths.Any(x => x.Item2.Name == prop.Name))
                            {
                                //Only set string length if it is undefined
                                if (!prop.CustomAttributes.Any(x => x.AttributeType == typeof(StringLengthAttribute) || x.AttributeType == typeof(StringLengthUnboundedAttribute)))
                                {
                                    modelBuilder.Entity(tableType.FullName).Property(prop.Name).HasMaxLength(50);
                                    auditAddedImplicitLengths++;
                                }
                            }
                        }

                        //Modified Date
                        var attr4 = prop.GetAttr<AuditModifiedDateAttribute>();
                        if (attr4 != null)
                        {
                            modelBuilder.Entity(tableType.FullName).Property(prop.Name).IsRequired();
                        }

                        //Audit Timestamp
                        var attr5 = prop.GetAttr<ConcurrencyCheckAttribute>();
                        if (attr5 != null)
                        {
                            modelBuilder.Entity(tableType.FullName).Property(prop.Name).IsRequired().IsConcurrencyToken(true).IsRowVersion();
                            if (!stringUnboundedLengths.Any(x => x.Item2.Name == prop.Name) && !stringUnboundedLengths.Any(x => x.Item2.Name == prop.Name))
                            {
                                modelBuilder.Entity(tableType.FullName).Property(prop.Name).HasMaxLength(50);
                                if (tableType.Props(false).Any(x => x.Name == prop.Name && x.PropertyType == typeof(string)))
                                    auditAddedImplicitLengths++;
                            }
                        }

                        //Timestamp (default .NET attribute)
                        var attr6 = prop.GetAttr<System.ComponentModel.DataAnnotations.TimestampAttribute>();
                        if (attr6 != null)
                        {
                            modelBuilder.Entity(tableType.FullName).Property(prop.Name).IsRequired().IsConcurrencyToken(true).IsRowVersion();
                            if (!stringUnboundedLengths.Any(x => x.Item2.Name == prop.Name) && !stringUnboundedLengths.Any(x => x.Item2.Name == prop.Name))
                            {
                                modelBuilder.Entity(tableType.FullName).Property(prop.Name).HasMaxLength(50);
                                auditAddedImplicitLengths++;
                            }
                        }
                    }
                    #endregion

                    #region Verify the same property does not have both string length and unbounded
                    if (stringLengths.Select(x => x.Item2).Concat(stringUnboundedLengths.Select(x => x.Item2)).Distinct().Count() != stringLengths.Count + stringUnboundedLengths.Count)
                    {
                        throw new Exception($"The entity {tableType.Name} has a property marked with a StringLengthAttribute and StringLengthUnboundedAttribute.");
                    }
                    //TODO: Verify "char" has length of 1
                    #endregion

                    #region Verify all strings have length attribute
                    var allStringCount = tableType.Props(false).Where(x => !x.NotMapped() && x.PropertyType == typeof(string)).Count();
                    if (allStringCount != stringLengths.Count + stringUnboundedLengths.Count + auditAddedImplicitLengths + skipStringLengthChecks)
                    {
                        throw new Exception($"The entity {tableType.Name} must have all string properties decorated with StringLengthAttribute or StringLengthUnboundedAttribute.");
                    }
                    #endregion

                    #region Set all properties to nullable/notnullable if explicitly set

                    var nullableFields = tableType.Props(false).Where(x => !x.NotMapped() && x.GetAttr<NullableAttribute>() != null)
                        .Select(x => x.GetAttrWithProp<NullableAttribute>())
                        .ToList();
                    var requiredFields = tableType.Props(false).Where(x => !x.NotMapped() && x.GetAttr<RequiredAttribute>() != null)
                        .Select(x => x.GetAttrWithProp<RequiredAttribute>())
                        .ToList();

                    //There shoudl be no overlap. These are multually exclusive attributes
                    var allRequiredCount = nullableFields.Select(x => x.Item2).ToList().Concat(requiredFields.Select(x => x.Item2)).Distinct().Count();
                    if (allRequiredCount != nullableFields.Count + requiredFields.Count)
                        throw new Exception($"The entity {tableType.Name} has one or more properties marked with the NullableAttribute and RequiredAttribute attributes. These are mutually exclusive.");

                    foreach (var item in nullableFields)
                    {
                        var propertyInfo = item.Item2;
                        bool canAssignNull =
                                !propertyInfo.PropertyType.IsValueType ||
                                propertyInfo.PropertyType.IsGenericType &&
                                propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);

                        if (item.Item1.AllowNull && !canAssignNull)
                            throw new Exception($"The property {tableType.Name}.{item.Item2.Name} cannot be marked as nullable.");

                        modelBuilder.Entity(tableType.FullName).Property(item.Item2.Name).IsRequired(!item.Item1.AllowNull);
                    }

                    #endregion

                    #region Verify immutable entities
                    if (tableType.GetCustomAttributes(typeof(System.ComponentModel.ImmutableObjectAttribute), true).Any())
                    {
                        foreach (var item in tableType.Props())
                        {
                            //If the set method is null then it is read-only
                            //If NOT read-only then throw exception
                            var s = item.GetSetMethod();
                            if (s != null)
                            {
                                //If there is a set method then make sure it is a collection map and not a property
                                if (!((System.Reflection.TypeInfo)item.PropertyType).ImplementedInterfaces.Any(x => x == typeof(IEnumerable) || x == typeof(IList) || x == typeof(ICollection)))
                                {
                                    throw new Exception($"Entity {tableType.Name} is immutable and cannot have writable properties.");
                                }
                            }
                        }
                    }
                    #endregion

                    #region Database Generated
                    var valueGeneratedAttrList = tableType.Props(false).Where(x => !x.NotMapped() && x.GetAttr<DatabaseGeneratedAttribute>() != null)
                        .Select(x => x.GetAttrWithProp<DatabaseGeneratedAttribute>())
                        .ToList();

                    foreach (var item in valueGeneratedAttrList)
                    {
                        modelBuilder.Entity(tableType.FullName).Property(item.Item2.Name).ValueGeneratedOnAdd();
                    }
                    #endregion

                }
                #endregion

                #region Verify all mapped entities inherit from "IEntity"

                var entities = modelBuilder.Model.GetEntityTypes().ToList();
                foreach (var ee in entities)
                {
                    if (ee.ClrType == null)
                        throw new Exception($"The entity type {ee.Name} was not declared on the context.");
                    if (!ee.ClrType.GetInterfaces().Contains(typeof(IEntity)))
                        throw new Exception($"The entity type {ee.ClrType.Name} does not implement IEntity.");
                }

                #endregion

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        protected T GetPropertyByAttribute<T>(object entity, System.Type attrType)
        {
            if (entity == null) return default(T);
            var attr = entity.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Where(x => x.GetCustomAttributes(true).Any(z => z.GetType() == attrType))
                .FirstOrDefault();
            if (attr != null)
            {
                return (T)entity.GetType().GetProperty(attr.Name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .GetValue(entity);
            }
            return default(T);
        }

        protected bool SetPropertyByAttribute(object entity, System.Type attrType, object value)
        {
            if (entity == null) return false;
            var attr = entity.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Where(x => x.GetCustomAttributes(true).Any(z => z.GetType() == attrType))
                .FirstOrDefault();
            if (attr != null)
            {
                entity.GetType().GetProperty(attr.Name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(entity, value);
                return true;
            }
            return false;
        }

        protected void SetPropertyConcurrency(object entity, System.Type attrType)
        {
            if (entity == null) return;
            var attr = entity.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Where(x => x.GetCustomAttributes(true).Any(z => z.GetType() == attrType))
                .FirstOrDefault();
            if (attr != null)
            {
                var property = entity.GetType().GetProperty(attr.Name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var value = property.GetValue(entity);
                if (value is int)
                {
                    SetPropertyByName(entity, property.Name, ((int)value) + 1);
                }
                else if (value is long)
                {
                    SetPropertyByName(entity, property.Name, ((long)value) + 1);
                }
                else if (value is Guid)
                {
                    SetPropertyByName(entity, property.Name, Guid.NewGuid());
                }
                else
                    throw new Exception("The concurrency token cannot be set.");
            }
        }

        protected bool SetPropertyByName(object entity, string propertyName, object value)
        {
            if (entity == null) return false;
            var attr = entity.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Where(x => x.Name == propertyName)
                .FirstOrDefault();
            if (attr != null)
            {
                entity.GetType().GetProperty(attr.Name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(entity, value);
                return true;
            }
            return false;
        }

    }

    internal class ModelCacheKeyFactory : Microsoft.EntityFrameworkCore.Infrastructure.IModelCacheKeyFactory
    {
        public object Create(DbContext context) 
            => context is ContextBase myContext ? (context.GetType(), myContext.ModelCacheKey) : (object)context.GetType();
    }
}
