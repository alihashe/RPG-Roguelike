using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager instance;

    void Awake()
    {
        instance = this;
    }
    #endregion

    StatHolder playerStats;
    bool hasGameDoneBeenCalled;

    void Start()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<StatHolder>();
        hasGameDoneBeenCalled = false;
    }

    // Update is called once per frame
    void Update()
    {
        GameDone();
    }

    void GameDone()
    {
        if (playerStats.GetHealth <= 0 && playerStats.GetPlayerDeathBool && hasGameDoneBeenCalled == false)
        {
            Debug.Log("You Died!");
        }
        hasGameDoneBeenCalled = true;
    }
}
