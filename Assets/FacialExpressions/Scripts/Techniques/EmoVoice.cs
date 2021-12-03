using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using utils;
using VRInputsAPI;

public class EmoVoice : MonoBehaviour
{


    private string PYTHON_PATH = Directory.GetCurrentDirectory() + "\\lib\\python39\\python.exe";

    public static float lastResponseTime = 0;
    public static bool usedHelp = false;

    private static float lastResponseStart;
    public static string fileName = "emovoice_record.wav";

    public Face mainFace;
    public Face feedbackFace;

    public GameObject microIcon, microIconMuted;

    public GameObject helpFr, helpEn;

    public TextMesh text;

    private Dictionary<string, Expression> keywordsFr = new Dictionary<string, Expression>();
    private Dictionary<string, Expression> keywordsEn = new Dictionary<string, Expression>();

    private void Start()
    {
        Dictionary<Expression, (string[], string[])> keyWordsFrEn = new Dictionary<Expression, (string[], string[])>()
        {
            [Expression.SLIGHTLY_SMILING] = (
                new string[] { "sourire", "souris", "sourit", "sourire léger", "souriant", "souriant légèrement" },
                new string[] { "slightly smiling", "slight smile", "smile", "smiling" }
            ),
            [Expression.FROWNING] = (
                new string[] { "triste", "pas content", "tristesse", "déçu", "déception" },
                new string[] { "frowning", "frown", "sad", "sadness" }
            ),
            [Expression.GRINNING] = (
                new string[] { "grand sourire", "souris fortement", "sourit fortement", "sourire fortement" },
                new string[] { "grinning", "grin", "big smile", "big smiling", "strong smile", "strongly smiling" }
            ),
            [Expression.WITH_OPEN_MOUTH] = (
                new string[] { "étonné", "étonnée", "étonnement", "bouche ouverte", "bouche bée", "béer", "surpris", "surprise" },
                new string[] { "open mouth", "surprised", "surprise", "surprising" }
            ),
            [Expression.SMILING_WITH_HEART_EYES] = (
                new string[] { "amoureux", "amoureuse", "aimant", "aimer", "cœurs", "coeurs", "cœur", "coeur", "admiration", "en admiration", "admiratif", "admirative" },
                new string[] { "in love", "loving", "love", "heart", "hearts", "heart eyes" }
            ),
            [Expression.ANGRY] = (
                new string[] { "colère", "coléreux", "colérique", "en colère", "énervé", "énervée" },
                new string[] { "angry", "angriness" }
            ),
            [Expression.THINKING] = (
                new string[] { "pensif", "pensive", "penser", "réfléchi", "réfléchir", "perplexe" },
                new string[] { "thinking", "think" }
            ),
            [Expression.SMILING_WITH_SMILING_EYES] = (
                new string[] { "yeux souriants", "yeux souriant", "yeux qui sourient", "apaisé", "apaisée" },
                new string[] { "smiling eyes", "smiling eye", "smile eye", "smile eyes" }
            ),
        };

        void addToMap(Dictionary<string, Expression> d, string w, Expression e)
        {
            w = w.ToLower();
            if (d.ContainsKey(w))
            {
                Debug.LogWarning("Cannot add (" + w + "," + e.ToString() + ") because the word " + w + " is already mapped to " + d[w].ToString());
                return;
            }
            d[w] = e;
        }

        foreach (var exPair in keyWordsFrEn)
        {
            Expression e = exPair.Key;
            foreach (string kFr in exPair.Value.Item1)
                addToMap(keywordsFr, kFr, e);
            foreach (string kEn in exPair.Value.Item2)
                addToMap(keywordsEn, kEn, e);
        }
    }

    void OnEnable()
    {
        Debug.Log("EmoVoice.OnEnable()");
        StartRecord();
        helpWanted = false;
    }

    void OnDisable()
    {
        Debug.Log("EmoVoice.OnDisable()");
        mainFace.SetExpression(Expression.NEUTRAL, 0);
        feedbackFace.SetExpression(Expression.NEUTRAL, 0);

    }

    // if english (true) or french (false) voice recognition and interface
    public bool english = false;


    private float startRecordTime = 0;

    private AudioClip record = null;
    private bool recording = false;

    private readonly int maxRecordDuration = 5; // 5 sec

    private bool helpWanted = false;

    // Update is called once per frame
    void Update()
    {
        feedbackFace.ReplicateGenderFrom(mainFace);

        microIcon.SetActive(recording);
        microIconMuted.SetActive(!recording);

        VRController ctrl = VRInputsManager.GetMainHandController();
        VRBooleanInput padPress = ctrl.HTCVive_PadPress;

        if (recording && (padPress.Up || Time.time >= startRecordTime + maxRecordDuration))
        {
            StopRecord();
        }
        else if (!recording && record == null && padPress)
        {
            StartRecord();
        }

        VRBooleanInput grip = ctrl.HTCVive_Grip;

        if (grip.Down)
        {
            helpWanted = !helpWanted;
            if (helpWanted)
                usedHelp = true;
        }

        if (helpWanted)
        {
            if (english)
                helpEn.SetActive(true);
            else
                helpFr.SetActive(true);
        }
        else
        {
            helpEn.SetActive(false);
            helpFr.SetActive(false);
        }

    }


