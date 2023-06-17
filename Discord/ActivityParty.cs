using System.Runtime.InteropServices;

namespace Discord;

public struct ActivityParty
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string Id;

	public PartySize Size;
}
