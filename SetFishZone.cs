using UnityEngine;

public class SetFishZone : MonoBehaviour
{
	[SerializeField]
	private bool onEnter = true;

	[SerializeField]
	private bool restorePreviousOnExit = true;

	public float suggestedFishingDistance = 1f;

	private float previousFishingDistance;

	private float previousMinDistance;

	[SerializeField]
	private bool customMinDistance;

	[SerializeField]
	private float minDistance = 1f;

	private void OnTriggerEnter(Collider other)
	{
		if (onEnter && other.gameObject.CompareTag("Player"))
		{
			Set();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (restorePreviousOnExit && other.gameObject.CompareTag("Player"))
		{
			Restore();
		}
	}

	public void Set()
	{
		previousFishingDistance = FishingRodWeapon.suggestedDistanceMulti;
		if (customMinDistance)
		{
			previousMinDistance = FishingRodWeapon.minDistanceMulti;
		}
		FishingRodWeapon.suggestedDistanceMulti = suggestedFishingDistance;
		if (customMinDistance)
		{
			FishingRodWeapon.minDistanceMulti = minDistance;
		}
	}

	public void Restore()
	{
		FishingRodWeapon.suggestedDistanceMulti = previousFishingDistance;
		if (customMinDistance)
		{
			FishingRodWeapon.minDistanceMulti = previousMinDistance;
		}
	}
}
