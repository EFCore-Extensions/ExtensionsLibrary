using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore.Extensions
{
	public interface IContextStartup
	{
		string Modifer { get; }
		bool AllowLazyLoading { get; }
		int CommandTimeout { get; }
	}
}
