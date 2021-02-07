namespace Gtt.CodeWorks
{
    public readonly struct TokenMask
    {
        public TokenMask(string token, string mask)
        {
            Token = token;
            Mask = mask;
        }

        public string Token { get; }
        public string Mask { get; }
    }
}