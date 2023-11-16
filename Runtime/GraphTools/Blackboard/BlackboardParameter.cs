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

        public virtual object GetValue() { return null; }

        public virtual bool SetValue(object value) { return true; }
        
        internal BlackboardParameter Clone() =>
            Instantiate(this);
    }

    public class BlackboardParameter<T> : BlackboardParameter
    {
        [SerializeField] private T _Value;

        public override object GetValue()
        {
            return _Value;
        }

        public override bool SetValue(object value)
        {
            if (value is T newValue)
            {
                _Value = newValue;
                return true;
            }
            else if (value == null)
            {
                _Value = default;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}