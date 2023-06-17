namespace ULTRAKILL.Cheats;

public class BlindEnemies : ICheat
{
	private static BlindEnemies _lastInstance;

	private bool active;

	public static bool Blind
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

	public string LongName => "Blind Enemies";

	public string Identifier => "ultrakill.blind-enemies";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "blind";

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
