using System;
using System.Collections.Generic;
using System.Linq;

namespace AliceMQ.Serialize
{
    public static class JsonPropertyNameSetterExtension
    {
        public static string FromPascalToJson(this string input)
        {
            var firstC = input[0].ToString().ToLower();
            var others = 
                new String(
                    input.ToCharArray().Skip(1).Select(c => UpperCaseChars.Contains(c) ? 
                            new[] { '_', CharToLower(c) } : new[] { c })
                        .SelectMany(s => s).ToArray()
                );
            return $"{firstC}{others}";
        }

        private static char CharToLower(char c) => c.ToString().ToLower()[0];

        private static IEnumerable<char> UpperCaseChars => Enumerable.Range(0, 26).Select(i => Convert.ToChar('A' + i));
    }
}