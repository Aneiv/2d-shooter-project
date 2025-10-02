using Mirror;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


public class DragWithInputSystem : Mirror.NetworkBehaviour
{
    private GameControls controls;
    private Camera cam;
    private bool isDragging = false;
    private Vector3 offset;
    [HideInInspector]
    [SyncVar] public float minX, minY, maxX, maxY; //screen boundaries
    [SyncVar] private Vector3 bottomLeft;
    [SyncVar] private Vector3 topRight;
    private GameSettings settings;
    private void Start()
    {
        settings = FindAnyObjectByType<GameSettings>();
        SetScreenClamp();
    }
    [Server]
    void SetScreenClamp()
    {
        //left bottom (0, 0)
        bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));

        //upper top
        topRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.nearClipPlane));

        //screen boundaries set
        minX = bottomLeft.x;
        maxX = topRight.x;
        minY = bottomLeft.y;
        maxY = topRight.y;
    }

    void Awake()
    {
        controls = new GameControls();
        cam = Camera.main;
    }

    void OnEnable()
    {
        //intercept
        controls.Gameplay.Enable();
        //event join
        controls.Gameplay.PointerPress.started += OnPressStarted;
        controls.Gameplay.PointerPress.canceled += OnPressCanceled;
    }

    private void OnDisable()
    {
        controls.Gameplay.PointerPress.started -= OnPressStarted;
        controls.Gameplay.PointerPress.canceled -= OnPressCanceled;
        controls.Gameplay.Disable();
    }

    void Update()
    {
        // Singleplayer - movement enable
        /*        if (settings.isSinglePlayerMode != true)
                {
                    HandleControls();
                }*/
        // Multiplayer - only owner of player instance can move their ship
        if (isLocalPlayer)
        {
            HandleControls();
        }

    }

    private void HandleControls()
    {
        if (!PauseMenu.GameIsPaused)
        {
            if (isDragging)
            {
                Vector2 pointerPos = controls.Gameplay.PointerPosition.ReadValue<Vector2>();
                Vector3 pointerScreenPos = new Vector3(pointerPos.x, pointerPos.y, Mathf.Abs(cam.transform.position.z));
                Vector3 worldPos = cam.ScreenToWorldPoint(pointerScreenPos);

                //Add offset
                Vector3 targetPos = worldPos + offset;

                // Clamp position
                float clampedX = Mathf.Clamp(targetPos.x, minX, maxX);
                float clampedY = Mathf.Clamp(targetPos.y, minY, maxY);
                Vector3 newPoss = new Vector3(clampedX, clampedY, 0f);

                transform.position = newPoss;

                // Debug
                //Debug.DrawLine(cam.transform.position, worldPos, Color.green);
                //Debug.Log($"Touch ScreenPos: {pointerPos} => WorldPos: {worldPos}");
            }
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;

                //Debug.Log("IsPressed: " + touch.press.isPressed);
                //Debug.Log("Touch Position: " + touch.position.ReadValue());
            }

        }
    }

    private void OnPressStarted(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;

        Vector2 pointerPos = controls.Gameplay.PointerPosition.ReadValue<Vector2>();
        Vector3 pointerScreenPos = new Vector3(pointerPos.x, pointerPos.y, Mathf.Abs(cam.transform.position.z));
        Vector3 worldPos = cam.ScreenToWorldPoint(pointerScreenPos);
        Vector2 world2D = new Vector2(worldPos.x, worldPos.y);

        offset = transform.position - new Vector3(world2D.x, world2D.y, 0f);

        isDragging = true;
    }

    private void OnPressCanceled(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;

        isDragging = false;
    }
    public void ResetScreenClamp()
    {
        //screen boundaries
        minX = bottomLeft.x;
        maxX = topRight.x;
        minY = bottomLeft.y;
        maxY = topRight.y;
    }
}
