using System;
using UnityEngine;

public class SeasonalHats : MonoBehaviour
{
	private DateTime time;

	[SerializeField]
	private GameObject christmas;

	[SerializeField]
	private GameObject halloween;

	[SerializeField]
	private GameObject easter;

	private void Start()
	{
		if (!MonoSingleton<PrefsManager>.Instance.GetBool("seasonalEvents"))
		{
			return;
		}
		time = DateTime.Now;
		switch (time.Month)
		{
		case 12:
			if (time.Day >= 22 && time.Day <= 28)
			{
				christmas.SetActive(value: true);
			}
			return;
		case 10:
			if (time.Day >= 25 && time.Day <= 31)
			{
				halloween.SetActive(value: true);
			}
			return;
		}
		DateTime dateTime = GetEaster(time.Year);
		if (time.DayOfYear >= dateTime.DayOfYear - 2 && time.DayOfYear <= dateTime.DayOfYear)
		{
			easter.SetActive(value: true);
		}
	}

	private DateTime GetEaster(int year)
	{
		int num = year % 19;
		int num2 = year / 100;
		int num3 = (num2 - num2 / 4 - (8 * num2 + 13) / 25 + 19 * num + 15) % 30;
		int num4 = num3 - num3 / 28 * (1 - num3 / 28 * (29 / (num3 + 1)) * ((21 - num) / 11));
		int num5 = num4 - (year + year / 4 + num4 + 2 - num2 + num2 / 4) % 7;
		int num6 = 3 + (num5 + 40) / 44;
		int day = num5 + 28 - 31 * (num6 / 4);
		return new DateTime(year, num6, day);
	}
}
