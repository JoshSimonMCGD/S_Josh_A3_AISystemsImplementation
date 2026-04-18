using UnityEngine;

public class CameraFollow3D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Position")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -8f);
    [SerializeField] private float smoothTime = 0.2f;

    [Header("Rotation")]
    [SerializeField] private bool lookAtTarget = true;
    [SerializeField] private float rotationSmoothSpeed = 8f;

    private Vector3 currentVelocity;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            smoothTime
        );

        if (lookAtTarget)
        {
            Vector3 lookDirection = target.position - transform.position;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSmoothSpeed * Time.deltaTime
                );
            }
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}