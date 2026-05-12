using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

namespace Ezereal
{
    public class EzerealCarController : MonoBehaviour // This is the main system resposible for car control.
    {
        [Header("Ezereal References")]

        [SerializeField] EzerealLightController ezerealLightController;
        [SerializeField] EzerealSoundController ezerealSoundController;
        [SerializeField] EzerealWheelFrictionController ezerealWheelFrictionController;
        private PlayerInput _playerInput;

        [Header("References")]

        public Rigidbody vehicleRB;
        public WheelCollider frontLeftWheelCollider;
        public WheelCollider frontRightWheelCollider;
        public WheelCollider rearLeftWheelCollider;
        public WheelCollider rearRightWheelCollider;
        WheelCollider[] wheels;

        [SerializeField] Transform frontLeftWheelMesh;
        [SerializeField] Transform frontRightWheelMesh;
        [SerializeField] Transform rearLeftWheelMesh;
        [SerializeField] Transform rearRightWheelMesh;

        [SerializeField] Transform steeringWheel;

        [SerializeField] TMP_Text currentGearTMP_UI;
        [SerializeField] TMP_Text currentGearTMP_Dashboard;

        [SerializeField] TMP_Text curretnRPMMeterTMP_UI;
        [SerializeField] TMP_Text currentRPMMeterTMP_Dashboard;

        [SerializeField] TMP_Text currentSpeedTMP_UI;
        [SerializeField] TMP_Text currentSpeedTMP_Dashboard;
        [SerializeField] Slider accelerationSlider;

        [Header("Settings")]
        public bool isStarted = true;
        public bool isUsingSteeringWheel = true;

        public float maxForwardSpeed = 50f; // 100f default
        public float maxReverseSpeed = 30f; // 30f default
        public float horsePower = 1000f; // 100f0 default
        public float brakePower = 2000f; // 2000f default
        public float handbrakeForce = 3000f; // 3000f default
        public float maxSteerAngle = 30f; // 30f default
        public float steeringSpeed = 5f; // 0.5f default
        public float stopThreshold = 1f; // 1f default. At what speed car will make a full stop
        public float decelerationSpeed = 0.5f; // 0.5f default
        public float maxSteeringWheelRotation = 360f; // 360 for real steering wheel. 120 would be more suitable for racing.

        [Header("Drivetrain")]
        public AnimationCurve torqueCurve; // Torque curve for calculating engine torque based on RPM. Set this up in the inspector for different engine characteristics.
        private float[] gearRatios = { 0f, 3.8f, 2.5f, 1.6f, 1.2f, 1f, 0.8f }; // Neutral, First, Second, Third, Fourth, Fifth, Sixth
        private float reverseGearRatio = -3.5f; // Reverse gear ratio. Adjust as needed for different cars.
        private float finalDriveRatio = 6.7f; // Final drive ratio for calculating wheel torque from engine torque. Adjust as needed for different cars.

        [Header("Engine")]
        [SerializeField] float maxEngineRPM = 7000f; // Maximum engine RPM for the torque curve. Adjust as needed for different engines.
        [SerializeField] float idleEngineRPM = 800f; // Engine RPM at idle. Adjust as needed for different engines.
        [SerializeField] float maxEngienTorque = 50f; // Maximum engine torque for scaling the torque curve. Adjust as needed for different engines.
        private float engineRPM = 0f;


        [Header("Drive Type")]
        public DriveTypes driveType = DriveTypes.RWD;

        [Header("Gearbox")]
        public AutomaticGears currentGear = AutomaticGears.Drive;
        public ManualGears currentManualGear = ManualGears.Neutral;
        [SerializeField] bool isManual = false;
        [SerializeField] bool autoClucth = true; // If true, the car will automatically stall if the engine RPM is too low for the current gear. Only applies to manual transmission.
        private bool isClutchDown = false;

        [Header("Engine Braking")]
        [SerializeField] float engineBrakingStrength = 250f;
        [SerializeField] float downshiftBrakingMultiplier = 2f;
        float lastEngineRPM;
        int lastGearIndex;

