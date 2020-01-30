using System;

namespace EFCore.Extensions
{
    #region TenantContextStartUp

	public class TenantContextStartup : ContextStartup
	{
		public TenantContextStartup(string modifier, string tenantId)
			: base(modifier)
		{
			if (string.IsNullOrEmpty(tenantId))
				throw new Exception("The tenant ID must be set!");

			this.TenantId = tenantId;
		}

		public TenantContextStartup(string modifier, string tenantId, bool allowLazyLoading, int commandTimeout)
			: base(modifier, allowLazyLoading, commandTimeout)
		{
			if (string.IsNullOrEmpty(tenantId))
				throw new Exception("The tenant ID must be set!");

			this.TenantId = tenantId;
		}

		public string TenantId { get; protected internal set; }
	}

	#endregion

}
