using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Steamworks.Data;
using Steamworks.Ugc;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance | SingletonFlags.PersistAutoInstance | SingletonFlags.DestroyDuplicates)]
public class SceneHelper : MonoSingleton<SceneHelper>
{
	[SerializeField]
	private string finalRoomPit;

	[SerializeField]
	private GameObject loadingBlocker;

	[SerializeField]
	private Text loadingBar;

	[SerializeField]
	private GameObject preloadingBadge;

	[SerializeField]
	private GameObject eventSystem;

	[Space]
	[SerializeField]
	private AudioMixerGroup masterMixer;

	[SerializeField]
	private AudioMixerGroup musicMixer;

	[SerializeField]
	private AudioMixer allSound;

	[SerializeField]
	private AudioMixer goreSound;

	[SerializeField]
	private AudioMixer musicSound;

	[SerializeField]
	private AudioMixer doorSound;

	[SerializeField]
	private AudioMixer unfreezeableSound;

	[Space]
	[SerializeField]
	private EmbeddedSceneInfo embeddedSceneInfo;

	public static bool IsPlayingCustom => GameStateManager.Instance.currentCustomGame != null;

	public static int CurrentLevelNumber
	{
		get
		{
			if (!IsPlayingCustom)
			{
				return MonoSingleton<StatsManager>.Instance.levelNumber;
			}
			return GameStateManager.Instance.currentCustomGame.levelNumber;
		}
	}

	public static string CurrentScene { get; private set; }

	public static string LastScene { get; private set; }

	public static string PendingScene { get; private set; }

