using UnityEngine;

public class ShopGearChecker : MonoBehaviour
{
	private ShopCategory[] shopcats;

	private void OnEnable()
	{
		if (shopcats == null)
		{
			shopcats = GetComponentsInChildren<ShopCategory>();
		}
		ShopCategory[] array = shopcats;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CheckGear();
		}
	}
}
