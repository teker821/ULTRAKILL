using UnityEngine;

namespace ULTRAKILL.Cheats;

public class FullBright : ICheat
{
	private bool active;

	private GameObject lightObject;

	public string LongName => "Fullbright";

	public string Identifier => "ultrakill.full-bright";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => null;

	public string Icon => "light";

	public bool IsActive => active;

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable()
	{
		active = true;
		lightObject = Object.Instantiate(MonoSingleton<CheatsController>.Instance.fullBrightLight);
	}

	public void Disable()
	{
		active = false;
		Object.Destroy(lightObject);
	}

	public void Update()
	{
	}
}
