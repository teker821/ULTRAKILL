using UnityEngine;

public class Torch : MonoBehaviour
{
	private Light torchLight;

	private bool pickedUp;

	private Vector3 originalPos;

	private ItemIdentifier itid;

	private void Start()
	{
		torchLight = GetComponentInChildren<Light>();
		originalPos = torchLight.transform.localPosition;
		itid = GetComponent<ItemIdentifier>();
	}

	private void Update()
	{
		if (pickedUp && (bool)torchLight && !itid.hooked)
		{
			torchLight.transform.position = MonoSingleton<PlayerTracker>.Instance.GetTarget().position;
		}
		else if (torchLight.transform.localPosition != originalPos)
		{
			torchLight.transform.localPosition = originalPos;
		}
	}

	public void HitWith(GameObject target)
	{
		Flammable component = target.gameObject.GetComponent<Flammable>();
		if (component != null)
		{
			component.Burn(4f);
		}
	}

	public void PickUp()
	{
		pickedUp = true;
	}

	public void PutDown()
	{
		pickedUp = false;
		torchLight.transform.localPosition = originalPos;
	}
}
