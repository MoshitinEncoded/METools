namespace MoshitinEncoded.GraphTools
{
    internal class ROBlackboardParameter<T,T2> where T2 : T
    {
        private readonly BlackboardParameter<T2> _BlackboardParameter;
        public ROBlackboardParameter(BlackboardParameter<T2> blackboardParameter)
        {
            _BlackboardParameter = blackboardParameter;
        }

        public T Value => _BlackboardParameter.Value;
    }
}
