using System;

namespace GameConsole;

[Flags]
public enum ConsoleLogType
{
	None = 0,
	Log = 1,
	Warning = 2,
	Error = 4,
	Cli = 8,
	All = 0xF
}
