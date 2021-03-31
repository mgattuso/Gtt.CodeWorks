using System;

namespace Gtt.CodeWorks
{
    public class ClientTokenString : TokenBase
    {
        private readonly string _value;

        protected ClientTokenString(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            _value = value;

            if (value.StartsWith(TokenPrefix) && value.EndsWith(TokenSuffix))
            {
                _value = value;
            }
            else if (value.StartsWith(TokenPrefix) && value.EndsWith(MaskSuffix))
            {
                var tokenEnd = value.IndexOf(TokenSuffix, StringComparison.Ordinal);
                var token = value.Substring(0, tokenEnd + TokenSuffix.Length);
                TokenValue = UnwrapValue(token, TokenPrefix, TokenSuffix);
                var mask = value.Substring(tokenEnd + TokenSuffix.Length);
                MaskedValue = UnwrapValue(mask, MaskPrefix, MaskSuffix);
            }
            else
            {
                MaskedValue = "";
            }
        }

        public string MaskedValue { get; }
        public string TokenValue { get; }

        public string OriginalValue => _value;

        public static implicit operator ClientTokenString(string value)
        {
            return new ClientTokenString(value);
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public bool IsTokenized()
        {
            return !string.IsNullOrEmpty(TokenValue);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return Equals(obj as ClientTokenString);
        }

        protected bool Equals(ClientTokenString other)
        {
            if (other == null) return false;
            return _value == other._value;
        }

        public override int GetHashCode()
        {
            return (_value != null ? _value.GetHashCode() : 0);
        }

        protected static string UnwrapValue(string value, string prefix, string suffix)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (string.IsNullOrWhiteSpace(prefix)) return value;
            if (string.IsNullOrWhiteSpace(suffix)) return value;

            var v = value;
            if (v.StartsWith(prefix) && v.EndsWith(suffix))
            {
                v = v.Substring(prefix.Length, v.Length - prefix.Length - suffix.Length);
            }

            return v;
        }
    }
}