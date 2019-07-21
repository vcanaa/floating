using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour {
    public Transform follow;
    public Vector3 direction;
    public float distance;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 target = new Vector3(follow.position.x, 0, follow.position.z) + direction.normalized * distance;
        transform.position = Vector3.Lerp(transform.position, target, 0.99f);
        transform.rotation = Quaternion.LookRotation(follow.position - transform.position);
	}
}
