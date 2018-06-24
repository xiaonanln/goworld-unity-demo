using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Transform target; 
	public float smoothing = 5f;
    private Vector3 cameraOffset;

    private void Awake()
    {
        cameraOffset = this.transform.position - target.transform.position;
        GameObject.Destroy(target.gameObject);
        target = null;
    }

    void FixedUpdate() {
		if (!target) {
			return;
		}

        Vector3 targetCamPos = target.position + cameraOffset;
		transform.position = Vector3.Lerp (transform.position, targetCamPos, smoothing * Time.deltaTime);
	}
}
