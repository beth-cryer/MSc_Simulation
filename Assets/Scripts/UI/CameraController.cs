using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float camMoveSpeed = 5.0f;
    [SerializeField] private float camFastMultiplier = 2.0f;
    [SerializeField] private float camZoomSpeed = 5.0f;
    [SerializeField] private float camZoomMin = 5.0f;
    [SerializeField] private float camZoomMax = 25.0f;

    private Camera m_cam;
    private float m_currentMoveSpeed = 0.0f;
    private bool m_isSlowing = false;

    void Start()
    {
        m_cam = GetComponent<Camera>();
    }

    void Update()
    {
        // Movement
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        bool isMoving = (x != 0) || (y != 0);
        if (!isMoving && !m_isSlowing) m_isSlowing = true;

        float moving = isMoving ? 1 : 0;
        float fastMultiplier = Mathf.Max(1.0f, camFastMultiplier * Input.GetAxis("Run"));
        float moveDistanceScaling = 1 + (m_cam.orthographicSize - camZoomMin) / (camZoomMax - camZoomMin);

        float targetMoveSpeed = camMoveSpeed * moveDistanceScaling * fastMultiplier * moving;
        m_currentMoveSpeed = Mathf.MoveTowards(m_currentMoveSpeed, targetMoveSpeed, (targetMoveSpeed * Time.deltaTime));

        float xSpeed = x * m_currentMoveSpeed * Time.deltaTime;
        float ySpeed = y * m_currentMoveSpeed * Time.deltaTime;

        transform.position = new Vector3(transform.position.x + xSpeed, transform.position.y + ySpeed, transform.position.z);

        // Zoom
        float z = Input.GetAxis("Mouse ScrollWheel");
        float zSpeed = z * camZoomSpeed * 1000 * Time.deltaTime;
        float zoomClamped = Mathf.Clamp(m_cam.orthographicSize + zSpeed, camZoomMin, camZoomMax);
        m_cam.orthographicSize = zoomClamped;
    }
}
