namespace ULTRAKILL.Cheats;

public class QuickSave : ICheat
{
	private bool active;

	public string LongName => "Quick Save";

	public string Identifier => "ultrakill.sandbox.quick-save";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => "SAVE";

	public string Icon => "save";

	public bool IsActive => active;

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public void Enable()
	{
		MonoSingleton<SandboxSaver>.Instance.QuickSave();
	}

	public void Disable()
	{
		active = false;
	}

	public void Update()
	{
	}
}
