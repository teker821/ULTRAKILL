using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[DefaultExecutionOrder(-20)]
public class PrefabReplacer : MonoBehaviour
{
	public static bool ForceDisable;

	public static PrefabReplacer Instance;

	private void Awake()
	{
		Instance = this;
	}

	public GameObject LoadPrefab(string address)
	{
		return AssetHelper.LoadPrefab(address);
	}

	private void PerformSwap(PlaceholderPrefab placeholder)
	{
		if (ForceDisable || !placeholder.gameObject.activeSelf)
		{
			return;
		}
		Debug.Log("Swapping " + placeholder.name);
		Transform transform = placeholder.transform;
		if (!(transform == null))
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(placeholder.uniqueId);
			asyncOperationHandle.WaitForCompletion();
			GameObject gameObject = Object.Instantiate(asyncOperationHandle.Result, transform.position, transform.rotation, transform.parent);
			Debug.Log("Swapped " + placeholder.name + " with " + gameObject.name);
			IPlaceholdableComponent[] componentsInChildren = placeholder.GetComponentsInChildren<IPlaceholdableComponent>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].WillReplace(placeholder.gameObject, gameObject, isSelfBeingReplaced: true);
			}
			componentsInChildren = gameObject.GetComponentsInChildren<IPlaceholdableComponent>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].WillReplace(placeholder.gameObject, gameObject, isSelfBeingReplaced: false);
			}
			Object.Destroy(placeholder.gameObject);
		}
	}

	public void ReplacePrefab(PlaceholderPrefab placeholder)
	{
		PerformSwap(placeholder);
	}
}