    private void StartRecord()
    {
        startRecordTime = Time.time;
        record = Microphone.Start(null, false, maxRecordDuration, 44100);
        recording = true;
        text.text = english ? "Microphone activated" : "Microphone activé";
    }



    private void StopRecord()
    {
        Microphone.End(null);
        recording = false;
        text.text = english ? "Loading..." : "Chargement...";

        Scheduler.RunSyncLater(0.1f, recordSaveAndSend);
    }



    private void recordSaveAndSend()
    {
        AudioClip sentAudioClip = record;
        try
        {
            if (sentAudioClip.samples == 0)
            {
                Debug.LogError("EmoVoice: too short to recognize");
                computeRecognition("error: record too short (no sample)");
            }
            sentAudioClip = SavWav.TrimSilence(sentAudioClip, 0.00001f);
            if (sentAudioClip.samples == 0)
            {
                Debug.LogError("EmoVoice: too short to recognize");
                computeRecognition("error: record too short (no sample)");
            }
            sentAudioClip = SavWav.Normalize(sentAudioClip, 10);
        }
        catch (Exception e)
        {
            Debug.LogError("EmoVoice: error whiler prepare audio");
            Debug.LogError(e);
            computeRecognition("error: invalid audio data. See log file.");
        }
        

        SavWav.Save(fileName, sentAudioClip);

        record = null;
        lastResponseStart = Time.realtimeSinceStartup;
        if (sentAudioClip.length >= 0.3)
        {
            Scheduler.RunAsync(sendRecord);
        }
        else
        {
            Debug.LogError("EmoVoice: too short to recognize");
            computeRecognition("error: record too short (< 0.3s)");
        }
    }





    private void sendRecord()
    {
        try
        {
            using (System.Diagnostics.Process pyProcess = new System.Diagnostics.Process())
            {
                pyProcess.StartInfo.UseShellExecute = false;
                
                pyProcess.StartInfo.FileName = PYTHON_PATH;

                string langArg = english ? "en_US" : "fr_FR";
                pyProcess.StartInfo.Arguments = "voice_recognition.py " + langArg + " \"" + fileName + "\"";
                pyProcess.StartInfo.WorkingDirectory = ".";
                pyProcess.StartInfo.CreateNoWindow = true;
                pyProcess.Start();
                pyProcess.WaitForExit();

                string output = File.ReadAllText(fileName + ".out");

                Scheduler.RunSync(() => computeRecognition(output));

            }
        } catch (Exception e)
        {
            Debug.LogError(e);
            Scheduler.RunSync(() => computeRecognition("error: " + e.Message));
        }
        
    }




    private void computeRecognition(string recognitionOutput)
    {
        lastResponseTime = Time.realtimeSinceStartup - lastResponseStart;
        if (recognitionOutput == "dont_understand")
        {
            text.text = english ? "Not recognized. Please try again." : "Non reconnu. Veuillez réessayer.";
        }
        else if (recognitionOutput.StartsWith("error: "))
        {
            text.text = (english ? "Error. Please try again." : "Erreur. Veuillez réessayer.")
                + "\n" + recognitionOutput.Substring(7);
        }
        else
        {
            /*
             *  {
             *      "alternative": [
             *          {"transcript": "Salut", "confidence": 0.83471632},
             *          {"transcript": "salut"},
             *          {"transcript": "SALUT"},
             *          {"transcript": "salue"},
             *          {"transcript": "\u00e7a lui"}
             *      ],
             *      "final": true
             *  }
             */
            JObject JSONObj = JsonConvert.DeserializeObject<JObject>(recognitionOutput);
            JArray alternative = (JArray) JSONObj["alternative"];
            List<string> transcripts = new List<string>();
            foreach (JObject trObj in alternative)
            {
                transcripts.Add(((string)trObj["transcript"]).ToLower());
            }

            // text.text = "\"" + string.Join(",", transcripts) + "\"";

            var keywords = english ? keywordsEn : keywordsFr;
            string recognized = null;
            Expression? expr = null;
            foreach (string transcript in transcripts)
            {
                if (keywords.ContainsKey(transcript))
                {
                    recognized = transcript;
                    expr = keywords[transcript];
                    break;
                }
            }


            if (expr.HasValue)
            {
                mainFace.SetExpression(expr.Value, 1);
                feedbackFace.SetExpression(expr.Value, 1);
                text.text = " \"" + recognized + "\"";
            }
            else
            {
                text.text = (english ? "Unrecognized: " : "Non reconnu : ") + " \"" + transcripts[0] + "\"";
            }
        }
    }

}
public enum Language
{
    fr_FR, en_US, en_GB
}

public static class LanguageMothods
{
    public static string getAsString(this Language lang)
    {
        return lang.ToString().Replace('_', '-');
    }
}