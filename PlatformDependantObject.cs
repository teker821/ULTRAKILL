using System;
using UnityEngine;

public class PlatformDependantObject : MonoBehaviour
{
	[SerializeField]
	private bool requiresSteam;

	[SerializeField]
	private bool requiresDiscord;

	[SerializeField]
	private bool requiresFileSystemAccess;

	[SerializeField]
	private bool hideInSolsticeRelease;

	[SerializeField]
	private UltrakillEvent onDestroy;

	private void Awake()
	{
		_ = requiresSteam;
		_ = requiresDiscord;
		_ = requiresFileSystemAccess;
		if (hideInSolsticeRelease && Environment.GetEnvironmentVariable("SOLSTICE_LAUNCH_MODE") == "RELEASE")
		{
			onDestroy.Invoke();
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
