using UnityEngine;

namespace TextToWorld.Player
{
    /// <summary>
    /// First-person controller using CharacterController.
    /// Supports WASD movement, mouse look, gravity, and collision.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FPSController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -15f;

        [Header("Mouse Look")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float verticalLookLimit = 85f;
        [SerializeField] private bool invertY = false;

        [Header("Ground Check")]
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundMask = ~0; // All layers by default

        // Components
        private CharacterController _controller;
        private Transform _cameraTransform;

        // State
        private Vector3 _velocity;
        private float _verticalRotation = 0f;
        private bool _isGrounded;
        private bool _cursorLocked = true;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            
            // Find or create camera
            _cameraTransform = GetComponentInChildren<Camera>()?.transform;
            if (_cameraTransform == null)
            {
                // Create camera if not found
                var camObj = new GameObject("PlayerCamera");
                camObj.transform.SetParent(transform);
                camObj.transform.localPosition = new Vector3(0, 1.6f, 0);
                camObj.transform.localRotation = Quaternion.identity;
                var cam = camObj.AddComponent<Camera>();
                cam.nearClipPlane = 0.1f;
                camObj.AddComponent<AudioListener>();
                _cameraTransform = camObj.transform;
            }
        }

        private void Start()
        {
            LockCursor(true);
        }

        private void Update()
        {
            HandleCursorLock();
            
            if (_cursorLocked)
            {
                HandleMouseLook();
                HandleMovement();
            }
        }

        #region Mouse Look

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            if (invertY) mouseY = -mouseY;

            // Horizontal rotation - rotate the whole player
            transform.Rotate(Vector3.up * mouseX);

            // Vertical rotation - rotate only the camera
            _verticalRotation -= mouseY;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -verticalLookLimit, verticalLookLimit);
            _cameraTransform.localEulerAngles = new Vector3(_verticalRotation, 0f, 0f);
        }

        #endregion

        #region Movement

        private void HandleMovement()
        {
            // Ground check
            _isGrounded = _controller.isGrounded;
            
            // Additional ground check using raycast for more reliability
            if (!_isGrounded)
            {
                _isGrounded = Physics.Raycast(
                    transform.position + Vector3.up * 0.1f, 
                    Vector3.down, 
                    groundCheckDistance + 0.1f, 
                    groundMask
                );
            }

            // Reset vertical velocity when grounded
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f; // Small downward force to keep grounded
            }

            // Get input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Calculate move direction relative to player facing
            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            move = Vector3.ClampMagnitude(move, 1f); // Normalize diagonal movement

            // Apply sprint
            float speed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed *= sprintMultiplier;
            }

            // Move horizontally
            _controller.Move(move * speed * Time.deltaTime);

            // Jump
            if (Input.GetButtonDown("Jump") && _isGrounded)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Apply gravity
            _velocity.y += gravity * Time.deltaTime;

            // Apply vertical movement
            _controller.Move(_velocity * Time.deltaTime);
        }

        #endregion

        #region Cursor Lock

        private void HandleCursorLock()
        {
            // Toggle cursor lock with Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LockCursor(!_cursorLocked);
            }

            // Re-lock on click
            if (!_cursorLocked && Input.GetMouseButtonDown(0))
            {
                LockCursor(true);
            }
        }

        private void LockCursor(bool locked)
        {
            _cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Teleport the player to a position.
        /// </summary>
        public void Teleport(Vector3 position)
        {
            _controller.enabled = false;
            transform.position = position;
            _controller.enabled = true;
            _velocity = Vector3.zero;
        }

        /// <summary>
        /// Set the player's rotation (Y axis only).
        /// </summary>
        public void SetRotation(float yRotation)
        {
            transform.rotation = Quaternion.Euler(0, yRotation, 0);
            _verticalRotation = 0;
            _cameraTransform.localEulerAngles = Vector3.zero;
        }

        /// <summary>
        /// Check if player is currently grounded.
        /// </summary>
        public bool IsGrounded => _isGrounded;

        /// <summary>
        /// Get the camera transform.
        /// </summary>
        public Transform CameraTransform => _cameraTransform;

        #endregion

        #region Debug

        private void OnDrawGizmosSelected()
        {
            // Draw ground check ray
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(
                transform.position + Vector3.up * 0.1f,
                transform.position + Vector3.up * 0.1f + Vector3.down * (groundCheckDistance + 0.1f)
            );
        }

        #endregion
    }
}
