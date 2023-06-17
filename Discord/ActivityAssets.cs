using System.Runtime.InteropServices;

namespace Discord;

public struct ActivityAssets
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string LargeImage;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string LargeText;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string SmallImage;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string SmallText;
}
