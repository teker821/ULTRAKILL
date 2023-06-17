using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Cheat Assets", menuName = "ULTRAKILL/Cheat Asset DB")]
public class CheatAssetObject : ScriptableObject
{
	[Serializable]
	public struct KeyIcon
	{
		public string key;

		public Sprite sprite;
	}

	public new string name;

	[Header("Cheats")]
	[FormerlySerializedAs("GenericIcon")]
	public Sprite genericCheatIcon;

	[FormerlySerializedAs("Icons")]
	public KeyIcon[] cheatIcons;

	[Header("Sandbox")]
	public Sprite genericSandboxToolIcon;

	[FormerlySerializedAs("sandboxToolIcons")]
	public KeyIcon[] sandboxMenuIcons;

	public KeyIcon[] sandboxArmHoloIcons;
}
