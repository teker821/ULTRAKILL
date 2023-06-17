using UnityEngine;

public static class LayerMaskDefaults
{
	public static LayerMask Get(LMD lmd)
	{
		LayerMask layerMask = default(LayerMask);
		switch (lmd)
		{
		case LMD.Enemies:
			layerMask = (int)layerMask | 0x400;
			return (int)layerMask | 0x800;
		case LMD.Environment:
			layerMask = (int)layerMask | 0x100;
			return (int)layerMask | 0x1000000;
		case LMD.EnvironmentAndBigEnemies:
			layerMask = (int)layerMask | 0x100;
			layerMask = (int)layerMask | 0x1000000;
			return (int)layerMask | 0x800;
		case LMD.EnemiesAndEnvironment:
			layerMask = (int)layerMask | 0x100;
			layerMask = (int)layerMask | 0x1000000;
			layerMask = (int)layerMask | 0x400;
			return (int)layerMask | 0x800;
		case LMD.EnemiesAndPlayer:
			layerMask = (int)layerMask | 4;
			layerMask = (int)layerMask | 0x400;
			return (int)layerMask | 0x800;
		case LMD.EnvironmentAndPlayer:
			layerMask = (int)layerMask | 4;
			layerMask = (int)layerMask | 0x100;
			return (int)layerMask | 0x1000000;
		case LMD.EnemiesEnvironmentAndPlayer:
			layerMask = (int)layerMask | 4;
			layerMask = (int)layerMask | 0x100;
			layerMask = (int)layerMask | 0x1000000;
			layerMask = (int)layerMask | 0x400;
			return (int)layerMask | 0x800;
		default:
			return layerMask;
		}
	}
}
