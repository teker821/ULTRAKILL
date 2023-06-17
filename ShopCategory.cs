using UnityEngine;

public class ShopCategory : MonoBehaviour
{
	public string weaponName;

	public void CheckGear()
	{
		if (GameProgressSaver.CheckGear(weaponName) == 0)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
	}
}
