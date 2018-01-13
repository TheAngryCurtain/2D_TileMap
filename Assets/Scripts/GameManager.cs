using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject[] mInitPrefabs;

    private void Start()
    {
        Debug.Log("Firing World Gen Request Event!");
        VSEventManager.Instance.TriggerEvent(new GameEvents.RequestWorldGenEvent(OnWorldGenerated));
    }

    private void OnWorldGenerated()
    {
        Debug.Log("World Generation complete, spawning initial prefabs");

        int totalPrefabs = mInitPrefabs.Length;
        for (int i = 0; i < totalPrefabs; i++)
        {
            Instantiate(mInitPrefabs[i], null);
        }
    }
}
