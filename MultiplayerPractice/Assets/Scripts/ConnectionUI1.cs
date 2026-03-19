using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField nameInput;

    // Статическая переменная для хранения имени между сценами/объектами
    public static string LastPlayerName { get; private set; } = "Player";

    private void Start()
    {
        // Очищаем старые подписки и добавляем новые
        hostButton.onClick.RemoveAllListeners();
        clientButton.onClick.RemoveAllListeners();

        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
    }

    private void OnHostClicked()
    {
        SavePlayerName();
        NetworkManager.Singleton.StartHost();
        gameObject.SetActive(false);
    }

    private void OnClientClicked()
    {
        SavePlayerName();
        NetworkManager.Singleton.StartClient();
        gameObject.SetActive(false);
    }

    private void SavePlayerName()
    {
        if (nameInput != null && !string.IsNullOrWhiteSpace(nameInput.text))
        {
            LastPlayerName = nameInput.text;
            Debug.Log($"Имя сохранено: {LastPlayerName}");
        }
        else
        {
            LastPlayerName = "Player";
            Debug.Log("Имя не введено, используется 'Player'");
        }
    }

    // Этот метод больше не нужен, но оставим для совместимости
    public string GetPlayerName()
    {
        return LastPlayerName;
    }
}