	protected override void OnEnable()
	{
		base.OnEnable();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		SceneManager.sceneLoaded += OnSceneLoaded;
		OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
		if (string.IsNullOrEmpty(CurrentScene))
		{
			CurrentScene = SceneManager.GetActiveScene().name;
		}
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	public bool IsSceneSpecial(string sceneName)
	{
		sceneName = SanitizeLevelPath(sceneName);
		if (embeddedSceneInfo == null)
		{
			return false;
		}
		return embeddedSceneInfo.specialScenes.Contains(sceneName);
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (EventSystem.current != null)
		{
			UnityEngine.Object.Destroy(EventSystem.current.gameObject);
		}
		UnityEngine.Object.Instantiate(eventSystem);
		if (mode != 0)
		{
			return;
		}
		if (IsPlayingCustom)
		{
			Debug.Log("SceneHelper: Setting up custom scene");
			MapInfo mapInfo = UnityEngine.Object.FindObjectOfType<MapInfo>();
			if ((bool)mapInfo && mapInfo.renderSkybox)
			{
				MonoSingleton<CameraController>.Instance.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
			}
			GameObject[] rootGameObjects = scene.GetRootGameObjects();
			List<PlaceholderPrefab> list = new List<PlaceholderPrefab>();
			GameObject[] array = rootGameObjects;
			foreach (GameObject gameObject in array)
			{
				list.AddRange(gameObject.GetComponentsInChildren<PlaceholderPrefab>(includeInactive: true));
				AudioSource[] componentsInChildren = gameObject.GetComponentsInChildren<AudioSource>(includeInactive: true);
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].outputAudioMixerGroup = masterMixer;
				}
			}
			if ((bool)MonoSingleton<MusicManager>.Instance)
			{
				if ((bool)MonoSingleton<MusicManager>.Instance.battleTheme)
				{
					MonoSingleton<MusicManager>.Instance.battleTheme.outputAudioMixerGroup = musicMixer;
				}
				if ((bool)MonoSingleton<MusicManager>.Instance.cleanTheme)
				{
					MonoSingleton<MusicManager>.Instance.cleanTheme.outputAudioMixerGroup = musicMixer;
				}
				if ((bool)MonoSingleton<MusicManager>.Instance.bossTheme)
				{
					MonoSingleton<MusicManager>.Instance.bossTheme.outputAudioMixerGroup = musicMixer;
				}
			}
			if ((bool)MonoSingleton<AudioMixerController>.Instance)
			{
				MonoSingleton<AudioMixerController>.Instance.allSound = allSound;
				MonoSingleton<AudioMixerController>.Instance.goreSound = goreSound;
				MonoSingleton<AudioMixerController>.Instance.musicSound = musicSound;
				MonoSingleton<AudioMixerController>.Instance.doorSound = doorSound;
				MonoSingleton<AudioMixerController>.Instance.unfreezeableSound = unfreezeableSound;
			}
		}
		else
		{
			MonoSingleton<AgonyController>.Instance.ResetAgony();
		}
	}

	public static string SanitizeLevelPath(string scene)
	{
		if (scene.StartsWith("Assets/Scenes/"))
		{
			scene = scene.Substring("Assets/Scenes/".Length);
		}
		if (scene.EndsWith(".unity"))
		{
			scene = scene.Substring(0, scene.Length - ".unity".Length);
		}
		return scene;
	}

	public static void ShowLoadingBlocker()
	{
		MonoSingleton<SceneHelper>.Instance.loadingBlocker.SetActive(value: true);
	}

	public static void DismissBlockers()
	{
		MonoSingleton<SceneHelper>.Instance.loadingBlocker.SetActive(value: false);
		MonoSingleton<SceneHelper>.Instance.loadingBar.gameObject.SetActive(value: false);
	}

	public static void LoadWorkshopMap(Item item, Action failureCallback = null)
	{
		MonoSingleton<SceneHelper>.Instance.StartCoroutine(MonoSingleton<SceneHelper>.Instance.LoadWorkshopMapAsync(item, failureCallback));
	}

	private IEnumerator LoadWorkshopMapAsync(Item? item, Action failureCallback = null)
	{
		PublishedFileId workshopId = item.Value.Id;
		bool alreadyDownloaded = item.Value.IsInstalled;
		Debug.Log("(LoadWorkshopMap) Loading workshop map " + workshopId);
		loadingBlocker.SetActive(value: true);
		if (!alreadyDownloaded)
		{
			loadingBar.gameObject.SetActive(value: true);
			loadingBar.text = "Downloading...";
		}
		yield return null;
		Task<Item?> workshopTask = WorkshopHelper.DownloadWorkshopMap(workshopId);
		while (!workshopTask.IsCompleted)
		{
			if (!alreadyDownloaded)
			{
				long downloadBytesTotal = item.Value.DownloadBytesTotal;
				loadingBar.text = $"Downloading... ({(float)downloadBytesTotal / 1024f / 1024f:0.00} MB)";
			}
			yield return null;
		}
		item = workshopTask.Result;
		bool num = item.Value.Tags.Contains("campaign");
		CustomGameDetails customGameDetails = new CustomGameDetails
		{
			workshopId = item.Value.Id.Value
		};
		string path;
		if (num)
		{
			CustomCampaign customCampaign = new CustomCampaign(item.Value.Directory + "campaign.json");
			path = Path.Combine(item.Value.Directory, customCampaign.levels[0].file);
			customGameDetails.campaign = customCampaign.json;
			customGameDetails.levelNumber = 1;
		}
		else
		{
			path = Directory.GetFiles(item.Value.Directory)[0];
			customGameDetails.levelNumber = 1;
		}
		yield return LoadCustomMapAsync(path, customGameDetails, failureCallback);
	}

	public static void LoadCustomMap(string path, CustomGameDetails gameDetails, Action failureCallback = null, bool isLocal = false)
	{
		if (isLocal)
		{
			MonoSingleton<AgonyController>.Instance.CustomLocalMapLoaded(path);
		}
		else
		{
			MonoSingleton<AgonyController>.Instance.ResetAgony();
		}
		MonoSingleton<SceneHelper>.Instance.StartCoroutine(MonoSingleton<SceneHelper>.Instance.LoadCustomMapAsync(path, gameDetails, failureCallback));
	}

	public IEnumerator LoadCustomMapAsync(string path, CustomGameDetails gameDetails, Action failureCallback = null)
	{
		Debug.Log("(LoadCustomMap) Loading custom map " + path);
		loadingBlocker.SetActive(value: true);
		SetLoadingSubtext("Draining blood...");
		yield return null;
		MapDataRebuild mapData = AgonyHelper.LoadMapData(path, withBundle: true);
		if (mapData == null)
		{
			Debug.LogError("Failed to load map data for " + path);
			yield break;
		}
		gameDetails.uniqueIdentifier = mapData.uniqueId;
		GameStateManager.Instance.currentCustomGame = gameDetails;
		BloodMapInstance mapInstance;
		try
		{
			mapInstance = new BloodMapInstance(mapData.catalog, mapData.bundle, mapData.bundleName);
		}
		catch (Exception message)
		{
			Debug.LogError("Failed to load map instance for " + path);
			Debug.LogError(message);
			loadingBlocker.SetActive(value: false);
			loadingBar.gameObject.SetActive(value: false);
			yield break;
		}
		gameDetails.mapInstance = mapInstance;
		Debug.Log("Loading catalog from blood");
		AsyncOperationHandle<IResourceLocator> locatorOperation = Addressables.LoadContentCatalogAsync(BloodMapInstance.CatalogPath);
		yield return locatorOperation;
		if (locatorOperation.Result == null)
		{
			Debug.LogError("Failed to load catalog");
			yield break;
		}
		if (CurrentScene != mapData.uniqueId)
		{
			LastScene = CurrentScene;
		}
		SetLoadingSubtext("Loading scene...");
		Debug.Log("Loading scene from blood");
		yield return Addressables.LoadSceneAsync(mapData.uniqueId);
		CurrentScene = mapData.uniqueId;
		loadingBlocker.SetActive(value: false);
		loadingBar.gameObject.SetActive(value: false);
	}

	public static void LoadScene(string sceneName, bool noBlocker = false)
	{
		MonoSingleton<SceneHelper>.Instance.StartCoroutine(MonoSingleton<SceneHelper>.Instance.LoadSceneAsync(sceneName, noBlocker));
	}

	private IEnumerator LoadSceneAsync(string sceneName, bool noSplash = false)
	{
		if (PendingScene == sceneName)
		{
			yield break;
		}
		PendingScene = sceneName;
		sceneName = SanitizeLevelPath(sceneName);
		switch (sceneName)
		{
		case "Main Menu":
		case "Tutorial":
		case "Credits":
		case "Endless":
		case "Custom Content":
			if (IsPlayingCustom)
			{
				GameStateManager.Instance.currentCustomGame?.mapInstance?.Close();
				GameStateManager.Instance.currentCustomGame = null;
			}
			break;
		}
		Debug.Log("(LoadSceneAsync) Loading scene " + sceneName);
		loadingBlocker.SetActive(!noSplash);
		yield return null;
		if (CurrentScene != sceneName)
		{
			LastScene = CurrentScene;
		}
		CurrentScene = sceneName;
		yield return Addressables.LoadSceneAsync(sceneName);
		if ((bool)GameStateManager.Instance)
		{
			GameStateManager.Instance.currentCustomGame = null;
		}
		if ((bool)preloadingBadge)
		{
			preloadingBadge.SetActive(value: false);
		}
		if ((bool)loadingBlocker)
		{
			loadingBlocker.SetActive(value: false);
		}
		if ((bool)loadingBar)
		{
			loadingBar.gameObject.SetActive(value: false);
		}
		PendingScene = null;
	}

	public static void RestartScene()
	{
		MonoBehaviour[] array = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in array)
		{
			if (!(monoBehaviour == null) && !(monoBehaviour.gameObject.scene.name == "DontDestroyOnLoad"))
			{
				monoBehaviour.enabled = false;
			}
		}
		if (string.IsNullOrEmpty(CurrentScene))
		{
			CurrentScene = SceneManager.GetActiveScene().name;
		}
		Addressables.LoadSceneAsync(CurrentScene).WaitForCompletion();
	}

	public static void LoadPreviousScene()
	{
		string text = LastScene;
		if (string.IsNullOrEmpty(text))
		{
			text = "Main Menu";
		}
		LoadScene(text);
	}

	public static void SpawnFinalPitAndFinish()
	{
		FinalRoom finalRoom = UnityEngine.Object.FindObjectOfType<FinalRoom>();
		if (finalRoom != null)
		{
			if ((bool)finalRoom.doorOpener)
			{
				finalRoom.doorOpener.SetActive(value: true);
			}
			MonoSingleton<NewMovement>.Instance.transform.position = finalRoom.dropPoint.position;
		}
		else
		{
			GameObject obj = UnityEngine.Object.Instantiate(AssetHelper.LoadPrefab(MonoSingleton<SceneHelper>.Instance.finalRoomPit));
			finalRoom = obj.GetComponent<FinalRoom>();
			obj.transform.position = new Vector3(50000f, -1000f, 50000f);
			MonoSingleton<NewMovement>.Instance.transform.position = finalRoom.dropPoint.position;
		}
	}

	public static void SetLoadingSubtext(string text)
	{
		if ((bool)MonoSingleton<SceneHelper>.Instance.loadingBlocker)
		{
			MonoSingleton<SceneHelper>.Instance.loadingBar.gameObject.SetActive(value: true);
			MonoSingleton<SceneHelper>.Instance.loadingBar.text = text;
		}
	}
}
