using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Logic;

[ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
public class MapVarManager : MonoSingleton<MapVarManager>
{
	private VarStore currentStore = new VarStore();

	private VarStore stashedStore;

	private readonly Dictionary<string, List<UnityAction<int>>> intSubscribers = new Dictionary<string, List<UnityAction<int>>>();

	private readonly Dictionary<string, List<UnityAction<bool>>> boolSubscribers = new Dictionary<string, List<UnityAction<bool>>>();

	private readonly Dictionary<string, List<UnityAction<float>>> floatSubscribers = new Dictionary<string, List<UnityAction<float>>>();

	private readonly Dictionary<string, List<UnityAction<string>>> stringSubscribers = new Dictionary<string, List<UnityAction<string>>>();

	public static bool LoggingEnabled;

	public bool HasStashedStore => stashedStore != null;

	public void ResetStores()
	{
		currentStore.Clear();
		stashedStore = null;
		intSubscribers.Clear();
		boolSubscribers.Clear();
		floatSubscribers.Clear();
		stringSubscribers.Clear();
		LoggingEnabled = false;
	}

	public void StashStore()
	{
		if (currentStore.intStore.Count == 0 && currentStore.boolStore.Count == 0 && currentStore.floatStore.Count == 0 && currentStore.stringStore.Count == 0)
		{
			stashedStore = null;
			return;
		}
		stashedStore = currentStore.DuplicateStore();
		if (LoggingEnabled)
		{
			Debug.Log("Stashed MapVar stores");
		}
	}

	public void RestoreStashedStore()
	{
		if (stashedStore == null)
		{
			if (LoggingEnabled)
			{
				Debug.LogWarning("No stashed store to restore");
			}
			return;
		}
		currentStore = stashedStore.DuplicateStore();
		if (LoggingEnabled)
		{
			Debug.Log("Restored MapVar stores");
		}
	}

	public void RegisterIntWatcher(string key, UnityAction<int> callback)
	{
		if (LoggingEnabled)
		{
			Debug.Log("Registering int watcher for " + key);
		}
		if (!intSubscribers.ContainsKey(key))
		{
			intSubscribers.Add(key, new List<UnityAction<int>>());
		}
		intSubscribers[key].Add(callback);
	}

	public void RegisterBoolWatcher(string key, UnityAction<bool> callback)
	{
		if (LoggingEnabled)
		{
			Debug.Log("Registering bool watcher for " + key);
		}
		if (!boolSubscribers.ContainsKey(key))
		{
			boolSubscribers.Add(key, new List<UnityAction<bool>>());
		}
		boolSubscribers[key].Add(callback);
	}

	public void RegisterFloatWatcher(string key, UnityAction<float> callback)
	{
		if (LoggingEnabled)
		{
			Debug.Log("Registering float watcher for " + key);
		}
		if (!floatSubscribers.ContainsKey(key))
		{
			floatSubscribers.Add(key, new List<UnityAction<float>>());
		}
		floatSubscribers[key].Add(callback);
	}

	public void RegisterStringWatcher(string key, UnityAction<string> callback)
	{
		if (LoggingEnabled)
		{
			Debug.Log("Registering string watcher for " + key);
		}
		if (!stringSubscribers.ContainsKey(key))
		{
			stringSubscribers.Add(key, new List<UnityAction<string>>());
		}
		stringSubscribers[key].Add(callback);
	}

	public void SetInt(string key, int value)
	{
		if (currentStore.intStore.ContainsKey(key))
		{
			currentStore.intStore[key] = value;
		}
		else
		{
			currentStore.intStore.Add(key, value);
		}
		if (!intSubscribers.ContainsKey(key))
		{
			return;
		}
		if (LoggingEnabled)
		{
			Debug.Log($"Notifying {intSubscribers[key].Count} int watchers for {key}");
		}
		foreach (UnityAction<int> item in intSubscribers[key])
		{
			item?.Invoke(value);
		}
	}

	public void AddInt(string key, int value)
	{
		int num = GetInt(key) ?? 0;
		SetInt(key, num + value);
	}

	public void SetBool(string key, bool value)
	{
		if (currentStore.boolStore.ContainsKey(key))
		{
			currentStore.boolStore[key] = value;
		}
		else
		{
			currentStore.boolStore.Add(key, value);
		}
		if (!boolSubscribers.ContainsKey(key))
		{
			return;
		}
		if (LoggingEnabled)
		{
			Debug.Log($"Notifying {boolSubscribers[key].Count} bool watchers for {key}");
		}
		foreach (UnityAction<bool> item in boolSubscribers[key])
		{
			item?.Invoke(value);
		}
	}

	public void SetFloat(string key, float value)
	{
		if (currentStore.floatStore.ContainsKey(key))
		{
			currentStore.floatStore[key] = value;
		}
		else
		{
			currentStore.floatStore.Add(key, value);
		}
		if (!floatSubscribers.ContainsKey(key))
		{
			return;
		}
		if (LoggingEnabled)
		{
			Debug.Log($"Notifying {floatSubscribers[key].Count} float watchers for {key}");
		}
		foreach (UnityAction<float> item in floatSubscribers[key])
		{
			item?.Invoke(value);
		}
	}

	public void SetString(string key, string value)
	{
		if (currentStore.stringStore.ContainsKey(key))
		{
			currentStore.stringStore[key] = value;
		}
		else
		{
			currentStore.stringStore.Add(key, value);
		}
		if (!stringSubscribers.ContainsKey(key))
		{
			return;
		}
		if (LoggingEnabled)
		{
			Debug.Log($"Notifying {stringSubscribers[key].Count} string watchers for {key}");
		}
		foreach (UnityAction<string> item in stringSubscribers[key])
		{
			item?.Invoke(value);
		}
	}

	public int? GetInt(string key)
	{
		if (currentStore.intStore.ContainsKey(key))
		{
			return currentStore.intStore[key];
		}
		return null;
	}

	public bool? GetBool(string key)
	{
		if (currentStore.boolStore.ContainsKey(key))
		{
			return currentStore.boolStore[key];
		}
		return null;
	}

	public float? GetFloat(string key)
	{
		if (currentStore.floatStore.ContainsKey(key))
		{
			return currentStore.floatStore[key];
		}
		return null;
	}

	public string GetString(string key)
	{
		if (currentStore.stringStore.ContainsKey(key))
		{
			return currentStore.stringStore[key];
		}
		return null;
	}

	public List<VariableSnapshot> GetAllVariables()
	{
		List<VariableSnapshot> list = new List<VariableSnapshot>();
		foreach (KeyValuePair<string, int> item in currentStore.intStore)
		{
			list.Add(new VariableSnapshot
			{
				type = typeof(int),
				name = item.Key,
				value = item.Value
			});
		}
		foreach (KeyValuePair<string, bool> item2 in currentStore.boolStore)
		{
			list.Add(new VariableSnapshot
			{
				type = typeof(bool),
				name = item2.Key,
				value = item2.Value
			});
		}
		foreach (KeyValuePair<string, float> item3 in currentStore.floatStore)
		{
			list.Add(new VariableSnapshot
			{
				type = typeof(float),
				name = item3.Key,
				value = item3.Value
			});
		}
		foreach (KeyValuePair<string, string> item4 in currentStore.stringStore)
		{
			list.Add(new VariableSnapshot
			{
				type = typeof(string),
				name = item4.Key,
				value = item4.Value
			});
		}
		return list;
	}
}
