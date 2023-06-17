using System.Collections.Generic;

public class CrossfadeTracker : MonoSingleton<CrossfadeTracker>
{
	public List<Crossfade> actives = new List<Crossfade>();
}
