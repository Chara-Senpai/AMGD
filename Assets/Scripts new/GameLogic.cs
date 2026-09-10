using UnityEngine;
using UnityEngine.SceneManagement;
public class GameLogic : MonoBehaviour
{
    public Transform player, finishZone;
    public Transform[] noGoZones;
    public GameObject winUI;
    public float movespeed = 8f, warnDist = 5f, failDist = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 move = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
        player.Translate(move.normalized * movespeed * Time.deltaTime);

        foreach(Transform zone in noGoZones)
        {
            float dist = Vector2.Distance(player.position, zone.position);

            if (dist < warnDist)
            {
                zone.GetComponent<SpriteRenderer>().color = Color.red;
                zone.position += (Vector3)Random.insideUnitCircle * 0.05f;

                if (dist < failDist) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        } 
        if(Vector2.Distance(player.position, finishZone.position) < 1f) winUI.SetActive(true);
    }
}
