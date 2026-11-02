using UnityEngine;

public class HouseSpawner : MonoBehaviour
{
    public GameObject[] housePrefabs;
    public int numberOfHouses = 5;
    public Vector2 spawnAreaMin = new Vector2(-8, -4);
    public Vector2 spawnAreaMax = new Vector2(8, 4);
    public int randomSeed = 12345;

    void Start()
    {
        Debug.Log("=== СПАВНЕР ЗАПУСТИЛСЯ! ===");

        if (housePrefabs.Length == 0)
        {
            Debug.LogError("ОШИБКА: Массив префабов пуст! Добавьте префабы в инспекторе.");
            return;
        }

        Random.InitState(randomSeed);
        for (int i = 0; i < numberOfHouses; i++)
        {
            GameObject prefab = housePrefabs[Random.Range(0, housePrefabs.Length)];

            if (prefab == null)
            {
                Debug.LogError("ОШИБКА: Один из префабов пустой (Missing)!");
                continue;
            }

            float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);

            Instantiate(prefab, new Vector2(x, y), Quaternion.identity);
            Debug.Log("Создан домик номер " + i);
        }
    }
}