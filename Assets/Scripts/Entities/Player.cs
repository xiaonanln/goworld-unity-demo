using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GoWorldUnity3D;
using System.Threading;

public class Player : GameEntity {

	Animator anim;
	PlayerShooting playerShooting;

    protected override void OnCreated() {
        GameObject.DontDestroyOnLoad(this.gameObject);

        anim = gameObject.GetComponent<Animator> ();
		playerShooting = gameObject.GetComponent<PlayerShooting> ();
		Debug.Log ("Player is created");
	}

    protected override void OnDestroy() {
		Debug.Log ("Player is destroyed");
	}

	protected override void OnEnterSpace() {
		if (this.IsClientOwner)
        {
            SceneManager.LoadScene("Level 01", LoadSceneMode.Single);
            // UnityEngine.SceneManagement.SceneManager.LoadScene("Level 01", LoadSceneMode.Single);
        }
        gameObject.GetComponent<PlayerMovement> ().enabled = this.IsClientOwner;

		string action = this.Attrs.GetStr("action");
        anim.SetTrigger (action);
	}

	public void OnAttrChange_action() {
        string action = this.Attrs.GetStr("action");
		Debug.Log (this.ToString() + "'s action is changed to " + action); 
		anim.SetTrigger (action);
	}

	public void Shoot() {
		playerShooting.Shoot ();
	}

    protected override void OnBecomeClientOwner()
    {

    }

    protected override void OnLeaveSpace()
    {
        throw new System.NotImplementedException();
    }

    void Update()
    {
        // GoWorldUnity3D.Logger.Debug("Player", "Player Update. ..");

        if (this.IsClientOwner)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == "Level 01")
            {
                GameObject camera = GameObject.Find("Main Camera");
                camera.GetComponent<CameraFollow>().target = gameObject.transform;
            }
        }
    }

    public static new GameObject CreateGameObject(MapAttr attrs)
    {
        return GameObject.Instantiate(GameObject.Find("GoWorldController").GetComponent<GoWorldController>().Player);
    }
}
