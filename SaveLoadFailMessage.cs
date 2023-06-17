using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class SaveLoadFailMessage : MonoSingleton<SaveLoadFailMessage>
{
	public enum SaveLoadError
	{
		Generic,
		TempValidation
	}

	[SerializeField]
	private GameObject saveMergeConsentPanel;

	[SerializeField]
	private Text rootMergeOptionBtnText;

	[SerializeField]
	private Text slotOneMergeOptionBtnText;

	[SerializeField]
	private GameObject errorGeneric;

	[SerializeField]
	private GameObject errorTempValidation;

	[SerializeField]
	private Text tempErrorCode;

	private SaveLoadError currentError;

	private bool beenActivated;

	private Action potentialSaveRedo;

	private static bool consentQueued;

	private static SaveSlotMenu.SlotData queuedRootSlot;

	private static SaveSlotMenu.SlotData queuedSlotOneData;

	public static void DisplayMergeConsent(SaveSlotMenu.SlotData rootSlot, SaveSlotMenu.SlotData slotOneData)
	{
		if (!MonoSingleton<SaveLoadFailMessage>.Instance)
		{
			queuedRootSlot = rootSlot;
			queuedSlotOneData = slotOneData;
			consentQueued = true;
		}
		else
		{
			MonoSingleton<SaveLoadFailMessage>.Instance.DisplayMergeConsentInstance(rootSlot, slotOneData);
		}
	}

	private void DisplayMergeConsentInstance(SaveSlotMenu.SlotData rootSlot, SaveSlotMenu.SlotData slotOneData)
	{
		rootMergeOptionBtnText.text = $"<b>Saves{Path.DirectorySeparatorChar}</b>\n{rootSlot}";
		slotOneMergeOptionBtnText.text = $"<b>Saves{Path.DirectorySeparatorChar}Slot1{Path.DirectorySeparatorChar}</b>\n{slotOneData}";
		saveMergeConsentPanel.SetActive(value: true);
	}

	private new void OnEnable()
	{
		if (consentQueued)
		{
			consentQueued = false;
			DisplayMergeConsentInstance(queuedRootSlot, queuedSlotOneData);
		}
	}

	public void ConfirmMergeRoot()
	{
		GameProgressSaver.MergeRootWithSlotOne(keepRoot: true);
		SceneManager.LoadScene("Main Menu");
	}

	public void ConfirmMergeFirstSlot()
	{
		GameProgressSaver.MergeRootWithSlotOne(keepRoot: false);
		SceneManager.LoadScene("Main Menu");
	}

	public void QuitGame()
	{
		if (!SceneHelper.IsPlayingCustom)
		{
			Application.Quit();
		}
	}

	public void ShowError(SaveLoadError error, string errorCode, Action saveRedo)
	{
		currentError = error;
		switch (error)
		{
		case SaveLoadError.Generic:
			errorGeneric.SetActive(value: true);
			break;
		case SaveLoadError.TempValidation:
			potentialSaveRedo = saveRedo;
			errorTempValidation.SetActive(value: true);
			if (!string.IsNullOrEmpty(errorCode))
			{
				tempErrorCode.text = errorCode;
				tempErrorCode.gameObject.SetActive(value: true);
			}
			break;
		}
		beenActivated = true;
	}

	private void HideAll()
	{
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(value: false);
		}
		beenActivated = false;
	}

	private void Update()
	{
		if (!beenActivated)
		{
			return;
		}
		switch (currentError)
		{
		case SaveLoadError.Generic:
			if (Input.GetKeyDown(KeyCode.Y))
			{
				HideAll();
			}
			if (Input.GetKeyDown(KeyCode.N))
			{
				Application.Quit();
			}
			break;
		case SaveLoadError.TempValidation:
			if (Input.GetKeyDown(KeyCode.Y))
			{
				if (potentialSaveRedo == null)
				{
					break;
				}
				HideAll();
				potentialSaveRedo();
			}
			if (Input.GetKeyDown(KeyCode.N))
			{
				HideAll();
				MonoSingleton<ProgressChecker>.Instance.DisableSaving();
			}
			break;
		}
	}
}
