namespace TwitchTokenPoc
{
    public abstract class ApiResult
    {
        public bool TokensChanged { get; set; }
        public string NewAccessToken { get; set; }
        public string NewRefreshToken { get; set; }
        public object Result { get; protected set; }
    }
    
    public class ApiResult<TInnerResult> : ApiResult
    {
        public void SetResult(TInnerResult value) => this.Result = value;
    }
}