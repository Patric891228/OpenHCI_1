using System;
using UnityEngine;

public class TTSManager : MonoBehaviour
{
    private OpenAIWrapper openAIWrapper;
    [SerializeField] private AudioPlayer audioPlayer;
    [SerializeField] private TTSModel model = TTSModel.TTS_1;
    [SerializeField] private TTSVoice voice = TTSVoice.Alloy;
    [SerializeField, Range(0.25f, 4.0f)] private float speed = 1f;
    private ChatManager chatManager;
    
    private void Start(){
        chatManager = FindObjectOfType<ChatManager>();
    }

    private void OnEnable()
    {
        if (!openAIWrapper) this.openAIWrapper = FindObjectOfType<OpenAIWrapper>();
        if (!audioPlayer) this.audioPlayer = GetComponentInChildren<AudioPlayer>();
    }

    private void OnValidate() => OnEnable();

    public async void SynthesizeAndPlay(string text)
    {
        Debug.Log("觀眾： " + text);
        byte[] audioData = await openAIWrapper.RequestTextToSpeech(text, model, voice, speed);
        if (audioData != null)
        {
            chatManager.AppendMessage("system", "觀眾： "+text);
            audioPlayer.ProcessAudioBytes(audioData);
            PlayerPrefs.SetFloat("play-time", audioData.Length/16000f);
        }
        else Debug.LogError("Failed to get audio data from OpenAI.");
    }

    public void SynthesizeAndPlay(string text, TTSModel model, TTSVoice voice, float speed)
    {
        this.model = model;
        this.voice = voice;
        this.speed = speed;
        SynthesizeAndPlay(text);
    }

    
}