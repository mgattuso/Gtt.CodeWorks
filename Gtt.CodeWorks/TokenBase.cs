using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{
    public abstract class TokenBase<T> : ITokenizable<T>
    {
        protected string TokenPrefix;
        protected string TokenSuffix;
        protected string MaskPrefix;
        protected string MaskSuffix;

        protected TokenBase()
        {
            TokenPrefix = TokenizeSettings.TokenPrefix ?? "[T__";
            TokenSuffix = TokenizeSettings.TokenSuffix ?? "__T]";
            MaskPrefix = TokenizeSettings.MaskPrefix ?? "[M__";
            MaskSuffix = TokenizeSettings.MaskSuffix ?? "__M]";
        }

        public object ValueObj { get; protected set; }
        public string Token { get; protected set; }
        public string MaskedValue { get; protected set; }

        public string Mask => UnwrapValue(MaskedValue, MaskPrefix, MaskSuffix);

        public abstract string ValueToStoredFormat();

        public void SetTokenAndMask(string token, string mask)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                Token = null;
                if (ValueIsPresent())
                {
                    throw new Exception("No token provided but value is present");
                }

                return;
            }

            if (token.StartsWith(TokenPrefix) && token.EndsWith(TokenSuffix))
            {
                Token = token;
                Value = default(T);
                return;
            }

            Value = default(T);
            Token = TokenPrefix + token + TokenSuffix;
            MaskedValue = mask;
        }

        public abstract void ValueFromStoredFormat(string val);

        protected abstract bool ValueIsPresent();

        public string RawToken()
        {
            var t = UnwrapValue(Token, TokenPrefix, TokenSuffix);
            return t;
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

        public T Value
        {
            get => (T)ValueObj;
            set => ValueObj = value;
        }

        public bool HasToken()
        {
            return !string.IsNullOrWhiteSpace(Token);
        }

        public bool HasOriginalValues()
        {
            return ValueObj != null;
        }
    }
}
