using System;
using Newtonsoft.Json;

[Serializable]
public class CampaignJson
{
	public string name;

	public string author;

	public string uuid;

	public CampaignLevel[] levels;

	[JsonIgnore]
	public string path;
}
