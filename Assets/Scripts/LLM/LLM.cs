using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenAI
{
    public class LLM : MonoBehaviour
    {
        private OpenAIApi openai = new OpenAIApi();

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "你是一場報告的觀眾，請針對演講者說出的內容，提供我五個講稿沒有提到且方向跟演講內容不同但觀眾可能會問的問題，並針對此講稿5份不同做出100字的回饋，關於內容是否切題，主題內容可以優化改進的地方。請你依照以下格式回答我，問題：1.<question>\n2.<question>\n3.<question>\n4.<question>\n5.<question>，回饋：1.<response>\n2.<question>\n3.<question>\n4.<question>\n5.<question>\n 請你把內容取代s<question>,<response>。以下是演講逐字稿：";
        private string transcript = "各位嘉賓，大家好！今天，我們齊聚一堂，共同探討科技如何改變我們的生活。在這個瞬息萬變的時代，科技的進步日新月異，從智能手機到人工智能，我們的世界因為科技而變得更加便利和高效。然而，我們也應該認識到科技的雙刃劍，合理使用才能真正造福於人類。我們要不斷學習、適應變化，並且勇於探索，為未來的科技發展貢獻我們的智慧和力量。讓我們攜手共進，共創美好未來。謝謝大家！";
        
        private void Start()
        {
            SendReply(prompt,transcript);
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
                    Debug.Log(message.Content); 
                    messages.Add(message);
                }

            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }

    }
}
