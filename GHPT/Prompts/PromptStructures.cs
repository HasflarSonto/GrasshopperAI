using System.Collections.Generic;
using System.Linq;

namespace GHPT.Prompts
{

    public struct PromptData
    {
        public string Advice { get; set; }
        public IEnumerable<Addition> Additions { get; set; }
        public IEnumerable<ConnectionPairing> Connections { get; set; }

        public void ComputeTiers()
        {
            List<Addition> additions = this.Additions.ToList();
            if (additions == null)
                return;

            for (int i = 0; i < additions.Count(); i++)
            {
                Addition addition = additions[i];
                int tier = FindParentsRecursive(addition);
                addition.Tier = tier;

                additions[i] = addition;
            }
            this.Additions = additions;
        }

        public int FindParentsRecursive(Addition child, int depth = 0)
        {
            try
            {
                List<ConnectionPairing> pairings = Connections.Where(c => c.ToComponentId == child.Id).ToList();
                List<int> depths = new();
                foreach (ConnectionPairing pairing in pairings)
                {
                    if (pairing.IsValid())
                    {
                        Addition parent = Additions.FirstOrDefault(a => a.Id == pairing.FromComponentId);
                        int maxDepth = FindParentsRecursive(parent, depth + 1);
                        depths.Add(maxDepth);
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
        public string? Value { get; set; }
        public int Tier { get; set; }
    }

    public struct ConnectionPairing
    {
        public int FromComponentId { get; set; }
        public string FromParameter { get; set; }
        public int ToComponentId { get; set; }
        public string ToParameter { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(FromParameter) && !string.IsNullOrEmpty(ToParameter);
        }
    }

    public struct Connection
    {
        private int _id;
        public int Id { get { return _id == default ? -1 : _id; } set { _id = value; } }
        public string ParameterName { get; set; }

        public bool IsValid()
        {
            return Id > 0 && !string.IsNullOrEmpty(ParameterName);
        }
    }


}
