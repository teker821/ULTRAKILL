using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class ScanningStuff : MonoSingleton<ScanningStuff>
{
	[SerializeField]
	private GameObject scanningPanel;

	[SerializeField]
	private GameObject readingPanel;

	[SerializeField]
	private Text readingText;

	[SerializeField]
	private ScrollRect scrollRect;

	public Image meter;

	private float loading;

	private bool scanning;

	private Dictionary<int, bool> scannedBooks = new Dictionary<int, bool>();

	private Dictionary<int, float> bookScrollStates = new Dictionary<int, float>();

	private int currentBookId;

	public bool oldWeaponState;

	public bool IsReading => readingPanel.activeInHierarchy;

	public void ReleaseScroll(int instanceId)
	{
		if (bookScrollStates.ContainsKey(instanceId))
		{
			bookScrollStates.Remove(instanceId);
		}
	}

	public void ScanBook(string text, bool noScan, int instanceId)
	{
		oldWeaponState = !MonoSingleton<GunControl>.Instance.noWeapons;
		MonoSingleton<GunControl>.Instance.NoWeapon();
		readingText.text = text;
		if (bookScrollStates.ContainsKey(instanceId))
		{
			scrollRect.verticalNormalizedPosition = bookScrollStates[instanceId];
		}
		else
		{
			scrollRect.verticalNormalizedPosition = 1f;
		}
		currentBookId = instanceId;
		if (noScan || (scannedBooks.ContainsKey(instanceId) && scannedBooks[instanceId]))
		{
			scanningPanel.SetActive(value: false);
			readingPanel.SetActive(value: true);
			scanning = false;
		}
		else
		{
			scanningPanel.SetActive(value: true);
			readingPanel.SetActive(value: false);
			scanning = true;
			loading = 0f;
			meter.fillAmount = 0f;
		}
	}

	public void ResetState()
	{
		if (bookScrollStates.ContainsKey(currentBookId))
		{
			bookScrollStates[currentBookId] = scrollRect.verticalNormalizedPosition;
		}
		else
		{
			bookScrollStates.Add(currentBookId, scrollRect.verticalNormalizedPosition);
		}
		scanning = false;
		loading = 0f;
		meter.fillAmount = 0f;
		scanningPanel.SetActive(value: false);
		readingPanel.SetActive(value: false);
		currentBookId = -1;
		scrollRect.verticalNormalizedPosition = 1f;
		if (oldWeaponState)
		{
			MonoSingleton<GunControl>.Instance.YesWeapon();
		}
		else
		{
			MonoSingleton<GunControl>.Instance.NoWeapon();
		}
	}

	private void Update()
	{
		if (scanning)
		{
			loading = Mathf.MoveTowards(loading, 1f, Time.deltaTime / 2f);
			meter.fillAmount = loading;
			if (loading == 1f)
			{
				scanning = false;
				scanningPanel.SetActive(value: false);
				readingPanel.SetActive(value: true);
				scannedBooks.Add(currentBookId, value: true);
			}
		}
	}
}
