using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.25f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    private Vector3 _velocity = Vector3.zero;

    private void Start()
    {
        // Try to find the player automatically if target is not manually assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                // Fallback: try to find by type
                PlayerMovement pm = FindObjectOfType<PlayerMovement>();
                if (pm != null) target = pm.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, smoothTime);
    }
}
