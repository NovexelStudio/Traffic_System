using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarSpawner : MonoBehaviour
{
    [Header("Car Spawner Settings")]
    [SerializeField] List<GameObject> carPrefabs;
    [SerializeField] Waypoint[] spawnPoints;
    [SerializeField] float spawnInterval = 3f;
    [SerializeField] float spawnLimit = 10f;
    [SerializeField] Transform parent;

    [Header("Npc Spawner Settings")]

    [SerializeField] int npcSpawnLimit = 5;
    [SerializeField] Transform npcParent;
    [SerializeField] List<GameObject> npcPrefab;
    [SerializeField] float npcSpawnInterval = 5f;
    [SerializeField] GameObject[] npcSpawnPoint;



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(SpawnCars());
            StartCoroutine(SpawnNpcs());
        }
    }

    IEnumerator SpawnCars()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Check if we haven't reached the spawn limit
            if (parent.childCount < spawnLimit)
            {
                int spawnPointIndex = Random.Range(0, spawnPoints.Length);
                int carIndex = Random.Range(0, carPrefabs.Count);

                // Get the spawn position from the waypoint
                Vector3 spawnPosition = spawnPoints[spawnPointIndex].transform.position;

                // Instantiate the car
                GameObject carPrefab = Instantiate(
                    carPrefabs[carIndex],
                    spawnPosition,
                    spawnPoints[spawnPointIndex].transform.rotation,
                    parent
                );

                // Optional: Set the car's initial waypoint
                CarAi carAi = carPrefab.GetComponent<CarAi>();
                if (carAi != null)
                {
                    carAi.SetCurrentWaypoint(spawnPoints[spawnPointIndex]);
                }
            }

        }
    }

    IEnumerator SpawnNpcs()
    {
        while (true)
        {
            yield return new WaitForSeconds(npcSpawnInterval);
            // Check if we haven't reached the spawn limit
            if (npcParent.childCount < npcSpawnLimit)
            {
                int npcIndex = Random.Range(0, npcPrefab.Count);
                int spawnPointIndex = Random.Range(0, npcSpawnPoint.Length); // Since there's only one npcSpawnPoint
                // Instantiate the NPC
                Instantiate(
                    npcPrefab[npcIndex],
                    npcSpawnPoint[spawnPointIndex].transform.position,
                    Quaternion.identity,
                    npcParent
                );
            }
        }
    }
}