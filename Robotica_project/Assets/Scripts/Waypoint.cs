using UnityEngine;
using System.Numerics;
using System.Collections.Generic;



public class Waypoint : MonoBehaviour
{
    public Waypoint previousWaypoint;
    public Waypoint nextWaypoint;

    [Range(0f,5f)]
    public float width = 0.35f;

    [Range(0f,1f)]
    public float branchRatio=0.5f;

    public List<Waypoint> branches = new List<Waypoint>();



    public UnityEngine.Vector3 GetPosition()
    {
        UnityEngine.Vector3 minBound = transform.position + transform.right * width / 2f;
        UnityEngine.Vector3 maxBound = transform.position - transform.right * width/2f;

        return UnityEngine.Vector3.Lerp(minBound, maxBound, Random.Range(0f,1f));
    }
}
