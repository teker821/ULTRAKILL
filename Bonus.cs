using UnityEngine;

public class Bonus : MonoBehaviour
{
	private Vector3 cRotation;

	public GameObject breakEffect;

	private bool activated;

	public bool ghost;

	public bool tutorial;

	public bool superCharge;

	public bool dontReplaceWithGhost;

	[HideInInspector]
	public bool beenFound;

	public int secretNumber = -1;

	public GameObject ghostVersion;

	private void Start()
	{
		cRotation = new Vector3(Random.Range(-5, 5), Random.Range(-5, 5), Random.Range(-5, 5));
		if (beenFound || (secretNumber >= 0 && MonoSingleton<StatsManager>.Instance.newSecrets.Contains(secretNumber)))
		{
			Debug.Log("Name: " + base.gameObject.name + ". Been Found: " + beenFound.ToString() + ". Secret Number: " + secretNumber);
			BeenFound();
		}
	}

	private void Update()
	{
		base.transform.Rotate(cRotation * Time.deltaTime * 5f);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!(other.gameObject.tag == "Player") || activated)
		{
			return;
		}
		if (!ghost)
		{
			activated = true;
			MonoSingleton<TimeController>.Instance.ParryFlash();
			StyleHUD instance = MonoSingleton<StyleHUD>.Instance;
			StatsManager instance2 = MonoSingleton<StatsManager>.Instance;
			Object.Instantiate(breakEffect, base.transform.position, Quaternion.identity);
			instance.AddPoints(0, "ultrakill.secret");
			instance2.secrets++;
			instance2.SecretFound(secretNumber);
			Object.Destroy(base.gameObject);
		}
		else
		{
			if (tutorial)
			{
				MonoSingleton<TimeController>.Instance.ParryFlash();
			}
			Object.Instantiate(breakEffect, base.transform.position, Quaternion.identity);
			Object.Destroy(base.gameObject);
		}
		if (superCharge)
		{
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
			{
				MonoSingleton<NewMovement>.Instance.SuperCharge();
			}
			else
			{
				MonoSingleton<PlatformerMovement>.Instance.AddExtraHit(2);
			}
			if (!MonoSingleton<PrefsManager>.Instance.GetBool("hideSuperChargePopup"))
			{
				MonoSingleton<PrefsManager>.Instance.SetBool("hideSuperChargePopup", content: true);
				MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=red>RED SOUL ORBS</color> give <color=lime>200 HEALTH</color>. \nOverheal cannot be regained with blood.", "", "", 1);
			}
		}
		else if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
		{
			MonoSingleton<PlatformerMovement>.Instance.AddExtraHit();
		}
	}

	public void BeenFound()
	{
		if (ghostVersion == null)
		{
			Debug.Log("No ghost version for " + base.gameObject.name);
			ghostVersion = PrefabReplacer.Instance.LoadPrefab("Bonus Ghost");
			return;
		}
		GameObject gameObject = Object.Instantiate(ghostVersion, base.transform.position, base.transform.rotation);
		if ((bool)base.transform.parent)
		{
			gameObject.transform.SetParent(base.transform.parent, worldPositionStays: true);
		}
		if (TryGetComponent<DualWieldPickup>(out var component) && gameObject.TryGetComponent<DualWieldPickup>(out var component2))
		{
			component2.juiceAmount = component.juiceAmount;
		}
		base.gameObject.SetActive(value: false);
		Object.Destroy(base.gameObject);
	}
}
