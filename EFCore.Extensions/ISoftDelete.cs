namespace EFCore.Extensions
{
    /// <summary>
    /// Entities implementing this interface will have soft delete functionality.
    /// When the entity has a boolean property named "IsDeleted", the functionality will be picked up by convention.
    /// The 'SoftDeleteFieldAttribute' can be used on a boolean property of this entity to mark the deleted discriminator if the name is not "IsDeleted".
    /// </summary>
    public interface ISoftDeleted : IEntity
    {
        bool IsDeleted { get; }
    }
}
