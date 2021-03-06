namespace TwitchTokenPoc
{
    public class ApiResult<TInnerResult>
    {
        public TInnerResult Result { get; set; }
        public bool TokensChanged { get; set; }
        public string NewAccessToken { get; set; }
        public string NewRefreshToken { get; set; }
    }
}