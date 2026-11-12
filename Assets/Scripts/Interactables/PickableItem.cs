using UnityEngine;

public class PickableItem : Interactable
{
    public Vector3 holdOffset = new Vector3(0.5f, 0.2f, 0f);
    public bool isHeld = false;
    private PlayerController holder = null;
    private Rigidbody2D rb;
    private Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public override void Interact(PlayerController player)
    {
        if (!isHeld && !player.isCarryingItem)
        {
            PickUp(player);
        }
    }

    public void PickUp(PlayerController player)
    {
        isHeld = true;
        holder = player;
        player.isCarryingItem = true;

        if (rb != null) rb.simulated = false;
        if (col != null) col.enabled = false;

        transform.SetParent(player.transform);
        transform.localPosition = holdOffset;
    }

    public void Drop()
    {
        if (holder != null)
        {
            holder.isCarryingItem = false;
        }

        isHeld = false;
        holder = null;
        transform.SetParent(null);

        if (rb != null) rb.simulated = true;
        if (col != null) col.enabled = true;
    }
}