using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class TutorialLine
{
    [TextArea(2, 4)]
    public string subtitleText;       // Textul subtitrarii
    public AudioClip voiceClip;       // Sunetul replicii
    public Sprite rightHandSprite;    // Sprite-ul mainii principale (dreapta)
    public Sprite leftHandSprite;     // Sprite-ul mainii stangi (optional per replica)
    public float fallbackDuration = 3f; // Durata folosita daca inca nu ai pus un AudioClip
}

public class TutorialIntroManager : MonoBehaviour
{
    [Header("Tutorial Lines (Replicile Mainilor)")]
    public TutorialLine[] lines;
    public bool allowSkipWithEnter = true;  // Permite skip la dialog cu Enter

    [Header("Cameras & Transition")]
    public Camera introCamera;              // Camera de intro (se misca la playerCamera si se distruge)
    public Camera playerCamera;             // Camera din Player (preia controlul la final)
    public float cameraMoveDuration = 1.0f; // Cat dureaza sa alunece camera pana la player

    [Header("Hands Visuals & Exit Animation")]
    public SpriteRenderer rightHandRenderer; // Mana naratorului
    public SpriteRenderer leftHandRenderer;  // Mana stanga
    public float handsExitDuration = 0.35f;  // Cat de repede fug in jos (secunde)
    public float handsDropDistance = 12f;    // Cat de mult coboara pe axa Y ca sa iasa din ecran

    [Header("Subtitles & Audio")]
    public TMP_Text subtitleTextUI;
    public GameObject subtitlePanel;
    public AudioSource voiceAudioSource;

    [Header("Intro Invisible Walls")]
    public GameObject[] introWalls;

    [Header("Background Curtain")]
    public float behindPlayersZ = 0.5f;
    public float levelFadeOutDuration = 1.5f;

    private bool isInDialogPhase = false;
    private bool skipRequested = false;

    void Start()
    {
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (subtitleTextUI != null) subtitleTextUI.text = "";

        if (introCamera != null)
            introCamera.gameObject.SetActive(true);
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        SetIntroWallsActive(true);

        StartCoroutine(PlayIntroSequence());
    }

    void Update()
    {
        // Detectam apasarea tastei Enter (atat Enter principal cat si cel de pe Numpad)
        if (allowSkipWithEnter && isInDialogPhase && !skipRequested)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                skipRequested = true;
                if (voiceAudioSource != null)
                    voiceAudioSource.Stop();
            }
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        Camera activeCam = introCamera != null ? introCamera : Camera.main;

        // 1. Cream peretele alb in spatele playerilor (la Z = 0.5)
        GameObject bgQuad = CreateBehindPlayersBackground(activeCam);
        Material quadMat = bgQuad != null ? bgQuad.GetComponent<Renderer>().material : null;

        SetupHandAtStart(rightHandRenderer);
        SetupHandAtStart(leftHandRenderer);

        isInDialogPhase = true;

        // Mic delay initial (intreruptibil daca apasa Enter instant)
        float initTimer = 0f;
        while (initTimer < 0.5f && !skipRequested)
        {
            initTimer += Time.deltaTime;
            yield return null;
        }

        // 2. Parcurgem fiecare replica pe rand
        if (!skipRequested && subtitlePanel != null)
            subtitlePanel.SetActive(true);

        foreach (TutorialLine line in lines)
        {
            if (skipRequested) break;

            if (subtitleTextUI != null)
                subtitleTextUI.text = line.subtitleText;

            if (rightHandRenderer != null && line.rightHandSprite != null)
                rightHandRenderer.sprite = line.rightHandSprite;

            if (leftHandRenderer != null && line.leftHandSprite != null)
                leftHandRenderer.sprite = line.leftHandSprite;

            float lineDuration = line.fallbackDuration;

            if (voiceAudioSource != null && line.voiceClip != null)
            {
                voiceAudioSource.clip = line.voiceClip;
                voiceAudioSource.Play();
                lineDuration = line.voiceClip.length;
            }

            // Asteptam durata replicii + pauza, dar verificam in fiecare frame daca s-a apasat Enter
            float totalWait = lineDuration + 0.25f;
            float timer = 0f;
            while (timer < totalWait && !skipRequested)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }

