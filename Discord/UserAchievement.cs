using System.Runtime.InteropServices;

namespace Discord;

public struct UserAchievement
{
	public long UserId;

	public long AchievementId;

	public byte PercentComplete;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
	public string UnlockedAt;
}
