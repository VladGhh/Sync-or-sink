using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public float promptHeight = 0.5f;  // cat de sus apare promptul peste obiect

    private TextMeshPro prompt;
    private int p1Frame = -1;
    private int p2Frame = -1;

    public virtual void Interact(PlayerController player)
    {
        Debug.Log("Interacting with " + gameObject.name);
    }

    // playerul cheama asta in fiecare frame cat e aproape
    public void ShowPrompt(bool isPlayer1)
    {
        if (isPlayer1) p1Frame = Time.frameCount;
        else p2Frame = Time.frameCount;
    }

    // daca niciun player nu a cerut promptul frame ul asta il ascundem
    void LateUpdate()
    {
        bool p1 = p1Frame == Time.frameCount;
        bool p2 = p2Frame == Time.frameCount;
        if (!p1 && !p2)
        {
            if (prompt != null) prompt.gameObject.SetActive(false);
            return;
        }

        if (prompt == null)
        {
            prompt = new GameObject("Prompt").AddComponent<TextMeshPro>();
            prompt.transform.SetParent(transform, false);
            prompt.alignment = TextAlignmentOptions.Center;
            prompt.fontSize = 14;
            prompt.fontStyle = FontStyles.Bold;
            prompt.sortingOrder = 100;
            // alb cu contur negru gros ca sa se vada si pe fundal deschis si pe fundal inchis
            prompt.color = Color.white;
            prompt.outlineColor = Color.black;
            prompt.outlineWidth = 0.35f;
            prompt.fontMaterial.EnableKeyword("OUTLINE_ON");  // fara asta shaderul fontului nu deseneaza conturul
        }

        prompt.text = p1 && p2 ? "E / Shift" : (p1 ? "E" : "Shift");

        // il tinem drept si de marime normala chiar daca obiectul e rotit sau scalat
        Vector3 s = transform.lossyScale;
        prompt.transform.localScale = new Vector3(1f / s.x, 1f / s.y, 1f);
        prompt.transform.rotation = Quaternion.identity;

        Collider2D col = GetComponent<Collider2D>();
        Bounds b = col != null ? col.bounds : new Bounds(transform.position, Vector3.zero);
        prompt.transform.position = new Vector3(b.center.x, b.max.y + promptHeight, transform.position.z);
        prompt.gameObject.SetActive(true);
    }
}
