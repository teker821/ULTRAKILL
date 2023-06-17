namespace ULTRAKILL.Cheats;

public class ManageSaves : ICheat
{
	private bool active;

	public string LongName => "Manage Saves";

	public string Identifier => "ultrakill.sandbox.save-menu";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => "OPEN";

	public string Icon => "load";

	public bool IsActive => active;

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public void Enable()
	{
		if (!GameStateManager.Instance.IsStateActive("sandbox-spawn-menu"))
		{
			MonoSingleton<CheatsManager>.Instance.ShowMenu();
			MonoSingleton<OptionsManager>.Instance.Pause();
		}
		MonoSingleton<SandboxHud>.Instance.ShowSavesMenu();
	}

	public void Disable()
	{
		active = false;
	}

	public void Update()
	{
	}
}
