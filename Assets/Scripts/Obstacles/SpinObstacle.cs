using System;
using System.Collections;
using System.Collections.Generic;
using Obstacles;
using UnityEngine;

public class SpinObstacle : MonoBehaviour
{
    [SerializeField] private float speed;
    
    private Vector3 startingRotation;
    
    // Start is called before the first frame update
    void Start()
    {
        startingRotation = transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        float angleChange = (speed *  Time.deltaTime) * GameInfoHolder.speedModifier;

        float newAngle = transform.localEulerAngles.y - angleChange;
        
        if (newAngle < 0)
        {
            newAngle += 360;
        }
        
        transform.localEulerAngles = new Vector3(startingRotation.x, newAngle, startingRotation.z);
    }
}
