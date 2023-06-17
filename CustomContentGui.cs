using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Steamworks;
using Steamworks.Ugc;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CustomContentGui : MonoBehaviour
{
	[SerializeField]
	private GameObject grid;

	[SerializeField]
	private CustomLevelButton buttonTemplate;

	[SerializeField]
	private GameObject workshopError;

	[SerializeField]
	private GameObject fetchingPanel;

	[SerializeField]
	private GameObject loadingFailed;

	[SerializeField]
	private InputField workshopSearch;

	[SerializeField]
	private Texture2D placeholderThumbnail;

	[SerializeField]
	private PersistentColors nameColors;

	[SerializeField]
	private Dropdown difficultyDropdown;

	[SerializeField]
	private GameObject workshopButtons;

	[SerializeField]
	private Button[] workshopTabButtons;

	[SerializeField]
	private GameObject localButtons;

	[SerializeField]
	private Dropdown localSortModeDropdown;

	[Space]
	[SerializeField]
	public CampaignViewScreen campaignView;

	private Action afterLegacyAgonyInterrupt;

	private UnscaledTimeSince timeSinceStart;

	private static LocalSortMode currentLocalSortMode = LocalSortMode.Name;

	private static WorkshopTab currentWorkshopTab = WorkshopTab.Trending;

	private static bool lastTabWorkshop;

	public static CustomCampaign lastCustomCampaign;

	public static bool wasAgonyOpen { get; private set; }

	private void Awake()
	{
		SceneHelper.LoadScene("Main Menu");
	}

	private void ResetGrid()
	{
		for (int i = 1; i < grid.transform.childCount; i++)
		{
			UnityEngine.Object.Destroy(grid.transform.GetChild(i).gameObject);
		}
	}

	public void DismissBlockers()
	{
		loadingFailed.SetActive(value: false);
	}

	public void ShowInExplorer()
	{
		Application.OpenURL("file://" + GameProgressSaver.customMapsPath);
	}

	public void SetLocalSortMode(int option)
	{
		currentLocalSortMode = (LocalSortMode)option;
		MonoSingleton<PrefsManager>.Instance.SetInt("agonyLocalSortMode", option);
		RefreshCustomMaps();
	}

	public void SetDifficulty(int dif)
	{
		MonoSingleton<PrefsManager>.Instance.SetInt("difficulty", dif);
	}

	public void SetWorkshopTab(int tab)
	{
		Button[] array = workshopTabButtons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].interactable = true;
		}
		MonoSingleton<PrefsManager>.Instance.SetInt("agonyWorkshopTab", tab);
		currentWorkshopTab = (WorkshopTab)tab;
		workshopTabButtons[(int)currentWorkshopTab].interactable = false;
		workshopSearch.interactable = tab > 2;
		if (!workshopSearch.interactable)
		{
			workshopSearch.text = string.Empty;
		}
		RefreshWorkshopItems();
	}

	public static Color NameToColor(PersistentColors colorData, string targetName)
	{
		int seed = BitConverter.ToInt32(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(targetName)), 0);
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(seed);
		Color result = colorData.colors[UnityEngine.Random.Range(0, colorData.colors.Length)];
		UnityEngine.Random.state = state;
		return result;
	}

	public async void RefreshWorkshopItems()
	{
		localButtons.SetActive(value: false);
		workshopButtons.SetActive(value: true);
		lastTabWorkshop = true;
		ResetGrid();
		if (!SteamClient.IsValid || !SteamClient.IsLoggedOn)
		{
			workshopError.SetActive(value: true);
			return;
		}
		fetchingPanel.SetActive(value: true);
		Query items = Query.Items;
		items = currentWorkshopTab switch
		{
			WorkshopTab.Subscribed => items.WhereUserSubscribed(), 
			WorkshopTab.Favorite => items.WhereUserFavorited(), 
			WorkshopTab.YourUploads => items.WhereUserPublished(), 
			WorkshopTab.Trending => items.RankedByTrend().SortByCreationDate(), 
			WorkshopTab.BestVoted => items.RankedByVote(), 
			WorkshopTab.Latest => items.RankedByPublicationDate(), 
			WorkshopTab.TotalLifetimePlaytime => items.RankedByTotalPlaytime(), 
			WorkshopTab.AveragePlaytime => items.RankedByLifetimeAveragePlaytime(), 
			_ => items.RankedByVote(), 
		};
		if (!string.IsNullOrEmpty(workshopSearch.text))
		{
			items.WhereSearchText(workshopSearch.text);
		}
		ResultPage? resultPage = await items.WithTag((currentWorkshopTab == WorkshopTab.Campaigns) ? "campaign" : "map").GetPageAsync(1);
		if (!resultPage.HasValue)
		{
			workshopError.SetActive(value: true);
			fetchingPanel.SetActive(value: false);
		}
		else
		{
			if (!lastTabWorkshop)
			{
				return;
			}
			Debug.Log($"ResultCount: {resultPage?.ResultCount}");
			Debug.Log($"TotalCount: {resultPage?.TotalCount}");
			foreach (Item entry in resultPage.Value.Entries)
			{
				CustomLevelButton customLevelButton = UnityEngine.Object.Instantiate(buttonTemplate, grid.transform, worldPositionStays: false);
				customLevelButton.mapName.text = Path.GetFileName(entry.Title) + " <b><size=10><color=gray>by</color> <color=#" + ColorUtility.ToHtmlStringRGB(NameToColor(nameColors, entry.Owner.Id.Value.ToString())) + ">" + entry.Owner.Name + "</color></size></b>";
				customLevelButton.mapDetails.text = $"<color=#8bea83>{entry.VotesUp} /\\</color>  <color=#e26363>{entry.VotesDown} \\/</color>  <color=#ece477>{entry.NumFavorites} *</color>  <color=#8e8e8e>{entry.NumComments} C</color>";
				if (entry.NumSecondsPlayed != 0)
				{
					Text mapDetails = customLevelButton.mapDetails;
					mapDetails.text = mapDetails.text + "  <color=#46d6f0>" + FormatTimeString(entry.NumSecondsPlayed) + "</color>";
				}
				if (entry.NumPlaytimeSessions != 0)
				{
					customLevelButton.mapDetails.text += $" (<color=#4e8a96>{entry.NumPlaytimeSessions} sessions</color>)";
				}
				if (!string.IsNullOrEmpty(entry.PreviewImageUrl))
				{
					StartCoroutine(FetchPreviewImage(customLevelButton.thumbnail, entry.PreviewImageUrl));
				}
				else
				{
					customLevelButton.thumbnail.texture = placeholderThumbnail;
				}
				customLevelButton.customCampaignBadge.SetActive(entry.Tags.Contains("campaign"));
				bool flag = entry.IsInstalled;
				if (entry.NeedsUpdate)
				{
					flag = false;
				}
				customLevelButton.downloadArrowIcon.gameObject.SetActive(!flag);
				customLevelButton.bodyButton.onClick.AddListener(delegate
				{
					if (!((float)timeSinceStart < 0.5f))
					{
						SceneHelper.LoadWorkshopMap(entry, delegate
						{
							loadingFailed.SetActive(value: true);
							SceneHelper.DismissBlockers();
						});
					}
				});
				customLevelButton.gameObject.SetActive(value: true);
			}
			buttonTemplate.gameObject.SetActive(value: false);
			fetchingPanel.SetActive(value: false);
		}
	}

	private string FormatTimeString(ulong seconds)
	{
		if (seconds < 60)
		{
			return $"{seconds}s";
		}
		if (seconds < 3600)
		{
			return $"{seconds / 60uL}m";
		}
		return $"{seconds / 3600uL}h";
	}

	public static IEnumerator FetchPreviewImage(RawImage target, string url)
	{
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
		yield return www.SendWebRequest();
		if (www.isNetworkError || www.isHttpError)
		{
			Debug.Log(www.error);
		}
		else if ((bool)target)
		{
			target.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
		}
	}

	public void RefreshCustomMaps()
	{
		workshopSearch.text = string.Empty;
		lastTabWorkshop = false;
		workshopError.SetActive(value: false);
		fetchingPanel.SetActive(value: false);
		localButtons.SetActive(value: true);
		workshopButtons.SetActive(value: false);
		ResetGrid();
		CustomGameContent[] localMaps = AgonyHelper.GetLocalMaps(GameProgressSaver.customMapsPath, currentLocalSortMode);
		Debug.Log($"Found {localMaps.Length} entries in {GameProgressSaver.customMapsPath}");
		CustomGameContent[] array = localMaps;
		foreach (CustomGameContent map in array)
		{
			CustomLevelButton customLevelButton = UnityEngine.Object.Instantiate(buttonTemplate, grid.transform, worldPositionStays: false);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append((map is CustomCampaign) ? map.name : Path.GetFileName(map.name));
			if (!string.IsNullOrEmpty(map.author))
			{
				stringBuilder.Append(" <size=10><color=gray>by</color> ");
				stringBuilder.Append("<color=#");
				stringBuilder.Append(ColorUtility.ToHtmlStringRGB(NameToColor(nameColors, map.author)));
				stringBuilder.Append(">");
				stringBuilder.Append(map.author);
				stringBuilder.Append("</color></size>");
			}
			else
			{
				stringBuilder.Append(" <size=10><color=gray>Unknown Author</color></size>");
			}
			CustomCampaign campaign = map as CustomCampaign;
			if (campaign != null)
			{
				if (campaign.valid)
				{
					customLevelButton.bodyButton.onClick.AddListener(delegate
					{
						if (!((float)timeSinceStart < 0.5f))
						{
							campaignView.Show(campaign);
							lastCustomCampaign = campaign;
						}
					});
				}
			}
			else
			{
				customLevelButton.bodyButton.onClick.AddListener(delegate
				{
					if (!((float)timeSinceStart < 0.5f))
					{
						SceneHelper.LoadCustomMap(map.path, new CustomGameDetails
						{
							levelNumber = 1,
							uniqueIdentifier = map.uniqueId
						}, delegate
						{
							loadingFailed.SetActive(value: true);
							SceneHelper.DismissBlockers();
						}, isLocal: true);
					}
				});
			}
			customLevelButton.customCampaignBadge.SetActive(campaign != null);
			customLevelButton.mapName.text = stringBuilder.ToString();
			customLevelButton.mapDetails.text = map.shortPath;
			customLevelButton.thumbnail.texture = (map.thumbnail ? map.thumbnail : placeholderThumbnail);
			customLevelButton.downloadArrowIcon.gameObject.SetActive(value: false);
			customLevelButton.gameObject.SetActive(value: true);
		}
		buttonTemplate.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			wasAgonyOpen = false;
			SceneHelper.LoadScene("Main Menu");
		}
	}
}
