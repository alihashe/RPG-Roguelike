using UnityEngine;

public class Spawner : MonoBehaviour
{
    Pooling itemPooler;
    
    void Start()
    {
        itemPooler = Pooling.Instance;
    }

    
    void FixedUpdate()
    {
        itemPooler.PullFromPool("Bullets", transform.position);
    }
}