        isInDialogPhase = false;

        if (subtitleTextUI != null) subtitleTextUI.text = "";
        if (subtitlePanel != null) subtitlePanel.SetActive(false);

        // 3. Ambele maini fug repede in jos in afara ecranului
        yield return StartCoroutine(DropHandsDown());

        // 4. IntroCamera aluneca lin pana la pozitia (si zoom-ul) lui PlayerCamera, apoi se distruge!
        if (introCamera != null && playerCamera != null)
        {
            Vector3 startPos = introCamera.transform.position;
            float startSize = introCamera.orthographicSize;

            float camTimer = 0f;
            while (camTimer < cameraMoveDuration)
            {
                camTimer += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, camTimer / cameraMoveDuration);

                introCamera.transform.position = Vector3.Lerp(startPos, playerCamera.transform.position, t);

                if (introCamera.orthographic && playerCamera.orthographic)
                    introCamera.orthographicSize = Mathf.Lerp(startSize, playerCamera.orthographicSize, t);

                yield return null;
            }

            AttachQuadToCamera(bgQuad, playerCamera);

            playerCamera.gameObject.SetActive(true);
            Destroy(introCamera.gameObject);
        }

        // 5. Dezactivam zidurile invizibile
        SetIntroWallsActive(false);

        // 6. Fundalul din spatele playerilor face fade out
        float fadeTimer = 0f;
        while (fadeTimer < levelFadeOutDuration)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeTimer / levelFadeOutDuration);

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

    private void SetupHandAtStart(SpriteRenderer hand)
    {
        if (hand == null) return;
        Vector3 pos = hand.transform.position;
        hand.transform.position = new Vector3(pos.x, pos.y, 0f);
        hand.gameObject.SetActive(true);
    }

    private IEnumerator DropHandsDown()
    {
        Vector3 rightStart = rightHandRenderer != null ? rightHandRenderer.transform.position : Vector3.zero;
        Vector3 leftStart = leftHandRenderer != null ? leftHandRenderer.transform.position : Vector3.zero;

        Vector3 rightEnd = rightStart + Vector3.down * handsDropDistance;
        Vector3 leftEnd = leftStart + Vector3.down * handsDropDistance;

        float timer = 0f;
        while (timer < handsExitDuration)
        {
            timer += Time.deltaTime;
            float t = timer / handsExitDuration;
            float smoothT = t * t;

            if (rightHandRenderer != null)
                rightHandRenderer.transform.position = Vector3.Lerp(rightStart, rightEnd, smoothT);

            if (leftHandRenderer != null)
                leftHandRenderer.transform.position = Vector3.Lerp(leftStart, leftEnd, smoothT);

            yield return null;
        }

        if (rightHandRenderer != null) rightHandRenderer.gameObject.SetActive(false);
        if (leftHandRenderer != null) leftHandRenderer.gameObject.SetActive(false);
    }

    private void SetIntroWallsActive(bool state)
    {
        if (introWalls == null) return;
        foreach (GameObject wall in introWalls)
        {
            if (wall != null)
                wall.SetActive(state);
        }
    }

    private GameObject CreateBehindPlayersBackground(Camera cam)
    {
        if (cam == null) return null;

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "IntroBackgroundQuad";
        Destroy(quad.GetComponent<Collider>());

        AttachQuadToCamera(quad, cam);

        Renderer rend = quad.GetComponent<Renderer>();
        Shader spriteShader = Shader.Find("Sprites/Default");
        if (spriteShader != null)
            rend.material = new Material(spriteShader);

        rend.material.color = Color.white;
        return quad;
    }

    private void AttachQuadToCamera(GameObject quad, Camera cam)
    {
        if (quad == null || cam == null) return;

        quad.transform.SetParent(cam.transform);
        float distanceFromCam = Mathf.Abs(behindPlayersZ - cam.transform.position.z);
        quad.transform.localPosition = new Vector3(0f, 0f, distanceFromCam);
        quad.transform.localRotation = Quaternion.identity;
        quad.transform.localScale = new Vector3(100f, 100f, 1f);
    }
}