using OpenAI;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace VoiceProcessing
{
    public class Whisper : MonoBehaviour
    {
        [SerializeField] private Text message;
        [SerializeField] private Button recordButton;        
        
        private readonly string fileName = "output.wav";
        private string microphoneName;
        private int duration = 5;
        
        private AudioClip clip;
        private bool isRecording;
        private float time;
        private OpenAIApi openai = new OpenAIApi();

        private void Start()
        {
            #if !(UNITY_WEBGL && !UNITY_EDITOR)
            recordButton.onClick.AddListener(StartRecording);
            #endif
            microphoneName = PlayerPrefs.GetString("MicrophoneName");
        }

        private void SetRecordTime(int time)
        {
            duration = time;
        }
        
        private void StartRecording()
        {
            isRecording = true;
            recordButton.enabled = false;

            var index = PlayerPrefs.GetInt("user-mic-device-index");
            
            #if !UNITY_WEBGL
            clip = Microphone.Start(microphoneName, false, duration, 44100);
            #endif
        }

        private async void EndRecording()
        {
            message.text = "Transcripting...";
            
            #if !UNITY_WEBGL
            Microphone.End(null);
            #endif
            
            byte[] data = SaveWav.Save(fileName, clip);
            
            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() {Data = data, Name = "audio.wav"},
                // File = Application.dataPath + "/" + fileName,
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);
            
            message.text = res.Text;
            recordButton.enabled = true;

            SaveTranscriptionToFile("output.txt",res.Text);
        }

        private void SaveTranscriptionToFile(string fileName,string transcription)
        {
            string path = Path.Combine(Application.dataPath+"/Files", fileName);

            try
            {
                File.WriteAllText(path, transcription);
                Debug.Log("Transcription saved to " + path);
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
                time += Time.deltaTime;
                // progressBar.fillAmount = time / duration;
                
                if (time >= duration)
                {
                    time = 0;
                    isRecording = false;
                    EndRecording();
                }
            }
        }
    }
}
