using UnityEngine;

namespace ULTRAKILL.Cheats;

public class PlayerParentingDebug : ICheat, ICheatGUI
{
	private static PlayerParentingDebug _lastInstance;

	private bool active;

	private PlayerMovementParenting[] pmp;

	public static bool Active
	{
		get
		{
			if (_lastInstance != null)
			{
				return _lastInstance.active;
			}
			return false;
		}
	}

	public string LongName => "Player Parenting Debug";

	public string Identifier => "ultrakill.debug.player-parent-debug";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => null;

	public bool IsActive => active;

	public bool DefaultState { get; }

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable()
	{
		active = true;
		_lastInstance = this;
	}

	public void Disable()
	{
		active = false;
	}

	public void Update()
	{
		pmp = Object.FindObjectsOfType<PlayerMovementParenting>();
		if (pmp != null)
		{
			_ = pmp.LongLength;
		}
	}

	public void OnGUI()
	{
		GUILayout.Label("Player Parenting Debug");
		if (pmp == null)
		{
			return;
		}
		PlayerMovementParenting[] array = pmp;
		foreach (PlayerMovementParenting playerMovementParenting in array)
		{
			if (playerMovementParenting == null)
			{
				continue;
			}
			GUILayout.Label(playerMovementParenting.gameObject.name);
			GUILayout.Label("Attached to:");
			foreach (Transform trackedObject in playerMovementParenting.TrackedObjects)
			{
				if (trackedObject == null)
				{
					GUILayout.Label("null");
				}
				else
				{
					GUILayout.Label("- " + trackedObject.name);
				}
			}
			GUILayout.Label("------------------------------");
		}
	}
}
