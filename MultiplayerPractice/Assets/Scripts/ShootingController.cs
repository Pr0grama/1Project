using Unity.Netcode;
using UnityEngine;

public class ShootingController : NetworkBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireRate = 0.3f;
    [SerializeField] private int maxAmmo = 30;

    private NetworkVariable<int> currentAmmo = new NetworkVariable<int>(30);
    private float lastFireTime;
    private PlayerNetwork playerNetwork;

    private void Awake()
    {
        playerNetwork = GetComponent<PlayerNetwork>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentAmmo.Value = maxAmmo;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Левая кнопка мыши для стрельбы
        if (Input.GetButtonDown("Fire1"))
        {
            TryShoot();
        }

        // Клавиша R для перезарядки
        if (Input.GetKeyDown(KeyCode.R))
        {
            RequestReloadServerRpc();
        }
    }

    private void TryShoot()
    {
        if (!IsOwner) return;

        // Получаем направление из центра камеры
        Vector3 shootDirection = GetShootDirection();

        // Отправляем запрос на сервер
        ShootServerRpc(shootDirection);
    }

    private Vector3 GetShootDirection()
    {
        // Если есть камера, стреляем из её центра
        Camera cam = Camera.main;
        if (cam != null)
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            return ray.direction;
        }

        return transform.forward;
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 direction)
    {
        // Серверная валидация
        if (!playerNetwork.IsAlive.Value)
        {
            Debug.Log($"[Server] {playerNetwork.PlayerName.Value} мёртв - стрельба запрещена");
            return;
        }

        if (currentAmmo.Value <= 0)
        {
            Debug.Log($"[Server] {playerNetwork.PlayerName.Value} нет патронов");
            return;
        }

        if (Time.time - lastFireTime < fireRate)
        {
            Debug.Log($"[Server] {playerNetwork.PlayerName.Value} кулдаун");
            return;
        }

        // Валидация пройдена
        lastFireTime = Time.time;
        currentAmmo.Value--;

        // Создаём снаряд
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
        bullet.GetComponent<NetworkObject>().Spawn();

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.Initialize(direction, bulletSpeed, OwnerClientId);

        Debug.Log($"[Server] {playerNetwork.PlayerName.Value} выстрелил. Осталось патронов: {currentAmmo.Value}");
    }

    [ServerRpc]
    private void RequestReloadServerRpc()
    {
        if (!playerNetwork.IsAlive.Value) return;
        currentAmmo.Value = maxAmmo;
        Debug.Log($"[Server] {playerNetwork.PlayerName.Value} перезарядился");
    }

    // Для UI
    public int GetCurrentAmmo() => currentAmmo.Value;
    public int GetMaxAmmo() => maxAmmo;
}