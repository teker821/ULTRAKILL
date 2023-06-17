using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsOptions : MonoBehaviour
{
	private InputManager inman;

	public OptionsManager opm;

	private List<Text> allTexts = new List<Text>();

	public Text wText;

	public Text sText;

	public Text aText;

	public Text dText;

	public Text jumpText;

	public Text dodgeText;

	public Text slideText;

	public Text fire1Text;

	public Text fire2Text;

	public Text punchText;

	public Text lastUsedWeaponText;

	public Text changeVariation;

	public Text changeFist;

	public Text hook;

	public Text slot1text;

	public Text slot2text;

	public Text slot3text;

	public Text slot4text;

	public Text slot5text;

	public Toggle scrollWheel;

	public Dropdown variationWheel;

	public Toggle reverseWheel;

	private GameObject currentKey;

	public Color normalColor;

	public Color pressedColor;

	private bool canUnpause;

	private void Start()
	{
		inman = MonoSingleton<InputManager>.Instance;
		opm = MonoSingleton<OptionsManager>.Instance;
		wText.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.W", 119));
		sText.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.S", 115));
		aText.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.A", 97));
		dText.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.D", 100));
		jumpText.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.Jump", 32));
		dodgeText.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.Dodge", 304));
		slideText.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.Slide", 306));
		fire1Text.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.Fire1", 323));
		fire2Text.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.Fire2", 324));
		punchText.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.Punch", 102));
		lastUsedWeaponText.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.LastUsedWeapon", 113));
		changeVariation.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.ChangeVariation", 101));
		changeFist.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.ChangeFist", 103));
		hook.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.Hook", 114));
		slot1text.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.Slot1", 49));
		slot2text.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.Slot2", 50));
		slot3text.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.Slot3", 51));
		slot4text.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.Slot4", 52));
		slot5text.text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding.Slot5", 53));
		allTexts.Add(wText);
		allTexts.Add(sText);
		allTexts.Add(aText);
		allTexts.Add(dText);
		allTexts.Add(jumpText);
		allTexts.Add(dodgeText);
		allTexts.Add(slideText);
		allTexts.Add(fire1Text);
		allTexts.Add(fire2Text);
		allTexts.Add(punchText);
		allTexts.Add(lastUsedWeaponText);
		allTexts.Add(changeVariation);
		allTexts.Add(changeFist);
		allTexts.Add(hook);
		allTexts.Add(slot1text);
		allTexts.Add(slot2text);
		allTexts.Add(slot3text);
		allTexts.Add(slot4text);
		allTexts.Add(slot5text);
		scrollWheel.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("scrollEnabled");
		bool @bool = MonoSingleton<PrefsManager>.Instance.GetBool("scrollVariations");
		bool bool2 = MonoSingleton<PrefsManager>.Instance.GetBool("scrollWeapons");
		if (@bool && bool2)
		{
			variationWheel.value = 2;
		}
		else if (@bool)
		{
			variationWheel.value = 1;
		}
		else
		{
			variationWheel.value = 0;
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBool("scrollReversed"))
		{
			reverseWheel.isOn = true;
		}
	}

	private void OnDisable()
	{
		if (currentKey != null)
		{
			if (opm == null)
			{
				opm = MonoSingleton<OptionsManager>.Instance;
			}
			currentKey.GetComponent<Image>().color = normalColor;
			currentKey = null;
			opm.dontUnpause = false;
		}
	}

	private void LateUpdate()
	{
		if (canUnpause)
		{
			if (opm == null)
			{
				opm = MonoSingleton<OptionsManager>.Instance;
			}
			canUnpause = false;
			opm.dontUnpause = false;
		}
	}

	private void OnGUI()
	{
		if (!(currentKey != null))
		{
			return;
		}
		Event current = Event.current;
		if (current.keyCode == KeyCode.Escape)
		{
			currentKey.GetComponent<Image>().color = normalColor;
			currentKey = null;
			canUnpause = true;
		}
		if (current.isKey || current.isMouse || current.button > 2 || current.shift)
		{
			KeyCode keyCode = KeyCode.Alpha0;
			if (current.isKey)
			{
				keyCode = current.keyCode;
			}
			else if (Input.GetKey(KeyCode.LeftShift))
			{
				keyCode = KeyCode.LeftShift;
			}
			else if (Input.GetKey(KeyCode.RightShift))
			{
				keyCode = KeyCode.RightShift;
			}
			else
			{
				if (current.button > 6)
				{
					currentKey.GetComponent<Image>().color = normalColor;
					currentKey = null;
					canUnpause = true;
					return;
				}
				keyCode = (KeyCode)(323 + current.button);
			}
			if (inman == null)
			{
				inman = MonoSingleton<InputManager>.Instance;
			}
			inman.Inputs[currentKey.name] = keyCode;
			currentKey.GetComponentInChildren<Text>().text = GetKeyName(keyCode);
			MonoSingleton<PrefsManager>.Instance.SetInt("keyBinding." + currentKey.name, (int)keyCode);
			inman.UpdateBindings();
			currentKey.GetComponent<Image>().color = normalColor;
			currentKey = null;
			canUnpause = true;
		}
		else if (Input.GetKey(KeyCode.Mouse3) || Input.GetKey(KeyCode.Mouse4) || Input.GetKey(KeyCode.Mouse5) || Input.GetKey(KeyCode.Mouse6))
		{
			KeyCode keyCode2 = KeyCode.Mouse3;
			if (Input.GetKey(KeyCode.Mouse4))
			{
				keyCode2 = KeyCode.Mouse4;
			}
			else if (Input.GetKey(KeyCode.Mouse5))
			{
				keyCode2 = KeyCode.Mouse5;
			}
			else if (Input.GetKey(KeyCode.Mouse6))
			{
				keyCode2 = KeyCode.Mouse6;
			}
			if (inman == null)
			{
				inman = MonoSingleton<InputManager>.Instance;
			}
			inman.Inputs[currentKey.name] = keyCode2;
			currentKey.GetComponentInChildren<Text>().text = GetKeyName(keyCode2);
			MonoSingleton<PrefsManager>.Instance.SetInt("keyBinding." + currentKey.name, (int)keyCode2);
			inman.UpdateBindings();
			currentKey.GetComponent<Image>().color = normalColor;
			currentKey = null;
			canUnpause = true;
		}
		else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			KeyCode keyCode3 = KeyCode.LeftShift;
			if (Input.GetKey(KeyCode.RightShift))
			{
				keyCode3 = KeyCode.RightShift;
			}
			inman.Inputs[currentKey.name] = keyCode3;
			currentKey.GetComponentInChildren<Text>().text = GetKeyName(keyCode3);
			MonoSingleton<PrefsManager>.Instance.SetInt("keyBinding." + currentKey.name, (int)keyCode3);
			inman.UpdateBindings();
			currentKey.GetComponent<Image>().color = normalColor;
			currentKey = null;
			canUnpause = true;
		}
	}

	public void ChangeKey(GameObject stuff)
	{
		if (opm == null)
		{
			opm = MonoSingleton<OptionsManager>.Instance;
		}
		opm.dontUnpause = true;
		currentKey = stuff;
		stuff.GetComponent<Image>().color = pressedColor;
	}

	public void ResetKeys()
	{
		if (inman == null)
		{
			inman = MonoSingleton<InputManager>.Instance;
		}
		for (int i = 0; i < allTexts.Count; i++)
		{
			switch (i)
			{
			case 0:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.W;
				break;
			case 1:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.S;
				break;
			case 2:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.A;
				break;
			case 3:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.D;
				break;
			case 4:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.Space;
				break;
			case 5:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.LeftShift;
				break;
			case 6:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.LeftControl;
				break;
			case 7:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.Mouse0;
				break;
			case 8:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.Mouse1;
				break;
			case 9:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.F;
				break;
			case 10:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.Q;
				break;
			case 11:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.E;
				break;
			case 12:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.G;
				break;
			case 13:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.R;
				break;
			case 14:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.Alpha1;
				break;
			case 15:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.Alpha2;
				break;
			case 16:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.Alpha3;
				break;
			case 17:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.Alpha4;
				break;
			case 18:
				inman.Inputs[allTexts[i].transform.parent.name] = KeyCode.Alpha5;
				break;
			}
			MonoSingleton<PrefsManager>.Instance.SetInt("keyBinding." + allTexts[i].transform.parent.name, (int)inman.Inputs[allTexts[i].transform.parent.name]);
			allTexts[i].text = GetKeyName((KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt("keyBinding." + allTexts[i].transform.parent.name));
			inman.UpdateBindings();
		}
	}

	public static string GetKeyName(KeyCode key)
	{
		return key switch
		{
			KeyCode.Alpha1 => "1", 
			KeyCode.Alpha2 => "2", 
			KeyCode.Alpha3 => "3", 
			KeyCode.Alpha4 => "4", 
			KeyCode.Alpha5 => "5", 
			KeyCode.Alpha6 => "6", 
			KeyCode.Alpha7 => "7", 
			KeyCode.Alpha8 => "8", 
			KeyCode.Alpha9 => "9", 
			KeyCode.Alpha0 => "0", 
			KeyCode.Mouse0 => "Left Mouse Button", 
			KeyCode.Mouse1 => "Right Mouse Button", 
			KeyCode.Mouse2 => "Middle Mouse Button", 
			KeyCode.Mouse3 => "Mouse4", 
			KeyCode.Mouse4 => "Mouse5", 
			KeyCode.Mouse5 => "Mouse6", 
			KeyCode.Mouse6 => "Mouse7", 
			KeyCode.LeftShift => "Left Shift", 
			KeyCode.RightShift => "Right Shift", 
			KeyCode.LeftControl => "Left Control", 
			KeyCode.RightControl => "Right Control", 
			KeyCode.LeftAlt => "Left Alt", 
			KeyCode.RightAlt => "Right Alt", 
			_ => key.ToString(), 
		};
	}

	public void ScrollOn(bool stuff)
	{
		if (inman == null)
		{
			inman = MonoSingleton<InputManager>.Instance;
		}
		if (stuff)
		{
			MonoSingleton<PrefsManager>.Instance.SetBool("scrollEnabled", content: true);
			inman.ScrOn = true;
		}
		else
		{
			MonoSingleton<PrefsManager>.Instance.SetBool("scrollEnabled", content: false);
			inman.ScrOn = false;
		}
	}

	public void ScrollVariations(int stuff)
	{
		if (inman == null)
		{
			inman = MonoSingleton<InputManager>.Instance;
		}
		switch (stuff)
		{
		case 0:
			MonoSingleton<PrefsManager>.Instance.SetBool("scrollWeapons", content: true);
			MonoSingleton<PrefsManager>.Instance.SetBool("scrollVariations", content: false);
			inman.ScrWep = true;
			inman.ScrVar = false;
			break;
		case 1:
			MonoSingleton<PrefsManager>.Instance.SetBool("scrollWeapons", content: false);
			MonoSingleton<PrefsManager>.Instance.SetBool("scrollVariations", content: true);
			inman.ScrWep = false;
			inman.ScrVar = true;
			break;
		default:
			MonoSingleton<PrefsManager>.Instance.SetBool("scrollWeapons", content: true);
			MonoSingleton<PrefsManager>.Instance.SetBool("scrollVariations", content: true);
			inman.ScrWep = true;
			inman.ScrVar = true;
			break;
		}
	}

	public void ScrollReverse(bool stuff)
	{
		if (inman == null)
		{
			inman = MonoSingleton<InputManager>.Instance;
		}
		if (stuff)
		{
			MonoSingleton<PrefsManager>.Instance.SetBool("scrollReversed", content: true);
			inman.ScrRev = true;
		}
		else
		{
			MonoSingleton<PrefsManager>.Instance.SetBool("scrollReversed", content: false);
			inman.ScrRev = false;
		}
	}
}
