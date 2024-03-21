using System.Collections.Generic;
namespace DreamNet
{
    public class MetaData
    {
        public string NodeKey { get; private set; }
        public object NodeValue { get; set; }
        public Dictionary<string, MetaData> Properties { get; private set; }
        public MetaData this[string key]
        {
            get
            {
                if (HasKey(key))
                {
                    return Properties[key];
                }
                else
                {
                    AddChild(key);
                    return Properties[key];
                }
            }
        }
        public MetaData(string nodeKey,object nodeValue)
        {
            NodeKey = nodeKey;
            NodeValue = nodeValue;
            // Children = new List<MetaNode>();
            Properties = new Dictionary<string, MetaData>();
        }
        public MetaData()
        {
            Properties = new Dictionary<string, MetaData>();
        }

        public string AsString
        {
            get
            {
                return (string)NodeValue;
            }
            set
            {
                NodeValue = value;
            }
        }

        public int AsInt
        {
            get => (int)NodeValue;
            set => NodeValue = value;
        }
        public float AsFloat
        {
            get => (float)NodeValue;
            set => NodeValue = value;
        }

        public string AsStringDef(string value)
        {
            if (NodeValue != null) return (string)NodeValue;
            return value;
        }

        public int AsIntDef(int value)
        {
            if (NodeValue != null) return (int)NodeValue;
            return value;
        }

        public float AsFloatDef(float value)
        {
            if (NodeValue != null) return (float)NodeValue;
            return value;
        }

        // Add a child node
        private void AddChild(string key)
        {
            Properties.Add(key,new MetaData());
        }

        public MetaData GetChild(string key)
        {
            return Properties[key];
        }

        // Remove a child node
        private void RemoveChild(string key)
        {
            // Children.Remove(node);
            Properties.Remove(key);
        }

        // Clear all children nodes
        private void ClearChildren()
        {
            // Children.Clear();
            Properties.Clear();
        }

        public bool HasKey(string key)
        {
            return Properties.ContainsKey(key);
        }
        // Override ToString for readability
        public override string ToString()
        {
            if (NodeValue != null)
                return $"{NodeKey}: {NodeValue}";
            else
                return NodeKey;
        }
    }
}