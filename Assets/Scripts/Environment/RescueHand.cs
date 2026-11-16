using System.Collections;
using UnityEngine;

public class RescueHand : MonoBehaviour
{
    public Sprite closedHandSprite;
    public Sprite openHandSprite;

    //Degetul din spatele obiectului)
    public GameObject fingerChild;

    
    public Transform holdPoint;
    public Vector3 dropOffset = new Vector3(0f, 0.5f, 0f);

    
    public float moveSpeed = 14f;
    public float offscreenYOffset = -15f;

    private SpriteRenderer spriteRenderer;

    public void InitRescue(Transform targetObj, Vector3 fallPosition)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(RescueRoutine(targetObj, fallPosition));
    }

    IEnumerator RescueRoutine(Transform targetObj, Vector3 fallPosition)
    {
        if (targetObj == null)
        {
            Destroy(gameObject);
            yield break;
        }

        Rigidbody2D targetRb = targetObj.GetComponent<Rigidbody2D>();
        PlayerController player = targetObj.GetComponent<PlayerController>();

        float originalGravity = 0f;
        if (targetRb != null)
        {
            originalGravity = targetRb.gravityScale;
            targetRb.gravityScale = 0f;
            targetRb.linearVelocity = Vector2.zero;
        }

        if (player != null)
        {
            player.enabled = false;
        }

        // Activez vizibilitatea mâinii și a degetului
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sprite = closedHandSprite;
        }

        if (fingerChild != null)
            fingerChild.SetActive(true);

        // 1. Poziționăm mâna sub ecran exact pe X-ul playerului
        Vector3 startPos = new Vector3(fallPosition.x, fallPosition.y + offscreenYOffset, 0f);
        transform.position = startPos;

        // 2. Mâna urcă pana la player
        while (targetObj != null && Vector3.Distance(transform.position, targetObj.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetObj.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 3. Găsim cel mai apropiat DropPoint de pe platformă pentru player
        Transform closestDropPoint = FindClosestDropPoint(fallPosition);
        Vector3 targetPlatformPos = closestDropPoint != null ? closestDropPoint.position + dropOffset : new Vector3(fallPosition.x, fallPosition.y + 5f, 0f);

        // 4. Mâna urcă spre platformă ținând jucătorul fix pe HoldPoint
        while (targetObj != null && Vector3.Distance(transform.position, targetPlatformPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPlatformPos, moveSpeed * Time.deltaTime);

            if (holdPoint != null)
            {
                targetObj.position = holdPoint.position;
            }
            else
            {
                targetObj.position = transform.position;
            }

            if (targetRb != null) targetRb.linearVelocity = Vector2.zero;
            yield return null;
        }

        // 5. Am ajuns pe platformă, asez jucătorul pe DropPoint
        if (targetObj != null)
        {
            if (closestDropPoint != null)
            {
                targetObj.position = closestDropPoint.position + dropOffset;
            }

            if (targetRb != null)
            {
                targetRb.gravityScale = originalGravity;
            }

            if (player != null)
            {
                player.enabled = true;
            }
        }

        // 6. Deschid mâna și ascund degetul
        if (spriteRenderer != null && openHandSprite != null)
            spriteRenderer.sprite = openHandSprite;

        if (fingerChild != null)
            fingerChild.SetActive(false);

        yield return new WaitForSeconds(0.4f);

        // 7. Mâna coboară înapoi sub ecran și se distruge
        Vector3 disappearPos = new Vector3(transform.position.x, transform.position.y + offscreenYOffset, 0f);
        while (Vector3.Distance(transform.position, disappearPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, disappearPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject); // Distrugem mâna după ce și-a terminat treaba
    }

    private Transform FindClosestDropPoint(Vector3 currentPos)
    {
        GameObject[] dropPoints = GameObject.FindGameObjectsWithTag("DropPoint");
        if (dropPoints.Length == 0) return null;

        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject dp in dropPoints)
        {
            float dist = Vector2.Distance(currentPos, dp.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = dp.transform;
            }
        }
        return closest;
    }
}