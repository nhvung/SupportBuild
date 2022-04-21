class CommandLineExtension
{
    const string _quot = "\"";
    const string _comment = "#";
    static readonly char[] _delimiters = new char[] { ' ', '\t' };
    public static string[] ReadValuesPerLine(string line, char[] delimiters = default)
    {
        if (delimiters == null || delimiters.Length == 0)
        {
            delimiters = _delimiters;
        }
        List<string> values = new List<string>();
        try
        {
            int lastIdx = -1;
            int idx = 0;
            bool inText = false;
            string value;
            while (idx < line.Length)
            {
                if (line[idx] == '\"')
                {
                    inText = !inText;
                }
                else if (delimiters.Contains(line[idx]))
                {
                    if (!inText)
                    {
                        value = line.Substring(lastIdx + 1, idx - lastIdx)?.Trim(' ', ',');
                        if (value.StartsWith(_quot) && value.EndsWith(_quot))
                        {
                            value = value.Substring(1, value.Length - 2)?.Replace(_quot + _quot, _quot);
                        }
                        values.Add(value);
                        lastIdx = idx;
                    }
                }
                idx++;
            }
            if (lastIdx > -1)
            {
                value = line.Substring(lastIdx)?.Trim(' ', ',');
                if (value.StartsWith(_quot) && value.EndsWith(_quot))
                {
                    value = value.Substring(1, value.Length - 2);
                }
            }
            else
            {
                value = line;
            }
            values.Add(value);
        }
        catch //(Exception ex)
        {

        }
        return values.ToArray();
    }


}