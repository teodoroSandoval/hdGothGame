﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour {

    // self's camera
    private GameObject camera;
    private Vector3 camRelMovement;

    // animation
    private Animator anim;
    public float walkLimit = 0.5f;

    // for movement
    public float globalSpeed = 1;
    public float jumpSpeed = 20;
    private CharacterController charController;
    public float gravity = -35f;
    private Vector2 stickVector;
    private Vector2 camForward;
    private Vector2 camRight;
    private float upMovement;
    private Vector3 movementToSpeed;
    // this value multiply the stick raw input, increase for more speed
    public float speedScale = 14f;
    public float brakeFactor = 35f;
    public float counterSpeed = 40f;

    // for collitions
    private bool isGro;
    private bool isWal;
    private Vector3 floorNormal;
    private Vector3 wallNormal;
    public float groundAngleTolerance=40;
    public float wallAngleTolerance = 10;

    // the direction you are looking at
    private Quaternion lookRot;
    public float rotSpeed = 400;
    private bool isLookDirLocked;

    // Use this for initialization
    void Start () {
        charController = GetComponent<CharacterController>();
        camera = GameObject.Find("Main Camera");
        anim = GetComponent<Animator>();
        isGro = false;
        isWal = false;
        upMovement = 0;
        movementToSpeed = Vector3.zero;
    }
	
	// Update is called once per frame
	void Update () {
		move();
	}
    void move() {
        stickVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        camForward = new Vector2(camera.transform.forward.x, camera.transform.forward.z);
        camRight = new Vector2(camera.transform.right.x, camera.transform.right.z);
        camForward.Normalize();
        camRight.Normalize();

        camRelMovement = new Vector3((camForward.x * stickVector.y) + (camRight.x * stickVector.x), 0f, (camForward.y * stickVector.y) + (camRight.y * stickVector.x));
        float magnitude = stickVector.magnitude;

        if (magnitude > 0) {
            if(!isLookDirLocked)
                lookRot = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, Angle(camRelMovement), 0), rotSpeed * Time.deltaTime);
            anim.SetBool("walking", true);
            if (magnitude > walkLimit) {
                anim.SetBool("running", true);
            }
            else {
                anim.SetBool("running", false);
            }
        }
        else {
            anim.SetBool("walking", false);
            anim.SetBool("running", false);
        }

        upMovement += gravity * Time.deltaTime;

        if (!isLookDirLocked)
            transform.rotation = lookRot;

        Debug.Log(magnitude);
        if(magnitude > 0) {
            movementToSpeed.x = axisMoveTowards(movementToSpeed.x, camRelMovement.x);
            movementToSpeed.z = axisMoveTowards(movementToSpeed.z, camRelMovement.z);
        }
        else {
            movementToSpeed.x = axisStop(movementToSpeed.x);
            movementToSpeed.z = axisStop(movementToSpeed.z);
        }
        
        movementToSpeed.y = upMovement;

        Debug.Log(movementToSpeed);
        isGro = false;
        isWal = false;
        charController.Move(movementToSpeed * Time.deltaTime * globalSpeed);

        if (Input.GetButtonDown("Jump") && isGro) {
            upMovement = jumpSpeed;
            anim.Play("Jump", 0);
        }
    }
    private float axisMoveTowards(float speedAxis, float moveAxis) {
        if (speedAxis != moveAxis * speedScale) {
            if (moveAxis > 0) {
                if (speedAxis > moveAxis * speedScale) {
                    return speedAxis - (brakeFactor * Time.deltaTime);
                }
                else {
                    return speedAxis + (counterSpeed * Time.deltaTime);
                }
            }
            else {
                if (speedAxis < moveAxis * speedScale) {
                    return speedAxis + (brakeFactor * Time.deltaTime);
                }
                else {
                    return speedAxis - (counterSpeed * Time.deltaTime);
                }
            }
        }
        return speedAxis;
    }

    private float axisStop(float speedAxis) {
        if (speedAxis != 0) {
            float sign = Mathf.Sign(speedAxis);
            float newSpd = speedAxis - (brakeFactor * Time.deltaTime * sign);
            if (sign != Mathf.Sign(newSpd))
                return 0;
            else
                return newSpd;
        }
        return speedAxis;
    }

    private float hitAngle;
    private void OnControllerColliderHit(ControllerColliderHit hit) {
        hitAngle = Vector3.Angle(hit.normal, Vector3.up);
        if (hitAngle < groundAngleTolerance) {
            isGro = true;
            upMovement = 0;
            floorNormal = hit.normal;
        }

        if (hitAngle > 90 - wallAngleTolerance && hitAngle < 90 + wallAngleTolerance) {
            isWal = true;
            wallNormal = hit.normal;
        }
    }


    // getters
    public bool isGrounded() {
        return isGro;
    }
    public bool isWalled() {
        return isWal;
    }
    public Vector3 getFloorNormal() {
        return floorNormal;
    }
    public Vector3 getWallNormal() {
        return wallNormal;
    }
    // setters
    public void lockLookDirection(Quaternion lookDirection) {
        isLookDirLocked = true;
        transform.rotation = lookDirection;
    }
    
    public void freeLookDirection() {
        isLookDirLocked = false;
    }
    // this function calculate the angle just in the bottom plane
    public static float Angle(Vector3 dirVector) {
        if (dirVector.x < 0) {
            return 360 - (Mathf.Atan2(dirVector.x, dirVector.z) * Mathf.Rad2Deg * -1);
        }
        else {
            return Mathf.Atan2(dirVector.x, dirVector.z) * Mathf.Rad2Deg;
        }
    }
}
