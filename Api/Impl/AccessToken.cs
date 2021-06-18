namespace MovingSpirit.Api.Impl
{
    internal class AccessToken : IAccessToken
    {
        private readonly string accessToken;

        internal AccessToken(string accessToken)
        {
            this.accessToken = accessToken;
        }

        public string Token => accessToken;
    }
}
