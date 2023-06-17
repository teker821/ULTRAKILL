namespace ULTRAKILL.Cheats;

public class DisableEnemySpawns : ICheat
{
	private static DisableEnemySpawns _lastInstance;

	private bool active;

	public static bool DisableArenaTriggers
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

	public string LongName => "Disable Enemy Spawns";

	public string Identifier => "ultrakill.disable-enemy-spawns";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "no-enemies";

	public bool IsActive => active;

	public bool DefaultState { get; }

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable()
	{
		_lastInstance = this;
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
