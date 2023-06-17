using System;

namespace GameConsole.Commands;

internal class Style : CommandRoot
{
	public override string Name => "Style";

	public override string Description => "Modify your style score";

	public override string Command => "style";

	protected override void BuildTree(Console con)
	{
		BuildBoolMenu("meter", () => MonoSingleton<StyleHUD>.Instance.forceMeterOn, delegate(bool value)
		{
			MonoSingleton<StyleHUD>.Instance.forceMeterOn = value;
		});
		Branch("freshness", delegate
		{
			Leaf("get", delegate
			{
				con.PrintLine($"Current weapon freshness is {MonoSingleton<StyleHUD>.Instance.GetFreshness(MonoSingleton<GunControl>.Instance.currentWeapon)}");
			});
			Leaf("set", delegate(float amt)
			{
				con.PrintLine($"Set current weapon freshness to {amt}");
				MonoSingleton<StyleHUD>.Instance.SetFreshness(MonoSingleton<GunControl>.Instance.currentWeapon, amt);
			}, requireCheats: true);
			Leaf("lock_state", delegate(int slot, StyleFreshnessState state)
			{
				con.PrintLine($"Locking slot {slot} to {Enum.GetName(typeof(StyleFreshnessState), state)}");
				MonoSingleton<StyleHUD>.Instance.LockFreshness(slot, state);
			}, requireCheats: true);
			Leaf("unlock", delegate(int slot)
			{
				con.PrintLine($"Unlocking slot {slot}");
				MonoSingleton<StyleHUD>.Instance.UnlockFreshness(slot);
			}, requireCheats: true);
		});
	}
}
