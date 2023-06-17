using UnityEngine;

public class DualWieldPickup : MonoBehaviour
{
	public float juiceAmount = 30f;

	public GameObject pickUpEffect;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			PickedUp();
		}
	}

	private void PickedUp()
	{
		if (!MonoSingleton<GunControl>.Instance)
		{
			return;
		}
		Object.Instantiate(pickUpEffect, base.transform.position, Quaternion.identity);
		MonoSingleton<CameraController>.Instance.CameraShake(0.35f);
		base.gameObject.SetActive(value: false);
		if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
		{
			MonoSingleton<PlatformerMovement>.Instance.AddExtraHit(3);
			return;
		}
		GameObject gameObject = new GameObject();
		gameObject.transform.SetParent(MonoSingleton<GunControl>.Instance.transform, worldPositionStays: true);
		gameObject.transform.localRotation = Quaternion.identity;
		DualWield[] componentsInChildren = MonoSingleton<GunControl>.Instance.GetComponentsInChildren<DualWield>();
		if (componentsInChildren != null && componentsInChildren.Length % 2 == 0)
		{
			gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
		}
		else
		{
			gameObject.transform.localScale = Vector3.one;
		}
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			gameObject.transform.localPosition = Vector3.zero;
		}
		else if (componentsInChildren.Length % 2 == 0)
		{
			gameObject.transform.localPosition = new Vector3((float)(componentsInChildren.Length / 2) * -1.5f, 0f, 0f);
		}
		else
		{
			gameObject.transform.localPosition = new Vector3((float)((componentsInChildren.Length + 1) / 2) * 1.5f, 0f, 0f);
		}
		DualWield dualWield = gameObject.AddComponent<DualWield>();
		dualWield.delay = 0.05f;
		dualWield.juiceAmount = juiceAmount;
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			dualWield.delay += (float)componentsInChildren.Length / 20f;
		}
	}
}
