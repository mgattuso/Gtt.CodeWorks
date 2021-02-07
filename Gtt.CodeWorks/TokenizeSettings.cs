using System;

namespace Gtt.CodeWorks
{
    public static class TokenizeSettings
    {
        private static string _tokenPrefix;
        private static string _tokenSuffix;
        private static string _maskPrefix;
        private static string _maskSuffix;
        private static bool _read;
        public static string TokenPrefix
        {
            get
            {
                _read = true;
                return _tokenPrefix;
            }
            set
            {
                if (_read)
                    throw new Exception("Cannot change TokenPrefix value after the data has been read");
                _tokenPrefix = value;
            }
        }
        public static string TokenSuffix
        {
            get
            {
                _read = true;
                return _tokenSuffix;
            }
            set
            {
                if (_read)
                    throw new Exception("Cannot change TokenSuffix value after the data has been read");
                _tokenSuffix = value;
            }
        }

        public static string MaskPrefix
        {
            get
            {
                _read = true;
                return _maskPrefix;
            }
            set
            {
                if (_read)
                    throw new Exception("Cannot change MaskPrefix value after the data has been read");
                _maskPrefix = value;
            }
        }

        public static string MaskSuffix
        {
            get
            {
                _read = true;
                return _maskSuffix;
            }
            set
            {
                if (_read)
                    throw new Exception("Cannot change MaskSuffix value after the data has been read");
                _maskSuffix = value;
            }
        }
    }
}