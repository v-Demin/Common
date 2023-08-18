using UnityEngine;

public class MouseCameraEdgeChecker : MonoBehaviour
{
    [SerializeField] private float _edgeDistance = 0.5f;
    
    private Bounds _fullCameraBounds;
    private Bounds _reducedCameraBounds;
    private Camera _camera;
    
    private void Start()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        GenerateBounds();
        
        var mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.Set(mousePosition.x, mousePosition.y, _camera.transform.position.z);
        var direction = Vector2.zero;

        if (!_reducedCameraBounds.Contains(mousePosition))
        {
            direction = mousePosition.normalized;
        }

        var isNearEdge = direction != Vector2.zero;

        if (isNearEdge)
        {
            Debug.Log(direction);
        }
    }

    private void GenerateBounds()
    {
        var distance = _camera.transform.position.z;
        var bottomLeft = _camera.ViewportToWorldPoint(new Vector3(0, 0, distance));
        var topRight = _camera.ViewportToWorldPoint(new Vector3(1, 1, distance));

        var size = topRight - bottomLeft;
        var center = bottomLeft + size / 2;

        var cameraBounds = new Bounds(center, size);

        _fullCameraBounds = cameraBounds;
        
        _reducedCameraBounds = new Bounds(_fullCameraBounds.center, _fullCameraBounds.size - Vector3.one * _edgeDistance * 2);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.2f, 0.4f);
        Gizmos.DrawCube(_fullCameraBounds.center, _fullCameraBounds.size);

        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawCube(_reducedCameraBounds.center, _reducedCameraBounds.size);
        
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        Gizmos.DrawSphere(mousePosition, 0.3f);
    }
}