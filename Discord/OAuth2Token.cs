using System.Runtime.InteropServices;

namespace Discord;

public struct OAuth2Token
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string AccessToken;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
	public string Scopes;

	public long Expires;
}
