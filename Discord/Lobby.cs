using System.Runtime.InteropServices;

namespace Discord;

public struct Lobby
{
	public long Id;

	public LobbyType Type;

	public long OwnerId;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string Secret;

	public uint Capacity;

	public bool Locked;
}
