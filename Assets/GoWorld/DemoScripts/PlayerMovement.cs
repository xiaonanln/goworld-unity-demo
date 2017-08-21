using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float speed = 6f; 
	Vector3 movement;
	Animator anim;
	Rigidbody playerRigidbody;
	int floorMask;
	float camRayLength = 100f;
	float syncEntityInfoAccumTime = 0;
	Player selfEntity;
	bool isWalking;


	void Awake() {
		floorMask = LayerMask.GetMask ("Floor");
		anim = GetComponent<Animator> ();
		playerRigidbody = GetComponent<Rigidbody> ();
		selfEntity = this.GetComponent<Player> ();
	}

	void FixedUpdate() {
		float h = Input.GetAxisRaw ("Horizontal");
		float v = Input.GetAxisRaw ("Vertical");

		Move (h, v);
		Turning();
		ChangeAction(h, v);
	}

	void Move(float h, float v) {
		movement.Set (h, 0f, v);
		movement = movement.normalized * speed * Time.fixedDeltaTime;
		playerRigidbody.MovePosition (transform.position + movement);

		syncEntityInfoAccumTime += Time.fixedDeltaTime;
		if (syncEntityInfoAccumTime > 0.1) {
			syncEntityInfoAccumTime = 0;
			//Debug.Log ("Move Syncing to server");
			selfEntity.SyncEntityInfoToServer ();
		}
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
			selfEntity.CallServer ("SetAction", "walk");
		} else if (this.isWalking && !walking){
			this.isWalking = walking;
			selfEntity.CallServer("SetAction", "idle");
		}
	}
}
