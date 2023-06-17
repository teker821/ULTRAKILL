using System.Collections;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LevelEndLeaderboard : MonoBehaviour
{
	[SerializeField]
	private GameObject template;

	[SerializeField]
	private Text templateUsername;

	[SerializeField]
	private Text templateTime;

	[SerializeField]
	private Text templateDifficulty;

	[Space]
	[SerializeField]
	private Transform container;

	[SerializeField]
	private Text leaderboardType;

	[SerializeField]
	private Text switchTypeInput;

	[Space]
	[SerializeField]
	private GameObject loadingPanel;

	private bool displayPRank;

	private const string LeftBracket = "<color=white>[";

	private const string RightBracket = "]</color>";

	private void OnEnable()
	{
		if (SceneHelper.IsPlayingCustom || !SceneHelper.CurrentScene.StartsWith("Level ") || !GameStateManager.ShowLeaderboards || !MonoSingleton<PrefsManager>.Instance.GetBool("levelLeaderboards"))
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		Debug.Log("Fetching level leaderboards for " + SceneHelper.CurrentScene);
		displayPRank = MonoSingleton<StatsManager>.Instance.rankScore == 12;
		StartCoroutine(Fetch(SceneHelper.CurrentScene));
	}

	private IEnumerator Fetch(string levelName)
	{
		if (string.IsNullOrEmpty(levelName))
		{
			yield break;
		}
		ResetEntries();
		loadingPanel.SetActive(value: true);
		leaderboardType.text = (displayPRank ? "P RANK" : "ANY RANK");
		Task<LeaderboardEntry[]> entryTask = MonoSingleton<LeaderboardController>.Instance.GetLevelScores(levelName, displayPRank);
		while (!entryTask.IsCompleted)
		{
			yield return null;
		}
		if (entryTask.Result == null)
		{
			yield break;
		}
		LeaderboardEntry[] result = entryTask.Result;
		for (int i = 0; i < result.Length; i++)
		{
			LeaderboardEntry leaderboardEntry = result[i];
			Text text = templateUsername;
			Friend user = leaderboardEntry.User;
			text.text = user.Name;
			int score = leaderboardEntry.Score;
			int num = score / 60000;
			float num2 = (float)(score - num * 60000) / 1000f;
			templateTime.text = $"{num}:{num2:00.000}";
			int? num3 = null;
			if (leaderboardEntry.Details.Length != 0)
			{
				num3 = leaderboardEntry.Details[0];
			}
			if (LeaderboardProperties.Difficulties.Length <= num3)
			{
				Debug.LogWarning($"Difficulty {num3} is out of range for {levelName}");
				continue;
			}
			templateDifficulty.text = ((!num3.HasValue) ? "UNKNOWN" : LeaderboardProperties.Difficulties[num3.Value].ToUpper());
			GameObject obj = Object.Instantiate(template, container);
			obj.SetActive(value: true);
			SteamController.FetchAvatar(obj.GetComponentInChildren<RawImage>(), leaderboardEntry.User);
		}
		loadingPanel.SetActive(value: false);
		container.gameObject.SetActive(value: true);
	}

	private void ResetEntries()
	{
		foreach (Transform item in container)
		{
			if (!(item == template.transform))
			{
				Object.Destroy(item.gameObject);
			}
		}
	}

	private void Update()
	{
		if (MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad)
		{
			if (MonoSingleton<InputManager>.Instance.InputSource.NextWeapon.Action.bindings.Count <= 0)
			{
				switchTypeInput.text = "<color=white>[<color=orange>NO BINDING</color>]</color>";
				return;
			}
			string text = "<color=white>[<color=orange>" + MonoSingleton<InputManager>.Instance.InputSource.NextWeapon.Action.bindings[0].ToDisplayString().ToUpper() + "</color>]</color>";
			switchTypeInput.text = text;
		}
		else
		{
			if (MonoSingleton<InputManager>.Instance.InputSource.LastWeapon.Action.bindings.Count <= 1)
			{
				switchTypeInput.text = "<color=white>[<color=orange>NO BINDING</color>]</color>";
				return;
			}
			switchTypeInput.text = "<color=white>[<color=orange>" + MonoSingleton<InputManager>.Instance.InputSource.LastWeapon.Action.bindings[1].ToDisplayString().ToUpper() + "</color>]</color>";
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.NextWeapon.WasPerformedThisFrame || MonoSingleton<InputManager>.Instance.InputSource.LastWeapon.WasPerformedThisFrame)
		{
			displayPRank = !displayPRank;
			StopAllCoroutines();
			StartCoroutine(Fetch(SceneHelper.CurrentScene));
		}
	}
}
