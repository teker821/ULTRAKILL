using System.Runtime.InteropServices;

namespace Discord;

public struct FileStat
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	public string Filename;

	public ulong Size;

	public ulong LastModified;
}
