namespace API.Errors
{
    public class ApiErrorResponse
    {
        public ApiErrorResponse(int statusCode = 0, string message = null, string details = null)
        {
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
            Details = details;
        }
        public string Message { get; set; }
        public string Details { get; set; }

        private string GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad request",
                401 => "Not authorized",
                403 => "Forbidden",
                404 => "No content found",
                500 => "Internal error",
                _ => null
            };
        }
    }
}