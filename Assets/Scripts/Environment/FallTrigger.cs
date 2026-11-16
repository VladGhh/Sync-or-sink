using UnityEngine;

public class FallTrigger : MonoBehaviour
{
    public GameObject rescueHandPrefab;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip[] playerFallSounds;  // Vector de sunete random pentru Player
    public AudioClip[] objectFallSounds;  // Vector de sunete random pentru Box / PickableItem
    public float minTimeBetweenSounds = 0.3f; // Cooldown extra ca sa nu se declanseze doua sunete in acelasi frame

    private float lastSoundTime = -10f;

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

            // Redãm sunetul potrivit (Player vs. alt obiect)
            PlayFallSound(player != null);

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

    private void PlayFallSound(bool isPlayer)
    {
        if (audioSource == null) return;

        // Prevenim suprapunerea: daca deja canta un sunet sau nu a trecut cooldown-ul, iesim
        if (audioSource.isPlaying || Time.time - lastSoundTime < minTimeBetweenSounds)
            return;

        AudioClip[] targetPool = isPlayer ? playerFallSounds : objectFallSounds;

        if (targetPool != null && targetPool.Length > 0)
        {
            AudioClip chosenClip = targetPool[Random.Range(0, targetPool.Length)];
            if (chosenClip != null)
            {
                // Folosim .clip + .Play() (in loc de PlayOneShot) ca audioSource.isPlaying sa stie exact cand s-a terminat sunetul
                audioSource.clip = chosenClip;
                audioSource.Play();
                lastSoundTime = Time.time;
            }
        }
    }
}