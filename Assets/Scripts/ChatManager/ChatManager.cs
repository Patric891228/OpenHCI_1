using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class ChatManager : MonoBehaviour
{
    public Button recordButton;
    public Text stateText; 
    private LLM LLM;
    private STTManager sttManager;
    private TTSManager ttsManager;

    private enum State { Reporting, QA, Commenting, End }
    private State currentState;

    private readonly List<TTSVoice> speakers = new List<TTSVoice> { TTSVoice.Alloy, TTSVoice.Echo, TTSVoice.Fable, TTSVoice.Onyx, TTSVoice.Nova, TTSVoice.Shimmer };
    private const string PlayerStateKey = "user-state";

    void Start()
    {   
        sttManager = FindObjectOfType<STTManager>();
        ttsManager = FindObjectOfType<TTSManager>();
        LLM = FindObjectOfType<LLM>(); // 确保LLM在场景中存在

        if (sttManager == null)
        {
            Debug.LogError("STTManager not found in the scene.");
            return;
        }
        if (ttsManager == null)
        {
            Debug.LogError("TTSManager not found in the scene.");
            return;
        }
        if (LLM == null)
        {
            Debug.LogError("LLM not found in the scene.");
            return;
        }
        if (recordButton == null)
        {
            Debug.LogError("RecordButton is not assigned in the Inspector.");
            return;
        }
        if (stateText == null)
        {
            Debug.LogError("StateText is not assigned in the Inspector.");
            return;
        }

        SetPlayerState(State.Reporting);
        sttManager.SetRecordTime(10);
        UpdateStateText();
        StartRecording();
        recordButton.onClick.AddListener(OnButtonClick);

        // 启动每0.1秒调用一次的协程
        StartCoroutine(UpdateRoutine(0.1f));
    }

    private IEnumerator UpdateRoutine(float interval)
    {
        while (true)
        {
            if (!sttManager.isRecording)
            {
                currentState = GetPlayerState();
                if (currentState != State.End){
                    HandleState(currentState);
                    StartRecording();
                }
            }
            yield return new WaitForSeconds(interval); // 每0.1秒调用一次
        }
    }

    private void StartRecording()
    {
        sttManager.StartRecording();
    }

    private void HandleState(State state)
    {
        switch (state)
        {
            case State.Reporting:
                SetPlayerState(State.QA);
                sttManager.SetRecordTime(6);
                break;
            case State.QA:
                if (LLM.questions.Count == 0)
                {
                    SetPlayerState(State.Commenting);
                    sttManager.SetRecordTime(6);
                }
                else
                {
                    sttManager.SetRecordTime(180);
                    SynthesizeAndPlay(LLM.PopFirst(LLM.questions));
                }
                break;
            case State.Commenting:
                sttManager.SetRecordTime(180);
                StartCoroutine(Comment());
                break;
            case State.End:
                sttManager.isRecording = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        UpdateStateText(); // 更新状态文本
    }

    private void SynthesizeAndPlay(string text)
    {
        TTSVoice randomSpeaker = speakers[UnityEngine.Random.Range(0, speakers.Count)];
        ttsManager.SynthesizeAndPlay(text, TTSModel.TTS_1, randomSpeaker, 1);
    }

    private void SetPlayerState(State state)
    {
        currentState = state;
        PlayerPrefs.SetString(PlayerStateKey, state.ToString());
    }

    private State GetPlayerState()
    {
        string stateString = PlayerPrefs.GetString(PlayerStateKey, State.Reporting.ToString());
        return (State)System.Enum.Parse(typeof(State), stateString); // 使用 System.Enum.Parse
    }

    private void UpdateStateText()
    {
        stateText.text = "Current State: " + currentState.ToString() + sttManager.duration;
    }

    void OnButtonClick()
    {
        Debug.Log("Click Button!");
        sttManager.isRecording = false;
        sttManager.EndRecording();
        sttManager.Transcript();
        Debug.Log(GetPlayerState());
        UpdateStateText();
    }

    private IEnumerator Comment()
    {
        // 假设循环10次，每次暂停1秒
        for (int i = 0; i < LLM.feedbacks.Count; i++)
        {
            SynthesizeAndPlay(LLM.feedbacks[i]);
            Debug.Log(PlayerPrefs.GetFloat("play-time") + 2f);
            yield return new WaitForSeconds(PlayerPrefs.GetFloat("play-time") + 2f);
        }
        SetPlayerState(State.End);
        Debug.Log("Time to end!");
        sttManager.SetRecordTime(6);
    }

}
