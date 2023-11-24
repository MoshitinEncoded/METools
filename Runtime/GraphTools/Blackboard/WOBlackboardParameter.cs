namespace MoshitinEncoded.GraphTools
{
    public class WOBlackboardParameter<T,T2> where T : T2
    {
        private readonly BlackboardParameter<T2> _BlackboardParameter;
        public WOBlackboardParameter(BlackboardParameter<T2> blackboardParameter)
        {
            _BlackboardParameter = blackboardParameter;
        }

        public T Value { set => _BlackboardParameter.Value = value; }
    }
}
