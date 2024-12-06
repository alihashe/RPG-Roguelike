using Pathfinding;
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    AIDestinationSetter aiScript; 
    Transform target; // Used to set the target of the Enemy AI
    Transform playerT; // The transform position of the player
    GameObject observeCircle; // Empty gameobject used to track if the player is near

    // float observeRange = 4f; // Observe circle size

    void Awake()
    {
        aiScript = GetComponent<AIDestinationSetter>();
        playerT = GameObject.FindGameObjectWithTag("Player").transform;
        observeCircle = this.gameObject.transform.GetChild(0).GetComponent<GameObject>(); // !!! MAKE SURE THE OBSERVE HITBOX GAMEOBJECT IS THE FIRST CHILD OF THE PLAYER !!!
    }
    
    void Update()
    {
        aiScript.target = target;
        //if (Physics2D.OverlapCircle(observeCircle.transform.position, observeRange))
        //{
        //    FollowPlayer();
        //}
    }

    private void OnTriggerStay2D()
    {
        if (observeCircle != null && observeCircle.CompareTag("Player"))
        {
            Debug.Log("PLAAAAAAYYYYYEEEEERRRRRRRRRR");
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

    //void OnDrawGizmosSelected()
    //{
    //    if (observeCircle != null)
    //        Gizmos.DrawWireSphere(observeCircle.transform.position, observeRange);
    //}

}