        [Header("Wheels")]
        [SerializeField] float wheelRadius = 0.35f; // Average wheel radius in meters. Adjust as needed for different cars.

        [Header("Debug Info")]
        public bool stationary = true;
        [SerializeField] float currentSpeed = 0f;
        [SerializeField] float currentAccelerationValue = 0f;
        [SerializeField] float currentBrakeValue = 0f;
        [SerializeField] float currentClutchValue = 0f;
        [SerializeField] float currentHandbrakeValue = 0f;
        [SerializeField] float currentSteerAngle = 0f;
        [SerializeField] float targetSteerAngle = 0f;
        [SerializeField] float FrontLeftWheelRPM = 0f;
        [SerializeField] float FrontRightWheelRPM = 0f;
        [SerializeField] float RearLeftWheelRPM = 0f;
        [SerializeField] float RearRightWheelRPM = 0f;


        [SerializeField] float speedFactor = 0f; // Leave at zero. Responsible for smooth acceleration and near-top-speed slowdown.

        private void Awake()
        {
            wheels = new WheelCollider[]
            {
            frontLeftWheelCollider,
            frontRightWheelCollider,
            rearLeftWheelCollider,
            rearRightWheelCollider,
            };

            wheelRadius = frontLeftWheelCollider.radius; // Set wheel radius based on the WheelCollider's radius. This assumes all wheels have the same radius. Adjust if your car has different sized wheels.update
            if (ezerealLightController == null)
            {
                Debug.LogWarning("EzerealLightController reference is missing. Ignore or attach one if you want to have light controls.");
            }

            if (ezerealSoundController == null)
            {
                Debug.LogWarning("EzerealSoundController reference is missing. Ignore or attach one if you want to have engine sounds.");
            }

            if (ezerealWheelFrictionController == null)
            {
                Debug.LogWarning("EzerealWheelFrictionController reference is missing. Ignore or attach one if you want to have friction controls.");
            }

            if (vehicleRB == null)
            {
                Debug.LogError("VehicleRB reference is missing for EzerealCarController!");
            }

            if (isStarted)
            {
                Debug.Log("Car is started.");

                if (ezerealLightController != null)
                {
                    ezerealLightController.MiscLightsOn();
                }

                if (ezerealSoundController != null)
                {
                    ezerealSoundController.TurnOnEngineSound();
                }
            }
        }

        private void Start()
        {
            EzerealSoundController ezerealSoundController = GetComponent<EzerealSoundController>();
            _playerInput = GetComponent<PlayerInput>();

            // Set up the torque curve with example values. Adjust these keyframes to create different engine characteristics.
            torqueCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(idleEngineRPM, 0.4f), new Keyframe(3500, 1f), new Keyframe(maxEngineRPM, 0.2f));
            SetGear(ManualGears.Neutral);

            if (isStarted)
            {
                ActivateHUD();
            }
            else
            {
                DisableHUD();
            }
        }


        void OnStopCar()
        {
            if (!isStarted) return;

            isStarted = false;
            ezerealSoundController.PlayEngineStopSound();
            Debug.Log("Engine Stalled");

            DisableHUD();

            if (ezerealLightController != null)
            {
                ezerealLightController.AllLightsOff();
            }

            if (ezerealSoundController != null)
            {
                ezerealSoundController.TurnOffEngineSound();
            }

            ApplyTorque(0);
        }
        void OnStartCar()
        {
            if (isStarted)
            {
                OnStopCar();
                return;
            }

            isStarted = true;
            ezerealSoundController.PlayEngineStartSound();

            if (currentManualGear != ManualGears.Neutral && currentManualGear != ManualGears.First && currentManualGear != ManualGears.Reverse)
            {
                Debug.LogWarning("Car is in high gear. Can't start the car. Shift to lower gear.");
                return;
            }

            Debug.Log("Car started.");
            ActivateHUD();

            if (ezerealLightController != null)
            {
                ezerealLightController.MiscLightsOn();
            }

            if (ezerealSoundController != null)
            {
                ezerealSoundController.TurnOnEngineSound();
            }
        }

