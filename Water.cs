using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Water : MonoBehaviour
{
	[HideInInspector]
	public List<Rigidbody> rbs = new List<Rigidbody>();

	private List<int> rbCalls = new List<int>();

	private List<Collider> contSplashables = new List<Collider>();

	private List<Rigidbody> rbsToRemove = new List<Rigidbody>();

	private List<Collider> enemiesToCheck = new List<Collider>();

	private List<int> enemyCalls = new List<int>();

	private List<Collider> enemiesToRemove = new List<Collider>();

	private List<GameObject> bubblesEffects = new List<GameObject>();

	public GameObject bubblesParticle;

	public GameObject splash;

	public GameObject smallSplash;

	private List<Collider> contSplashColliders = new List<Collider>();

	private List<GameObject> continuousSplashes = new List<GameObject>();

	private Collider[] colliders;

	public Color clr = new Color(0f, 0.5f, 1f);

	private bool inWater;

	private int waterRequests;

	public bool notWet;

	private UnderwaterController currentUwc;

	public List<Collider> enteredColliders = new List<Collider>();

	[Header("Optional, for fishing")]
	public FishDB fishDB;

	public Transform overrideFishingPoint;

	public FishObject[] attractFish;

	private void Start()
	{
		Invoke("SlowUpdate", 5f);
		colliders = GetComponentsInChildren<Collider>();
		MonoSingleton<DryZoneController>.Instance.waters.Add(this);
		if ((bool)fishDB)
		{
			fishDB.SetupWater(this);
		}
	}

	private void OnDestroy()
	{
		Shader.DisableKeyword("ISUNDERWATER");
		if (base.gameObject.scene.isLoaded)
		{
			if ((bool)MonoSingleton<UnderwaterController>.Instance && inWater)
			{
				MonoSingleton<UnderwaterController>.Instance.OutWater();
			}
			if (MonoSingleton<DryZoneController>.Instance.waters.Contains(this))
			{
				MonoSingleton<DryZoneController>.Instance.waters.Remove(this);
			}
		}
	}

	private void FixedUpdate()
	{
		foreach (Collider item in enemiesToCheck)
		{
			if (!item || !item.attachedRigidbody)
			{
				enemiesToRemove.Add(item);
				continue;
			}
			bool flag = false;
			for (int i = 0; i < colliders.Length; i++)
			{
				if (Vector3.Distance(colliders[i].ClosestPoint(item.transform.position), item.transform.position) < 1f)
				{
					Vector3 origin = new Vector3(item.transform.position.x, colliders[i].bounds.max.y + 0.1f, item.transform.position.z);
					if (Physics.Raycast(origin, Vector3.down, out var hitInfo, Mathf.Abs(origin.y - item.bounds.min.y), 16, QueryTriggerInteraction.Collide) && item.bounds.max.y - 1f < hitInfo.point.y)
					{
						flag = true;
						break;
					}
				}
			}
			Rigidbody attachedRigidbody = item.attachedRigidbody;
			if (flag && !rbs.Contains(attachedRigidbody))
			{
				AddRigidbody(attachedRigidbody, item);
				EnemyIdentifier component = attachedRigidbody.GetComponent<EnemyIdentifier>();
				if ((bool)component)
				{
					component.underwater = true;
				}
			}
			else
			{
				if (flag || !rbs.Contains(attachedRigidbody))
				{
					continue;
				}
				int index = rbs.IndexOf(attachedRigidbody);
				if (rbCalls[index] == 1)
				{
					RemoveRigidbody(attachedRigidbody, item);
					EnemyIdentifier component2 = attachedRigidbody.GetComponent<EnemyIdentifier>();
					if ((bool)component2)
					{
						component2.underwater = false;
					}
				}
			}
		}
		foreach (Collider item2 in enemiesToRemove)
		{
			int index2 = enemiesToCheck.IndexOf(item2);
			enemiesToCheck.Remove(item2);
			enemyCalls.RemoveAt(index2);
		}
		enemiesToRemove.Clear();
		foreach (Rigidbody rb in rbs)
		{
			if (!rb || !rb.gameObject.activeInHierarchy)
			{
				rbsToRemove.Add(rb);
			}
			else if (rb.useGravity && !rb.isKinematic)
			{
				if (rb.velocity.y < Physics.gravity.y * 0.2f)
				{
					rb.velocity = Vector3.MoveTowards(target: new Vector3(rb.velocity.x, Physics.gravity.y * 0.2f, rb.velocity.z), current: rb.velocity, maxDistanceDelta: Time.deltaTime * 10f * Mathf.Abs(rb.velocity.y - Physics.gravity.y * 0.2f + 0.5f));
				}
				else if (rb.gameObject.layer == 10 || rb.gameObject.layer == 9)
				{
					rb.AddForce(Physics.gravity * -0.45f * rb.mass);
				}
				else
				{
					rb.AddForce(Physics.gravity * -0.75f * rb.mass);
				}
			}
		}
		foreach (Rigidbody item3 in rbsToRemove)
		{
			int index3 = rbs.IndexOf(item3);
			rbs.Remove(item3);
			rbCalls.RemoveAt(index3);
		}
		rbsToRemove.Clear();
		if (contSplashables.Count > 0)
		{
			for (int num = contSplashables.Count - 1; num >= 0; num--)
			{
				if (contSplashables[num] == null)
				{
					contSplashables.RemoveAt(num);
				}
				else
				{
					Vector3 closestPointOnSurface = GetClosestPointOnSurface(contSplashables[num]);
					if (contSplashables[num].bounds.max.y > closestPointOnSurface.y && contSplashables[num].bounds.min.y < closestPointOnSurface.y)
					{
						if (!contSplashColliders.Contains(contSplashables[num]))
						{
							contSplashColliders.Add(contSplashables[num]);
							GameObject gameObject = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.continuousSplash, closestPointOnSurface, Quaternion.LookRotation(Vector3.up));
							gameObject.transform.localScale = Vector3.one * contSplashables[num].bounds.size.magnitude * 3f;
							if (enemiesToCheck.Contains(contSplashables[num]) && gameObject.TryGetComponent<SplashContinuous>(out var component3) && contSplashables[num].TryGetComponent<NavMeshAgent>(out var component4))
							{
								component3.nma = component4;
							}
							continuousSplashes.Add(gameObject);
						}
						else
						{
							continuousSplashes[contSplashColliders.IndexOf(contSplashables[num])].transform.position = closestPointOnSurface;
						}
					}
					else if (contSplashColliders.Contains(contSplashables[num]))
					{
						int index4 = contSplashColliders.IndexOf(contSplashables[num]);
						continuousSplashes[index4].SendMessage("DestroySoon");
						continuousSplashes.RemoveAt(index4);
						contSplashColliders.RemoveAt(index4);
					}
				}
			}
		}
		if (contSplashColliders.Count <= 0)
		{
			return;
		}
		for (int num2 = contSplashColliders.Count - 1; num2 >= 0; num2--)
		{
			if (contSplashColliders[num2] == null || !contSplashables.Contains(contSplashColliders[num2]))
			{
				continuousSplashes[num2].SendMessage("DestroySoon");
				continuousSplashes.RemoveAt(num2);
				contSplashColliders.RemoveAt(num2);
			}
		}
	}

	private void SlowUpdate()
	{
		if (bubblesEffects.Count > 0)
		{
			bubblesEffects.RemoveAll((GameObject GameObject) => GameObject == null);
		}
		if (enteredColliders.Count > 0)
		{
			enteredColliders.RemoveAll((Collider Collider) => Collider == null);
		}
		Invoke("SlowUpdate", Random.Range(0.5f, 1f));
	}

	private void OnDisable()
	{
		foreach (GameObject bubblesEffect in bubblesEffects)
		{
			if ((bool)bubblesEffect)
			{
				Object.Destroy(bubblesEffect);
			}
		}
		bubblesEffects.Clear();
		for (int i = 0; i < rbs.Count; i++)
		{
			if (rbs[i] != null && (bool)rbs[i].GetComponentInChildren<Collider>())
			{
				RemoveRigidbody(rbs[i], rbs[i].GetComponentInChildren<Collider>());
			}
		}
		rbs.Clear();
		contSplashables.Clear();
		rbCalls.Clear();
		enemiesToCheck.Clear();
		enemyCalls.Clear();
		foreach (GameObject continuousSplash in continuousSplashes)
		{
			Object.Destroy(continuousSplash);
		}
		continuousSplashes.Clear();
		contSplashables.Clear();
		contSplashColliders.Clear();
		if (inWater)
		{
			inWater = false;
			waterRequests = 0;
			Shader.DisableKeyword("ISUNDERWATER");
			if (MonoSingleton<UnderwaterController>.Instance != null)
			{
				MonoSingleton<UnderwaterController>.Instance.OutWater();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (MonoSingleton<DryZoneController>.Instance.colliders.Count == 0 || !MonoSingleton<DryZoneController>.Instance.colliders.Contains(other))
		{
			Enter(other);
		}
		enteredColliders.Add(other);
	}

	private void Enter(Collider other)
	{
		if (other.gameObject.name == "CameraCollisionChecker")
		{
			if (currentUwc != null && MonoSingleton<UnderwaterController>.Instance != currentUwc)
			{
				waterRequests = 0;
			}
			Shader.EnableKeyword("ISUNDERWATER");
			inWater = true;
			GameObject gameObject = Object.Instantiate(bubblesParticle, MonoSingleton<PlayerTracker>.Instance.GetPlayer());
			gameObject.transform.forward = Vector3.up;
			bubblesEffects.Add(gameObject);
			waterRequests++;
			MonoSingleton<UnderwaterController>.Instance.InWater(clr);
			currentUwc = MonoSingleton<UnderwaterController>.Instance;
		}
		if (other.isTrigger)
		{
			return;
		}
		Rigidbody component = other.GetComponent<Rigidbody>();
		if (!component)
		{
			return;
		}
		if (other.gameObject.layer == 12 && !rbs.Contains(component))
		{
			Collider component2 = other.gameObject.GetComponent<Collider>();
			if ((bool)component2 && !enemiesToCheck.Contains(component2))
			{
				enemiesToCheck.Add(component2);
				enemyCalls.Add(1);
				contSplashables.Add(component2);
			}
			else if ((bool)component2)
			{
				enemyCalls[enemiesToCheck.IndexOf(component2)]++;
			}
		}
		else if (!rbs.Contains(component))
		{
			AddRigidbody(component, other);
		}
		else
		{
			rbCalls[rbs.IndexOf(component)]++;
		}
	}

	private void AddRigidbody(Rigidbody rb, Collider other)
	{
		rbs.Add(rb);
		rbCalls.Add(1);
		Vector3 position = new Vector3(other.transform.position.x, colliders[0].bounds.max.y, other.transform.position.z);
		Vector3 vector = colliders[0].ClosestPoint(position);
		if (colliders.Length > 1)
		{
			for (int i = 1; i < colliders.Length; i++)
			{
				Vector3 position2 = new Vector3(other.transform.position.x, colliders[i].bounds.max.y, other.transform.position.z);
				Vector3 vector2 = colliders[i].ClosestPoint(position2);
				if (Vector3.Distance(vector2, other.transform.position) < Vector3.Distance(vector, other.transform.position))
				{
					vector = vector2;
				}
			}
		}
		GameObject gameObject = null;
		if (Vector3.Distance(vector, other.ClosestPoint(vector)) < 1f && rb != null)
		{
			if ((rb.velocity.y < -25f || other.gameObject.layer == 11) && rb.mass >= 1f && other.gameObject.layer != 10 && other.gameObject.layer != 9)
			{
				gameObject = Object.Instantiate(splash, vector, Quaternion.LookRotation(Vector3.up));
			}
			else if (!rb.isKinematic)
			{
				gameObject = Object.Instantiate(smallSplash, vector, Quaternion.LookRotation(Vector3.up));
			}
			if ((bool)gameObject)
			{
				gameObject.transform.localScale = Vector3.one * other.bounds.size.magnitude * 3f;
			}
		}
		if (other.gameObject.tag == "Player")
		{
			if ((bool)gameObject)
			{
				gameObject.GetComponent<RandomPitch>().defaultPitch = 0.45f;
			}
			contSplashables.Add(other);
			return;
		}
		GameObject gameObject2 = Object.Instantiate(bubblesParticle, other.transform);
		gameObject2.transform.forward = Vector3.up;
		bubblesEffects.Add(gameObject2);
		AudioSource[] array = ((other.gameObject.layer != 0 || !(other.gameObject.name == "UnderwaterChecker")) ? other.GetComponentsInChildren<AudioSource>() : other.transform.parent.GetComponentsInChildren<AudioSource>());
		AudioSource[] array2 = array;
		foreach (AudioSource audioSource in array2)
		{
			AudioLowPassFilter audioLowPassFilter = audioSource.GetComponent<AudioLowPassFilter>();
			if (!audioLowPassFilter)
			{
				audioLowPassFilter = audioSource.gameObject.AddComponent<AudioLowPassFilter>();
			}
			audioLowPassFilter.cutoffFrequency = 1000f;
			audioLowPassFilter.lowpassResonanceQ = 1f;
		}
		if (notWet)
		{
			return;
		}
		Flammable[] componentsInChildren = other.GetComponentsInChildren<Flammable>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			componentsInChildren[j].PutOut();
		}
		if (other.gameObject.layer != 10 && other.gameObject.layer != 9)
		{
			Wet component = other.GetComponent<Wet>();
			if (!component)
			{
				other.gameObject.AddComponent<Wet>();
			}
			else
			{
				component.Refill();
			}
		}
		if (other.gameObject.layer == 12)
		{
			EnemyIdentifier component2 = other.gameObject.GetComponent<EnemyIdentifier>();
			if ((bool)component2 && component2.enemyType == EnemyType.Streetcleaner && !component2.dead)
			{
				component2.InstaKill();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (MonoSingleton<DryZoneController>.Instance.colliders.Count == 0 || !MonoSingleton<DryZoneController>.Instance.colliders.Contains(other))
		{
			Exit(other);
		}
		if (enteredColliders.Contains(other))
		{
			enteredColliders.Remove(other);
		}
	}

	public void Exit(Collider other)
	{
		if (other.gameObject.name == "CameraCollisionChecker")
		{
			Shader.DisableKeyword("ISUNDERWATER");
			waterRequests--;
			if (waterRequests <= 0)
			{
				waterRequests = 0;
				inWater = false;
			}
			MonoSingleton<UnderwaterController>.Instance.OutWater();
		}
		Rigidbody component = other.GetComponent<Rigidbody>();
		if (!component)
		{
			return;
		}
		if (other.gameObject.layer == 12)
		{
			if (!enemiesToCheck.Contains(other))
			{
				return;
			}
			int index = enemiesToCheck.IndexOf(other);
			if (enemyCalls[index] > 1)
			{
				enemyCalls[index]--;
			}
			else if (enemiesToCheck.Contains(other))
			{
				enemiesToCheck.Remove(other);
				enemyCalls.RemoveAt(index);
				if (contSplashables.Contains(other))
				{
					contSplashables.Remove(other);
				}
				if (rbs.Contains(component))
				{
					int index2 = rbs.IndexOf(component);
					rbCalls[index2] = 1;
					RemoveRigidbody(component, other);
				}
			}
		}
		else if (rbs.Contains(component))
		{
			RemoveRigidbody(component, other);
		}
	}

	private void RemoveRigidbody(Rigidbody rb, Collider other)
	{
		int index = rbs.IndexOf(rb);
		if (rbCalls[index] > 1)
		{
			rbCalls[index]--;
			return;
		}
		rbs.RemoveAt(index);
		rbCalls.RemoveAt(index);
		if ((bool)other)
		{
			ParticleSystem[] componentsInChildren = other.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particleSystem in componentsInChildren)
			{
				if (bubblesEffects.Contains(particleSystem.gameObject))
				{
					bubblesEffects.Remove(particleSystem.gameObject);
					Object.Destroy(particleSystem.gameObject);
				}
			}
		}
		if ((bool)other && other.gameObject.tag == "Player")
		{
			contSplashables.Remove(other);
			return;
		}
		AudioSource[] array = null;
		if ((bool)other)
		{
			array = ((other.gameObject.layer != 0 || !(other.gameObject.name == "UnderwaterChecker")) ? other.GetComponentsInChildren<AudioSource>() : other.transform.parent.GetComponentsInChildren<AudioSource>());
		}
		else if ((bool)rb)
		{
			array = rb.GetComponentsInChildren<AudioSource>();
		}
		AudioSource[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			AudioLowPassFilter component = array2[i].GetComponent<AudioLowPassFilter>();
			if ((bool)component)
			{
				Object.Destroy(component);
			}
		}
		if ((bool)other)
		{
			Vector3 position = new Vector3(other.transform.position.x, colliders[0].bounds.max.y, other.transform.position.z);
			Vector3 vector = colliders[0].ClosestPointOnBounds(position);
			if (colliders.Length > 1)
			{
				for (int j = 1; j < colliders.Length; j++)
				{
					Vector3 position2 = new Vector3(other.transform.position.x, colliders[j].bounds.max.y, other.transform.position.z);
					Vector3 vector2 = colliders[j].ClosestPointOnBounds(position2);
					if (Vector3.Distance(vector2, other.transform.position) < Vector3.Distance(vector, other.transform.position))
					{
						vector = vector2;
					}
				}
			}
			if (Vector3.Distance(vector, other.ClosestPoint(vector)) < 1f)
			{
				if (rb.velocity.y > 25f && rb.mass >= 1f && base.gameObject.layer != 10)
				{
					Object.Instantiate(splash, vector + Vector3.up * 0.5f, Quaternion.LookRotation(Vector3.up));
				}
				else if (rb.velocity.y > 10f)
				{
					Object.Instantiate(smallSplash, vector + Vector3.up * 0.5f, Quaternion.LookRotation(Vector3.up));
				}
			}
			if (other.gameObject.layer != 10 && other.gameObject.layer != 9 && !notWet)
			{
				Wet component2 = other.GetComponent<Wet>();
				if (!component2)
				{
					other.gameObject.AddComponent<Wet>();
				}
				else
				{
					component2.Dry();
				}
			}
		}
		else if ((bool)rb && !notWet && rb.gameObject.layer != 10 && rb.gameObject.layer != 9)
		{
			Wet component3 = rb.GetComponent<Wet>();
			if (!component3)
			{
				rb.gameObject.AddComponent<Wet>();
			}
			else
			{
				component3.Dry();
			}
		}
	}

	public void EnterDryZone(Collider other)
	{
		if (enteredColliders.Contains(other))
		{
			Exit(other);
		}
	}

	public void ExitDryZone(Collider other)
	{
		if (enteredColliders.Contains(other))
		{
			Enter(other);
		}
	}

	private Vector3 GetClosestPointOnSurface(Collider target)
	{
		for (int i = 0; i < colliders.Length; i++)
		{
			if (Vector3.Distance(colliders[i].ClosestPoint(target.transform.position), target.transform.position) < 1f)
			{
				Vector3 origin = new Vector3(target.transform.position.x, colliders[i].bounds.max.y + 0.1f, target.transform.position.z);
				if (Physics.Raycast(origin, Vector3.down, out var hitInfo, Mathf.Abs(origin.y - target.bounds.min.y), 16, QueryTriggerInteraction.Collide))
				{
					return hitInfo.point;
				}
			}
		}
		return Vector3.one * 9999f;
	}
}
