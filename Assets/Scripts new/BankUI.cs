using TMPro;
using UnityEngine;

public class BankUI : MonoBehaviour
{
    public RectTransform coinTarget;
    public TMP_Text label;
    public int startCoins = 0;
    public float countDuration = 0.5f;
    public float punchScale = 0.35f;
    public float punchDuration = 0.3f;

    private int target;
    private float shown, from, countT = 1f, punchT = 1f;
    private Vector3 baseScale;

    void Start()
    {
        baseScale = coinTarget.localScale;
        target = startCoins;
        shown = from = startCoins;
        label.text = "Bank: " + startCoins;
    }

    public void AddCoins(int n)
    {
        target += n;
        from = shown;
        countT = 0f;
        punchT = 0f;
    }

    void Update()
    {
        if (countT < 1f)
        {
            countT += Time.deltaTime / countDuration;
            shown = Mathf.Lerp(from, target, Easing.OutCubic(countT));
        }
        else shown = target;
        label.text = "Bank: " + Mathf.RoundToInt(shown);

        if (punchT < 1f)
        {
            punchT += Time.deltaTime / punchDuration;
            coinTarget.localScale = baseScale * (1f + punchScale * Mathf.Sin(Mathf.Clamp01(punchT) * Mathf.PI));
        }
        else coinTarget.localScale = baseScale;
    }
}