# Entity Framework Core Extensions
This library is a set of extension attributes for Entity Framework Core that allows additional functionality to be defined in a code first model. The default attribute set allows for the decoration of a code model but is lacking in some more advanced functionality. This extension library allows for a more explcit, advanced model syntax that extends Entity Framework Core.

## Audit Properties
Audit properties are useful to track when a database row was created and modified. There are four pre-defined audit fields: CreatedBy, CreatedDate, ModifiedBy, and ModifiedDate. One or more of these properties can be added to any entity. When the appropriate attribute is on a field, it becomes an audit field and is tracked automatically.

```
public class Customer : IEntity
{
    [PrimaryKey]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [StringLength(50)]
    [Required]
    public string Name { get; set; }

    [AuditCreatedBy]
    public virtual string CreatedBy { get; protected set; }

    [AuditCreatedDate]
    public virtual DateTime CreatedDate { get; protected set; }

    [AuditModifiedBy]
    public virtual string ModifiedBy { get; protected set; }

    [AuditModifiedDate]
    public virtual DateTime ModifiedDate { get; protected set; }
}
```

Alternatively, an entity can derive from "AuditableBaseEntity" and inherit all of this functionality. The following is functionally equivalent to the class above.

```
public class Customer : AuditableBaseEntity
{
    [PrimaryKey]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [StringLength(50)]
    [Required]
    public string Name { get; set; }
}
```

## Concurrency
The ConcurrencyCheckkAttribute can be used on an int, long, guid, or byte array property to specify that it be used as the concurrency token. SQL Server will manage the byte array as a built in timestamp field. However on other database types this is not implemented. Also the int, long, and guid options are managed by the framework and are database independent.

```
public class Customer : IEntity
{
    [PrimaryKey]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [StringLength(50)]
    [Required]
    public string Name { get; set; }

    [Required]
    [VersionField]
    public int Version { get; protected set; }
}

public class UserAccount : IEntity
{
    [PrimaryKey]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [StringLength(50)]
    [Required]
    public string Name { get; set; }

    [Required]
    [VersionField]
    public Guid Version { get; protected set; }
}

```

## DefaultValue (Special)
TODO

## Composite Primary Key
TODO

## Composite Index
Adding a composite index with more than one property is easy with the IndexedAttribute. Define a name for the grouping and each property has an order that defines its position in the index. The following snippet will create a single index on "FirstName" and "LastName", in that order. The property order is important for how an index is used for searching.

```
public class Customer : AuditableBaseEntity
{
    [PrimaryKey]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [StringLength(50)]
    [Required]
    [Indexed("MyIndex", 1)]
    public string FirstName { get; set; }

    [StringLength(50)]
    [Required]
    [Indexed("MyIndex", 2)]
    public string LastName { get; set; }
}
```

## Soft Delete
A soft delete allows an entity to be marked as deleted in the database using a Boolean flag instead of actually removing it. The where filter is implied, so there is no reason to include it. The snippet below marks the first item as deleted and saves the context. The next line selects all items from the table with no where clause. The soft deleted item is not in the list, even though it is still in the database.

```
//This entity implements the ISoftDeleted interface and has a IsDeleted property
public class Car : ISoftDeleted
{
    [PrimaryKey]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual int CarId { get; protected set; }

    [StringLength(50)]
    public virtual string Name { get; set; }

    public virtual bool IsDeleted { get; set; }
}

using (var context = new DataContext(connectionString))
{
    //Soft delete the first item
    var car = context.Car.First();
    car.IsDeleted = true;
    context.SaveChanges();
    
    //Select from table with no where clause and the item is not retrieved
    var list = context.Car.ToList();
}

```

## Version Property
The VersionFieldAttribute allows an integer property to be incremented on each save. Each time an entity is modified in any way and saved back to the database, this property will have its value incremented automatically.

```
public class Customer : IEntity
{
    [PrimaryKey]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [StringLength(50)]
    [Required]
    public string Name { get; set; }

    [Required]
    [VersionField]
    public int Version { get; protected set; }
}
```

## Seed Data
TODO

## Multi-tenancy
TODO

## Class Diagram
TODO

