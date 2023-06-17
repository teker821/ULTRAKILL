using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class DefaultReferenceManager : MonoSingleton<DefaultReferenceManager>
{
	public GameObject wetParticle;

	public GameObject sandDrip;

	public GameObject blessingGlow;

	public GameObject sandificationEffect;

	public GameObject enrageEffect;

	public GameObject ineffectiveSound;

	public GameObject continuousSplash;

	public GameObject projectile;

	public GameObject parryableFlash;

	public GameObject unparryableFlash;

	public GameObject explosion;

	public GameObject superExplosion;
}
