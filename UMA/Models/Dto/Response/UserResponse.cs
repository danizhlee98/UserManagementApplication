namespace UMA.Models.Dto.Response
{
    public class UserResponse
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public string? AccessToken { get; set; }

        public DateTime? AccessTokenExpire { get; set; }
    }
}
