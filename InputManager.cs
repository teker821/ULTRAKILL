using System;
using System.Collections.Generic;
using NewBlood;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class InputManager : MonoSingleton<InputManager>
{
	private class BindingInfo
	{
		public InputAction Action;

		public string Name;

		public int Offset;

		public KeyCode DefaultKey;

		public string PrefName => "keyBinding." + Name;
	}

	private sealed class ButtonPressListener : IObserver<InputControl>
	{
		public static ButtonPressListener Instance { get; } = new ButtonPressListener();


		public void OnCompleted()
		{
		}

		public void OnError(Exception error)
		{
		}

		public void OnNext(InputControl value)
		{
			if (!(value.device is LegacyInput))
			{
				MonoSingleton<InputManager>.Instance.LastButtonDevice = value.device;
			}
		}
	}

	public Dictionary<string, KeyCode> inputsDictionary = new Dictionary<string, KeyCode>();

	public bool ScrOn;

	public bool ScrWep;

	public bool ScrVar;

	public bool ScrRev;

	private IDisposable anyButtonListener;

	private BindingInfo[] bindings;

	public PlayerInput InputSource { get; private set; }

	public InputDevice LastButtonDevice { get; private set; }

	private static IObservable<InputControl> onAnyInput => from e in InputSystem.onEvent
		select (e.type != 1398030676 && e.type != 1145852993) ? null : e.GetFirstButtonPressOrNull(-1f, buttonControlsOnly: false) into c
		where c != null && !c.noisy
		select c;

	public Dictionary<string, KeyCode> Inputs => inputsDictionary;

	public bool PerformingCheatMenuCombo()
	{
		if (!MonoSingleton<CheatsController>.Instance.cheatsEnabled)
		{
			return false;
		}
		if (!(LastButtonDevice is Gamepad))
		{
			return false;
		}
		if (Gamepad.current == null)
		{
			return false;
		}
		return Gamepad.current.selectButton.isPressed;
	}

	public string GetBindingString(string action)
	{
		if (!InputSource.Actions.TryGetValue(action, out var value))
		{
			return null;
		}
		ReadOnlyArray<InputBinding> readOnlyArray = value.Action.bindings;
		string text = string.Empty;
		int num = 0;
		for (int i = 0; i < readOnlyArray.Count; i++)
		{
			if (readOnlyArray[i].isComposite)
			{
				num = i;
				continue;
			}
			InputControl inputControl = InputSystem.FindControl(readOnlyArray[i].path);
			InputDevice inputDevice = inputControl?.device;
			if (inputControl == null || inputDevice != LastButtonDevice)
			{
				continue;
			}
			if (readOnlyArray[i].isPartOfComposite)
			{
				for (int j = num + 1; j < readOnlyArray.Count && readOnlyArray[j].isPartOfComposite; j++)
				{
					if (j > num + 1)
					{
						text += " + ";
					}
					text += InputSystem.FindControl(readOnlyArray[j].path)?.displayName ?? "?";
				}
				return text;
			}
			return inputControl.displayName;
		}
		return null;
	}

	protected override void Awake()
	{
		base.Awake();
		InputSource = new PlayerInput();
		InputSource.Move.Action.AddCompositeBinding("Dpad").With("Up", "<Keyboard>/w", "Keyboard").With("Down", "<Keyboard>/s", "Keyboard")
			.With("Left", "<Keyboard>/a", "Keyboard")
			.With("Right", "<Keyboard>/d", "Keyboard");
		InputSource.Look.Action.AddBinding("<Mouse>/delta").WithProcessor("scaleVector2(x=.05,y=.05)");
		InputSource.WheelLook.Action.AddBinding("<Mouse>/delta").WithProcessor("scaleVector2(x=.05,y=.05)");
		InputSource.Jump.Action.AddBinding("<Keyboard>/space").WithGroup("Keyboard");
		InputSource.Dodge.Action.AddBinding("<Keyboard>/leftShift").WithGroup("Keyboard");
		InputSource.Slide.Action.AddBinding("<Keyboard>/leftCtrl").WithGroup("Keyboard");
		InputSource.Fire1.Action.AddBinding("<Mouse>/leftButton").WithGroup("Keyboard");
		InputSource.Fire2.Action.AddBinding("<Mouse>/rightButton").WithGroup("Keyboard");
		InputSource.Punch.Action.AddBinding("<Keyboard>/f").WithGroup("Keyboard");
		InputSource.Hook.Action.AddBinding("<Keyboard>/r").WithGroup("Keyboard");
		InputSource.LastWeapon.Action.AddBinding("<Keyboard>/q").WithGroup("Keyboard");
		InputSource.ChangeVariation.Action.AddBinding("<Keyboard>/e").WithGroup("Keyboard");
		InputSource.ChangeFist.Action.AddBinding("<Keyboard>/g").WithGroup("Keyboard");
		InputSource.Pause.Action.AddBinding("<Keyboard>/escape").WithGroup("Keyboard");
		InputSource.Stats.Action.AddBinding("<Keyboard>/tab").WithGroup("Keyboard");
		InputSource.Slot0.Action.AddBinding("<Keyboard>/0").WithGroup("Keyboard");
		InputSource.Slot1.Action.AddBinding("<Keyboard>/1").WithGroup("Keyboard");
		InputSource.Slot2.Action.AddBinding("<Keyboard>/2").WithGroup("Keyboard");
		InputSource.Slot3.Action.AddBinding("<Keyboard>/3").WithGroup("Keyboard");
		InputSource.Slot4.Action.AddBinding("<Keyboard>/4").WithGroup("Keyboard");
		InputSource.Slot5.Action.AddBinding("<Keyboard>/5").WithGroup("Keyboard");
		InputSource.Slot6.Action.AddBinding("<Keyboard>/6").WithGroup("Keyboard");
		InputSource.Slot7.Action.AddBinding("<Keyboard>/7").WithGroup("Keyboard");
		InputSource.Slot8.Action.AddBinding("<Keyboard>/8").WithGroup("Keyboard");
		InputSource.Slot9.Action.AddBinding("<Keyboard>/9").WithGroup("Keyboard");
		InputSource.Enable();
		bindings = new BindingInfo[24]
		{
			new BindingInfo
			{
				Action = InputSource.Move.Action,
				DefaultKey = KeyCode.W,
				Name = "W"
			},
			new BindingInfo
			{
				Action = InputSource.Move.Action,
				Offset = 1,
				DefaultKey = KeyCode.S,
				Name = "S"
			},
			new BindingInfo
			{
				Action = InputSource.Move.Action,
				Offset = 2,
				DefaultKey = KeyCode.A,
				Name = "A"
			},
			new BindingInfo
			{
				Action = InputSource.Move.Action,
				Offset = 3,
				DefaultKey = KeyCode.D,
				Name = "D"
			},
			new BindingInfo
			{
				Action = InputSource.Jump.Action,
				DefaultKey = KeyCode.Space,
				Name = "Jump"
			},
			new BindingInfo
			{
				Action = InputSource.Dodge.Action,
				DefaultKey = KeyCode.LeftShift,
				Name = "Dodge"
			},
			new BindingInfo
			{
				Action = InputSource.Slide.Action,
				DefaultKey = KeyCode.LeftControl,
				Name = "Slide"
			},
			new BindingInfo
			{
				Action = InputSource.Fire1.Action,
				DefaultKey = KeyCode.Mouse0,
				Name = "Fire1"
			},
			new BindingInfo
			{
				Action = InputSource.Fire2.Action,
				DefaultKey = KeyCode.Mouse1,
				Name = "Fire2"
			},
			new BindingInfo
			{
				Action = InputSource.Punch.Action,
				DefaultKey = KeyCode.F,
				Name = "Punch"
			},
			new BindingInfo
			{
				Action = InputSource.Hook.Action,
				DefaultKey = KeyCode.R,
				Name = "Hook"
			},
			new BindingInfo
			{
				Action = InputSource.LastWeapon.Action,
				DefaultKey = KeyCode.Q,
				Name = "LastUsedWeapon"
			},
			new BindingInfo
			{
				Action = InputSource.ChangeVariation.Action,
				DefaultKey = KeyCode.E,
				Name = "ChangeVariation"
			},
			new BindingInfo
			{
				Action = InputSource.ChangeFist.Action,
				DefaultKey = KeyCode.G,
				Name = "ChangeFist"
			},
			new BindingInfo
			{
				Action = InputSource.Slot0.Action,
				DefaultKey = KeyCode.Alpha0,
				Name = "Slot0"
			},
			new BindingInfo
			{
				Action = InputSource.Slot1.Action,
				DefaultKey = KeyCode.Alpha1,
				Name = "Slot1"
			},
			new BindingInfo
			{
				Action = InputSource.Slot2.Action,
				DefaultKey = KeyCode.Alpha2,
				Name = "Slot2"
			},
			new BindingInfo
			{
				Action = InputSource.Slot3.Action,
				DefaultKey = KeyCode.Alpha3,
				Name = "Slot3"
			},
			new BindingInfo
			{
				Action = InputSource.Slot4.Action,
				DefaultKey = KeyCode.Alpha4,
				Name = "Slot4"
			},
			new BindingInfo
			{
				Action = InputSource.Slot5.Action,
				DefaultKey = KeyCode.Alpha5,
				Name = "Slot5"
			},
			new BindingInfo
			{
				Action = InputSource.Slot6.Action,
				DefaultKey = KeyCode.Alpha6,
				Name = "Slot6"
			},
			new BindingInfo
			{
				Action = InputSource.Slot7.Action,
				DefaultKey = KeyCode.Alpha7,
				Name = "Slot7"
			},
			new BindingInfo
			{
				Action = InputSource.Slot8.Action,
				DefaultKey = KeyCode.Alpha8,
				Name = "Slot8"
			},
			new BindingInfo
			{
				Action = InputSource.Slot9.Action,
				DefaultKey = KeyCode.Alpha9,
				Name = "Slot9"
			}
		};
		if (MonoSingleton<PrefsManager>.Instance.GetBool("scrollEnabled"))
		{
			ScrOn = true;
		}
		else
		{
			ScrOn = false;
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBool("scrollWeapons"))
		{
			ScrWep = true;
		}
		else
		{
			ScrWep = false;
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBool("scrollVariations"))
		{
			ScrVar = true;
		}
		else
		{
			ScrVar = false;
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBool("scrollReversed"))
		{
			ScrRev = true;
		}
		else
		{
			ScrRev = false;
		}
		UpdateBindings();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		anyButtonListener = onAnyInput.Subscribe(ButtonPressListener.Instance);
	}

	private void OnDisable()
	{
		anyButtonListener?.Dispose();
	}

	public void UpdateBindings()
	{
		BindingInfo[] array = bindings;
		foreach (BindingInfo bindingInfo in array)
		{
			InputBinding bindingMask = InputBinding.MaskByGroup("Keyboard");
			int bindingIndex = bindingInfo.Action.GetBindingIndex(bindingMask);
			Inputs[bindingInfo.Name] = (KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt(bindingInfo.PrefName, (int)bindingInfo.DefaultKey);
			if (bindingIndex != -1 && MonoSingleton<PrefsManager>.Instance.HasKey(bindingInfo.PrefName))
			{
				KeyCode @int = (KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt(bindingInfo.PrefName);
				if (LegacyInput.current.TryGetButton(@int, out var button))
				{
					bindingInfo.Action.ChangeBinding(bindingIndex + bindingInfo.Offset).WithPath(button.path);
				}
			}
		}
	}
}
