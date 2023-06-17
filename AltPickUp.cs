using UnityEngine;
using UnityEngine.Events;

public class AltPickUp : MonoBehaviour
{
	public string pPref;

	public UnityEvent onPickUp;

	private void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			GotActivated();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			GotActivated();
		}
	}

	private void GotActivated()
	{
		GameProgressSaver.AddGear(pPref + "alt");
		MonoSingleton<PrefsManager>.Instance.SetInt("weapon." + pPref + "0", 2);
		MonoSingleton<GunSetter>.Instance.ResetWeapons();
		MonoSingleton<GunSetter>.Instance.ForceWeapon(pPref + "0");
		onPickUp?.Invoke();
		Object.Destroy(base.gameObject);
	}
}
