using System.Runtime.InteropServices;

namespace Discord;

public struct InputMode
{
	public InputModeType Type;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	public string Shortcut;
}
