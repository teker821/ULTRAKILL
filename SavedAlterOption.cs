using System;
using Newtonsoft.Json;

[Serializable]
public class SavedAlterOption
{
	public string Key;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public float? FloatValue;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public bool? BoolValue;
}
