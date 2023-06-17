using Steamworks;
using Steamworks.Ugc;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class WorkshopMapEndRating : MonoSingleton<WorkshopMapEndRating>
{
	[SerializeField]
	private GameObject container;

	[SerializeField]
	private Text mapName;

	[SerializeField]
	private Button voteUpButton;

	[SerializeField]
	private GameObject votedUpObject;

	[SerializeField]
	private Button voteDownButton;

	[SerializeField]
	private GameObject votedDownObject;

	[SerializeField]
	private Texture2D placeholderThumbnail;

	[SerializeField]
	private RawImage thumbnail;

	[SerializeField]
	private PersistentColors nameColors;

	private Item cachedItem;

	protected override void Awake()
	{
		base.Awake();
		voteUpButton.interactable = false;
		voteDownButton.interactable = false;
	}

	private new void OnEnable()
	{
		container.SetActive(value: true);
		FetchWorkshopData();
		GameStateManager.Instance.RegisterState(new GameState("workshop-rate-screen", container)
		{
			cursorLock = LockMode.Unlock
		});
	}

	private async void FetchWorkshopData()
	{
		if (!GameStateManager.Instance.currentCustomGame.workshopId.HasValue)
		{
			return;
		}
		Item? item = await AgonyHelper.FetchWorkshopItemInfo(GameStateManager.Instance.currentCustomGame.workshopId.Value);
		if (!item.HasValue)
		{
			return;
		}
		Item item2 = (cachedItem = item.Value);
		mapName.text = item2.Title + " <b><size=12><color=gray>by</color> <color=#" + ColorUtility.ToHtmlStringRGB(CustomContentGui.NameToColor(nameColors, item2.Owner.Id.Value.ToString())) + ">" + item2.Owner.Name + "</color></size></b>";
		if (item2.PreviewImageUrl == string.Empty)
		{
			thumbnail.texture = placeholderThumbnail;
		}
		else
		{
			StartCoroutine(CustomContentGui.FetchPreviewImage(thumbnail, item2.PreviewImageUrl));
		}
		UserItemVote? userItemVote = await item2.GetUserVote();
		if (userItemVote.HasValue)
		{
			UserItemVote value = userItemVote.Value;
			voteUpButton.interactable = !value.VotedUp;
			if ((bool)votedUpObject)
			{
				votedUpObject.SetActive(value.VotedUp);
			}
			voteDownButton.interactable = !value.VotedDown;
			if ((bool)votedDownObject)
			{
				votedDownObject.SetActive(value.VotedDown);
			}
		}
	}

	public void VoteUp()
	{
		cachedItem.Vote(up: true);
		voteUpButton.interactable = false;
		voteDownButton.interactable = true;
		if ((bool)votedUpObject)
		{
			votedUpObject.SetActive(value: true);
		}
		if ((bool)votedDownObject)
		{
			votedDownObject.SetActive(value: false);
		}
	}

	public void VoteDown()
	{
		cachedItem.Vote(up: false);
		voteUpButton.interactable = true;
		voteDownButton.interactable = false;
		if ((bool)votedUpObject)
		{
			votedUpObject.SetActive(value: false);
		}
		if ((bool)votedDownObject)
		{
			votedDownObject.SetActive(value: true);
		}
	}

	public void LeaveAComment()
	{
		SteamFriends.OpenWebOverlay($"https://steamcommunity.com/sharedfiles/filedetails/?id={cachedItem.Id}#PublicComments");
	}

	public void ToggleFavorite()
	{
	}

	public void JustContinue()
	{
		MonoSingleton<FinalRank>.Instance.LevelChange(force: true);
	}
}
