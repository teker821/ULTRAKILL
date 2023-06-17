using System.Runtime.InteropServices;

namespace Discord;

public struct ActivitySecrets
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string Match;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string Join;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string Spectate;
}
