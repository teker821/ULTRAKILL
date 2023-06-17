namespace ULTRAKILL.Cheats;

public class PauseTimedBombs : ICheat
{
	private static PauseTimedBombs _lastInstance;

	private bool active;

	public static bool Paused
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

	public string LongName => "Pause Timed Bombs";

	public string Identifier => "ultrakill.pause-timed-bombs";

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
