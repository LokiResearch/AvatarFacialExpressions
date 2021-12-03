using OneDollarRecognizer;
using System;
using UnityEngine;
using VRInputsAPI;

public class EmoGest : MonoBehaviour
{
    public static float MIN_GESTURE_LENGTH = 0.2f; // unit is based on touchpad coordinate system, not real life measurement




    public static bool usedHelp = false;

    /// <summary>
    /// If null, the user gesture is not saved in the file
    /// </summary>
    public static string xpGestureFileSave = null;
    public static string xpGestureTrial = null;

    private static string currentGestureLogFile = null;
    private static string currentGestureTrial = null;
    private static int currentGestureTrialCount = 0;
    private static OneDollar<string> currentGestureLog = null;


    public Face mainFace;
    public Face feedbackFace;

    public GameObject help;

    public bool reloadFile;
    public bool registerGesture;
    public string registerName;
    public Expression registeredExpression;



    private OneDollar<Expression> recognizer = new OneDollar<Expression>(
        rotateToZero: false,
        scaleMethod: ScaleMethod.FIT_IN_SQUARE,
        reversible: true
        );

    private bool helpWanted = false;
    private Unistroke currentStroke = null;

    private void Start()
    {
        recognizer.Load("expression_gestures.1d", s => (Expression)Enum.Parse(typeof(Expression), s));
        reloadFile = false;
    }

    void OnEnable()
    {
        Debug.Log("EmoGest.OnEnable()");
        helpWanted = false;
    }

    void OnDisable()
    {
        Debug.Log("EmoGest.OnDisable()");
        mainFace.SetExpression(Expression.NEUTRAL, 0);
        feedbackFace.SetExpression(Expression.NEUTRAL, 0);
    }

    // Update is called once per frame
    void Update()
    {
        feedbackFace.ReplicateGenderFrom(mainFace);

        if (reloadFile)
        {
            recognizer.Load("expression_gestures.1d", s => (Expression)Enum.Parse(typeof(Expression), s));
            reloadFile = false;
        }
        VRController ctrl = VRInputsManager.GetMainHandController();
        if (ctrl.HTCVive_PadTouch.Available && ctrl.HTCVive_PadTouch.Down)
        {
            currentStroke = new Unistroke(registerName);
        }

        if (currentStroke != null && ctrl.HTCVive_PadTouch.Available && ctrl.HTCVive_PadTouch && ctrl.Compat_circularTouchpad.Available)
        {
            currentStroke.Add(ctrl.Compat_circularTouchpad.Value);
        }

        if (currentStroke != null && ctrl.HTCVive_PadTouch.Available && ctrl.HTCVive_PadTouch.Up)
        {
            if (currentStroke.Count > 1 && currentStroke.Length >= MIN_GESTURE_LENGTH)
            {
                if (registerGesture)
                {
                    recognizer.Add(registeredExpression, currentStroke);
                    recognizer.ShowInFile(registeredExpression);
                    recognizer.Save("expression_gestures.1d", e => e.ToString());
                    Debug.Log("Gesture added to recognizer as " + registeredExpression);
                }
                else
                {
                    Tuple<Expression, float> recognized = recognizer.Recognize(currentStroke);
                    if (recognized != null)
                    {
                        Debug.Log("Gesture recognized as " + recognized.Item1);
                        mainFace.SetExpression(recognized.Item1, 1);
                        feedbackFace.SetExpression(recognized.Item1, 1);

                        /*
                         * Gesture log here
                         */
                        if (xpGestureFileSave != currentGestureLogFile)
                        {
                            if (currentGestureLogFile != null)
                            {
                                currentGestureLog = null;
                            }
                            currentGestureLogFile = xpGestureFileSave;
                            if (currentGestureLogFile != null)
                            {
                                currentGestureLog = new OneDollar<string>(
                                    rotateToZero: false,
                                    scaleMethod: ScaleMethod.NONE,
                                    adjustRotation: false
                                );
                            }
                        }

                        if (xpGestureTrial != currentGestureTrial)
                        {
                            currentGestureTrial = xpGestureTrial;
                            currentGestureTrialCount = 0;
                        }

                        if (currentGestureLog != null)
                        {
                            Unistroke strokeCopy = new Unistroke(currentStroke);
                            strokeCopy.Name = "g" + (++currentGestureTrialCount) + "_rec" + recognized.Item1;
                            currentGestureLog.Add(xpGestureTrial, strokeCopy);
                            currentGestureLog.Save(currentGestureLogFile, s => s); // save afer every gesture to be sure (crash, ...)
                        }

                        /*
                         * Propagate event to XP manager
                         */
                        //if (ModesManager.instance != null)
                        //    ModesManager.instance.OnExpressionSet((recognized.Item1, recognized.Item2));
                        //TechniquesManager.EnableTechnique(null);
                    }
                }
            }
            currentStroke = null;
        }



        VRBooleanInput grip = ctrl.HTCVive_Grip;

        if (grip.Down)
        {
            helpWanted = !helpWanted;
            if (helpWanted)
                usedHelp = true;
        }

        help.SetActive(helpWanted);
    }
    
}
