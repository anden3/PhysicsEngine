using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject item;
    public float delay;

    private float lastSpawned = 0.0f;

    private void Update()
    {
        float currentTime = Time.time;

        if (currentTime - lastSpawned >= delay)
        {
            lastSpawned = currentTime;

            Instantiate(item);
        }
    }
}
