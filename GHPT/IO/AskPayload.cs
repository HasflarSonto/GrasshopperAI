using Newtonsoft.Json;

namespace GHPT.IO
{
    public class AskPayload
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("messages")]
        public List<Message> Messages { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; }

        public AskPayload()
        {
            Messages = new List<Message>();
        }

        public AskPayload(string model, List<Message> messages, double temperature = 0.7)
        {
            this.Model = model;
            this.Messages = messages;
            this.Temperature = temperature;
        }

        public AskPayload(string model, Message msg, double temperature = 0.7)
        {
            this.Model = model;
            this.Messages = new List<Message> { msg };
            this.Temperature = temperature;
        }

        public AskPayload(string model, string msg, double temperature = 0.7)
        {
            this.Model = model;
            this.Messages = new List<Message> { new Message(msg) };
            this.Temperature = temperature;
        }

        public void AddSystemMessage(string content)
        {
            Messages.Add(new Message(content, "system"));
        }

        public void AddUserMessage(string content)
        {
            Messages.Add(new Message(content, "user"));
        }
    }
}
