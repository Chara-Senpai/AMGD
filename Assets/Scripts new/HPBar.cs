using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public Image realFill, ghostFill;
    public TMP_Text label;
    public float ghostDelay = 0.5f;
    public float ghostDuration = 0.6f;

    private float max = 20f, real = 1f, ghost = 1f;
    private float easeFrom, holdTimer, easeTimer;

    public void Init(int maxHp)
    {
        max = maxHp;
        real = ghost = 1f;
        Apply(maxHp);
    }

    public void SetHp(int hp)
    {
        float n = Mathf.Clamp01(hp / max);

        if (n < real)
        {
            easeFrom = ghost;
            holdTimer = ghostDelay;
            easeTimer = 0f;
        }
        else
        {
            ghost = n;
        }

        real = n;
        Apply(hp);
    }

    void Update()
    {
        if (ghost > real)
        {
            if (holdTimer > 0f) holdTimer -= Time.deltaTime;
            else
            {
                easeTimer += Time.deltaTime;
                float k = Easing.OutCubic(easeTimer / ghostDuration);
                ghost = Mathf.Lerp(easeFrom, real, k);
                if (easeTimer >= ghostDuration) ghost = real;
            }
        }
        ghostFill.fillAmount = ghost;
    }

    void Apply(int hp)
    {
        realFill.fillAmount = real;
        ghostFill.fillAmount = ghost;
        if (label) label.text = "HP " + hp + "/" + (int)max;
    }
}