using UnityEngine;
using UnityEngine.SceneManagement;

public class TurretLevel : MonoBehaviour
{
    public Transform player, goal;
    public GameObject winUI, bulletPrefab;
    public float moveSpeed = 8f;
    public Turret[] turrets;

    private bool isGameWon = false;

    [System.Serializable]
    public class Turret {
        public Transform t;
        public enum Type { Flame, Sniper, Shotgun }
        public Type type;
        public float range = 6f, angle = 45f, fireRate = 1f;
        public LineRenderer line;
        [HideInInspector] public float timer;
    }

    void Update() {
        foreach (var tur in turrets) {
            if (tur.line) tur.line.useWorldSpace = true;
            DrawRange(tur);
        }

        if (isGameWon) return;

        float h = Input.GetAxisRaw("Horizontal"), v = Input.GetAxisRaw("Vertical");
        Vector3 move = h != 0 ? new Vector3(h, 0, 0) : new Vector3(0, v, 0);
        player.Translate(move.normalized * moveSpeed * Time.deltaTime);

        if (goal && Vector2.Distance(player.position, goal.position) < 1f) {
            isGameWon = true;
            winUI.SetActive(true); 
            return;
        }

        foreach (var tur in turrets) {
            if (tur.t == null) continue;
            
            Vector3 dirToPlayer = (player.position - tur.t.position).normalized;
            float dist = Vector2.Distance(player.position, tur.t.position);
            float angleToPlayer = Vector3.Angle(tur.t.up, dirToPlayer);
            bool inRange = dist <= tur.range && angleToPlayer <= (tur.angle / 2f);

            if (tur.type == Turret.Type.Sniper) {
                inRange = dist <= tur.range && angleToPlayer <= 5f;
            }

            if (inRange && (tur.timer += Time.deltaTime) >= tur.fireRate) {
                tur.timer = 0f;

                if (tur.type == Turret.Type.Flame) {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                } 
                else if (tur.type == Turret.Type.Sniper) {
                    SpawnBullet(tur.t.position, dirToPlayer);
                } 
                else if (tur.type == Turret.Type.Shotgun) {
                    for (int i = -2; i <= 2; i++) {
                        float rad = (tur.t.eulerAngles.z + (i * 10f)) * Mathf.Deg2Rad;
                        SpawnBullet(tur.t.position, new Vector3(-Mathf.Sin(rad), Mathf.Cos(rad), 0f));
                    }
                }
            }
        }
    }

    void SpawnBullet(Vector3 pos, Vector3 dir) {
        GameObject b = Instantiate(bulletPrefab, pos, Quaternion.identity);
        b.AddComponent<SimpleBullet>().Setup(dir, player);
    }

    void DrawRange(Turret tur) {
        if (!tur.line || !tur.t) return;
        
        tur.line.sortingOrder = 10;
        int pts = 20; 
        tur.line.positionCount = pts + 2;
        tur.line.SetPosition(0, tur.t.position);
        
        float baseAngle = tur.t.eulerAngles.z;
        float startAngle = baseAngle - (tur.angle / 2f);
        
        for (int i = 0; i <= pts; i++) {
            float currentAngle = startAngle + (i * (tur.angle / pts));
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(-Mathf.Sin(rad), Mathf.Cos(rad), 0f);
            Vector3 pos = tur.t.position + dir * tur.range;
            tur.line.SetPosition(i + 1, pos);
        }
    }
}

public class SimpleBullet : MonoBehaviour {
    private Vector3 dir; 
    private Transform player;

    public void Setup(Vector3 d, Transform p) { 
        dir = d; 
        player = p; 
        Destroy(gameObject, 4f); 
    }

    void Update() {
        transform.position += dir * 10f * Time.deltaTime;
        
        if (player && Vector2.Distance(transform.position, player.position) < 0.5f) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}