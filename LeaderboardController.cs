using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
public class LeaderboardController : MonoSingleton<LeaderboardController>
{
	private readonly Dictionary<string, Leaderboard> cachedLeaderboards = new Dictionary<string, Leaderboard>();

	public async void SubmitCyberGrindScore(int difficulty, float wave, int kills, int style, float seconds)
	{
		if (!SteamClient.IsValid)
		{
			return;
		}
		int majorVersion = -1;
		int minorVersion = -1;
		string[] array = Application.version.Split('.');
		if (int.TryParse(array[0], out var result))
		{
			majorVersion = result;
		}
		if (array.Length > 1 && int.TryParse(array[1], out var result2))
		{
			minorVersion = result2;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Cyber Grind Wave ");
		stringBuilder.Append(LeaderboardProperties.Difficulties[difficulty]);
		Leaderboard? leaderboard = await FetchLeaderboard(stringBuilder.ToString());
		if (leaderboard.HasValue)
		{
			await leaderboard.Value.SubmitScoreAsync(Mathf.FloorToInt(wave), new int[6]
			{
				kills,
				style,
				Mathf.FloorToInt(seconds * 1000f),
				majorVersion,
				minorVersion,
				DateTimeOffset.UtcNow.Millisecond
			});
			stringBuilder.Append(" Precise");
			Leaderboard? leaderboard2 = await FetchLeaderboard(stringBuilder.ToString());
			if (leaderboard2.HasValue)
			{
				await leaderboard2.Value.SubmitScoreAsync(Mathf.FloorToInt(wave * 1000f), new int[6]
				{
					kills,
					style,
					Mathf.FloorToInt(seconds * 1000f),
					majorVersion,
					minorVersion,
					DateTimeOffset.UtcNow.Millisecond
				});
				Debug.Log($"Score {wave} submitted to Steamworks");
			}
		}
	}

	public async void SubmitLevelScore(string levelName, int difficulty, float seconds, int kills, int style, int restartCount, bool pRank = false)
	{
		if (SteamClient.IsValid)
		{
			int majorVersion = -1;
			int minorVersion = -1;
			string[] array = Application.version.Split('.');
			if (int.TryParse(array[0], out var result))
			{
				majorVersion = result;
			}
			if (array.Length > 1 && int.TryParse(array[1], out var result2))
			{
				minorVersion = result2;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(levelName);
			if (pRank)
			{
				stringBuilder.Append(" PRank");
			}
			else
			{
				stringBuilder.Append(" Any%");
			}
			Leaderboard? leaderboard = await FetchLeaderboard(stringBuilder.ToString(), createIfNotFound: true, LeaderboardSort.Ascending);
			if (leaderboard.HasValue)
			{
				Leaderboard value = leaderboard.Value;
				int score = Mathf.FloorToInt(seconds * 1000f + 0.5f);
				await value.SubmitScoreAsync(score, new int[7]
				{
					difficulty,
					kills,
					style,
					restartCount,
					majorVersion,
					minorVersion,
					DateTimeOffset.UtcNow.Millisecond
				});
				Debug.Log($"Score {seconds}s submitted to {stringBuilder} Steamworks");
			}
		}
	}

	public async Task<LeaderboardEntry[]> GetLevelScores(string levelName, bool pRank)
	{
		if (!SteamClient.IsValid)
		{
			return null;
		}
		if (!levelName.StartsWith("Level "))
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(levelName);
		if (pRank)
		{
			stringBuilder.Append(" PRank");
		}
		else
		{
			stringBuilder.Append(" Any%");
		}
		return await FetchLeaderboardEntries(stringBuilder.ToString(), LeaderboardType.Friends, 10, createIfNotFound: true, LeaderboardSort.Ascending);
	}

	public async Task<LeaderboardEntry[]> GetCyberGrindScores(int difficulty, LeaderboardType type)
	{
		if (!SteamClient.IsValid)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Cyber Grind Wave ");
		stringBuilder.Append(LeaderboardProperties.Difficulties[difficulty]);
		stringBuilder.Append(" Precise");
		return await FetchLeaderboardEntries(stringBuilder.ToString(), type);
	}

	public async Task<LeaderboardEntry[]> GetFishScores(LeaderboardType type)
	{
		if (!SteamClient.IsValid)
		{
			return null;
		}
		return (await FetchLeaderboardEntries("Fish Size", type, 20)).Where((LeaderboardEntry fs) => fs.Score == 1 || SteamController.BuiltInVerifiedSteamIds.Contains(fs.User.Id.Value)).Take(20).ToArray();
	}

	public async void SubmitFishSize(int fishSize)
	{
		if (!SteamClient.IsValid)
		{
			return;
		}
		int majorVersion = -1;
		int minorVersion = -1;
		string[] array = Application.version.Split('.');
		if (int.TryParse(array[0], out var result))
		{
			majorVersion = result;
		}
		if (array.Length > 1 && int.TryParse(array[1], out var result2))
		{
			minorVersion = result2;
		}
		Leaderboard? leaderboard = await FetchLeaderboard("Fish Size");
		if (leaderboard.HasValue)
		{
			Leaderboard value = leaderboard.Value;
			if (!SteamController.BuiltInVerifiedSteamIds.Contains<ulong>(SteamClient.SteamId))
			{
				await value.ReplaceScore(Mathf.FloorToInt(1f), new int[3]
				{
					majorVersion,
					minorVersion,
					DateTimeOffset.UtcNow.Millisecond
				});
			}
			else
			{
				await value.ReplaceScore(fishSize, new int[3]
				{
					majorVersion,
					minorVersion,
					DateTimeOffset.UtcNow.Millisecond
				});
			}
			Debug.Log("Fish submitted to Steamworks");
		}
	}

	private async Task<LeaderboardEntry[]> FetchLeaderboardEntries(string key, LeaderboardType type, int count = 10, bool createIfNotFound = false, LeaderboardSort defaultSortMode = LeaderboardSort.Descending)
	{
		if (!SteamClient.IsValid)
		{
			return null;
		}
		Leaderboard? leaderboard = await FetchLeaderboard(key, createIfNotFound, defaultSortMode);
		if (!leaderboard.HasValue)
		{
			return null;
		}
		Leaderboard value = leaderboard.Value;
		LeaderboardEntry[] array = type switch
		{
			LeaderboardType.Friends => await value.GetScoresFromFriendsAsync(), 
			LeaderboardType.Global => await value.GetScoresAsync(count), 
			LeaderboardType.GlobalAround => await value.GetScoresAroundUserAsync(-4, 3), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
		if (array == null)
		{
			return new LeaderboardEntry[0];
		}
		return array.Take(count).ToArray();
	}

	private async Task<Leaderboard?> FetchLeaderboard(string key, bool createIfNotFound = false, LeaderboardSort defaultSortMode = LeaderboardSort.Descending)
	{
		if (cachedLeaderboards.TryGetValue(key, out var value))
		{
			Debug.Log("Resolved leaderboard '" + key + "' from cache");
			return value;
		}
		Leaderboard? leaderboard = await SteamController.FetchSteamLeaderboard(key, createIfNotFound, defaultSortMode);
		if (!leaderboard.HasValue)
		{
			Debug.LogError("Failed to resolve leaderboard '" + key + "' from Steamworks");
			return null;
		}
		Leaderboard value2 = leaderboard.Value;
		cachedLeaderboards.Add(key, value2);
		Debug.Log("Resolved leaderboard '" + key + "' from Steamworks");
		return value2;
	}
}
