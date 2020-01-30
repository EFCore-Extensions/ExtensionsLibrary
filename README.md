# Entity Framework Core Extensions
This library is a set of extension attributes for Entity Framework Core that allows additional functionality to be defined in a code first model.

## Audit Properties
TODO

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
TODO

## Soft Delete
Hello
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
    
    //Select from database with no where and the item is not retrieved
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
