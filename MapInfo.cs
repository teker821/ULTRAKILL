using UnityEngine;

[DefaultExecutionOrder(-300)]
public class MapInfo : MapInfoBase
{
	public static MapInfo Instance;

	public string uniqueId;

	public string mapName;

	public string description;

	public string author;

	[Header("Has to be 640x480")]
	public Texture2D thumbnail;

	[Header("Map Configuration")]
	public bool renderSkybox;

	private void Awake()
	{
		if (!Instance)
		{
			Instance = this;
		}
		MapInfoBase.InstanceAnyType = this;
	}
}
