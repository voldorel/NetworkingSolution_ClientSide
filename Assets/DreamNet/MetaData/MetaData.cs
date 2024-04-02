using System;
using System.Collections.Generic;
using UnityEngine;

namespace DreamNet
{
    public class MetaData
    {
        public enum DataType
        {
            String,
            Int,
            Float
        }
        public Action<MetaData> OnUpdate;
        public Action<MetaData> OnSetData;
        public MetaData BaseNode { get; private set; }
        public string NodeKey { get;  private set; }
        public DataType ValueDataType { get; private set; }
        public object NodeValue { get;  set; }
        private Dictionary<string, MetaData> Properties { get;  set; }
        
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
            set
            {
                Properties.Add(key,value);
            }
        }
        public MetaData(string nodeKey,MetaData baseNode)
        {
            this.NodeKey = nodeKey;
            this.BaseNode = baseNode;
            Properties = new Dictionary<string, MetaData>();
        }

        public string AsString
        {
            get
            {
                Debug.Log("Node Value :  "+NodeValue.ToString());
                return NodeValue.ToString();
            }
            set
            {
                ValueDataType = DataType.String;
                NodeValue = value;
                OnSetMetaValue(this);
            }
        }

        public int AsInt
        {
            get => (int)NodeValue;
            set
            {
                NodeValue = value;
                ValueDataType = DataType.Int;
                OnSetMetaValue(this);
            }
        }
        public float AsFloat
        {
            get => (float)NodeValue;
            set
            {
                ValueDataType = DataType.Float;
                NodeValue = value;
                OnSetMetaValue(this);
            }
        }

        protected virtual void OnSetMetaValue(MetaData metaData)
        {
            OnSetData?.Invoke(metaData);
            if (BaseNode!=null)
            {
                Debug.Log("OnSetMetaValue  Execute On Base  : "+BaseNode.NodeKey);
                BaseNode.OnSetMetaValue(metaData);
            }
            Debug.Log("OnSetMetaValue  Execute On : "+NodeKey);
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
        protected void AddChild(string key)
        {
            Properties.Add(key,new MetaData(key,this));
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
        public List<string> ModifiedAddress()
        {
            List<string> keyAddress = new List<string>();
            MetaData tempMetaData = this;
            while (tempMetaData.BaseNode!=null)
            {
                keyAddress.Add(tempMetaData.NodeKey);
                tempMetaData = tempMetaData.BaseNode;
            }
            keyAddress.Reverse();
            return keyAddress;
        }
    }
}