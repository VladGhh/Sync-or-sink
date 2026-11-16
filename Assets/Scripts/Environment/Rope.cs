using UnityEngine;

public class Rope : MonoBehaviour
{
    
    public Transform anchorPoint;      // Top point where the rope hangs from
    public Rigidbody2D attachedBox;    // The box hanging from the rope

    private DistanceJoint2D boxJoint;
    private bool isCut = false;

    void Start()
    {
        if (attachedBox != null)
        {
            boxJoint = attachedBox.GetComponent<DistanceJoint2D>();
        }
    }

    void Update()
    {
        // Keep the rope stretched, positioned, and rotated between the anchor and the box
        if (!isCut && anchorPoint != null && attachedBox != null)
        {
            Vector3 anchorPos = anchorPoint.position;
            Vector3 boxPos = attachedBox.transform.position;

            // Position the rope halfway between the two points
            transform.position = (anchorPos + boxPos) / 2f;

            // Rotate the rope to point towards the box
            Vector3 direction = boxPos - anchorPos;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

            // Scale the rope height to match the distance (works best with a 1x1 unit Square sprite)
            float distance = direction.magnitude;
            transform.localScale = new Vector3(transform.localScale.x, distance, 1f);
        }
    }

    public void CutRope()
    {
        if (isCut) return;
        isCut = true;

        // 1. Destroy the joint on the box so it falls
        if (boxJoint != null)
        {
            Destroy(boxJoint);
        }

        // 2. Disable the rope object
        gameObject.SetActive(false);
    }
}