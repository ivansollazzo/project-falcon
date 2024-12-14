using System.Collections;
using UnityEngine;

public class PedestrianSpawner : MonoBehaviour
{

    public GameObject pedestrianPrefab;
    public GameObject pedestrianPrefab2;

    public GameObject pedestrianPrefab3;
    public GameObject pedestrianPrefab4;
    public GameObject pedestrianPrefab5;


    public int pedestriansToSpawn;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        GameObject[] prefabs = { pedestrianPrefab, pedestrianPrefab2, pedestrianPrefab3, pedestrianPrefab4, pedestrianPrefab5 };
        int count = 0;

        while (count < pedestriansToSpawn)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            Transform child = transform.GetChild(Random.Range(0, transform.childCount - 1));

            GameObject obj = Instantiate(prefab);
            obj.GetComponent<WaypointNavigator>().currrentWaypoint = child.GetComponent<Waypoint>();
            obj.transform.position = child.position;

            count++; // Incrementa il contatore
            yield return new WaitForEndOfFrame();
        }
    }

}
