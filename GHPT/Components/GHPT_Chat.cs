using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using GHPT.Prompts;
using GHPT.Utils;
using GHPT.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace GHPT.Components
{
    public class GHPT_Chat : GH_Component
    {
        private ChatPromptBuilder _promptBuilder;
        private List<ChatMessage> _chatHistory;
        private const int MAX_HISTORY_LENGTH = 50;
        private GPTConfig _currentConfig;
        public List<GH_Attributes<IGH_Param>> MessageHandlers { get; private set; }

        public GHPT_Chat()
          : base("GHPT Chat", "GHPT Chat",
              "Chat interface for GHPT",
              "GHPT", "Interface")
        {
            _promptBuilder = new ChatPromptBuilder("GHPT/Prompts/chat_examples.txt");
            _chatHistory = new List<ChatMessage>();
            _currentConfig = ConfigUtil.Configs.FirstOrDefault();
            MessageHandlers = new List<GH_Attributes<IGH_Param>>();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Message", "M", "User message", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Send", "S", "Send message", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Response", "R", "AI response", GH_ParamAccess.item);
            pManager.AddGenericParameter("JSON", "J", "JSON response", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string message = string.Empty;
            bool send = false;

            if (!DA.GetData(0, ref message)) return;
            if (!DA.GetData(1, ref send)) return;

            if (send)
            {
                try
                {
                    // Add user message to history
                    _promptBuilder.AddMessage("user", message);
                    _chatHistory.Add(new ChatMessage
                    {
                        Role = "user",
                        Content = message,
                        Timestamp = DateTime.Now
                    });

                    // Get current Grasshopper state
                    var currentState = GetCurrentGrasshopperState();

                    // Build prompt in the same format as prompt.txt
                    string prompt = BuildPrompt(message, currentState);
                    
                    // Process prompt and get response
                    var response = ProcessPrompt(prompt).Result;
                    
                    // Parse JSON response
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response);
                    
                    // Add AI response to history
                    _promptBuilder.AddMessage("assistant", response);
                    _chatHistory.Add(new ChatMessage
                    {
                        Role = "assistant",
                        Content = response,
                        Timestamp = DateTime.Now
                    });

                    // Output results
                    DA.SetData(0, jsonResponse.Advice.ToString());
                    DA.SetData(1, jsonResponse);
                }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error processing message: {ex.Message}");
                }
            }
        }

        private string BuildPrompt(string message, GrasshopperState currentState)
        {
            // Format prompt similar to prompt.txt
            return $@"// Question: {message}
// Reasoning: {GenerateReasoning(message, currentState)}
// JSON: {JsonConvert.SerializeObject(currentState, Formatting.Indented)}";
        }

        private string GenerateReasoning(string message, GrasshopperState currentState)
        {
            // Analyze the current state and message to generate appropriate reasoning
            // This should follow the same pattern as in prompt.txt
            return "Analyzing the current state and user request...";
        }

        private async Task<string> ProcessPrompt(string prompt)
        {
            try
            {
                var response = await ClientUtil.Ask(_currentConfig, prompt);
                return response.Choices.FirstOrDefault()?.Message?.Content ?? "No response received";
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"API Error: {ex.Message}");
                throw;
            }
        }

        private GrasshopperState GetCurrentGrasshopperState()
        {
            var state = new GrasshopperState
            {
                Components = new List<ComponentState>()
            };

            // Get all components in the current document
            var doc = OnPingDocument();
            if (doc == null) return state;

            var components = doc.Objects
                .OfType<IGH_Component>()
                .Where(c => c.ComponentGuid != this.ComponentGuid);

            foreach (var component in components)
            {
                var componentState = new ComponentState
                {
                    Id = component.InstanceGuid.GetHashCode(),
                    Type = component.Name,
                    Position = new PointF(component.Attributes.Pivot.X, component.Attributes.Pivot.Y),
                    Parameters = new Dictionary<string, object>(),
                    Connections = new List<ConnectionPairing>()
                };

                // Get component parameters
                foreach (var param in component.Params.Input)
                {
                    if (param.SourceCount > 0)
                    {
                        var sources = param.Sources;
                        foreach (var source in sources)
                        {
                            componentState.Connections.Add(new ConnectionPairing
                            {
                                FromComponentId = source.Attributes.GetTopLevel.DocObject.InstanceGuid.GetHashCode(),
                                FromParameter = source.Name,
                                ToComponentId = component.InstanceGuid.GetHashCode(),
                                ToParameter = param.Name
                            });
                        }
                    }
                }

                state.Components.Add(componentState);
            }

            return state;
        }

        public override void CreateAttributes()
        {
            m_attributes = (IGH_Attributes)new GHPT_ChatAttributes(this);
        }

        protected override Bitmap Icon => null;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        public override Guid ComponentGuid => new Guid("f1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");
    }

    public class GHPT_ChatAttributes : GH_ComponentAttributes
    {
        private const int PADDING = 5;
        private const int INPUT_HEIGHT = 30;
        private const int BUTTON_WIDTH = 80;
        private const int CHAT_HEIGHT = 200;

        private GHPT_Chat ChatOwner => (GHPT_Chat)Owner;

        public GHPT_ChatAttributes(GHPT_Chat owner) : base(owner)
        {
        }

        protected override void Layout()
        {
            base.Layout();

            // Set component size
            Bounds = new RectangleF(
                Bounds.X,
                Bounds.Y,
                400,
                CHAT_HEIGHT + INPUT_HEIGHT + (PADDING * 3));

            // Layout input controls
            var inputBox = new RectangleF(
                Bounds.X + PADDING,
                Bounds.Y + CHAT_HEIGHT + PADDING,
                Bounds.Width - BUTTON_WIDTH - (PADDING * 3),
                INPUT_HEIGHT);

            var sendButton = new RectangleF(
                inputBox.Right + PADDING,
                inputBox.Y,
                BUTTON_WIDTH,
                INPUT_HEIGHT);

            // Layout chat display
            var chatDisplay = new RectangleF(
                Bounds.X + PADDING,
                Bounds.Y + PADDING,
                Bounds.Width - (PADDING * 2),
                CHAT_HEIGHT);

            // Create controls if they don't exist
            if (ChatOwner.MessageHandlers.Count == 0)
            {
                var inputHandler = new GH_TextBoxAttributes(
                    Rectangle.Round(inputBox),
                    "Type your message...");
                var buttonHandler = new GH_ButtonAttributes(
                    Rectangle.Round(sendButton),
                    "Send");
                var chatHandler = new GH_RichTextBoxAttributes(
                    Rectangle.Round(chatDisplay));

                ChatOwner.MessageHandlers.Add(inputHandler);
                ChatOwner.MessageHandlers.Add(buttonHandler);
                ChatOwner.MessageHandlers.Add(chatHandler);
            }
            else
            {
                // Update existing controls
                var inputHandler = ChatOwner.MessageHandlers[0] as GH_TextBoxAttributes;
                var buttonHandler = ChatOwner.MessageHandlers[1] as GH_ButtonAttributes;
                var chatHandler = ChatOwner.MessageHandlers[2] as GH_RichTextBoxAttributes;

                inputHandler.Bounds = Rectangle.Round(inputBox);
                buttonHandler.Bounds = Rectangle.Round(sendButton);
                chatHandler.Bounds = Rectangle.Round(chatDisplay);
            }
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                // Render chat history
                var chatHandler = ChatOwner.MessageHandlers[2] as GH_RichTextBoxAttributes;
                if (chatHandler != null)
                {
                    chatHandler.Render(canvas, graphics, channel);
                }

                // Render input controls
                var inputHandler = ChatOwner.MessageHandlers[0] as GH_TextBoxAttributes;
                var buttonHandler = ChatOwner.MessageHandlers[1] as GH_ButtonAttributes;

                if (inputHandler != null && buttonHandler != null)
                {
                    inputHandler.Render(canvas, graphics, channel);
                    buttonHandler.Render(canvas, graphics, channel);
                }
            }
        }
    }

    public class ChatStringParam : GH_Param<GH_String>
    {
        public ChatStringParam() : base(new GH_InstanceDescription("Chat String", "CS", "String parameter for chat", "GHPT", "Interface"))
        {
        }

        public override Guid ComponentGuid => new Guid("ea3a2f90-b8b9-406f-bb66-f2a4b9fa3814");
    }

    public class ChatBooleanParam : GH_Param<GH_Boolean>
    {
        public ChatBooleanParam() : base(new GH_InstanceDescription("Chat Boolean", "CB", "Boolean parameter for chat", "GHPT", "Interface"))
        {
        }

        public override Guid ComponentGuid => new Guid("ea3a2f90-b8b9-406f-bb66-f2a4b9fa3815");
    }

    public class GH_TextBoxAttributes : GH_Attributes<IGH_Param>
    {
        private RectangleF _bounds;
        private string _placeholder;
        private ChatStringParam _param;

        public override RectangleF Bounds
        {
            get => _bounds;
            set
            {
                _bounds = value;
                ExpireLayout();
            }
        }

        public GH_TextBoxAttributes(Rectangle bounds, string placeholder) : base(new ChatStringParam())
        {
            _bounds = new RectangleF(bounds.Location, bounds.Size);
            _placeholder = placeholder;
            _param = (ChatStringParam)Owner;
        }

        protected override void Layout()
        {
            base.Layout();
            Bounds = _bounds;
        }

        public new void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != GH_CanvasChannel.Objects)
                return;

            var pen = new Pen(Color.Black);
            var brush = new SolidBrush(Color.White);
            var font = new Font("Arial", 10);

            graphics.FillRectangle(brush, Rectangle.Round(_bounds));
            graphics.DrawRectangle(pen, Rectangle.Round(_bounds));

            if (string.IsNullOrEmpty(_placeholder))
                return;

            var format = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(_placeholder, font, Brushes.Gray, _bounds, format);
        }
    }

    public class GH_ButtonAttributes : GH_Attributes<IGH_Param>
    {
        private RectangleF _bounds;
        private string _text;
        private ChatBooleanParam _param;

        public override RectangleF Bounds
        {
            get => _bounds;
            set
            {
                _bounds = value;
                ExpireLayout();
            }
        }

        public GH_ButtonAttributes(Rectangle bounds, string text) : base(new ChatBooleanParam())
        {
            _bounds = new RectangleF(bounds.Location, bounds.Size);
            _text = text;
            _param = (ChatBooleanParam)Owner;
        }

        protected override void Layout()
        {
            base.Layout();
            Bounds = _bounds;
        }

        public new void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != GH_CanvasChannel.Objects)
                return;

            var pen = new Pen(Color.Black);
            var brush = new SolidBrush(Color.LightGray);
            var font = new Font("Arial", 10);

            graphics.FillRectangle(brush, Rectangle.Round(_bounds));
            graphics.DrawRectangle(pen, Rectangle.Round(_bounds));

            var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(_text, font, Brushes.Black, _bounds, format);
        }
    }

    public class GH_RichTextBoxAttributes : GH_Attributes<IGH_Param>
    {
        private RectangleF _bounds;
        private string _text;
        private ChatStringParam _param;

        public override RectangleF Bounds
        {
            get => _bounds;
            set
            {
                _bounds = value;
                ExpireLayout();
            }
        }

        public GH_RichTextBoxAttributes(Rectangle bounds) : base(new ChatStringParam())
        {
            _bounds = new RectangleF(bounds.Location, bounds.Size);
            _text = string.Empty;
            _param = (ChatStringParam)Owner;
        }

        protected override void Layout()
        {
            base.Layout();
            Bounds = _bounds;
        }

        public new void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != GH_CanvasChannel.Objects)
                return;

            var pen = new Pen(Color.Black);
            var brush = new SolidBrush(Color.White);
            var font = new Font("Arial", 10);

            graphics.FillRectangle(brush, Rectangle.Round(_bounds));
            graphics.DrawRectangle(pen, Rectangle.Round(_bounds));

            if (string.IsNullOrEmpty(_text))
                return;

            var format = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near
            };

            graphics.DrawString(_text, font, Brushes.Black, _bounds, format);
        }
    }
} 