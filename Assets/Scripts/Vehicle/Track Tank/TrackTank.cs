using System.Collections.Generic;
using UnityEngine;
using Mirror;

[System.Serializable]
public class TrackWheelRow
{
    [SerializeField] private WheelCollider[] colliders;
    [SerializeField] private Transform[] meshs;

    public float minRPM;

    public void SetTorque(float torque)
    {
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].motorTorque = torque;
    }

    public void SetBrake(float brake)
    {
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].brakeTorque = brake;
    }

    public void Reset()
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].motorTorque = 0;
            colliders[i].brakeTorque = 0;
        }
    }

    public void SetSidewaysStiffness(float stiffness)
    {
        WheelFrictionCurve wheelFrictionCurve = new WheelFrictionCurve();

        for (int i = 0; i < colliders.Length; i++)
        {
            wheelFrictionCurve = colliders[i].sidewaysFriction;
            wheelFrictionCurve.stiffness = stiffness;

            colliders[i].sidewaysFriction = wheelFrictionCurve;
        }
    }

    public void UpdateMeshTransform()
    {
        // Find min RPM
        List<float> allRPM = new List<float>();

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].isGrounded == true)
            {
                allRPM.Add(colliders[i].rpm);
            }
        }

        if (allRPM.Count > 0)
        {
            minRPM = Mathf.Abs(allRPM[0]);

            for (int i = 0; i < allRPM.Count; i++)
            {
                if (Mathf.Abs(allRPM[i]) < minRPM)
                {
                    minRPM = Mathf.Abs(allRPM[i]);
                }
            }

            minRPM = minRPM * Mathf.Sign(allRPM[0]);
        }

        // Mesh
        float angle = minRPM * 360.0f / 60.0f * Time.fixedDeltaTime;

        for (int i = 0; i < meshs.Length; i++)
        {
            Vector3 position;
            Quaternion rotation;

            colliders[i].GetWorldPose(out position, out rotation);

            meshs[i].position = position;
            meshs[i].Rotate(angle, 0, 0);
        }
    }

    public void UpdateMeshRotationByRPM(float rpm)
    {
        float angle = rpm * 360.0f / 60.0f * Time.fixedDeltaTime;

        for (int i = 0;i < meshs.Length;i++)
        {
            Vector3 position;
            Quaternion rotation;

            colliders[i].GetWorldPose(out position, out rotation);

            meshs[i].position = position;
            meshs[i].Rotate(angle, 0, 0);
        }
    }
}

[RequireComponent(typeof(Rigidbody))]
public class TrackTank : Vehicle
{
    public override float LinearVelocity => rigidBody.linearVelocity.magnitude;

    [SerializeField] private Transform centerOfMass;

    [Header("Tracks")]
    [SerializeField] private TrackWheelRow leftWheelRow;
    [SerializeField] private TrackWheelRow rightWheelRow;
    [SerializeField] private GameObject visualModel;
    [SerializeField] private GameObject destroyedPrefab;

    [Header("Movement")]
    [SerializeField] private float maxForwardTorque;
    [SerializeField] private float maxBackwardTorque;
    [SerializeField] private ParameterCurve forwardTorqueCurve;
    [SerializeField] private ParameterCurve backwardTorqueCurve;
    [SerializeField] private float brakeTorque;
    [SerializeField] private float rollingResistance;

    [Header("Rotation")]
    [SerializeField] private float rotateTorqueInPlace;
    [SerializeField] private float rotateBrakeInPlace;
    [SerializeField] private float rotateTorqueInMotion;
    [SerializeField] private float rotateBrakeInMotion;

    [Header("Rotation")]
    [SerializeField] private float minSidewayStiffnessInPlace;
    [SerializeField] private float minSidewayStiffnessInMotion;

    private Rigidbody rigidBody;
    [SerializeField] private float currentMotorTorque;

    public float LeftWheelRMP => leftWheelRow.minRPM;
    public float RightWheelRMP => rightWheelRow.minRPM;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rigidBody.centerOfMass = centerOfMass.localPosition;

