using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

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

    public NetworkVariable<bool> IsAlive = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Respawn Settings")]
    [SerializeField] private GameObject playerModel;
    [SerializeField] private float respawnDelay = 3f;

    private CharacterController characterController;
    private PlayerMovement playerMovement;
    private ShootingController shootingController;
    private bool isRespawning = false;  // Флаг предотвращения повторного респавна

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        shootingController = GetComponent<ShootingController>();
    }

    private void Start()
    {
        if (IsLocalPlayer)
        {
            string nameToSend = ConnectionUI.LastPlayerName;
            Debug.Log($"Отправляем имя на сервер: {nameToSend}");
            SubmitNameServerRpc(nameToSend);
        }

        PlayerHealth.OnValueChanged += OnHealthChanged;
    }

    private void OnDestroy()
    {
        PlayerHealth.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        if (!IsServer) return;

        if (newValue <= 0 && IsAlive.Value && !isRespawning)
        {
            Die();
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
        if (!IsAlive.Value) return;
        if (isRespawning) return;

        int newHealth = PlayerHealth.Value - damage;
        PlayerHealth.Value = Mathf.Max(0, newHealth);
        Debug.Log($"Игрок {PlayerName.Value} получил {damage} урона. HP: {PlayerHealth.Value}");
    }

    private void Die()
    {
        if (!IsServer) return;
        if (isRespawning) return;

        isRespawning = true;
        IsAlive.Value = false;
        Debug.Log($"Игрок {PlayerName.Value} умер");

        // Отключаем управление
        if (characterController != null)
            characterController.enabled = false;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (shootingController != null)
            shootingController.enabled = false;

        // Скрываем модель на всех клиентах
        if (playerModel != null)
        {
            DisablePlayerModelClientRpc();
        }

        // Запускаем респавн (используем Invoke вместо корутины)
        Invoke(nameof(Respawn), respawnDelay);
    }

    private void Respawn()
    {
        if (!IsServer) return;

        // Находим точку респавна
        Transform respawnPoint = GetRandomRespawnPoint();
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        // Восстанавливаем здоровье
        PlayerHealth.Value = 100;
        IsAlive.Value = true;
        isRespawning = false;

        // Включаем управление обратно
        if (characterController != null)
        {
            characterController.enabled = false;  // Сброс
            characterController.enabled = true;   // Включение
        }

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (shootingController != null)
            shootingController.enabled = true;

        // Показываем модель
        if (playerModel != null)
        {
            EnablePlayerModelClientRpc();
        }

        Debug.Log($"Игрок {PlayerName.Value} возродился");
    }

    [ClientRpc]
    private void DisablePlayerModelClientRpc()
    {
        if (playerModel != null)
            playerModel.SetActive(false);
    }

    [ClientRpc]
    private void EnablePlayerModelClientRpc()
    {
        if (playerModel != null)
            playerModel.SetActive(true);
    }

    private Transform GetRandomRespawnPoint()
    {
        GameObject[] points = GameObject.FindGameObjectsWithTag("RespawnPoint");
        if (points.Length > 0)
        {
            int randomIndex = Random.Range(0, points.Length);
            return points[randomIndex].transform;
        }

        Debug.LogWarning("Нет точек респавна, использую начальную позицию");
        return transform;
    }

    public bool IsAliveClient() => IsAlive.Value;
}