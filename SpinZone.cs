using UnityEngine;

public class SpinZone : MonoBehaviour
{
	public GameObject spinSound;

	public bool dontSpinEnemies;

	private bool interactedWithItem;

	private void OnDisable()
	{
		interactedWithItem = false;
	}

	private void OnEnable()
	{
		if ((bool)MonoSingleton<FistControl>.Instance.heldObject)
		{
			MonoSingleton<FistControl>.Instance.currentPunch.ForceThrow();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		EnemyIdentifier component2;
		Breakable component3;
		Glass component4;
		if (other.gameObject.layer == 10 || other.gameObject.layer == 11)
		{
			if (other.TryGetComponent<EnemyIdentifierIdentifier>(out var component) && (bool)component.eid && !component.eid.dead)
			{
				SpinEnemy(component.eid);
			}
		}
		else if ((other.gameObject.layer == 12 || other.gameObject.CompareTag("Armor")) && other.TryGetComponent<EnemyIdentifier>(out component2) && !component2.dead)
		{
			SpinEnemy(component2);
		}
		else if (other.TryGetComponent<Breakable>(out component3) && !component3.precisionOnly)
		{
			component3.Break();
		}
		else if (other.TryGetComponent<Glass>(out component4))
		{
			component4.Shatter();
		}
		else
		{
			if (other.gameObject.layer != 22 || interactedWithItem)
			{
				return;
			}
			ItemIdentifier component5 = other.GetComponent<ItemIdentifier>();
			ItemPlaceZone[] components = other.GetComponents<ItemPlaceZone>();
			if ((bool)MonoSingleton<FistControl>.Instance.heldObject && components != null && components.Length != 0)
			{
				interactedWithItem = true;
				MonoSingleton<FistControl>.Instance.heldObject.SendMessage("PutDown", SendMessageOptions.DontRequireReceiver);
				MonoSingleton<FistControl>.Instance.currentPunch.PlaceHeldObject(components, other.transform);
			}
			else if (!MonoSingleton<FistControl>.Instance.heldObject && component5 != null)
			{
				interactedWithItem = true;
				component5.SendMessage("PickUp", SendMessageOptions.DontRequireReceiver);
				MonoSingleton<FistControl>.Instance.currentPunch.ForceHold(component5);
				if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
				{
					MonoSingleton<PlatformerMovement>.Instance.CheckItem();
				}
			}
			else if ((bool)MonoSingleton<FistControl>.Instance.heldObject)
			{
				MonoSingleton<FistControl>.Instance.heldObject.SendMessage("HitWith", other.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void SpinEnemy(EnemyIdentifier eid)
	{
		if (eid.blessed)
		{
			return;
		}
		if (dontSpinEnemies)
		{
			eid.Explode();
			return;
		}
		GameObject gameObject = new GameObject();
		gameObject.transform.position = eid.transform.position;
		gameObject.transform.SetParent(GoreZone.ResolveGoreZone(eid.transform).gibZone);
		gameObject.gameObject.layer = 1;
		GameObject gameObject2 = Object.Instantiate(eid.gameObject, eid.transform.position, eid.transform.rotation);
		gameObject2.transform.localScale = eid.transform.lossyScale;
		gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: true);
		eid.gameObject.SetActive(value: false);
		eid.hitter = "spin";
		eid.InstaKill();
		Object.Destroy(eid.gameObject);
		SandboxUtils.StripForPreview(gameObject2.transform);
		Object.Instantiate(spinSound, base.transform.position + (gameObject2.transform.position - base.transform.position).normalized, Quaternion.identity, gameObject2.transform);
		Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
		rigidbody.velocity = (new Vector3(gameObject.transform.position.x, base.transform.position.y, gameObject.transform.position.z) - base.transform.position).normalized * 250f;
		rigidbody.useGravity = false;
		if (gameObject2.TryGetComponent<Collider>(out var component))
		{
			BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
			boxCollider.size = new Vector3(component.bounds.size.x, component.bounds.size.y, component.bounds.size.z);
			boxCollider.isTrigger = true;
			gameObject.AddComponent<SpinZone>().dontSpinEnemies = true;
		}
		gameObject.AddComponent<RemoveOnTime>().time = 3f;
	}
}
