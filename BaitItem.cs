using System.Linq;
using UnityEngine;

public class BaitItem : MonoBehaviour
{
	[SerializeField]
	private bool silentFail;

	[SerializeField]
	private GameObject consumedPrefab;

	[SerializeField]
	private FishObject[] attractFish;

	[SerializeField]
	private FishDB[] supportedWaters;

	private bool used;

	private void OnTriggerEnter(Collider other)
	{
		if (used || other.gameObject.layer != 4)
		{
			return;
		}
		Water component = other.GetComponent<Water>();
		if (component.fishDB == null)
		{
			return;
		}
		used = true;
		if (!supportedWaters.Contains(component.fishDB))
		{
			if (!silentFail)
			{
				MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=red>This bait didn't work here!</color>");
			}
		}
		else
		{
			Object.Instantiate(consumedPrefab, base.transform.position, Quaternion.identity);
			component.attractFish = attractFish;
			MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("A fish took the bait.");
			Object.Destroy(base.gameObject);
		}
	}
}
