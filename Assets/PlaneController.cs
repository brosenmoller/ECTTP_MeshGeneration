using UnityEngine;

public class PlaneController : MonoBehaviour
{
    [Header("General")]
    [SerializeField] float movementSpeed;
    [SerializeField] float elevationSpeed;
    [SerializeField] float rotationSpeed;

    [Header("Looks")]
    [SerializeField] float bodyRotationSpeed;
    [SerializeField] float bodyMaxRotation;
    [SerializeField] float propelorSpeed;

    [Header("Rotations")]
    [SerializeField] Vector3 baseRotation;
    [SerializeField] Vector3 leftRotation;
    [SerializeField] Vector3 rightRotation;

    [Header("References")]
    [SerializeField] Transform propelor;
    [SerializeField] Transform body;

    int lastDirection = 0;
    float timer = 0;

    Vector3 oldRotation;
    Vector3 newRotation;

    private void Start()
    {
        oldRotation = newRotation = baseRotation;
    }

    private void Update()
    {
        int direction = (int)Input.GetAxisRaw("Horizontal");
        int elevation = (int)Input.GetAxisRaw("Vertical");

        if (direction != lastDirection)
        {
            timer = 0;
            //oldRotation = newRotation;
            //switch (direction) 
            //{
            //    case 0: newRotation = baseRotation; break;
            //    case 1: newRotation = rightRotation; break;
            //    case -1: newRotation = leftRotation; break;
            //}
        }

        // Movement
        transform.position += -1f * movementSpeed * Time.deltaTime * transform.right;
        transform.Rotate(direction * rotationSpeed * Time.deltaTime * Vector3.up);
        transform.position += -1f * elevation * elevationSpeed * Time.deltaTime * Vector3.up;

        // Body Rotation
        float sideRotation = Mathf.Lerp(lastDirection, direction, timer);
        timer += bodyRotationSpeed * Time.deltaTime;
        body.localRotation = Quaternion.Euler(sideRotation * bodyMaxRotation, transform.localRotation.y, body.localRotation.z);

        //Vector3 rotation = Vector3.RotateTowards(oldRotation, newRotation, bodyRotationSpeed * Time.deltaTime, bodyRotationSpeed * Time.deltaTime);
        //body.localRotation = Quaternion.Euler(rotation);

        // Propelor
        propelor.Rotate(rotationSpeed * Vector3.left);

        lastDirection = direction;
    }
}
