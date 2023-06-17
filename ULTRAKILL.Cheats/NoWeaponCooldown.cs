namespace ULTRAKILL.Cheats;

public class NoWeaponCooldown : ICheat
{
	private static NoWeaponCooldown _lastInstance;

	private bool active;

	public static bool NoCooldown
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

	public string LongName => "No Weapon Cooldown";

	public string Identifier => "ultrakill.no-weapon-cooldown";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "no-weapon-cooldown";

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
