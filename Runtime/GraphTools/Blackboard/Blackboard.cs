using System.Linq;
using UnityEngine;

namespace MoshitinEncoded.GraphTools
{
    public class Blackboard : ScriptableObject
    {
        [SerializeField] private BlackboardParameter[] _Parameters = new BlackboardParameter[0];

        public BlackboardParameter[] Parameters => _Parameters;

        public BlackboardParameter GetParameter(string name) => _Parameters.FirstOrDefault(p => p.ParameterName == name);

        public Blackboard Clone()
        {
            var blackboardClone = Instantiate(this);

            var clonedParameters = new BlackboardParameter[_Parameters.Length];
            for (var i = 0; i < _Parameters.Length; i++)
            {
                clonedParameters[i] = _Parameters[i].Clone();
            }
            
            blackboardClone._Parameters = clonedParameters;

            return blackboardClone;
        }
    }
}
