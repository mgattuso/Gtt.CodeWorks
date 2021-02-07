namespace Gtt.CodeWorks
{
    public interface ITokenizable
    {
        object ValueObj { get; }
        string Token { get; }
        string MaskedValue { get; }

        string ValueToStoredFormat();
        void SetTokenAndMask(string tokValue, string mask);
        void ValueFromStoredFormat(string val);
        string RawToken();
        bool HasToken();
    }

    public interface ITokenizable<out T> : ITokenizable
    {
        T Value { get; }
    }
}