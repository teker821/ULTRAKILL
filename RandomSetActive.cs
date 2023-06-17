public class RandomSetActive : RandomBase<RandomGameObjectEntry>
{
	public bool resetStatesOnRandomize = true;

	public override void PerformTheAction(RandomEntry entry)
	{
		((RandomGameObjectEntry)entry)?.targetObject.SetActive(value: true);
	}

	public override void RandomizeWithCount(int count)
	{
		if (resetStatesOnRandomize)
		{
			RandomGameObjectEntry[] array = entries;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].targetObject.SetActive(value: false);
			}
		}
		base.RandomizeWithCount(count);
	}
}
