using System;
using Discord;

[Serializable]
public struct SerializedActivityAssets
{
	public string LargeImage;

	public string LargeText;

	public ActivityAssets Deserialize()
	{
		ActivityAssets result = default(ActivityAssets);
		result.LargeImage = LargeImage;
		result.LargeText = LargeText;
		return result;
	}
}
