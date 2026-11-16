using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Boat : Interactable
{
    [Header("Movement Settings")]
    public float boatSpeed = 5f;
    public bool useHorizontalLimits = false;
    public float minX = -10f;
    public float maxX = 10f;

    [Header("Rowing Seat")]
    public Transform rowingSeat; // Un Empty GameObject copil pe barca unde sta playerul cand vasleste

    private Rigidbody2D rb;
    private PlayerController driver = null;
    private float moveInput = 0f;

    // Tin evidenta obiectelor (cutii, celalalt player) care stau pe barca ca sa se miste odata cu ea
    private HashSet<Rigidbody2D> passengers = new HashSet<Rigidbody2D>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Kinematic ca sa nu se scufunde cand cade o cutie grea pe ea
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public override void Interact(PlayerController player)
    {
        // 1. Daca nimeni nu conduce barca -> playerul acesta urca la vasle
        if (driver == null && !player.isCarryingItem)
        {
            StartRowing(player);
        }
        // 2. Daca acelasi player apasa din nou Interact -> iese de la vasle
        else if (driver == player)
        {
            StopRowing();
        }
    }

    private void StartRowing(PlayerController player)
    {
        driver = player;
        driver.isRowing = true;

        if (rowingSeat != null)
        {
            driver.transform.position = rowingSeat.position;
        }
    }

    private void StopRowing()
    {
        if (driver != null)
        {
            driver.isRowing = false;
            driver.platformVelocity = Vector2.zero;
            driver = null;
        }
        moveInput = 0f;
    }

    void Update()
    {
        if (driver != null)
        {
            // Citim input-ul exact al playerului care conduce barca
            moveInput = driver.isPlayer1 ? Input.GetAxisRaw("Horizontal1") : Input.GetAxisRaw("Horizontal2");

            // Intoarcem sprite-ul playerului spre directia in care vasleste
            if (moveInput > 0)
                driver.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            else if (moveInput < 0)
                driver.transform.localScale = new Vector3(-0.5f, 0.5f, 1f);
        }
        else
        {
            moveInput = 0f;
        }
    }

    void FixedUpdate()
    {
        Vector2 currentPos = rb.position;
        float targetX = currentPos.x + moveInput * boatSpeed * Time.fixedDeltaTime;

        if (useHorizontalLimits)
        {
            targetX = Mathf.Clamp(targetX, minX, maxX);
        }

        Vector2 newPos = new Vector2(targetX, currentPos.y);
        Vector2 deltaMove = newPos - currentPos;
        Vector2 boatVelocity = deltaMove / Time.fixedDeltaTime;

        rb.MovePosition(newPos);

        // 1. Tin playerul care vasleste fixat pe scaunul de vaslit
        if (driver != null)
        {
            driver.platformVelocity = boatVelocity;
            if (rowingSeat != null)
            {
                driver.transform.position = new Vector3(rowingSeat.position.x, driver.transform.position.y, driver.transform.position.z);
            }
        }

        // 2. Mutam cutiile si celalalt player odata cu barca ca sa nu alunece
        foreach (Rigidbody2D passengerRb in passengers)
        {
            if (passengerRb == null) continue;
            if (driver != null && passengerRb.gameObject == driver.gameObject) continue;

            PlayerController otherPlayer = passengerRb.GetComponent<PlayerController>();
            if (otherPlayer != null)
            {
                otherPlayer.platformVelocity = boatVelocity;
            }
            else
            {
                // Este o cutie (Box) -> o mutam odata cu barca
                passengerRb.position += deltaMove;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        AddPassengerIfOnTop(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        AddPassengerIfOnTop(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Rigidbody2D passengerRb = collision.rigidbody;
        if (passengerRb != null && passengers.Contains(passengerRb))
        {
            PlayerController pc = passengerRb.GetComponent<PlayerController>();
            if (pc != null && pc != driver)
            {
                pc.platformVelocity = Vector2.zero;
            }
            passengers.Remove(passengerRb);
        }
    }

    private void AddPassengerIfOnTop(Collision2D collision)
    {
        if (collision.rigidbody == null) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Verificam ca obiectul sa stea deasupra barcii
            if (contact.normal.y < -0.5f)
            {
                passengers.Add(collision.rigidbody);
                return;
            }
        }
    }
}