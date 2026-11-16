using System.Collections;
using UnityEngine;

public class DistractionMonkey : Interactable
{
    
    public float distractionDuration = 5f;
    public bool isActive = false;

    public Animator monkeyAnimator;
    public AudioSource monkeyAudio;

    private Coroutine activeRoutine;

    public override void Interact(PlayerController player)
    {
        if (isActive) return;

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ActivateMonkeyRoutine());
    }

    private IEnumerator ActivateMonkeyRoutine()
    {
        isActive = true;
        if (monkeyAnimator != null) monkeyAnimator.SetBool("IsActive", true);
        if (monkeyAudio != null) monkeyAudio.Play();

        yield return new WaitForSeconds(distractionDuration);

        isActive = false;
        if (monkeyAnimator != null) monkeyAnimator.SetBool("IsActive", false);
        if (monkeyAudio != null) monkeyAudio.Stop();
    }
}