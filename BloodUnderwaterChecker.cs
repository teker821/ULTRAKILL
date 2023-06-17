using UnityEngine;

public class BloodUnderwaterChecker : MonoBehaviour
{
	private bool cancelled;

	private void OnTriggerEnter(Collider other)
	{
		if (cancelled || other.gameObject.layer != 4)
		{
			return;
		}
		Vector3 vector = new Vector3(base.transform.position.x, base.transform.position.y + 1.5f, base.transform.position.z);
		if (!(Vector3.Distance(other.ClosestPointOnBounds(vector), vector) < 0.5f))
		{
			return;
		}
		if (MonoSingleton<DryZoneController>.Instance.dryZones != null && MonoSingleton<DryZoneController>.Instance.dryZones.Count > 0)
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, 0.01f, 65536, QueryTriggerInteraction.Collide);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].TryGetComponent<DryZone>(out var _))
				{
					base.gameObject.SetActive(value: false);
					cancelled = true;
					return;
				}
			}
		}
		GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Body, isUnderwater: true);
		if ((bool)gore)
		{
			Bloodsplatter component2 = base.transform.parent.GetComponent<Bloodsplatter>();
			Bloodsplatter component3 = gore.GetComponent<Bloodsplatter>();
			component3.hpAmount = component2.hpAmount;
			if (component2.ready)
			{
				component3.GetReady();
			}
			gore.transform.position = base.transform.position;
			GoreZone componentInParent = GetComponentInParent<GoreZone>();
			if (componentInParent != null && componentInParent.goreZone != null)
			{
				gore.transform.SetParent(componentInParent.goreZone, worldPositionStays: true);
			}
			_ = component2.GetComponent<ParticleSystem>().collision;
			gore.SetActive(value: true);
			base.transform.parent.gameObject.SetActive(value: false);
		}
	}
}
