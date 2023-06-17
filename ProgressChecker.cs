using System;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class ProgressChecker : MonoSingleton<ProgressChecker>
{
	public bool continueWithoutSaving;

	protected override void Awake()
	{
		base.Awake();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (!GameProgressSaver.GetTutorial() || !GameProgressSaver.GetIntro())
		{
			MonoSingleton<PrefsManager>.Instance.SetInt("weapon.arm0", 1);
			SceneHelper.LoadScene("Tutorial");
			continueWithoutSaving = false;
		}
	}

	public void DisableSaving()
	{
		continueWithoutSaving = true;
	}

	public void SaveLoadError(SaveLoadFailMessage.SaveLoadError error = SaveLoadFailMessage.SaveLoadError.Generic, string tempValidationError = "", Action saveRedo = null)
	{
		if (!continueWithoutSaving)
		{
			if (error == SaveLoadFailMessage.SaveLoadError.Generic)
			{
				continueWithoutSaving = true;
			}
			MonoSingleton<SaveLoadFailMessage>.Instance.ShowError(error, tempValidationError, saveRedo);
		}
	}
}
