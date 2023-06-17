using UnityEngine;
using UnityEngine.UI;

public class MoneyText : MonoBehaviour
{
	private Text text;

	private void OnEnable()
	{
		UpdateMoney();
	}

	public void UpdateMoney()
	{
		if (text == null)
		{
			text = GetComponent<Text>();
		}
		text.text = DivideMoney(GameProgressSaver.GetMoney()) + "<color=orange>P</color>";
	}

	public static string DivideMoney(int dosh)
	{
		int num = dosh;
		int num2 = 0;
		int num3 = 0;
		if (dosh > 1000000000)
		{
			return "LIKE, A LOT OF ";
		}
		while (num >= 1000)
		{
			num2++;
			num -= 1000;
		}
		while (num2 >= 1000)
		{
			num3++;
			num2 -= 1000;
		}
		if (num3 > 0)
		{
			string text = num3 + ",";
			text = ((num2 < 10) ? (text + "00" + num2 + ",") : ((num2 >= 100) ? (text + num2 + ",") : (text + "0" + num2 + ",")));
			if (num < 10)
			{
				return text + "00" + num;
			}
			if (num < 100)
			{
				return text + "0" + num;
			}
			return text + num;
		}
		if (num2 > 0)
		{
			string text = num2 + ",";
			if (num < 10)
			{
				return text + "00" + num;
			}
			if (num < 100)
			{
				return text + "0" + num;
			}
			return text + num;
		}
		return string.Concat(num);
	}
}
