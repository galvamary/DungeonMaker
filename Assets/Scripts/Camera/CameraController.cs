using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;
    
    private void Start()
    {
        cam = GetComponent<Camera>();
        
        if (cam != null)
        {
            // Set camera to show the entire 1920x1080 grid
            // With 48 pixel cells: height = 23 cells * 0.48 units = 11.04 units
            // Orthographic size is half the height
            cam.orthographicSize = 5.52f;
            
            // Center the camera
            transform.position = new Vector3(0, 0, -10);
        }
    }
}