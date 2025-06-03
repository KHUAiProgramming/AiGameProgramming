using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace BehaviorTree
{

    public class Blackboard
    {
        private Dictionary<string, object> data = new Dictionary<string, object>();

        public T GetValue<T>(string key)
        {
            if (data.TryGetValue(key, out object value))
                return (T)value;
            return default;
        }

        public void SetValue<T>(string key, T value)
        {
            data[key] = value;
        }

        public bool HasKey(string key)
        {
            return data.ContainsKey(key);
        }

        public bool HasValue<T>(string key)
        {
            if (!data.ContainsKey(key))
                return false;

            var value = data[key];
            return value != null && value is T;
        }
    }

    public class BehaviorTree : MonoBehaviour
    {
        private Node root;

        void Start()
        {
            ConstructTree();
        }

        void Update()
        {
            if (root != null)
            {
                root.Evaluate();
            }
        }

        protected virtual void ConstructTree() { }

        public void SetRootNode(Node root)
        {
            this.root = root;
        }
    }
}