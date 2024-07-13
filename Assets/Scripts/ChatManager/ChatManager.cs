using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
// A!!!!!
public class ChatManager : MonoBehaviour
{
    public Button recordButton;
    private LLM LLM;
    private STTManager sttManager;
    private TTSManager ttsManager;
    private SwitchSceneManager SwitchSceneManager;
    private enum State { Reporting, QA, Commenting, End }
    private State currentState;

    private readonly List<TTSVoice> speakers = new List<TTSVoice> { TTSVoice.Alloy, TTSVoice.Echo, TTSVoice.Fable, TTSVoice.Onyx, TTSVoice.Nova, TTSVoice.Shimmer };
    private const string PlayerStateKey = "user-state";
    public ScrollRect chatScrollRect;
    public RectTransform chatContent;
    public RectTransform userMessageTemplate;
    public RectTransform aiMessageTemplate;

    private List<RectTransform> messageInstances = new List<RectTransform>();
    public Text countdownText; // 指向 CountdownText 的引用
    public int countdownTime = 180; // 設置倒數時間

    void Start()
    {
        sttManager = FindObjectOfType<STTManager>();
        ttsManager = FindObjectOfType<TTSManager>();
        LLM = FindObjectOfType<LLM>();

        SetPlayerState(State.Reporting);
        sttManager.SetRecordTime(countdownTime);
        sttManager.isPaused = false;
        StartRecording();
        recordButton.onClick.AddListener(OnButtonClick);

        // 启动每0.1秒调用一次的协程
        userMessageTemplate.gameObject.SetActive(false);
        aiMessageTemplate.gameObject.SetActive(false);
        StartCoroutine(StartCountdown());
        StartCoroutine(UpdateRoutine(0.1f));
    }

    private IEnumerator UpdateRoutine(float interval)
    {
        while (true)
        {
            if (!sttManager.isRecording && !sttManager.istranslating)
            {
                currentState = GetPlayerState();
                HandleState(currentState);
                StartRecording();
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
                Debug.Log("QA time");
                SetPlayerState(State.QA);
                OnCountdownEnd();
                break;
            case State.QA:
                Debug.Log("We have"+LLM.questions.Count+"questions.");
                if (LLM.questions.Count == 0)
                {
                    Debug.Log("Comment time");
                    SetPlayerState(State.Commenting);
                }
                else
                {
                    Debug.Log("QA time");
                    sttManager.SetRecordTime(180);
                    // Debug.Log(LLM.PopFirst(LLM.questions));
                    SynthesizeAndPlay(LLM.PopFirst(LLM.questions));
                }
                break;
            case State.Commenting:
                sttManager.SetRecordTime(1800);
                StartCoroutine(Comment());
                break;
            case State.End:
                sttManager.isPaused = false;
                // sttManager.isRecording = false;
                sttManager.SetRecordTime(10);
                Debug.Log("Transfer to End State");
                SceneManager.LoadScene("Result_Scene");
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
        OnCountdownEnd();
        if (sttManager.isPaused != true){
            sttManager.Transcript();
        }
    }

    private IEnumerator Comment()
    {
        for (int i = 0; i < 1; i++)
        {
            SynthesizeAndPlay(LLM.feedbacks[i]);
            Debug.Log(PlayerPrefs.GetFloat("play-time") + 2f);
            yield return new WaitForSeconds(PlayerPrefs.GetFloat("play-time") + 2f);
        }
        SetPlayerState(State.End);
        Debug.Log("Time to end!");
        sttManager.isPaused = true;
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

    private IEnumerator StartCountdown()
    {
        int currentTime = countdownTime;
        int min,sec;

        while (currentTime > 0)
        {   
            min = currentTime/60;
            sec = currentTime%60;
            if (sec < 10){
                countdownText.text = min.ToString() + " : 0" + sec.ToString();
            }else{
                countdownText.text = min.ToString() + " : " + sec.ToString();
            }
            yield return new WaitForSeconds(1f);
            currentTime--;
        }

        countdownText.text = "Time's Up!";
        OnCountdownEnd();
    }

    private void OnCountdownEnd()
    {
        countdownText.enabled = false;
    }
}
