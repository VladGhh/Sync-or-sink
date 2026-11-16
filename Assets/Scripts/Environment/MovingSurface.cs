using UnityEngine;

public class MovingSurface : MonoBehaviour
{
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            // Verificam ca playerul sa fie deasupra obiectului (normala coliziunii pointeaza in jos spre obiect)
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    player.platformVelocity = rb.linearVelocity;
                    return;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.platformVelocity = Vector2.zero;
        }
    }
}