namespace HypnoTech.Common.Dtos.Responses
{
    public class NextPreviousResponse<TClass> where TClass : class
    {
        public TClass Item { get; }

        public string Next { get; }

        public string Previous { get; }

        public bool HasNext => !string.IsNullOrEmpty(Next);

        public bool HasPrevious => !string.IsNullOrEmpty(Previous);

        public NextPreviousResponse(TClass item, string next, string previous)
        {
            Item = item;
            Next = next;
            Previous = previous;
        }
    }
}
