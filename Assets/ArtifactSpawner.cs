using UnityEngine;

public class ArtifactSpawner : MonoBehaviour
{
    public GameObject artifactPrefab; // Префаб артефакта
    public Transform[] spawnPoints;   // Точки, где они могут появиться (пустые объекты)

    void Start()
    {
        // Сколько нам нужно заспавнить в этой комнате?
        int needed = GameSession.maxArtifacts - GameSession.collectedArtifacts;

        for (int i = 0; i < needed; i++)
        {
            if (i >= spawnPoints.Length) break; // Если точек меньше, чем предметов

            // Спавним артефакт в случайной точке
            Instantiate(artifactPrefab, spawnPoints[i].position, Quaternion.identity);
        }
    }
}