using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sandbox;
using UnityEngine;
using UnityEngine.SceneManagement;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class SandboxSaver : MonoSingleton<SandboxSaver>
{
	public const string SaveExtension = ".pitr";

	[SerializeField]
	private SpawnableObjectsDatabase objects;

	private Dictionary<string, SpawnableObject> registeredObject;

	public static string SavePath => Path.Combine(GameProgressSaver.BaseSavePath, "Sandbox");

	private static void SetupDirs()
	{
		if (!Directory.Exists(SavePath))
		{
			Directory.CreateDirectory(SavePath);
		}
	}

	public string[] ListSaves()
	{
		SetupDirs();
		return (from f in new DirectoryInfo(SavePath).GetFileSystemInfos()
			orderby f.LastWriteTime descending
			select Path.GetFileNameWithoutExtension(f.Name)).ToArray();
	}

	public void QuickSave()
	{
		Save($"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} {DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}");
	}

	public void QuickLoad()
	{
		string[] array = ListSaves();
		if (array.Length != 0)
		{
			Load(array[0]);
		}
	}

	public void Delete(string name)
	{
		SetupDirs();
		string path = Path.Combine(SavePath, name + ".pitr");
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public void Save(string name)
	{
		SetupDirs();
		CreateSaveAndWrite(name);
	}

	public void Load(string name)
	{
		SetupDirs();
		Clear();
		RebuildObjectList();
		SandboxSaveData sandboxSaveData = JsonConvert.DeserializeObject<SandboxSaveData>(File.ReadAllText(Path.Combine(SavePath, name + ".pitr")));
		Debug.Log($"Loaded {sandboxSaveData.Blocks.Length} blocks\nLoaded {sandboxSaveData.Props.Length} props");
		Debug.Log("Save Version: " + sandboxSaveData.SaveVersion);
		Vector3? vector = null;
		Vector3 position = MonoSingleton<NewMovement>.Instance.transform.position;
		SavedProp[] props = sandboxSaveData.Props;
		foreach (SavedProp savedProp in props)
		{
			RecreateProp(savedProp, sandboxSaveData.SaveVersion > 1);
			if (!(savedProp.ObjectIdentifier != "ultrakill.spawn-point"))
			{
				if (!vector.HasValue)
				{
					vector = savedProp.Position.ToVector3();
				}
				else if (Vector3.Distance(position, savedProp.Position.ToVector3()) < Vector3.Distance(position, vector.Value))
				{
					vector = savedProp.Position.ToVector3();
				}
			}
		}
		if (vector.HasValue)
		{
			MonoSingleton<NewMovement>.Instance.transform.position = vector.Value;
			MonoSingleton<NewMovement>.Instance.rb.velocity = Vector3.zero;
		}
		SavedBlock[] blocks = sandboxSaveData.Blocks;
		foreach (SavedBlock block in blocks)
		{
			RecreateBlock(block);
		}
		MonoSingleton<SandboxNavmesh>.Instance.Rebake();
		SavedEnemy[] enemies = sandboxSaveData.Enemies;
		foreach (SavedEnemy genericObject in enemies)
		{
			RecreateEnemy(genericObject, sandboxSaveData.SaveVersion > 1);
		}
	}

	private void RecreateEnemy(SavedGeneric genericObject, bool newSizing)
	{
		if (!registeredObject.ContainsKey(genericObject.ObjectIdentifier))
		{
			Debug.LogError(genericObject.ObjectIdentifier + " missing from registered objects");
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(registeredObject[genericObject.ObjectIdentifier].gameObject);
		gameObject.transform.position = genericObject.Position.ToVector3();
		if (!newSizing)
		{
			gameObject.transform.localScale = genericObject.Scale.ToVector3();
		}
		if (gameObject.TryGetComponent<KeepInBounds>(out var component))
		{
			component.ForceApproveNewPosition();
		}
		SandboxEnemy sandboxEnemy = gameObject.AddComponent<SandboxEnemy>();
		sandboxEnemy.sourceObject = registeredObject[genericObject.ObjectIdentifier];
		sandboxEnemy.RestoreRadiance(((SavedEnemy)genericObject).Radiance);
		if (genericObject is SavedPhysical savedPhysical && savedPhysical.Kinematic)
		{
			sandboxEnemy.Pause();
		}
		if (newSizing)
		{
			sandboxEnemy.SetSize(genericObject.Scale.ToVector3());
		}
		ApplyData(gameObject, genericObject.Data);
	}

	private void RecreateProp(SavedProp prop, bool newSizing)
	{
		if (!registeredObject.ContainsKey(prop.ObjectIdentifier))
		{
			Debug.LogError(prop.ObjectIdentifier + " missing from registered objects");
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(registeredObject[prop.ObjectIdentifier].gameObject);
		gameObject.transform.position = prop.Position.ToVector3();
		gameObject.transform.rotation = prop.Rotation.ToQuaternion();
		if (!newSizing)
		{
			gameObject.transform.localScale = prop.Scale.ToVector3();
		}
		SandboxProp component = gameObject.GetComponent<SandboxProp>();
		component.sourceObject = registeredObject[prop.ObjectIdentifier];
		if (newSizing)
		{
			component.SetSize(prop.Scale.ToVector3());
		}
		if (prop.Kinematic)
		{
			component.Pause();
		}
		ApplyData(gameObject, prop.Data);
	}

	private void RecreateBlock(SavedBlock block)
	{
		if (!registeredObject.ContainsKey(block.ObjectIdentifier))
		{
			Debug.LogError(block.ObjectIdentifier + " missing from registered objects");
			return;
		}
		SpawnableObject spawnableObject = registeredObject[block.ObjectIdentifier];
		GameObject gameObject = SandboxUtils.CreateFinalBlock(spawnableObject, block.Position.ToVector3(), block.BlockSize.ToVector3(), spawnableObject.isWater);
		gameObject.transform.rotation = block.Rotation.ToQuaternion();
		SandboxProp component = gameObject.GetComponent<SandboxProp>();
		component.sourceObject = registeredObject[block.ObjectIdentifier];
		if (block.Kinematic)
		{
			component.Pause();
		}
		else
		{
			component.Resume();
		}
		ApplyData(gameObject, block.Data);
	}

	private void ApplyData(GameObject go, SavedAlterData[] data)
	{
		IAlter[] componentsInChildren = go.GetComponentsInChildren<IAlter>();
		foreach (IAlter alterComponent in componentsInChildren)
		{
			if (data == null)
			{
				Debug.LogWarning("No data for " + go.name);
				continue;
			}
			if (!data.Select((SavedAlterData d) => d.Key).Contains(alterComponent.alterKey))
			{
				Debug.LogWarning("No data for " + alterComponent.alterKey + " on " + go.name);
				continue;
			}
			SavedAlterData savedAlterData = data.FirstOrDefault((SavedAlterData d) => d.Key == alterComponent.alterKey);
			if (savedAlterData == null)
			{
				continue;
			}
			SavedAlterOption[] options2 = savedAlterData.Options;
			foreach (SavedAlterOption options in options2)
			{
				if (options.BoolValue.HasValue && alterComponent is IAlterOptions<bool> alterOptions)
				{
					AlterOption<bool> alterOption = alterOptions.options.FirstOrDefault((AlterOption<bool> o) => o.key == options.Key);
					if (alterOption == null)
					{
						continue;
					}
					alterOption.callback?.Invoke(options.BoolValue.Value);
				}
				if (options.FloatValue.HasValue && alterComponent is IAlterOptions<float> alterOptions2)
				{
					alterOptions2.options.FirstOrDefault((AlterOption<float> o) => o.key == options.Key)?.callback?.Invoke(options.FloatValue.Value);
				}
			}
		}
	}

	public void RebuildObjectList()
	{
		if (registeredObject == null)
		{
			registeredObject = new Dictionary<string, SpawnableObject>();
		}
		registeredObject.Clear();
		RegisterObjects(objects.objects);
		RegisterObjects(objects.enemies);
		RegisterObjects(objects.sandboxTools);
		RegisterObjects(objects.sandboxObjects);
		RegisterObjects(objects.specialSandbox);
	}

	private void RegisterObjects(SpawnableObject[] objs)
	{
		foreach (SpawnableObject spawnableObject in objs)
		{
			if (!string.IsNullOrEmpty(spawnableObject.identifier))
			{
				if (registeredObject.ContainsKey(spawnableObject.identifier))
				{
					Debug.LogError("Duplicate Object Identifier, pls fix. " + spawnableObject.identifier);
				}
				else
				{
					registeredObject.Add(spawnableObject.identifier, spawnableObject);
				}
			}
		}
	}

	public static void Clear()
	{
		DefaultSandboxCheckpoint defaultSandboxCheckpoint = MonoSingleton<DefaultSandboxCheckpoint>.Instance;
		if (defaultSandboxCheckpoint == null)
		{
			MonoSingleton<StatsManager>.Instance.currentCheckPoint = null;
		}
		else
		{
			MonoSingleton<StatsManager>.Instance.currentCheckPoint = defaultSandboxCheckpoint.checkpoint;
		}
		SandboxSpawnableInstance[] array = UnityEngine.Object.FindObjectsOfType<SandboxSpawnableInstance>();
		for (int i = 0; i < array.Length; i++)
		{
			UnityEngine.Object.Destroy(array[i].gameObject);
		}
		Resources.UnloadUnusedAssets();
	}

	private static void CreateSaveAndWrite(string name)
	{
		SandboxProp[] array = UnityEngine.Object.FindObjectsOfType<SandboxProp>();
		Debug.Log($"{array.Length} props found");
		BrushBlock[] array2 = UnityEngine.Object.FindObjectsOfType<BrushBlock>();
		Debug.Log($"{array2.Length} procedural blocks found");
		SandboxEnemy[] array3 = UnityEngine.Object.FindObjectsOfType<SandboxEnemy>();
		Debug.Log($"{array3.Length} sandbox enemies found");
		List<SavedBlock> list = new List<SavedBlock>();
		BrushBlock[] array4 = array2;
		foreach (BrushBlock brushBlock in array4)
		{
			Debug.Log($"Position: {brushBlock.transform.position}\nRotation: {brushBlock.transform.rotation}\nSize: {brushBlock.DataSize}\nType: {brushBlock.Type}");
			list.Add(brushBlock.SaveBrushBlock());
		}
		List<SavedProp> list2 = new List<SavedProp>();
		SandboxProp[] array5 = array;
		foreach (SandboxProp sandboxProp in array5)
		{
			if (!sandboxProp.GetComponent<BrushBlock>())
			{
				Debug.Log($"Position: {sandboxProp.transform.position}\nRotation: {sandboxProp.transform.rotation}");
				list2.Add(sandboxProp.SaveProp());
			}
		}
		List<SavedEnemy> list3 = new List<SavedEnemy>();
		SandboxEnemy[] array6 = array3;
		for (int i = 0; i < array6.Length; i++)
		{
			SavedEnemy savedEnemy = array6[i].SaveEnemy();
			if (savedEnemy != null)
			{
				list3.Add(savedEnemy);
			}
		}
		string contents = JsonConvert.SerializeObject(new SandboxSaveData
		{
			MapName = SceneManager.GetActiveScene().name,
			Blocks = list.ToArray(),
			Props = list2.ToArray(),
			Enemies = list3.ToArray()
		});
		File.WriteAllText(Path.Combine(SavePath, name + ".pitr"), contents);
	}
}
