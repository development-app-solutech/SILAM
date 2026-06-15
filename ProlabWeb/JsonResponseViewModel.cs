namespace ProlabWeb
{
    public class JsonResponseViewModel
    {
        public bool success { get; set; }

        public Object data { get; set; }

        public string message { get; set; } = string.Empty;

        public JsonResponseViewModel()
        {
            this.success = false;
        }
    }
}