        void DisableHUD()
        {
            Debug.Log("Disabling HUD");
            currentGearTMP_UI.gameObject.SetActive(false);
            currentGearTMP_Dashboard.gameObject.SetActive(false);
            currentSpeedTMP_UI.gameObject.SetActive(false);
            currentSpeedTMP_Dashboard.gameObject.SetActive(false);
            curretnRPMMeterTMP_UI.gameObject.SetActive(false);
            currentRPMMeterTMP_Dashboard.gameObject.SetActive(false);
            accelerationSlider.gameObject.SetActive(false);
        }

        void ActivateHUD()
        {
            Debug.Log("Activating HUD");
            currentGearTMP_UI.gameObject.SetActive(true);
            currentGearTMP_Dashboard.gameObject.SetActive(true);
            currentSpeedTMP_Dashboard.gameObject.SetActive(true);
            currentSpeedTMP_UI.gameObject.SetActive(true);
            curretnRPMMeterTMP_UI.gameObject.SetActive(true);
            currentRPMMeterTMP_Dashboard.gameObject.SetActive(true);
            accelerationSlider.gameObject.SetActive(true);
        }

        void OnAccelerate(InputValue accelerationValue)
        {
            if (isUsingSteeringWheel)
            {
                currentAccelerationValue = 1f - accelerationValue.Get<float>();
            }
            else
            {
                currentAccelerationValue = accelerationValue.Get<float>();
            }
        }

        void OnBrake(InputValue brakeValue)
        {

            if (isUsingSteeringWheel)
            {
                currentBrakeValue = 1f - brakeValue.Get<float>();
            }
            else
            {
                currentBrakeValue = brakeValue.Get<float>();
            }

            if (isStarted && ezerealLightController != null)
            {
                if (currentBrakeValue > 0.05f)
                {
                    ezerealLightController.BrakeLightsOn();
                }
                else
                {
                    ezerealLightController.BrakeLightsOff();
                }
            }
        }

        void OnClutch(InputValue clutchValue)
        {
            if (isManual)
            {
                currentClutchValue = 1f - clutchValue.Get<float>();

                // deadzones
                if (currentClutchValue < 0.05f)
                {
                    currentClutchValue = 0f;
                }
                else if (currentClutchValue > 0.95f)
                {
                    currentClutchValue = 1f;
                }

                isClutchDown = currentClutchValue < 0.2f;

                if (autoClucth)
                {
                    isClutchDown = true;
                    return;
                }

                if (currentClutchValue < 0.5f)
                {
                    isClutchDown = true;
                }
                else
                {
                    isClutchDown = false;
                }
            }
        }

        float GetMaxSpeedForGear(float gearRation)
        {
            float maxWheelRPM = maxEngineRPM / (Mathf.Abs(gearRation) * finalDriveRatio);
            float wheelCircumference = 2 * Mathf.PI * wheelRadius;
            float maxSpeed = (maxWheelRPM * wheelCircumference) / 60f; // Convert from m/s to km/h
            return maxSpeed;
        }

        void ManualAcceleration()
        {
            if (!isStarted)
                return;

            float gearRatio = GetCurrentGearRatio();

            // Engine torque from torque curve
            float normalizedTorque = torqueCurve.Evaluate(engineRPM);
            float engineTorque = normalizedTorque * maxEngienTorque;

            // Soft RPM limiter
            float rpmLimiterFactor =
                1f - Mathf.InverseLerp(maxEngineRPM - 200f, maxEngineRPM, engineRPM);

            // Low RPM bogging
            float bogFactor =
                Mathf.InverseLerp(idleEngineRPM, idleEngineRPM + 800f, engineRPM);

            // Prevent unrealistic high gear launches
            float drivetrainLoad =
                Mathf.InverseLerp(0f, 15f, Mathf.Abs(rearLeftWheelCollider.rpm));

            // 0 = clutch pressed
            // 1 = clutch released
            float clutchEngagement = currentClutchValue;

            // MAIN DRIVE TORQUE
            float wheelTorque =
                engineTorque *
                gearRatio *
                finalDriveRatio *
                currentAccelerationValue *
                rpmLimiterFactor *
                bogFactor *
                drivetrainLoad *
                clutchEngagement;

            // =========================================================
            // ENGINE BRAKING
            // =========================================================

            bool throttleReleased = currentAccelerationValue < 0.05f;

            if (currentManualGear != ManualGears.Neutral &&
                throttleReleased &&
                clutchEngagement > 0.8f)
            {
                float wheelRPM =
                    Mathf.Abs(rearLeftWheelCollider.rpm);

                // Stronger braking at higher RPM
                float rpmBrakeFactor =
                    Mathf.InverseLerp(idleEngineRPM, maxEngineRPM, engineRPM);

                float engineBrakeTorque =
                    engineBrakingStrength *
                    gearRatio *
                    finalDriveRatio *
                    rpmBrakeFactor;

                // Downshift braking boost
                if ((int)currentManualGear < lastGearIndex)
                {
                    engineBrakeTorque *= downshiftBrakingMultiplier;
                }

                // Apply opposite torque
                wheelTorque -= engineBrakeTorque;
            }

            ApplyTorque(wheelTorque);

            UpdateAccelerationSlider();

            lastGearIndex = (int)currentManualGear;
        }

