using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.PersistAutoInstance | SingletonFlags.DestroyDuplicates)]
public class PrefsManager : MonoSingleton<PrefsManager>
{
	private enum PrefsCommitMode
	{
		Immediate,
		OnQuit,
		DirtySlowTick
	}

	private FileStream prefsStream;

	private FileStream localPrefsStream;

	private static PrefsCommitMode CommitMode = ((!Application.isEditor) ? PrefsCommitMode.DirtySlowTick : PrefsCommitMode.Immediate);

	private TimeSince timeSinceLastTick;

	private const float SlowTickCommitInterval = 3f;

	private const bool DebugLogging = false;

	private bool isDirty;

	private bool isLocalDirty;

	public static int monthsSinceLastPlayed = 0;

	public Dictionary<string, object> prefMap;

	public Dictionary<string, object> localPrefMap;

	private readonly Dictionary<string, Func<object, object>> propertyValidators = new Dictionary<string, Func<object, object>> { 
	{
		"difficulty",
		delegate(object value)
		{
			if (!(value is int num))
			{
				Debug.LogWarning("Difficulty value is not an int");
				return 2;
			}
			if (num < 0 || num > 3)
			{
				Debug.LogWarning("Difficulty validation error");
				return 3;
			}
			return null;
		}
	} };

	public readonly Dictionary<string, object> defaultValues = new Dictionary<string, object>
	{
		{ "difficulty", 2 },
		{ "scrollEnabled", true },
		{ "scrollWeapons", true },
		{ "scrollVariations", false },
		{ "scrollReversed", false },
		{ "mouseSensitivity", 50f },
		{ "discordIntegration", true },
		{ "levelLeaderboards", true },
		{ "subtitlesEnabled", false },
		{ "seasonalEvents", true },
		{ "majorAssist", false },
		{ "gameSpeed", 1f },
		{ "damageTaken", 1f },
		{ "infiniteStamina", false },
		{ "disableWhiplashHardDamage", false },
		{ "disableHardDamage", false },
		{ "disableWeaponFreshness", false },
		{ "autoAim", false },
		{ "autoAimAmount", 0.2f },
		{ "bossDifficultyOverride", 0 },
		{ "hideMajorAssistPopup", false },
		{ "frameRateLimit", 1 },
		{ "fullscreen", true },
		{ "fieldOfView", 105f },
		{ "vSync", true },
		{ "totalRumbleIntensity", 1f },
		{ "musicVolume", 0.6f },
		{ "allVolume", 1f },
		{ "screenShake", 1f },
		{ "cameraTilt", true },
		{ "dithering", 0.2f },
		{ "colorCompression", 2 },
		{ "vertexWarping", 0 },
		{ "textureWarping", 0f },
		{ "pixelization", 0 },
		{ "crossHair", 1 },
		{ "crossHairColor", 1 },
		{ "crossHairHud", 2 },
		{ "hudType", 1 },
		{ "hudBackgroundOpacity", 50f },
		{ "weaponHoldPosition", 0 },
		{ "pauseMenuConfirmationDialogs", true },
		{ "variationMemory", false },
		{ "bloodEnabled", true },
		{ "bloodStainChance", 0.5f },
		{ "maxGore", 3000f },
		{ "weaponIcons", true },
		{ "armIcons", true },
		{ "styleMeter", true },
		{ "styleInfo", true },
		{ "crossHairHudFade", true },
		{ "powerUpMeter", true },
		{ "railcannonMeter", true },
		{ "simpleNailPhysics", true },
		{ "selectedSaveSlot", 0 }
	};

	private static string prefsNote = "LocalPrefs.json contains preferences local to this machine. It does NOT get backed up in any way.\nPrefs.json contains preferences that are synced across all of your machines via Steam Cloud.\n\nPlaylist.json contains the IDs of all the songs in the user's Cybergrind Playlist.All prefs files must be valid json.\nIf you edit them, make sure you don't break the format.\n\nIf a pref key is missing from the prefs files, the game will use the default value and save ONLY if overridden.\nAdditionally, you can NOT move things between Prefs.json and LocalPrefs.json, the game will ignore them if misplaced.";

	private static string PrefsPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Preferences");

