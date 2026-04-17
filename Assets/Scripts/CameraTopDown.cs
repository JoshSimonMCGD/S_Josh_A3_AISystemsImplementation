using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Position")]
    public float height = 15f;
    public float followSpeed = 10f;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 0f, -5f);

    // Top Down Camera follow based on x,z
    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = new Vector3(
            target.position.x,
            height,
            target.position.z
        ) + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }
}