        void UpdateEngineRPM()
        {
            if (!isStarted)
            {
                engineRPM = 0f;
                return;
            }

            float gearRatio = GetCurrentGearRatio();

            // Wheel-connected RPM
            float wheelRPMConnection =
                Mathf.Abs(
                    rearLeftWheelCollider.rpm *
                    gearRatio *
                    finalDriveRatio
                );

            // Free rev RPM from throttle
            float targetFreeRPM =
                idleEngineRPM +
                ((maxEngineRPM - idleEngineRPM) * currentAccelerationValue);

            // 0 = clutch pressed
            // 1 = clutch released
            float clutchEngagement = currentClutchValue;

            if (currentManualGear == ManualGears.Neutral)
            {
                // Neutral free rev
                engineRPM = Mathf.Lerp(
                    engineRPM,
                    targetFreeRPM,
                    Time.deltaTime * 4f
                );
            }
            else
            {
                // Blend wheel RPM and free RPM
                float targetRPM =
                    (wheelRPMConnection * clutchEngagement) +
                    (targetFreeRPM * (1f - clutchEngagement));

                // Engine inertia
                float rpmChangeSpeed =
                    clutchEngagement > 0.8f
                    ? 8f
                    : 3f;

                engineRPM = Mathf.Lerp(
                    engineRPM,
                    targetRPM,
                    Time.deltaTime * rpmChangeSpeed
                );
            }

            // Stall protection
            if (engineRPM < idleEngineRPM)
            {
                engineRPM = idleEngineRPM;
            }

            engineRPM = Mathf.Clamp(
                engineRPM,
                idleEngineRPM,
                maxEngineRPM
            );

            // UI
            curretnRPMMeterTMP_UI.text =
                engineRPM.ToString("F0");

            currentRPMMeterTMP_Dashboard.text =
                engineRPM.ToString("F0");

            lastEngineRPM = engineRPM;
        }

        float GetCurrentGearRatio()
        {
            if (currentManualGear == ManualGears.Reverse)
            {
                return reverseGearRatio;
            }
            else
            {
                return gearRatios[(int)currentManualGear];
            }
        }

