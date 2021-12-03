using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

namespace VRInputsAPI
{

    public enum VRNodeStatus
    {
        UNCHECKED, NO_DEVICE, MULTIPLE_DEVICES, DEVICE_OK
    }

    public enum VRManufacturer
    {
        OVR_HTCVIVE,

        OVR_WINDOWS_MR, // Manufacturer = "WindowsMR: 0x045E" (Lenovo Win MR left controller)

        OVR_VALVE_INDEX // Manufacturer/name/SN are 'Valve'/'OpenVR Controller(Knuckles Left) - Left'/'LHR-xxxxxxxx'
                        // Manufacturer/name/SN are 'Valve'/'OpenVR Controller(Knuckles Right) - Right'/'LHR-xxxxxxxx'
    }


    public class VRDevice
    {
        public readonly VRInputsManager inputs;
        internal readonly GameObject gameObject;
        public readonly XRNode xRNode;
        public VRManufacturer? manufacturer { get; protected set; }
        private InputDevice? device = null;
        VRNodeStatus status = VRNodeStatus.UNCHECKED;

        internal VRDevice(VRInputsManager man, GameObject obj, XRNode node)
        {
            inputs = man;
            gameObject = obj;
            xRNode = node;
        }

        
        private void InitDevice()
        {
            var leftHandDevices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(xRNode, leftHandDevices);

            if (leftHandDevices.Count == 1)
            {
                device = leftHandDevices[0];
                status = VRNodeStatus.DEVICE_OK;
                DetectManufacturer();
                Debug.Log(xRNode + " is now detected. Manufacturer/name/SN are '" + device.Value.manufacturer + "'/'" + device.Value.name + "'/'" + device.Value.serialNumber + "'. Recognized manufacturer is " + manufacturer);
            }
            else if (leftHandDevices.Count > 1)
            {
                if (status != VRNodeStatus.MULTIPLE_DEVICES)
                    Debug.LogWarning(leftHandDevices.Count + " devices found for " + xRNode + " but needs only one. Please disconnect all the extra devices.");
                status = VRNodeStatus.MULTIPLE_DEVICES;
            }
            else
            {
                if (status != VRNodeStatus.NO_DEVICE)
                    Debug.LogWarning("No device found for " + xRNode + "!");
                status = VRNodeStatus.NO_DEVICE;
            }
        }


        private void DetectManufacturer()
        {
            if (device.Value.manufacturer.Contains("WindowsMR"))
                manufacturer = VRManufacturer.OVR_WINDOWS_MR;
            else if (device.Value.manufacturer.Equals("Valve"))
                manufacturer = VRManufacturer.OVR_VALVE_INDEX;
            else
                manufacturer = VRManufacturer.OVR_HTCVIVE;
        }


        public bool Available
        {
            get => device.HasValue;
        }

        public InputDevice Device
        {
            get => device.Value;
        }



        public virtual GameObject GetGameObject() { return gameObject; }

        public virtual bool IsController() { return false; }

        public void CheckIsController()
        {
            if (!IsController())
                throw new Exception(this.GetType().Name + " n'est pas un controleur");
        }

        /// <summary>Attention à bien utiliser les positions/rotations relatives au parent et non relative au monde</summary>
        public virtual Transform GetTransform()
        {
            GameObject g = GetGameObject();
            return g == null ? null : g.transform;
        }

        internal TrackedPoseDriver GetTrackedPoseDriver()
        {
            return GetGameObject().GetComponent<TrackedPoseDriver>();
        }

        internal virtual void UpdateInputs()
        {
            if (!Available)
                InitDevice(); // detect connected device
            if (Available && !Device.isValid)
            {
                device = null; // detect disconnected device
                Debug.LogWarning("Device lost for " + xRNode + "!");
                status = VRNodeStatus.UNCHECKED;
            }
        }
    }






    public class VRController : VRDevice
    {
        public readonly GameObject simulatedController = null;

        private GameObject childContainer;
        public GameObject ChildContainer
        {
            get
            {
                return childContainer;
            }
            internal set
            {
                childContainer = value;
                childContainer.transform.parent = GetTransform();
                childContainer.transform.localPosition = Vector3.zero;
                childContainer.transform.localRotation = Quaternion.identity;
            }
        }

        internal VRController(VRInputsManager man, GameObject obj, XRNode node, GameObject cCont)
            : base(man, obj, node)
        {
            ChildContainer = cCont;
        }

        public override bool IsController() { return true; }





        public void AddChild(GameObject obj)
        {
            obj.transform.parent = ChildContainer.transform;
        }

