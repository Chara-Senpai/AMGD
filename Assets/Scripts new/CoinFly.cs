using System.Collections;
using UnityEngine;

public class CoinFly : MonoBehaviour
{
    public float delay = 0.1f;
    public float duration = 0.7f;
    public float arcHeight = 90f;

    public void Fly(RectTransform target, int value, BankUI bank)
    {
        StartCoroutine(Run(target, value, bank));
    }

    IEnumerator Run(RectTransform target, int value, BankUI bank)
    {
        Vector3 start = transform.position;
        yield return new WaitForSeconds(delay);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = Easing.InQuad(t);
            Vector3 p = Vector3.Lerp(start, target.position, k);
            p.y += Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * arcHeight * (1f - k);
            transform.position = p;
            yield return null;
        }

        bank.AddCoins(value);
        Destroy(gameObject);
    }
}