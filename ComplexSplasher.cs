using System.Collections.Generic;
using UnityEngine;

public class ComplexSplasher : MonoBehaviour
{
	[SerializeField]
	private ParticleCluster splashParticles;

	[SerializeField]
	private float maxSplashDistance = 80f;

	[SerializeField]
	private float keepAliveFor = 3f;

	private List<SplashingElement> children;

	private Dictionary<ParticleCluster, TimeSince> currentSplashes;

	private int splashElementIndex;

	private void Awake()
	{
		children = new List<SplashingElement>();
		currentSplashes = new Dictionary<ParticleCluster, TimeSince>();
		SplashingElement previousElement = null;
		SplashingElement[] componentsInChildren = GetComponentsInChildren<SplashingElement>();
		foreach (SplashingElement splashingElement in componentsInChildren)
		{
			children.Add(splashingElement);
			splashingElement.previousElement = previousElement;
			previousElement = splashingElement;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (children != null)
		{
			Gizmos.color = Color.red;
			for (int i = 0; i < children.Count && i + 1 < children.Count; i++)
			{
				SplashingElement splashingElement = children[i];
				SplashingElement splashingElement2 = children[i + 1];
				Gizmos.color = ((children[i].isSplashing || children[i + 1].isSplashing) ? Color.green : Color.red);
				Gizmos.DrawLine(splashingElement.transform.position, splashingElement2.transform.position);
			}
		}
	}

	private void FixedUpdate()
	{
		List<ParticleCluster> list = new List<ParticleCluster>();
		foreach (KeyValuePair<ParticleCluster, TimeSince> currentSplash in currentSplashes)
		{
			if ((float)currentSplash.Value > keepAliveFor)
			{
				list.Add(currentSplash.Key);
			}
			currentSplash.Key.EmissionOff();
		}
		list.ForEach(delegate(ParticleCluster x)
		{
			Object.Destroy(x.gameObject);
			currentSplashes.Remove(x);
		});
		foreach (SplashingElement child in children)
		{
			if (!child.isSplashing)
			{
				continue;
			}
			ParticleCluster particleCluster = null;
			foreach (KeyValuePair<ParticleCluster, TimeSince> currentSplash2 in currentSplashes)
			{
				if (Vector3.Distance(currentSplash2.Key.transform.position, child.splashPosition) <= maxSplashDistance)
				{
					particleCluster = currentSplash2.Key;
					currentSplashes[particleCluster] = 0f;
					break;
				}
			}
			if (particleCluster == null)
			{
				particleCluster = Object.Instantiate(splashParticles);
				particleCluster.transform.SetParent(GoreZone.ResolveGoreZone(base.transform).transform);
				currentSplashes.Add(particleCluster, 0f);
			}
			particleCluster.EmissionOn();
			particleCluster.transform.position = child.splashPosition + Vector3.up * 3f;
		}
	}
}
