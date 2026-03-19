using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class AttackController : NetworkBehaviour
{
    [SerializeField] private Button attackButton;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private int attackDamage = 20;

    private void Start()
    {
        // Запасной вариант: если OnNetworkSpawn не сработает
        if (attackButton != null && IsOwner)
        {
            attackButton.onClick.AddListener(TryAttack);
            Debug.Log("AttackController: кнопка подключена в Start");
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            Debug.Log("AttackController: не владелец, отключаем кнопку");
            if (attackButton != null)
                attackButton.interactable = false;
            return;
        }

        if (attackButton == null)
        {
            Debug.LogError("AttackController: кнопка не назначена в инспекторе!");
            return;
        }

        // Убираем старые подписки и добавляем новую
        attackButton.onClick.RemoveAllListeners();
        attackButton.onClick.AddListener(TryAttack);

        Debug.Log("AttackController: кнопка подключена в OnNetworkSpawn");
    }

    private void TryAttack()
    {
        Debug.Log($"Попытка атаки. IsOwner = {IsOwner}, IsServer = {IsServer}, IsClient = {IsClient}");

        if (!IsOwner)
        {
            Debug.Log("Не владелец, атака невозможна");
            return;
        }

        PlayerNetwork closestEnemy = FindClosestEnemy();

        if (closestEnemy != null)
        {
            Debug.Log($"Атакуем: {closestEnemy.PlayerName.Value}");
            RequestAttackServerRpc(closestEnemy.GetComponent<NetworkObject>());
        }
        else
        {
            Debug.Log("Рядом нет врагов");
        }
    }

    private PlayerNetwork FindClosestEnemy()
    {
        PlayerNetwork[] players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
        PlayerNetwork closest = null;
        float minDistance = float.MaxValue;

        foreach (PlayerNetwork player in players)
        {
            if (player == GetComponent<PlayerNetwork>()) continue;

            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < attackRange && dist < minDistance)
            {
                minDistance = dist;
                closest = player;
            }
        }
        return closest;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestAttackServerRpc(NetworkObjectReference targetRef)
    {
        if (targetRef.TryGet(out NetworkObject targetObj))
        {
            PlayerNetwork target = targetObj.GetComponent<PlayerNetwork>();
            if (target != null)
            {
                target.TakeDamage(attackDamage);
                Debug.Log($"ServerRpc: урон нанесён {target.PlayerName.Value}");
            }
        }
    }
}