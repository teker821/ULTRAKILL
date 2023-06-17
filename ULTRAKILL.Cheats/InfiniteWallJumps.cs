namespace ULTRAKILL.Cheats;

public class InfiniteWallJumps : ICheat
{
	private bool active;

	public string LongName => "Infinite Wall Jumps";

	public string Identifier => "ultrakill.infinite-wall-jumps";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "infinite-wall-jumps";

	public bool IsActive => active;

	public bool DefaultState { get; }

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
		MonoSingleton<NewMovement>.Instance.currentWallJumps = 0;
	}
}
