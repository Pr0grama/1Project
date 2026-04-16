using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifeTime = 5f;

    private Vector3 direction;
    private float speed;
    private ulong shooterId;
    private Rigidbody rb;
    private bool isDespawning = false;  // Флаг для предотвращения двойного удаления

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 dir, float spd, ulong shooter)
    {
        direction = dir;
        speed = spd;
        shooterId = shooter;

        if (IsServer)
        {
            if (rb != null)
            {
                rb.velocity = direction * speed;
                Debug.Log($"[Bullet] Скорость: {rb.velocity.magnitude}");
            }

            // Запускаем таймер уничтожения
            Invoke(nameof(DestroyBullet), lifeTime);
        }
    }

    private void Start()
    {
        if (!IsServer && rb != null && direction != Vector3.zero)
        {
            rb.velocity = direction * speed;
        }
    }

    private void DestroyBullet()
    {
        if (isDespawning) return;
        if (!IsServer) return;
        if (!IsSpawned) return;

        isDespawning = true;
        GetComponent<NetworkObject>().Despawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (isDespawning) return;  // Уже уничтожается
        if (!IsSpawned) return;     // Не заспавнен

        PlayerNetwork target = other.GetComponent<PlayerNetwork>();

        if (target != null)
        {
            // Не наносим урон себе
            if (target.OwnerClientId == shooterId)
            {
                DestroyBullet();
                return;
            }

            target.TakeDamage((int)damage);
            Debug.Log($"[Server] Снаряд нанёс {damage} урона {target.PlayerName.Value}");
        }

        // Уничтожаем при любом столкновении
        DestroyBullet();
    }

    private void OnDestroy()
    {
        // Отменяем Invoke при уничтожении объекта
        CancelInvoke();
    }
}