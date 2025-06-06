using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using GHPT.Prompts;
using GHPT.Utils;
using GHPT.IO;
using GHPT.UI;
using GHPT.Builders;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel;

namespace GHPT.Components
{
    public class GHPT_Chat : GH_Component
    {
        private PromptCoordinator _promptCoordinator;
        internal List<ChatMessage> _chatHistory;
        private const int MAX_HISTORY_LENGTH = 50;
        private const int BUTTON_WIDTH = 80;
        private GPTConfig _currentConfig;
        internal string _currentInput = "";
        internal bool _isTyping = false;
        internal bool _isHoveringButton = false;
        public List<GH_Attributes<IGH_Param>> MessageHandlers { get; private set; }
        internal GH_Panel InputPanel { get; private set; }

        public GHPT_Chat()
          : base("GHPT Chat", "GHPT Chat",
              "Chat interface for GHPT",
              "GHPT", "Interface")
        {
            _currentConfig = ConfigUtil.Configs.FirstOrDefault();
            _promptCoordinator = new PromptCoordinator(new GPTClient(_currentConfig));
            _chatHistory = new List<ChatMessage>();
            MessageHandlers = new List<GH_Attributes<IGH_Param>>();
            
            // Initialize the input panel
            InputPanel = new GH_Panel();
            InputPanel.NickName = "Input";
            InputPanel.Description = "Type your message here";
            InputPanel.CreateAttributes();
            
            // Set panel properties
            var props = InputPanel.Properties;
            props.Multiline = true;
            props.Wrap = false; // Don't wrap text
            props.Alignment = GH_Panel.Alignment.Left; // Left align text
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            Instances.DocumentEditor.KeyPress += DocumentEditor_KeyPress;
            
            // Add the input panel to the document
            if (InputPanel != null)
            {
                document.AddObject(InputPanel, false);
                UpdateInputPanelPosition();
            }
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            base.RemovedFromDocument(document);
            Instances.DocumentEditor.KeyPress -= DocumentEditor_KeyPress;
            
            // Remove the input panel from the document
            if (InputPanel != null)
            {
                document.RemoveObject(InputPanel, false);
            }
        }

        private void DocumentEditor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (_isTyping && !char.IsControl(e.KeyChar))
            {
                _currentInput += e.KeyChar;
                Instances.ActiveCanvas.Refresh();
            }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // No input parameters needed anymore
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Response", "R", "AI response", GH_ParamAccess.item);
            pManager.AddTextParameter("Complexity", "C", "Request complexity", GH_ParamAccess.item);
            pManager.AddTextParameter("Analysis", "A", "Request analysis", GH_ParamAccess.item);
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
                    _promptCoordinator.AddMessage("user", message);
                    _chatHistory.Add(new ChatMessage
                    {
                        Role = "user",
                        Content = message,
                        Timestamp = DateTime.Now
                    });

                    // Get current Grasshopper state
                    var currentState = GetCurrentGrasshopperState();

                    // Process request using PromptCoordinator
                    var result = _promptCoordinator.ProcessRequest(message).Result;
                    
