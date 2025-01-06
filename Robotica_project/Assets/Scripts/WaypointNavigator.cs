using UnityEngine;

public class WaypointNavigator : MonoBehaviour
{

    PedestrianController controller;
    public Waypoint currrentWaypoint;

    int direction;

    private void Awake()
    {
        controller = GetComponent<PedestrianController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        direction = Mathf.RoundToInt(Random.Range(0f,1f));

        controller.SetDestination(currrentWaypoint.GetPosition());
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.reachedDestination)
        {
            bool shouldBranch = false;
            if(currrentWaypoint.branches !=null && currrentWaypoint.branches.Count > 0)
            {
                shouldBranch = Random.Range(0f,1f) <= currrentWaypoint.branchRatio? true : false;
            }
            if (shouldBranch)
            {
                currrentWaypoint = currrentWaypoint. branches [Random. Range(0, currrentWaypoint.branches. Count - 1)];
            }
            
            else 
            {
                if(direction==0)
                {
                    if(currrentWaypoint.nextWaypoint!=null)
                    {
                        currrentWaypoint = currrentWaypoint.nextWaypoint;
                    }
                    else
                    {
                        currrentWaypoint = currrentWaypoint.previousWaypoint;
                        direction=1;
                    }
                    
                }
                else if (direction==1)
                {
                    if(currrentWaypoint.previousWaypoint!=null)
                    {
                        currrentWaypoint= currrentWaypoint.previousWaypoint;
                    }
                    else
                    { 
                        currrentWaypoint = currrentWaypoint.nextWaypoint;
                        direction=0;
                    }
                }
                controller.SetDestination(currrentWaypoint.GetPosition());
            }
        }
    }

}
