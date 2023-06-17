namespace ULTRAKILL.Cheats;

public class KeepEnabled : ICheat
{
	private bool active;

	public string LongName => "Keep Cheats Enabled";

	public string Identifier => "ultrakill.keep-enabled";

	public string ButtonEnabledOverride => "STAY ACTIVE";

	public string ButtonDisabledOverride => "DISABLE ON RELOAD";

	public string Icon => "warning";

	public bool IsActive => active;

	public bool DefaultState => false;

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
}
