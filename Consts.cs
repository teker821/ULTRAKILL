using UnityEngine;

public static class Consts
{
	public const bool STEAM_SUPPORTED = true;

	public const bool DISCORD_SUPPORTED = true;

	public const bool LEADERBOARDS_SUPPORTED = true;

	public const bool UNITY_MANAGED_SAVES = false;

	public const bool ENSURE_NO_FREEZE = false;

	public const bool AMAZON_LUNA_BUILD = false;

	public const bool AGONY_BUILD = false;

	public static bool CONSOLE_ERROR_BADGE => Debug.isDebugBuild;
}
