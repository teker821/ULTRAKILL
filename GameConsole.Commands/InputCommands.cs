namespace GameConsole.Commands;

internal class InputCommands : CommandRoot
{
	public override string Name => "Input";

	public override string Description => "Modify inputs";

	public override string Command => "input";

	protected override void BuildTree(Console con)
	{
		Branch("mouse", delegate
		{
			Leaf("sensitivity", delegate(float amount)
			{
				con.PrintLine($"Set mouse sensitivity to {amount}");
				MonoSingleton<OptionsMenuToManager>.Instance.MouseSensitivity(amount);
				MonoSingleton<OptionsMenuToManager>.Instance.UpdateSensitivitySlider(amount);
			});
		});
	}
}