        Destroyed += OnTrackTankDestroyed;
    }

    private void OnDestroy()
    {
        Destroyed -= OnTrackTankDestroyed;
    }

    private void FixedUpdate()
    {
        if (isServer)
        {
            UpdateMotorTorque();
            SvUpdateWheelRPM(leftWheelRow.minRPM, rightWheelRow.minRPM);
            SvUpdateLinearVelocity(LinearVelocity);
        }

        if (isOwned)
        {
            UpdateMotorTorque();
            CmdUpdateWheelRPM(leftWheelRow.minRPM, rightWheelRow.minRPM);
            CmdUpdateLinearVelocity(LinearVelocity);
        }
    }

    private void OnTrackTankDestroyed(Destructible destructible)
    {
        GameObject destroyedVisualModel = Instantiate(destroyedPrefab);

        destroyedVisualModel.transform.position = visualModel.transform.position;
        destroyedVisualModel.transform.rotation = visualModel.transform.rotation;

        gameObject.SetActive(false);
    }

    [Command]
    private void CmdUpdateLinearVelocity(float velocity)
    {
        SvUpdateLinearVelocity(velocity);
    }

    [Server]
    private void SvUpdateLinearVelocity(float velocity)
    {
        syncLinearVelocity = velocity;
    }

    [Command]
    private void CmdUpdateWheelRPM(float leftRPM, float rightRPM)
    {
        SvUpdateWheelRPM(leftRPM, rightRPM);
    }

    [Server]
    private void SvUpdateWheelRPM(float leftRPM, float rightRPM)
    {
        RpcUpdateWheelRPM(leftRPM, rightRPM);
    }

    [ClientRpc(includeOwner = false)]
    private void RpcUpdateWheelRPM(float leftRPM, float rightRPM)
    {
        leftWheelRow.minRPM = leftRPM;
        rightWheelRow.minRPM = rightRPM;

        leftWheelRow.UpdateMeshRotationByRPM(leftRPM);
        rightWheelRow.UpdateMeshRotationByRPM(rightRPM);
    }

    private void UpdateMotorTorque()
    {
        float motorTorque = targetInputControl.z > 0 ? maxForwardTorque * Mathf.RoundToInt(targetInputControl.z) : maxBackwardTorque * Mathf.RoundToInt(targetInputControl.z);
        float brakeTorque = this.brakeTorque * targetInputControl.y;
        float steering = targetInputControl.x;

        // Update target motor torque
        if (motorTorque > 0)
        {
            currentMotorTorque = forwardTorqueCurve.MoveToward(Time.fixedDeltaTime) * motorTorque;
        }

        if (motorTorque < 0)
        {
            currentMotorTorque = backwardTorqueCurve.MoveToward(Time.fixedDeltaTime) * motorTorque;
        }

        if (motorTorque == 0)
        {
            currentMotorTorque = forwardTorqueCurve.Reset();
            currentMotorTorque = backwardTorqueCurve.Reset();
        }

        // Brake
        leftWheelRow.SetBrake(brakeTorque);
        rightWheelRow.SetBrake(brakeTorque);

        // Rolling
        if (motorTorque == 0 && steering == 0)
        {
            leftWheelRow.SetBrake(rollingResistance);
            rightWheelRow.SetBrake(rollingResistance);
        }
        else
        {
            leftWheelRow.Reset();
            rightWheelRow.Reset();
        }

        // Rotate in place
        if (motorTorque == 0 && steering != 0)
        {
            if (Mathf.Abs(leftWheelRow.minRPM) < 1 || Mathf.Abs(rightWheelRow.minRPM) < 1)
            {
                leftWheelRow.SetTorque(rotateTorqueInPlace);
                rightWheelRow.SetTorque(rotateTorqueInPlace);
            }
            else
            {
                if (steering < 0)
                {
                    rightWheelRow.SetTorque(rotateTorqueInPlace);
                    leftWheelRow.SetBrake(rotateBrakeInPlace);
                }

                if (steering > 0)
                {
                    leftWheelRow.SetTorque(rotateTorqueInPlace);
                    rightWheelRow.SetBrake(rotateBrakeInPlace);
                }
            }

            leftWheelRow.SetSidewaysStiffness(1.0f + minSidewayStiffnessInPlace - Mathf.Abs(steering));
            rightWheelRow.SetSidewaysStiffness(1.0f + minSidewayStiffnessInPlace - Mathf.Abs(steering));
        }

        // Move
        if (motorTorque != 0)
        {
            if (steering == 0)
            {
                if (LinearVelocity < maxLinearVelocity)
                {
                    leftWheelRow.SetTorque(currentMotorTorque);
                    rightWheelRow.SetTorque(currentMotorTorque);
                }
            }

            if (steering != 0 && (Mathf.Abs(leftWheelRow.minRPM) < 1 || Mathf.Abs(rightWheelRow.minRPM) < 1))
            {
                leftWheelRow.SetTorque(rotateTorqueInMotion * Mathf.Sign(currentMotorTorque));
                rightWheelRow.SetTorque(rotateTorqueInMotion * Mathf.Sign(currentMotorTorque));
            }
            else
            {
                if (steering < 0)
                {
                    rightWheelRow.SetTorque(rotateTorqueInMotion * Mathf.Sign(currentMotorTorque));
                    leftWheelRow.SetBrake(rotateBrakeInMotion);
                }

                if (steering > 0)
                {
                    leftWheelRow.SetTorque(rotateTorqueInMotion * Mathf.Sign(currentMotorTorque));
                    rightWheelRow.SetBrake(rotateBrakeInMotion);
                }
            }

            leftWheelRow.SetSidewaysStiffness(1.0f + minSidewayStiffnessInMotion - Mathf.Abs(steering));
            rightWheelRow.SetSidewaysStiffness(1.0f + minSidewayStiffnessInMotion - Mathf.Abs(steering));
        }

        leftWheelRow.UpdateMeshTransform();
        rightWheelRow.UpdateMeshTransform();
    }
}