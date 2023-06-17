using System.Runtime.InteropServices;

namespace Discord;

public struct Sku
{
	public long Id;

	public SkuType Type;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	public string Name;

	public SkuPrice Price;
}
