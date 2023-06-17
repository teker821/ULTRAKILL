using System;
using UnityEngine;

[Serializable]
public class ForcedLoadout
{
	public VariantSetting revolver;

	public VariantSetting altRevolver;

	public VariantSetting shotgun;

	public VariantSetting nailgun;

	public VariantSetting altNailgun;

	public VariantSetting railcannon;

	public VariantSetting rocketLauncher;

	[Space]
	public ArmVariantSetting arm;
}
