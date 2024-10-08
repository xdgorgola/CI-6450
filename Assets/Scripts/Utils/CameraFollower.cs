using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollower : MonoBehaviour
{
    [SerializeField]
    private Transform _target = null;
    [Min(0.001f)]
    [SerializeField]
    private float _smoothTime = 0.1f;
    [Min(0.001f)]
    [SerializeField]
    private float _zoomSpeed = 1f;

    private Camera _cam = null;
    private Vector2 _curVel = Vector2.zero;

    private void Awake()
    {
        if (_target is null) throw new System.NullReferenceException();
        _cam = GetComponent<Camera>();
    }

    private void Update()
    {
        float wheelInput = (Input.GetKey(KeyCode.LeftControl) ? 1 : Input.GetKey(KeyCode.LeftShift) ? -1 : 0);
        _cam.orthographicSize += wheelInput * _zoomSpeed * Time.deltaTime;
    }

    private void LateUpdate()
    {
        Vector2 wishedPosition = _target.position;
        Vector3 nextPosition = Vector2.SmoothDamp(transform.position, wishedPosition, ref _curVel, _smoothTime);
        transform.position = new(nextPosition.x, nextPosition.y, -10) ;
    }
}
