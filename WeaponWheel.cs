using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class WeaponWheel : MonoSingleton<WeaponWheel>
{
	private List<WheelSegment> segments;

	public int segmentCount;

	public GameObject clickSound;

	public GameObject background;

	private int selectedSegment;

	private int lastSelectedSegment;

	private Vector2 direction;

	private void Start()
	{
		base.gameObject.SetActive(value: false);
		background.SetActive(value: true);
	}

	private new void OnEnable()
	{
		if (!(MonoSingleton<InputManager>.Instance == null))
		{
			Time.timeScale = 0.25f;
			selectedSegment = -1;
			direction = Vector2.zero;
			MonoSingleton<InputManager>.Instance.InputSource.Look.Action.Disable();
			GameStateManager.Instance.RegisterState(new GameState("weapon-wheel", base.gameObject)
			{
				timerModifier = 4f
			});
		}
	}

	private void OnDisable()
	{
		Time.timeScale = 1f;
		if ((bool)MonoSingleton<InputManager>.Instance && (bool)MonoSingleton<FistControl>.Instance)
		{
			MonoSingleton<InputManager>.Instance.InputSource.Look.Action.Enable();
			MonoSingleton<FistControl>.Instance.RefreshArm();
		}
	}

	private void Update()
	{
		if (!MonoSingleton<GunControl>.Instance || !MonoSingleton<GunControl>.Instance.activated || MonoSingleton<OptionsManager>.Instance.paused || MonoSingleton<NewMovement>.Instance.dead || GameStateManager.Instance.PlayerInputLocked)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		direction = Vector2.ClampMagnitude(direction + MonoSingleton<InputManager>.Instance.InputSource.WheelLook.ReadValue<Vector2>(), 1f);
		float num = Mathf.Repeat(Mathf.Atan2(direction.x, direction.y) * 57.29578f + 90f, 360f);
		selectedSegment = ((direction.sqrMagnitude > 0f) ? ((int)(num / (360f / (float)segmentCount))) : selectedSegment);
		if (MonoSingleton<InputManager>.Instance.InputSource.NextWeapon.WasCanceledThisFrame || MonoSingleton<InputManager>.Instance.InputSource.PrevWeapon.WasCanceledThisFrame || MonoSingleton<InputManager>.Instance.InputSource.LastWeapon.WasCanceledThisFrame)
		{
			if (selectedSegment != -1)
			{
				MonoSingleton<GunControl>.Instance.SwitchWeapon(selectedSegment + 1);
			}
			base.gameObject.SetActive(value: false);
			return;
		}
		for (int i = 0; i < segments.Count; i++)
		{
			if (i == selectedSegment)
			{
				segments[i].SetActive(active: true);
			}
			else
			{
				segments[i].SetActive(active: false);
			}
		}
		if (selectedSegment != lastSelectedSegment)
		{
			UnityEngine.Object.Instantiate(clickSound);
			lastSelectedSegment = selectedSegment;
			if ((bool)MonoSingleton<RumbleManager>.Instance)
			{
				MonoSingleton<RumbleManager>.Instance.SetVibration("rumble.weapon_wheel_tick");
			}
		}
	}

	public void Show()
	{
		if (!base.gameObject.activeSelf)
		{
			lastSelectedSegment = -1;
			base.gameObject.SetActive(value: true);
		}
	}

	public void SetSegments(WeaponDescriptor[] weaponDescriptors)
	{
		int num = weaponDescriptors.Length;
		if (num == segmentCount)
		{
			return;
		}
		segmentCount = num;
		lastSelectedSegment = -1;
		if (segments == null)
		{
			segments = new List<WheelSegment>(segmentCount);
		}
		foreach (WheelSegment segment in segments)
		{
			segment.DestroySegment();
		}
		segments.Clear();
		for (int i = 0; i < segmentCount; i++)
		{
			UICircle uICircle = new GameObject().AddComponent<UICircle>();
			uICircle.name = "Segment " + i;
			uICircle.Arc = 1f / (float)segmentCount - 0.005f;
			uICircle.ArcRotation = (int)(360f * ((float)i / (float)segmentCount) + 1.8f);
			uICircle.Fill = false;
			uICircle.transform.SetParent(base.transform, worldPositionStays: false);
			uICircle.rectTransform.anchorMin = Vector2.zero;
			uICircle.rectTransform.anchorMax = Vector2.one;
			uICircle.rectTransform.anchoredPosition = Vector2.zero;
			uICircle.rectTransform.sizeDelta = Vector2.zero;
			Outline outline = uICircle.gameObject.AddComponent<Outline>();
			outline.effectDistance = new Vector2(2f, -2f);
			outline.effectColor = Color.white;
			UICircle uICircle2 = new GameObject().AddComponent<UICircle>();
			uICircle2.name = "Segment Divider " + i;
			uICircle2.Arc = 0.005f;
			uICircle2.ArcRotation = (int)(360f * ((float)i / (float)segmentCount) + 1.8f - 0.9f);
			uICircle2.Fill = false;
			uICircle2.transform.SetParent(base.transform, worldPositionStays: false);
			uICircle2.rectTransform.anchorMin = Vector2.zero;
			uICircle2.rectTransform.anchorMax = Vector2.one;
			uICircle2.rectTransform.sizeDelta = new Vector2(256f, 256f);
			uICircle2.Thickness = 128f;
			Image image = new GameObject().AddComponent<Image>();
			image.name = "Icon " + i;
			image.sprite = weaponDescriptors[i].icon;
			image.transform.SetParent(uICircle.transform, worldPositionStays: false);
			float num2 = (float)i * 360f / (float)segmentCount;
			float num3 = uICircle.Arc * 360f / 2f;
			float num4 = num2 + num3;
			float f = num4 * ((float)Math.PI / 180f);
			float num5 = 112f;
			Vector2 vector = new Vector2(0f - Mathf.Cos(f), Mathf.Sin(f)) * num5;
			image.transform.localPosition = vector;
			float num6 = num4 + 180f;
			image.transform.localRotation = Quaternion.Euler(0f, 0f, 0f - num6);
			Vector2 size = image.sprite.rect.size;
			image.rectTransform.sizeDelta = new Vector2(size.x, size.y) * 0.12f;
			Image image2 = new GameObject().AddComponent<Image>();
			image2.name = "Icon Outline " + i;
			image2.sprite = weaponDescriptors[i].glowIcon;
			image2.transform.SetParent(uICircle.transform, worldPositionStays: false);
			image2.transform.localPosition = image.transform.localPosition;
			image2.transform.localRotation = image.transform.localRotation;
			image2.rectTransform.sizeDelta = image.rectTransform.sizeDelta;
			image2.transform.SetAsFirstSibling();
			WheelSegment wheelSegment = new WheelSegment
			{
				segment = uICircle,
				icon = image,
				iconGlow = image2,
				descriptor = weaponDescriptors[i],
				divider = uICircle2
			};
			segments.Add(wheelSegment);
			wheelSegment.SetActive(active: false);
		}
	}
}
