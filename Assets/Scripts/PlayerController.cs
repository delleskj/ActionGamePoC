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

    [SerializeField, Tooltip("The physics layer on which to detect obstacles.")]
    LayerMask _obstacleLayers;

    CapsuleCollider _characterCollider;

    [SerializeField]
    string _horizontalMovementInputAxis = "Horizontal";
    
    [SerializeField]
    string _verticalMovementInputAxis = "Vertical";

    [Header("DEBUG")]
    [SerializeField]
    float _currentSpeed = 0f;

    [SerializeField]
    bool _debugVisuals = false;

	private void Awake() {
        _characterCollider = GetComponent<CapsuleCollider>();
	}

    ///
    // Open questions:
    // F: is input binary, or analogoue? => can we walk slow if only 0.5 of forward is pressed?
    // A: flowchart says accelerate when forward until max speed
    // F: should there be turnspeed?
    // A: assuming no because turn at any moment in any direction
    ///


	void Update() {
        float horizontal = Input.GetAxis(_horizontalMovementInputAxis);
        float vertical = Input.GetAxis(_verticalMovementInputAxis);

        Vector3 input = new Vector3(horizontal, 0, vertical);
        // clamp input so diagonal walking is not faster
        input = Vector3.ClampMagnitude(input, 1f);

		if (_debugVisuals) {
            Debug.DrawRay(transform.position + new Vector3(0,1.5f,0), input, Color.grey);
		}



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
        desiredMovement = CalculateMaxPossibleMovement(desiredMovement);
    	if (_debugVisuals) {
            Debug.Log("going to move: " + desiredMovement);
            Debug.DrawRay(transform.position + new Vector3(0,2f,0), desiredMovement, Color.magenta);
		}


        transform.position += desiredMovement;
    }

    /// <summary>
    /// Detects obstacles in the way and returns the maximum possible movement without collision.
    /// </summary>
    /// <param name="desiredMovement"></param>
    /// <returns></returns>
	private Vector3 CalculateMaxPossibleMovement(Vector3 desiredMovement) {
        Ray leftMoveDirection = new Ray(transform.position + new Vector3(0, 1, 0) + transform.right*-_characterCollider.radius, desiredMovement);
        Ray rightMoveDirection = new Ray(transform.position + new Vector3(0, 1, 0) + transform.right*_characterCollider.radius, desiredMovement);
        Ray centerMoveDirection = new Ray(transform.position + new Vector3(0, 1, 0), desiredMovement);

        Vector3 forwardPointOnCharacterHull = transform.position + (transform.forward * _characterCollider.radius) + new Vector3(0, 1, 0);
        Vector3 forwardPointOnCharacterHullLeft = transform.position + (transform.forward * _characterCollider.radius) + new Vector3(0, 1, 0) + transform.right* -_characterCollider.radius;
        Vector3 forwardPointOnCharacterHullRight = transform.position + (transform.forward * _characterCollider.radius) + new Vector3(0, 1, 0) + transform.right*_characterCollider.radius;
        RaycastHit obstacleInfo;

        // center point cast
        if(Physics.Raycast(centerMoveDirection, out obstacleInfo, _maxSpeed * 2f, _obstacleLayers)) {

            float distToObstacle = (obstacleInfo.point - forwardPointOnCharacterHull).magnitude;
            if (_debugVisuals) {
                Debug.Log("hit " + distToObstacle);
                Debug.DrawRay(centerMoveDirection.origin, centerMoveDirection.direction * _maxSpeed * 2f, Color.red);
		    }
            if(distToObstacle < desiredMovement.magnitude) {
                return Vector3.ClampMagnitude(desiredMovement, distToObstacle);
			}
		}

        // left point cast
        if(Physics.Raycast(leftMoveDirection, out obstacleInfo, _maxSpeed * 2f, _obstacleLayers)) {
            float distToObstacle = (obstacleInfo.point - forwardPointOnCharacterHullLeft).magnitude;
            if (_debugVisuals) {
                Debug.Log("left hit " + distToObstacle);
                Debug.DrawRay(leftMoveDirection.origin, leftMoveDirection.direction * _maxSpeed * 2f, Color.red);
		    }
            if(distToObstacle < desiredMovement.magnitude) {
                return Vector3.ClampMagnitude(desiredMovement, distToObstacle);
			}
		}

        // right point cast
        if(Physics.Raycast(rightMoveDirection, out obstacleInfo, _maxSpeed * 2f, _obstacleLayers)) {

            float distToObstacle = (obstacleInfo.point - forwardPointOnCharacterHullRight).magnitude;
            if (_debugVisuals) {
                Debug.Log("left hit " + distToObstacle);
                Debug.DrawRay(rightMoveDirection.origin, rightMoveDirection.direction * _maxSpeed * 2f, Color.red);
		    }
            if(distToObstacle < desiredMovement.magnitude) {
                return Vector3.ClampMagnitude(desiredMovement, distToObstacle);
			}
		}


		if (_debugVisuals) {
            Debug.DrawRay(centerMoveDirection.origin, centerMoveDirection.direction * _maxSpeed * 2f, Color.green);
            Debug.DrawRay(leftMoveDirection.origin, leftMoveDirection.direction * _maxSpeed * 2f, Color.green);
            Debug.DrawRay(rightMoveDirection.origin, rightMoveDirection.direction * _maxSpeed * 2f, Color.green);
		}

        return desiredMovement;
	}


}
