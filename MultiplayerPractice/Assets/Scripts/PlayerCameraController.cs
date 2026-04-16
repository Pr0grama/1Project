using Unity.Netcode;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 1.6f, -4f);
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private float mouseSensitivity = 2f;

    private Camera playerCamera;
    private float xRotation = 0f;

    public override void OnNetworkSpawn()
    {
        // Только владелец создаёт и управляет камерой
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        // Создаём камеру для этого игрока
        GameObject camObj = new GameObject("PlayerCamera");
        playerCamera = camObj.AddComponent<Camera>();
        playerCamera.tag = "MainCamera";

        // Настройка камеры
        camObj.transform.SetParent(transform);
        camObj.transform.localPosition = cameraOffset;

        // Блокируем курсор для FPS-стиля
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        if (!IsOwner || playerCamera == null) return;

        HandleMouseLook();
        UpdateCameraPosition();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void UpdateCameraPosition()
    {
        Vector3 targetPosition = transform.position + cameraOffset;
        playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    private void OnDestroy()
    {
        if (playerCamera != null && playerCamera.gameObject != null)
        {
            Destroy(playerCamera.gameObject);
        }
    }
}