        void ApplyTorque(float torque)
        {
            switch (driveType)
            {
                case DriveTypes.FWD:
                    frontLeftWheelCollider.motorTorque = torque;
                    frontRightWheelCollider.motorTorque = torque;
                    rearLeftWheelCollider.motorTorque = 0;
                    rearRightWheelCollider.motorTorque = 0;
                    break;

                case DriveTypes.RWD:
                    frontLeftWheelCollider.motorTorque = 0;
                    frontRightWheelCollider.motorTorque = 0;
                    rearLeftWheelCollider.motorTorque = torque;
                    rearRightWheelCollider.motorTorque = torque;
                    break;

                case DriveTypes.AWD:
                    frontLeftWheelCollider.motorTorque = torque;
                    frontRightWheelCollider.motorTorque = torque;
                    rearLeftWheelCollider.motorTorque = torque;
                    rearRightWheelCollider.motorTorque = torque;
                    break;
            }
        }

        
        ////*****This is the original accelaration method, which is now used for automatic transmission*****////
        void Acceleration()
        {
            if (isStarted)
            {
                if (currentGear == AutomaticGears.Drive)
                {
                    // Calculate how close the car is to top speed
                    // as a number from zero to one
                    speedFactor = Mathf.InverseLerp(0, maxForwardSpeed, currentSpeed);

                    // Use that to calculate how much torque is available 
                    // (zero torque at top speed)
                    float currentMotorTorque = Mathf.Lerp(horsePower, 0, speedFactor);

                    if (currentAccelerationValue > 0.05f && currentSpeed < maxForwardSpeed)
                    {
                        if (driveType == DriveTypes.RWD)
                        {
                            rearLeftWheelCollider.motorTorque = currentMotorTorque * currentAccelerationValue;
                            rearRightWheelCollider.motorTorque = currentMotorTorque * currentAccelerationValue;
                        }
                        else if (driveType == DriveTypes.FWD)
                        {
                            frontLeftWheelCollider.motorTorque = currentMotorTorque * currentAccelerationValue;
                            frontRightWheelCollider.motorTorque = currentMotorTorque * currentAccelerationValue;
                        }
                        else if (driveType == DriveTypes.AWD)
                        {
                            frontLeftWheelCollider.motorTorque = currentMotorTorque * currentAccelerationValue;
                            frontRightWheelCollider.motorTorque = currentMotorTorque * currentAccelerationValue;
                            rearLeftWheelCollider.motorTorque = currentMotorTorque * currentAccelerationValue;
                            rearRightWheelCollider.motorTorque = currentMotorTorque * currentAccelerationValue;
                        }
                    }
                    else
                    {
                        frontLeftWheelCollider.motorTorque = 0;
                        frontRightWheelCollider.motorTorque = 0;
                        rearLeftWheelCollider.motorTorque = 0;
                        rearRightWheelCollider.motorTorque = 0;
                    }
                }

                if (currentGear == AutomaticGears.Reverse)
                {
                    if (currentAccelerationValue > 0f && currentSpeed > -maxReverseSpeed)
                    {
                        currentAccelerationValue = 1; //Invert Acceleration value

                        if (driveType == DriveTypes.RWD)
                        {
                            rearLeftWheelCollider.motorTorque = -currentAccelerationValue * horsePower;
                            rearRightWheelCollider.motorTorque = -currentAccelerationValue * horsePower;
                        }
                        else if (driveType == DriveTypes.FWD)
                        {
                            frontLeftWheelCollider.motorTorque = -currentAccelerationValue * horsePower;
                            frontRightWheelCollider.motorTorque = -currentAccelerationValue * horsePower;
                        }
                        else if (driveType == DriveTypes.AWD)
                        {
                            frontLeftWheelCollider.motorTorque = -currentAccelerationValue * horsePower;
                            frontRightWheelCollider.motorTorque = -currentAccelerationValue * horsePower;
                            rearLeftWheelCollider.motorTorque = -currentAccelerationValue * horsePower;
                            rearRightWheelCollider.motorTorque = -currentAccelerationValue * horsePower;
                        }

                    }
                    else
                    {
                        frontLeftWheelCollider.motorTorque = 0;
                        frontRightWheelCollider.motorTorque = 0;
                        rearLeftWheelCollider.motorTorque = 0;
                        rearRightWheelCollider.motorTorque = 0;
                    }
                }

                UpdateAccelerationSlider();
            }
        }

        // Apply brake torque
        void Braking()
        {
            if (currentBrakeValue > 0f)
            {
                frontLeftWheelCollider.brakeTorque = currentBrakeValue * brakePower;
                frontRightWheelCollider.brakeTorque = currentBrakeValue * brakePower;
            }
            else
            {
                frontLeftWheelCollider.brakeTorque = 0;
                frontRightWheelCollider.brakeTorque = 0;
            }
        }

