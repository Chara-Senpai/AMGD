using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ObstacleType
{
    public Item prefab;
    public float weight = 1f;
    public int prewarm = 8;
}
public class ItemSpawner : MonoBehaviour
{
    public List<ObstacleType> obstacleTypes = new List<ObstacleType>();
    public Transform Parent;
    public SubRunControl player;

    [Header ("Spawning")]
    public float spawnZ = 600f;
    public float despawnZ = -100f;
    public float speed = 200f;
    public float spawnInterval = 0.8f;

    [Header ("Hit Direction Ranges")]
    public float hitDepth = 15f;
    public float hitWidth = 1.2f;

    private List<Item> active = new List<Item>();
    private List<Stack<Item>> pools = new  List<Stack<Item>>();
    private float timer;
    void Start()
    {
        for (int t = 0; t < obstacleTypes.Count; t++)
        {
            Stack<Item> pool = new Stack<Item>();
            for (int i = 0; i < obstacleTypes[t].prewarm; i++)
            {
                Item item = Instantiate(obstacleTypes[t].prefab, Parent);
                item.poolIndex = t;
                item.gameObject.SetActive(false);
                pool.Push(item);
            }
            pools.Add(pool);
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        timer += dt;
        if(timer >= spawnInterval)
        {
            timer -= spawnInterval;
            Spawn();
        }

        for (int i = active.Count - 1; i >= 0; i--)
        {
            Item item = active[i];
            item.itemPosition.z -= speed * dt;

            CheckHit(item);

            if (item.itemPosition.z < despawnZ)
            {
                active.RemoveAt(i);
                Release(item);
            } 
        }
    }

   private int PickType()
{
    float total = 0f;
    for (int i = 0; i < obstacleTypes.Count; i++)
        total += obstacleTypes[i].weight;

    float roll = Random.Range(0f, total);
    for (int i = 0; i < obstacleTypes.Count; i++)
    {
        roll -= obstacleTypes[i].weight;
        if (roll <= 0f) return i;
    }
    return obstacleTypes.Count - 1;
}

    private Item Get(int typeIndex)
    {
        Stack<Item> pool = pools[typeIndex];
        Item item;
        if (pool.Count > 0)
        {
            item = pool.Pop();
        }
        else
        {
            item = Instantiate(obstacleTypes[typeIndex].prefab, Parent);
            item.poolIndex = typeIndex;
        }
        item.gameObject.SetActive(true);
        return item;
    }

    private void Release(Item item)
    {
        item.gameObject.SetActive(false);
        pools[item.poolIndex].Push(item);
    }

    private void Spawn()
    {
        if (obstacleTypes.Count == 0) return;
        Item spawned = Get(PickType());
        int lane = Random.Range(0, 3);
        float x = (lane - 1) * player.laneWidth;

        spawned.hasHit = false;
        spawned.itemPosition = new Vector3(x, player.groundY, spawnZ);
        active.Add(spawned);
    }

    private void CheckHit(Item item)
    {
        if (item.hasHit) return;

        Vector3 diff = item.itemPosition - player.playerPosition;

        bool sameDepth = Mathf.Abs(diff.z) < hitDepth;
        bool sameLane = Mathf.Abs(diff.x) < hitWidth;
        if (!sameDepth || !sameLane) return;

        float playerHeight = player.playerPosition.y - player.groundY;
        bool cleared = item.jumpable && playerHeight > item.clearHeight;

        if (!cleared)
        {
            item.hasHit = true;
            player.TakeHit();
        }
    }
}