                    if (!result.Success)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Error);
                        return;
                    }

                    // Add AI response to history
                    _promptCoordinator.AddMessage("assistant", result.Result.Advice);
                    _chatHistory.Add(new ChatMessage
                    {
                        Role = "assistant",
                        Content = result.Result.Advice,
                        Timestamp = DateTime.Now
                    });

                    // Output results
                    DA.SetData(0, result.Result.Advice);
                    DA.SetData(1, result.Result.Complexity);
                    DA.SetData(2, result.Result.Analysis);
                    DA.SetData(3, JsonConvert.SerializeObject(result.Result));
                }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error processing message: {ex.Message}");
                }
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
            m_attributes = new GHPT_ChatAttributes(this);
        }

        protected override Bitmap Icon => null;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        public override Guid ComponentGuid => new Guid("f1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");

        internal async void SendMessage(string message)
        {
            try
            {
                // Add user message to history
                _promptCoordinator.AddMessage("user", message);
                _chatHistory.Add(new ChatMessage
                {
                    Role = "user",
                    Content = message,
                    Timestamp = DateTime.Now
                });

                // Add status message about starting analysis
                _chatHistory.Add(new ChatMessage
                {
                    Role = "system",
                    Content = "🤔 Analyzing your request...",
                    Timestamp = DateTime.Now
                });

                // Get current Grasshopper state
                var currentState = GetCurrentGrasshopperState();

                // Process request using PromptCoordinator
                var result = await _promptCoordinator.ProcessRequest(message);
                
                if (!result.Success)
                {
                    // Add error message to chat
                    _chatHistory.Add(new ChatMessage
                    {
                        Role = "system",
                        Content = $"❌ Error: {result.Error}",
                        Timestamp = DateTime.Now
                    });
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Error);
                    return;
                }

                // Add analysis result message
                string analysisMessage = "✅ Analysis complete!\n";
                if (result.Result.Additions != null && result.Result.Additions.Any())
                {
                    int componentCount = result.Result.Additions.Count();
                    analysisMessage += $"📊 Complexity: {(componentCount > 1 ? "Complex" : "Simple")}\n";
                    analysisMessage += $"🔧 Will create {componentCount} component{(componentCount > 1 ? "s" : "")}\n";
                    
                    if (componentCount > 1)
                    {
                        analysisMessage += "\n📋 Plan:\n";
                        foreach (var addition in result.Result.Additions)
                        {
                            analysisMessage += $"  • {addition.Name}\n";
                        }
                    }
                }
                else
                {
                    analysisMessage += "📝 No components to create - providing advice only";
                }

                _chatHistory.Add(new ChatMessage
                {
                    Role = "system",
                    Content = analysisMessage,
                    Timestamp = DateTime.Now
                });

                // Add AI response to history
                _promptCoordinator.AddMessage("assistant", result.Result.Advice);
                _chatHistory.Add(new ChatMessage
                {
                    Role = "assistant",
                    Content = result.Result.Advice,
                    Timestamp = DateTime.Now
                });

                // Create components if any
                if (result.Result.Additions != null && result.Result.Additions.Any())
                {
                    var doc = OnPingDocument();
                    if (doc != null)
                    {
                        // Add status message about component creation
                        _chatHistory.Add(new ChatMessage
                        {
                            Role = "system",
                            Content = "🛠️ Creating components...",
                            Timestamp = DateTime.Now
                        });

                        // Compute tiers
                        Dictionary<int, List<Addition>> buckets = new();

                        foreach (Addition addition in result.Result.Additions)
                        {
                            if (buckets.ContainsKey(addition.Tier))
                            {
                                buckets[addition.Tier].Add(addition);
                            }
                            else
                            {
                                buckets.Add(addition.Tier, new List<Addition>() { addition });
                            }
                        }

                        foreach (int tier in buckets.Keys)
                        {
                            int xIncrement = 250;
                            int yIncrement = 100;
                            float x = this.Attributes.Pivot.X + 100 + (xIncrement * tier);
                            float y = this.Attributes.Pivot.Y;

                            foreach (Addition addition in buckets[tier])
                            {
                                try
                                {
                                    GraphUtil.InstantiateComponent(doc, addition, new System.Drawing.PointF(x, y));
                                    y += yIncrement;
                                }
                                catch (Exception ex)
                                {
                                    _chatHistory.Add(new ChatMessage
                                    {
                                        Role = "system",
                                        Content = $"❌ Failed to create component {addition.Name}: {ex.Message}",
                                        Timestamp = DateTime.Now
                                    });
                                }
                            }
                        }

                        // Connect components
                        if (result.Result.Connections != null)
                        {
                            _chatHistory.Add(new ChatMessage
                            {
                                Role = "system",
                                Content = "🔌 Connecting components...",
                                Timestamp = DateTime.Now
                            });

                            foreach (ConnectionPairing connection in result.Result.Connections)
                            {
                                try
                                {
                                    GraphUtil.ConnectComponent(doc, connection);
                                }
                                catch (Exception ex)
                                {
                                    _chatHistory.Add(new ChatMessage
                                    {
                                        Role = "system",
                                        Content = $"❌ Failed to connect components: {ex.Message}",
                                        Timestamp = DateTime.Now
                                    });
                                }
                            }
                        }

                        _chatHistory.Add(new ChatMessage
                        {
                            Role = "system",
                            Content = "✅ Component creation complete!",
                            Timestamp = DateTime.Now
                        });

                        doc.ScheduleSolution(5, d =>
                        {
                            this.ExpireSolution(true);
                        });
                    }
                }
                else
                {
                    // If no components were created, still refresh the solution
                    var doc = OnPingDocument();
                    if (doc != null)
                    {
                        doc.ScheduleSolution(5, d =>
                        {
                            this.ExpireSolution(true);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Add error message to chat
                _chatHistory.Add(new ChatMessage
                {
                    Role = "system",
                    Content = $"❌ Error processing message: {ex.Message}",
                    Timestamp = DateTime.Now
                });
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error processing message: {ex.Message}");
            }
        }

        internal void UpdateInputPanelPosition()
        {
            if (InputPanel == null) return;
            
            // Position the panel relative to the component
            var bounds = this.Attributes.Bounds;
            var panelBounds = new RectangleF(
                bounds.X + 5,
                bounds.Bottom - 125,  // Move up slightly to accommodate taller height
                bounds.Width - GHPT_ChatAttributes.BUTTON_WIDTH - 15, // Use the constant from GHPT_ChatAttributes
                120 // Increased height for input
            );
            
            InputPanel.Attributes.Bounds = panelBounds;
            InputPanel.Attributes.Pivot = new PointF(panelBounds.X, panelBounds.Y);
            InputPanel.Attributes.ExpireLayout();
        }
    }

    public class GHPT_ChatAttributes : GH_ComponentAttributes
    {
        // Constants for layout
        private const int PADDING = 5;
        private const int INPUT_HEIGHT = 120;  // Match the new panel height
        internal const int BUTTON_WIDTH = 40;
        private const int CHAT_HEIGHT = 300;
        private const int SCROLLBAR_WIDTH = 10;

        private RectangleF _inputBox;
        private RectangleF _sendButton;
        private RectangleF _chatDisplay;
        private RectangleF _scrollbar;
        private float _scrollOffset = 0;  // Track scroll position
        private float _maxScroll = 0;     // Maximum scroll value
        private bool _isScrolling = false; // Track if we're currently scrolling
        private bool _autoScroll = true;  // Track if we should auto-scroll

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
                400, // Fixed width
                CHAT_HEIGHT + INPUT_HEIGHT + (PADDING * 3));

            // Layout chat display - now extends to input area
            _chatDisplay = new RectangleF(
                Bounds.X + PADDING,
                Bounds.Y + PADDING,
                Bounds.Width - (PADDING * 2) - SCROLLBAR_WIDTH,
                Bounds.Height - INPUT_HEIGHT - (PADDING * 3)); // Fill space to input

            // Layout scrollbar
            _scrollbar = new RectangleF(
                _chatDisplay.Right + PADDING,
                _chatDisplay.Y,
                SCROLLBAR_WIDTH,
                _chatDisplay.Height);

            // Position send button at bottom
            _sendButton = new RectangleF(
                Bounds.Right - BUTTON_WIDTH - PADDING,
                Bounds.Bottom - INPUT_HEIGHT - PADDING,
                BUTTON_WIDTH,
                INPUT_HEIGHT);

            // Update panel position
            if (ChatOwner.InputPanel != null)
            {
                ChatOwner.UpdateInputPanelPosition();
            }
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (_sendButton.Contains(e.CanvasLocation))
                {
                    if (ChatOwner.InputPanel != null && !string.IsNullOrWhiteSpace(ChatOwner.InputPanel.UserText))
                    {
                        string message = ChatOwner.InputPanel.UserText;
                        ChatOwner.InputPanel.UserText = "";
                        ChatOwner.SendMessage(message);
                        sender.Refresh();
                    }
                    return GH_ObjectResponse.Handled;
                }
                else if (_scrollbar.Contains(e.CanvasLocation))
                {
                    _isScrolling = true;
                    // Calculate scroll position based on click position
                    float scrollRatio = (e.CanvasLocation.Y - _scrollbar.Y) / _scrollbar.Height;
                    _scrollOffset = Math.Max(0, Math.Min(_maxScroll, scrollRatio * _maxScroll));
                    _autoScroll = false; // Disable auto-scroll when user manually scrolls
                    sender.Refresh();
                    return GH_ObjectResponse.Handled;
                }
            }
            return base.RespondToMouseDown(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            bool wasHovering = ChatOwner._isHoveringButton;
            ChatOwner._isHoveringButton = _sendButton.Contains(e.CanvasLocation);
            
            if (_isScrolling && _scrollbar.Contains(e.CanvasLocation))
            {
                // Calculate scroll position based on mouse Y position
                float scrollRatio = (e.CanvasLocation.Y - _scrollbar.Y) / _scrollbar.Height;
                _scrollOffset = Math.Max(0, Math.Min(_maxScroll, scrollRatio * _maxScroll));
                sender.Refresh();
                return GH_ObjectResponse.Handled;
            }
            
            if (wasHovering != ChatOwner._isHoveringButton)
                sender.Refresh();

            return base.RespondToMouseMove(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            _isScrolling = false;
            return base.RespondToMouseUp(sender, e);
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel == GH_CanvasChannel.Objects)
            {
                // Draw component background - darker shade
                var darkBackground = Color.FromArgb(45, 45, 48);
                graphics.FillRectangle(new SolidBrush(darkBackground), Bounds);
                graphics.DrawRectangle(new Pen(Color.FromArgb(60, 60, 65)), Rectangle.Round(Bounds));

                // Draw chat display background
                graphics.FillRectangle(new SolidBrush(darkBackground), _chatDisplay);
                graphics.DrawRectangle(new Pen(Color.FromArgb(60, 60, 65)), Rectangle.Round(_chatDisplay));

                // Draw scrollbar background
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 65)), _scrollbar);
                graphics.DrawRectangle(new Pen(Color.FromArgb(80, 80, 85)), Rectangle.Round(_scrollbar));

                // Draw scrollbar thumb
                if (_maxScroll > 0)
                {
                    float thumbHeight = Math.Max(20, _chatDisplay.Height * (_chatDisplay.Height / (_maxScroll + _chatDisplay.Height)));
                    float thumbY = _scrollbar.Y + (_scrollbar.Height - thumbHeight) * (_scrollOffset / _maxScroll);
                    var thumbRect = new RectangleF(_scrollbar.X, thumbY, _scrollbar.Width, thumbHeight);
                    graphics.FillRectangle(new SolidBrush(Color.FromArgb(80, 80, 85)), thumbRect);
                }

                // Set up text format
                var font = GH_FontServer.Standard;
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Near,
                    Trimming = StringTrimming.None,
                    FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.LineLimit
                };

                // Calculate available width for text
                float availableWidth = _chatDisplay.Width - (PADDING * 2);

                // First measure all messages to get their heights
                var messageHeights = new List<(float Height, bool IsUser, bool IsSystem)>();
                float totalHeight = 0;
                foreach (var message in ChatOwner._chatHistory)
                {
                    string prefix = message.Role == "user" ? "You: " : 
                                  message.Role == "system" ? "System: " : "AI: ";
                    string text = prefix + message.Content;
                    
                    var size = graphics.MeasureString(text, font, (int)availableWidth, format);
                    messageHeights.Add((size.Height + 8, message.Role == "user", message.Role == "system")); // Increased padding
                    totalHeight += size.Height + 8;
                }

                // Update max scroll value
                _maxScroll = Math.Max(0, totalHeight - _chatDisplay.Height);

                // If auto-scroll is enabled, scroll to bottom
                if (_autoScroll)
                {
                    _scrollOffset = _maxScroll;
                }

                // Calculate starting Y position with scroll offset
                float y = _chatDisplay.Y + PADDING - _scrollOffset;

                // Create clipping region for chat display
                graphics.SetClip(_chatDisplay);

                // Draw messages
                for (int i = 0; i < ChatOwner._chatHistory.Count; i++)
                {
                    var message = ChatOwner._chatHistory[i];
                    var (height, isUser, isSystem) = messageHeights[i];
                    
                    if (y + height > _chatDisplay.Y && y < _chatDisplay.Bottom)
                    {
                        // Draw message background
                        var msgBounds = new RectangleF(
                            _chatDisplay.X + PADDING,
                            y,
                            availableWidth,
                            height - 5
                        );

                        if (isUser)
                        {
                            // User message - darker shade
                            graphics.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 65)), msgBounds);
                        }
                        else if (isSystem)
                        {
                            // System message - different shade
                            graphics.FillRectangle(new SolidBrush(Color.FromArgb(50, 50, 55)), msgBounds);
                        }

                        string prefix = message.Role == "user" ? "You: " : 
                                      message.Role == "system" ? "System: " : "AI: ";
                        string text = prefix + message.Content;
                        
                        // Draw the text with appropriate color
                        var textColor = isSystem ? Color.FromArgb(180, 180, 180) : Color.FromArgb(230, 230, 230);
                        graphics.DrawString(text, font, new SolidBrush(textColor),
                            new RectangleF(_chatDisplay.X + PADDING, y, availableWidth, height - 5),
                            format);
                    }
                    
                    y += height;
                }

                // Reset clipping
                graphics.ResetClip();

                // Draw send button
                var buttonBrush = ChatOwner._isHoveringButton ? 
                    new SolidBrush(Color.FromArgb(70, 70, 75)) : 
                    new SolidBrush(Color.FromArgb(60, 60, 65));
                graphics.FillRectangle(buttonBrush, _sendButton);
                graphics.DrawRectangle(new Pen(Color.FromArgb(80, 80, 85)), Rectangle.Round(_sendButton));
                
                var buttonText = "Send";
                var buttonFont = GH_FontServer.Standard;
                var buttonSize = graphics.MeasureString(buttonText, buttonFont);
                var buttonPoint = new PointF(
                    _sendButton.X + (_sendButton.Width - buttonSize.Width) / 2,
                    _sendButton.Y + (_sendButton.Height - buttonSize.Height) / 2
                );
                graphics.DrawString(buttonText, buttonFont, new SolidBrush(Color.FromArgb(230, 230, 230)), buttonPoint);
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