	public bool HasKey(string key)
	{
		EnsureInitialized();
		if (!prefMap.ContainsKey(key))
		{
			return localPrefMap.ContainsKey(key);
		}
		return true;
	}

	public void DeleteKey(string key)
	{
		EnsureInitialized();
		if (prefMap.ContainsKey(key))
		{
			prefMap.Remove(key);
			if (CommitMode == PrefsCommitMode.DirtySlowTick)
			{
				isDirty = true;
			}
			if (CommitMode == PrefsCommitMode.Immediate)
			{
				CommitPrefs(local: false);
			}
		}
		if (localPrefMap.ContainsKey(key))
		{
			localPrefMap.Remove(key);
			if (CommitMode == PrefsCommitMode.DirtySlowTick)
			{
				isLocalDirty = true;
			}
			if (CommitMode == PrefsCommitMode.Immediate)
			{
				CommitPrefs(local: true);
			}
		}
	}

	public bool GetBoolLocal(string key, bool fallback = false)
	{
		EnsureInitialized();
		object obj;
		if (localPrefMap.ContainsKey(key) && (obj = localPrefMap[key]) is bool)
		{
			return (bool)obj;
		}
		if (defaultValues.ContainsKey(key) && (obj = defaultValues[key]) is bool)
		{
			return (bool)obj;
		}
		return fallback;
	}

	public bool GetBool(string key, bool fallback = false)
	{
		EnsureInitialized();
		object obj;
		if (prefMap.ContainsKey(key) && (obj = prefMap[key]) is bool)
		{
			return (bool)obj;
		}
		if (defaultValues.ContainsKey(key) && (obj = defaultValues[key]) is bool)
		{
			return (bool)obj;
		}
		return fallback;
	}

