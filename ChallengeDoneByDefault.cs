[ConfigureSingleton(SingletonFlags.DestroyDuplicates)]
public class ChallengeDoneByDefault : MonoSingleton<ChallengeDoneByDefault>
{
	private bool prepared;

	private void Start()
	{
		if (!prepared)
		{
			Prepare();
		}
	}

	public void Prepare()
	{
		if (!prepared)
		{
			MonoSingleton<ChallengeManager>.Instance.challengeDone = true;
			prepared = true;
		}
	}
}
