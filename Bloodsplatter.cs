using System.Collections.Generic;
using UnityEngine;

public class Bloodsplatter : MonoBehaviour
{
	[HideInInspector]
	public ParticleSystem part;

	public List<ParticleCollisionEvent> collisionEvents;

	public GameObject bloodStain;

	private GameObject bldstn;

	private int i;

	private AudioSource[] bloodStainAud;

	private AudioSource aud;

	public Sprite[] sprites;

	private SpriteRenderer sr;

	private MeshRenderer mr;

	private NewMovement nmov;

	public int hpAmount;

	private SphereCollider col;

	public bool hpOnParticleCollision;

	private OptionsManager opm;

	public bool halfChance;

	public bool ready;

	private GoreZone gz;

	public bool underwater;

	private MaterialPropertyBlock propertyBlock;

	private void Start()
	{
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		part = GetComponent<ParticleSystem>();
		collisionEvents = new List<ParticleCollisionEvent>();
		aud = GetComponent<AudioSource>();
		if (aud != null)
		{
			aud.pitch = Random.Range(0.75f, 1.5f);
			aud.Play();
		}
		col = GetComponent<SphereCollider>();
		if (underwater)
		{
			Invoke("DestroyCollider", 2.5f);
		}
		else
		{
			Invoke("DestroyCollider", 0.25f);
		}
		if (hpOnParticleCollision)
		{
			Invoke("DestroyIt", Random.Range(2.5f, 5f));
		}
		else if (underwater)
		{
			Invoke("DestroyIt", Random.Range(3, 5));
		}
		else
		{
			Invoke("DestroyIt", Random.Range(2.5f, 5f));
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBoolLocal("bloodEnabled"))
		{
			part.Play();
		}
	}

	private void OnParticleCollision(GameObject other)
	{
		if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Floor") || other.gameObject.CompareTag("Moving") || ((other.gameObject.CompareTag("Glass") || other.gameObject.CompareTag("GlassFloor")) && other.transform.childCount > 0))
		{
			part.GetCollisionEvents(other, collisionEvents);
			if (collisionEvents == null || collisionEvents.Count == 0)
			{
				return;
			}
			if (opm == null)
			{
				opm = MonoSingleton<OptionsManager>.Instance;
			}
			if ((halfChance || !((float)Random.Range(0, 100) < opm.bloodstainChance)) && (!halfChance || !((float)Random.Range(0, 100) < opm.bloodstainChance / 2f)))
			{
				return;
			}
			bldstn = Object.Instantiate(bloodStain, collisionEvents[0].intersection, base.transform.rotation, base.transform);
			bldstn.transform.forward = collisionEvents[0].normal * -1f;
			mr = bldstn.GetComponent<MeshRenderer>();
			mr.GetPropertyBlock(propertyBlock);
			propertyBlock.SetFloat("_Index", Random.Range(0, sprites.Length));
			mr.SetPropertyBlock(propertyBlock);
			bldstn.transform.Rotate(Vector3.forward * Random.Range(0, 359), Space.Self);
			if (other.gameObject.CompareTag("Moving") || ((bool)MonoSingleton<ComponentsDatabase>.Instance && MonoSingleton<ComponentsDatabase>.Instance.scrollers.Contains(other.transform)))
			{
				bldstn.transform.SetParent(other.transform, worldPositionStays: true);
				if (!gz)
				{
					gz = GoreZone.ResolveGoreZone(base.transform);
				}
				if (gz != null)
				{
					gz.outsideGore.Add(bldstn);
				}
				if ((bool)MonoSingleton<ComponentsDatabase>.Instance && MonoSingleton<ComponentsDatabase>.Instance.scrollers.Contains(other.transform) && other.transform.TryGetComponent<ScrollingTexture>(out var component))
				{
					component.attachedObjects.Add(bldstn.transform);
				}
			}
			else if (other.gameObject.CompareTag("Glass") || other.gameObject.CompareTag("GlassFloor"))
			{
				bldstn.transform.SetParent(other.transform, worldPositionStays: true);
			}
			else
			{
				bldstn.transform.SetParent(base.transform.parent, worldPositionStays: true);
			}
		}
		else if (ready && hpOnParticleCollision && other.gameObject.CompareTag("Player"))
		{
			MonoSingleton<NewMovement>.Instance.GetHealth(3, silent: false);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (ready && other.gameObject.CompareTag("Player"))
		{
			Object.Destroy(col);
			MonoSingleton<NewMovement>.Instance.GetHealth(hpAmount, silent: false);
		}
	}

	private void DestroyIt()
	{
		Object.Destroy(base.gameObject);
	}

	private void DestroyCollider()
	{
		if (col != null)
		{
			Object.Destroy(col);
		}
	}

	public void GetReady()
	{
		ready = true;
	}
}
