using System;
using UnityEngine;

[Serializable]
public class SandboxSaveData
{
	public string MapName;

	public string MapIdentifier;

	public int SaveVersion = 2;

	public string GameVersion = Application.version;

	public SavedBlock[] Blocks;

	public SavedProp[] Props;

	public SavedEnemy[] Enemies;
}
