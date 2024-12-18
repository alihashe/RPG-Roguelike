using UnityEngine;
using Pathfinding;

public class EnemyActions : MonoBehaviour
{
    public AIPath aiPath;

    void Start()
    {
        aiPath = GetComponent<AIPath>();
    }

    
    void Update()
    {
        if (aiPath.desiredVelocity.x >= 0.01f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (aiPath.desiredVelocity.x <= -0.01f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }
}
