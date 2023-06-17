using System;

namespace GameConsole;

[Flags]
public enum LogFilters
{
	None = 0,
	Errors = 1,
	Warnings = 2,
	Info = 4,
	All = 7
}
