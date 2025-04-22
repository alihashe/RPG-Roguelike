using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    Pooling itemPooler; // References the Pooling script used for object pooling
    Vector3 randomSpawn;
    
    void Start()
    {
        itemPooler = Pooling.Instance; // References the singular instance for object pooling
        InvokeRepeating("SpawnBullets", 2f, 1f); // Every 2 seconds a bullet will spawn
    }
    
    void SpawnBullets()
    {
        float xRange = Random.Range(-15, 15);
        float yRange = Random.Range(-9, 9);
        while ((-12 <= xRange && xRange <= 12) && (-7 <= yRange && yRange <= 7))
        {
            xRange = Random.Range(-15, 15);
            yRange = Random.Range(-9, 9);
        }
        randomSpawn = new Vector3(xRange, yRange);
        itemPooler.PullFromPool("Bullets", randomSpawn);
    }
}
