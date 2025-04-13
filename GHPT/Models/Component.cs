using System.Drawing;

namespace GHPT.Models
{
    public class Component
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public object Value { get; set; }
        public PointF Position { get; set; }
    }
} 