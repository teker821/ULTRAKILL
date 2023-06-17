using System;

[Flags]
public enum SingletonFlags
{
	None = 0,
	NoAutoInstance = 1,
	HideAutoInstance = 2,
	NoAwakeInstance = 4,
	PersistAutoInstance = 8,
	DestroyDuplicates = 0x10
}
