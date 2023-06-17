using UnityEngine;

[DefaultExecutionOrder(-300)]
public class StockMapInfo : MapInfoBase
{
	public static StockMapInfo Instance;

	public SerializedActivityAssets assets;

	private void Awake()
	{
		if (!Instance)
		{
			Instance = this;
		}
		MapInfoBase.InstanceAnyType = this;
	}
}
