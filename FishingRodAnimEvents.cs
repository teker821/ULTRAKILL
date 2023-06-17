using UnityEngine;

public class FishingRodAnimEvents : MonoBehaviour
{
	[SerializeField]
	private FishingRodWeapon weapon;

	public void ThrowBaitEvent()
	{
		weapon.ThrowBaitEvent();
	}
}
