using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public enum GameState { Playing, Won, Lost }
    public GameState State { get; private set; }

    [Header("Player HP")]
    public int maxHp = 20;
    public int Hp { get; private set; }
    public HPBar hpBar;

    [Header("Coins")]
    public BankUI bank;
    public CoinFly coinPrefab;
    public RectTransform coinLayer;
    public int coinsPerKill = 1;

    [Header("UI")]
    public EnemySpawner spawner;
    public GameObject winPanel, losePanel;
    public TMP_Text enemiesLeftText;

    void Awake()
    {
        I = this;
        Hp = maxHp;
        State = GameState.Playing;
        Time.timeScale = 1f;
    }

    void Start()
    {
        hpBar.Init(maxHp);
        hpBar.SetHp(Hp);
        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) Restart();

        if (enemiesLeftText)
            enemiesLeftText.text = "Enemies: " + (spawner.RemainingToSpawn + Enemy.All.Count);

        if (State == GameState.Playing && spawner.Finished && Enemy.All.Count == 0)
        {
            State = GameState.Won;
            if (winPanel) winPanel.SetActive(true);
        }
    }

    public void DamagePlayer(int amount)
    {
        if (State != GameState.Playing) return;
        Hp = Mathf.Max(0, Hp - amount);
        hpBar.SetHp(Hp);
        if (Hp <= 0)
        {
            State = GameState.Lost;
            if (losePanel) losePanel.SetActive(true);
        }
    }

    public void OnEnemyKilled(Vector3 worldPos)
    {
        Vector3 screen = Camera.main.WorldToScreenPoint(worldPos);
        CoinFly coin = Instantiate(coinPrefab, coinLayer);
        coin.transform.position = screen;
        coin.Fly(bank.coinTarget, coinsPerKill, bank);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}