        public void RemoveChild(GameObject obj)
        {
            obj.transform.parent = null;
        }


        internal override void UpdateInputs()
        {
            bool wasAvailable = Available;

            base.UpdateInputs(); // update Available status

            if (!wasAvailable && Available)
            {
                gameObject.transform.Find("Model Vive").gameObject.SetActive(!inputs.hideControllers && manufacturer == VRManufacturer.OVR_HTCVIVE);
                //gameObject.transform.Find("Model WinMR").gameObject.SetActive(!inputs.hideControllers && manufacturer == VRManufacturer.OVR_WINDOWS_MR);
                //gameObject.transform.Find("Model Index").gameObject.SetActive(!inputs.hideControllers && manufacturer == VRManufacturer.OVR_VALVE_INDEX);
            }


            /*
             * Common usages update
             */
            foreach (IVRInput i in commonUsages.Values)
                i.UpdateState();

            /*
             * HTC vive input update
             */

            if (_cacheHTCVive_PadAxis != null)
                _cacheHTCVive_PadAxis.UpdateState();

            if (_cacheHTCVive_TriggerDTPress != null)
                _cacheHTCVive_TriggerDTPress.UpdateState(); // important après triggerAxis (màj avec Common usages)

            /*
             * Windows MR input update
             */

            if (_cacheWinMR_PadAxis != null)
                _cacheWinMR_PadAxis.UpdateState();

            /*
             * Valve Index input update
             */

            if (_cacheValveIndex_PadAxis != null)
                _cacheValveIndex_PadAxis.UpdateState();

            /*
             * Compat position/speed/acceleration
             */

            if (_cachePosition != null)
                _cachePosition.UpdateState();

            if (_cachePositionBasedVelocity != null)
                _cachePositionBasedVelocity.UpdateState();

            if (_cacheVelocityBasedAcceleration != null)
                _cacheVelocityBasedAcceleration.UpdateState();
        }



        /*
         * Common usages input
         */

        private Dictionary<object, IVRInput> commonUsages = new Dictionary<object, IVRInput>();

        public VRBooleanInput GetCommonUsagesInput(InputFeatureUsage<bool> usage, string name)
        {
            if (!commonUsages.ContainsKey(usage))
                commonUsages[usage] = new VRBooleanInput(inputs, name, (out bool v) =>
                {
                    if (Available && Device.TryGetFeatureValue(usage, out bool val)) { v = val; return true; }
                    v = false; return false;
                });
            return (VRBooleanInput)commonUsages[usage];
        }

        public VR1DAxisInput GetCommonUsagesInput(InputFeatureUsage<float> usage, string name)
        {
            if (!commonUsages.ContainsKey(usage))
                commonUsages[usage] = new VR1DAxisInput(inputs, name, (out float v) =>
                {
                    if (Available && Device.TryGetFeatureValue(usage, out float val)) { v = val; return true; }
                    v = 0; return false;
                });
            return (VR1DAxisInput)commonUsages[usage];
        }

        public VR2DAxisInput GetCommonUsagesInput(InputFeatureUsage<Vector2> usage, string name)
        {
            if (!commonUsages.ContainsKey(usage))
                commonUsages[usage] = new VR2DAxisInput(inputs, name, (out Vector2 v) =>
                {
                    if (Available && Device.TryGetFeatureValue(usage, out Vector2 val)) { v = val; return true; }
                    v = Vector2.zero; return false;
                });
            return (VR2DAxisInput)commonUsages[usage];
        }

        public VR3DAxisInput GetCommonUsagesInput(InputFeatureUsage<Vector3> usage, string name)
        {
            if (!commonUsages.ContainsKey(usage))
                commonUsages[usage] = new VR3DAxisInput(inputs, name, (out Vector3 v) =>
                {
                    if (Available && Device.TryGetFeatureValue(usage, out Vector3 val)) { v = val; return true; }
                    v = Vector3.zero; return false;
                });
            return (VR3DAxisInput)commonUsages[usage];
        }

        public VRQuaternionInput GetCommonUsagesInput(InputFeatureUsage<Quaternion> usage, string name)
        {
            if (!commonUsages.ContainsKey(usage))
                commonUsages[usage] = new VRQuaternionInput(inputs, name, (out Quaternion v) =>
                {
                    if (Available && Device.TryGetFeatureValue(usage, out Quaternion val)) { v = val; return true; }
                    v = Quaternion.identity; return false;
                });
            return (VRQuaternionInput)commonUsages[usage];
        }

