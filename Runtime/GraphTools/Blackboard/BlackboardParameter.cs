using UnityEngine;

namespace MoshitinEncoded.GraphTools
{
    public abstract class BlackboardParameter : ScriptableObject
    {

#if UNITY_EDITOR
        [SerializeField] private bool _IsExpanded;
#endif

        [SerializeField] private string _ParameterName;

        public string ParameterName
        {
            get => _ParameterName;
            set => _ParameterName = value;
        }

        internal abstract bool TryGetValue<T>(out T value);
        internal abstract bool TrySetValue<T>(T value);
        internal BlackboardParameter Clone() =>
            Instantiate(this);
    }

    public abstract class BlackboardParameter<T> : BlackboardParameter
    {
        [SerializeField] private T _Value;

        public T Value {
            get => _Value;
            set => _Value = value;
        }

        internal override bool TryGetValue<T1>(out T1 value)
        {
            if (Value != null && Value is T1 returnValue)
            {
                value = returnValue;
            }
            else
            {
                value = default;
            }

            return typeof(T1).IsAssignableFrom(typeof(T));
        }

        internal override bool TrySetValue<T1>(T1 value)
        {
            if (!typeof(T).IsAssignableFrom(typeof(T1)))
            {
                return false;
            }

            if (value != null && value is T setValue)
            {
                Value = setValue;
            }
            else
            {
                Value = default;
            }

            return true;
        }
    }
}