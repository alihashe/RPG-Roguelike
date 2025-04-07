using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
    /* Always attach to the game manager object in the scene */
{
    public static BulletSpawner instance;

    List<GameObject> pooledProjectiles = new List<GameObject>();
    int amountProjectiles = 20;

    [SerializeField] GameObject projectilePrefab;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        for (int i = 0; i < amountProjectiles; i++)
        {
            GameObject obj = Instantiate(projectilePrefab);
            obj.SetActive(false);
            pooledProjectiles.Add(obj);
        }
    }

    public GameObject RetrieveProjectile()
    {
        for(int i = 0; i < pooledProjectiles.Count; i++)
        {
            if (!pooledProjectiles[i].activeInHierarchy)
            {
                return pooledProjectiles[i];
            }
        }

        return null;
    }
}
