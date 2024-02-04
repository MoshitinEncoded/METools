using System.Collections.Generic;

using UnityEngine;

namespace MoshitinEncoded.GraphTools
{
    public class Blackboard : ScriptableObject
    {
        [SerializeField] private BlackboardParameter[] _Parameters = new BlackboardParameter[0];

        private Dictionary<string, BlackboardParameter> _ParametersDictionary;

        public BlackboardParameter[] Parameters => _Parameters;

        public Blackboard Clone() =>
            CloneAndOverride(null);

        public Blackboard CloneAndOverride(BlackboardParameterOverride[] overrides)
        {
            var blackboardClone = Instantiate(this);

            var clonedParameters = new BlackboardParameter[_Parameters.Length];
            for (var i = 0; i < _Parameters.Length; i++)
            {
                var parameter = _Parameters[i];
                if (overrides != null && parameter != null)
                {
                    clonedParameters[i] = FindOverride(parameter, overrides);
                }
                
                if (clonedParameters[i] == null)
                {
                    clonedParameters[i] = parameter ? parameter.Clone() : null;
                }
            }

            blackboardClone._Parameters = clonedParameters;

            blackboardClone.InitializeDictionary();

            return blackboardClone;
        }

        private BlackboardParameter FindOverride(BlackboardParameter parameter, BlackboardParameterOverride[] overrides)
        {
            for (var i = 0; i < overrides.Length; i++)
            {
                if (overrides[i].OriginalParameter == parameter)
                {
                    return overrides[i].OverrideParameter;
                }
            }

            return null;
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
            BlackboardParameter parameter = null;

            _ParametersDictionary?.TryGetValue(name, out parameter);
            
            if (parameter == null)
            {
                parameter = FindParameter(name);
            }

            return parameter;
        }

        private BlackboardParameter FindParameter(string name)
        {
            for (var i = 0; i < _Parameters.Length; i++)
            {
                var tempParameter = _Parameters[i];
                if (tempParameter.ParameterName == name)
                {
                    return tempParameter;
                }
            }

            return null;
        }

        private void InitializeDictionary()
        {
            _ParametersDictionary = new(_Parameters.Length);
            foreach (var parameter in _Parameters)
            {
                if (!parameter) continue;
                _ParametersDictionary.Add(parameter.ParameterName, parameter);
            }
        }
    }
}
