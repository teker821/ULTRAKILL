using UnityEngine;

public class AttributeChecker : MonoBehaviour
{
	public HitterAttribute targetAttribute;

	public GameObject toActivate;

	public void DelayedActivate(float time = 0.5f)
	{
		Invoke("Activate", time);
	}

	public void Activate()
	{
		toActivate.gameObject.SetActive(value: true);
		base.gameObject.SetActive(value: false);
	}
}
