using System;
using System.Globalization;

namespace Gtt.CodeWorks
{
    public class ClientTokenDate : TokenBase
    {
        private readonly string _value;

        protected ClientTokenDate(DateTime date)
        {
            OriginalDate = date;
            _value = date.ToString("O");
            MaskedValue = "";
        }

        protected ClientTokenDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            _value = value;

            if (value.StartsWith(TokenPrefix) && value.EndsWith(TokenSuffix))
            {
                _value = value;
                TokenValue = UnwrapValue(value, TokenPrefix, TokenSuffix);
                MaskedValue = "";
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

        public DateTime? OriginalDate { get; }

        public string OriginalValue => _value;

        public static implicit operator ClientTokenDate(string value)
        {
            return new ClientTokenDate(value);
        }

        public static implicit operator ClientTokenDate(DateTime value)
        {
            return new ClientTokenDate(value);
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public bool IsTokenized()
        {
            return OriginalDate == null;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return Equals(obj as ClientTokenDate);
        }

        protected bool Equals(ClientTokenDate other)
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