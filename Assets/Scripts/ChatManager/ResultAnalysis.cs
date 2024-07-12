using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using OpenAI;
public class ResultAnalysis : MonoBehaviour
{
    private OpenAIApi openai = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();

    // private string theme = "香港旅遊介紹";
    private string prompt = "以下是報告主題為" + "香港旅遊介紹" + "的逐字稿：\n";
    private string question = "你是一個演講的專業評審，請針對以上演講者的逐字稿，以結構性（報告的結構是否合理，包括開頭、主體和結尾部分的連貫性和邏輯性）、清晰性（分析語言表達是否清晰易懂，有無模糊或含糊不清的部分）、完整性（檢查報告是否涵蓋了所有必要的內容，是否有關鍵點遺漏）、正確性（報告內容是否符合事實）分別做評判並嚴格評分（每個項目最高分為25分），並具體說明評分原因和可改進的地方，依照以下格式輸出：\n結構性：<response>分數：<score>\n清晰性：<response>分數：<score>\n完整性：<response>分數：<score>\n正確性：<response>分數：<score>\n將建議取代於<response>、分數取代於<score>，整理一份報告";
    public Text structureAnalysis;
    public Text clarityAnalysis;
    public Text completenessAnalysis;
    public Text accuracyAnalysis;
    public Text structureAnalysisGrade;
    public Text clarityAnalysisGrade;
    public Text completenessAnalysisGrade;
    public Text accuracyAnalysisGrade;
    private string response = "";
    void Start()
    {
        string transcriptPath  = Application.dataPath + "/Files/transcripts.txt";
        string text = ReadContent(transcriptPath);
        Debug.Log("transcript: " + text);
        SendReply(prompt, text);
    }


    private string ReadContent(string path) {
        if (File.Exists(path)) {
            return File.ReadAllText(path);
        }
        else {
            Debug.LogError("Transcript file not found at " + path);
            return null;
        }
    }

    private async void SendReply(string prompt, string input) {
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = input
        };

        // complete the prompt
        if (messages.Count == 0) {
            newMessage.Content = prompt + input + question;
            Debug.Log("Prompt: " + newMessage.Content);
            // Debug.Log("prompt: " + prompt);
            // Debug.Log("input: " + input);
            // Debug.Log("question: " + question);
        }

        messages.Add(newMessage);

        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            // Model = "gpt-4-turbo",
            Model = "gpt-3.5-turbo",
            Messages = messages,
            Temperature = 0.7f,
            N = 1,
            PresencePenalty = 0.5f,
            FrequencyPenalty = 0.1f
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0) {
            response = completionResponse.Choices[0].Message.Content;
            Debug.Log("Response: " + response);

            AnalysisResult result = ResultParser.Parse(response);
            structureAnalysis.text = $"結構性：{result.StructureContent}";
            clarityAnalysis.text = $"清晰性：{result.ClarityContent}";
            completenessAnalysis.text = $"完整性：{result.CompletenessContent}";
            accuracyAnalysis.text = $"正確性：{result.AccuracyContent}";
            structureAnalysisGrade.text = $"{result.StructureScore}";
            clarityAnalysisGrade.text = $"{result.ClarityScore}";
            completenessAnalysisGrade.text = $"{result.CompletenessScore}";
            accuracyAnalysisGrade.text = $"{result.AccuracyScore}";
            Debug.Log("Structure: " + structureAnalysis.text);
            Debug.Log("Clarity: " + clarityAnalysis.text);
            Debug.Log("Completeness: " + completenessAnalysis.text);
            Debug.Log("Accuracy: " + accuracyAnalysis.text);
        }
        else {
            Debug.LogWarning("No text was generated from this prompt.");
        }
    }
}

public class AnalysisResult
{
    public string StructureContent { get; set; }
    public int StructureScore { get; set; }
    public string ClarityContent { get; set; }
    public int ClarityScore { get; set; }
    public string CompletenessContent { get; set; }
    public int CompletenessScore { get; set; }
    public string AccuracyContent { get; set; }
    public int AccuracyScore { get; set; }
}

public class ResultParser
{
    public static AnalysisResult Parse(string response)
    {
        AnalysisResult result = new AnalysisResult();

        string structurePattern = @"結構性：(.*?)分數：(\d+)";
        string clarityPattern = @"清晰性：(.*?)分數：(\d+)";
        string completenessPattern = @"完整性：(.*?)分數：(\d+)";
        string accuracyPattern = @"正確性：(.*?)分數：(\d+)";

        result.StructureContent = Regex.Match(response, structurePattern, RegexOptions.Singleline).Groups[1].Value.Trim();
        result.StructureScore = int.Parse(Regex.Match(response, structurePattern).Groups[2].Value.Trim());

        result.ClarityContent = Regex.Match(response, clarityPattern, RegexOptions.Singleline).Groups[1].Value.Trim();
        result.ClarityScore = int.Parse(Regex.Match(response, clarityPattern).Groups[2].Value.Trim());

        result.CompletenessContent = Regex.Match(response, completenessPattern, RegexOptions.Singleline).Groups[1].Value.Trim();
        result.CompletenessScore = int.Parse(Regex.Match(response, completenessPattern).Groups[2].Value.Trim());

        result.AccuracyContent = Regex.Match(response, accuracyPattern, RegexOptions.Singleline).Groups[1].Value.Trim();
        result.AccuracyScore = int.Parse(Regex.Match(response, accuracyPattern).Groups[2].Value.Trim());

        return result;
    }
}