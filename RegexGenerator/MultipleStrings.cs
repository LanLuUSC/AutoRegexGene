using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexGenerator
{
    public class MultipleStrings
    {


        public static void Test()
        {
            List<string> inputs = new List<string>();
            inputs.Add("(213)304-1072");
            inputs.Add("234-345-3432");
            inputs.Add("2153456789");
            List<List<string>> suggestions;
            GiveSuggestions(inputs, out suggestions);

            return;

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
