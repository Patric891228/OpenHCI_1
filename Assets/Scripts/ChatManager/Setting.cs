using OpenAI;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    [SerializeField] private Dropdown dropdown;
    [SerializeField] private InputField numberInputField;
    [SerializeField] private Text displayText;

    private void Start()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        dropdown.options.Add(new Dropdown.OptionData("Microphone not supported on WebGL"));
        #else
        foreach (var device in Microphone.devices)
        {
            dropdown.options.Add(new Dropdown.OptionData(device));
        }
        dropdown.onValueChanged.AddListener(ChangeMicrophone);

        var index = PlayerPrefs.GetInt("user-mic-device-index");
        dropdown.SetValueWithoutNotify(index);
        #endif

        numberInputField.contentType = InputField.ContentType.IntegerNumber; // 設置 InputField 只接受數字
        numberInputField.onEndEdit.AddListener(OnEndEdit); // 設置輸入完成的監聽器
    }

    private void ChangeMicrophone(int index)
    {
        PlayerPrefs.SetInt("user-mic-device-index", index);
        PlayerPrefs.SetString("user-mic-name", dropdown.options[index].text);
    }

    private void SetRecordTime(int time)
    {
        PlayerPrefs.SetInt("user-mic-duration", time);
    }

    private void OnEndEdit(string input)
    {
        if (int.TryParse(input, out int number))
        {
            displayText.text = "您輸入的數字是: " + number;
            SetRecordTime(number);
        }
        else
        {
            displayText.text = "請輸入一個有效的數字。";
        }
    }

}
