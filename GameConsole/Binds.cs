using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace GameConsole;

public class Binds
{
	public Dictionary<string, InputActionState> registeredBinds;

	public Dictionary<string, string> defaultBinds = new Dictionary<string, string>
	{
		{ "open", "/Keyboard/f8" },
		{ "submit", "/Keyboard/enter" },
		{ "command_history_up", "/Keyboard/upArrow" },
		{ "command_history_down", "/Keyboard/downArrow" },
		{ "scroll_up", "/Keyboard/pageUp" },
		{ "scroll_down", "/Keyboard/pageUp" },
		{ "scroll_to_bottom", "/Keyboard/home" },
		{ "scroll_to_top", "/Keyboard/end" }
	};

	public bool OpenPressed => SafeWasPerformed("open");

	public bool SubmitPressed => SafeWasPerformed("submit");

	public bool CommandHistoryUpPressed => SafeWasPerformed("command_history_up");

	public bool CommandHistoryDownPressed => SafeWasPerformed("command_history_down");

	public bool ScrollUpPressed => SafeWasPerformed("scroll_up");

	public bool ScrollDownPressed => SafeWasPerformed("scroll_down");

	public bool ScrollToBottomPressed => SafeWasPerformed("scroll_to_bottom");

	public bool ScrollToTopPressed => SafeWasPerformed("scroll_to_top");

	public bool ScrollUpHeld => SafeIsHeld("scroll_up");

	public bool ScrollDownHeld => SafeIsHeld("scroll_down");

	private bool SafeWasPerformed(string key)
	{
		if (registeredBinds != null && registeredBinds.ContainsKey(key))
		{
			return registeredBinds[key].WasPerformedThisFrame;
		}
		return false;
	}

	private bool SafeIsHeld(string key)
	{
		if (registeredBinds != null && registeredBinds.ContainsKey(key))
		{
			return registeredBinds[key].IsPressed;
		}
		return false;
	}

	public void Initialize()
	{
		registeredBinds = new Dictionary<string, InputActionState>();
		foreach (KeyValuePair<string, string> defaultBind in defaultBinds)
		{
			registeredBinds.Add(defaultBind.Key, new InputActionState(new InputAction(defaultBind.Key)));
			registeredBinds[defaultBind.Key].Action.AddBinding(MonoSingleton<PrefsManager>.Instance.GetString("consoleBinding." + defaultBind.Key, defaultBind.Value)).WithGroup("Keyboard");
			registeredBinds[defaultBind.Key].Action.Enable();
		}
	}

	public void Rebind(string key, string bind)
	{
		if (!defaultBinds.ContainsKey(key))
		{
			MonoSingleton<Console>.Instance.PrintLine("Invalid console bind key: " + key);
			return;
		}
		MonoSingleton<PrefsManager>.Instance.SetString("consoleBinding." + key, bind);
		registeredBinds[key].Action.Disable();
		registeredBinds[key].Action.Dispose();
		registeredBinds[key] = new InputActionState(new InputAction(key));
		registeredBinds[key].Action.AddBinding(bind).WithGroup("Keyboard");
		registeredBinds[key].Action.Enable();
		MonoSingleton<Console>.Instance.UpdateDisplayString();
	}
}
