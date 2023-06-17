using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fishing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FishingHUD : MonoSingleton<FishingHUD>
{
	[SerializeField]
	private GameObject powerMeterContainer;

	[SerializeField]
	private Slider powerMeter;

	[SerializeField]
	private GameObject hookedContainer;

	[Space]
	[SerializeField]
	private GameObject fishCaughtContainer;

	[SerializeField]
	private Text fishCaughtText;

	[SerializeField]
	private GameObject fishRenderContainer;

	[SerializeField]
	private GameObject fishSizeContainer;

	[Space]
	[SerializeField]
	private GameObject struggleContainer;

	[SerializeField]
	private GameObject outOfWaterMessage;

	[SerializeField]
	private Image struggleProgressIcon;

	[SerializeField]
	private Image struggleProgressIconOverlay;

	[SerializeField]
	private Image struggleNub;

	[SerializeField]
	private RectTransform desireBar;

	[SerializeField]
	private RectTransform fishIcon;

	[SerializeField]
	private Slider struggleProgressSlider;

	[SerializeField]
	private Text struggleLMB;

	[SerializeField]
	private Text struggleRMB;

	[SerializeField]
	private Image upArrow;

	[SerializeField]
	private Image downArrow;

	[Space]
	[SerializeField]
	private Image fishIconTemplate;

	[SerializeField]
	private Transform fishIconContainer;

	private Dictionary<FishObject, Image> fishHudIcons = new Dictionary<FishObject, Image>();

	private static Color orangeColor = new Color(1f, 0.5f, 0.1f);

	private TimeSince timeSinceLMBReleased;

	private TimeSince timeSinceRMBReleased;

	[HideInInspector]
	public TimeSince timeSinceFishCaught;

	private float containerHeight => struggleNub.rectTransform.parent.GetComponent<RectTransform>().rect.height;

	private void Start()
	{
		fishIconTemplate.gameObject.SetActive(value: false);
		foreach (KeyValuePair<FishObject, bool> recognizedFish in MonoSingleton<FishManager>.Instance.recognizedFishes)
		{
			Image image = UnityEngine.Object.Instantiate(fishIconTemplate, fishIconContainer, worldPositionStays: false);
			image.gameObject.SetActive(value: true);
			image.sprite = recognizedFish.Key.blockedIcon;
			image.color = Color.black;
			fishHudIcons.Add(recognizedFish.Key, image);
			Image component = image.GetComponentInChildren<FishIconGlow>().GetComponent<Image>();
			component.sprite = recognizedFish.Key.blockedIcon;
			component.color = new Color(1f, 1f, 1f, 0f);
		}
		fishIconContainer.gameObject.SetActive(value: false);
	}

	public void ShowHUD()
	{
		fishIconContainer.gameObject.SetActive(value: true);
	}

	public void SetFishHooked(bool hooked)
	{
		hookedContainer.SetActive(hooked);
	}

	private void OnFishUnlocked(FishObject obj)
	{
		fishHudIcons[obj].sprite = obj.icon;
		fishHudIcons[obj].color = Color.white;
		fishHudIcons[obj].GetComponentInChildren<FishIconGlow>().Blink();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		FishManager fishManager = MonoSingleton<FishManager>.Instance;
		fishManager.onFishUnlocked = (Action<FishObject>)Delegate.Combine(fishManager.onFishUnlocked, new Action<FishObject>(OnFishUnlocked));
	}

	private void OnDisable()
	{
		if ((bool)MonoSingleton<FishManager>.Instance)
		{
			FishManager fishManager = MonoSingleton<FishManager>.Instance;
			fishManager.onFishUnlocked = (Action<FishObject>)Delegate.Remove(fishManager.onFishUnlocked, new Action<FishObject>(OnFishUnlocked));
		}
	}

	public void SetState(FishingRodState state)
	{
		if (!struggleContainer.activeSelf && state == FishingRodState.FishStruggle)
		{
			outOfWaterMessage.SetActive(value: false);
		}
		powerMeterContainer.SetActive(state == FishingRodState.SelectingPower || state == FishingRodState.Throwing);
		struggleContainer.SetActive(state == FishingRodState.FishStruggle);
	}

	public void SetPowerMeter(float value, bool canFish)
	{
		powerMeter.value = value;
		powerMeter.targetGraphic.color = (canFish ? Color.white : Color.red);
	}

	private void Update()
	{
		float num = Mathf.Sin(struggleProgressSlider.value * 20f);
		fishIcon.localRotation = Quaternion.Euler(0f, 0f, num * 10f);
		if (struggleContainer.activeSelf)
		{
			Color color = Color.Lerp(orangeColor, Color.white, (float)timeSinceLMBReleased * 4f);
			Color color2 = Color.Lerp(orangeColor, Color.white, (float)timeSinceRMBReleased * 4f);
			struggleLMB.color = color;
			struggleRMB.color = color2;
			upArrow.color = color2;
			downArrow.color = color;
		}
	}

	public void ShowFishCaught(bool show = true, FishObject fish = null)
	{
		if (!show)
		{
			StopAllCoroutines();
		}
		else
		{
			timeSinceFishCaught = 0f;
		}
		fishSizeContainer.SetActive(value: false);
		fishCaughtContainer.SetActive(show);
		if (show && fish != null)
		{
			fishCaughtText.text = "<size=28>You caught</size> <color=orange>" + fish.fishName + "</color>";
		}
		foreach (Transform item in fishRenderContainer.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		if (show && fish != null)
		{
			GameObject obj = fish.InstantiateDumb();
			obj.transform.SetParent(fishRenderContainer.transform);
			obj.transform.localPosition = Vector3.zero;
			SandboxUtils.SetLayerDeep(obj.transform, LayerMask.NameToLayer("VirtualRender"));
			obj.transform.localScale *= fish.previewSizeMulti;
			StartCoroutine(AutoDismissFishCaught());
			StartCoroutine(ShowSize());
		}
	}

	public void ShowOutOfWater()
	{
		outOfWaterMessage.SetActive(value: true);
	}

	public void SetStruggleProgress(float progress, Sprite fishIconLocked, Sprite fishIconUnlocked)
	{
		struggleProgressSlider.value = progress;
		struggleProgressIcon.sprite = fishIconUnlocked;
		struggleProgressIconOverlay.sprite = fishIconLocked;
		Color color = struggleProgressIconOverlay.color;
		color.a = 1f - progress;
		struggleProgressIconOverlay.color = color;
	}

	public void SetStruggleSatisfied(bool satisfied)
	{
		struggleNub.color = (satisfied ? Color.green : Color.white);
	}

	public void SetPlayerStrugglePosition(float pos)
	{
		struggleNub.rectTransform.anchoredPosition = new Vector2(0f, (0f - pos) * containerHeight);
		if (MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad)
		{
			struggleLMB.text = MonoSingleton<InputManager>.Instance.InputSource.Fire1.Action.bindings.First().ToDisplayString();
			struggleRMB.text = MonoSingleton<InputManager>.Instance.InputSource.Fire2.Action.bindings.First().ToDisplayString();
		}
		else
		{
			struggleLMB.text = "LMB";
			struggleRMB.text = "RMB";
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)
		{
			struggleLMB.color = new Color(1f, 0.5f, 0.1f);
			downArrow.color = new Color(1f, 0.5f, 0.1f);
			timeSinceLMBReleased = 0f;
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed)
		{
			struggleRMB.color = new Color(1f, 0.5f, 0.1f);
			upArrow.color = new Color(1f, 0.5f, 0.1f);
			timeSinceRMBReleased = 0f;
		}
	}

	public void SetFishDesire(float top, float bottom)
	{
		desireBar.offsetMin = new Vector2(desireBar.offsetMin.x, (1f - bottom) * containerHeight);
		desireBar.offsetMax = new Vector2(desireBar.offsetMax.x, (0f - top) * containerHeight);
	}

	private IEnumerator ShowSize()
	{
		yield return new WaitForSeconds(1f);
		fishSizeContainer.SetActive(value: true);
	}

	private IEnumerator AutoDismissFishCaught()
	{
		yield return new WaitForSeconds(4f);
		ShowFishCaught(show: false);
	}
}
