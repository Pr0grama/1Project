using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PickupManager : NetworkBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private GameObject healthPackPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float respawnTime = 10f;

    private List<HealthPack> activeHealthPacks = new List<HealthPack>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        foreach (Transform point in spawnPoints)
        {
            SpawnHealthPack(point.position);
        }
    }

    private void SpawnHealthPack(Vector3 position)
    {
        GameObject pack = Instantiate(healthPackPrefab, position, Quaternion.identity);
        NetworkObject netObj = pack.GetComponent<NetworkObject>();
        netObj.Spawn();

        HealthPack healthPack = pack.GetComponent<HealthPack>();
        healthPack.Initialize(this);
        activeHealthPacks.Add(healthPack);
    }

    public void OnHealthPackCollected(HealthPack pack)
    {
        if (!IsServer) return;

        Vector3 position = pack.transform.position;
        activeHealthPacks.Remove(pack);
        pack.GetComponent<NetworkObject>().Despawn();

        StartCoroutine(RespawnCoroutine(position));
    }

    private System.Collections.IEnumerator RespawnCoroutine(Vector3 position)
    {
        yield return new WaitForSeconds(respawnTime);

        if (IsServer)
        {
            SpawnHealthPack(position);
        }
    }
}