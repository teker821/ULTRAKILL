using System;
using Discord;
using UnityEngine;

public class DiscordController : MonoBehaviour
{
	public static DiscordController Instance;

	[SerializeField]
	private long discordClientId;

	[Space]
	[SerializeField]
	private SerializedActivityAssets customLevelActivityAssets;

	[SerializeField]
	private SerializedActivityAssets missingActivityAssets;

	[SerializeField]
	private RankIcon[] rankIcons;

	private global::Discord.Discord discord;

	private ActivityManager activityManager;

	private int lastPoints;

	private bool disabled;

	private Activity cachedActivity;

	private void Awake()
	{
		if ((bool)Instance)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		base.transform.SetParent(null);
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		bool @bool = MonoSingleton<PrefsManager>.Instance.GetBool("discordIntegration");
		if (@bool)
		{
			Enable();
		}
		disabled = !@bool;
	}

	private void Update()
	{
		if (discord == null || disabled)
		{
			return;
		}
		try
		{
			discord.RunCallbacks();
		}
		catch (Exception)
		{
			Debug.Log("Discord lost");
			disabled = true;
			discord.Dispose();
		}
	}

	private void OnApplicationQuit()
	{
		if (discord != null && !disabled)
		{
			discord.Dispose();
		}
	}

	public static void UpdateRank(int rank)
	{
		if ((bool)Instance && !Instance.disabled)
		{
			if (Instance.rankIcons.Length <= rank)
			{
				Debug.LogError("Discord Controller is missing rank names/icons!");
				return;
			}
			Instance.cachedActivity.Assets.SmallText = Instance.rankIcons[rank].Text;
			Instance.cachedActivity.Assets.SmallImage = Instance.rankIcons[rank].Image;
			Instance.SendActivity();
		}
	}

	public static void UpdateStyle(int points)
	{
		if ((bool)Instance && !Instance.disabled && Instance.lastPoints != points)
		{
			Instance.lastPoints = points;
			Instance.cachedActivity.Details = "STYLE: " + points;
			Instance.SendActivity();
		}
	}

	public static void UpdateWave(int wave)
	{
		if ((bool)Instance && !Instance.disabled && Instance.lastPoints != wave)
		{
			Instance.lastPoints = wave;
			Instance.cachedActivity.Details = "WAVE: " + wave;
			Instance.SendActivity();
		}
	}

	public static void Disable()
	{
		if ((bool)Instance && Instance.discord != null && !Instance.disabled)
		{
			Instance.disabled = true;
			Instance.activityManager.ClearActivity(delegate
			{
			});
		}
	}

	public static void Enable()
	{
		if (!Instance || Instance.discord != null)
		{
			return;
		}
		try
		{
			Instance.discord = new global::Discord.Discord(Instance.discordClientId, 1uL);
			Instance.activityManager = Instance.discord.GetActivityManager();
			Debug.Log("Discord initialized!");
			Instance.disabled = false;
			Instance.ResetActivityCache();
		}
		catch (Exception)
		{
			Debug.Log("Couldn't initialize Discord");
			Instance.disabled = true;
		}
	}

	private void ResetActivityCache()
	{
		cachedActivity = new Activity
		{
			State = "LOADING",
			Assets = 
			{
				LargeImage = "generic",
				LargeText = "LOADING"
			},
			Instance = true
		};
	}

	public void FetchSceneActivity(string scene)
	{
		if (!Instance || Instance.disabled || Instance.discord == null)
		{
			return;
		}
		ResetActivityCache();
		if (SceneHelper.IsPlayingCustom)
		{
			cachedActivity.State = "Playing Custom Level";
			cachedActivity.Assets = customLevelActivityAssets.Deserialize();
		}
		else
		{
			StockMapInfo instance = StockMapInfo.Instance;
			if ((bool)instance)
			{
				cachedActivity.Assets = instance.assets.Deserialize();
				if (string.IsNullOrEmpty(cachedActivity.Assets.LargeImage))
				{
					cachedActivity.Assets.LargeImage = missingActivityAssets.Deserialize().LargeImage;
				}
				if (string.IsNullOrEmpty(cachedActivity.Assets.LargeText))
				{
					cachedActivity.Assets.LargeText = missingActivityAssets.Deserialize().LargeText;
				}
			}
			else
			{
				cachedActivity.Assets = missingActivityAssets.Deserialize();
			}
			if (scene == "Main Menu")
			{
				cachedActivity.State = "Main Menu";
			}
			else
			{
				cachedActivity.State = "DIFFICULTY: " + MonoSingleton<PresenceController>.Instance.diffNames[MonoSingleton<PrefsManager>.Instance.GetInt("difficulty")];
			}
		}
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		long start = (long)(DateTime.UtcNow - dateTime).TotalSeconds;
		cachedActivity.Timestamps = new ActivityTimestamps
		{
			Start = start
		};
		SendActivity();
	}

	private void SendActivity()
	{
		if (discord != null && activityManager != null && !disabled)
		{
			activityManager.UpdateActivity(cachedActivity, delegate
			{
			});
		}
	}
}
