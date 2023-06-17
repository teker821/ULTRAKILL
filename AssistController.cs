using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class AssistController : MonoSingleton<AssistController>
{
	public bool majorEnabled;

	[HideInInspector]
	public bool cheatsEnabled;

	[HideInInspector]
	public bool hidePopup;

	[HideInInspector]
	public float gameSpeed;

	[HideInInspector]
	public float damageTaken;

	[HideInInspector]
	public bool infiniteStamina;

	[HideInInspector]
	public bool disableHardDamage;

	[HideInInspector]
	public bool disableWhiplashHardDamage;

	[HideInInspector]
	public bool disableWeaponFreshness;

	[HideInInspector]
	public int difficultyOverride = -1;

	private StatsManager sman;

	private void Start()
	{
		majorEnabled = MonoSingleton<PrefsManager>.Instance.GetBool("majorAssist");
		gameSpeed = MonoSingleton<PrefsManager>.Instance.GetFloat("gameSpeed");
		damageTaken = MonoSingleton<PrefsManager>.Instance.GetFloat("damageTaken");
		infiniteStamina = MonoSingleton<PrefsManager>.Instance.GetBool("infiniteStamina");
		disableHardDamage = MonoSingleton<PrefsManager>.Instance.GetBool("disableHardDamage");
		disableWhiplashHardDamage = MonoSingleton<PrefsManager>.Instance.GetBool("disableWhiplashHardDamage");
		disableWeaponFreshness = MonoSingleton<PrefsManager>.Instance.GetBool("disableWeaponFreshness");
	}

	public void MajorEnabled()
	{
		majorEnabled = true;
		if (sman == null)
		{
			sman = MonoSingleton<StatsManager>.Instance;
		}
		sman.MajorUsed();
	}
}
