using UnityEngine;
using UnityEngine.UI;

public class SubRunControl : MonoBehaviour
{
    [Header("Movement")]
    public float laneWidth = 3f;
    public float groundY = -3f;
    public float laneSmoothTime = 0.25f;
    public float jumpVelocity = 12f;
    public float gravity = -30f;

    [Header("Health")]
    public float maxHP = 100f;
    public float damagePerHit = 5f;
    public float regenPerSecond = 1f;
    public Image hpBar;
    public Text hpText;

    [Header("Hit Feedback")]
    public Color normalColor = Color.white;
    public Color hitColor = Color.red;
    public float hitFrameTime = 0.25f;

    [HideInInspector] public Vector3 playerPosition;

    private SpriteRenderer sr;
    private int lane = 1;
    private float velocityX;
    private float jumpOffset, jumpVel;
    private float hp, hitTimer;

    public bool IsJumping {get {return jumpOffset > 0f; } }
        void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        hp = maxHP;
        playerPosition = new Vector3(0f, groundY, 0f);
    }
    void Update()
    {
        float dt = Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            lane = Mathf.Max(0, lane -1);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            lane = Mathf.Min(2, lane +1);

        float targetX = (lane - 1) * laneWidth;
        playerPosition.x = Mathf.SmoothDamp(playerPosition.x, targetX, ref velocityX, laneSmoothTime);

        if (Input.GetKeyDown(KeyCode.Space) && !IsJumping)
            jumpVel = jumpVelocity;

        if (IsJumping || jumpVel > 0f)
        {
            jumpVel += gravity * dt;
            jumpOffset += jumpVel * dt;
            if (jumpOffset <= 0)
            {
                jumpOffset = 0f;
                jumpVel = 0f;
            }
        }

        playerPosition.y = groundY + jumpOffset;
        transform.position = new Vector2(playerPosition.x, playerPosition.y);

        hp = Mathf.Min(maxHP, hp + regenPerSecond * dt);
        hitTimer = Mathf.Max(0f, hitTimer - dt); 

        if (sr != null)
            sr.color = hitTimer > 0f ? hitColor : normalColor;

        UpdateUI();
    }

    public void TakeHit()
    {
        hp = Mathf.Max(0f, hp - damagePerHit);
        hitTimer = hitFrameTime;
    }

    private void UpdateUI()
    {
        if (hpBar != null) hpBar.fillAmount = hp / maxHP;
        if (hpText != null) hpText.text = Mathf.CeilToInt(hp) + " / " + maxHP;
    }
}
