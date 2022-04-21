using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{

    [SerializeField, Tooltip("Maximum movespeed of the character.")]
    float _maxSpeed = 5f;
    [SerializeField]
    float _movementAcceleration = 10f;
    [SerializeField]
    float _movementDeceleration = 20f;

    //[SerializeField, Tooltip("Maximum turning speed of the character.")]
    //float _turnSpeed = 5f;

    [SerializeField]
    LayerMask _obstacleLayers;

    CapsuleCollider _characterCollider;


    [Header("DEBUG")]
    [SerializeField]
    float _currentSpeed = 0f;

    [SerializeField]
    bool _debugVisuals = false;

	private void Awake() {
        _characterCollider = GetComponent<CapsuleCollider>();
	}


	void Update() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(horizontal, 0, vertical);
        // clamp input so diagonal walking is not faster
        //input = input.normalized;

		if (_debugVisuals) {
            Debug.DrawRay(transform.position + new Vector3(0,1.5f,0), input, Color.grey);
		}

        ///
        // Open questions:
        // is input binary, or analogoue? => can we walk slow if only 0.5 of forward is pressed?
        //  A: flowchart says accelerate when forward until max speed
        ///



        if(input.magnitude > 0f) {
            // Acceleration
            _currentSpeed += _movementAcceleration * Time.deltaTime;
		} else {
            // Deceleration
            _currentSpeed -= _movementDeceleration * Time.deltaTime;
		}

        _currentSpeed = Mathf.Clamp(_currentSpeed, 0f, _maxSpeed);

        // Turning
        transform.LookAt(transform.position + input * 5f);


        // obstacles
        Vector3 desiredMovement = _currentSpeed * Time.deltaTime * transform.forward;
    	if (_debugVisuals) {
            Debug.DrawRay(transform.position + new Vector3(0,2f,0), desiredMovement, Color.magenta);
		}
        desiredMovement = HandleObstacles(desiredMovement);


        transform.position += desiredMovement;
    }

    /// <summary>
    /// Detects obstacles in the way and returns the maximum possible movement without collision.
    /// </summary>
    /// <param name="desiredMovement"></param>
    /// <returns></returns>
	private Vector3 HandleObstacles(Vector3 desiredMovement) {
        Ray moveDirection = new Ray(transform.position + new Vector3(0, 1, 0), desiredMovement);
        Vector3 forwardPointOnCharacterHull = transform.position + (transform.forward * _characterCollider.radius) + new Vector3(0, 1, 0);
        RaycastHit obstacleInfo = new RaycastHit();

        if(Physics.Raycast(moveDirection, out obstacleInfo, _maxSpeed * 2f, _obstacleLayers)) {

            float distToObstacle = (obstacleInfo.point - forwardPointOnCharacterHull).magnitude;
            Debug.Log("hit " + distToObstacle);
            if (_debugVisuals) {
                Debug.DrawRay(moveDirection.origin, moveDirection.direction * _maxSpeed * 2f, Color.red);
		    }
            if(distToObstacle < desiredMovement.magnitude) {
                return Vector3.ClampMagnitude(desiredMovement, distToObstacle);
			}
		}
		if (_debugVisuals) {
            Debug.DrawRay(moveDirection.origin, moveDirection.direction * _maxSpeed * 2f, Color.green);
		}

        return desiredMovement;
	}
}
