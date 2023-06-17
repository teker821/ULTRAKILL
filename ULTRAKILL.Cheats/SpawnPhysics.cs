namespace ULTRAKILL.Cheats;

public class SpawnPhysics : ICheat
{
	private static SpawnPhysics _lastInstance;

	private bool active;

	public static bool PhysicsDynamic
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

	public string LongName => "Spawn With Physics";

	public string Identifier => "ultrakill.sandbox.physics";

	public string ButtonEnabledOverride => "DYNAMIC";

	public string ButtonDisabledOverride => "STATIC";

	public string Icon => "physics";

	public bool IsActive => active;

	public bool DefaultState => false;

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
