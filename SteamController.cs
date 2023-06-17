using System;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.UI;

public class SteamController : MonoBehaviour
{
	public static SteamController Instance;

	private Leaderboard? fishBoard;

	[SerializeField]
	private uint appId;

	private static string fishLeaderboard = "Fish Size";

	public static readonly ulong[] BuiltInVerifiedSteamIds = new ulong[2] { 76561198135929436uL, 76561197998177443uL };

	public static int FishSizeMulti
	{
		get
		{
			if (!SteamClient.IsValid)
			{
				return 1;
			}
			if (!BuiltInVerifiedSteamIds.Contains<ulong>(SteamClient.SteamId))
			{
				return 1;
			}
			return 2;
		}
	}

	private void Awake()
	{
		if ((bool)Instance)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		Instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		base.transform.SetParent(null);
		try
		{
			SteamClient.Init(appId);
			Debug.Log("Steam initialized!");
		}
		catch (Exception)
		{
			Debug.Log("Couldn't initialize steam");
		}
	}

	public static async void FetchAvatar(RawImage target, Friend user)
	{
		Steamworks.Data.Image? image = await user.GetMediumAvatarAsync();
		if (image.HasValue)
		{
			Texture2D texture2D = new Texture2D((int)image.Value.Width, (int)image.Value.Height, TextureFormat.RGBA32, mipChain: false);
			texture2D.LoadRawTextureData(image.Value.Data);
			texture2D.Apply();
			target.texture = texture2D;
		}
	}

	public void UpdateTimeInSandbox(float deltaTime)
	{
		if (SteamClient.IsValid)
		{
			deltaTime /= 3600f;
			SteamUserStats.AddStat("sandbox_total_time", deltaTime);
		}
	}

	public void AddToStatInt(string statKey, int amount)
	{
		if (SteamClient.IsValid)
		{
			SteamUserStats.AddStat(statKey, amount);
		}
	}

	public SandboxStats GetSandboxStats()
	{
		if (!SteamClient.IsValid)
		{
			return new SandboxStats();
		}
		return new SandboxStats
		{
			brushesBuilt = SteamUserStats.GetStatInt("sandbox_built_brushes"),
			propsSpawned = SteamUserStats.GetStatInt("sandbox_spawned_props"),
			enemiesSpawned = SteamUserStats.GetStatInt("sandbox_spawned_enemies"),
			hoursSpend = SteamUserStats.GetStatFloat("sandbox_total_time")
		};
	}

	public void UpdateWave(int wave)
	{
		if (SteamClient.IsValid)
		{
			SteamFriends.SetRichPresence("wave", wave.ToString());
		}
	}

	public static async Task<Leaderboard?> FetchSteamLeaderboard(string key, bool createIfNotFound = false, LeaderboardSort sort = LeaderboardSort.Descending, LeaderboardDisplay display = LeaderboardDisplay.TimeMilliSeconds)
	{
		if (createIfNotFound)
		{
			return await SteamUserStats.FindOrCreateLeaderboardAsync(key, sort, display);
		}
		return await SteamUserStats.FindLeaderboardAsync(key);
	}

	public void FetchSceneActivity(string scene)
	{
		if (!SteamClient.IsValid)
		{
			return;
		}
		if (SceneHelper.IsPlayingCustom)
		{
			SteamFriends.SetRichPresence("steam_display", "#AtCustomLevel");
			return;
		}
		StockMapInfo instance = StockMapInfo.Instance;
		if (scene == "Main Menu")
		{
			SteamFriends.SetRichPresence("steam_display", "#AtMainMenu");
		}
		else if (scene == "Endless")
		{
			SteamFriends.SetRichPresence("steam_display", "#AtCyberGrind");
			SteamFriends.SetRichPresence("difficulty", MonoSingleton<PresenceController>.Instance.diffNames[MonoSingleton<PrefsManager>.Instance.GetInt("difficulty")]);
			SteamFriends.SetRichPresence("wave", "0");
		}
		else if (instance != null && !string.IsNullOrEmpty(instance.assets.Deserialize().LargeText))
		{
			SteamFriends.SetRichPresence("steam_display", "#AtStandardLevel");
			SteamFriends.SetRichPresence("difficulty", MonoSingleton<PresenceController>.Instance.diffNames[MonoSingleton<PrefsManager>.Instance.GetInt("difficulty")]);
			SteamFriends.SetRichPresence("level", instance.assets.Deserialize().LargeText);
		}
		else
		{
			SteamFriends.SetRichPresence("steam_display", "#UnknownLevel");
		}
	}

	private void OnApplicationQuit()
	{
		if (SteamClient.IsValid)
		{
			SteamClient.Shutdown();
		}
	}
}