	public void SetBoolLocal(string key, bool content)
	{
		EnsureInitialized();
		if (localPrefMap.ContainsKey(key))
		{
			localPrefMap[key] = content;
		}
		else
		{
			localPrefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: true);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isLocalDirty = true;
		}
	}

	public void SetBool(string key, bool content)
	{
		EnsureInitialized();
		if (prefMap.ContainsKey(key))
		{
			prefMap[key] = content;
		}
		else
		{
			prefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: false);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isDirty = true;
		}
	}

	public int GetIntLocal(string key, int fallback = 0)
	{
		EnsureInitialized();
		if (localPrefMap.ContainsKey(key))
		{
			if (localPrefMap[key] is int num)
			{
				return (int)EnsureValid(key, num);
			}
			if (localPrefMap[key] is long num2)
			{
				return (int)EnsureValid(key, (int)num2);
			}
			if (localPrefMap[key] is float num3)
			{
				return (int)EnsureValid(key, (int)num3);
			}
			if (localPrefMap[key] is double num4)
			{
				return (int)EnsureValid(key, (int)num4);
			}
		}
		object obj;
		if (defaultValues.ContainsKey(key) && (obj = defaultValues[key]) is int)
		{
			return (int)obj;
		}
		return fallback;
	}

	public int GetInt(string key, int fallback = 0)
	{
		EnsureInitialized();
		if (prefMap.ContainsKey(key))
		{
			if (prefMap[key] is int num)
			{
				return (int)EnsureValid(key, num);
			}
			if (prefMap[key] is long num2)
			{
				return (int)EnsureValid(key, (int)num2);
			}
			if (prefMap[key] is float num3)
			{
				return (int)EnsureValid(key, (int)num3);
			}
			if (prefMap[key] is double num4)
			{
				return (int)EnsureValid(key, (int)num4);
			}
		}
		object obj;
		if (defaultValues.ContainsKey(key) && (obj = defaultValues[key]) is int)
		{
			return (int)obj;
		}
		return fallback;
	}

	public void SetIntLocal(string key, int content)
	{
		EnsureInitialized();
		content = (int)EnsureValid(key, content);
		if (localPrefMap.ContainsKey(key))
		{
			localPrefMap[key] = content;
		}
		else
		{
			localPrefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: true);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isLocalDirty = true;
		}
	}

	public void SetInt(string key, int content)
	{
		EnsureInitialized();
		content = (int)EnsureValid(key, content);
		if (prefMap.ContainsKey(key))
		{
			prefMap[key] = content;
		}
		else
		{
			prefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: false);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isDirty = true;
		}
	}

	public float GetFloatLocal(string key, float fallback = 0f)
	{
		EnsureInitialized();
		if (localPrefMap.ContainsKey(key))
		{
			if (localPrefMap[key] is float num)
			{
				return (float)EnsureValid(key, num);
			}
			if (localPrefMap[key] is int num2)
			{
				return (int)EnsureValid(key, num2);
			}
			if (localPrefMap[key] is long num3)
			{
				return (long)EnsureValid(key, num3);
			}
			if (localPrefMap[key] is double num4)
			{
				return (float)EnsureValid(key, (float)num4);
			}
		}
		object obj;
		if (defaultValues.ContainsKey(key) && (obj = defaultValues[key]) is float)
		{
			return (float)obj;
		}
		return fallback;
	}

	public float GetFloat(string key, float fallback = 0f)
	{
		EnsureInitialized();
		if (prefMap.ContainsKey(key))
		{
			if (prefMap[key] is float num)
			{
				return (float)EnsureValid(key, num);
			}
			if (prefMap[key] is int num2)
			{
				return (int)EnsureValid(key, num2);
			}
			if (prefMap[key] is long num3)
			{
				return (long)EnsureValid(key, num3);
			}
			if (prefMap[key] is double num4)
			{
				return (float)EnsureValid(key, (float)num4);
			}
		}
		object obj;
		if (defaultValues.ContainsKey(key) && (obj = defaultValues[key]) is float)
		{
			return (float)obj;
		}
		return fallback;
	}

	public void SetFloatLocal(string key, float content)
	{
		EnsureInitialized();
		content = (float)EnsureValid(key, content);
		if (localPrefMap.ContainsKey(key))
		{
			localPrefMap[key] = content;
		}
		else
		{
			localPrefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: true);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isLocalDirty = true;
		}
	}

	public void SetFloat(string key, float content)
	{
		EnsureInitialized();
		content = (float)EnsureValid(key, content);
		if (prefMap.ContainsKey(key))
		{
			prefMap[key] = content;
		}
		else
		{
			prefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: false);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isDirty = true;
		}
	}

	public string GetStringLocal(string key, string fallback = null)
	{
		EnsureInitialized();
		if (localPrefMap.ContainsKey(key) && localPrefMap[key] is string value)
		{
			return EnsureValid(key, value) as string;
		}
		if (defaultValues.ContainsKey(key) && defaultValues[key] is string result)
		{
			return result;
		}
		return fallback;
	}

	public string GetString(string key, string fallback = null)
	{
		EnsureInitialized();
		if (prefMap.ContainsKey(key) && prefMap[key] is string value)
		{
			return EnsureValid(key, value) as string;
		}
		if (defaultValues.ContainsKey(key) && defaultValues[key] is string result)
		{
			return result;
		}
		return fallback;
	}

	public void SetStringLocal(string key, string content)
	{
		EnsureInitialized();
		content = EnsureValid(key, content) as string;
		if (localPrefMap.ContainsKey(key))
		{
			localPrefMap[key] = content;
		}
		else
		{
			localPrefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: true);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isLocalDirty = true;
		}
	}

	public void SetString(string key, string content)
	{
		EnsureInitialized();
		content = EnsureValid(key, content) as string;
		if (prefMap.ContainsKey(key))
		{
			prefMap[key] = content;
		}
		else
		{
			prefMap.Add(key, content);
		}
		if (CommitMode == PrefsCommitMode.Immediate)
		{
			CommitPrefs(local: false);
		}
		if (CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			isDirty = true;
		}
	}

	private void CommitPrefs(bool local)
	{
		if (local)
		{
			string value = JsonConvert.SerializeObject(localPrefMap, Formatting.Indented);
			localPrefsStream.SetLength(0L);
			StreamWriter streamWriter = new StreamWriter(localPrefsStream);
			streamWriter.Write(value);
			streamWriter.Flush();
		}
		else
		{
			string value2 = JsonConvert.SerializeObject(prefMap, Formatting.Indented);
			prefsStream.SetLength(0L);
			StreamWriter streamWriter2 = new StreamWriter(prefsStream);
			streamWriter2.Write(value2);
			streamWriter2.Flush();
		}
	}

	private void UpdateTimestamp()
	{
		EnsureInitialized();
		DateTime now = DateTime.Now;
		if (!prefMap.ContainsKey("lastTimePlayed.year"))
		{
			prefMap.Add("lastTimePlayed.year", now.Year);
			isDirty = true;
		}
		if (!prefMap.ContainsKey("lastTimePlayed.month"))
		{
			prefMap.Add("lastTimePlayed.month", now.Month);
			isDirty = true;
		}
		if (prefMap["lastTimePlayed.year"] is int num)
		{
			if (num != now.Year)
			{
				prefMap["lastTimePlayed.year"] = now.Year;
				prefMap["lastTimePlayed.month"] = now.Month;
				isDirty = true;
			}
			object obj;
			if ((obj = prefMap["lastTimePlayed.month"]) is int)
			{
				int num2 = (int)obj;
				if (num2 != now.Month)
				{
					prefMap["lastTimePlayed.month"] = now.Month;
					isDirty = true;
				}
			}
		}
		else
		{
			prefMap["lastTimePlayed.year"] = now.Year;
			prefMap["lastTimePlayed.month"] = now.Month;
			isDirty = true;
		}
	}

	private void EnsureInitialized()
	{
		if (prefMap == null || localPrefMap == null)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		if (!Directory.Exists(PrefsPath))
		{
			Directory.CreateDirectory(PrefsPath);
		}
		timeSinceLastTick = 0f;
		if (prefsStream == null)
		{
			prefsStream = new FileStream(Path.Combine(PrefsPath, "Prefs.json"), FileMode.OpenOrCreate);
		}
		if (localPrefsStream == null)
		{
			localPrefsStream = new FileStream(Path.Combine(PrefsPath, "LocalPrefs.json"), FileMode.OpenOrCreate);
		}
		prefMap = LoadPrefs(prefsStream);
		localPrefMap = LoadPrefs(localPrefsStream);
		if (!File.Exists(Path.Combine(PrefsPath, "NOTE.txt")))
		{
			File.WriteAllText(Path.Combine(PrefsPath, "NOTE.txt"), prefsNote);
			Debug.LogWarning("NOTE.txt created in prefs folder. Please read it.");
		}
		Debug.Log("PrefsManager initialized");
	}

	private object EnsureValid(string key, object value)
	{
		if (!propertyValidators.ContainsKey(key))
		{
			return value;
		}
		return propertyValidators[key](value) ?? value;
	}

	private Dictionary<string, object> LoadPrefs(FileStream stream)
	{
		return JsonConvert.DeserializeObject<Dictionary<string, object>>(new StreamReader(stream).ReadToEnd()) ?? new Dictionary<string, object>();
	}

	protected override void Awake()
	{
		base.Awake();
		if (!(MonoSingleton<PrefsManager>.Instance != this))
		{
			Initialize();
			int @int = GetInt("lastTimePlayed.year", -1);
			int int2 = GetInt("lastTimePlayed.month", -1);
			if (@int == -1 || int2 == -1)
			{
				monthsSinceLastPlayed = 0;
				return;
			}
			DateTime now = DateTime.Now;
			monthsSinceLastPlayed = (now.Year - @int) * 12 + now.Month - int2;
		}
	}

	private void Start()
	{
		UpdateTimestamp();
	}

	private void FixedUpdate()
	{
		if ((isDirty || isLocalDirty) && CommitMode == PrefsCommitMode.DirtySlowTick && (float)timeSinceLastTick >= 3f)
		{
			timeSinceLastTick = 0f;
			if (isLocalDirty)
			{
				CommitPrefs(local: true);
			}
			if (isDirty)
			{
				CommitPrefs(local: false);
			}
			isLocalDirty = false;
			isDirty = false;
		}
	}

	private void OnApplicationQuit()
	{
		UpdateTimestamp();
		if (CommitMode == PrefsCommitMode.OnQuit || CommitMode == PrefsCommitMode.DirtySlowTick)
		{
			CommitPrefs(local: false);
			CommitPrefs(local: true);
		}
		prefsStream?.Close();
		localPrefsStream?.Close();
	}
}
