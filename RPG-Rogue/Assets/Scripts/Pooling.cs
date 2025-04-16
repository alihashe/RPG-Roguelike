using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Pooling : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    #region Singleton
    public static Pooling Instance;

    void Awake()
    {
        Instance = this;
    }
    #endregion

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDict;

    void Start()
    {
        poolDict = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools) 
        {
            Queue<GameObject> itemPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject itm = Instantiate(pool.prefab);
                itm.SetActive(false);
                itemPool.Enqueue(itm);
            }

            poolDict.Add(pool.tag, itemPool);
        }

    }

    public GameObject PullFromPool(string tag, Vector2 position)
    {
        if (!poolDict.ContainsKey(tag)) 
        { 
            Debug.LogWarning("Pool does not contain the object with the tag " + tag + ".");
            return null;
        }
        GameObject itmToPull = poolDict[tag].Dequeue();

        itmToPull.SetActive(true);
        itmToPull.transform.position = position;
        poolDict[tag].Enqueue(itmToPull);
        return itmToPull;
    }

}