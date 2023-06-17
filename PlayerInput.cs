using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerInput
{
	private readonly Dictionary<string, InputActionState> actions;

	public Dictionary<string, InputActionState> Actions => actions;

	public InputActionState Move => actions["Move"];

	public InputActionState Look => actions["Look"];

	public InputActionState WheelLook => actions["WheelLook"];

	public InputActionState Punch => actions["Punch"];

	public InputActionState Hook => actions["Hook"];

	public InputActionState Fire1 => actions["Fire1"];

	public InputActionState Fire2 => actions["Fire2"];

	public InputActionState Jump => actions["Jump"];

	public InputActionState Slide => actions["Slide"];

	public InputActionState Dodge => actions["Dodge"];

	public InputActionState ChangeFist => actions["ChangeFist"];

	public InputActionState ChangeVariation => actions["ChangeVariation"];

	public InputActionState NextWeapon => actions["NextWeapon"];

	public InputActionState PrevWeapon => actions["PrevWeapon"];

	public InputActionState LastWeapon => actions["LastWeapon"];

	public InputActionState Pause => actions["Pause"];

	public InputActionState Stats => actions["Stats"];

	public InputActionState Slot0 => actions["Slot0"];

	public InputActionState Slot1 => actions["Slot1"];

	public InputActionState Slot2 => actions["Slot2"];

	public InputActionState Slot3 => actions["Slot3"];

	public InputActionState Slot4 => actions["Slot4"];

	public InputActionState Slot5 => actions["Slot5"];

	public InputActionState Slot6 => actions["Slot6"];

	public InputActionState Slot7 => actions["Slot7"];

	public InputActionState Slot8 => actions["Slot8"];

	public InputActionState Slot9 => actions["Slot9"];

	public PlayerInput()
	{
		actions = new Dictionary<string, InputActionState>();
		AddAction(new InputAction("Move"));
		AddAction(new InputAction("Look"));
		AddAction(new InputAction("WheelLook"));
		AddAction(new InputAction("Punch", InputActionType.Button));
		AddAction(new InputAction("Hook", InputActionType.Button));
		AddAction(new InputAction("Fire1", InputActionType.Button));
		AddAction(new InputAction("Fire2", InputActionType.Button));
		AddAction(new InputAction("Jump", InputActionType.Button));
		AddAction(new InputAction("Slide", InputActionType.Button));
		AddAction(new InputAction("Dodge", InputActionType.Button));
		AddAction(new InputAction("ChangeFist", InputActionType.Button));
		AddAction(new InputAction("ChangeVariation", InputActionType.Button));
		AddAction(new InputAction("NextWeapon", InputActionType.Button));
		AddAction(new InputAction("PrevWeapon", InputActionType.Button));
		AddAction(new InputAction("LastWeapon", InputActionType.Button));
		AddAction(new InputAction("Pause", InputActionType.Button));
		AddAction(new InputAction("Stats", InputActionType.Button));
		AddAction(new InputAction("Slot0", InputActionType.Button));
		AddAction(new InputAction("Slot1", InputActionType.Button));
		AddAction(new InputAction("Slot2", InputActionType.Button));
		AddAction(new InputAction("Slot3", InputActionType.Button));
		AddAction(new InputAction("Slot4", InputActionType.Button));
		AddAction(new InputAction("Slot5", InputActionType.Button));
		AddAction(new InputAction("Slot6", InputActionType.Button));
		AddAction(new InputAction("Slot7", InputActionType.Button));
		AddAction(new InputAction("Slot8", InputActionType.Button));
		AddAction(new InputAction("Slot9", InputActionType.Button));
		AddGamepadBindings();
	}

	private void AddAction(InputAction action)
	{
		actions[action.name] = new InputActionState(action);
	}

	private void AddGamepadBindings()
	{
		Look.Action.AddBinding("<Gamepad>/rightStick").WithProcessor("scaleVector2(x=50,y=50)").WithProcessor("scaleVector2DeltaTime")
			.WithGroup("Gamepad");
		Move.Action.AddBinding("<Gamepad>/leftStick").WithGroup("Gamepad");
		WheelLook.Action.AddBinding("<Gamepad>/rightStick").WithGroup("Gamepad");
		Punch.Action.AddBinding("<Gamepad>/buttonWest").WithGroup("Gamepad");
		Hook.Action.AddCompositeBinding("ButtonWithOneModifier").With("Modifier", "<Gamepad>/leftShoulder", "Gamepad").With("Button", "<Gamepad>/rightShoulder", "Gamepad");
		Fire1.Action.AddBinding("<Gamepad>/rightTrigger").WithGroup("Gamepad").WithInteraction("hold");
		Fire2.Action.AddBinding("<Gamepad>/leftTrigger").WithGroup("Gamepad").WithInteraction("hold");
		Jump.Action.AddBinding("<Gamepad>/buttonSouth").WithGroup("Gamepad");
		Slide.Action.AddBinding("<Gamepad>/buttonEast").WithGroup("Gamepad");
		Slide.Action.AddBinding("<Gamepad>/rightStickPress").WithGroup("Gamepad");
		Dodge.Action.AddBinding("<Gamepad>/leftStickPress").WithGroup("Gamepad");
		ChangeFist.Action.AddBinding("<Gamepad>/dpad/down").WithGroup("Gamepad");
		ChangeVariation.Action.AddBinding("<Gamepad>/buttonNorth").WithGroup("Gamepad");
		LastWeapon.Action.AddBinding("<Gamepad>/dpad/up").WithGroup("Gamepad");
		NextWeapon.Action.AddBinding("<Gamepad>/rightShoulder").WithGroup("Gamepad");
		PrevWeapon.Action.AddBinding("<Gamepad>/leftShoulder").WithGroup("Gamepad");
		Pause.Action.AddBinding("<Gamepad>/start").WithGroup("Gamepad");
		Stats.Action.AddBinding("<Gamepad>/select").WithGroup("Gamepad");
	}

	public void Enable()
	{
		foreach (KeyValuePair<string, InputActionState> action in actions)
		{
			action.Value.Action.Enable();
		}
	}

	public void Disable()
	{
		foreach (KeyValuePair<string, InputActionState> action in actions)
		{
			action.Value.Action.Disable();
		}
	}
}
