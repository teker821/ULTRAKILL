using System;
using Newtonsoft.Json;

[Serializable]
public class SavedGeneric
{
	public string ObjectIdentifier;

	public SavedVector3 Position;

	public SavedQuaternion Rotation;

	public SavedVector3 Scale;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public SavedAlterData[] Data;
}
