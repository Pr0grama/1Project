using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(ClientNetworkTransform))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController controller;
    private PlayerNetwork playerNetwork;
    private Vector3 velocity;
    private ClientNetworkTransform clientNetworkTransform;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerNetwork = GetComponent<PlayerNetwork>();
        clientNetworkTransform = GetComponent<ClientNetworkTransform>();
    }

    private void Update()
    {
        // Только владелец и только живой игрок может двигаться
        if (!IsOwner || !playerNetwork.IsAlive.Value) return;

        HandleMovement();
        ApplyGravity();

        // Принудительная синхронизация позиции
        //if (clientNetworkTransform != null)
        //{
          //  clientNetworkTransform.Teleport(transform.position, transform.rotation);
        //}
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * walkSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}