namespace ULTRAKILL.Cheats;

public class TeleportMenu : ICheat
{
	public string LongName => "Teleport Menu";

	public string Identifier => "ultrakill.teleport-menu";

	public string ButtonEnabledOverride => "CLOSE";

	public string ButtonDisabledOverride => "OPEN";

	public string Icon => "teleport";

	public bool IsActive { get; }

	public bool DefaultState { get; }

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public void Enable()
	{
		if (!GameStateManager.Instance.IsStateActive("sandbox-spawn-menu"))
		{
			MonoSingleton<CheatsManager>.Instance.HideMenu();
			MonoSingleton<OptionsManager>.Instance.UnPause();
		}
		MonoSingleton<CheatsController>.Instance.ShowTeleportPanel();
	}

	public void Disable()
	{
	}

	public void Update()
	{
	}
}
