namespace ULTRAKILL.Cheats;

public class QuickLoad : ICheat
{
	private bool active;

	public string LongName => "Quick Load";

	public string Identifier => "ultrakill.sandbox.quick-load";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => "LOAD LATEST SAVE";

	public string Icon => "quick-load";

	public bool IsActive => active;

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public void Enable()
	{
		MonoSingleton<SandboxSaver>.Instance.QuickLoad();
	}

	public void Disable()
	{
		active = false;
	}

	public void Update()
	{
	}
}
