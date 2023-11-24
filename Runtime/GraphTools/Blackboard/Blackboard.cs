using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace MoshitinEncoded.GraphTools
{
    public class Blackboard : ScriptableObject
    {
        [SerializeField] private BlackboardParameter[] _Parameters = new BlackboardParameter[0];

        private Dictionary<string, BlackboardParameter> _ParametersDictionary;

        public BlackboardParameter[] Parameters => _Parameters;

        public Blackboard Clone()
        {
            var blackboardClone = Instantiate(this);

            var clonedParameters = new BlackboardParameter[_Parameters.Length];
            for (var i = 0; i < _Parameters.Length; i++)
            {
                clonedParameters[i] = _Parameters[i].Clone();
            }

            blackboardClone._Parameters = clonedParameters;

            blackboardClone.InitializeDictionary();

            return blackboardClone;
        }

        public BlackboardParameter<T> GetParameter<T>(string name)
        {
            var parameter = GetParameter(name);
            if (parameter && parameter is BlackboardParameter<T> typedParameter)
            {
                return typedParameter;
            }
            else
            {
                return null;
            }
        }

        public BlackboardParameter GetParameter(string name)
        {
            BlackboardParameter parameter;
            if (_ParametersDictionary != null)
            {
                _ParametersDictionary.TryGetValue(name, out parameter);
            }
            else
            {
                parameter = _Parameters.FirstOrDefault(p => p.ParameterName == name);
            }

            return parameter;
        }

        private void InitializeDictionary()
        {
            _ParametersDictionary = new(Parameters.Length);
            foreach (var parameter in Parameters)
            {
                _ParametersDictionary.Add(parameter.ParameterName, parameter);
            }
        }
    }
}
