using System.Text;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.UI;

public class FishLeaderboard : MonoBehaviour
{
	[SerializeField]
	private Text globalText;

	[SerializeField]
	private Text friendsText;

	private void OnEnable()
	{
		Fetch();
	}

	private async void Fetch()
	{
		LeaderboardEntry[] obj = await MonoSingleton<LeaderboardController>.Instance.GetFishScores(LeaderboardType.Global);
		StringBuilder strBlrd = new StringBuilder();
		strBlrd.AppendLine("<b>GLOBAL</b>");
		int num = 1;
		LeaderboardEntry[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			LeaderboardEntry leaderboardEntry = array[i];
			Friend user = leaderboardEntry.User;
			string text = user.Name;
			if (text.Length > 15)
			{
				text = text.Substring(0, 15);
			}
			user = leaderboardEntry.User;
			if (user.IsMe)
			{
				strBlrd.Append("<color=orange>");
			}
			strBlrd.AppendLine($"[{num}] {leaderboardEntry.Score} - {text}");
			user = leaderboardEntry.User;
			if (user.IsMe)
			{
				strBlrd.Append("</color>");
			}
			num++;
		}
		globalText.text = strBlrd.ToString();
		LeaderboardEntry[] obj2 = await MonoSingleton<LeaderboardController>.Instance.GetFishScores(LeaderboardType.Friends);
		strBlrd.Clear();
		strBlrd.AppendLine("<b>FRIENDS</b>");
		array = obj2;
		for (int i = 0; i < array.Length; i++)
		{
			LeaderboardEntry leaderboardEntry2 = array[i];
			Friend user = leaderboardEntry2.User;
			string text2 = user.Name;
			if (text2.Length > 15)
			{
				text2 = text2.Substring(0, 15);
			}
			user = leaderboardEntry2.User;
			if (user.IsMe)
			{
				strBlrd.Append("<color=orange>");
			}
			strBlrd.AppendLine($"[{leaderboardEntry2.GlobalRank}] {leaderboardEntry2.Score} - {text2}");
			user = leaderboardEntry2.User;
			if (user.IsMe)
			{
				strBlrd.Append("</color>");
			}
		}
		friendsText.text = strBlrd.ToString();
	}
}
