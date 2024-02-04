using UnityEngine;

namespace MoshitinEncoded.GraphTools
{
    [System.Serializable]
    public class BlackboardParameterOverride : ScriptableObject
    {
        [SerializeField] private BlackboardParameter _OriginalParameter;
        [SerializeField] private BlackboardParameter _OverrideParameter;

        public BlackboardParameter OriginalParameter => _OriginalParameter;

        public BlackboardParameter OverrideParameter => _OverrideParameter;
    }
}
