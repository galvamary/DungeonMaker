using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float smoothTime = 0.1f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 10f;
    [SerializeField] private float defaultZoom = 3f;

    private Camera cam;
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;
    
    private void Start()
    {
        cam = GetComponent<Camera>();
        targetPosition = transform.position;
        
        if (cam != null)
        {
            cam.orthographicSize = defaultZoom;
        }
    }

    public void FocusOnPosition(Vector3 position)
    {
        // Set camera position to focus on target
        targetPosition = new Vector3(position.x, position.y, transform.position.z);
        transform.position = targetPosition;
    }

    public void SetZoom(float zoom)
    {
        if (cam != null)
        {
            cam.orthographicSize = Mathf.Clamp(zoom, minZoom, maxZoom);
        }
    }

    public void ResetToDefaultZoom()
    {
        if (cam != null)
        {
            cam.orthographicSize = defaultZoom;
        }
    }
    
    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }
    
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(horizontal, vertical, 0) * moveSpeed * Time.deltaTime;
        targetPosition += movement;
        
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
    
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (scroll != 0 && cam != null)
        {
            float newSize = cam.orthographicSize - scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }
}