        public VRInput<InputTrackingState> GetCommonUsagesInput(InputFeatureUsage<InputTrackingState> usage, string name)
        {
            if (!commonUsages.ContainsKey(usage))
                commonUsages[usage] = new VRInput<InputTrackingState>(inputs, name, (out InputTrackingState v) =>
                {
                    if (Available && Device.TryGetFeatureValue(usage, out InputTrackingState val)) { v = val; return true; }
                    v = InputTrackingState.None; return false;
                });
            return (VRInput<InputTrackingState>)commonUsages[usage];
        }

        public VRInput<Hand> GetCommonUsagesInput(InputFeatureUsage<Hand> usage, string name)
        {
            if (!commonUsages.ContainsKey(usage))
                commonUsages[usage] = new VRInput<Hand>(inputs, name, (out Hand v) =>
                {
                    if (Available && Device.TryGetFeatureValue(usage, out Hand val)) { v = val; return true; }
                    v = new Hand(); return false;
                });
            return (VRInput<Hand>)commonUsages[usage];
        }

        public VRInput<Eyes> GetCommonUsagesInput(InputFeatureUsage<Eyes> usage, string name)
        {
            if (!commonUsages.ContainsKey(usage))
                commonUsages[usage] = new VRInput<Eyes>(inputs, name, (out Eyes v) =>
                {
                    if (Available && Device.TryGetFeatureValue(usage, out Eyes val)) { v = val; return true; }
                    v = new Eyes(); return false;
                });

            return (VRInput<Eyes>)commonUsages[usage];
        }

