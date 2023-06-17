namespace ULTRAKILL.Cheats;

public class ExperimentalArmRotation : ICheat
{
	public static bool Enabled;

	public string LongName => "Experimental Arm Rotation";

	public string Identifier => "ultrakill.sandbox.enable-experimental-rotation";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => null;

	public string Icon => "rotate";

	public bool IsActive => Enabled;

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable()
	{
		Enabled = true;
	}

	public void Disable()
	{
		Enabled = false;
	}

	public void Update()
	{
	}
}
