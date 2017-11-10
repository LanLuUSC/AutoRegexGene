using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RegexGenerator
{
    public class TwoStrings
    {
        // traceback direction in direction matrix
        const int DIR_DOWN = 1;
        const int DIR_LEFT = 1 << 1;
        const int DIR_DIAGONAL = 1 << 2;
        // edit distance of delete, insert and replace
        const int DIS_DEL_INS = 1;
        const int DIS_SUB = 1;
        public static void Test()
        {
            List<string> results = new List<string>();
            List<List<string>> suggestions;
            CreateRegexSuggestion("(213)304-1072", "234-345-3432", out suggestions);
            return;
        }

        /// <summary>
        /// Create regex suggestions with two string inputs
        /// </summary>
        /// <param name="first">the first input string</param>
        /// <param name="second">the second input string</param>
        /// <param name="suggestions"></param>
        public static void CreateRegexSuggestion(string first, string second, out List<List<string>> suggestions)
        {
            // do permutation and get all the results
            suggestions = new List<List<string>>();
            List<string> curRegex = new List<string>();
            List<List<string>> regexSuggetions = new List<List<string>>();

            // the permutation don't add "/" to the beginning of special characters
            // give regex suggestions for single string
            SingleStringSuggestions(SegmentizeByGroup(first), 0, curRegex, regexSuggetions);

            List<List<string>> candidates = new List<List<string>>();
            int distance = Int32.MaxValue;
            List<string> secondCharsList = SegmentizeByCharacter(second);
            foreach (var regexSuggestion in regexSuggetions)
            {
                List<List<int>> direction;
                // calculate edit distance of first string regex and second string
                int curDistance = GetEditDistance(regexSuggestion, secondCharsList, out direction);
                if (curDistance < distance)
                {
                    distance = curDistance;
                    suggestions.Clear();
                }

                // if edit distance is equal or smaller, modify first string regex to satisfy second string
                if (distance == curDistance)
                {
                    List<List<string>> newSuggestion;
                    ModifySuggestion(regexSuggestion, secondCharsList, direction, out newSuggestion);
                    suggestions.AddRange(newSuggestion);
                }

            }
        }

        static int GetEditDistance(List<string> first, List<string> second,out List<List<int>> direction)
        {
            // calculate edit distance and keep the backtrace path
            int rows = first.Count + 1;
            int cols = second.Count + 1;

            List<List<int>> matrix = new List<List<int>>(rows);
            direction = new List<List<int>>(rows);
            // initialize the dp matrix
            for (int i = 0; i < rows; ++i)
            {
                List<int> tmp1 = new List<int>();
                List<int> tmp2 = new List<int>();

                for (int j = 0; j < cols; ++j)
                {
                    tmp1.Add(0);
                    tmp2.Add(0);
                }
                matrix.Add(tmp1);
                direction.Add(tmp2);
            }


            // get edit distance and its trace back instance using dp
            // M[i][j]: edit distance of first[0..i-1] and second[0..j-1]
            // suppose edit distance of a single delete or insert is 2, while replace is 1
            // eg. ag->ags insert, distance 2; agb->agc replace, distance 1
            // induction rule: M[i][j] = min(M[i-1][j] + 2, M[i][j-1] + 2, M[i-1][j-1] + first[i-1] == second[j-1]? 0: 1)
            // initial: M[0][j] = j, M[i][0]  = i 
            // direction matrix: 0b001 y direction, 0b010 x direction, ob100 diagnal direction

            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    if (i == 0 || j == 0)
                    {
                        matrix[i][j] = i == 0 ? j * DIS_DEL_INS : i * DIS_DEL_INS;
                        direction[i][j] = i == 0 ? DIR_LEFT : DIR_DOWN;
                    }
                    else
                    {
                        if (isEqual(first[i - 1], second[j - 1]))
                        {
                            matrix[i][j] = Math.Min(Math.Min(matrix[i - 1][j] + DIS_DEL_INS, matrix[i][j - 1] + DIS_DEL_INS), matrix[i - 1][j - 1]);
                            if (matrix[i][j] == matrix[i - 1][j] + DIS_DEL_INS)
                            {
                                direction[i][j] |= DIR_DOWN;
                            }
                            if (matrix[i][j] == matrix[i][j - 1] + DIS_DEL_INS)
                            {
                                direction[i][j] |= DIR_LEFT;
                            }
                            if (matrix[i][j] == matrix[i - 1][j - 1])
                            {
                                direction[i][j] |= DIR_DIAGONAL;
                            }
                        }
                        else
                        {
                            matrix[i][j] = Math.Min(Math.Min(matrix[i - 1][j] + DIS_DEL_INS, matrix[i][j - 1] + DIS_DEL_INS), matrix[i - 1][j - 1] + DIS_SUB);
                            if (matrix[i][j] == matrix[i - 1][j] + DIS_DEL_INS)
                            {
                                direction[i][j] |= DIR_DOWN;
                            }
                            if (matrix[i][j] == matrix[i][j - 1] + DIS_DEL_INS)
                            {
                                direction[i][j] |= DIR_LEFT;
                            }
                            if (matrix[i][j] == matrix[i - 1][j - 1] + DIS_SUB)
                            {
                                direction[i][j] |= DIR_DIAGONAL;
                            }
                        }
                    }
                }
            }

            return matrix[rows - 1][cols - 1];

        }

        static bool isEqual(string regex, string input)
        {

            Regex reg = new Regex(regex);
            if (reg.IsMatch(input)) { return true; }
            //if (first == second) { return true; }
            //if (first == @"\d" && second.All(char.IsDigit)) { return true; }
            //if (first == @"[a-zA-Z]" && second.All(char.IsLetter)) { return true; }

            return false;
        }
        /// <summary>
        /// Segmentize the first input string, consecutive numbers or letters will be in the same element of any output array
        /// </summary>
        /// <param name="input">first input string</param>
        /// <returns>array of segmentation strings</returns>
        static List<string> SegmentizeByGroup(string input)
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

            return segments;
        }

        /// <summary>
        /// Segmentize the second input string, simply cut each character to an element
        /// </summary>
        /// <param name="input">second input string</param>
        /// <returns>array of segmentation strings</returns>
        static List<string> SegmentizeByCharacter(string input)
        {
            List<string> list = new List<string>();

            foreach (char ch in input)
            {
                list.Add(ch.ToString());
            }

            return list;
        }

        /// <summary>
        /// Give regex suggestions based on segmentation results of first string
        /// </summary>
        /// <param name="segments">array of segmentation results of first string</param>
        /// <param name="index">Recursion parameter</param>
        /// <param name="curRegex">Recursion parameter</param>
        /// <param name="suggestions">results</param>
        static void SingleStringSuggestions(List<string> segments, int index, List<string> curRegex, List<List<string>> suggestions)
        {
            // do recursion to each segment
            // add its regex representation
            // or original character to curRegex
            // then Recur Permutation
            // if there is no more segments, put curRegex to our final Results

            if (index >= segments.Count)
            {
                List<string> result = new List<string>(curRegex);
                suggestions.Add(result);
                return;
            }

            List<string> curSeg =  new List<string>();
            List<string> segRegex = new List<string>();
            foreach (var ch in segments[index])
            {
                curSeg.Add(ConvertSpecial(ch.ToString()));
                segRegex.Add(ConvertNumLetter(ch.ToString()));
            }


            curRegex.AddRange(curSeg);
            SingleStringSuggestions(segments, index + 1, curRegex, suggestions);
            curRegex.RemoveRange(curRegex.Count - curSeg.Count, curSeg.Count);

            if (curSeg.First().All(char.IsLetterOrDigit))
            {
                curRegex.AddRange(segRegex);
                SingleStringSuggestions(segments, index + 1, curRegex, suggestions);
                curRegex.RemoveRange(curRegex.Count - segRegex.Count, segRegex.Count);
            }

            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static string ConvertNumLetter(string input)
        {
            StringBuilder regex = new StringBuilder();

            foreach (var ch in input)
            {
                string cur = ch.ToString();
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

        /// <summary>
        /// if it is a special character, add backslash in fornt of the character, or do nothing
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static string ConvertSpecial(string input)
        {
            const string specialCharacters = @"[\/^$.|?*+(){}";
            if (input.Count() == 1 && specialCharacters.Contains(input))
            {
                return  @"\" + input.ToString();
            }

            return input;
        }

        /// <summary>
        /// Modify the regex so input string can also be represented by output regex list
        /// </summary>
        /// <param name="regex"> a regular expression, which is also a parameter in GetEditDistance Method</param>
        /// <param name="input"> normal input without being regexed, which is also a parameter in GetEditDistance Method</param>
        /// <param name="direction">direction matrix created from GetEditDistance Method</param>
        /// <param name="results"> the results regex</param>
        static void ModifySuggestion(List<string> regex, List<string> input, List<List<int>> direction, out List<List<string>> results)
        {
            int rows = regex.Count + 1;
            int cols = input.Count + 1;
            results = new List<List<string>>();
            if (direction.Count != rows) { return; }
            if (direction[0].Count != cols) { return; }
            Queue<Tuple<int, int, List<string>>> queue = new Queue<Tuple<int, int, List<string>>>();
            queue.Enqueue(new Tuple<int, int, List<string>>(rows - 1, cols - 1, new List<string>())); // trace back from the last direction
            while (queue.Count != 0)
            {
                int size = queue.Count;
                for (int i = 0; i < size; ++i)
                {
                    var pos = queue.Dequeue();
                    int x = pos.Item1;
                    int y = pos.Item2;
                    List<string> curStr = pos.Item3;
                    int locDir = direction[x][y];
                    // trace back ends
                    // add results to list
                    if (x == 0 && y == 0)
                    {
                        results.Add(curStr);
                        break;
                    }

                    string curRegex = string.Empty;
                    string curString = string.Empty;
                    if (x > 0) { curRegex = ConvertSpecial(regex[x - 1]); }
                    if (y > 0) { curString = ConvertSpecial(input[y - 1]); }

                    if ((locDir & DIR_LEFT) != 0) // current second string character is optional in regex
                    {
                        if (curString.All(char.IsLetterOrDigit))
                        {
                            queue.Enqueue(x, y - 1, "(" + ConvertNumLetter(curString) + ")?", curStr);
                        }

                        queue.Enqueue(x, y - 1, "(" + curString + ")?", curStr);
                    }
                    if ((locDir & DIR_DOWN) != 0) // current first string character is optional in regex
                    {
                        queue.Enqueue(x - 1, y, "(" + curRegex + ")?", curStr);
                    }
                    if ((locDir & DIR_DIAGONAL) != 0) // second or first string is the same or can be replaced
                    {
                        if (isEqual(curRegex, curString))
                        {
                            queue.Enqueue(x - 1, y - 1, curRegex, curStr);
                        }
                        else
                        {
                            // if the regex segment is not like "[ab]"
                            if (!curRegex.First().Equals("[") || !curRegex.Last().Equals("]"))
                            {
                                if (curString.All(char.IsLetterOrDigit))
                                {
                                    queue.Enqueue(x - 1, y - 1, @"[" + curRegex + ConvertNumLetter(curString) + "]", curStr);
                                }

                                queue.Enqueue(x - 1, y - 1, @"[" + curRegex + curString + "]", curStr);
                            }
                            else
                            {
                                if (curString.All(char.IsLetterOrDigit))
                                {
                                    queue.Enqueue(x - 1, y - 1, @"[" + curRegex.Substring(1, curRegex.Length - 2) + ConvertNumLetter(curString) + "]", curStr);
                                }

                                queue.Enqueue(x - 1, y - 1, @"[" + curRegex.Substring(1, curRegex.Length - 2) + curString + "]", curStr);
                            }
                        }
                    }
                }
            }
        }

    }

    public static class ExtensionMethod
    {
        public static void Enqueue(this Queue<Tuple<int, int, List<string>>> queue, int x, int y, string addedStr, List<string> curStr)
        {
            List<string> added = new List<string>();
            added.Add(addedStr);
            added.AddRange(curStr);
            queue.Enqueue(new Tuple<int, int, List<string>>(x, y, added));

        }
    }
}
