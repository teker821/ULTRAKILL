using UnityEngine;

namespace GameConsole.Commands;

public class Scene : ICommand
{
	public string Name => "Scene";

	public string Description => "Loads a scene.";

	public string Command => "scene";

	public void Execute(Console con, string[] args)
	{
		if (con.CheatBlocker())
		{
			return;
		}
		if (args.Length == 0)
		{
			con.PrintLine("Usage: scene <scene name>");
			return;
		}
		string sceneName = string.Join(" ", args);
		if (!UnityEngine.Debug.isDebugBuild && MonoSingleton<SceneHelper>.Instance.IsSceneSpecial(sceneName))
		{
			con.PrintLine("Scene is special and cannot be loaded in release mode. \ud83e\udd7a", ConsoleLogType.Warning);
		}
		else
		{
			SceneHelper.LoadScene(sceneName);
		}
	}
}
