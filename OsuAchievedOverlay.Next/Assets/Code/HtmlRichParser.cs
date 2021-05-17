using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using System;

public static class HtmlRichParser
{
    public static IEnumerator ParseAsync(Ref<string> input, Action callback = null)
    {
        input.Value = Parse(input.Value);
        yield return ParseImageTag(input);
        if (callback != null)
            callback.Invoke();
        yield return null;
    }

    public static string Parse(string input)
    {
        input = input.Replace("<p>", "").Replace("</p>", "");
        input = input.Replace("<ul>", "").Replace("</ul>", "");
        input = input.Replace("<li>", " • ");
        input = input.Replace("</li>", "");
        input = input.Replace("<hr />", "\n");
        input = input.Replace("<h2>", "<size=29>");
        input = input.Replace("</h2>", "</size>");

        return input;
    }

    private static IEnumerator ParseImageTag(Ref<string> input)
    {
        string regexImgSrc = @"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>";
        MatchCollection matchesImgSrc = Regex.Matches(input.Value, regexImgSrc, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        foreach (Match m in matchesImgSrc)
        {
            List<Group> coll = m.Groups.Cast<Group>().ToList();
            string full = coll[0].Value;
            input.Value = input.Value.Replace(full, "");
        }
        yield return null;
    }
}