        public VRBooleanInput CU_isTracked => GetCommonUsagesInput(CommonUsages.isTracked, "isTracked");
        public VR3DAxisInput CU_deviceAngularVelocity => GetCommonUsagesInput(CommonUsages.deviceAngularVelocity, "deviceAngularVelocity");
        public VR3DAxisInput CU_leftEyeVelocity => GetCommonUsagesInput(CommonUsages.leftEyeVelocity, "leftEyeVelocity");
        public VR3DAxisInput CU_leftEyeAngularVelocity => GetCommonUsagesInput(CommonUsages.leftEyeAngularVelocity, "leftEyeAngularVelocity");
        public VR3DAxisInput CU_rightEyeVelocity => GetCommonUsagesInput(CommonUsages.rightEyeVelocity, "rightEyeVelocity");
        public VR3DAxisInput CU_rightEyeAngularVelocity => GetCommonUsagesInput(CommonUsages.rightEyeAngularVelocity, "rightEyeAngularVelocity");
        public VR3DAxisInput CU_centerEyeVelocity => GetCommonUsagesInput(CommonUsages.centerEyeVelocity, "centerEyeVelocity");
        public VR3DAxisInput CU_centerEyeAngularVelocity => GetCommonUsagesInput(CommonUsages.centerEyeAngularVelocity, "centerEyeAngularVelocity");
        public VR3DAxisInput CU_colorCameraVelocity => GetCommonUsagesInput(CommonUsages.colorCameraVelocity, "colorCameraVelocity");
        public VR3DAxisInput CU_colorCameraAngularVelocity => GetCommonUsagesInput(CommonUsages.colorCameraAngularVelocity, "colorCameraAngularVelocity");
        public VR3DAxisInput CU_deviceAcceleration => GetCommonUsagesInput(CommonUsages.deviceAcceleration, "deviceAcceleration");
        public VR3DAxisInput CU_deviceAngularAcceleration => GetCommonUsagesInput(CommonUsages.deviceAngularAcceleration, "deviceAngularAcceleration");
        public VR3DAxisInput CU_deviceVelocity => GetCommonUsagesInput(CommonUsages.deviceVelocity, "deviceVelocity");
        public VR3DAxisInput CU_leftEyeAcceleration => GetCommonUsagesInput(CommonUsages.leftEyeAcceleration, "leftEyeAcceleration");
        public VR3DAxisInput CU_rightEyeAcceleration => GetCommonUsagesInput(CommonUsages.rightEyeAcceleration, "rightEyeAcceleration");
        public VR3DAxisInput CU_rightEyeAngularAcceleration => GetCommonUsagesInput(CommonUsages.rightEyeAngularAcceleration, "rightEyeAngularAcceleration");
        public VR3DAxisInput CU_centerEyeAcceleration => GetCommonUsagesInput(CommonUsages.centerEyeAcceleration, "centerEyeAcceleration");
        public VR3DAxisInput CU_centerEyeAngularAcceleration => GetCommonUsagesInput(CommonUsages.centerEyeAngularAcceleration, "centerEyeAngularAcceleration");
        public VR3DAxisInput CU_colorCameraAcceleration => GetCommonUsagesInput(CommonUsages.colorCameraAcceleration, "colorCameraAcceleration");
        public VR3DAxisInput CU_colorCameraAngularAcceleration => GetCommonUsagesInput(CommonUsages.colorCameraAngularAcceleration, "colorCameraAngularAcceleration");
        public VRQuaternionInput CU_deviceRotation => GetCommonUsagesInput(CommonUsages.deviceRotation, "deviceRotation");
        public VRQuaternionInput CU_leftEyeRotation => GetCommonUsagesInput(CommonUsages.leftEyeRotation, "leftEyeRotation");
        public VRQuaternionInput CU_rightEyeRotation => GetCommonUsagesInput(CommonUsages.rightEyeRotation, "rightEyeRotation");
        public VRQuaternionInput CU_centerEyeRotation => GetCommonUsagesInput(CommonUsages.centerEyeRotation, "centerEyeRotation");
        public VRQuaternionInput CU_colorCameraRotation => GetCommonUsagesInput(CommonUsages.colorCameraRotation, "colorCameraRotation");
        public VR3DAxisInput CU_leftEyeAngularAcceleration => GetCommonUsagesInput(CommonUsages.leftEyeAngularAcceleration, "leftEyeAngularAcceleration");
        public VR3DAxisInput CU_colorCameraPosition => GetCommonUsagesInput(CommonUsages.colorCameraPosition, "colorCameraPosition");
        public VR3DAxisInput CU_centerEyePosition => GetCommonUsagesInput(CommonUsages.centerEyePosition, "centerEyePosition");
        public VR3DAxisInput CU_rightEyePosition => GetCommonUsagesInput(CommonUsages.rightEyePosition, "rightEyePosition");
        public VRBooleanInput CU_primaryButton => GetCommonUsagesInput(CommonUsages.primaryButton, "primaryButton");
        public VRBooleanInput CU_primaryTouch => GetCommonUsagesInput(CommonUsages.primaryTouch, "primaryTouch");
        public VRBooleanInput CU_secondaryButton => GetCommonUsagesInput(CommonUsages.secondaryButton, "secondaryButton");
        public VRBooleanInput CU_secondaryTouch => GetCommonUsagesInput(CommonUsages.secondaryTouch, "secondaryTouch");
        public VRBooleanInput CU_gripButton => GetCommonUsagesInput(CommonUsages.gripButton, "gripButton");
        public VRBooleanInput CU_triggerButton => GetCommonUsagesInput(CommonUsages.triggerButton, "triggerButton");
        public VRBooleanInput CU_menuButton => GetCommonUsagesInput(CommonUsages.menuButton, "menuButton");
        public VRBooleanInput CU_primary2DAxisClick => GetCommonUsagesInput(CommonUsages.primary2DAxisClick, "primary2DAxisClick");
        public VRBooleanInput CU_primary2DAxisTouch => GetCommonUsagesInput(CommonUsages.primary2DAxisTouch, "primary2DAxisTouch");
        public VRInput<InputTrackingState> CU_trackingState => GetCommonUsagesInput(CommonUsages.trackingState, "trackingState");
        public VR1DAxisInput CU_batteryLevel => GetCommonUsagesInput(CommonUsages.batteryLevel, "batteryLevel");
        public VR1DAxisInput CU_trigger => GetCommonUsagesInput(CommonUsages.trigger, "trigger");
        public VR1DAxisInput CU_grip => GetCommonUsagesInput(CommonUsages.grip, "grip");
        public VR2DAxisInput CU_primary2DAxis => GetCommonUsagesInput(CommonUsages.primary2DAxis, "primary2DAxis");
        public VR2DAxisInput CU_secondary2DAxis => GetCommonUsagesInput(CommonUsages.secondary2DAxis, "secondary2DAxis");
        public VR3DAxisInput CU_devicePosition => GetCommonUsagesInput(CommonUsages.devicePosition, "devicePosition");
        public VR3DAxisInput CU_leftEyePosition => GetCommonUsagesInput(CommonUsages.leftEyePosition, "leftEyePosition");
        public VRInput<Hand> CU_handData => GetCommonUsagesInput(CommonUsages.handData, "handData");
        public VRInput<Eyes> CU_eyesData => GetCommonUsagesInput(CommonUsages.eyesData, "eyesData");


