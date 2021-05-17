using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OsuAchievedOverlay.Helpers
{
    public static class StringHelper
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static List<Tuple<string, string, string>> QueryParser(string query)
        {
            string[] data = Regex.Split(query, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            List<Tuple<string, string, string>> res = new List<Tuple<string, string, string>>();
            if (data.Length > 0)
            {
                foreach (string queryPart in data)
                {
                    if (queryPart.Contains("="))
                    {
                        string[] splitPart = queryPart.Split(new[] { '=' }, 2);
                        res.Add(new Tuple<string, string, string>(splitPart[0], splitPart[1].Trim('"'), "="));
                    }
                }
            }

            return res;
        }

        public static bool Contains(this string[] arr, string data, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase){
            foreach (string s in arr)
                if (s.Contains(data) || s.Equals(data, stringComparison))
                    return true;

            return false;
        }
    }
}
