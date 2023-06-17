using ULTRAKILL.Cheats;
using UnityEngine;

public class DisablePowerUp : MonoBehaviour
{
	private void Start()
	{
		if (MonoSingleton<PowerUpMeter>.Instance.juice > 0f && !InfinitePowerUps.Enabled)
		{
			MonoSingleton<PowerUpMeter>.Instance.EndPowerUp();
		}
	}
}
