using NUnit.Framework;
using Pathfinding;
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    AIDestinationSetter aiScript; // Reference to the script used to set the target of movement for the enemy
    Transform target; // Used to set the target of the Enemy AI
    Transform playerT; // The transform position of the player
    Transform[] waypoints; // The points that will be used to direct enemy movement
    [SerializeField] float distanceToWaypoint; // The distance between this object and the current waypoint that is targeted
    int stateNumber; // Determines the state that the enemy is in

    void Awake()
    {
        aiScript = GetComponent<AIDestinationSetter>();
        playerT = GameObject.FindGameObjectWithTag("Player").transform; // Find the player location
    }
    
    void Update()
    {
        FollowPlayer();
        distanceToWaypoint = Vector2.Distance(this.transform.position, target.position);
        aiScript.target = target;
        //if (target != playerT)
        //{
        //    foreach (Transform wp in waypoints)
        //    {
        //        if (distanceToWaypoint < )
        //        {
        //        }
        //    }
        //}
        switch (stateNumber)
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
                break;
        }
    }

    public void FollowNewTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    public void FollowPlayer()
    {
        target = playerT;
    }

}
