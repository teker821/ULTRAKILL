using System.Globalization;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class CrateCounter : MonoSingleton<CrateCounter>
{
	public int crateAmount;

	private int currentCrates;

	private int unsavedCrates;

	[SerializeField]
	private Text display;

	private int currentCoins;

	private int savedCoins;

	private bool success;

	public UltrakillEvent onAllCratesGet;

	private void Start()
	{
		UpdateDisplay();
	}

	public void AddCrate()
	{
		currentCrates++;
		unsavedCrates++;
		UpdateDisplay();
	}

	public void AddCoin()
	{
		currentCoins++;
	}

	public void SaveStuff()
	{
		unsavedCrates = 0;
		savedCoins += currentCoins;
		currentCoins = 0;
	}

	public void CoinsToPoints()
	{
		if (savedCoins > 0)
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			GameProgressSaver.AddMoney(savedCoins * 100);
			MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=grey>TRANSACTION COMPLETE:</color> " + savedCoins + " COINS <color=orange>=></color> " + StatsManager.DivideMoney(savedCoins * 100) + "<color=orange>P</color>");
			savedCoins = 0;
		}
	}

	public void ResetUnsavedStuff()
	{
		currentCrates -= unsavedCrates;
		unsavedCrates = 0;
		currentCoins = 0;
		UpdateDisplay();
	}

	private void UpdateDisplay()
	{
		if ((bool)display)
		{
			display.text = currentCrates.ToString() + " / " + crateAmount;
		}
		if (crateAmount != 0)
		{
			if (!success && currentCrates >= crateAmount)
			{
				success = true;
				onAllCratesGet?.Invoke();
			}
			else if (success && currentCrates < crateAmount)
			{
				success = false;
				onAllCratesGet?.Revert();
			}
		}
	}
}
