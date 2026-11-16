using UnityEngine;

public class Scissors : PickableItem
{
 
    public Sprite closedSprite;
    public Sprite openSprite;
    //sprite-ul foarfecii nu e desenat perfect spre dreapta
    public float spriteAngleOffset = 0f;

    [Header("Throw Settings")]
    public float throwForce = 12f;

    [Header("Trajectory Dots Settings")]
    public GameObject dotPrefab;
    public int dotsCount = 15;
    public float timeBetweenDots = 0.06f;
    public float startDotSize = 0.2f;
    public float endDotSize = 0.04f;

    private bool isThrown = false;
    private float throwTime = 0f;
    private Rigidbody2D scissorsRb;
    private Collider2D scissorsCol;
    private SpriteRenderer spriteRenderer;
    private GameObject[] dots;

    void Start()
    {
        scissorsRb = GetComponent<Rigidbody2D>();
        scissorsCol = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && closedSprite != null)
        {
            spriteRenderer.sprite = closedSprite;
        }

        if (dotPrefab != null)
        {
            dots = new GameObject[dotsCount];
            for (int i = 0; i < dotsCount; i++)
            {
                dots[i] = Instantiate(dotPrefab);
                dots[i].SetActive(false);

                float progress = (float)i / (dotsCount - 1);
                float currentSize = Mathf.Lerp(startDotSize, endDotSize, progress);
                dots[i].transform.localScale = new Vector3(currentSize, currentSize, 1f);
            }
        }
    }

    public override void Interact(PlayerController player)
    {
        if (!isHeld && !player.isCarryingItem)
        {
            isThrown = false;
            if (spriteRenderer != null && closedSprite != null)
            {
                spriteRenderer.sprite = closedSprite;
            }
            PickUp(player);
        }
    }

    void Update()
    {
        if (isHeld)
        {
            Vector2 throwDir = GetAimDirection();
            UpdateTrajectoryDots(transform.position, throwDir * throwForce);

            // Orientam foarfeca spre cursor si cat timp e in mana playerului
            AimSpriteTowards(throwDir);

            if (Input.GetMouseButtonDown(0))
            {
                ThrowScissors(throwDir);
            }
        }
        else
        {
            SetDotsActive(false);

            // Cat timp e in zbor, o tinem orientata pe directia in care se misca
            if (isThrown && scissorsRb != null && scissorsRb.linearVelocity.sqrMagnitude > 0.1f)
            {
                AimSpriteTowards(scissorsRb.linearVelocity.normalized);
            }
        }
    }

    private void AimSpriteTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + spriteAngleOffset);
    }

    private Vector2 GetAimDirection()
    {
        Camera activeCam = GetActiveCamera();
        if (activeCam == null) return Vector2.right;

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(activeCam.transform.position.z - transform.position.z);
        Vector3 mouseWorldPos = activeCam.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = transform.position.z;

        return (mouseWorldPos - transform.position).normalized;
    }

    private Camera GetActiveCamera()
    {
        if (Camera.main != null && Camera.main.enabled) return Camera.main;
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.enabled) return cam;
        }
        return null;
    }

    private void UpdateTrajectoryDots(Vector2 startPos, Vector2 startVelocity)
    {
        if (dots == null) return;

        float gravityScale = scissorsRb != null ? scissorsRb.gravityScale : 1f;
        Vector2 gravity = Physics2D.gravity * gravityScale;

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] == null) continue;

            dots[i].SetActive(true);
            float t = (i + 1) * timeBetweenDots;
            Vector2 point = startPos + startVelocity * t + 0.5f * gravity * (t * t);
            dots[i].transform.position = point;
        }
    }

    private void SetDotsActive(bool state)
    {
        if (dots == null) return;

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] != null && dots[i].activeSelf != state)
            {
                dots[i].SetActive(state);
            }
        }
    }

    private void ThrowScissors(Vector2 throwDirection)
    {
        SetDotsActive(false);

        Collider2D holderCol = GetComponentInParent<Collider2D>();
        if (holderCol != null && scissorsCol != null)
        {
            Physics2D.IgnoreCollision(scissorsCol, holderCol, true);
        }

        Drop();

        isThrown = true;
        throwTime = Time.time;

        if (spriteRenderer != null && openSprite != null)
        {
            spriteRenderer.sprite = openSprite;
        }

        AimSpriteTowards(throwDirection);

        if (scissorsRb != null)
        {
            scissorsRb.freezeRotation = true;
            scissorsRb.angularVelocity = 0f;
            scissorsRb.linearVelocity = throwDirection * throwForce;
        }
    }

 //pentru Rope
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isThrown) return;

        Rope rope = collision.GetComponentInParent<Rope>();
        if (rope != null)
        {
            rope.CutRope();
            if (scissorsRb != null) scissorsRb.linearVelocity *= 0.4f;
        }
    }

    // cand loveste solul
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isThrown) return;

        // Verificam mai intai daca a lovit sfoara
        Rope rope = collision.gameObject.GetComponentInParent<Rope>();
        if (rope != null)
        {
            rope.CutRope();
            if (scissorsRb != null) scissorsRb.linearVelocity *= 0.4f;
            return; // Nu o inchidem imediat, o lasam sa treaca/cada dupa taiere
        }

        // Ignoram coliziunile din primele 0.1s sau cu playerii
        if (Time.time - throwTime < 0.1f) return;
        if (collision.gameObject.GetComponent<PlayerController>() != null) return;

        isThrown = false;

        if (spriteRenderer != null && closedSprite != null)
        {
            spriteRenderer.sprite = closedSprite;
        }
        if (scissorsRb != null)
        {
            scissorsRb.freezeRotation = false;
        }
    }

    private void OnDestroy()
    {
        if (dots != null)
        {
            for (int i = 0; i < dots.Length; i++)
            {
                if (dots[i] != null) Destroy(dots[i]);
            }
        }
    }
}