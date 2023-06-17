public class StatueIntroChecker : MonoSingleton<StatueIntroChecker>
{
	public bool beenSeen;

	public void BeenSeen()
	{
		beenSeen = true;
	}
}
