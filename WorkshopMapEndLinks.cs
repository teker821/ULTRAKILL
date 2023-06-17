using System.Text.RegularExpressions;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class WorkshopMapEndLinks : MonoSingleton<WorkshopMapEndLinks>
{
	[SerializeField]
	private AuthorLinkRow baseRow;

	[SerializeField]
	private GameObject container;

	public void Show()
	{
		AdditionalMapDetails additionalMapDetails = MonoSingleton<AdditionalMapDetails>.Instance;
		bool flag = (bool)additionalMapDetails && additionalMapDetails.hasAuthorLinks;
		container.SetActive(flag);
		if (flag)
		{
			AuthorLink[] authorLinks = additionalMapDetails.authorLinks;
			foreach (AuthorLink authorLink in authorLinks)
			{
				baseRow.Instantiate(authorLink.platform.ToString(), authorLink.username, authorLink.displayName, GetColor(authorLink.platform), GetLink(authorLink.platform, authorLink.username), authorLink.description);
			}
			GameStateManager.Instance.RegisterState(new GameState("workshop-map-credits", container)
			{
				cursorLock = LockMode.Unlock
			});
		}
	}

	public static string GetLink(LinkPlatform platform, string username)
	{
		username = username.Replace(" ", "");
		username = username.Replace(".", "");
		username = Regex.Replace(username, "[^a-zA-Z0-9_]", "");
		return platform switch
		{
			LinkPlatform.YouTube => "https://www.youtube.com/channel/" + username, 
			LinkPlatform.Twitch => "https://twitch.tv/" + username, 
			LinkPlatform.Twitter => "https://twitter.com/" + username, 
			LinkPlatform.Steam => "https://steamcommunity.com/id/" + username, 
			LinkPlatform.SoundCloud => "https://soundcloud.com/" + username, 
			LinkPlatform.KoFi => "https://ko-fi.com/" + username, 
			LinkPlatform.Patreon => "https://www.patreon.com/" + username, 
			LinkPlatform.Bandcamp => "https://" + username + ".bandcamp.com", 
			LinkPlatform.PayPalMe => "https://www.paypal.me/" + username, 
			_ => string.Empty, 
		};
	}

	public static Color GetColor(LinkPlatform platform)
	{
		return platform switch
		{
			LinkPlatform.YouTube => new Color(1f, 0.4392157f, 0.4392157f), 
			LinkPlatform.Twitch => new Color(48f / 85f, 0.34901962f, 82f / 85f), 
			LinkPlatform.Twitter => new Color(32f / 51f, 0.85490197f, 1f), 
			LinkPlatform.Steam => new Color(0.6039216f, 0.64705884f, 0.81960785f), 
			LinkPlatform.SoundCloud => new Color(1f, 0.7f, 0.47f), 
			LinkPlatform.KoFi => new Color(0.65f, 0.95f, 1f), 
			LinkPlatform.Patreon => new Color(1f, 0.6f, 0.44f), 
			LinkPlatform.Bandcamp => new Color(0.6f, 0.97f, 1f), 
			LinkPlatform.PayPalMe => new Color(0.63f, 0.77f, 1f), 
			_ => Color.white, 
		};
	}

	public void Continue()
	{
		MonoSingleton<AdditionalMapDetails>.Instance.hasAuthorLinks = false;
		container.SetActive(value: false);
		MonoSingleton<FinalRank>.Instance.LevelChange();
	}
}
