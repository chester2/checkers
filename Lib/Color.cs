namespace Lib
{
    public enum Color
    {
        Black,
        White,
    }

    public static class ColorExtensions
    {
        public static Color Flip(this Color color)
            => (Color)((int)color ^ 1);

        public static char ToChar(this Color color)
            => color.ToString()[0];
    }
}
