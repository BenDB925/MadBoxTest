using System;
using System.Collections;
using System.Collections.Generic;
using Obstacles;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float playerSpeed = 0.1f;
    [SerializeField] private PathController pathController;
    private float percentThroughCourse = 0;

    private Rigidbody rigidbody;
    private bool isDead;

    private Vector3 startPosition;
    private Vector3 startOrientation;
    private Vector3 startCameraPos;
    private Vector3 startCameraRotation;
    
    public static UnityEvent deathEvent;
    
    void Awake()
    {
        deathEvent = new UnityEvent();
        rigidbody = GetComponent<Rigidbody>();
        isDead = false;
        MovePlayer();
        startPosition = transform.position;
        startOrientation = transform.eulerAngles;
        startCameraPos = Camera.main.transform.position;
        startCameraRotation = Camera.main.transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0 || Input.GetKey(KeyCode.Space) && isDead == false)
        {
            MovePlayer();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPlayer();
        }
    }

    public void ResetPlayer()
    {
        percentThroughCourse = 0;
        transform.position = startPosition;
        transform.eulerAngles = startOrientation;
        isDead = false;
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.position = startCameraPos;
        Camera.main.transform.eulerAngles = startCameraRotation;
        rigidbody.useGravity = false;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        GameInfoHolder.speedModifier = 1;
        GameInfoHolder.numLapsCompleted = 0;
    }

    private void MovePlayer()
    {
        percentThroughCourse += (playerSpeed * Time.deltaTime) * GameInfoHolder.speedModifier;
        percentThroughCourse = Mathf.Clamp(percentThroughCourse, 0, 0.99f);

        float lookAheadPoint = percentThroughCourse + 0.1f;
        lookAheadPoint = Mathf.Clamp(lookAheadPoint, 0, 0.99f);
        
        Vector3 pointOnCourse = pathController.GetPointOnCourse(percentThroughCourse);
        pointOnCourse.y = transform.position.y;
        transform.position = pointOnCourse;

        if (lookAheadPoint < 0.95f)
        {
            Vector3 lookAtPoint = pathController.GetPointOnCourse(lookAheadPoint);
            transform.LookAt( lookAtPoint);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        switch (other.gameObject.tag)
        {
            case "Obstacle":
                isDead = true;
                Camera.main.transform.SetParent(null);
                rigidbody.useGravity = true;
                deathEvent.Invoke();
                break;
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead)
        {
            return;
        }

        switch (other.gameObject.tag)
        {
            case "Finish":
                percentThroughCourse = 0;
                GameInfoHolder.speedModifier += 0.2f;
                GameInfoHolder.numLapsCompleted++;
                break;
        }
    }
}
