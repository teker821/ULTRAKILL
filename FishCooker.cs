using UnityEngine;

public class FishCooker : MonoBehaviour
{
	[SerializeField]
	private bool unusable;

	private TimeSince timeSinceLastError;

	[SerializeField]
	private ItemIdentifier fishPickupTemplate;

	[SerializeField]
	private FishObject cookedFish;

	[SerializeField]
	private FishObject failedFish;

	[SerializeField]
	private GameObject cookedSound;

	[SerializeField]
	private GameObject cookedParticles;

	private void Awake()
	{
		if (unusable)
		{
			timeSinceLastError = 0f;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.TryGetComponent<FishObjectReference>(out var component))
		{
			return;
		}
		if (unusable)
		{
			if ((float)timeSinceLastError > 2f)
			{
				timeSinceLastError = 0f;
				MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Too small for this fish.\n:^(");
			}
		}
		else if (!(component.fishObject == cookedFish) && !(component.fishObject == failedFish))
		{
			_ = MonoSingleton<FishManager>.Instance.recognizedFishes[cookedFish];
			GameObject obj = FishingRodWeapon.CreateFishPickup(fishPickupTemplate, component.fishObject.canBeCooked ? cookedFish : failedFish, grab: false, unlock: false);
			if (!component.fishObject.canBeCooked)
			{
				MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Cooking failed.");
			}
			obj.transform.position = base.transform.position;
			obj.transform.rotation = Quaternion.identity;
			obj.GetComponent<Rigidbody>().velocity = (MonoSingleton<NewMovement>.Instance.transform.position - base.transform.position).normalized * 18f + Vector3.up * 10f;
			Object.Instantiate(cookedSound, base.transform.position, Quaternion.identity);
			if ((bool)cookedParticles)
			{
				Object.Instantiate(cookedParticles, base.transform.position, Quaternion.identity);
			}
			Object.Destroy(component.gameObject);
		}
	}
}
