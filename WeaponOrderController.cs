using UnityEngine;
using UnityEngine.UI;

public class WeaponOrderController : MonoBehaviour
{
	private Text text;

	public int variationNumber;

	public string variationName;

	private string variationOrder;

	private int currentOrderNumber;

	public bool revolver;

	private void Start()
	{
		ResetValues();
	}

	private void OnEnable()
	{
		ResetValues();
	}

	public void ChangeOrderNumber(int additive)
	{
		int num = currentOrderNumber + additive;
		if (num <= 0 || num >= 4)
		{
			return;
		}
		for (int i = 0; i < variationOrder.Length; i++)
		{
			if (variationOrder[i] - 48 == num)
			{
				variationOrder = variationOrder.Replace(variationOrder[i], variationOrder[variationNumber]);
			}
		}
		variationOrder = variationOrder.Remove(variationNumber, 1);
		variationOrder = variationOrder.Insert(variationNumber, num.ToString());
		MonoSingleton<PrefsManager>.Instance.SetString("weapon." + variationName + ".order", variationOrder);
		WeaponOrderController[] componentsInChildren = base.transform.parent.parent.parent.GetComponentsInChildren<WeaponOrderController>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			componentsInChildren[j].ResetValues();
		}
		Object.FindObjectOfType<GunSetter>()?.ResetWeapons();
	}

	public void ResetValues()
	{
		if (!text)
		{
			text = GetComponentInChildren<Text>();
		}
		if (revolver)
		{
			variationOrder = MonoSingleton<PrefsManager>.Instance.GetString("weapon." + variationName + ".order", "1324");
		}
		else
		{
			variationOrder = MonoSingleton<PrefsManager>.Instance.GetString("weapon." + variationName + ".order", "1234");
		}
		currentOrderNumber = variationOrder[variationNumber] - 48;
		text.text = variationOrder[variationNumber].ToString() ?? "";
		Debug.Log("Order in WeaponOrderController: " + variationOrder);
	}
}
