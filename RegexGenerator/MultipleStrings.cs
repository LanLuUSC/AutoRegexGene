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
            //inputs.Add("AB23456d-234s");
            //inputs.Add("AC999999-9998");
            //inputs.Add("BB34234c-223");
            //inputs.Add("CD34234h-1000");
            inputs.Add("(213)304-1072");
            inputs.Add("234-345-3432");
            inputs.Add("2153456789");
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

                List<string> output = new List<string>();
                PrintHelper(suggest, 0, string.Empty, output);
                foreach (var str in output)
                {
                    Console.Write("{0}\n\t", str);
                }
                Console.Write("\n");

            }
            return;

        }

        public static void PrintHelper(List<string> input, int index, string curResult, List<string> results)
        {
            // use List of candidates to get all the available combinations
            if (input.Count <= index)
            {
                results.Add(curResult);
                return;
            }
            var candidates = GetCandidates(input[index]);

            foreach (string candidate in candidates)
            {
                PrintHelper(input, index + 1, curResult + candidate, results);
            }
            return; 
        }

        private static List<string> GetCandidates(string input)
        {
            // cut input based on regex
            // read until a | or ) is met
            // randomly pick one ascii character and match the part-of-regex
            // if matches, put it in candidate list
            // add empty string to the list if it is also a match

            int index = 0;
            List<string> candidates = new List<string>();

            // corner case string.Empty
            Regex regex = new Regex(input);
            MatchCollection matches = regex.Matches(string.Empty);
            if (hasMatch(matches, string.Empty))
            {
                candidates.Add(string.Empty);
            }
            // add candidates
            while (index < input.Length)
            {
                while (index < input.Length && (input[index] == '(' || input[index] == ')' || input[index] == '|' || input[index] == '?'))
                {
                    ++index;
                }

                string part = string.Empty;
                if (index < input.Length && (input[index] != '(' && input[index] != ')' && input[index] != '|' && input[index] != '?'))
                {
                    while (index < input.Length && (input[index] != '(' && input[index] != ')' && input[index] != '|' && input[index] != '?'))
                    {
                        part += input[index];
                        if (input[index] == '\\')
                        {
                            ++index;
                            if (index < input.Length)
                            {
                                part += input[index];
                            }
                        }
                        ++index;
                    }

                    Random ran = new Random(Guid.NewGuid().GetHashCode());
                    while (true)
                    {
                        int value = ran.Next() % 223 + 32;
                        string ascii = ((char)value).ToString();
                        regex = new Regex(part);
                        matches = regex.Matches(ascii);
                        if (hasMatch(matches, ascii))
                        {
                            candidates.Add(ascii);
                            break;
                        }
                    }
                }
            }

            return candidates;
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
