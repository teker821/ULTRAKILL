using UnityEngine;
using UnityEngine.UI;

public class CampaignViewScreen : MonoBehaviour
{
	[SerializeField]
	private Text campaignTitle;

	[SerializeField]
	private CustomLevelButton buttonTemplate;

	[SerializeField]
	private Texture2D placeholderThumbnail;

	[SerializeField]
	private GameObject grid;

	public void Show(CustomCampaign campaign)
	{
	}

	public void Close()
	{
	}

	private void ResetGrid()
	{
		for (int i = 1; i < grid.transform.childCount; i++)
		{
			Object.Destroy(grid.transform.GetChild(i).gameObject);
		}
	}
}
