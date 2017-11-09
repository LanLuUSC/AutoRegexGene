using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexGenerator
{
    public class SingleString
    {
        public void CreateRegexSuggestion(string input, out List<string> regexList, out List<string> regexShortList)
        {
            // trim
            input.Trim();

            // segment regular expression
            int index = 0;
            List<string> segments = new List<string>();
            while (index < input.Length)
            {
                string newSeg = input[index].ToString();

                if (Char.IsLetter(input[index]))
                {
                    index++;
                    while (index < input.Length && Char.IsLetter(input[index]))
                    {
                        newSeg += input[index++];
                    }
                }
                else if (Char.IsDigit(input[index]))
                {
                    index++;
                    while (index < input.Length && Char.IsDigit(input[index]))
                    {
                        newSeg += input[index++];
                    }
                }
                else
                {
                    index++;
                }
                segments.Add(newSeg);
            }

            // do permutation and get all the results
            regexList = new List<string>();
            regexShortList = new List<string>();
            Permutation(segments, 0, string.Empty, string.Empty, regexList, regexShortList);

            // finalResults is shown here
            return;
        }

        static void Permutation(List<string> segments, int index, string curRegex, string curShortRegex, List<string> finalResults, List<string> finalShortResults)
        {
            // use recursion
            // to each segment
            // add its reges representation
            // or original character to curRegex
            // then Recur Permutation
            // if there is no more segments, put curRegex to our final Results

            if (index >= segments.Count)
            {
                finalResults.Add(curRegex);
                finalShortResults.Add(curShortRegex);
                return;
            }

            string curSeg = segments[index];
            string segRegex = CreateRegex(curSeg);

            if (curSeg.All(char.IsLetterOrDigit))
            {
                Permutation(segments, index + 1, curRegex + segRegex, curShortRegex + CreateRegex(curSeg[0].ToString()) + "{" + curSeg.Length + "}", finalResults, finalShortResults);
                Permutation(segments, index + 1, curRegex + curSeg, curShortRegex + curSeg, finalResults, finalShortResults);
            }
            else
            {
                Permutation(segments, index + 1, curRegex + segRegex, curShortRegex + segRegex, finalResults, finalShortResults);
            }

            return;
        }

        static string CreateRegex(string input)
        {
            StringBuilder regex = new StringBuilder();

            const string specialCharacters = @"[\/^$.|?*+(){}";
            foreach (var ch in input)
            {
                string cur = specialCharacters.Contains(ch) ? @"\" + ch.ToString() : ch.ToString();
                if (Char.IsNumber(ch))
                {
                    cur = @"\d";
                }
                else if (Char.IsLetter(ch))
                {
                    cur = @"[a-zA-Z]";
                }

                regex.Append(cur);
            }

            return regex.ToString();
        }
    }
}
