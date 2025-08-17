using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float smoothTime = 0.1f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 20f;
    
    private Camera cam;
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;
    
    private void Start()
    {
        cam = GetComponent<Camera>();
        targetPosition = transform.position;
        
        if (cam != null)
        {
            cam.orthographicSize = 5f;  // 화면 높이를 10 유닛으로 줄여서 격자가 더 크게 보이도록
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