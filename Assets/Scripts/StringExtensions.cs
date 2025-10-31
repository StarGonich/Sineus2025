using System.Text;

public static class StringExtensions
{
    public static string Repeat(this string str, int count)
    {
        if (count <= 0) return "";

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            sb.Append(str);
        }
        return sb.ToString();
    }
}