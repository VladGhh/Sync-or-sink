using UnityEngine;

public class EyeTracker : MonoBehaviour
{
    public Transform eyeCenter;
    public float radius = 0.25f;

    void LateUpdate()
    {
        Transform closestPlayer = GetClosestPlayer();
        if (closestPlayer == null || eyeCenter == null) return;

        Vector2 dir = ((Vector2)closestPlayer.position - (Vector2)eyeCenter.position).normalized;
        Vector2 targetPos = (Vector2)eyeCenter.position + dir * radius;

        transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
    }

    Transform GetClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject p in players)
        {
            float dist = Vector2.Distance(eyeCenter.position, p.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = p.transform;
            }
        }
        return closest;
    }
}