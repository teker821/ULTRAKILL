using System.Linq;
using Logic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class OptionsManager : MonoSingleton<OptionsManager>
{
	public bool mainMenu;

	[HideInInspector]
	public bool paused;

	public bool inIntro;

	public bool frozen;

	[HideInInspector]
	public GameObject pauseMenu;

	[HideInInspector]
	public GameObject optionsMenu;

	[HideInInspector]
	public GameObject progressChecker;

	private NewMovement nm;

	private GunControl gc;

	private FistControl fc;

	[HideInInspector]
	public float mouseSensitivity;

	[HideInInspector]
	public float simplifiedDistance;

	[HideInInspector]
	public bool simplifyEnemies;

	[HideInInspector]
	public bool outlinesOnly;

	private int screenWidth;

	private int screenHeight;

	[HideInInspector]
	public Toggle fullScreen;

	[HideInInspector]
	public float bloodstainChance;

	[HideInInspector]
	public float maxGore;

	[HideInInspector]
	public GameObject playerPosInfo;

	[HideInInspector]
	public bool dontUnpause;

	public bool previousWeaponState;

	public static bool forceRadiance;

	public static float radianceTier = 1f;

	protected override void Awake()
	{
		base.Awake();
		if (GameObject.FindWithTag("OptionsManager") == null)
		{
			Object.Instantiate(progressChecker);
		}
		base.transform.SetParent(null);
	}

	private void Start()
	{
		if (!MonoSingleton<CheatsController>.Instance.cheatsEnabled)
		{
			if (forceRadiance)
			{
				forceRadiance = false;
			}
			if (radianceTier != 1f)
			{
				radianceTier = 1f;
			}
		}
	}

	private void Update()
	{
		if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.Return))
		{
			if (!Screen.fullScreen)
			{
				Screen.SetResolution(Screen.resolutions.Last().width, Screen.resolutions.Last().height, fullscreen: true);
			}
			else
			{
				Screen.fullScreen = false;
			}
		}
		if (frozen)
		{
			return;
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Pause.WasPerformedThisFrame && !inIntro && !mainMenu)
		{
			if (!paused)
			{
				Pause();
			}
			else if (!dontUnpause)
			{
				if (SandboxHud.SavesMenuOpen)
				{
					Debug.Log("Closing sandbox saves menu first");
					MonoSingleton<SandboxHud>.Instance.HideSavesMenu();
					return;
				}
				CloseOptions();
				UnPause();
			}
		}
		if (mainMenu && !paused)
		{
			Pause();
		}
		if (paused)
		{
			if (mainMenu)
			{
				Time.timeScale = 1f;
			}
			else
			{
				Time.timeScale = 0f;
			}
		}
	}

	public void Pause()
	{
		if (nm == null)
		{
			nm = MonoSingleton<NewMovement>.Instance;
			gc = nm.GetComponentInChildren<GunControl>();
			fc = nm.GetComponentInChildren<FistControl>();
		}
		if (!mainMenu)
		{
			nm.enabled = false;
			MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allPitch", 0f);
			MonoSingleton<AudioMixerController>.Instance.doorSound.SetFloat("allPitch", 0f);
			if ((bool)MonoSingleton<MusicManager>.Instance)
			{
				MonoSingleton<MusicManager>.Instance.FilterMusic();
			}
		}
		GameStateManager.Instance.RegisterState(new GameState("pause", new GameObject[2] { pauseMenu, optionsMenu })
		{
			cursorLock = LockMode.Unlock,
			cameraInputLock = LockMode.Lock,
			playerInputLock = LockMode.Lock
		});
		MonoSingleton<CameraController>.Instance.activated = false;
		gc.activated = false;
		paused = true;
		if ((bool)pauseMenu)
		{
			pauseMenu.SetActive(value: true);
		}
		VideoPlayer[] array = Object.FindObjectsOfType<VideoPlayer>();
		foreach (VideoPlayer videoPlayer in array)
		{
			if (videoPlayer.isPlaying)
			{
				videoPlayer.Pause();
			}
		}
	}

	public void UnPause()
	{
		if (nm == null)
		{
			nm = MonoSingleton<NewMovement>.Instance;
			gc = nm.GetComponentInChildren<GunControl>();
			fc = nm.GetComponentInChildren<FistControl>();
		}
		CloseOptions();
		paused = false;
		Time.timeScale = MonoSingleton<TimeController>.Instance.timeScale * MonoSingleton<TimeController>.Instance.timeScaleModifier;
		MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allPitch", 1f);
		MonoSingleton<AudioMixerController>.Instance.doorSound.SetFloat("allPitch", 1f);
		if ((bool)MonoSingleton<MusicManager>.Instance)
		{
			MonoSingleton<MusicManager>.Instance.UnfilterMusic();
		}
		if (!nm.dead)
		{
			nm.enabled = true;
			MonoSingleton<CameraController>.Instance.activated = true;
			if (!fc || !fc.shopping)
			{
				if (!gc.stayUnarmed)
				{
					gc.activated = true;
				}
				if (fc != null)
				{
					fc.activated = true;
				}
			}
		}
		if ((bool)pauseMenu)
		{
			pauseMenu.SetActive(value: false);
		}
		VideoPlayer[] array = Object.FindObjectsOfType<VideoPlayer>();
		foreach (VideoPlayer videoPlayer in array)
		{
			if (videoPlayer.isPaused)
			{
				videoPlayer.Play();
			}
		}
	}

	public void Freeze()
	{
		frozen = true;
		if (nm == null)
		{
			nm = MonoSingleton<NewMovement>.Instance;
			gc = nm.GetComponentInChildren<GunControl>();
			fc = nm.GetComponentInChildren<FistControl>();
		}
		MonoSingleton<CameraController>.Instance.activated = false;
		previousWeaponState = !gc.noWeapons;
		gc.NoWeapon();
		gc.enabled = false;
	}

	public void UnFreeze()
	{
		frozen = false;
		if (nm == null)
		{
			nm = MonoSingleton<NewMovement>.Instance;
			gc = nm.GetComponentInChildren<GunControl>();
			fc = nm.GetComponentInChildren<FistControl>();
		}
		MonoSingleton<CameraController>.Instance.activated = true;
		if (previousWeaponState)
		{
			gc.YesWeapon();
		}
		gc.enabled = true;
	}

	public void RestartCheckpoint()
	{
		UnPause();
		StatsManager statsManager = MonoSingleton<StatsManager>.Instance;
		if (!statsManager.infoSent)
		{
			statsManager.Restart();
		}
	}

	public void RestartMission()
	{
		Time.timeScale = 1f;
		SceneHelper.RestartScene();
		if ((bool)MonoSingleton<MapVarManager>.Instance)
		{
			MonoSingleton<MapVarManager>.Instance.ResetStores();
		}
		Object.Destroy(base.gameObject);
	}

	public void OpenOptions()
	{
		pauseMenu.SetActive(value: false);
		optionsMenu.SetActive(value: true);
	}

	public void CloseOptions()
	{
		optionsMenu.SetActive(value: false);
		if ((bool)MonoSingleton<CheatsManager>.Instance)
		{
			MonoSingleton<CheatsManager>.Instance.HideMenu();
		}
		pauseMenu.SetActive(value: true);
	}

	public void QuitMission()
	{
		Time.timeScale = 1f;
		if (CustomContentGui.wasAgonyOpen)
		{
			SceneHelper.LoadScene("Custom Content");
		}
		else
		{
			SceneHelper.LoadScene("Main Menu");
		}
	}

	public void QuitGame()
	{
		if (!SceneHelper.IsPlayingCustom)
		{
			Application.Quit();
		}
	}

	public void ChangeLevel(string levelname)
	{
		SetChangeLevelPosition(noPosition: true);
		SceneHelper.LoadScene(levelname);
	}

	public void ChangeLevelAbrupt(string scene)
	{
		SceneHelper.LoadScene(scene);
	}

	public void ChangeLevelWithPosition(string levelname)
	{
		if (Application.CanStreamedLevelBeLoaded(levelname))
		{
			SetChangeLevelPosition(noPosition: false);
			SceneManager.LoadScene(levelname);
		}
		else
		{
			SceneHelper.LoadScene("Main Menu");
		}
	}

	public void SetChangeLevelPosition(bool noPosition)
	{
		if (nm == null)
		{
			nm = MonoSingleton<NewMovement>.Instance;
		}
		PlayerPosInfo component = Object.Instantiate(playerPosInfo).GetComponent<PlayerPosInfo>();
		component.velocity = nm.GetComponent<Rigidbody>().velocity;
		component.wooshTime = nm.GetComponentInChildren<WallCheck>().GetComponent<AudioSource>().time;
		component.noPosition = noPosition;
	}
}
