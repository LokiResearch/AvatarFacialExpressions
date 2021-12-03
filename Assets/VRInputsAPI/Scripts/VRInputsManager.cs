using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace VRInputsAPI
{


    public enum VRDeviceEnum
    {
        HMD, LeftCtrl, RightCtrl
    }




    public class VRInputsManager : MonoBehaviour
    {
        private static VRInputsManager instance;

        public GameObject hmd;
        public GameObject leftController;
        public GameObject rightController;
        
        /// <summary>
        /// Le gameobject associé à l'espace dans lequel le joueur peut se ballader avec le HMD et les controleurs
        /// </summary>
        public GameObject VRRig;

        public GameObject mainOrigin, offOrigin;

        public bool hideControllers = false;


        public static Vector2 NaNVector2 = new Vector2(float.NaN, float.NaN);

        private static VRDeviceEnum mainHand = VRDeviceEnum.LeftCtrl, offHand = VRDeviceEnum.RightCtrl;

        public static void SetMainHand(VRDeviceEnum mainHandDevice, VRDeviceEnum offHandDevice)
        {
            mainHand = mainHandDevice;
            offHand = offHandDevice;
            GetMainHandController().ChildContainer = instance.mainOrigin;
            GetOffHandController().ChildContainer = instance.offOrigin;
        }


        private static List<VRDevice> devices;
        

        private void OnEnable()
        {
            instance = this;
            devices = new List<VRDevice>
            {
                new VRDevice(this, hmd, XRNode.Head),
                new VRController(this, leftController, XRNode.LeftHand, mainOrigin),
                new VRController(this, rightController, XRNode.RightHand, offOrigin)
            };

            SetMainHand(VRDeviceEnum.RightCtrl, VRDeviceEnum.LeftCtrl);
        }


        void Update()
        {
            UpdateDevices();
        }


        private void UpdateDevices()
        {
            foreach(VRDevice d in devices)
                d.UpdateInputs();
        }






        public static VRDevice GetDevice(VRDeviceEnum device)
        {
            return devices == null ? null : devices[(int)device];
        }

        public static VRController GetController(VRDeviceEnum deviceC)
        {
            VRDevice device = GetDevice(deviceC);
            if (device == null)
                return null;
            device.CheckIsController();
            return (VRController)device;
        }

        public static VRDeviceEnum GetMainHand() => mainHand;
        public static VRController GetMainHandController() => GetController(mainHand);

        public static VRDeviceEnum GetOffHand() => offHand;
        public static VRController GetOffHandController() => GetController(offHand);




        public static bool Available => devices != null && GetDevice(VRDeviceEnum.HMD).Available;


    }
    
}