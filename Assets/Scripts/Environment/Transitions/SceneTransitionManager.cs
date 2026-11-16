using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Black Screen UI")]
    public CanvasGroup blackScreenOverlay;
    public string playersLayerName = "Players";

    [Header("Audio & Timing")]
    public AudioSource audioSource;
    public AudioClip lightsOffSound;
    public float blackScreenHoldTime = 1.2f;   // Cat sta bezna totala
    public float playersOnlyDuration = 0.8f;   // Cat timp se vad doar playerii pe negru
    public float fadeOutDuration = 1.2f;       // Cat dureaza fade-ul peretelui negru

    private bool isTransitioning = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (blackScreenOverlay != null)
            blackScreenOverlay.alpha = 0f;
    }

    public void TriggerLoseTransition()
    {
        if (isTransitioning) return;
        StartCoroutine(LoseRoutine());
    }

    // Funcție nouă pentru trecerea la un nivel/scenă nouă prin ușă
    public void TriggerLevelTransition(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(LevelTransitionRoutine(sceneName));
    }

    private IEnumerator LoseRoutine()
    {
        isTransitioning = true;

        if (blackScreenOverlay != null) blackScreenOverlay.alpha = 1f;
        if (audioSource != null && lightsOffSound != null)
            audioSource.PlayOneShot(lightsOffSound);

        yield return new WaitForSeconds(blackScreenHoldTime);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        while (!asyncLoad.isDone)
            yield return null;

        yield return StartCoroutine(FadeOutBlackCurtainRoutine());
        isTransitioning = false;
    }

    private IEnumerator LevelTransitionRoutine(string sceneName)
    {
        isTransitioning = true;

        // 1. Ecran negru instant + sunet
        if (blackScreenOverlay != null) blackScreenOverlay.alpha = 1f;
        if (audioSource != null && lightsOffSound != null)
            audioSource.PlayOneShot(lightsOffSound);

        yield return new WaitForSeconds(blackScreenHoldTime);

        // 2. Încărcăm noua scenă asincron
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
            yield return null;

        yield return StartCoroutine(FadeOutBlackCurtainRoutine());
        isTransitioning = false;
    }

    private IEnumerator FadeOutBlackCurtainRoutine()
    {
        Camera cam = Camera.main;

        // 3. Creăm peretele negru în spatele playerilor pe noua scenă (la Z = 0.5)
        GameObject bgQuad = CreateBehindPlayersFadeQuad(cam);
        Material quadMat = bgQuad != null ? bgQuad.GetComponent<Renderer>().material : null;

        // Ridicăm cortina UI -> se văd DOAR playerii pe negru
        if (blackScreenOverlay != null) blackScreenOverlay.alpha = 0f;

        yield return new WaitForSeconds(playersOnlyDuration);

        // 4. Facem fade-out peretelui negru din spatele playerilor
        float timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);

            if (quadMat != null)
            {
                Color c = quadMat.color;
                c.a = alpha;
                quadMat.color = c;
            }
            yield return null;
        }

        if (bgQuad != null) Destroy(bgQuad);
    }

    private GameObject CreateBehindPlayersFadeQuad(Camera cam)
    {
        if (cam == null) return null;

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "BehindPlayersBlackout";
        Destroy(quad.GetComponent<Collider>());

        quad.transform.SetParent(cam.transform);
        float targetWorldZ = 0.5f;
        float distanceFromCam = Mathf.Abs(targetWorldZ - cam.transform.position.z);
        quad.transform.localPosition = new Vector3(0f, 0f, distanceFromCam);
        quad.transform.localRotation = Quaternion.identity;
        quad.transform.localScale = new Vector3(100f, 100f, 1f);

        Renderer rend = quad.GetComponent<Renderer>();
        Shader spriteShader = Shader.Find("Sprites/Default");
        if (spriteShader != null)
        {
            rend.material = new Material(spriteShader);
        }
        rend.material.color = new Color(0f, 0f, 0f, 1f);

        return quad;
    }
}