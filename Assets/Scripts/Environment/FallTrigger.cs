using UnityEngine;

public class FallTrigger : MonoBehaviour
{
    public GameObject rescueHandPrefab; 

    void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        Box box = collision.GetComponent<Box>();
        PickableItem item = collision.GetComponent<PickableItem>();

        if (player != null || box != null || item != null)
        {
            // Dacã obiectul are deja o mânã care îl salveazã, sã nu spawnãm alta 
            if (collision.transform.parent != null && collision.transform.parent.GetComponent<RescueHand>() != null)
                return;

            if (rescueHandPrefab != null)
            {
                
                GameObject handObj = Instantiate(rescueHandPrefab, collision.transform.position, Quaternion.identity);
                RescueHand handScript = handObj.GetComponent<RescueHand>();

                if (handScript != null)
                {
                    handScript.InitRescue(collision.transform, collision.transform.position);
                }
            }
        }
    }
}