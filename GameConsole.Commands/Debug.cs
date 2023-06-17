using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameConsole.Commands;

public class Debug : CommandRoot
{
	public static bool AgonyDebugOverlay = true;

	private Console console;

	public override string Name => "Debug";

	public override string Description => "Console debug stuff.";

	protected override void BuildTree(Console con)
	{
		console = con;
		Leaf("burst_print", delegate(string count)
		{
			con.StartCoroutine(BurstPrint(int.Parse(count), ConsoleLogType.Log));
		});
		Leaf("bulk_print", delegate(string count)
		{
			int num = int.Parse(count);
			for (int j = 0; j < num; j++)
			{
				con.PrintLine("Bulk print " + j);
			}
		});
		Leaf("toggle_overlay", delegate
		{
			AgonyDebugOverlay = !AgonyDebugOverlay;
			con.PrintLine("AgonyDebugOverlay: " + AgonyDebugOverlay);
		});
		Leaf("error", delegate
		{
			throw new Exception("Umm, ermm, guuh!!");
		});
		Leaf("freeze_game", delegate(string confrm)
		{
			if (confrm == "pretty_please")
			{
				while (true)
				{
				}
			}
			con.PrintLine("Usage: freeze_game pretty_please");
		});
		Leaf("timescale", delegate(string timescale)
		{
			Time.timeScale = float.Parse(timescale);
		}, requireCheats: true);
		Leaf("die_respawn", delegate
		{
			con.PrintLine("Killing and immediately respawning player...");
			bool paused = MonoSingleton<OptionsManager>.Instance.paused;
			if (paused)
			{
				MonoSingleton<OptionsManager>.Instance.UnPause();
			}
			con.StartCoroutine(KillRespawnDelayed(paused));
		}, requireCheats: true);
		Leaf("total_secrets", delegate
		{
			con.PrintLine(GameProgressSaver.GetTotalSecretsFound().ToString());
		});
		Leaf("auto_register", delegate
		{
			con.PrintLine("Attempting to auto register all commands...");
			List<ICommand> list = new List<ICommand>();
			Type[] types = typeof(ICommand).Assembly.GetTypes();
			foreach (Type type in types)
			{
				if (!con.registeredCommandTypes.Contains(type) && typeof(ICommand).IsAssignableFrom(type) && !type.IsInterface)
				{
					list.Add((ICommand)Activator.CreateInstance(type));
				}
			}
			con.RegisterCommands(list);
		});
	}

	private IEnumerator BurstPrint(int count, ConsoleLogType type)
	{
		for (int i = 0; i < count; i++)
		{
			console.PrintLine("Hello World " + i, type);
			yield return new WaitForSecondsRealtime(3f / (float)count);
		}
	}

	private IEnumerator KillRespawnDelayed(bool wasPaused)
	{
		yield return new WaitForEndOfFrame();
		MonoSingleton<NewMovement>.Instance.GetHurt(999999, invincible: false, 1f, explosion: false, instablack: true);
		MonoSingleton<StatsManager>.Instance.Restart();
		if (wasPaused)
		{
			MonoSingleton<OptionsManager>.Instance.Pause();
		}
	}
}
