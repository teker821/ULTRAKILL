using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class CustomCampaign : CustomGameContent
{
	public int levelCount;

	public CampaignLevel[] levels;

	public bool valid;

	public CampaignJson json;

	public CustomCampaign(string path)
	{
		if (path.EndsWith("campaign.json"))
		{
			path = path.Substring(0, path.Length - "campaign.json".Length);
		}
		lastModified = File.GetLastWriteTime(path);
		base.path = path;
		if (!File.Exists(Path.Combine(path, "campaign.json")))
		{
			Debug.Log(path);
			Debug.LogError("Campaign is missing campaign.json");
			name = "<color=red>Loading Failed!</color>";
			levels = Array.Empty<CampaignLevel>();
			valid = false;
			return;
		}
		string text = File.ReadAllText(Path.Combine(path, "campaign.json"));
		Debug.Log(text);
		CampaignJson campaignJson = (json = JsonConvert.DeserializeObject<CampaignJson>(text));
		name = campaignJson.name;
		uniqueId = campaignJson.uuid;
		levelCount = campaignJson.levels.Length;
		levels = campaignJson.levels;
		author = campaignJson.author;
		valid = true;
		campaignJson.path = path;
		if (File.Exists(Path.Combine(path, "thumbnail.png")))
		{
			byte[] data = File.ReadAllBytes(Path.Combine(path, "thumbnail.png"));
			thumbnail = new Texture2D(640, 480)
			{
				filterMode = FilterMode.Point
			};
			thumbnail.LoadImage(data);
		}
	}
}
