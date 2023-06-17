using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

[ConfigureSingleton(SingletonFlags.NoAutoInstance | SingletonFlags.DestroyDuplicates)]
public class PresenceController : MonoSingleton<PresenceController>
{
	public string[] diffNames;

	private bool trackingTimeInSandbox;

	private TimeSince timeInSandbox;

	private void Start()
	{
		base.transform.SetParent(null);
		Object.DontDestroyOnLoad(base.gameObject);
		SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
		SceneManagerOnsceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
		if (SteamClient.IsValid)
		{
			SteamUGC.StopPlaytimeTrackingForAllItems();
		}
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;
	}

	public static void UpdateCyberGrindWave(int wave)
	{
		DiscordController.UpdateWave(wave);
		SteamController.Instance.UpdateWave(wave);
	}

	private void SceneManagerOnsceneLoaded(Scene _, LoadSceneMode mode)
	{
		if (mode == LoadSceneMode.Additive)
		{
			return;
		}
		string currentScene = SceneHelper.CurrentScene;
		Debug.Log(currentScene);
		DiscordController.Instance.FetchSceneActivity(currentScene);
		SteamController.Instance.FetchSceneActivity(currentScene);
		if (MapInfoBase.InstanceAnyType != null && MapInfoBase.InstanceAnyType.sandboxTools)
		{
			if (!trackingTimeInSandbox)
			{
				Debug.Log("Starting sandbox time tracking");
				trackingTimeInSandbox = true;
				timeInSandbox = 0f;
			}
		}
		else if (trackingTimeInSandbox)
		{
			Debug.Log("Submitting sandbox time");
			trackingTimeInSandbox = false;
			SteamController.Instance.UpdateTimeInSandbox(timeInSandbox);
		}
	}

	public void AddToStatInt(string statKey, int amount)
	{
		SteamController.Instance.AddToStatInt(statKey, amount);
	}

	private void OnApplicationQuit()
	{
		if (trackingTimeInSandbox)
		{
			SteamController.Instance.UpdateTimeInSandbox(timeInSandbox);
		}
	}
}
