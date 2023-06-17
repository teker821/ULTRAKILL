using UnityEngine;

namespace GameConsole.Commands;

internal class Buffs : CommandRoot
{
	public override string Name => "Buffs";

	public override string Description => "Modify buffs for enemies";

	public override string Command => "buffs";

	protected override void BuildTree(Console con)
	{
		BuildBoolMenu("forceradiance", () => OptionsManager.forceRadiance, delegate(bool value)
		{
			OptionsManager.forceRadiance = value;
			EnemyIdentifier[] array2 = Object.FindObjectsOfType<EnemyIdentifier>();
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j].UpdateBuffs();
			}
		});
		Branch("radiancetier", delegate
		{
			Leaf("get", delegate
			{
				con.PrintLine($"Current radiance tier is {OptionsManager.radianceTier}");
			});
			Leaf("set", delegate(float amt)
			{
				con.PrintLine($"Set current radiance tier to {amt}");
				OptionsManager.radianceTier = amt;
				EnemyIdentifier[] array = Object.FindObjectsOfType<EnemyIdentifier>();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].UpdateBuffs();
				}
			}, requireCheats: true);
		});
	}
}
