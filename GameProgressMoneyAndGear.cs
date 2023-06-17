using System;

[Serializable]
public class GameProgressMoneyAndGear
{
	public int money;

	public bool introSeen;

	public bool tutorialBeat;

	public bool clashModeUnlocked;

	public int rev0;

	public int rev1;

	public int rev2;

	public int rev3;

	public int revalt;

	public int sho0;

	public int sho1;

	public int sho2;

	public int sho3;

	public int nai0;

	public int nai1;

	public int nai2;

	public int nai3;

	public int naialt;

	public int rai0;

	public int rai1;

	public int rai2;

	public int rai3;

	public int rock0;

	public int rock1;

	public int rock2;

	public int rock3;

	public int beam0;

	public int beam1;

	public int beam2;

	public int beam3;

	public int arm1;

	public int arm2;

	public int arm3;

	public int[] secretMissions;

	public bool[] limboSwitches;

	public int[] newEnemiesFound;

	public bool[] unlockablesFound;

	public bool revCustomizationUnlocked;

	public bool shoCustomizationUnlocked;

	public bool naiCustomizationUnlocked;

	public bool raiCustomizationUnlocked;

	public bool rockCustomizationUnlocked;

	public GameProgressMoneyAndGear()
	{
		secretMissions = new int[10];
		newEnemiesFound = new int[Enum.GetValues(typeof(EnemyType)).Length];
	}
}
