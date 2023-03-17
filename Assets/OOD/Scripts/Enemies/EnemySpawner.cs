using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject meeleEnemy;

    public int numEnemies;
    public Vector3 spawnAreaSize = new Vector3(10, 0, 10);
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numEnemies; i++) {
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-spawnAreaSize.x, spawnAreaSize.x), 0, Random.Range(-spawnAreaSize.z, spawnAreaSize.z));
            Instantiate(meeleEnemy, spawnPosition, Quaternion.identity);
        }
    }
}
