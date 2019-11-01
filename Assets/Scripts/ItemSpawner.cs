using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject item;
    public float delay;
    public float lifetime;
    [Space]
    public Bounds spawnArea;

    private float lastSpawned = 0.0f;

    private void Update()
    {
        float currentTime = Time.time;

        if (currentTime - lastSpawned >= delay)
        {
            lastSpawned = currentTime;

            Vector3 point = new Vector3(
                Random.Range(spawnArea.min.x, spawnArea.max.x),
                Random.Range(spawnArea.min.y, spawnArea.max.y),
                Random.Range(spawnArea.min.z, spawnArea.max.z)
            );

            var i = Instantiate(item, point, Quaternion.identity, transform);
            Destroy(i, lifetime);
        }
    }
}