        /*
         * HTC Vive
         */

        public VRBooleanInput HTCVive_Menu => CU_primaryButton;
        public VRBooleanInput HTCVive_Grip => CU_gripButton;
        public VRBooleanInput HTCVive_PadPress => CU_primary2DAxisClick;
        public VRBooleanInput HTCVive_PadTouch => CU_primary2DAxisTouch;
        public VR1DAxisInput HTCVive_TriggerAxis => CU_trigger;
        public VRBooleanInput HTCVive_TriggerPress => CU_triggerButton;

        private VRCircularTouchpad _cacheHTCVive_PadAxis;
        public VRCircularTouchpad HTCVive_PadAxis => _cacheHTCVive_PadAxis ?? (_cacheHTCVive_PadAxis = new VRCircularTouchpad(CU_primary2DAxis, () => HTCVive_PadTouch.Value, 0.02f));

        private VRBooleanInput _cacheHTCVive_TriggerDTPress;
        public VRBooleanInput TriggerDTPress => _cacheHTCVive_TriggerDTPress ?? (_cacheHTCVive_TriggerDTPress = new VRMultiThresholdedInput(
                HTCVive_TriggerAxis.GetThresholdedInput(0.10f, 0.13f),
                HTCVive_TriggerAxis.GetThresholdedInput(0.99f, 0.99f)
            ));


        /*
         * Windows MR
         */

        private VRCircularTouchpad _cacheWinMR_PadAxis;
        public VRCircularTouchpad WinMR_PadAxis => _cacheWinMR_PadAxis ?? (_cacheWinMR_PadAxis = new VRCircularTouchpad(CU_secondary2DAxis, null, 0.0135f));

        /*
         * Valve Index
         */

        private VRCircularTouchpad _cacheValveIndex_PadAxis;
        public VRCircularTouchpad ValveIndex_PadAxis => _cacheValveIndex_PadAxis ?? (_cacheValveIndex_PadAxis = new VRCircularTouchpad(CU_primary2DAxis, null, new Vector2(0.0085f, 0.015f)));


        /*
         * Compat
         */

        public VRCircularTouchpad Compat_circularTouchpad
        {
            get
            {
                switch (manufacturer)
                {
                    case VRManufacturer.OVR_WINDOWS_MR:
                        return WinMR_PadAxis;
                    case VRManufacturer.OVR_VALVE_INDEX:
                        return ValveIndex_PadAxis;
                    case VRManufacturer.OVR_HTCVIVE:
                    default:
                        return HTCVive_PadAxis;
                }
            }
        }






        /*
         * Speed / acceleration
         */
        private VR3DAxisInput _cachePosition;
        public VR3DAxisInput Compat_position => CU_devicePosition.Available ? CU_devicePosition :
            (_cachePosition ?? (_cachePosition = new VR3DAxisInput(inputs, "position", (out Vector3 pos) => {
                pos = GetTransform().localPosition;
                if (!Available)
                    return false;
                return true;
            })));

        private VR3DAxisInput _cachePositionBasedVelocity;
        public VR3DAxisInput Compat_velocity => CU_deviceVelocity.Available ? CU_deviceVelocity : (_cachePositionBasedVelocity ??
            (
                _cachePositionBasedVelocity = new VR3DAxisInput(inputs, "velocity", (out Vector3 velocity) => {
                    velocity = Compat_position.Speed;
                    return Compat_position.Available;
                })
            )
        );

        private VR3DAxisInput _cacheVelocityBasedAcceleration;
        public VR3DAxisInput Compat_acceleration => CU_deviceAcceleration.Available ? CU_deviceAcceleration : (_cacheVelocityBasedAcceleration ??
            (
                _cacheVelocityBasedAcceleration = new VR3DAxisInput(inputs, "acceleration", (out Vector3 acceleration) => {
                    acceleration = Compat_velocity.Speed;
                    return Compat_velocity.Available;
                })
            )
        );







        /*
         * Haptic feedback
         */
        public void TriggerHapticImpulse(float duration, float strength)
        {
            if (!Available)
                return;
            if (Device.TryGetHapticCapabilities(out HapticCapabilities hc) && hc.supportsImpulse)
                GetTrackedPoseDriver().StartCoroutine(RumbleControllerRoutine(duration, strength));
        }

        private IEnumerator RumbleControllerRoutine(float duration, float strength)
        {
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime <= duration)
            {
                if (!Available)
                    yield break;
                Device.SendHapticImpulse(0, strength);
                yield return null;
            }
        }
    }
}
