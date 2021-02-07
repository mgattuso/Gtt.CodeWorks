using System;

namespace Gtt.CodeWorks
{
    public class TokenDate : TokenBase<DateTime?>
    {
        public TokenDate(TokenMask tokenMask) : this(tokenMask.Token)
        {
            MaskedValue = tokenMask.Mask;
        }
        public TokenDate(DateTime? value)
        {
            Value = value;
            MaskedValue = value?.ToMasked();
        }

        public TokenDate(string value)
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
                if (DateTime.TryParse(value, out var parsedDate))
                {
                    Value = parsedDate;
                }
                MaskedValue = Value.ToMasked();
            }
        }

        public override string ValueToStoredFormat()
        {
            return Value?.ToString("yyyy-MM-dd");
        }

        public override void ValueFromStoredFormat(string val)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                Value = null;
            }

            Value = DateTime.Parse(val);
        }

        protected override bool ValueIsPresent()
        {
            return Value != null;
        }

        public static implicit operator TokenDate(string value)
        {
            return new TokenDate(value);
        }

        public static implicit operator TokenDate(TokenMask value)
        {
            return new TokenDate(value);
        }

        public static implicit operator TokenDate(DateTime value)
        {
            return new TokenDate(value);
        }

        public static implicit operator TokenDate(DateTime? value)
        {
            return new TokenDate(value);
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
    }
}