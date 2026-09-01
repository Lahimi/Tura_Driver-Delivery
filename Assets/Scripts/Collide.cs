using UnityEngine;

public class Collide : MonoBehaviour
{
    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collided with " + collision.gameObject.name);
        Destroy(collision.gameObject);
    }*/

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Triggered with: " + collision.gameObject.name);
        //Destroy(collision.gameObject);
    }
}
