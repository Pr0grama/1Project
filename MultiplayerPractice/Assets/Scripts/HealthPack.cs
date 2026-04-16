using Unity.Netcode;
using UnityEngine;

public class HealthPack : NetworkBehaviour
{
    [SerializeField] private int healAmount = 30;

    private PickupManager manager;
    private bool isCollected = false;

    public void Initialize(PickupManager pickupManager)
    {
        manager = pickupManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (isCollected) return;

        PlayerNetwork player = other.GetComponent<PlayerNetwork>();

        if (player != null)
        {
            // Проверки
            if (!player.IsAlive.Value)
            {
                Debug.Log("[Server] Мёртвый не может подобрать аптечку");
                return;
            }

            if (player.PlayerHealth.Value >= 100)
            {
                Debug.Log("[Server] HP уже полное");
                return;
            }

            // Лечим
            int newHealth = Mathf.Min(100, player.PlayerHealth.Value + healAmount);
            player.PlayerHealth.Value = newHealth;

            Debug.Log($"[Server] {player.PlayerName.Value} подобрал аптечку. HP: {newHealth}");

            isCollected = true;
            if (manager != null)
            {
                manager.OnHealthPackCollected(this);
            }
            else
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}