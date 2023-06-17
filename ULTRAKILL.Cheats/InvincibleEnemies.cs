namespace ULTRAKILL.Cheats;

public class InvincibleEnemies : ICheat
{
	private static InvincibleEnemies _lastInstance;

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

	public string LongName => "Invincible Enemies";

	public string Identifier => "ultrakill.invincible-enemies";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "invincible-enemies";

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
		MonoSingleton<NewMovement>.Instance.currentWallJumps = 0;
	}
}
