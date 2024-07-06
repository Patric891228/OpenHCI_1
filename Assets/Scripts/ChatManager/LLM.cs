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

    private string prompt = "你是一場報告的觀眾，請針對演講者說出的內容，提供我4個講稿沒有提到且方向跟演講內容不同但觀眾可能會問的問題，並針對此講稿5份不同做出100字的回饋，關於內容是否切題，主題內容可以優化改進的地方。請你依照以下格式回答我，問題：1.<question>,2.<question>,3.<question>,4.<question>，回饋：1.<response>,2.<question>,3.<question>,4.<question>,5.<question>\n 請你把內容取代<question>,<response>，請在每個內容中間以空格為間隔。以下是演講逐字稿：";
    // private string transcript = "各位嘉賓，大家好！今天，我們齊聚一堂，共同探討科技如何改變我們的生活。在這個瞬息萬變的時代，科技的進步日新月異，從智能手機到人工智能，我們的世界因為科技而變得更加便利和高效。然而，我們也應該認識到科技的雙刃劍，合理使用才能真正造福於人類。我們要不斷學習、適應變化，並且勇於探索，為未來的科技發展貢獻我們的智慧和力量。讓我們攜手共進，共創美好未來。謝謝大家！";
    private string[] humanproblems = {"- DETR有什麼實際應用到的產品或案例嗎？","目前DETR訓練時間是多久？","如果想要學習這個技術，請問可以怎麼入門？","DETR除了影像辨識之外，還可以訓練甚麼其他方向嗎？","DETR的訓練成本會很高嗎？","DETR跟其他模型架構的差別比較?"};
    private void Start()
    {
        string filePath = Application.dataPath+"/Files/report.pdf";
        string text = ExtractTextFromPDF(filePath);
        Debug.Log("成功解析報告");
        SendReply(prompt ,text);
    }

    private async void SendReply(string prompt,string input)
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
            Model = "gpt-3.5-turbo-0125",
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
                questions = ExtractItems(message.Content, "問題：(.*?)回饋：");
                questions.Add(humanproblems[UnityEngine.Random.Range(0, humanproblems.Length-1)]);
                feedbacks = ExtractItems(message.Content, "回饋：(.*)");
                WriteListToFile(Application.dataPath+"/Files/questions.txt",questions);
                WriteListToFile(Application.dataPath+"/Files/feedbacks.txt",feedbacks);
                messages.Add(message);
            }

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
    static void WriteListToFile(string path,List<string> list)
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
