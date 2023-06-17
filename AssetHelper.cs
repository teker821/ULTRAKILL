using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[ConfigureSingleton(SingletonFlags.NoAutoInstance | SingletonFlags.PersistAutoInstance | SingletonFlags.DestroyDuplicates)]
public class AssetHelper : MonoSingleton<AssetHelper>
{
	protected override void OnEnable()
	{
		base.OnEnable();
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public static GameObject LoadPrefab(string address)
	{
		return Addressables.LoadAssetAsync<GameObject>(address).WaitForCompletion();
	}

	public static void SpawnPrefabAsync(string prefab, Vector3 position, Quaternion rotation)
	{
		MonoSingleton<AssetHelper>.Instance.StartCoroutine(MonoSingleton<AssetHelper>.Instance.LoadPrefab(prefab, position, rotation));
	}

	public IEnumerator LoadPrefab(string prefab, Vector3 position, Quaternion rotation)
	{
		AsyncOperationHandle<GameObject> loadOperation = Addressables.LoadAssetAsync<GameObject>(prefab);
		yield return loadOperation;
		Object.Instantiate(loadOperation.Result, position, rotation);
	}
}
