using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox;

public class StatsDisplay : MonoBehaviour
{
	[SerializeField]
	private Text textContent;

	private TimeSince timeSinceUpdate;

	private void UpdateDisplay()
	{
		if (!(SteamController.Instance == null) && SteamClient.IsValid)
		{
			SandboxStats sandboxStats = SteamController.Instance.GetSandboxStats();
			textContent.text = $"<color=orange>{sandboxStats.brushesBuilt}</color> - TOTAL BOXES BUILT\n" + $"<color=orange>{sandboxStats.propsSpawned}</color> - TOTAL PROPS PLACED\n" + $"<color=orange>{sandboxStats.enemiesSpawned}</color> - TOTAL ENEMIES SPAWNED\n" + $"<color=orange>{sandboxStats.hoursSpend:F1}h</color> - TOTAL TIME IN SANDBOX\n";
		}
	}

	private void OnEnable()
	{
		UpdateDisplay();
		timeSinceUpdate = 0f;
	}

	private void Update()
	{
		if ((float)timeSinceUpdate > 2f)
		{
			timeSinceUpdate = 0f;
			UpdateDisplay();
		}
	}
}
