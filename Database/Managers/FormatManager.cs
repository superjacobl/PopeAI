using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopeAI.Database.Managers;

public enum FormatType
{
    Bytes,
    Numbers
}

public class FormatManager
{

    public static string Format(long num, FormatType type)
    {
        List<string> sizes = new();
        long div = 0;
        if (type == FormatType.Bytes)
        {
            string[] data = { "B", "KB", "MB", "GB", "TB" };
            div = 1024;
            foreach (string item in data) { sizes.Add(item); }
        }
        else if (type == FormatType.Numbers)
        {
            string[] data = { "", "k", "m", "b" };
            div = 1000;
            foreach (string item in data) { sizes.Add(item); }
        }
        int order = 0;
        while (num >= div && order < sizes.Count - 1)
        {
            order++;
            num = num / div;
        }

        string result = string.Format("{0:0.##}{1}", num, sizes[order]);

        return result;
    }
}