        void OnHandbrake(InputValue handbrakeValue)
        {
            currentHandbrakeValue = handbrakeValue.Get<float>();

            if (isStarted)
            {
                if (currentHandbrakeValue > 0)
                {
                    if (ezerealWheelFrictionController != null)
                    {
                        ezerealWheelFrictionController.StartDrifting(currentHandbrakeValue);
                    }

                    if (ezerealLightController != null)
                    {
                        ezerealLightController.HandbrakeLightOn();
                    }
                }
                else
                {
                    if (ezerealWheelFrictionController != null)
                    {
                        ezerealWheelFrictionController.StopDrifting();
                    }

                    if (ezerealLightController != null)
                    {
                        ezerealLightController.HandbrakeLightOff();
                    }
                }
            }
        }

        void Handbraking()
        {
            if (currentHandbrakeValue > 0f)
            {
                rearLeftWheelCollider.motorTorque = 0;
                rearRightWheelCollider.motorTorque = 0;
                rearLeftWheelCollider.brakeTorque = currentHandbrakeValue * handbrakeForce;
                rearRightWheelCollider.brakeTorque = currentHandbrakeValue * handbrakeForce;


            }
            else
            {
                rearLeftWheelCollider.brakeTorque = 0;
                rearRightWheelCollider.brakeTorque = 0;
            }
        }

        void OnSteer(InputValue turnValue)
        {
            targetSteerAngle = turnValue.Get<float>() * maxSteerAngle;
        }

        void Steering()
        {
            float adjustedspeedFactor = Mathf.InverseLerp(20, maxForwardSpeed, currentSpeed); //minimum speed affecting steerAngle is 20
            float adjustedTurnAngle = targetSteerAngle * (1 - adjustedspeedFactor); //based on current speed.
            currentSteerAngle = Mathf.Lerp(currentSteerAngle, adjustedTurnAngle, Time.deltaTime * steeringSpeed);

            frontLeftWheelCollider.steerAngle = currentSteerAngle;
            frontRightWheelCollider.steerAngle = currentSteerAngle;

            UpdateWheel(frontLeftWheelCollider, frontLeftWheelMesh);
            UpdateWheel(frontRightWheelCollider, frontRightWheelMesh);
            UpdateWheel(rearLeftWheelCollider, rearLeftWheelMesh);
            UpdateWheel(rearRightWheelCollider, rearRightWheelMesh);
        }

        void Slowdown()
        {
            if (vehicleRB != null)
            {
                if (currentAccelerationValue == 0 && currentBrakeValue == 0 && currentHandbrakeValue == 0)
                {
#if UNITY_6000_0_OR_NEWER
                    vehicleRB.linearVelocity = Vector3.Lerp(vehicleRB.linearVelocity, Vector3.zero, Time.deltaTime * decelerationSpeed);
#else
                    vehicleRB.velocity = Vector3.Lerp(vehicleRB.velocity, Vector3.zero, Time.deltaTime * decelerationSpeed);
#endif
                }
            }
        }

        void OnDownShift()
        {
            switch (currentGear)
            {
                case AutomaticGears.Reverse:
                    //Debug.Log("Reverse, can't go any lower");
                    break;

                case AutomaticGears.Neutral:
                    currentGear--;
                    UpdateGearText("R");
                    if (isStarted && ezerealLightController != null)
                    {
                        ezerealLightController.ReverseLightsOn();
                    }
                    break;

                case AutomaticGears.Drive:
                    currentGear--;
                    UpdateGearText("N");
                    break;
            }
        }

        void OnUpShift()
        {
            switch (currentGear)
            {
                case AutomaticGears.Reverse:
                    currentGear++;
                    UpdateGearText("N");

                    if (isStarted && ezerealLightController != null)
                    {
                        ezerealLightController.ReverseLightsOff();
                    }

                    break;
                case AutomaticGears.Neutral:
                    currentGear++;
                    UpdateGearText("D");
                    break;
                case AutomaticGears.Drive:
                    //Debug.Log("Drive, can't go any higher");
                    break;
            }
        }

