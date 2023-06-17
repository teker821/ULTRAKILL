namespace ULTRAKILL.Cheats;

public class ClearMap : ICheat
{
	private bool active;

	public string LongName => "Clear Map";

	public string Identifier => "ultrakill.sandbox.clear";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => "CLEAR";

	public string Icon => "delete";

	public bool IsActive => active;

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public void Enable()
	{
		SandboxSaver.Clear();
	}

	public void Disable()
	{
		active = false;
	}

	public void Update()
	{
	}
}
