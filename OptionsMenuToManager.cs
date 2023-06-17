using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class OptionsMenuToManager : MonoSingleton<OptionsMenuToManager>
{
	public GameObject pauseMenu;

	public GameObject optionsMenu;

	public Slider mouseSensitivitySlider;

	private bool ignoreSensitivitySliderChange;

	public Toggle reverseMouseX;

	public Toggle reverseMouseY;

	public Slider simplifiedDistanceSlider;

	public GameObject simplifiedDistanceGroup;

	public Toggle simplifyEnemies;

	public Toggle outlinesOnly;

	public Slider outlineThickness;

	public Slider screenShakeSlider;

	public Toggle cameraTilt;

	public Toggle discordIntegration;

	[FormerlySerializedAs("steamLeaderboards")]
	public Toggle levelLeaderboards;

	public Toggle seasonalEvents;

	public Toggle restartWarning;

	public Dropdown resolutionDropdown;

	private Resolution[] resolutions;

	private List<Resolution> availableResolutions = new List<Resolution>();

	public Toggle fullScreen;

	public Dropdown framerateLimiter;

	public Toggle simplerExplosions;

	public Toggle simplerFire;

	public Toggle simplerSpawns;

	public Toggle noEnviroParts;

	[SerializeField]
	private Toggle simpleNailPhysicsToggle;

	[HideInInspector]
	public static bool simpleNailPhysics = true;

	public Toggle bloodAndGore;

	[HideInInspector]
	public static bool bloodEnabled = true;

	public Toggle freezeGore;

	public Slider bloodstainChanceSlider;

	public Slider maxGoreSlider;

	public Toggle subtitles;

	public Slider masterVolume;

	public Slider musicVolume;

	private OptionsManager opm;

	private MusicManager muman;

	private Camera mainCam;

	private CameraController cc;

	public Slider fovSlider;

	public Dropdown weaponPosDropdown;

	public Toggle variationMemory;

	private List<string> options;

	public AudioClip normalJump;

	public AudioClip quakeJump;

	public bool selectedSomethingThisFrame;

	[Space]
	public BasicConfirmationDialog quitDialog;

	public BasicConfirmationDialog resetDialog;

	private NewMovement nmov;

	private void LateUpdate()
	{
		selectedSomethingThisFrame = false;
	}

	private void Start()
	{
		SetPauseMenu();
		framerateLimiter.value = MonoSingleton<PrefsManager>.Instance.GetIntLocal("frameRateLimit");
		framerateLimiter.RefreshShownValue();
		simplerExplosions.isOn = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("simpleExplosions");
		Physics.IgnoreLayerCollision(23, 9, simplerExplosions.isOn);
		Physics.IgnoreLayerCollision(23, 27, simplerExplosions.isOn);
		fullScreen.isOn = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("fullscreen");
		simplerFire.isOn = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("simpleFire");
		simplerSpawns.isOn = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("simpleSpawns");
		noEnviroParts.isOn = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("disableEnvironmentParticles");
		if (MonoSingleton<PrefsManager>.Instance.GetBoolLocal("simpleNailPhysics"))
		{
			simpleNailPhysicsToggle.isOn = true;
		}
		else
		{
			simpleNailPhysics = false;
		}
		if (!MonoSingleton<PrefsManager>.Instance.GetBoolLocal("bloodEnabled"))
		{
			bloodAndGore.isOn = false;
			bloodEnabled = false;
		}
		freezeGore.isOn = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("freezeGore");
		if (MonoSingleton<PrefsManager>.Instance.GetBool("simplifyEnemies"))
		{
			simplifyEnemies.isOn = true;
			opm.simplifyEnemies = true;
			Shader.SetGlobalFloat("_UseEdgeDetection", 1f);
		}
		else
		{
			Shader.SetGlobalFloat("_UseEdgeDetection", 0f);
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBool("enemyOutlinesOnly"))
		{
			outlinesOnly.isOn = true;
			opm.outlinesOnly = true;
		}
		opm.simplifiedDistance = MonoSingleton<PrefsManager>.Instance.GetFloat("simplifyEnemiesDistance");
		simplifiedDistanceSlider.value = opm.simplifiedDistance;
		MonoSingleton<PostProcessV2_Handler>.Instance.distance = MonoSingleton<PrefsManager>.Instance.GetInt("outlineThickness", 1);
		outlineThickness.value = MonoSingleton<PostProcessV2_Handler>.Instance.distance;
		opm.mouseSensitivity = MonoSingleton<PrefsManager>.Instance.GetFloatLocal("mouseSensitivity");
		UpdateSensitivitySlider(opm.mouseSensitivity);
		reverseMouseX.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("mouseReverseX");
		reverseMouseY.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("mouseReverseY");
		screenShakeSlider.value = MonoSingleton<PrefsManager>.Instance.GetFloat("screenShake") * 100f;
		cameraTilt.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("cameraTilt");
		discordIntegration.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("discordIntegration");
		levelLeaderboards.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("levelLeaderboards");
		seasonalEvents.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("seasonalEvents");
		restartWarning.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("pauseMenuConfirmationDialogs");
		opm.bloodstainChance = MonoSingleton<PrefsManager>.Instance.GetFloatLocal("bloodStainChance") * 100f;
		bloodstainChanceSlider.value = opm.bloodstainChance;
		opm.maxGore = MonoSingleton<PrefsManager>.Instance.GetFloatLocal("maxGore");
		maxGoreSlider.value = opm.maxGore / 100f;
		subtitles.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("subtitlesEnabled");
		if ((bool)MonoSingleton<SubtitleController>.Instance)
		{
			MonoSingleton<SubtitleController>.Instance.subtitlesEnabled = subtitles.isOn;
		}
		if (GameProgressSaver.GetIntro())
		{
			AudioListener.volume = MonoSingleton<PrefsManager>.Instance.GetFloat("allVolume");
			masterVolume.value = AudioListener.volume * 100f;
			if ((bool)MonoSingleton<AudioMixerController>.Instance)
			{
				float @float = MonoSingleton<PrefsManager>.Instance.GetFloat("musicVolume");
				MonoSingleton<AudioMixerController>.Instance.SetMusicVolume(@float);
				musicVolume.value = @float * 100f;
			}
		}
		resolutions = Screen.resolutions;
		availableResolutions.Clear();
		resolutionDropdown.ClearOptions();
		options = new List<string>();
		int valueWithoutNotify = 0;
		for (int i = 0; i < resolutions.Length; i++)
		{
			string item = resolutions[i].width + " x " + resolutions[i].height;
			if (!options.Contains(item))
			{
				options.Add(item);
				availableResolutions.Add(resolutions[i]);
				if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
				{
					valueWithoutNotify = availableResolutions.Count - 1;
				}
			}
		}
		resolutionDropdown.AddOptions(options);
		resolutionDropdown.SetValueWithoutNotify(valueWithoutNotify);
		resolutionDropdown.RefreshShownValue();
		weaponPosDropdown.value = MonoSingleton<PrefsManager>.Instance.GetInt("weaponHoldPosition");
		weaponPosDropdown.RefreshShownValue();
		variationMemory.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("variationMemory");
		nmov = MonoSingleton<NewMovement>.Instance;
		cc = MonoSingleton<CameraController>.Instance;
		mainCam = cc.GetComponent<Camera>();
		cc.defaultFov = MonoSingleton<PrefsManager>.Instance.GetFloat("fieldOfView");
		mainCam.fieldOfView = cc.defaultFov;
		fovSlider.value = cc.defaultFov;
	}

	private new void OnEnable()
	{
		SetPauseMenu();
	}

	private void SetPauseMenu()
	{
		opm = MonoSingleton<OptionsManager>.Instance;
		muman = MonoSingleton<MusicManager>.Instance;
		if ((bool)opm.pauseMenu)
		{
			if (opm.pauseMenu == pauseMenu)
			{
				return;
			}
			opm.pauseMenu.SetActive(value: false);
			opm.optionsMenu.SetActive(value: false);
		}
		opm.pauseMenu = pauseMenu;
		opm.optionsMenu = optionsMenu;
	}

	public void OpenAgony()
	{
		SceneHelper.LoadScene("Custom Content");
	}

	public void EnableGamepadLookAndMove()
	{
		EnableGamepadLook();
		EnableGamepadMove();
	}

	public void DisableGamepadLookAndMove()
	{
		DisableGamepadLook();
		DisableGamepadMove();
	}

	public void EnableGamepadMove()
	{
		if (MonoSingleton<NewMovement>.Instance.gamepadFreezeCount > 0)
		{
			MonoSingleton<NewMovement>.Instance.gamepadFreezeCount--;
		}
	}

	public void EnableGamepadLook()
	{
		if (MonoSingleton<CameraController>.Instance.gamepadFreezeCount > 0)
		{
			MonoSingleton<CameraController>.Instance.gamepadFreezeCount--;
		}
	}

	public void DisableGamepadMove()
	{
		MonoSingleton<NewMovement>.Instance.gamepadFreezeCount++;
	}

	public void DisableGamepadLook()
	{
		MonoSingleton<CameraController>.Instance.gamepadFreezeCount++;
	}

	public void SetSelected(Selectable selectable)
	{
		selectedSomethingThisFrame = true;
		EventSystem.current.SetSelectedGameObject(selectable.gameObject);
	}

	public void ResolutionChange(int stuff)
	{
		Resolution resolution = availableResolutions[stuff];
		Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
		MonoSingleton<PrefsManager>.Instance.SetIntLocal("resolutionWidth", resolution.width);
		MonoSingleton<PrefsManager>.Instance.SetIntLocal("resolutionHeight", resolution.height);
		MonoSingleton<PrefsManager>.Instance.SetBoolLocal("fullscreen", Screen.fullScreen);
		MonoSingleton<CameraController>.Instance.CheckAspectRatio();
	}

	public void SetFullScreen(bool stuff)
	{
		Screen.fullScreen = stuff;
		MonoSingleton<PrefsManager>.Instance.SetBoolLocal("fullscreen", stuff);
	}

	public void FrameRateLimiter(int stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetIntLocal("frameRateLimit", stuff);
		switch (stuff)
		{
		case 0:
			Application.targetFrameRate = -1;
			break;
		case 1:
			Application.targetFrameRate = Screen.currentResolution.refreshRate * 2;
			break;
		case 2:
			Application.targetFrameRate = 30;
			break;
		case 3:
			Application.targetFrameRate = 60;
			break;
		case 4:
			Application.targetFrameRate = 120;
			break;
		case 5:
			Application.targetFrameRate = 144;
			break;
		case 6:
			Application.targetFrameRate = 240;
			break;
		case 7:
			Application.targetFrameRate = 288;
			break;
		}
	}

	public void Pause()
	{
		opm.Pause();
	}

	public void UnPause()
	{
		opm.UnPause();
	}

	public void RestartCheckpoint()
	{
		opm.RestartCheckpoint();
	}

	public void RestartMission()
	{
		if (MonoSingleton<PrefsManager>.Instance.GetBool("pauseMenuConfirmationDialogs"))
		{
			resetDialog.ShowDialog();
		}
		else
		{
			RestartMissionNoConfirm();
		}
	}

	public void RestartMissionNoConfirm()
	{
		opm.RestartMission();
	}

	public void OpenOptions()
	{
		opm.OpenOptions();
	}

	public void CloseOptions()
	{
		opm.CloseOptions();
	}

	public void QuitMission()
	{
		if (MonoSingleton<PrefsManager>.Instance.GetBool("pauseMenuConfirmationDialogs"))
		{
			quitDialog.ShowDialog();
		}
		else
		{
			QuitMissionNoConfirm();
		}
	}

	public void QuitMissionNoConfirm()
	{
		opm.QuitMission();
	}

	public void QuitGame()
	{
		opm.QuitGame();
	}

	public void CheckIfTutorialBeaten()
	{
		if (!GameProgressSaver.GetTutorial())
		{
			SceneHelper.LoadScene("Tutorial");
		}
	}

	public void ChangeLevel(string levelname)
	{
		opm.ChangeLevel(levelname);
	}

	public void SimpleExplosions(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBoolLocal("simpleExplosions", stuff);
		Physics.IgnoreLayerCollision(23, 9, stuff);
		Physics.IgnoreLayerCollision(23, 27, stuff);
	}

	public void SimpleFire(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBoolLocal("simpleFire", stuff);
	}

	public void SimpleSpawns(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBoolLocal("simpleSpawns", stuff);
	}

	public void DisableEnviroParts(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBoolLocal("disableEnvironmentParticles", stuff);
		EnviroParticle[] array = Object.FindObjectsOfType<EnviroParticle>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CheckEnviroParticles();
		}
	}

	public void SimpleNailPhysics(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBoolLocal("simpleNailPhysics", stuff);
		simpleNailPhysics = stuff;
		Nail[] array = Object.FindObjectsOfType<Nail>();
		foreach (Nail nail in array)
		{
			if (nail.magnets.Count > 0 && nail.TryGetComponent<Rigidbody>(out var component))
			{
				if (simpleNailPhysics)
				{
					component.collisionDetectionMode = CollisionDetectionMode.Discrete;
				}
				else
				{
					component.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
				}
			}
		}
	}

	public void BloodAndGoreOn(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBoolLocal("bloodEnabled", stuff);
		bloodEnabled = stuff;
	}

	public void FreezeGore(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBoolLocal("freezeGore", stuff);
	}

	public void SimplifyEnemies(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("simplifyEnemies", stuff);
		Shader.SetGlobalFloat("_UseEdgeDetection", stuff ? 1 : 0);
		opm.simplifyEnemies = stuff;
		simplifiedDistanceGroup.SetActive(stuff);
		PostProcessV2_Handler postProcessV2_Handler = MonoSingleton<PostProcessV2_Handler>.Instance;
		if ((bool)postProcessV2_Handler)
		{
			postProcessV2_Handler.enableJFA = stuff;
		}
	}

	public void OutlinesOnly(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("enemyOutlinesOnly", stuff);
		opm.outlinesOnly = stuff;
	}

	public void SimplifyEnemiesDistance(float stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloat("simplifyEnemiesDistance", stuff);
		opm.simplifiedDistance = stuff;
	}

	public void OutlineThickness(float stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetInt("outlineThickness", (int)stuff);
		MonoSingleton<PostProcessV2_Handler>.Instance.distance = (int)stuff;
	}

	public void MouseSensitivity(float stuff)
	{
		if (!ignoreSensitivitySliderChange)
		{
			MonoSingleton<PrefsManager>.Instance.SetFloatLocal("mouseSensitivity", stuff);
			opm.mouseSensitivity = stuff;
		}
	}

	public void UpdateSensitivitySlider(float stuff)
	{
		ignoreSensitivitySliderChange = true;
		mouseSensitivitySlider.value = stuff;
		ignoreSensitivitySliderChange = false;
	}

	public void ReverseMouseX(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("mouseReverseX", stuff);
		MonoSingleton<CameraController>.Instance.CheckMouseReverse();
	}

	public void ReverseMouseY(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("mouseReverseY", stuff);
		MonoSingleton<CameraController>.Instance.CheckMouseReverse();
	}

	public void ScreenShake(float stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloat("screenShake", stuff / 100f);
	}

	public void CameraTilt(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("cameraTilt", stuff);
		if (cc == null)
		{
			cc = MonoSingleton<CameraController>.Instance;
			mainCam = cc.GetComponent<Camera>();
		}
		cc.CheckTilt();
	}

	public void DiscordIntegration(bool stuff)
	{
		if (stuff)
		{
			MonoSingleton<PrefsManager>.Instance.SetBool("discordIntegration", content: true);
			DiscordController.Enable();
		}
		else
		{
			MonoSingleton<PrefsManager>.Instance.SetBool("discordIntegration", content: false);
			DiscordController.Disable();
		}
	}

	public void LevelLeaderboards(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("levelLeaderboards", stuff);
	}

	public void SeasonalEvents(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("seasonalEvents", stuff);
	}

	public void RestartWarning(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("pauseMenuConfirmationDialogs", stuff);
	}

	public void VariationMemory(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("variationMemory", stuff);
		if ((bool)MonoSingleton<GunControl>.Instance)
		{
			MonoSingleton<GunControl>.Instance.variationMemory = stuff;
		}
	}

	public void BloodStainChance(float stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloatLocal("bloodStainChance", stuff / 100f);
		opm.bloodstainChance = stuff;
	}

	public void maxGore(float stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloatLocal("maxGore", stuff * 100f);
		opm.maxGore = stuff * 100f;
		GoreZone[] array = Object.FindObjectsOfType<GoreZone>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].maxGore = stuff * 100f;
		}
	}

	public void MasterVolume(float stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloat("allVolume", stuff / 100f);
		AudioListener.volume = stuff / 100f;
	}

	public void MusicVolume(float stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloat("musicVolume", stuff / 100f);
		if ((bool)MonoSingleton<AudioMixerController>.Instance)
		{
			MonoSingleton<AudioMixerController>.Instance.optionsMusicVolume = stuff / 100f;
			MonoSingleton<AudioMixerController>.Instance.SetMusicVolume(stuff / 100f);
		}
	}

	public void SetSubtitles(bool state)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("subtitlesEnabled", state);
		if ((bool)MonoSingleton<SubtitleController>.Instance)
		{
			MonoSingleton<SubtitleController>.Instance.subtitlesEnabled = state;
		}
	}

	public void FieldOfView(float stuff)
	{
		if (cc == null)
		{
			cc = MonoSingleton<CameraController>.Instance;
			mainCam = cc.GetComponent<Camera>();
		}
		MonoSingleton<PrefsManager>.Instance.SetFloat("fieldOfView", stuff);
		mainCam.fieldOfView = stuff;
		cc.defaultFov = stuff;
	}

	public void WeaponPosition(int stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetInt("weaponHoldPosition", stuff);
		ViewModelFlip[] componentsInChildren = MonoSingleton<NewMovement>.Instance.GetComponentsInChildren<ViewModelFlip>();
		WeaponPos weaponPos = Object.FindObjectOfType<WeaponPos>();
		HUDPos[] array = Object.FindObjectsOfType<HUDPos>();
		if (weaponPos != null)
		{
			weaponPos.CheckPosition();
		}
		HUDPos[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].CheckPos();
		}
		if (stuff == 2)
		{
			ViewModelFlip[] array3 = componentsInChildren;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].Left();
			}
		}
		else
		{
			ViewModelFlip[] array3 = componentsInChildren;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].Right();
			}
		}
		CheckEasterEgg();
	}

	public void CheckEasterEgg()
	{
		if (!nmov)
		{
			nmov = MonoSingleton<NewMovement>.Instance;
		}
		if (MonoSingleton<PrefsManager>.Instance.GetInt("weaponHoldPosition") == 1 && MonoSingleton<PrefsManager>.Instance.GetInt("hudType") >= 2)
		{
			nmov.quakeJump = true;
		}
		else
		{
			nmov.quakeJump = false;
		}
	}
}
