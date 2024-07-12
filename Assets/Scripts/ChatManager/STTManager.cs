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
    private ChatManager chatManager;
    
    private AudioClip clip;
    public bool isRecording;
    public bool isPaused;
    public bool istranslating;
    private float time;
    private OpenAIApi openai = new OpenAIApi();
    private string folderPath = Application.dataPath + "/Files";
    private string filePath;

    private void Start()
    {
        chatManager = FindObjectOfType<ChatManager>();
        microphoneName = PlayerPrefs.GetString("user-mic-duration");
        filePath = Path.Combine(folderPath, "Transcript.txt");
        // DeleteFileAtStart();
    }

    public void SetRecordTime(int time)
    {
        duration = time;
        // Debug.Log("set record time:" + duration);
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
        istranslating = true;
        SetRecordTime(180);
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
        chatManager.AppendMessage("system", "你: "+res.Text);
        Debug.Log("You said: " + res.Text);
        WriteTextToFile(res.Text);
        SaveTranscriptionToFile("output.txt",res.Text);
        istranslating = false;
    }

    private void SaveTranscriptionToFile(string fileName,string transcription)
    {
        string path = Path.Combine(Application.dataPath+"/Files", fileName);

        try
        {
            File.WriteAllText(path, transcription);
            // Debug.Log("使用者：" + transcription);
            // Debug.Log("Transcription saved to " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save transcription: " + e.Message);
        }
    }


    private void Update()
    {
        if (isRecording && !isPaused)
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

    public void WriteTextToFile(string text)
    {
        // 確保目錄存在，若不存在則創建
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 使用 StreamWriter 的 Append 模式來寫入文字
        using (StreamWriter sw = new StreamWriter(filePath, true))
        {
            sw.WriteLine(text);
        }

        // Debug.Log("Text written to file: " + text);
    }

    void DeleteFileAtStart()
    {
        // 確保目錄存在
        if (Directory.Exists(folderPath))
        {
            // 檢查檔案是否存在
            if (File.Exists(filePath))
            {
                // 刪除檔案
                File.Delete(filePath);
                Debug.Log("File deleted at start: " + filePath);
            }
            else
            {
                Debug.Log("File not found: " + filePath);
            }
        }
        else
        {
            Debug.Log("Directory not found: " + folderPath);
        }
    }


}
