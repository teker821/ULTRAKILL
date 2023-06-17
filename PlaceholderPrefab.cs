using UnityEngine;

[DefaultExecutionOrder(-150)]
public class PlaceholderPrefab : MonoBehaviour
{
	public string uniqueId;

	private void Awake()
	{
		PrefabReplacer.Instance.ReplacePrefab(this);
	}
}
