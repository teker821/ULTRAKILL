using UnityEngine;

public class PlayerActivatorRelay : MonoSingleton<PlayerActivatorRelay>
{
	[SerializeField]
	private GameObject[] toActivate;

	[SerializeField]
	private GameObject gunPanel;

	[SerializeField]
	private GameObject crosshair;

	[SerializeField]
	private float delay = 0.2f;

	private int index;

	private void Start()
	{
		GameStateManager.Instance.RegisterState(new GameState("pit-falling", base.gameObject)
		{
			cameraInputLock = LockMode.Lock,
			cursorLock = LockMode.Lock
		});
	}

	public void Activate()
	{
		if (index >= toActivate.Length)
		{
			return;
		}
		if (toActivate[index] == gunPanel)
		{
			if (!MonoSingleton<GunControl>.Instance.noWeapons && MonoSingleton<PrefsManager>.Instance.GetBool("weaponIcons") && !MapInfoBase.InstanceAnyType.hideStockHUD)
			{
				gunPanel.SetActive(value: true);
			}
		}
		else if (toActivate[index] == crosshair)
		{
			crosshair.SetActive(value: true);
		}
		else if (!MapInfoBase.InstanceAnyType.hideStockHUD)
		{
			toActivate[index].SetActive(value: true);
		}
		index++;
		if (index < toActivate.Length)
		{
			Invoke("Activate", delay);
		}
	}
}
