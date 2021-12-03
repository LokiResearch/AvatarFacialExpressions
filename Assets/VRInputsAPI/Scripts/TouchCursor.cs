using UnityEngine;
using VRInputsAPI;

public class TouchCursor : MonoBehaviour
{
    public VRDeviceEnum device;

    private bool availableBefore = false;

    void Update()
    {
        VRController controller = VRInputsManager.GetController(device);

        if (controller.Available && !availableBefore)
        {
            if (controller.manufacturer == VRManufacturer.OVR_HTCVIVE)
            {
                transform.parent.localPosition = new Vector3(0, .0062f, -.0493f);
                transform.parent.localRotation = Quaternion.Euler(84, 0, 0);
            }
            else if (controller.manufacturer == VRManufacturer.OVR_WINDOWS_MR)
            {
                transform.parent.localPosition = new Vector3(0, .0015f, -.0547f);
                transform.parent.localRotation = Quaternion.Euler(125, 0, 0);
            }
            else if (controller.manufacturer == VRManufacturer.OVR_VALVE_INDEX)
            {
                if (controller.xRNode == UnityEngine.XR.XRNode.LeftHand)
                {
                    transform.parent.localPosition = new Vector3(.0039f, .0013f, -.0527f);
                    transform.parent.localRotation = Quaternion.Euler(112.7f, -7.5f, -3.1f);
                }
                else // RightHand
                {
                    transform.parent.localPosition = new Vector3(-.0039f, .0013f, -.0527f);
                    transform.parent.localRotation = Quaternion.Euler(112.7f, 7.5f, 3.1f);
                }
            }
        }


        if (controller.Available && controller.Compat_circularTouchpad.Available)
        {
            GetComponent<MeshRenderer>().enabled = true;
            transform.localPosition = controller.Compat_circularTouchpad.ValueInMeter;
        }
        else
        {
            GetComponent<MeshRenderer>().enabled = false;
        }

        availableBefore = controller.Available;
    }
}
