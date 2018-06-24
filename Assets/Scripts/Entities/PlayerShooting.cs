using UnityEngine;
using GoWorldUnity3D;

public class PlayerShooting : MonoBehaviour
{
    public int damagePerShot = 20;
    public float timeBetweenBullets = 0.15f;
    public float range = 100f;


	float shootDisplayTime;
	float sendShootCD;
    Ray shootRay = new Ray();
    RaycastHit shootHit;
    int shootableMask;
    ParticleSystem gunParticles;
    LineRenderer gunLine;
    AudioSource gunAudio;
    Light gunLight;
    float effectsDisplayTime = 0.2f;
	GameObject gunBarrelEnd;


    void Awake ()
    {
        shootableMask = LayerMask.GetMask ("Shootable");

		gunBarrelEnd = this.transform.Find ("GunBarrelEnd").gameObject;

		gunParticles = gunBarrelEnd.GetComponent<ParticleSystem> ();
		gunLine = gunBarrelEnd.GetComponent <LineRenderer> ();
		gunAudio = gunBarrelEnd.GetComponent<AudioSource> ();
		gunLight = gunBarrelEnd.GetComponent<Light> ();
    }

    void Update ()
    {
		if (GetComponent<ClientEntity>().IsClientOwner && GetComponent<ClientEntity>().Attrs.GetInt("hp") > 0) {
			sendShootCD += Time.deltaTime;

			if(Input.GetButton ("Fire1") && sendShootCD >= timeBetweenBullets && Time.timeScale != 0)
			{
				sendShootCD = 0;
				SendShoot ();
			}
		}

		if(Time.time >= shootDisplayTime + timeBetweenBullets * effectsDisplayTime)
        {
            DisableEffects ();
        }
    }


    public void DisableEffects ()
    {
        gunLine.enabled = false;
        gunLight.enabled = false;
    }

	void SendShoot ()
	{
		shootRay.origin = gunBarrelEnd.transform.position;
		shootRay.direction = transform.forward;

		if(Physics.Raycast (shootRay, out shootHit, range, shootableMask))
		{
			ClientEntity enemyEntity = shootHit.collider.GetComponent <ClientEntity> ();
			if (enemyEntity != null) {
                GetComponent<ClientEntity>().CallServer ("ShootHit", enemyEntity.ID);
			} else {
                GetComponent<ClientEntity>().CallServer ("ShootMiss");
			}
		}
		else
		{
            GetComponent<ClientEntity>().CallServer ("ShootMiss");
		}
	}

    internal void Shoot ()
    {
		shootDisplayTime = Time.time;

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
            gunLine.SetPosition (1, shootHit.point);
        }
        else
        {
            gunLine.SetPosition (1, shootRay.origin + shootRay.direction * range);
        }
    }
}
