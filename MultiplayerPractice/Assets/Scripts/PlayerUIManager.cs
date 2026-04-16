using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text nameText;
    [SerializeField] private Text healthText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private TextMeshProUGUI respawnTimerText;

    private PlayerNetwork playerNetwork;
    private ShootingController shootingController;
    private float respawnTimer;
    private bool isDead = false;

    private void Start()
    {
        playerNetwork = GetComponent<PlayerNetwork>();
        shootingController = GetComponent<ShootingController>();

        if (playerNetwork == null) return;

        // Подписываемся
        playerNetwork.PlayerName.OnValueChanged += OnNameChanged;
        playerNetwork.PlayerHealth.OnValueChanged += OnHealthChanged;
        playerNetwork.IsAlive.OnValueChanged += OnIsAliveChanged;

        // Инициализация
        OnNameChanged("", playerNetwork.PlayerName.Value);
        OnHealthChanged(0, playerNetwork.PlayerHealth.Value);

        if (deathPanel != null)
            deathPanel.SetActive(false);
    }

    private void Update()
    {
        if (!playerNetwork.IsOwner) return;

        // Обновление патронов
        if (shootingController != null && ammoText != null)
        {
            ammoText.text = $"Ammo: {shootingController.GetCurrentAmmo()}/{shootingController.GetMaxAmmo()}";
        }

        // Таймер респавна
        if (isDead && respawnTimerText != null)
        {
            respawnTimer -= Time.deltaTime;
            respawnTimerText.text = $"Respawn in: {Mathf.CeilToInt(respawnTimer)}";
        }
    }

    private void OnDestroy()
    {
        if (playerNetwork == null) return;

        playerNetwork.PlayerName.OnValueChanged -= OnNameChanged;
        playerNetwork.PlayerHealth.OnValueChanged -= OnHealthChanged;
        playerNetwork.IsAlive.OnValueChanged -= OnIsAliveChanged;
    }

    private void OnNameChanged(FixedString128Bytes oldValue, FixedString128Bytes newValue)
    {
        if (nameText != null)
            nameText.text = newValue.ToString();
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        if (healthText != null)
            healthText.text = $"HP: {newValue}";
    }

    private void OnIsAliveChanged(bool oldValue, bool newValue)
    {
        if (!playerNetwork.IsOwner) return;

        if (!newValue) // Умер
        {
            isDead = true;
            respawnTimer = 3f;
            if (deathPanel != null)
                deathPanel.SetActive(true);
        }
        else // Возродился
        {
            isDead = false;
            if (deathPanel != null)
                deathPanel.SetActive(false);
        }
    }
}