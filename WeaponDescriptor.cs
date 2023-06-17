using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Weapon Descriptor", menuName = "ULTRAKILL/Weapon Descriptor")]
public class WeaponDescriptor : ScriptableObject
{
	public string weaponName;

	[FormerlySerializedAs("weaponIcon")]
	public Sprite icon;

	public Sprite glowIcon;

	public WeaponVariant variationColor;
}
