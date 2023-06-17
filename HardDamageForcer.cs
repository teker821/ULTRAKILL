using UnityEngine;

public class HardDamageForcer : MonoBehaviour
{
	[HideInInspector]
	private bool activated;

	public bool activateOnEnable = true;

	private void Start()
	{
		if (!activated && activateOnEnable)
		{
			On();
		}
	}

	private void OnEnable()
	{
		if (!activated && activateOnEnable)
		{
			On();
		}
	}

	private void Update()
	{
		if (activated)
		{
			MonoSingleton<NewMovement>.Instance.ForceAntiHP(99f, silent: true, dontOverwriteHp: true, addToCooldown: false);
		}
	}

	public void On()
	{
		activated = true;
	}

	public void Off()
	{
		activated = false;
	}
}
