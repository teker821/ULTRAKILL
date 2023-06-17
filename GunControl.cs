using System.Collections.Generic;
using System.Linq;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class GunControl : MonoSingleton<GunControl>
{
	private InputManager inman;

	public bool activated = true;

	private int rememberedSlot;

	public int currentVariation;

	public int currentSlot;

	public GameObject currentWeapon;

	public List<List<GameObject>> slots = new List<List<GameObject>>();

	public List<GameObject> slot1 = new List<GameObject>();

	public List<GameObject> slot2 = new List<GameObject>();

	public List<GameObject> slot3 = new List<GameObject>();

	public List<GameObject> slot4 = new List<GameObject>();

	public List<GameObject> slot5 = new List<GameObject>();

	public List<GameObject> slot6 = new List<GameObject>();

	public List<GameObject> allWeapons = new List<GameObject>();

	private AudioSource aud;

	public float killCharge;

	public Slider killMeter;

	public bool noWeapons = true;

	public int lastUsedSlot = 69;

	public int lastUsedVariation = 69;

	private Dictionary<int, int> retainedVariations = new Dictionary<int, int>();

	public float headShotComboTime;

	public int headshots;

	private bool hookCombo;

	private StyleHUD shud;

	public GameObject[] gunPanel;

	private float scrollCooldown;

	private const float WeaponWheelTime = 0.25f;

	[HideInInspector]
	public bool stayUnarmed;

	[HideInInspector]
	public bool variationMemory;

	private void Start()
	{
		inman = MonoSingleton<InputManager>.Instance;
		currentVariation = PlayerPrefs.GetInt("CurVar", 0);
		currentSlot = PlayerPrefs.GetInt("CurSlo", 1);
		lastUsedVariation = PlayerPrefs.GetInt("LasVar", 69);
		lastUsedSlot = PlayerPrefs.GetInt("LasSlo", 69);
		aud = GetComponent<AudioSource>();
		variationMemory = MonoSingleton<PrefsManager>.Instance.GetBool("variationMemory");
		slots.Add(slot1);
		slots.Add(slot2);
		slots.Add(slot3);
		slots.Add(slot4);
		slots.Add(slot5);
		slots.Add(slot6);
		if (currentSlot > slots.Count)
		{
			currentSlot = 1;
		}
		foreach (List<GameObject> slot in slots)
		{
			foreach (GameObject item in slot)
			{
				if (item != null)
				{
					allWeapons.Add(item);
				}
			}
			if (slot.Count != 0)
			{
				noWeapons = false;
			}
		}
		if (currentWeapon == null && slots[currentSlot - 1].Count > currentVariation)
		{
			currentWeapon = slots[currentSlot - 1][currentVariation];
		}
		else if (currentWeapon == null && slot1.Count != 0)
		{
			currentSlot = 1;
			currentVariation = 0;
			currentWeapon = slot1[0];
		}
		shud = MonoSingleton<StyleHUD>.Instance;
		UpdateWeaponList(firstTime: true);
	}

	private void CalculateSlotCount()
	{
		List<WeaponDescriptor> list = new List<WeaponDescriptor>();
		foreach (List<GameObject> slot in slots)
		{
			GameObject gameObject = slot.FirstOrDefault();
			if (!(gameObject == null))
			{
				WeaponIcon component = gameObject.GetComponent<WeaponIcon>();
				if (component != null)
				{
					list.Add(component.weaponDescriptor);
				}
			}
		}
		MonoSingleton<WeaponWheel>.Instance.SetSegments(list.ToArray());
	}

	private void Update()
	{
		if (activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			PlayerInput inputSource = MonoSingleton<InputManager>.Instance.InputSource;
			if (headShotComboTime > 0f)
			{
				headShotComboTime = Mathf.MoveTowards(headShotComboTime, 0f, Time.deltaTime);
			}
			else
			{
				headshots = 0;
			}
			if (lastUsedSlot == 0)
			{
				lastUsedSlot = 69;
			}
			if (!MonoSingleton<OptionsManager>.Instance.inIntro && !noWeapons && !MonoSingleton<OptionsManager>.Instance.paused && !MonoSingleton<NewMovement>.Instance.dead)
			{
				if (inputSource.NextWeapon.IsPressed && inputSource.PrevWeapon.IsPressed)
				{
					hookCombo = true;
					if (MonoSingleton<WeaponWheel>.Instance.gameObject.activeSelf)
					{
						MonoSingleton<WeaponWheel>.Instance.gameObject.SetActive(value: false);
					}
				}
				if (((inputSource.NextWeapon.IsPressed && inputSource.NextWeapon.HoldTime >= 0.25f && !inputSource.PrevWeapon.IsPressed) || (inputSource.PrevWeapon.IsPressed && inputSource.PrevWeapon.HoldTime >= 0.25f && !inputSource.NextWeapon.IsPressed) || (inputSource.LastWeapon.IsPressed && inputSource.LastWeapon.HoldTime >= 0.25f)) && !hookCombo)
				{
					MonoSingleton<WeaponWheel>.Instance.Show();
				}
			}
			if (MonoSingleton<InputManager>.Instance.InputSource.Slot1.WasPerformedThisFrame)
			{
				if (slot1.Count > 0 && slot1[0] != null && (slot1.Count > 1 || currentSlot != 1))
				{
					SwitchWeapon(1, slot1);
				}
			}
			else if (MonoSingleton<InputManager>.Instance.InputSource.Slot2.WasPerformedThisFrame)
			{
				if (slot2.Count > 0 && slot2[0] != null && (slot2.Count > 1 || currentSlot != 2))
				{
					SwitchWeapon(2, slot2);
				}
			}
			else if (MonoSingleton<InputManager>.Instance.InputSource.Slot3.WasPerformedThisFrame && (slot3.Count > 1 || currentSlot != 3))
			{
				if (slot3.Count > 0 && slot3[0] != null)
				{
					SwitchWeapon(3, slot3);
				}
			}
			else if (MonoSingleton<InputManager>.Instance.InputSource.Slot4.WasPerformedThisFrame && (slot4.Count > 1 || currentSlot != 4))
			{
				if (slot4.Count > 0 && slot4[0] != null)
				{
					SwitchWeapon(4, slot4);
				}
			}
			else if (MonoSingleton<InputManager>.Instance.InputSource.Slot5.WasPerformedThisFrame && (slot5.Count > 1 || currentSlot != 5))
			{
				if (slot5.Count > 0 && slot5[0] != null)
				{
					SwitchWeapon(5, slot5);
				}
			}
			else if (MonoSingleton<InputManager>.Instance.InputSource.Slot6.WasPerformedThisFrame && (slot6.Count > 1 || currentSlot != 6))
			{
				if (slot6.Count > 0 && slot6[0] != null)
				{
					SwitchWeapon(6, slot6, lastUsedSlot: false, useRetainedVariation: true);
				}
			}
			else if (MonoSingleton<InputManager>.Instance.InputSource.LastWeapon.WasCanceledThisFrame && MonoSingleton<InputManager>.Instance.InputSource.LastWeapon.HoldTime < 0.25f && lastUsedSlot != 69)
			{
				if (slots[lastUsedSlot - 1] != null)
				{
					SwitchWeapon(lastUsedSlot, slots[lastUsedSlot - 1], lastUsedSlot: true);
				}
			}
			else if (MonoSingleton<InputManager>.Instance.InputSource.ChangeVariation.WasPerformedThisFrame && slots[currentSlot - 1].Count > 1)
			{
				SwitchWeapon(currentSlot, slots[currentSlot - 1]);
			}
			else if (!noWeapons)
			{
				float num = Mouse.current.scroll.ReadValue().y;
				if (inman.ScrRev)
				{
					num *= -1f;
				}
				if (inputSource.NextWeapon.HoldTime < 0.25f && !hookCombo && ((num > 0f && inman.ScrOn) || inputSource.NextWeapon.WasCanceledThisFrame) && scrollCooldown <= 0f)
				{
					int num2 = 0;
					if (inman.ScrWep && inman.ScrVar)
					{
						foreach (List<GameObject> slot in slots)
						{
							if (slot.Count > 0)
							{
								num2++;
							}
						}
					}
					bool flag = false;
					if (inman.ScrVar)
					{
						if (slots[currentSlot - 1].Count > currentVariation + 1 || ((!inman.ScrWep || num2 <= 1) && slots[currentSlot - 1].Count > 1))
						{
							SwitchWeapon(currentSlot, slots[currentSlot - 1], lastUsedSlot: false, useRetainedVariation: false, scrolled: true);
							scrollCooldown = 0.5f;
							flag = true;
						}
						else if (!inman.ScrWep)
						{
							flag = true;
						}
					}
					if (!flag && inman.ScrWep)
					{
						if (!flag && currentSlot < slots.Count)
						{
							for (int i = currentSlot; i < slots.Count; i++)
							{
								if (slots[i].Count > 0)
								{
									flag = true;
									SwitchWeapon(i + 1, slots[i], lastUsedSlot: false, useRetainedVariation: false, scrolled: true);
									scrollCooldown = 0.5f;
									break;
								}
							}
						}
						if (!flag)
						{
							for (int j = 0; j < currentSlot; j++)
							{
								if (slots[j].Count > 0)
								{
									if (j != currentSlot - 1)
									{
										SwitchWeapon(j + 1, slots[j], lastUsedSlot: false, useRetainedVariation: false, scrolled: true);
										scrollCooldown = 0.5f;
									}
									break;
								}
							}
						}
					}
				}
				else if (inputSource.PrevWeapon.HoldTime < 0.25f && !hookCombo && ((num < 0f && inman.ScrOn) || inputSource.PrevWeapon.WasCanceledThisFrame) && scrollCooldown <= 0f)
				{
					int num3 = 0;
					if (inman.ScrWep && inman.ScrVar)
					{
						foreach (List<GameObject> slot2 in slots)
						{
							if (slot2.Count > 0)
							{
								num3++;
							}
						}
					}
					if ((inman.ScrWep && !inman.ScrVar) || (inman.ScrWep && num3 > 1))
					{
						if (inman.ScrVar)
						{
							if (currentVariation != 0)
							{
								GameObject weapon = slots[currentSlot - 1][currentVariation - 1];
								ForceWeapon(weapon);
								scrollCooldown = 0.5f;
							}
							else if (currentSlot == 1)
							{
								for (int num4 = slots.Count - 1; num4 >= 0; num4--)
								{
									if (slots[num4].Count > 0)
									{
										if (num4 != currentSlot - 1)
										{
											GameObject weapon2 = slots[num4][slots[num4].Count - 1];
											ForceWeapon(weapon2);
											scrollCooldown = 0.5f;
										}
										break;
									}
								}
							}
							else
							{
								bool flag2 = false;
								for (int num5 = currentSlot - 2; num5 >= 0; num5--)
								{
									if (slots[num5].Count > 0)
									{
										GameObject weapon3 = slots[num5][slots[num5].Count - 1];
										ForceWeapon(weapon3);
										scrollCooldown = 0.5f;
										flag2 = true;
										break;
									}
								}
								if (!flag2)
								{
									for (int num6 = slots.Count - 1; num6 >= 0; num6--)
									{
										if (slots[num6].Count > 0)
										{
											if (num6 != currentSlot - 1)
											{
												GameObject weapon4 = slots[num6][slots[num6].Count - 1];
												ForceWeapon(weapon4);
												scrollCooldown = 0.5f;
											}
											break;
										}
									}
								}
							}
						}
						else if (currentSlot == 1)
						{
							for (int num7 = slots.Count - 1; num7 >= 0; num7--)
							{
								if (slots[num7].Count > 0)
								{
									if (num7 != currentSlot - 1)
									{
										SwitchWeapon(num7 + 1, slots[num7], lastUsedSlot: false, useRetainedVariation: false, scrolled: true);
										scrollCooldown = 0.5f;
									}
									break;
								}
							}
						}
						else
						{
							bool flag3 = false;
							for (int num8 = currentSlot - 2; num8 >= 0; num8--)
							{
								if (slots[num8].Count > 0)
								{
									SwitchWeapon(num8 + 1, slots[num8], lastUsedSlot: false, useRetainedVariation: false, scrolled: true);
									scrollCooldown = 0.5f;
									flag3 = true;
									break;
								}
							}
							if (!flag3)
							{
								for (int num9 = slots.Count - 1; num9 >= 0; num9--)
								{
									if (slots[num9].Count > 0)
									{
										if (num9 != currentSlot - 1)
										{
											SwitchWeapon(num9 + 1, slots[num9], lastUsedSlot: false, useRetainedVariation: false, scrolled: true);
											scrollCooldown = 0.5f;
										}
										break;
									}
								}
							}
						}
					}
					else if (slots[currentSlot - 1].Count > 1)
					{
						if (currentVariation != 0)
						{
							GameObject weapon5 = slots[currentSlot - 1][currentVariation - 1];
							ForceWeapon(weapon5);
							scrollCooldown = 0.5f;
						}
						else
						{
							GameObject weapon6 = slots[currentSlot - 1][slots[currentSlot - 1].Count - 1];
							ForceWeapon(weapon6);
							scrollCooldown = 0.5f;
						}
					}
				}
			}
			if (hookCombo && !inputSource.NextWeapon.IsPressed && !inputSource.PrevWeapon.IsPressed)
			{
				hookCombo = false;
			}
		}
		if (scrollCooldown > 0f)
		{
			scrollCooldown = Mathf.MoveTowards(scrollCooldown, 0f, Time.deltaTime * 5f);
		}
	}

	private void OnGUI()
	{
		if (!GunControlDebug.GunControlActivated)
		{
			return;
		}
		GUILayout.Label("Gun Control");
		GUILayout.Label("Last Used Slot: " + lastUsedSlot);
		GUILayout.Label("Current Slot: " + currentSlot);
		GUILayout.Label("Current Variation: " + currentVariation);
		GUILayout.Space(12f);
		GUILayout.Label("Retained Variations:");
		foreach (KeyValuePair<int, int> retainedVariation in retainedVariations)
		{
			GUILayout.Label(retainedVariation.Key + ": " + retainedVariation.Value);
		}
	}

	public void SwitchWeapon(int target)
	{
		if (target - 1 < slots.Count)
		{
			List<GameObject> list = slots[target - 1];
			if (list != null && list.Count > 0)
			{
				SwitchWeapon(target, slots[target - 1]);
			}
		}
	}

	private void RetainVariation(int slot, int variationIndex)
	{
		if (retainedVariations.ContainsKey(slot))
		{
			retainedVariations[slot] = variationIndex;
		}
		else
		{
			retainedVariations.Add(slot, currentVariation);
		}
	}

	public void SwitchWeapon(int target, List<GameObject> slot, bool lastUsedSlot = false, bool useRetainedVariation = false, bool scrolled = false)
	{
		if (currentWeapon != null)
		{
			currentWeapon.SetActive(value: false);
		}
		target = Mathf.Clamp(target, 1, slots.Count);
		int num = target;
		int value = currentVariation;
		if (useRetainedVariation && target != currentSlot && retainedVariations.TryGetValue(target - 1, out var value2))
		{
			currentVariation = value2;
		}
		else if (lastUsedSlot)
		{
			if (slots[this.lastUsedSlot - 1].Count == 0)
			{
				this.lastUsedSlot = currentSlot;
				slot = slots[currentSlot - 1];
				num = currentSlot;
				if (currentVariation == 0)
				{
					if (slots[this.lastUsedSlot - 1].Count > 1)
					{
						lastUsedVariation = slots[this.lastUsedSlot - 1].Count - 1;
					}
					else
					{
						lastUsedVariation = 0;
					}
				}
				else
				{
					lastUsedVariation = currentVariation - 1;
				}
			}
			int num2 = lastUsedVariation;
			int num3 = currentVariation;
			currentVariation = num2;
			lastUsedVariation = num3;
			RetainVariation(num - 1, currentVariation);
			this.lastUsedSlot = currentSlot;
		}
		else if (currentSlot == target && currentVariation + 1 < slot.Count)
		{
			if (this.lastUsedSlot == 69)
			{
				this.lastUsedSlot = currentSlot;
				lastUsedVariation = currentVariation;
			}
			currentVariation++;
			RetainVariation(target - 1, currentVariation);
		}
		else
		{
			if (currentSlot != target)
			{
				this.lastUsedSlot = currentSlot;
				lastUsedVariation = currentVariation;
			}
			else
			{
				int num4 = 0;
				foreach (List<GameObject> slot2 in slots)
				{
					if (slot2.Count != 0)
					{
						num4++;
					}
				}
				if (num4 <= 1)
				{
					this.lastUsedSlot = currentSlot;
					lastUsedVariation = currentVariation;
				}
			}
			currentVariation = 0;
			RetainVariation(target - 1, currentVariation);
		}
		if (variationMemory)
		{
			string key = "Slot" + currentSlot + "Var";
			if (currentSlot != num && (!scrolled || !inman.ScrWep || !inman.ScrVar))
			{
				PlayerPrefs.SetInt(key, value);
				key = "Slot" + num + "Var";
				currentVariation = PlayerPrefs.GetInt(key, 0);
				RetainVariation(num - 1, currentVariation);
			}
			else
			{
				PlayerPrefs.SetInt(key, value);
			}
		}
		currentSlot = num;
		if (slots[currentSlot - 1].Count < currentVariation + 1)
		{
			currentVariation = 0;
			RetainVariation(num - 1, currentVariation);
		}
		if (!noWeapons && currentVariation < slots[currentSlot - 1].Count)
		{
			currentWeapon = slots[currentSlot - 1][currentVariation];
			currentWeapon.SetActive(value: true);
			aud.Play();
			PlayerPrefs.SetInt("CurVar", currentVariation);
			PlayerPrefs.SetInt("CurSlo", currentSlot);
			PlayerPrefs.SetInt("LasVar", lastUsedVariation);
			PlayerPrefs.SetInt("LasSlo", this.lastUsedSlot);
		}
		shud.SnapFreshnessSlider();
	}

	public void ForceWeapon(GameObject weapon)
	{
		new List<GameObject>();
		foreach (List<GameObject> slot in slots)
		{
			for (int i = 0; i < slot.Count; i++)
			{
				if (slot[i].name == weapon.name + "(Clone)" || slot[i].name == weapon.name)
				{
					if (currentWeapon != null)
					{
						currentWeapon.SetActive(value: false);
					}
					currentSlot = slots.IndexOf(slot) + 1;
					currentVariation = i;
					RetainVariation(currentSlot - 1, currentVariation);
					currentWeapon = slot[currentVariation];
					currentWeapon.SetActive(value: true);
					aud.Play();
					break;
				}
			}
		}
	}

	public void NoWeapon()
	{
		if (currentWeapon != null)
		{
			currentWeapon.SetActive(value: false);
			rememberedSlot = currentSlot;
			activated = false;
		}
	}

	public void YesWeapon()
	{
		if (slots[currentSlot - 1].Count > currentVariation && slots[currentSlot - 1][currentVariation] != null)
		{
			currentWeapon = slots[currentSlot - 1][currentVariation];
			currentWeapon.SetActive(value: true);
		}
		else if (slots[currentSlot - 1].Count > 0)
		{
			currentWeapon = slots[currentSlot - 1][0];
			currentVariation = 0;
			RetainVariation(currentSlot - 1, currentVariation);
			currentWeapon.SetActive(value: true);
		}
		else
		{
			int num = -1;
			for (int i = 0; i < currentSlot; i++)
			{
				if (slots[i].Count > 0)
				{
					num = i;
				}
			}
			if (num == -1)
			{
				num = 99;
				for (int j = currentSlot; j < slots.Count; j++)
				{
					if (slots[j].Count > 0 && j < num)
					{
						num = j;
					}
				}
			}
			if (num != 99)
			{
				currentWeapon = slots[num][0];
				currentSlot = num + 1;
				currentVariation = 0;
			}
			else
			{
				noWeapons = true;
			}
		}
		if (currentWeapon != null)
		{
			currentWeapon.SetActive(value: false);
			activated = true;
			currentWeapon.SetActive(value: true);
		}
	}

	public void AddKill()
	{
		if (killCharge < killMeter.maxValue)
		{
			killCharge += 1f;
			if (killCharge > killMeter.maxValue)
			{
				killCharge = killMeter.maxValue;
			}
			killMeter.value = killCharge;
		}
	}

	public void ClearKills()
	{
		killCharge = 0f;
		killMeter.value = killCharge;
	}

	public void UpdateWeaponList(bool firstTime = false)
	{
		allWeapons.Clear();
		noWeapons = true;
		foreach (List<GameObject> slot in slots)
		{
			foreach (GameObject item in slot)
			{
				if (item != null)
				{
					allWeapons.Add(item);
					if (noWeapons)
					{
						noWeapons = false;
					}
				}
			}
		}
		UpdateWeaponIcon(firstTime);
		MonoSingleton<RailcannonMeter>.Instance?.CheckStatus();
		if (shud == null)
		{
			shud = MonoSingleton<StyleHUD>.Instance;
		}
		shud.ResetFreshness();
		CalculateSlotCount();
	}

	public void UpdateWeaponIcon(bool firstTime = false)
	{
		if (gunPanel == null || gunPanel.Length == 0)
		{
			return;
		}
		GameObject[] array;
		if (noWeapons || !MonoSingleton<PrefsManager>.Instance.GetBool("weaponIcons") || MapInfoBase.InstanceAnyType.hideStockHUD)
		{
			array = gunPanel;
			foreach (GameObject gameObject in array)
			{
				if ((bool)gameObject)
				{
					gameObject.SetActive(value: false);
				}
			}
			return;
		}
		array = gunPanel;
		foreach (GameObject gameObject2 in array)
		{
			if (gameObject2 != null && (!firstTime || gameObject2 != gunPanel[0]))
			{
				gameObject2.SetActive(value: true);
			}
		}
	}
}
