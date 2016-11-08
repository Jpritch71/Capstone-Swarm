using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    // The target we are following
    public Transform target;
    // The distance in the x-z plane to the target
    private float distance = 17f;
    // the height we want the camera to be above the target
    private float height = 10f;
    // How much we 
    private float heightDamping = 5f;
    private float rotationDamping = 3f;

    private float maxHeight = 20f;
    private float minHeight = 5f;

    void LateUpdate()
    {
        if (!target)
            return;

        if (Input.GetKey(KeyCode.Q))
        {
            transform.RotateAround(target.position, -Vector3.up, 4f);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.RotateAround(target.position, Vector3.up, 4f);
        }

        // Calculate the current rotation angles
        var wantedRotationAngle = transform.eulerAngles.y;
        var wantedHeight = target.position.y + height;

        var currentRotationAngle = transform.eulerAngles.y;
        var currentHeight = transform.position.y;

        if(Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            height = Mathf.Clamp(height - 2f, minHeight, maxHeight);
        }
        else if(Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            height = Mathf.Clamp(height + 2f, minHeight, maxHeight);
        }
        Mathf.Clamp(height, minHeight, maxHeight);

        // Damp the rotation around the y-axis
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

        // Damp the height
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        // Convert the angle into a rotation
        var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        transform.position = target.position;
        transform.position -= currentRotation * Vector3.forward * distance;

        // Set the height of the camera
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

        // Always look at the target
        transform.LookAt(target);
    }
}