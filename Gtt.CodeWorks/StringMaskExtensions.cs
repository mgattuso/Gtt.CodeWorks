using System;

namespace Gtt.CodeWorks
{
    public static class StringMaskExtensions
    {
        public static string ToMasked(this string value, Reveal reveal = Reveal.FirstLast, int charsFirst = 1, int charsLast = 1, char mask = '*')
        {
            if (value == null)
            {
                return null;
            }

            int length = value.Length;

            if (length == 0)
            {
                return Empty;
            }

            if (length == 1)
            {
                return new string(mask, 1);
            }

            if (charsFirst < 1)
            {
                charsFirst = 1;
            }

            if (charsFirst > 4)
            {
                charsFirst = 4;
            }

            if (charsLast < 1)
            {
                charsLast = 1;
            }

            if (charsLast > 4)
            {
                charsLast = 4;
            }

            switch (reveal)
            {
                case Reveal.None:
                    return new string(mask, length);
                case Reveal.Email:
                    if (value.Contains("@"))
                    {
                        var segments = value.Split('@');
                        if (segments[0].Length > 2)
                        {
                            return segments[0].Substring(0, 1)
                                   + new string(mask, segments[0].Length - 1)
                                   + "@"
                                   + segments[1];
                        }

                        return new string(mask, segments[0].Length) + "@" + segments[1];
                    }
                    else
                    {
                        charsFirst = charsFirst * 2 >= length ? length / 2 : charsFirst;
                        return value.Substring(0, charsFirst) + new string(mask, length - charsFirst);
                    }
                case Reveal.First:
                    charsFirst = charsFirst * 2 >= length ? length / 2 : charsFirst;
                    return value.Substring(0, charsFirst) + new string(mask, length - charsFirst);
                case Reveal.Last:
                    charsLast = charsLast * 2 >= length ? length / 2 : charsLast;
                    return new string(mask, length - charsLast) + value.Substring(length - charsLast, charsLast);
                case Reveal.FirstLast:

                    if (length <= 3)
                    {
                        return value.Substring(0, 1) + new string(mask, length - 1);
                    }

                    if (length == 4)
                    {
                        charsFirst = 1;
                        charsLast = 1;
                    }

                    charsFirst = charsFirst * 4 >= length ? length / 4 : charsFirst;
                    charsLast = charsLast * 4 >= length ? length / 4 : charsLast;

                    return value.Substring(0, charsFirst)
                           + new string(mask, length - charsFirst - charsLast)
                           + value.Substring(length - charsLast, charsLast);

                default:
                    throw new ArgumentOutOfRangeException(nameof(reveal), reveal, null);
            }
        }

        public static string ToMasked(this DateTime date)
        {
            return date.ToString("MM/**/yyyy");
        }

        public static string ToMasked(this DateTime? date)
        {
            return date?.ToString("MM/**/yyyy") ?? Empty;
        }

        public const string Empty = "<empty>";
    }
}