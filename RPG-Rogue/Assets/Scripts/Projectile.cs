using UnityEngine;

public class Projectile : MonoBehaviour
{
    GameObject player;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, player.transform.position, out hit))
            print("Found an object - distance: " + hit.distance);

        Debug.DrawLine(transform.position, hit.point);
    }
}
