using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour
{
    public Transform player, finishZone;
    public Transform[] noGoZones;
    public List<Transform> powerUps;
    public GameObject winUI, rocketPrefab;
    public float movespeed = 8f, warnDist = 5f, failDist = 1f, pickupDist = 0.8f, fireInterval = 3f, rocketSpeed = 8f;
    public int rocketCount = 4, maxRockets = 8;
    
    private float fireTimer;
    private List<(Transform obj, Vector3 dir, float time)> rockets = new();

    void Update()
    {
        // 1. Cardinal Movement
        float h = Input.GetAxisRaw("Horizontal"), v = Input.GetAxisRaw("Vertical");
        Vector3 move = h != 0 ? new Vector3(h, 0, 0) : new Vector3(0, v, 0);
        player.Translate(move.normalized * movespeed * Time.deltaTime);

        // 2. Zone & Win Logic
        foreach (Transform z in noGoZones) if (z && Vector2.Distance(player.position, z.position) < warnDist) {
            z.GetComponent<SpriteRenderer>().color = Color.red;
            z.position += (Vector3)Random.insideUnitCircle * 0.05f;
            if (Vector2.Distance(player.position, z.position) < failDist) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        if (finishZone && Vector2.Distance(player.position, finishZone.position) < 1f) winUI.SetActive(true);

        // 3. Power-Ups
        for (int i = powerUps.Count - 1; i >= 0; i--) if (powerUps[i] && Vector2.Distance(player.position, powerUps[i].position) <= pickupDist) {
            if (rocketCount < maxRockets) rocketCount++;
            Destroy(powerUps[i].gameObject); powerUps.RemoveAt(i);
        }

        // 4. Pure-Radian Rocket Barrage System
        if ((fireTimer += Time.deltaTime) >= fireInterval) {
            fireTimer = 0f;
            float step = (2f * Mathf.PI) / rocketCount, offset = Mathf.PI / 4f; // Full circle = 2*PI, offset = 45 deg in rads
            for (int i = 0; i < rocketCount; i++) {
                float theta = offset + (i * step);
                Vector3 dir = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0f); // x = cos(theta), y = sin(theta)
                GameObject r = Instantiate(rocketPrefab, player.position, Quaternion.Euler(0, 0, theta * Mathf.Rad2Deg));
                rockets.Add((r.transform, dir, 0f));
            }
        }

        // 5. Rocket Lifetime & Movement
        for (int i = rockets.Count - 1; i >= 0; i--) {
            rockets[i] = (rockets[i].obj, rockets[i].dir, rockets[i].time + Time.deltaTime);
            rockets[i].obj.position += rockets[i].dir * rocketSpeed * Time.deltaTime;
            if (rockets[i].time >= 3f) { Destroy(rockets[i].obj.gameObject); rockets.RemoveAt(i); }
        }
    }
}