namespace EFCore.Extensions
{
    /// <summary>
    /// Entities implementing this interface will have tenant based row security functionality.
    /// When the entity has a string property named "TenanId", the functionality will be picked up by convention.
    /// The 'TenantIDFieldAttribute' can be used on a string property of the entity to mark the tenant discriminator if the name is not "TenantId".
    /// </summary>
    public interface ITenantEntity : IEntity
    {
        string TenantId { get; }
    }
}
