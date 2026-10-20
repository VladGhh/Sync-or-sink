using UnityEngine;

public class SplitScreenCamera: MonoBehaviour
{
    public Transform player1;
    public Transform player2;
    public float mergeThreshold = 5f;
    public Camera fullScreenCamera;
    public Camera splitScreenCamera1;
    public Camera splitScreenCamera2;

    private bool isMerged = false;

    private void Update()
    {
        // Calculate the distance between the players
        float distance = Vector3.Distance(player1.position, player2.position);

        if (distance >= mergeThreshold && isMerged)
        {
            // Switch to split-screen view
            fullScreenCamera.enabled = false;
            splitScreenCamera1.enabled = true;
            splitScreenCamera2.enabled = true;
            isMerged = false;
        }
        else if (distance < mergeThreshold && !isMerged)
        {
            // Switch to full-screen view
            fullScreenCamera.enabled = true;
            splitScreenCamera1.enabled = false;
            splitScreenCamera2.enabled = false;
            isMerged = true;
        }
    }
}
