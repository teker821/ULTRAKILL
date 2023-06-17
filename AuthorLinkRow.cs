using UnityEngine;
using UnityEngine.UI;

public class AuthorLinkRow : MonoBehaviour
{
	public Text platformName;

	public Text platformUsername;

	public Text platformDisplayName;

	public Text description;

	private string url;

	public void Instantiate(string platform, string username, string displayName, Color platformColor, string targetURL, string descriptionText = "")
	{
		AuthorLinkRow authorLinkRow = Object.Instantiate(this, base.transform.parent, worldPositionStays: false);
		authorLinkRow.platformName.text = platform;
		authorLinkRow.platformName.color = platformColor;
		authorLinkRow.platformUsername.text = username;
		authorLinkRow.platformDisplayName.text = displayName;
		authorLinkRow.description.text = descriptionText;
		authorLinkRow.url = targetURL;
		authorLinkRow.gameObject.SetActive(value: true);
	}

	public void OnClick()
	{
		Application.OpenURL(url);
	}
}
