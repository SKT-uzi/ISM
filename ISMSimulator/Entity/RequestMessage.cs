namespace ISMSimulator.Entity
{
    public class RequestMessage
    {
        public string Method { get; set; }

        public string Type { get; set; }

        public string Args { get; set; }

        public object Value { get; set; }
    }
}
