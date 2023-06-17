namespace ULTRAKILL.Cheats;

public class InfinitePowerUps : ICheat
{
	private static InfinitePowerUps _lastInstance;

	private bool active;

	public static bool Enabled
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

	public string LongName => "Infinite Power-Ups";

	public string Identifier => "ultrakill.infinite-power-ups";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "infinite-power-ups";

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
