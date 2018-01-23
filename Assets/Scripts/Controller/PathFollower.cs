﻿using System.Collections.Generic;
using UnityEngine;

public class PathFollower : PausableObject
{
    private IPath path;

    [SerializeField]
    public float maxForce;

    [SerializeField]
    public float mass;

    [SerializeField]
    public float maxSpeed;

    [SerializeField]
    public bool reverseOrder = false;

    [SerializeField]
    public bool followPath = true;

    private Vector3 velocity = Vector3.zero;

    private int currentNode = 0;

    private Vector3 currentTarget;

    private Vector3 PathFollowing()
    {
        if (path == null)
            return new Vector3();

        List<Vector3> nodes = path.GetNodes(reverseOrder);

        currentTarget = nodes[currentNode];

        if (Vector3.Distance(transform.position, currentTarget) <= 0.01f)
        {
            if (simController.SimulationRunning)
                currentNode++;

            if (currentNode >= nodes.Count)
                followPath = false;
        }

        return Seek(currentTarget);
    }

    private Vector3 Seek(Vector3 target)
    {
        Vector3 desiredVelocity = Vector3.Normalize(target - transform.position) * maxSpeed;
        Vector3 steering = desiredVelocity - velocity;
        return steering;
    }

    public void SetPath(IPath path)
    {
        this.path = path;
        currentNode = 0;
    }
    protected override void Update()
    {
        HandleUpdate();
    }

    protected override void HandleUpdate()
    {
        if (!followPath)
            return;

        Vector3 steering = Vector3.zero;
        steering += PathFollowing();

        steering = Vector3.ClampMagnitude(steering, maxForce);
        steering /= mass;

        if(simController.SimulationRunning)
        {
            velocity = Vector3.ClampMagnitude(velocity + steering, maxSpeed);
            transform.position += velocity;
        }
        else
        {
            Vector3 direction = currentTarget - transform.position;

            transform.position += direction;
        }
    }

    protected override void HandleFixedUpdate()
    {
        // not implemended
    }
}
