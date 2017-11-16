using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RegexGenerator
{
    public class MultipleStrings
    {


        public static void Test()
        {
            List<string> inputs = new List<string>();
            inputs.Add("AB23456d-234s");
            inputs.Add("AC999999-9998");
            inputs.Add("BB34234c-223");
            inputs.Add("CD34234h-1000");
            //inputs.Add("(213)304-1072");
            //inputs.Add("234-345-3432");
            //inputs.Add("2153456789");
            List<List<string>> suggestions;
            GiveSuggestions(inputs, out suggestions);

            foreach (var suggest in suggestions)
            {
                Console.Write("--\t");
                foreach (var ch in suggest)
                {
                    Console.Write("{0}", ch);
                }
                Console.Write("\n\t");

                for (int i = 0; i < 10; ++i)
                {

                    List<string> output = new List<string>();
                    PrintHelper(suggest, out output);
                    foreach (var ch in output)
                    {
                        Console.Write("{0}", ch);
                    }
                    Console.Write("\n\t");
                }
                Console.Write("\n");
            }
            return;

        }

        public static void PrintHelper(List<string> input, out List<string> output)
        {
            output = new List<string>();

            Random ran = new Random(Guid.NewGuid().GetHashCode());

            foreach (string symbol in input)
            {
                while (true)
                {
                    int value = ran.Next() % 250 + 33;
                    string ascii = value < 255 ? ((char)value).ToString() : string.Empty;
                    Regex regex = new Regex(symbol);
                    MatchCollection matches = regex.Matches(ascii);
                    if (hasMatch(matches, ascii))
                    {
                        output.Add(ascii);
                        break;
                    }
                }
            }
        }

        private static bool hasMatch(MatchCollection collection, string value)
        {
            foreach (var match in collection)
            {
                if (match.ToString() == value)
                {
                    return true;
                }
            }
            return false;
        }


        public static void GiveSuggestions(List<string> inputs, out List<List<string>> suggestions)
        {
            // do permutation and get all the results
            suggestions = new List<List<string>>();
            List<string> curRegex = new List<string>();
            List<List<string>> regexSuggetions = new List<List<string>>();

            // the permutation don't add "/" to the beginning of special characters
            // give regex suggestions for single string
            TwoStrings.SingleStringSuggestions(TwoStrings.SegmentizeByGroup(inputs[0]), 0, curRegex, regexSuggetions);

            for (int i = 1; i < inputs.Count; ++i)
            {
                List<List<string>> candidates = new List<List<string>>();
                int distance = Int32.MaxValue;
                List<string> secondCharsList = TwoStrings.SegmentizeByCharacter(inputs[i]);
                
                suggestions = new List<List<string>>();
                foreach (var regexSuggestion in regexSuggetions)
                {
                    List<List<int>> direction;
                    // calculate edit distance of first string regex and second string
                    int curDistance = TwoStrings.GetEditDistance(regexSuggestion, secondCharsList, out direction);
                    if (curDistance < distance)
                    {
                        distance = curDistance;
                        suggestions.Clear();
                    }

                    // if edit distance is equal or smaller, modify first string regex to satisfy second string
                    if (distance == curDistance)
                    {
                        List<List<string>> newSuggestion;
                        TwoStrings.ModifySuggestion(regexSuggestion, secondCharsList, direction, out newSuggestion);
                        suggestions.AddRange(newSuggestion);
                    }

                }
                regexSuggetions = suggestions;
            }

            return;
        }
    }
}
