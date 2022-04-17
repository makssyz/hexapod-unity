using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;
using Debug = UnityEngine.Debug;

public class SpiderAgent : Agent
{
    public List<HingeJoint> joints;
    private Rigidbody _rigidbody;

    public GameObject head;

    public void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }



    public override void OnEpisodeBegin()
    {
        // Reset all joint angles.
        foreach (var joint in joints)
            SetSpringPosition(joint, 0);
        
        // Bring spider back to the start position.
        var spiderTransform = transform;
        spiderTransform.localPosition = new Vector3(0, 3.5f, 0);
        spiderTransform.rotation = Quaternion.identity;
        
        // Reset velocity
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.velocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Joints' rotations.
        foreach (var joint in joints)
            sensor.AddObservation(joint.spring.targetPosition);
        
        // Spider's position.
        sensor.AddObservation(transform.localPosition.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Set the spring positions for each joint.
        for (var i = 0; i < 12; i++)
        {
            var targetPosition = (int) joints[i].spring.targetPosition + actions.DiscreteActions[i] - 1;
            SetSpringPosition(joints[i], targetPosition);
        }
           
        // Set reward based an the target's distance.
        //var distanceToTarget = Vector3.Distance(transform.localPosition, target.transform.localPosition);
        SetReward(_rigidbody.velocity.z);

        // End episode when spider has reached the target.
        //if (distanceToTarget < 2)
            //EndEpisode();
        
        // End episode when spider falls off the plane.
        if (transform.localPosition.y < 0)
            EndEpisode();
        
        // End episode when spider turns on its head.
        if (transform.position.y > head.transform.position.y)
            EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        Debug.Log("Velocity: " + _rigidbody.velocity.z);
    }

    private static void SetSpringPosition(HingeJoint joint, float position)
    {
        var jointSpring = joint.spring;
        jointSpring.targetPosition = Mathf.Clamp(position, -45, 45);
        joint.spring = jointSpring;
    }
}
