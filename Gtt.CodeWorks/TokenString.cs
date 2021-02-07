using System;

namespace Gtt.CodeWorks
{
    public class TokenString : TokenBase<string>
    {
        public TokenString(TokenMask tokenMask) : this(tokenMask.Token)
        {
            MaskedValue = tokenMask.Mask;
        }

        public TokenString(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            if (value.StartsWith(TokenPrefix) && value.EndsWith(TokenSuffix))
            {
                Token = value;
            }
            else if (value.StartsWith(TokenPrefix) && value.EndsWith(MaskSuffix))
            {
                var tokenEnd = value.IndexOf(TokenSuffix, StringComparison.Ordinal);

                var token = value.Substring(0, tokenEnd + TokenSuffix.Length);
                var mask = value.Substring(tokenEnd + TokenSuffix.Length);
                Token = token;
                MaskedValue = UnwrapValue(mask, MaskPrefix, MaskSuffix);
            }
            else
            {
                Value = value;
                MaskedValue = value?.ToMasked();
            }
        }

        public override string ValueToStoredFormat()
        {
            if (!string.IsNullOrWhiteSpace(Value))
            {
                return Value;
            }

            return null;
        }

        public override void ValueFromStoredFormat(string val)
        {
            Value = val;
        }

        protected override bool ValueIsPresent()
        {
            return !string.IsNullOrWhiteSpace(Value);
        }

        public static implicit operator TokenString(string value)
        {
            return new TokenString(value);
        }

        public static implicit operator TokenString(TokenMask value)
        {
            return new TokenString(value);
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Token)) return Value.ToMasked();

            var t = Token;
            if (!string.IsNullOrWhiteSpace(MaskedValue))
            {
                t = t + MaskPrefix + Mask + MaskSuffix;
            }
            return t;

        }

        public override int GetHashCode()
        {
            if (HasToken())
            {
                return Token.GetHashCode();
            }

            if (ValueIsPresent())
            {
                return Value.GetHashCode();
            }

            return "".GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return Token == null && Value == null;
            }

            if (HasToken())
            {
                return obj.ToString().Equals(Token) || obj.ToString().Equals(ToString());
            }

            if (ValueIsPresent())
            {
                return Value.Equals(obj.ToString());
            }

            return false;
        }
    }
}