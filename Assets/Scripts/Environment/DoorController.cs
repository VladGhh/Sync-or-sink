using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Settings")]
    public string nextSceneName;              // Numele scenei următoare (adăugată în Build Settings/Profiles)

    [System.Serializable]
    public class PadlockData
    {
        public GameObject padlockObject;      // Obiectul lacătului (copil al ușii)
        public string requiredPlayerName;     // Numele exact al playerului din Hierarchy (ex: "Lady")
        public Sprite unlockedSprite;         // Sprite-ul când cade
        [HideInInspector] public bool isUnlocked = false;
    }

    public PadlockData[] padlocks;            

    private bool isTransitioning = false;

    // Detectează când un player intră în coliderul ușii
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTransitioning) return;

        string playerName = collision.gameObject.name;

        foreach (var pad in padlocks)
        {
            if (!pad.isUnlocked && playerName == pad.requiredPlayerName)
            {
                UnlockPadlock(pad);
                break;
            }
        }
    }

    private void UnlockPadlock(PadlockData pad)
    {
        pad.isUnlocked = true;

        // Schimbăm sprite-ul lacătului
        SpriteRenderer sr = pad.padlockObject.GetComponent<SpriteRenderer>();
        if (sr != null && pad.unlockedSprite != null)
            sr.sprite = pad.unlockedSprite;

        // Activăm gravitația ca lacătul să cadă
        Rigidbody2D rb = pad.padlockObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2f;
        }

        CheckAllPadlocks();
    }

    private void CheckAllPadlocks()
    {
        bool allDone = true;
        foreach (var pad in padlocks)
        {
            if (!pad.isUnlocked)
            {
                allDone = false;
                break;
            }
        }

        // Dacă ambele lacăte s-au deschis, pornim tranziția spre noua scenă
        if (allDone)
        {
            isTransitioning = true;
            StartCoroutine(LoadNextSceneRoutine());
        }
    }

    private IEnumerator LoadNextSceneRoutine()
    {
        yield return new WaitForSeconds(0.8f); // Timp ca lacătele să cadă vizual puțin

        // Apelăm managerul de tranziții ca să facă ecranul negru și efectul pe noua hartă
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TriggerLevelTransition(nextSceneName);
        }
        else
        {
            Debug.LogError("Lipsește SceneTransitionManager din scenă!");
        }
    }
}