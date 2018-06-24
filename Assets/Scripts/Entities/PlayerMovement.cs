using UnityEngine;
using GoWorldUnity3D;

public class PlayerMovement : MonoBehaviour
{
	public float speed = 6f; 
	Vector3 movement;
	//Animator anim;
	Rigidbody playerRigidbody;
	int floorMask;
	float camRayLength = 100f;
	bool isWalking;


	void Start() {
		floorMask = LayerMask.GetMask ("Floor");
		//anim = GetComponent<Animator> ();
		playerRigidbody = GetComponent<Rigidbody> ();
	}

	void FixedUpdate() {
		float h = Input.GetAxisRaw ("Horizontal");
		float v = Input.GetAxisRaw ("Vertical");

        if (GetComponent<ClientEntity>().Attrs.GetInt("hp") <= 0) {
			return;
		}

		Move (h, v);
		Turning();
		ChangeAction(h, v);
	}

	void Move(float h, float v) {
		movement.Set (h, 0f, v);
		movement = movement.normalized * speed * Time.fixedDeltaTime;
		playerRigidbody.MovePosition (transform.position + movement);
	}

	void Turning() {
		Ray camRay = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit floorHit;
		if (Physics.Raycast (camRay, out floorHit, camRayLength, floorMask)) {
			Vector3 playerToMouse = floorHit.point - transform.position;
			playerToMouse.y = 0f;
			Quaternion newRotation = Quaternion.LookRotation (playerToMouse);
			playerRigidbody.MoveRotation (newRotation);
		}
	}

	void ChangeAction(float h, float v) {
		bool walking = h != 0f || v != 0f;
		if (!this.isWalking && walking) {
			this.isWalking = walking;
            GetComponent<ClientEntity>().CallServer ("SetAction", "move");
		} else if (this.isWalking && !walking){
			this.isWalking = walking;
            GetComponent<ClientEntity>().CallServer("SetAction", "idle");
		}
	}
}
