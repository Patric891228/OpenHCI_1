using OpenAI;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class STTManager : MonoBehaviour
{
    private readonly string fileName = "output.wav";
    private string microphoneName;
    public float duration = 180;
    
    private AudioClip clip;
    public bool isRecording;
    private float time;
    private OpenAIApi openai = new OpenAIApi();

    private void Start()
    {
        microphoneName = PlayerPrefs.GetString("user-mic-duration");
    }

    public void SetRecordTime(int time)
    {
        duration = time;
        Debug.Log("set record time:" + duration);
    }
    
    public void StartRecording()
    {
        isRecording = true;
        var index = PlayerPrefs.GetInt("user-mic-device-index");
        
        #if !UNITY_WEBGL
        clip = Microphone.Start(microphoneName, false, (int)Math.Round(duration), 44100);
        #endif
    }

    public void EndRecording(){
        isRecording = false;
    }

    public async void Transcript()
    {
        // message.text = "Transcripting...";
        
        #if !UNITY_WEBGL
        Microphone.End(null);
        #endif
        
        byte[] data = SaveWav.Save(fileName, clip);
        
        var req = new CreateAudioTranscriptionsRequest
        {
            FileData = new FileData() {Data = data, Name = "audio.wav"},
            // File = Application.dataPath + "/" + fileName,
            Model = "whisper-1",
            Language = "zh"
        };
        var res = await openai.CreateAudioTranscription(req);
        
        SaveTranscriptionToFile("output.txt",res.Text);
    }

    private void SaveTranscriptionToFile(string fileName,string transcription)
    {
        string path = Path.Combine(Application.dataPath+"/Files", fileName);

        try
        {
            File.WriteAllText(path, transcription);
            Debug.Log("使用者：" + transcription);
            // Debug.Log("Transcription saved to " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save transcription: " + e.Message);
        }
    }


    private void Update()
    {
        if (isRecording)
        {
            duration -= Time.deltaTime;                
            if (duration <= 0)
            {
                duration = 180;
                isRecording = false;
                Transcript();
            }
        }
    }
}
