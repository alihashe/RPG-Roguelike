using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.CompareTag("Player"))
            {
                // Find a way to get rid of item without destroy/setactive
                Debug.Log("Touched " + gameObject.name);
                gameObject.SetActive(false);
            }
        }
    }

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision != null)
    //    {
    //        if (collision.CompareTag("Player"))
    //        {
    //            // Pass
    //        }
    //    }
    //}
}
