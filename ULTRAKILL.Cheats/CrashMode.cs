namespace ULTRAKILL.Cheats;

public class CrashMode : ICheat
{
	private bool active;

	public string LongName => "Clash Mode";

	public string Identifier => "ultrakill.clash-mode";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => null;

	public string Icon => "clash";

	public bool IsActive => active;

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable()
	{
		if (MonoSingleton<PlayerTracker>.Instance.levelStarted)
		{
			MonoSingleton<CheatsManager>.Instance.DisableCheat("ultrakill.flight");
			MonoSingleton<CheatsManager>.Instance.DisableCheat("ultrakill.noclip");
		}
		active = true;
		MonoSingleton<PlayerTracker>.Instance.ChangeToPlatformer();
	}

	public void Disable()
	{
		active = false;
		MonoSingleton<PlayerTracker>.Instance.ChangeToFPS();
	}

	public void Update()
	{
	}
}
