using System.Collections.Generic;
using UnityEngine;

public class ComponentsDatabase : MonoSingleton<ComponentsDatabase>
{
	public List<Transform> scrollers = new List<Transform>();
}
