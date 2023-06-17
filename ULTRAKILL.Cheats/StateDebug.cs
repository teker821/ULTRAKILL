using UnityEngine;

namespace ULTRAKILL.Cheats;

public class StateDebug : ICheat, ICheatGUI
{
	private bool active;

	public string LongName => "Game State Debug";

	public string Identifier => "ultrakill.debug.game-state";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => null;

	public bool IsActive => active;

	public bool DefaultState { get; }

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable()
	{
		active = true;
	}

	public void Disable()
	{
		active = false;
	}

	public void Update()
	{
	}

	public void OnGUI()
	{
		GUILayout.Label("Game State:");
		GUILayout.Label("opman paused: " + MonoSingleton<OptionsManager>.Instance.paused);
		GUILayout.Label("opman frozen: " + MonoSingleton<OptionsManager>.Instance.frozen);
		GUILayout.Label("fc shopping: " + MonoSingleton<FistControl>.Instance.shopping);
		GUILayout.Label("gc activated: " + MonoSingleton<GunControl>.Instance.activated);
		if ((bool)MonoSingleton<WeaponCharges>.Instance)
		{
			GUILayout.Label("rc: " + MonoSingleton<WeaponCharges>.Instance.rocketCount);
			if (MonoSingleton<WeaponCharges>.Instance.rocketCount != 0 && MonoSingleton<WeaponCharges>.Instance.rocketFrozen)
			{
				GUILayout.Label("ts: " + MonoSingleton<WeaponCharges>.Instance.timeSinceIdleFrozen.ToString());
			}
		}
	}
}
