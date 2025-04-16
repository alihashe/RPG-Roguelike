using UnityEngine;

public class Projectile : MonoBehaviour
{
    GameObject player;
    Rigidbody2D rb; 
    StatHolder playerStats;
    float defaultMoveSpeed;
    [SerializeField] float moveSpeed;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerStats = player.GetComponent<StatHolder>();
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 5;
        defaultMoveSpeed = moveSpeed;
        RotateProjectile();
    }

    void FixedUpdate ()
    {
        MoveProjectile(moveSpeed);
    }

    void OnTriggerEnter2D(Collider2D contact)
    {
        if (contact.gameObject.tag == "Player") 
        {
            Debug.Log(this.name + " hit " + contact.gameObject.name);
            DoDamage(15);
        }
    }

    void MoveProjectile(float projMoveSpeed)
    {
        rb.AddForce(transform.up * projMoveSpeed, ForceMode2D.Force);
    }

    void RotateProjectile()
    {
        rb.linearVelocity = Vector2.zero;

        Vector2 pDirection = (player.transform.position - transform.position).normalized; // Direction to player

        float pAngle = Mathf.Atan2(pDirection.y, pDirection.x) * Mathf.Rad2Deg - 90f; // From rads to degreees

        rb.MoveRotation(pAngle); // Rotate towards player
    }

    void DoDamage(int damage)
    {
        playerStats.TakeDamage(damage);
        this.gameObject.SetActive(false);
    }
}
