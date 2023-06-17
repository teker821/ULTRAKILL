using UnityEngine;
using UnityEngine.UI;

public class GunColorLock : MonoBehaviour
{
	private int weaponNumber;

	public bool alreadyUnlocked;

	public UltrakillEvent onUnlock;

	public Button button;

	public Text buttonText;

	private void OnEnable()
	{
		if (weaponNumber == 0)
		{
			weaponNumber = GetComponentInParent<GunColorTypeGetter>().weaponNumber;
		}
		if (GameProgressSaver.HasWeaponCustomization((GameProgressSaver.WeaponCustomizationType)(weaponNumber - 1)))
		{
			onUnlock?.Invoke();
		}
		else if (GameProgressSaver.GetMoney() < 1000000)
		{
			button.interactable = false;
			buttonText.text = "<color=red>1,000,000P</color>";
			button.GetComponent<ShopButton>().failure = true;
		}
		else
		{
			button.interactable = true;
			buttonText.text = "1,000,000<color=orange>P</color>";
			button.GetComponent<ShopButton>().failure = false;
		}
	}

	public void Unlock()
	{
		GameProgressSaver.AddMoney(-1000000);
		GameProgressSaver.UnlockWeaponCustomization((GameProgressSaver.WeaponCustomizationType)(weaponNumber - 1));
		onUnlock?.Invoke();
		GetComponentInParent<GunColorTypeGetter>().SetType(isCustom: true);
		MoneyText[] array = Object.FindObjectsOfType<MoneyText>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateMoney();
		}
		VariationInfo[] array2 = Object.FindObjectsOfType<VariationInfo>();
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].UpdateMoney();
		}
	}
}
