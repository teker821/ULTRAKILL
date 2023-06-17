using pcon;

namespace GameConsole.Commands;

public class Pcon : CommandRoot
{
	public override string Name => "pcon";

	public override string Description => "pcon commands";

	protected override void BuildTree(Console con)
	{
		Leaf("connect", delegate
		{
			PconClient.StartClient();
		});
		BuildBoolMenu("autostart", () => MonoSingleton<PrefsManager>.Instance.GetBoolLocal("pcon.autostart"), delegate(bool value)
		{
			MonoSingleton<PrefsManager>.Instance.SetBoolLocal("pcon.autostart", value);
			if (value)
			{
				MonoSingleton<Console>.Instance.StartPcon();
			}
		});
	}
}
