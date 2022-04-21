using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    Transform _followTarget;

	[SerializeField]
	Vector3 _followOffset = new Vector3(0,6,-4);

	private void Update() {
		transform.position = _followTarget.position + _followOffset;
	}
}
