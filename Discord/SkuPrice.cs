using System.Runtime.InteropServices;

namespace Discord;

public struct SkuPrice
{
	public uint Amount;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
	public string Currency;
}
