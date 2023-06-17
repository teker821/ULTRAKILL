using Sandbox.Arm;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class CheatsController : MonoSingleton<CheatsController>
{
	public GameObject spawnerArm;

	public GameObject fullBrightLight;

	private static readonly KeyCode[] Sequence = new KeyCode[10]
	{
		KeyCode.UpArrow,
		KeyCode.UpArrow,
		KeyCode.DownArrow,
		KeyCode.DownArrow,
		KeyCode.LeftArrow,
		KeyCode.RightArrow,
		KeyCode.LeftArrow,
		KeyCode.RightArrow,
		KeyCode.B,
		KeyCode.A
	};

	[Space]
	[SerializeField]
	private GameObject consentScreen;

	[SerializeField]
	private GameObject cheatsEnabledPanel;

	[SerializeField]
	private GameObject cheatsInfoPanel;

	[SerializeField]
	public GameObject cheatsTeleportPanel;

	public Text cheatsInfo;

	[Space]
	[SerializeField]
	private AudioSource cheatEnabledSound;

	[SerializeField]
	private AudioSource cheatDisabledSound;

	private int sequenceIndex;

	public bool cheatsEnabled;

	private bool noclip;

	private bool flight;

	private bool infiniteJumps;

	private bool stayEnabled;

	public SandboxArm arm;

	private bool spawnerArmSpawned => arm;

	private static bool TryGetKeyboardButton(int sequenceIndex, out ButtonControl button)
	{
		button = null;
		if (Keyboard.current == null)
		{
			return false;
		}
		switch (Sequence[sequenceIndex])
		{
		case KeyCode.UpArrow:
			button = Keyboard.current.upArrowKey;
			break;
		case KeyCode.DownArrow:
			button = Keyboard.current.downArrowKey;
			break;
		case KeyCode.LeftArrow:
			button = Keyboard.current.leftArrowKey;
			break;
		case KeyCode.RightArrow:
			button = Keyboard.current.rightArrowKey;
			break;
		case KeyCode.A:
			button = Keyboard.current.aKey;
			break;
		case KeyCode.B:
			button = Keyboard.current.bKey;
			break;
		}
		return button != null;
	}

	private static bool TryGetGamepadButton(int sequenceIndex, out ButtonControl button)
	{
		button = null;
		if (Gamepad.current == null)
		{
			return false;
		}
		switch (Sequence[sequenceIndex])
		{
		case KeyCode.UpArrow:
			button = Gamepad.current.dpad.up;
			break;
		case KeyCode.DownArrow:
			button = Gamepad.current.dpad.down;
			break;
		case KeyCode.LeftArrow:
			button = Gamepad.current.dpad.left;
			break;
		case KeyCode.RightArrow:
			button = Gamepad.current.dpad.right;
			break;
		case KeyCode.A:
			button = Gamepad.current.buttonSouth;
			break;
		case KeyCode.B:
			button = Gamepad.current.buttonEast;
			break;
		}
		return button != null;
	}

	public void ShowTeleportPanel()
	{
		cheatsTeleportPanel.SetActive(value: true);
		GameStateManager.Instance.RegisterState(new GameState("teleport-menu", cheatsTeleportPanel)
		{
			cursorLock = LockMode.Unlock
		});
		MonoSingleton<OptionsManager>.Instance.Freeze();
	}

	private void Start()
	{
		if (CheatsManager.KeepCheatsEnabled)
		{
			MonoSingleton<AssistController>.Instance.cheatsEnabled = true;
			consentScreen.SetActive(value: false);
			cheatsEnabled = true;
			cheatsEnabledPanel.SetActive(value: true);
		}
	}

	public void PlayToggleSound(bool newState)
	{
		if (newState)
		{
			cheatEnabledSound.Play();
		}
		else
		{
			cheatDisabledSound.Play();
		}
	}

	private void ProcessInput()
	{
		TryGetGamepadButton(sequenceIndex, out var button);
		TryGetKeyboardButton(sequenceIndex, out var button2);
		if ((button2 != null && button2.wasPressedThisFrame) || (button != null && button.wasPressedThisFrame))
		{
			sequenceIndex++;
			if (sequenceIndex == Sequence.Length)
			{
				MonoSingleton<OptionsManager>.Instance.Pause();
				consentScreen.SetActive(value: true);
				sequenceIndex = 0;
			}
		}
		else
		{
			Keyboard current = Keyboard.current;
			if ((current != null && current.anyKey.wasPressedThisFrame) || AnyGamepadButtonPressed())
			{
				sequenceIndex = 0;
			}
		}
	}

	private bool AnyGamepadButtonPressed()
	{
		if (Gamepad.current == null)
		{
			return false;
		}
		foreach (InputControl allControl in Gamepad.current.allControls)
		{
			if (allControl is ButtonControl buttonControl && buttonControl.wasPressedThisFrame)
			{
				return true;
			}
		}
		return false;
	}

	private bool GamepadCombo()
	{
		if (Gamepad.current == null)
		{
			return false;
		}
		if (Gamepad.current.selectButton.isPressed)
		{
			return Gamepad.current.rightTrigger.wasPressedThisFrame;
		}
		return false;
	}

	public void Update()
	{
		if (!cheatsEnabled)
		{
			ProcessInput();
		}
		cheatsInfoPanel.SetActive(cheatsEnabled);
		if (MonoSingleton<CheatBinds>.Instance.isRebinding || !cheatsEnabled)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Home) || Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote) || GamepadCombo())
		{
			if (MonoSingleton<OptionsManager>.Instance.paused)
			{
				Debug.Log("Unpaused");
				if (SandboxHud.SavesMenuOpen)
				{
					MonoSingleton<SandboxHud>.Instance.HideSavesMenu();
				}
				MonoSingleton<CheatsManager>.Instance.HideMenu();
			}
			else
			{
				Debug.Log("Unpaused");
				MonoSingleton<OptionsManager>.Instance.Pause();
				if (Debug.isDebugBuild)
				{
					cheatsEnabledPanel.SetActive(cheatsInfoPanel.activeSelf);
				}
				MonoSingleton<CheatsManager>.Instance.ShowMenu();
			}
		}
		if (HideCheatsStatus.HideStatus)
		{
			cheatsInfoPanel.SetActive(value: false);
			cheatsEnabledPanel.SetActive(value: false);
		}
	}

	public void ActivateCheats()
	{
		MonoSingleton<AssistController>.Instance.cheatsEnabled = true;
		consentScreen.SetActive(value: false);
		cheatsEnabledPanel.SetActive(value: true);
		cheatsEnabled = true;
	}

	public void Cancel()
	{
		consentScreen.SetActive(value: false);
	}
}
