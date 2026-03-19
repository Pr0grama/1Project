using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<FixedString128Bytes> PlayerName = new NetworkVariable<FixedString128Bytes>(
        "Player",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> PlayerHealth = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Start()
    {
        if (IsLocalPlayer)
        {
            // ✅ БЕРЁМ ИМЯ ИЗ СТАТИЧЕСКОЙ ПЕРЕМЕННОЙ (не ищем UI)
            string nameToSend = ConnectionUI.LastPlayerName;

            Debug.Log($"Отправляем имя на сервер: {nameToSend}");
            SubmitNameServerRpc(nameToSend);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNameServerRpc(string name)
    {
        PlayerName.Value = new FixedString128Bytes(name);
        Debug.Log($"Сервер установил имя: {name}");
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;

        int newHealth = PlayerHealth.Value - damage;
        PlayerHealth.Value = Mathf.Max(0, newHealth);
        Debug.Log($"Игрок {PlayerName.Value} получил {damage} урона. HP: {PlayerHealth.Value}");
    }
}