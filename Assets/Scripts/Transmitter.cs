using System;
using System.Collections.Generic;
using UnityEngine;

public class Transmitter : MonoBehaviour
{
    [HideInInspector] public bool isConnected;
    [HideInInspector] public float minDistFromHome;
    [HideInInspector] public bool wasConnectedPrevPulse;
    [HideInInspector] public List<Tuple<float, Transmitter>> connectedParents;
    [HideInInspector] public bool isAlive;

    protected virtual void Awake()
    {
        isAlive = true;
        wasConnectedPrevPulse = false;
        isConnected = false;
        minDistFromHome = 0f;
        connectedParents = new List<Tuple<float, Transmitter>>();
    }

    public void ResetConnections()
    {
        if (isConnected)
            wasConnectedPrevPulse = true;
        isConnected = false;
        minDistFromHome = 0f;
        connectedParents = new List<Tuple<float, Transmitter>>();
    }

    // Send a pulse from the home base to all the visible transmitters in order to check who is visable through who.
    public void SendAckPulse(float radius)
    {
        List<Transmitter> nearbyTransmitters = new List<Transmitter>();
        int layerMask = LayerMask.GetMask("Bot", "Edge Transmitter");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, layerMask);
        Array.Sort(colliders, (c1, c2) => DistanceFrom(c1.bounds.center).CompareTo(DistanceFrom(c2.bounds.center)));
        foreach (Collider2D coll in colliders)
        {
            Transmitter other = coll.GetComponent<Transmitter>();
            if (!other || !other.isAlive || !IsParentOf(other))
                continue;
            float distance = DistanceFrom(other.transform.position);
            if (!other.connectedParents.Exists((tup) => tup.Item2 == this))
            {
                other.connectedParents.Add(new Tuple<float, Transmitter>(minDistFromHome + distance, this));
            }
            if (!other.isConnected)
            {
                nearbyTransmitters.Add(other);
                other.minDistFromHome = minDistFromHome + distance;
            }
            else
            {
                other.minDistFromHome = Mathf.Min(other.minDistFromHome, minDistFromHome + distance);
            }
            other.isConnected = true;
        }

        foreach (Transmitter transmitter in nearbyTransmitters)
        {
            if (transmitter.wasConnectedPrevPulse)
                transmitter.SendAckPulse(radius);
        }
    }

    // Check if this transmitter provides data to another transmitter.
    private bool IsParentOf(Transmitter other)
    {
        if (!other || other == this || connectedParents.Exists((tup) => tup.Item2 == other))
            return false;
        // Check collide with wall.
        Vector2 delta = other.transform.position - transform.position;
        if (Physics2D.Raycast(transform.position, delta, delta.magnitude, LayerMask.GetMask("Wall")))
            return false;
        // Special logic for ckecking goal transmitter.
        GoalTransmitter goalTransmitter = other.GetComponent<GoalTransmitter>();
        if (goalTransmitter && delta.magnitude > goalTransmitter.recieveRadius)
            return false;
        return (minDistFromHome <= other.minDistFromHome || !other.isConnected);
    }

    private float DistanceFrom(Vector3 position)
    {
        return (position - transform.position).magnitude;
    }
}