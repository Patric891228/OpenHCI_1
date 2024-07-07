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

public class LLM : MonoBehaviour
{
    private OpenAIApi openai = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();
    public List<string> questions = new List<string>();
    public List<string> feedbacks = new List<string>();

    private string prompt = "你是一場報告的觀眾，請針對演講者說出的内容，提供我5個講稿没有提到且方向跟演講内容不同但觀眾可能會問的問題，并針對此講稿5份不同做出100字的回饋，關於内容是否切题，主題内容可以優化改進的地方。請你依照以下格式回答我，問題：1.<question>,2.<question>,3.<question>,4.<question>，回饋：1.<response>,2.<question>,3.<question>,4.<question>,5.<question>\n 請你把内容取代<question>,<response>，請在每個内容中間以空格為間隔。以下是演講逐字稿：";
    private string[] humanproblems = { "香港有哪些適合帶小朋友參觀的景點？", "香港的最佳旅遊季節是什麼時候？", "香港有哪些購物場所推薦？", "香港有哪些必嘗的街頭小吃？", "香港有什麼特別的節慶活動？", "在香港，公共交通工具的運作時間是什麼時候？", "香港有什麼夜生活推薦？", "香港的安全性如何？", "如何從香港國際機場前往市區？"};

    private string cleanTrashPrompt = "請針對以下逐字稿，嘗試把不相關的句子移除，把剩下的字句接起來。";
    private string finalPrompt = "針對以下演講者說出的文字稿，幫我假設你是一個演講的評審，請你針對此演講是否有結構性、清晰性、完整性、流暢性分別做評判，每個判斷都需要舉例，以及說出可以改進的地方，每個項目最高分為25分，請在50字以內并依照以下格式輸出：結構性：<response>分數：<score>清晰性：<response>分數：<score>完整性：<response>分數：<score>流暢性：<response>分數：<score>請幫我取代掉<response>跟<score>，以下是逐字稿：";
    string resultPath = Application.dataPath + "/Files/result.txt";

    private void Start()
    {
        string filePath = Application.dataPath + "/Files/report.pdf";
        string text = ExtractTextFromPDF(filePath);
        SendReply(prompt, text);
    }

    private async void SendReply(string prompt, string input)
    {
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = input
        };

        if (messages.Count == 0) newMessage.Content = prompt + "\n" + input;

        messages.Add(newMessage);

        // Complete the instruction
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-3.5-turbo",
            Messages = messages,
            Temperature = 0.7f,
            N = 1,
            PresencePenalty = 0.5f,
            FrequencyPenalty = 0.1f
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            foreach (var choice in completionResponse.Choices)
            {
                var message = choice.Message;
                message.Content = message.Content.Trim();
                Debug.Log(message.Content);

                questions = ExtractItems(message.Content, "問題：(.*?)回饋：");
                Debug.Log(questions.Count);
                questions.Add(humanproblems[UnityEngine.Random.Range(0, humanproblems.Length)]);
                feedbacks = ExtractItems(message.Content, "回饋：(.*)");
                WriteListToFile(Application.dataPath + "/Files/questions.txt", questions);
                WriteListToFile(Application.dataPath + "/Files/feedbacks.txt", feedbacks);
                messages.Add(message);
            }

        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
        }
    }

    private async void CreateResult()  // fix here
    {
        string finalPath = Application.dataPath + "/Files/Transcript.txt";
        string content = string.Empty;

        if (File.Exists(finalPath))
        {
            content = File.ReadAllText(finalPath); // 讀取整個文件的內容
            Debug.Log(content); // 在控制台中輸出內容
        }
        else
        {
            Debug.LogError("File not found: " + finalPath);
            return;
        }

        var newMessage = new List<ChatMessage>()
        {
            new ChatMessage()
            {
                Role = "user",
                Content = cleanTrashPrompt + content
            }
        };

        // Complete the instruction
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-3.5-turbo",
            Messages = newMessage,
            Temperature = 0.7f,
            N = 1,
            PresencePenalty = 0.5f,
            FrequencyPenalty = 0.1f
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var resultMessage = new ChatMessage()
            {
                Role = "user",
                Content = finalPrompt + completionResponse.Choices[0].Message.Content.Trim()
            };
            var resultResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo",
                Messages = new List<ChatMessage>() { resultMessage },
                Temperature = 0.7f,
                N = 1,
                PresencePenalty = 0.5f,
                FrequencyPenalty = 0.1f
            });

            WriteTextToFile(resultPath, resultResponse.Choices[0].Message.Content.Trim());
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
        }
    }

    private static string ExtractTextFromPDF(string path)
    {
        StringBuilder text = new StringBuilder();
        using (PdfDocument document = PdfDocument.Open(path))
        {
            foreach (Page page in document.GetPages())
            {
                text.Append(page.Text);
            }
        }
        return text.ToString();
    }

    private static List<string> ExtractItems(string text, string pattern)
    {
        List<string> items = new List<string>();
        Match match = Regex.Match(text, pattern, RegexOptions.Singleline);
        if (match.Success)
        {
            string[] parts = match.Groups[1].Value.Split(new string[] { "1.", "2.", "3.", "4.", "5." }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                items.Add(part.Trim());
            }
        }
        return items;
    }

    private void WriteTextToFile(string path, string content)
    {
        File.WriteAllText(path, content);
    }

    static void WriteListToFile(string path, List<string> list)
    {
        File.WriteAllLines(path, list);
    }

    public static string PopFirst(List<string> list)
    {
        if (list.Count == 0)
        {
            return "";
        }
        string firstItem = list[0];
        list.RemoveAt(0);
        return firstItem;
    }
}
