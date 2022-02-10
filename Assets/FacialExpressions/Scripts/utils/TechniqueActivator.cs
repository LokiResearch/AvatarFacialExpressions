using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechniqueActivator : MonoBehaviour
{
    public GameObject techniqueToActivate;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (VRInputsAPI.VRInputsManager.GetMainHandController().HTCVive_PadPress.Down)
        {
            if (techniqueToActivate == null)
            {
                Debug.LogError("Technique not found. Please set the component attribute \"Technique To Activate\" in the Unity interface.");
            }
            else if (!techniqueToActivate.activeSelf)
            {
                techniqueToActivate.SetActive(true);
            }
        }
    }
}
