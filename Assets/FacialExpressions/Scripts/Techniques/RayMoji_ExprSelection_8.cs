
using System.Collections.Generic;
using UnityEngine;
using VRInputsAPI;

public class RayMoji_ExprSelection_8 : MonoBehaviour
{
    public static float ITEM_SCALE = 0.1f;
    public static float ITEM_SHIFT = 0.12f;
    public static float MENU_DISTANCE = 1f;

    public int nbPerLine;

    public GameObject baseElement;

    public GameObject globalMenuElement;

    public RayCursor.RayCursor rayCursor;

    public Face mainFace;
    public Face feedbackFace;

    private void Start()
    {
        feedbackFace.transform.parent = VRInputsManager.GetMainHandController().ChildContainer.transform;
        feedbackFace.transform.localPosition = new Vector3(0, 0, 0.05f);
        feedbackFace.transform.localRotation = Quaternion.Euler(-60, 180, 0);
    }


    private static List<Expression> listedExpressions = new List<Expression>()
    {
        Expression.GRINNING,
        Expression.SMILING_WITH_SMILING_EYES,
        Expression.SLIGHTLY_SMILING,
        Expression.SMILING_WITH_HEART_EYES,
        Expression.THINKING,
        Expression.FROWNING,
        Expression.WITH_OPEN_MOUTH,
        Expression.ANGRY
    };

    void OnEnable()
    {
        if (baseElement != null) // should be null at the end of initializing
        {
            float xStart = -(nbPerLine - 1) / 2f * ITEM_SHIFT;

            int r = 0, c = 0;
            foreach (Expression e in listedExpressions)
            {
                if (e == Expression.NEUTRAL)
                    continue;
                GameObject go = Instantiate(baseElement);
                go.name = "Menu Item " + e.ToString();
                go.transform.parent = globalMenuElement.transform;
                go.transform.localPosition = new Vector3(xStart + c * ITEM_SHIFT, r * -ITEM_SHIFT, 0);
                go.transform.localScale = new Vector3(ITEM_SCALE, ITEM_SCALE, ITEM_SCALE);
                go.transform.localRotation = Quaternion.identity;

                go.GetComponent<MeshRenderer>().material.mainTexture = e.LoadTexture();
                go.GetComponent<RayCursor.Selectable>().OnSelect += () =>
                {
                    selected = e;
                };
                go.GetComponent<RayCursor.Selectable>().OnHighlightIn += () =>
                {
                    feedbackFace.SetExpression(e, 1);
                };

                if (c == nbPerLine - 1)
                {
                    r++;
                    c = 0;
                }
                else
                    c++;
            }

            rayCursor.ray.Parent = VRInputsManager.GetMainHandController().GetGameObject();

            Destroy(baseElement);
        }

        feedbackFace.gameObject.SetActive(true);
        globalMenuElement.SetActive(true);



        Transform headPos = VRInputsManager.GetDevice(VRDeviceEnum.HMD).GetTransform();

        Vector3 fw = headPos.forward;
        fw.y = 0;
        fw.Normalize();

        transform.position = headPos.position + fw * MENU_DISTANCE;

        transform.forward = fw;

        rayCursor.gameObject.SetActive(true);
        rayCursor.ray.gameObject.SetActive(true);

    }


    private void OnDisable()
    {
        rayCursor.gameObject.SetActive(false);
        rayCursor.ray.gameObject.SetActive(false);
        feedbackFace.gameObject.SetActive(false);
        menuHidden = false;
        selected = Expression.NEUTRAL;
        mainFace.SetExpression(Expression.NEUTRAL, 0);
        feedbackFace.SetExpression(Expression.NEUTRAL, 0);
    }



    Expression selected = Expression.NEUTRAL;

    bool menuHidden = false;

    // Update is called once per frame
    void Update()
    {
        feedbackFace.ReplicateGenderFrom(mainFace);

        if (selected == Expression.NEUTRAL)
            return;
        // the user have selected something

        // hide the menu and raycursor
        if (!menuHidden)
        {

            rayCursor.gameObject.SetActive(false);
            rayCursor.ray.gameObject.SetActive(false);
            globalMenuElement.SetActive(false);

            mainFace.SetExpression(selected, 1);
            feedbackFace.SetExpression(selected, 1);

            menuHidden = true;

        }
        


        VRThresholdedInput triggerPressed = VRInputsManager.GetMainHandController().HTCVive_TriggerAxis.GetThresholdedInput(0.1f, 0.13f);

        if (triggerPressed.Up)
        {
            gameObject.SetActive(false);
        }

    }




    
}
