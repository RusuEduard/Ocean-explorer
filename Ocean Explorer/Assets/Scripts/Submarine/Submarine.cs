using UnityEngine;
using UnityEngine.InputSystem;

public class Submarine : MonoBehaviour
{
    private PlayerInput controls;
    //scale 1.5
    public float maxSpeed = 5;
    public float maxPitchSpeed = 50;
    public float maxTurnSpeed = 50;
    public float acceleration = 2;

    public float smoothSpeed = 3;
    public float smoothTurnSpeed = 3;

    public Transform propeller;
    public float propellerSpeedFac = 800;
    Vector3 velocity;
    float yawVelocity;
    float pitchVelocity;
    float currentSpeed;

    void Start()
    {
        currentSpeed = maxSpeed;
        controls = GetComponent<PlayerInput>();
    }

    void Update()
    {
        float accelDir = 0;

        Vector2 input = controls.actions["Movement"].ReadValue<Vector2>();
        var accelerationAmount = controls.actions["Acceleration"].ReadValue<float>();
        accelDir += accelerationAmount;

        currentSpeed += acceleration * Time.deltaTime * accelDir;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
        float speedPercent = currentSpeed / maxSpeed;

        Vector3 targetVelocity = transform.forward * currentSpeed;
        velocity = Vector3.Lerp(velocity, targetVelocity, Time.deltaTime * smoothSpeed);

        float targetPitchVelocity = input.y * maxPitchSpeed;
        pitchVelocity = Mathf.Lerp(pitchVelocity, targetPitchVelocity, Time.deltaTime * smoothTurnSpeed);

        float targetYawVelocity = input.x * maxTurnSpeed;
        yawVelocity = Mathf.Lerp(yawVelocity, targetYawVelocity, Time.deltaTime * smoothTurnSpeed);
        transform.localEulerAngles += (Vector3.up * yawVelocity + Vector3.left * pitchVelocity) * Time.deltaTime * speedPercent;
        transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);

        propeller.Rotate(Vector3.forward * Time.deltaTime * propellerSpeedFac * speedPercent, Space.Self);
    }
}