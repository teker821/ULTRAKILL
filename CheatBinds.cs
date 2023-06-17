using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class CheatBinds : MonoSingleton<CheatBinds>
{
	public Dictionary<string, InputActionState> registeredCheatBinds;

	public bool isRebinding;

	private readonly Dictionary<string, string> defaultBinds = new Dictionary<string, string>
	{
		{ "ultrakill.flight", "<Keyboard>/b" },
		{ "ultrakill.noclip", "<Keyboard>/v" },
		{ "ultrakill.blind-enemies", "<Keyboard>/m" },
		{ "ultrakill.infinite-wall-jumps", "<Keyboard>/n" },
		{ "ultrakill.no-weapon-cooldown", "<Keyboard>/c" },
		{ "ultrakill.keep-enabled", "<Keyboard>/o" },
		{ "ultrakill.teleport-menu", "<Keyboard>/l" },
		{ "ultrakill.spawner-arm", "<Keyboard>/p" },
		{ "ultrakill.disable-enemy-spawns", "<Keyboard>/i" },
		{ "ultrakill.sandbox.physics", "<Keyboard>/j" },
		{ "ultrakill.sandbox.snapping", "<Keyboard>/h" }
	};

	private readonly string[] bannedBinds = new string[5] { "/Keyboard/home", "/Keyboard/backquote", "/Mouse/press", "/Mouse/leftButton", "/Mouse/rightButton" };

	private InputAction rebindAction;

	private ICheat rebindCheat;

	protected override void Awake()
	{
		base.Awake();
		registeredCheatBinds = new Dictionary<string, InputActionState>();
	}

	public void RestoreBinds(Dictionary<string, List<ICheat>> allRegisteredCheats)
	{
		foreach (KeyValuePair<string, List<ICheat>> allRegisteredCheat in allRegisteredCheats)
		{
			foreach (ICheat item in allRegisteredCheat.Value)
			{
				string @string = MonoSingleton<PrefsManager>.Instance.GetString("cheatBinding." + item.Identifier, string.Empty);
				if (string.IsNullOrEmpty(@string))
				{
					if (defaultBinds.ContainsKey(item.Identifier))
					{
						AddBinding(item.Identifier, defaultBinds[item.Identifier]);
					}
				}
				else if (!string.IsNullOrEmpty(@string) && @string != "blank")
				{
					AddBinding(item.Identifier, @string);
				}
			}
		}
	}

	public void ResetCheatBind(string cheatIdentifier)
	{
		if (isRebinding)
		{
			CancelRebind();
		}
		if (registeredCheatBinds.ContainsKey(cheatIdentifier))
		{
			registeredCheatBinds[cheatIdentifier].Action.Disable();
			registeredCheatBinds[cheatIdentifier].Action.Dispose();
			registeredCheatBinds.Remove(cheatIdentifier);
		}
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cheatBinding." + cheatIdentifier);
		if (defaultBinds.ContainsKey(cheatIdentifier))
		{
			AddBinding(cheatIdentifier, defaultBinds[cheatIdentifier]);
		}
	}

	public void CancelRebind()
	{
		rebindAction.Disable();
		rebindAction.Dispose();
		MonoSingleton<OptionsManager>.Instance.dontUnpause = false;
		isRebinding = false;
		MonoSingleton<CheatsManager>.Instance.UpdateCheatState(rebindCheat);
	}

	public void SetupRebind(ICheat targetCheat)
	{
		isRebinding = true;
		MonoSingleton<OptionsManager>.Instance.dontUnpause = true;
		rebindCheat = targetCheat;
		rebindAction = new InputAction(null, InputActionType.Value, "<Keyboard>/*");
		rebindAction.AddBinding("<Mouse>/<Button>");
		rebindAction.performed += RebindHandler;
		rebindAction.Enable();
		void RebindHandler(InputAction.CallbackContext context)
		{
			if (!(context.control.path == "/Keyboard/anyKey"))
			{
				if (context.control.path == "/Keyboard/escape")
				{
					isRebinding = false;
					rebindAction.performed -= RebindHandler;
					MonoSingleton<PrefsManager>.Instance.SetString("cheatBinding." + targetCheat.Identifier, "blank");
					MonoSingleton<OptionsManager>.Instance.dontUnpause = false;
					if (registeredCheatBinds.ContainsKey(targetCheat.Identifier))
					{
						registeredCheatBinds[targetCheat.Identifier].Action.Disable();
						registeredCheatBinds[targetCheat.Identifier].Action.Dispose();
						registeredCheatBinds.Remove(targetCheat.Identifier);
						MonoSingleton<CheatsManager>.Instance.UpdateCheatState(targetCheat);
					}
				}
				else if (!bannedBinds.Contains(context.control.path))
				{
					isRebinding = false;
					Rebind(targetCheat.Identifier, context.control.path);
					MonoSingleton<CheatsManager>.Instance.UpdateCheatState(targetCheat);
					rebindAction.performed -= RebindHandler;
					MonoSingleton<OptionsManager>.Instance.dontUnpause = false;
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		foreach (KeyValuePair<string, InputActionState> registeredCheatBind in registeredCheatBinds)
		{
			registeredCheatBind.Value.Action.Disable();
			registeredCheatBind.Value.Action.Dispose();
		}
		registeredCheatBinds.Clear();
	}

	private void Rebind(string cheatIdentifier, string path)
	{
		if (registeredCheatBinds.ContainsKey(cheatIdentifier))
		{
			registeredCheatBinds[cheatIdentifier].Action.Disable();
			registeredCheatBinds[cheatIdentifier].Action.Dispose();
			registeredCheatBinds.Remove(cheatIdentifier);
		}
		AddBinding(cheatIdentifier, path);
		MonoSingleton<PrefsManager>.Instance.SetString("cheatBinding." + cheatIdentifier, path);
	}

	private void AddBinding(string cheatIdentifier, string path)
	{
		registeredCheatBinds.Add(cheatIdentifier, new InputActionState(new InputAction(cheatIdentifier)));
		registeredCheatBinds[cheatIdentifier].Action.AddBinding(path).WithGroup("Keyboard");
		registeredCheatBinds[cheatIdentifier].Action.performed += delegate
		{
			MonoSingleton<CheatsManager>.Instance.HandleCheatBind(cheatIdentifier);
		};
		registeredCheatBinds[cheatIdentifier].Action.Enable();
	}

	public string ResolveCheatKey(string cheatIdentifier)
	{
		if (!registeredCheatBinds.ContainsKey(cheatIdentifier))
		{
			return null;
		}
		return registeredCheatBinds[cheatIdentifier].Action.bindings[0].ToDisplayString();
	}
}
