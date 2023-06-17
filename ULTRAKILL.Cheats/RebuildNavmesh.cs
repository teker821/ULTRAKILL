using System;
using System.Collections;
using UnityEngine.Events;

namespace ULTRAKILL.Cheats;

public class RebuildNavmesh : ICheat
{
	private bool active;

	public string LongName => "Enemy Navigation";

	public string Identifier => "ultrakill.sandbox.rebuild-nav";

	public string ButtonEnabledOverride => "REBUILDING...";

	public string ButtonDisabledOverride => "REBUILD";

	public string Icon => "navmesh";

	public bool IsActive => active;

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public void Enable()
	{
		active = true;
		MonoSingleton<CheatsManager>.Instance.StartCoroutine(RebuildDelayed());
	}

	private IEnumerator RebuildDelayed()
	{
		yield return null;
		SandboxNavmesh instance = MonoSingleton<SandboxNavmesh>.Instance;
		instance.navmeshBuilt = (UnityAction)Delegate.Combine(instance.navmeshBuilt, new UnityAction(NavmeshBuilt));
		MonoSingleton<SandboxNavmesh>.Instance.Rebake();
	}

	private void NavmeshBuilt()
	{
		SandboxNavmesh instance = MonoSingleton<SandboxNavmesh>.Instance;
		instance.navmeshBuilt = (UnityAction)Delegate.Remove(instance.navmeshBuilt, new UnityAction(NavmeshBuilt));
		active = false;
		MonoSingleton<CheatsManager>.Instance.UpdateCheatState(this);
	}

	public void Disable()
	{
	}

	public void Update()
	{
	}
}
