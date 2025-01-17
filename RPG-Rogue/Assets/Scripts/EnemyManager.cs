using Pathfinding;
using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    AIDestinationSetter aiScript; // Reference to the script used to set the target of movement for the enemy
    Transform target; // Used to set the target of the Enemy AI
    Transform playerT; // The transform position of the player
    [SerializeField] Transform[] waypoints; // The points that will be used to direct enemy movement
    [SerializeField] float distanceToWaypoint; // The distance between this object and the current waypoint that is targeted
    float currentDistanceFromPlayer; // The current distance between this object and the player
    float playerPursuitDistance = 8; // The distance the enemy can be before they start to pursue the player
    float cautiousTimer = 5f; // The amount of time the enemy will stop for after losing sight of the player and before returning to patrol
    float cautiousTemp;
    int currentWaypointIndex = 0; // Used to determine which waypoint the enemy will move toward
    EnemyState enemyState; // Determines the state that the enemy is in
    StatHolder enemyStatHolder; // Instance of the stat holder attached to this object

    void Awake()
    {
        aiScript = GetComponent<AIDestinationSetter>();
        playerT = GameObject.FindGameObjectWithTag("Player").transform; // Find the player location
        enemyStatHolder = GetComponent<StatHolder>();
    }

    void Start()
    {
        cautiousTemp = cautiousTimer;
        target = waypoints.Length > 0 ? waypoints[0] : playerT; // Set target to the first waypoint, or player if no waypoints
        enemyState = EnemyState.Patrol;
    }

    void Update()
    {
        currentDistanceFromPlayer = Vector2.Distance(playerT.position, this.transform.position);
        aiScript.target = target;
        cautiousTimer -= Time.deltaTime;
        switch (enemyState)
        {
            case EnemyState.Patrol:
                HandlePatrol();
                break;
            case EnemyState.Pursuit:
                HandlePursuit();
                break;
            case EnemyState.Cautious:
                HandleCautious();
                break;
            case EnemyState.Dead:
                HandleDeath();
                break;
        }
        if (enemyStatHolder.GetHealth <= 0)
            enemyState = EnemyState.Dead;
    }

    #region Enemy Handle Functions
    void HandlePatrol()
    {
        this.GetComponent<SpriteRenderer>().color = Color.white;
        if (target != playerT)
        {
            target = waypoints[currentWaypointIndex];
            distanceToWaypoint = Vector2.Distance(this.transform.position, target.position);
            if (distanceToWaypoint <= 1.5f)
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
        if (currentDistanceFromPlayer < playerPursuitDistance)
            enemyState = EnemyState.Pursuit;
    }

    void HandlePursuit()
    {
        this.GetComponent<SpriteRenderer>().color = Color.red;
        target = playerT;
        if (currentDistanceFromPlayer >= playerPursuitDistance)
        {
            target = null;
            cautiousTimer = cautiousTemp;
            enemyState = EnemyState.Cautious;
        }
    }

    void HandleCautious()
    {
        this.GetComponent<SpriteRenderer>().color = Color.yellow;
        if (currentDistanceFromPlayer < playerPursuitDistance)
            enemyState = EnemyState.Pursuit;
        else if (cautiousTimer < 0)
            enemyState = EnemyState.Patrol;
    }

    void HandleDeath()
    {
        target = null;
        Color tmpCol = this.GetComponent<SpriteRenderer>().color;
        tmpCol.a = 0.4f;
        GetComponent<SpriteRenderer>().color = tmpCol;
        GetComponent<Collider2D>().isTrigger = true;
        // Create loot state here later
    }

    #endregion

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(this.transform.position, (playerPursuitDistance)); // Draw the circle that displays how close the object can be to the player before pursuing
    }

}
