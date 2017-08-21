using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public int damagePerShot = 20;
    public float timeBetweenBullets = 0.15f;
    public float range = 100f;


    float timer;
    Ray shootRay = new Ray();
    RaycastHit shootHit;
    int shootableMask;
    ParticleSystem gunParticles;
    LineRenderer gunLine;
    AudioSource gunAudio;
    Light gunLight;
    float effectsDisplayTime = 0.2f;
	ClientEntity self;
	GameObject gunBarrelEnd;


    void Awake ()
    {
        shootableMask = LayerMask.GetMask ("Shootable");

		gunBarrelEnd = this.transform.Find ("GunBarrelEnd").gameObject;

		gunParticles = gunBarrelEnd.GetComponent<ParticleSystem> ();
		gunLine = gunBarrelEnd.GetComponent <LineRenderer> ();
		gunAudio = gunBarrelEnd.GetComponent<AudioSource> ();
		gunLight = gunBarrelEnd.GetComponent<Light> ();

		self = GetComponent<Player> ();
    }


    void Update ()
    {
		if (self.IsPlayer) {
			timer += Time.deltaTime;

			if(Input.GetButton ("Fire1") && timer >= timeBetweenBullets && Time.timeScale != 0)
			{
				Shoot ();
			}
		}

        if(timer >= timeBetweenBullets * effectsDisplayTime)
        {
            DisableEffects ();
        }
    }


    public void DisableEffects ()
    {
        gunLine.enabled = false;
        gunLight.enabled = false;
    }


    void Shoot ()
    {
        timer = 0f;

        gunAudio.Play ();

        gunLight.enabled = true;

        gunParticles.Stop ();
        gunParticles.Play ();

        gunLine.enabled = true;
		gunLine.SetPosition (0, gunBarrelEnd.transform.position);

		shootRay.origin = gunBarrelEnd.transform.position;
        shootRay.direction = transform.forward;

        if(Physics.Raycast (shootRay, out shootHit, range, shootableMask))
        {
			MonsterHealth enemyHealth = shootHit.collider.GetComponent <MonsterHealth> ();
            if(enemyHealth != null)
            {
				Debug.Log ("Hit enemy!!!");
                enemyHealth.TakeDamage (damagePerShot, shootHit.point);
            }
            gunLine.SetPosition (1, shootHit.point);
        }
        else
        {
            gunLine.SetPosition (1, shootRay.origin + shootRay.direction * range);
        }
    }
}
