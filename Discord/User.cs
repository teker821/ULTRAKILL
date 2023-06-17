using System.Runtime.InteropServices;

namespace Discord;

public struct User
{
	public long Id;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	public string Username;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
	public string Discriminator;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string Avatar;

	public bool Bot;
}
