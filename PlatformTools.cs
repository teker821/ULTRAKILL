using System;

public static class PlatformTools
{
	public static string ResolveArg(string key)
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (!(commandLineArgs[i] != key) && commandLineArgs.Length > i + 1)
			{
				return commandLineArgs[i + 1];
			}
		}
		return null;
	}
}