        /// ****Gear metod for manula transition *****/////
        void SetGear(ManualGears gear)
        {
            currentManualGear = gear;
            switch (currentManualGear)
            {
                case ManualGears.Reverse:
                    UpdateGearText("R");
                    break;
                case ManualGears.Neutral:
                    UpdateGearText("N");
                    break;
                case ManualGears.First:
                    UpdateGearText("1");
                    break;
                case ManualGears.Second:
                    UpdateGearText("2");
                    break;
                case ManualGears.Third:
                    UpdateGearText("3");
                    break;
                case ManualGears.Fourth:
                    UpdateGearText("4");
                    break;
                case ManualGears.Fifth:
                    UpdateGearText("5");
                    break;
                case ManualGears.Sixth:
                    UpdateGearText("6");
                    break;
            }
        }

        void OnGearReverse(InputValue value)
        {
            if (value.isPressed)
            {
                SetGear(ManualGears.Reverse);
                if (!isClutchDown)
                {
                    Debug.Log("Engine RPM too low for current gear, car engine stoped!");
                    OnStopCar();
                }
            }
            else if (isUsingSteeringWheel)
            {
                SetGear(ManualGears.Neutral);
            }
        }
        void OnGearNeutral() => SetGear(ManualGears.Neutral);
        void OnGear1(InputValue value)
        {
            if (value.isPressed)
            {
                SetGear(ManualGears.First);
                if (!isClutchDown)
                {
                    Debug.Log("Engine RPM too low for current gear, car engine stoped!");
                    OnStopCar();
                }
            }
            else if (isUsingSteeringWheel)
            {
                SetGear(ManualGears.Neutral);
            }
        }

        void OnGear2(InputValue value)
        {
            if (value.isPressed)
            {
                SetGear(ManualGears.Second);

                if (!isClutchDown)
                {
                    Debug.Log("Engine RPM too low for current gear, car engine stoped!");
                    OnStopCar();
                }

            }
            else if (isUsingSteeringWheel)
            {
                SetGear(ManualGears.Neutral);
            }
        }

        void OnGear3(InputValue value)
        {
            if (value.isPressed)
            {
                SetGear(ManualGears.Third);

                if (!isClutchDown)
                {
                    Debug.Log("Engine RPM too low for current gear, car engine stoped!");
                    OnStopCar();
                }
            }
            else if (isUsingSteeringWheel)
            {
                SetGear(ManualGears.Neutral);
            }
        }
        void OnGear4(InputValue value)
        {
            if (value.isPressed)
            {
                SetGear(ManualGears.Fourth);

                if (!isClutchDown)
                {
                    Debug.Log("Engine RPM too low for current gear, car engine stoped!");
                    OnStopCar();
                }
            }
            else if (isUsingSteeringWheel)
            {
                SetGear(ManualGears.Neutral);
            }
        }
        void OnGear5(InputValue value)
        {
            if (value.isPressed)
            {
                SetGear(ManualGears.Fifth);

                if (!isClutchDown)
                {
                    Debug.Log("Engine RPM too low for current gear, car engine stoped!");
                    OnStopCar();
                }
            }
            else if (isUsingSteeringWheel)
            {
                SetGear(ManualGears.Neutral);
            }
        }
        void OnGear6(InputValue value)
        {
            if (value.isPressed)
            {
                SetGear(ManualGears.Sixth);

                if (!isClutchDown)
                {
                    Debug.Log("Engine RPM too low for current gear, car engine stoped!");
                    OnStopCar();
                }
            }
            else if (isUsingSteeringWheel)
            {
                SetGear(ManualGears.Neutral);
            }
        }



        void StallingEngine()
        {
            if (!isStarted || currentManualGear == ManualGears.Neutral || isClutchDown) return;

            // 1. Calculate what the engine RPM WOULD be based on wheel speed
            float gearRatio = GetCurrentGearRatio();
            float wheelRPM = rearLeftWheelCollider.rpm;
            float calculatedEngineRPM = Mathf.Abs(wheelRPM * gearRatio * finalDriveRatio);

            // 2. If the wheels are moving so slowly that the engine is forced below a "stall threshold"
            float stallThreshold = idleEngineRPM;

            if (calculatedEngineRPM < stallThreshold)
            {
                Debug.Log("Engine stalled: Load too high at low speed.");
                OnStopCar();
            }
        }

