namespace ULTRAKILL.Cheats;

public class HideCheatsStatus : ICheat
{
	private static HideCheatsStatus _lastInstance;

	private bool active;

	public static bool HideStatus
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

	public string LongName => "Hide Cheats Enabled Status";

	public string Identifier => "ultrakill.debug.hide-cheats-status";

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
	}
}
