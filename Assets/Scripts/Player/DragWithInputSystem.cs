using UnityEngine;
using UnityEngine.InputSystem;


public class DragWithInputSystem : MonoBehaviour
{
    private GameControls controls;
    private Camera cam;
    private bool isDragging = false;
    private Vector3 offset;
    private float minX, minY, maxX, maxY; //screen boundaries

    private void Start()
    {
        //left bottom (0, 0)
        Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));

        //upper top
        Vector3 topRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.nearClipPlane));

        //screen boundaries
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

                transform.position = new Vector3(clampedX, clampedY, 0f);

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
        Vector2 pointerPos = controls.Gameplay.PointerPosition.ReadValue<Vector2>();
        Vector3 pointerScreenPos = new Vector3(pointerPos.x, pointerPos.y, Mathf.Abs(cam.transform.position.z));
        Vector3 worldPos = cam.ScreenToWorldPoint(pointerScreenPos);
        Vector2 world2D = new Vector2(worldPos.x, worldPos.y);

        offset = transform.position - new Vector3(world2D.x, world2D.y, 0f);
        isDragging = true;       
    }

    private void OnPressCanceled(InputAction.CallbackContext context)
    {
        isDragging = false;
    }
}
