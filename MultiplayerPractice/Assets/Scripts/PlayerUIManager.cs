using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text healthText;

    private PlayerNetwork playerNetwork;

    private void Start()
    {
        playerNetwork = GetComponent<PlayerNetwork>();

        if (playerNetwork == null) return;

        // Подписываемся на изменения сетевых переменных
        playerNetwork.PlayerName.OnValueChanged += OnNameChanged;
        playerNetwork.PlayerHealth.OnValueChanged += OnHealthChanged;

        // Инициализация начальных значений
        OnNameChanged("", playerNetwork.PlayerName.Value);
        OnHealthChanged(0, playerNetwork.PlayerHealth.Value);
    }

    private void OnDestroy()
    {
        if (playerNetwork == null) return;

        playerNetwork.PlayerName.OnValueChanged -= OnNameChanged;
        playerNetwork.PlayerHealth.OnValueChanged -= OnHealthChanged;
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
}