        void HandleClutchEngagement()
        {
            if (!isStarted || currentManualGear == ManualGears.Neutral) return;

            // If the clutch is being engaged (let go)
            if (!isClutchDown)
            {
                // If the car is barely moving and the player isn't giving it enough gas (RPM is low)
                if (rearLeftWheelCollider.rpm < 50f && engineRPM < idleEngineRPM * 1.2f)
                {
                    Debug.Log("Stalled: Popped the clutch with no gas!");
                    OnStopCar();
                }
            }
        }

        private void FixedUpdate()
        {
            UpdateEngineRPM();

            if (isManual)
            {
                HandleClutchEngagement();
               // StallingEngine();
                ManualAcceleration();
            }

            if (!isManual)
            {
                Acceleration();
            }

            Braking();

            Handbraking();

            Steering();

            Slowdown();

            RotateSteeringWheel();

            if (Mathf.Abs(frontLeftWheelCollider.rpm) < stopThreshold &&
                Mathf.Abs(frontRightWheelCollider.rpm) < stopThreshold &&
                Mathf.Abs(rearLeftWheelCollider.rpm) < stopThreshold &&
                Mathf.Abs(rearRightWheelCollider.rpm) < stopThreshold)
            {
                stationary = true;
            }
            else
            {
                stationary = false;
            }

            if (vehicleRB != null) // Unity uses m/s as for default. So I convert from m/s to km/h. For mph use 2.23694f instead of 3.6f.
            {
#if UNITY_6000_0_OR_NEWER
                currentSpeed = Vector3.Dot(vehicleRB.gameObject.transform.forward, vehicleRB.linearVelocity);
                currentSpeed *= 3.6f;
                UpdateSpeedText(currentSpeed);
#else
                currentSpeed = Vector3.Dot(vehicleRB.gameObject.transform.forward, vehicleRB.velocity);
                currentSpeed *= 3.6f; 
                UpdateSpeedText(currentSpeed);
#endif

            }


            FrontLeftWheelRPM = frontLeftWheelCollider.rpm;
            FrontRightWheelRPM = frontRightWheelCollider.rpm;
            RearLeftWheelRPM = rearLeftWheelCollider.rpm;
            RearRightWheelRPM = rearRightWheelCollider.rpm;
        }

        private void UpdateWheel(WheelCollider col, Transform mesh)
        {
            col.GetWorldPose(out Vector3 position, out Quaternion rotation);
            mesh.SetPositionAndRotation(position, rotation);
        }


        void RotateSteeringWheel()
        {
            float currentXAngle = steeringWheel.transform.localEulerAngles.x; // Maximum steer angle in degrees

            // Calculate the rotation based on the steer angle
            float normalizedSteerAngle = Mathf.Clamp(frontLeftWheelCollider.steerAngle, -maxSteerAngle, maxSteerAngle);
            float rotation = Mathf.Lerp(maxSteeringWheelRotation, -maxSteeringWheelRotation, (normalizedSteerAngle + maxSteerAngle) / (2 * maxSteerAngle));

            // Set the local rotation of the steering wheel
            steeringWheel.localRotation = Quaternion.Euler(currentXAngle, 0, rotation);
        }

        void UpdateGearText(string gear)
        {
            currentGearTMP_UI.text = gear;
            currentGearTMP_Dashboard.text = gear;
        }

        void UpdateSpeedText(float speed)
        {
            speed = Mathf.Abs(speed);

            currentSpeedTMP_UI.text = speed.ToString("F0");
            currentSpeedTMP_Dashboard.text = speed.ToString("F0");
        }

        void UpdateAccelerationSlider()
        {
            if (currentGear == AutomaticGears.Drive || currentGear == AutomaticGears.Reverse)
            {
                accelerationSlider.value = Mathf.Lerp(accelerationSlider.value, currentAccelerationValue, Time.deltaTime * 15f);
            }
            else
            {
                accelerationSlider.value = 0;
            }
        }

        public bool InAir()
        {
            foreach (WheelCollider wheel in wheels)
            {
                if (wheel.GetGroundHit(out _))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
