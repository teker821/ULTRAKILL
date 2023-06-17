using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class GameProgressSaver
{
	public enum WeaponCustomizationType
	{
		Revolver,
		Shotgun,
		Nailgun,
		Railcannon,
		RocketLauncher
	}

	public static int currentSlot = 0;

	private static int lastTotalSecrets = -1;

	private static bool initialized;

	private static readonly string[] SlotIgnoreFiles = new string[1] { "prefs" };

	public static string BaseSavePath => Path.Combine((SystemInfo.deviceType == DeviceType.Desktop) ? Directory.GetParent(Application.dataPath).FullName : Application.persistentDataPath, "Saves");

	private static string savePath => Path.Combine(BaseSavePath, $"Slot{currentSlot + 1}");

	private static string currentDifficultyPath => DifficultySavePath(MonoSingleton<PrefsManager>.Instance.GetInt("difficulty"));

	private static string customSavesDir => Path.Combine(savePath, "Custom");

	private static string customCampaignsDir => Path.Combine(customSavesDir, "Campaigns");

	private static string generalProgressPath => Path.Combine(savePath, "generalprogress.bepis");

	private static string cyberGrindHighScorePath => Path.Combine(savePath, "cybergrindhighscore.bepis");

	private static string customLevelProgressPath => Path.Combine(customSavesDir, GameStateManager.Instance.currentCustomGame.uniqueIdentifier + ".bepis");

	public static string customMapsPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Mods", "Maps");

	private static string resolveCurrentLevelPath
	{
		get
		{
			if (!SceneHelper.IsPlayingCustom)
			{
				return LevelProgressPath(MonoSingleton<StatsManager>.Instance.levelNumber);
			}
			return customLevelProgressPath;
		}
	}

	private static string DifficultySavePath(int diff)
	{
		if (!SceneHelper.IsPlayingCustom)
		{
			return Path.Combine(savePath, $"difficulty{diff}progress.bepis");
		}
		if (!string.IsNullOrEmpty(GameStateManager.Instance.currentCustomGame.campaignId))
		{
			return Path.Combine(customCampaignsDir, GameStateManager.Instance.currentCustomGame.campaignId, $"difficulty{diff}progress.bepis");
		}
		return Path.Combine(customCampaignsDir, $"difficulty{diff}progress.bepis");
	}

	private static string LevelProgressPath(int lvl)
	{
		return Path.Combine(savePath, $"lvl{lvl}progress.bepis");
	}

	public static void SetSlot(int slot)
	{
		currentSlot = slot;
		lastTotalSecrets = -1;
		MonoSingleton<PrefsManager>.Instance.SetInt("selectedSaveSlot", slot);
		MonoSingleton<BestiaryData>.Instance.CheckSave();
	}

	public static void CreateSaveDirs(bool forceCustom = false)
	{
		if (!Directory.Exists(savePath))
		{
			Directory.CreateDirectory(savePath);
		}
		if (SceneHelper.IsPlayingCustom || forceCustom)
		{
			if (!Directory.Exists(customSavesDir))
			{
				Directory.CreateDirectory(customSavesDir);
			}
			if (!Directory.Exists(customCampaignsDir))
			{
				Directory.CreateDirectory(customCampaignsDir);
			}
			if (!Directory.Exists(customMapsPath))
			{
				Directory.CreateDirectory(customMapsPath);
			}
		}
	}

	public static void WipeSlot(int slot)
	{
		int num = currentSlot;
		currentSlot = slot;
		try
		{
			if (Directory.Exists(savePath))
			{
				string[] files = Directory.GetFiles(savePath, "*.bepis", SearchOption.TopDirectoryOnly);
				for (int i = 0; i < files.Length; i++)
				{
					File.Delete(files[i]);
				}
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		currentSlot = num;
	}

	private static SaveSlotMenu.SlotData GetDirectorySlotData(string path)
	{
		Debug.Log("Generating SlotData for " + path);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 6; i++)
		{
			if (ReadFile(Path.Combine(path, $"difficulty{i}progress.bepis")) is GameProgressData gameProgressData && (gameProgressData.levelNum > num || (gameProgressData.levelNum == num && gameProgressData.difficulty > num2)))
			{
				num = gameProgressData.levelNum;
				num2 = gameProgressData.difficulty;
			}
		}
		return new SaveSlotMenu.SlotData
		{
			exists = true,
			highestDifficulty = num2,
			highestLvlNumber = num
		};
	}

	public static SaveSlotMenu.SlotData[] GetSlots()
	{
		int num = currentSlot;
		List<SaveSlotMenu.SlotData> list = new List<SaveSlotMenu.SlotData>();
		for (int i = 0; i < 5; i++)
		{
			currentSlot = i;
			if (!Directory.Exists(savePath))
			{
				list.Add(new SaveSlotMenu.SlotData
				{
					exists = false
				});
			}
			else if (!(ReadFile(generalProgressPath) is GameProgressMoneyAndGear))
			{
				list.Add(new SaveSlotMenu.SlotData
				{
					exists = false
				});
			}
			else
			{
				SaveSlotMenu.SlotData directorySlotData = GetDirectorySlotData(savePath);
				list.Add(directorySlotData);
			}
		}
		currentSlot = num;
		return list.ToArray();
	}

	private static void PrepareFs()
	{
		CreateSaveDirs();
		if (initialized)
		{
			return;
		}
		initialized = true;
		if ((from a in Directory.GetFiles(BaseSavePath, "*.bepis", SearchOption.TopDirectoryOnly)
			where !SlotIgnoreFiles.Contains(Path.GetFileNameWithoutExtension(a))
			select a).ToArray().Length != 0)
		{
			Debug.Log("Old saves found");
			int num = currentSlot;
			currentSlot = 0;
			try
			{
				CreateSaveDirs();
				if (Directory.GetFiles(savePath, "*.bepis").Length != 0)
				{
					Debug.Log("Slot 1 is populated while old saves also exist. Showing consent screen.");
					SaveSlotMenu.SlotData directorySlotData = GetDirectorySlotData(BaseSavePath);
					SaveSlotMenu.SlotData directorySlotData2 = GetDirectorySlotData(savePath);
					SaveLoadFailMessage.DisplayMergeConsent(directorySlotData, directorySlotData2);
				}
				else
				{
					MergeRootWithSlotOne(keepRoot: true);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			currentSlot = num;
		}
		if (Directory.Exists(Path.Combine(BaseSavePath, "Custom")))
		{
			string[] files = Directory.GetFiles(Path.Combine(BaseSavePath, "Custom"), "*.bepis", SearchOption.TopDirectoryOnly);
			if (files.Length != 0)
			{
				int num2 = currentSlot;
				currentSlot = 0;
				try
				{
					CreateSaveDirs(forceCustom: true);
					string[] array = files;
					foreach (string text in array)
					{
						SafeMove(text, Path.Combine(customSavesDir, Path.GetFileName(text)));
					}
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
				currentSlot = num2;
			}
		}
		currentSlot = MonoSingleton<PrefsManager>.Instance.GetInt("selectedSaveSlot");
	}

	public static void MergeRootWithSlotOne(bool keepRoot)
	{
		string[] array = (from a in Directory.GetFiles(BaseSavePath, "*.bepis", SearchOption.TopDirectoryOnly)
			where !SlotIgnoreFiles.Contains(Path.GetFileNameWithoutExtension(a))
			select a).ToArray();
		foreach (string text in array)
		{
			Debug.Log(text);
			if (!keepRoot)
			{
				File.Delete(text);
			}
			else
			{
				SafeMove(text, Path.Combine(savePath, Path.GetFileName(text)));
			}
		}
	}

	private static void SafeMove(string source, string target)
	{
		if (File.Exists(target))
		{
			File.Delete(target);
		}
		File.Move(source, target);
	}

	private static object ReadFile(string path)
	{
		PrepareFs();
		if (!File.Exists(path))
		{
			return null;
		}
		using FileStream fileStream = new FileStream(path, FileMode.Open);
		if (fileStream.Length == 0L)
		{
			throw new Exception("Stream Length 0");
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		try
		{
			object result = binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
			return result;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private static void WriteFile(string path, object data)
	{
		PrepareFs();
		Debug.Log("[FS] Writing To " + path);
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		using FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate);
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		try
		{
			binaryFormatter.Serialize(fileStream, data);
			fileStream.Close();
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	private static RankData GetRankData(bool returnNull)
	{
		string path;
		return GetRankData(out path, -1, returnNull);
	}

	private static RankData GetRankData(int lvl = -1)
	{
		string path;
		return GetRankData(out path, lvl);
	}

	private static RankData GetRankData(out string path, int lvl = -1, bool returnNull = false)
	{
		path = ((lvl < 0) ? resolveCurrentLevelPath : LevelProgressPath(lvl));
		object obj = ReadFile(path);
		Debug.Log(obj);
		RankData result;
		if ((result = obj as RankData) == null)
		{
			result = (returnNull ? null : new RankData(MonoSingleton<StatsManager>.Instance));
		}
		return result;
	}

	private static GameProgressData GetGameProgress(int difficulty = -1)
	{
		string path;
		return GetGameProgress(out path, difficulty);
	}

	private static GameProgressData GetGameProgress(out string path, int difficulty = -1)
	{
		path = ((difficulty < 0) ? currentDifficultyPath : DifficultySavePath(difficulty));
		GameProgressData gameProgressData;
		if ((gameProgressData = ReadFile(path) as GameProgressData) == null)
		{
			gameProgressData = new GameProgressData();
		}
		if (gameProgressData.primeLevels == null || gameProgressData.primeLevels.Length == 0)
		{
			gameProgressData.primeLevels = new int[3];
		}
		return gameProgressData;
	}

	public static void ChallengeComplete()
	{
		RankData rankData = GetRankData();
		if (!rankData.challenge && (rankData.levelNumber == MonoSingleton<StatsManager>.Instance.levelNumber || SceneHelper.IsPlayingCustom))
		{
			rankData.challenge = true;
			WriteFile(resolveCurrentLevelPath, rankData);
		}
	}

	public static void SaveProgress(int levelNum)
	{
		Debug.Log($"[FS] Saving Progress for Level {levelNum}");
		string path;
		GameProgressData gameProgress = GetGameProgress(out path);
		if (gameProgress.levelNum < levelNum || gameProgress.difficulty != MonoSingleton<PrefsManager>.Instance.GetInt("difficulty"))
		{
			gameProgress.levelNum = levelNum;
			WriteFile(path, gameProgress);
		}
	}

	public static void SaveRank()
	{
		WriteFile(resolveCurrentLevelPath, new RankData(MonoSingleton<StatsManager>.Instance));
	}

	public static RankData GetRank(bool returnNull)
	{
		return GetRankData(returnNull);
	}

	public static void SecretFound(int secretNum)
	{
		lastTotalSecrets = -1;
		string path;
		RankData rankData = GetRankData(out path);
		if ((rankData.levelNumber == MonoSingleton<StatsManager>.Instance.levelNumber || SceneHelper.IsPlayingCustom) && !rankData.secretsFound[secretNum])
		{
			rankData.secretsFound[secretNum] = true;
			WriteFile(path, rankData);
		}
	}

	public static int GetProgress(int difficulty)
	{
		int num = 1;
		for (int i = difficulty; i <= 5; i++)
		{
			GameProgressData gameProgress = GetGameProgress(i);
			if (gameProgress != null && gameProgress.difficulty == i && gameProgress.levelNum > num)
			{
				num = gameProgress.levelNum;
			}
		}
		return num;
	}

	public static RankData GetRank(int levelNumber, bool returnNull = false)
	{
		string path;
		return GetRankData(out path, levelNumber, returnNull);
	}

	public static void SetPrime(int level, int state)
	{
		if (SceneHelper.IsPlayingCustom)
		{
			return;
		}
		level--;
		string path;
		GameProgressData gameProgress = GetGameProgress(out path);
		if (level < gameProgress.primeLevels.Length)
		{
			if (state <= gameProgress.primeLevels[level])
			{
				return;
			}
			gameProgress.primeLevels[level] = state;
		}
		else
		{
			int[] array = new int[3];
			for (int i = 0; i < gameProgress.primeLevels.Length; i++)
			{
				array[i] = gameProgress.primeLevels[i];
			}
			for (int j = gameProgress.primeLevels.Length; j < array.Length; j++)
			{
				if (j == level)
				{
					array[j] = state;
				}
				else
				{
					array[j] = 0;
				}
			}
			gameProgress.primeLevels = array;
		}
		WriteFile(path, gameProgress);
	}

	public static int GetPrime(int difficulty, int level)
	{
		if (SceneHelper.IsPlayingCustom)
		{
			return 0;
		}
		level--;
		int num = 0;
		for (int i = difficulty; i <= 5; i++)
		{
			GameProgressData gameProgress = GetGameProgress(i);
			if (gameProgress != null && gameProgress.difficulty == i && gameProgress.primeLevels != null && gameProgress.primeLevels.Length > level && gameProgress.primeLevels[level] > num)
			{
				Debug.Log("Highest: . Data: " + gameProgress.primeLevels[level]);
				if (gameProgress.primeLevels[level] >= 2)
				{
					return 2;
				}
				num = gameProgress.primeLevels[level];
			}
		}
		return num;
	}

	public static GameProgressMoneyAndGear GetGeneralProgress()
	{
		PrepareFs();
		GameProgressMoneyAndGear gameProgressMoneyAndGear;
		if ((gameProgressMoneyAndGear = ReadFile(generalProgressPath) as GameProgressMoneyAndGear) == null)
		{
			gameProgressMoneyAndGear = new GameProgressMoneyAndGear();
		}
		if (gameProgressMoneyAndGear.secretMissions == null || gameProgressMoneyAndGear.secretMissions.Length == 0)
		{
			gameProgressMoneyAndGear.secretMissions = new int[10];
		}
		if (gameProgressMoneyAndGear.limboSwitches == null || gameProgressMoneyAndGear.limboSwitches.Length == 0)
		{
			gameProgressMoneyAndGear.limboSwitches = new bool[4];
		}
		if (gameProgressMoneyAndGear.newEnemiesFound == null)
		{
			gameProgressMoneyAndGear.newEnemiesFound = new int[Enum.GetValues(typeof(EnemyType)).Length];
		}
		if (gameProgressMoneyAndGear.unlockablesFound == null)
		{
			gameProgressMoneyAndGear.unlockablesFound = new bool[Enum.GetValues(typeof(UnlockableType)).Length];
		}
		return gameProgressMoneyAndGear;
	}

	public static void AddGear(string gear)
	{
		if (!SceneHelper.IsPlayingCustom)
		{
			GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
			FieldInfo field = typeof(GameProgressMoneyAndGear).GetField(gear, BindingFlags.Instance | BindingFlags.Public);
			if (!(field == null))
			{
				field.SetValue(generalProgress, 1);
				WriteFile(generalProgressPath, generalProgress);
			}
		}
	}

	public static int CheckGear(string gear)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		FieldInfo field = typeof(GameProgressMoneyAndGear).GetField(gear, BindingFlags.Instance | BindingFlags.Public);
		if (field == null)
		{
			return 0;
		}
		object value = field.GetValue(generalProgress);
		if (value == null)
		{
			return 0;
		}
		return (int)value;
	}

	public static void AddMoney(int money)
	{
		if (!SceneHelper.IsPlayingCustom)
		{
			GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
			if (generalProgress.money + money >= 0)
			{
				generalProgress.money += money;
			}
			else
			{
				generalProgress.money = 0;
			}
			WriteFile(generalProgressPath, generalProgress);
		}
	}

	public static void UnlockWeaponCustomization(WeaponCustomizationType weapon)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		switch (weapon)
		{
		case WeaponCustomizationType.Revolver:
			generalProgress.revCustomizationUnlocked = true;
			break;
		case WeaponCustomizationType.Shotgun:
			generalProgress.shoCustomizationUnlocked = true;
			break;
		case WeaponCustomizationType.Nailgun:
			generalProgress.naiCustomizationUnlocked = true;
			break;
		case WeaponCustomizationType.Railcannon:
			generalProgress.raiCustomizationUnlocked = true;
			break;
		case WeaponCustomizationType.RocketLauncher:
			generalProgress.rockCustomizationUnlocked = true;
			break;
		}
		WriteFile(generalProgressPath, generalProgress);
	}

	public static bool HasWeaponCustomization(WeaponCustomizationType weapon)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		return weapon switch
		{
			WeaponCustomizationType.Revolver => generalProgress.revCustomizationUnlocked, 
			WeaponCustomizationType.Shotgun => generalProgress.shoCustomizationUnlocked, 
			WeaponCustomizationType.Nailgun => generalProgress.naiCustomizationUnlocked, 
			WeaponCustomizationType.Railcannon => generalProgress.raiCustomizationUnlocked, 
			WeaponCustomizationType.RocketLauncher => generalProgress.rockCustomizationUnlocked, 
			_ => false, 
		};
	}

	public static int GetMoney()
	{
		return GetGeneralProgress().money;
	}

	public static void SetTutorial(bool beat)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		generalProgress.tutorialBeat = beat;
		WriteFile(generalProgressPath, generalProgress);
	}

	public static bool GetTutorial()
	{
		if (SceneHelper.IsPlayingCustom)
		{
			return true;
		}
		return GetGeneralProgress().tutorialBeat;
	}

	public static void SetIntro(bool seen)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		generalProgress.introSeen = seen;
		WriteFile(generalProgressPath, generalProgress);
	}

	public static bool GetIntro()
	{
		if (SceneHelper.IsPlayingCustom)
		{
			return true;
		}
		return GetGeneralProgress().introSeen;
	}

	public static void SetClashModeUnlocked(bool unlocked)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		generalProgress.clashModeUnlocked = unlocked;
		WriteFile(generalProgressPath, generalProgress);
	}

	public static bool GetClashModeUnlocked()
	{
		if (SceneHelper.IsPlayingCustom)
		{
			return true;
		}
		return GetGeneralProgress().clashModeUnlocked;
	}

	public static void SetUnlockable(UnlockableType unlockable, bool state)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		bool[] array = new bool[Enum.GetValues(typeof(UnlockableType)).Length];
		for (int i = 0; i < generalProgress.unlockablesFound.Length; i++)
		{
			array[i] = generalProgress.unlockablesFound[i];
		}
		for (int j = generalProgress.newEnemiesFound.Length; j < array.Length; j++)
		{
			array[j] = false;
		}
		array[(int)unlockable] = state;
		generalProgress.unlockablesFound = array;
		WriteFile(generalProgressPath, generalProgress);
	}

	public static UnlockableType[] GetUnlockables()
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		List<UnlockableType> list = new List<UnlockableType>();
		for (int i = 0; i < generalProgress.unlockablesFound.Length; i++)
		{
			if (generalProgress.unlockablesFound[i])
			{
				list.Add((UnlockableType)i);
			}
		}
		return list.ToArray();
	}

	public static void SetBestiary(EnemyType enemy, int state)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		int[] array = new int[Enum.GetValues(typeof(EnemyType)).Length];
		for (int i = 0; i < generalProgress.newEnemiesFound.Length; i++)
		{
			array[i] = generalProgress.newEnemiesFound[i];
		}
		for (int j = generalProgress.newEnemiesFound.Length; j < array.Length; j++)
		{
			array[j] = 0;
		}
		array[(int)enemy] = state;
		generalProgress.newEnemiesFound = array;
		WriteFile(generalProgressPath, generalProgress);
	}

	public static int[] GetBestiary()
	{
		return GetGeneralProgress().newEnemiesFound;
	}

	public static void SetLimboSwitch(int switchNum)
	{
		if (!SceneHelper.IsPlayingCustom)
		{
			GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
			if (switchNum < generalProgress.limboSwitches.Length)
			{
				generalProgress.limboSwitches[switchNum] = true;
			}
			WriteFile(generalProgressPath, generalProgress);
		}
	}

	public static bool GetLimboSwitch(int switchNum)
	{
		return GetGeneralProgress().limboSwitches[switchNum];
	}

	public static int GetSecretMission(int missionNumber)
	{
		GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
		if (generalProgress.secretMissions.Length > missionNumber)
		{
			return generalProgress.secretMissions[missionNumber];
		}
		return 0;
	}

	public static void FoundSecretMission(int missionNumber)
	{
		if (!SceneHelper.IsPlayingCustom)
		{
			GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
			if (generalProgress.secretMissions[missionNumber] != 2)
			{
				generalProgress.secretMissions[missionNumber] = 1;
			}
			WriteFile(generalProgressPath, generalProgress);
		}
	}

	public static void SetSecretMission(int missionNumber)
	{
		if (!SceneHelper.IsPlayingCustom)
		{
			GameProgressMoneyAndGear generalProgress = GetGeneralProgress();
			generalProgress.secretMissions[missionNumber] = 2;
			WriteFile(generalProgressPath, generalProgress);
		}
	}

	public static int GetTotalSecretsFound()
	{
		if (lastTotalSecrets != -1)
		{
			return lastTotalSecrets;
		}
		FileInfo[] files = new DirectoryInfo(savePath).GetFiles("lvl*progress.bepis");
		int num = 0;
		FileInfo[] array = files;
		for (int i = 0; i < array.Length; i++)
		{
			if (ReadFile(array[i].FullName) is RankData rankData)
			{
				num += rankData.secretsFound.Count((bool a) => a);
			}
		}
		lastTotalSecrets = num;
		return num;
	}

	private static CyberRankData GetCyberRankData()
	{
		CyberRankData cyberRankData;
		if ((cyberRankData = ReadFile(cyberGrindHighScorePath) as CyberRankData) == null)
		{
			cyberRankData = new CyberRankData();
		}
		if (cyberRankData.preciseWavesByDifficulty == null || cyberRankData.preciseWavesByDifficulty.Length != 6)
		{
			cyberRankData.preciseWavesByDifficulty = new float[6];
		}
		if (cyberRankData.style == null || cyberRankData.style.Length != 6)
		{
			cyberRankData.style = new int[6];
		}
		if (cyberRankData.kills == null || cyberRankData.kills.Length != 6)
		{
			cyberRankData.kills = new int[6];
		}
		if (cyberRankData.time == null || cyberRankData.time.Length != 6)
		{
			cyberRankData.time = new float[6];
		}
		return cyberRankData;
	}

	public static CyberRankData GetBestCyber()
	{
		return GetCyberRankData();
	}

	public static void SetBestCyber(FinalCyberRank fcr)
	{
		if (!SceneHelper.IsPlayingCustom)
		{
			CyberRankData cyberRankData = GetCyberRankData();
			int @int = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
			cyberRankData.preciseWavesByDifficulty[@int] = fcr.savedWaves;
			cyberRankData.kills[@int] = fcr.savedKills;
			cyberRankData.style[@int] = fcr.savedStyle;
			cyberRankData.time[@int] = fcr.savedTime;
			WriteFile(cyberGrindHighScorePath, cyberRankData);
		}
	}
}
