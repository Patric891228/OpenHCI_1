using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class ChatManager : MonoBehaviour
{
    public Button recordButton;
    private LLM LLM;
    private STTManager sttManager;
    private TTSManager ttsManager;
    private enum State { Reporting, QA, Commenting, End }
    private State currentState;

    private readonly List<TTSVoice> speakers = new List<TTSVoice> { TTSVoice.Alloy, TTSVoice.Echo, TTSVoice.Fable, TTSVoice.Onyx, TTSVoice.Nova, TTSVoice.Shimmer };
    private const string PlayerStateKey = "user-state";
    public ScrollRect chatScrollRect;
    public RectTransform chatContent;
    public RectTransform userMessageTemplate;
    public RectTransform aiMessageTemplate;

    private List<RectTransform> messageInstances = new List<RectTransform>();

    void Start()
    {
        sttManager = FindObjectOfType<STTManager>();
        ttsManager = FindObjectOfType<TTSManager>();
        LLM = FindObjectOfType<LLM>();

        SetPlayerState(State.Reporting);
        sttManager.SetRecordTime(10);
        StartRecording();
        recordButton.onClick.AddListener(OnButtonClick);

        // 启动每0.1秒调用一次的协程
        userMessageTemplate.gameObject.SetActive(false);
        aiMessageTemplate.gameObject.SetActive(false);

        StartCoroutine(UpdateRoutine(0.1f));
    }

    private IEnumerator UpdateRoutine(float interval)
    {
        while (true)
        {
            if (!sttManager.isRecording)
            {
                currentState = GetPlayerState();
                if (currentState != State.End)
                {
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

    void OnButtonClick()
    {
        Debug.Log("Click Button!");
        sttManager.isRecording = false;
        sttManager.EndRecording();
        sttManager.Transcript();
        Debug.Log(GetPlayerState());
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

    public void AppendMessage(string role, string message)
    {
        RectTransform messageTemplate = role == "user" ? userMessageTemplate : aiMessageTemplate;

        // 实例化消息模板
        RectTransform messageInstance = Instantiate(messageTemplate, chatContent);
        messageInstance.gameObject.SetActive(true);

        // 设置消息内容
        Text messageText = messageInstance.GetComponentInChildren<Text>();
        messageText.text = message;

        // 添加到消息实例列表
        messageInstances.Add(messageInstance);

        // 如果消息数量超过最大值，则删除旧消息
        if (messageInstances.Count > 10)
        {
            RemoveOldMessages();
        }

        // 强制刷新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatContent);
        chatScrollRect.verticalNormalizedPosition = 0; // 滚动到底部
    }

    void RemoveOldMessages()
    {
        // 删除最早添加的消息实例
        RectTransform oldMessage = messageInstances[0];
        messageInstances.RemoveAt(0);
        Destroy(oldMessage.gameObject);
    }

    private void OnDisable()
    {
        if (chatContent != null)
        {
            foreach (Transform child in chatContent.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

}
