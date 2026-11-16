using UnityEngine;
using UnityEngine.SceneManagement;

public class LadyHead : MonoBehaviour
{
    [Header("Targets (Se detecteaza automat)")]
    public Transform[] players;
    private DistractionMonkey[] allMonkeys;

    [Header("Movement & Depth (Z) Settings")]
    public float normalSpeed = 3.5f;
    public float attackSpeed = 5.5f;           // Viteza mai mare cand e in modul de atac!
    public float monkeyAttractionSpeed = 7.0f;
    public float maxReachDistance = 15f;
    public float backgroundZ = 6f;
    public float attackZ = 0f;

    [Header("Neck Setup (Pivot jos la baza gatului!)")]
    public Transform neckTransform;
    public Transform neckAnchorPoint;

    [Header("Attack, Escape & Face Sprites")]
    public SpriteRenderer faceRenderer;
    public Sprite normalFaceSprite;
    public Sprite attackFaceSprite;
    public float triggerRadius = 2.2f;         // Raza la care INCEPE atacul (cercul rosu)
    public float escapeRadius = 4.8f;          // Raza dincolo de care trebuie sa fugi ca sa SCAPI (cercul portocaliu)
    public float catchTimeRequired = 5f;
    public float timerDecayRate = 1.5f;        // Cat de repede scade cronometrul dupa ce scapi (nu se reseteaza instant!)

    private float catchTimer = 0f;
    private bool isAttacking = false;

    void Start()
    {
        if (faceRenderer == null) faceRenderer = GetComponent<SpriteRenderer>();
        if (faceRenderer != null && normalFaceSprite != null)
            faceRenderer.sprite = normalFaceSprite;

        transform.position = new Vector3(transform.position.x, transform.position.y, backgroundZ);

        if (players == null || players.Length == 0)
        {
            PlayerController[] foundPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            players = new Transform[foundPlayers.Length];
            for (int i = 0; i < foundPlayers.Length; i++)
                players[i] = foundPlayers[i].transform;
        }

        allMonkeys = FindObjectsByType<DistractionMonkey>(FindObjectsSortMode.None);
    }

    void Update()
    {
        Transform currentTarget = GetCurrentTarget(out bool isChasingMonkey, out float closestPlayerDist);

        // Stabilim viteza in functie de stare: maimutica > atac > urmarire normala
        float currentSpeed = normalSpeed;
        if (isChasingMonkey)
            currentSpeed = monkeyAttractionSpeed;
        else if (isAttacking)
            currentSpeed = attackSpeed;

        // 1. Miscam capul pe X si Y spre tinta
        if (currentTarget != null)
        {
            Vector2 currentXY = new Vector2(transform.position.x, transform.position.y);
            Vector2 targetXY = new Vector2(currentTarget.position.x, currentTarget.position.y);

            Vector2 desiredXY = Vector2.MoveTowards(currentXY, targetXY, currentSpeed * Time.deltaTime);

            if (neckAnchorPoint != null)
            {
                Vector2 anchorXY = new Vector2(neckAnchorPoint.position.x, neckAnchorPoint.position.y);
                Vector2 offsetFromAnchor = desiredXY - anchorXY;
                if (offsetFromAnchor.magnitude > maxReachDistance)
                {
                    desiredXY = anchorXY + offsetFromAnchor.normalized * maxReachDistance;
                }
            }

            transform.position = new Vector3(desiredXY.x, desiredXY.y, transform.position.z);
        }

        // 2. Verificam starea de atac, cronometrul si adancimea Z
        CheckPlayerCatchAndDepth(isChasingMonkey, closestPlayerDist);

        // 3. Actualizam pozitia si rotatia gatului
        UpdateNeck();
    }

    private Transform GetCurrentTarget(out bool isChasingMonkey, out float closestPlayerDist)
    {
        closestPlayerDist = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (Transform p in players)
        {
            if (p == null) continue;
            float dist = Vector2.Distance(transform.position, p.position);
            if (dist < closestPlayerDist)
            {
                closestPlayerDist = dist;
                closestPlayer = p;
            }
        }

        // Cautam daca exista vreo maimuta ACTIVA in scena
        DistractionMonkey closestActiveMonkey = null;
        float minMonkeyDist = Mathf.Infinity;

        if (allMonkeys != null)
        {
            foreach (DistractionMonkey m in allMonkeys)
            {
                if (m != null && m.isActive)
                {
                    float dist = Vector2.Distance(transform.position, m.transform.position);
                    if (dist < minMonkeyDist)
                    {
                        minMonkeyDist = dist;
                        closestActiveMonkey = m;
                    }
                }
            }
        }

        if (closestActiveMonkey != null)
        {
            isChasingMonkey = true;
            return closestActiveMonkey.transform;
        }

        isChasingMonkey = false;
        return closestPlayer;
    }

    private void CheckPlayerCatchAndDepth(bool isDistracted, float closestPlayerDist)
    {
        // Daca e distrasa de maimuta, intrerupe imediat atacul
        if (isDistracted)
        {
            isAttacking = false;
        }
        else
        {
            // Porneste atacul la triggerRadius, dar se opreste DOAR daca playerul fuge dincolo de escapeRadius!
            if (!isAttacking && closestPlayerDist <= triggerRadius)
            {
                isAttacking = true;
            }
            else if (isAttacking && closestPlayerDist > escapeRadius)
            {
                isAttacking = false;
            }
        }

        if (isAttacking)
        {
            if (faceRenderer != null && attackFaceSprite != null)
                faceRenderer.sprite = attackFaceSprite;

            catchTimer += Time.deltaTime;

            if (catchTimer >= catchTimeRequired)
            {
                if (SceneTransitionManager.Instance != null)
                    SceneTransitionManager.Instance.TriggerLoseTransition();
                else
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        else
        {
            // Cronometrul scade treptat in loc sa se reseteze instant la 0
            catchTimer = Mathf.Max(0f, catchTimer - Time.deltaTime * timerDecayRate);

            if (faceRenderer != null && normalFaceSprite != null)
                faceRenderer.sprite = normalFaceSprite;
        }

        // Adancimea Z este legata direct de progresul cronometrului (iese in fata cand ataca, se retrage lin cand scapi)
        float progress = Mathf.Clamp01(catchTimer / catchTimeRequired);
        float targetZ = Mathf.Lerp(backgroundZ, attackZ, progress);
        transform.position = new Vector3(transform.position.x, transform.position.y, targetZ);
    }

    private void UpdateNeck()
    {
        if (neckTransform == null || neckAnchorPoint == null) return;

        neckTransform.position = transform.position;
        Vector3 dirToAnchor = neckAnchorPoint.position - transform.position;
        neckTransform.up = dirToAnchor.normalized;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f); // Portocaliu - raza de scapare
        Gizmos.DrawWireSphere(transform.position, escapeRadius);

        if (neckAnchorPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(neckAnchorPoint.position, maxReachDistance);
        }
    }
}