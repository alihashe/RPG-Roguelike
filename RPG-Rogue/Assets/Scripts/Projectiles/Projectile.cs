using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] GameObject target;
    Rigidbody2D rb; 
    StatHolder playerStats;
    [SerializeField] float moveSpeed;
    [SerializeField] int pDamage;

    void OnEnable()
    {
        target = GameObject.FindWithTag("Player");
        playerStats = target.GetComponent<StatHolder>();
        rb = GetComponent<Rigidbody2D>();

        pDamage = 18;
        moveSpeed = 11;
        Invoke("MoveToTarget", 0.15f);
    }

    void OnDisable()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
    }

    void OnBecameInvisible()
    {
        rb.angularVelocity = 0;
        gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D contact)
    {
        if (contact.gameObject.tag == "Player") 
        {
            Debug.Log(this.name + " hit " + contact.gameObject.name);
            DoDamage(pDamage);
        }
    }

    void DoDamage(int damage)
    {
        playerStats.TakeDamage(damage);
        this.gameObject.SetActive(false);
    }

    void MoveToTarget()
    {
        Vector2 directionTarget = (target.transform.position - transform.position).normalized;

        float pAngle = (Mathf.Atan2(directionTarget.y, directionTarget.x) * Mathf.Rad2Deg) - 90.0f;
        transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position, Vector3.up);

        rb.AddForce(directionTarget * moveSpeed, ForceMode2D.Impulse);
    }
}
