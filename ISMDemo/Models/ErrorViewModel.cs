namespace ISMDemo.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public string ErrorMessage { get; set; }

        public string ErrorDetail { get; set; }

        public bool IsOriginalError { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}