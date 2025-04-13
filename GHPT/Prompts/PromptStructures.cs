using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GHPT.Prompts
{
    public class PromptData
    {
        public string Advice { get; set; }
        public IEnumerable<Addition> Additions { get; set; }
        public IEnumerable<ConnectionPairing> Connections { get; set; }

        public void ComputeTiers()
        {
            if (Additions == null)
            {
                Additions = new List<Addition>();
                return;
            }

            List<Addition> additions = Additions.ToList();
            if (additions.Count == 0)
                return;

            for (int i = 0; i < additions.Count; i++)
            {
                Addition addition = additions[i];
                int tier = FindParentsRecursive(addition);
                addition.Tier = tier;
                additions[i] = addition;
            }
            Additions = additions;
        }

        public int FindParentsRecursive(Addition child, int depth = 0)
        {
            if (Connections == null)
                return depth;

            try
            {
                List<ConnectionPairing> pairings = Connections.Where(c => c.ToComponentId == child.Id).ToList();
                List<int> depths = new();
                foreach (ConnectionPairing pairing in pairings)
                {
                    if (pairing.IsValid())
                    {
                        Addition parent = Additions.FirstOrDefault(a => a.Id == pairing.FromComponentId);
                        if (parent.Id != 0) // Check if parent was found
                        {
                            int maxDepth = FindParentsRecursive(parent, depth + 1);
                            depths.Add(maxDepth);
                        }
                    }
                }

                if (depths.Count == 0) return depth;
                else if (depths.Count == 1) return depths[0];
                else return depths.Max();
            }
            catch
            {
                return depth;
            }
        }
    }

    public struct Addition
    {
        public string Name { get; set; }
        public int Id { get; set; }
        [JsonProperty("value")]
        public string? Value { get; set; }
        public int Tier { get; set; }
    }

    public struct ConnectionPairing
    {
        [JsonProperty("From")]
        public Connection From { get; set; }
        [JsonProperty("To")]
        public Connection To { get; set; }

        public int FromComponentId { get => From.Id; set => From = new Connection { Id = value, ParameterName = From.ParameterName }; }
        public string FromParameter { get => From.ParameterName; set => From = new Connection { Id = From.Id, ParameterName = value }; }
        public int ToComponentId { get => To.Id; set => To = new Connection { Id = value, ParameterName = To.ParameterName }; }
        public string ToParameter { get => To.ParameterName; set => To = new Connection { Id = To.Id, ParameterName = value }; }

        public bool IsValid()
        {
            return From.IsValid() && To.IsValid();
        }
    }

    public struct Connection
    {
        public int Id { get; set; }
        public string ParameterName { get; set; }

        public bool IsValid()
        {
            return Id > 0 && !string.IsNullOrEmpty(ParameterName);
        }
    }
}
