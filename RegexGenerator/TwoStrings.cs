using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        const int DIS_DEL_INS = 2;
        const int DIS_REP = 1;
        public static void Test()
        {
            //List<string> A = new List<string>(new string[]{ @"\d", @"\d", @"\d", "-", @"\d", @"\d", @"\d" });
            //List<string> B = new List<string>(new string[] { @"(", @"2", @"1", @"3", @")", @"3", @"0", @"4" });

            // do permutation and get all the results
            //List<string> curResults = new List<string>();
            //List<List<string>> finalResults = new List<List<string>>();
            //Permutation(Cut(A), 0, curResults, finalResults);

            //int index = 0;
            //List<List<int>> direction;
            //int test = GetEditDistance(A, B, out direction);
            //List<string> results;
            //ModifyRegex(A, B, direction, out results);
            //while (index < finalResults.Count)
            //{
            //    Console.WriteLine("--{0}", finalResults[index]);
            //    if (finalShortResults[index] != finalResults[index])
            //    {
            //        Console.WriteLine("  {0}", finalShortResults[index]);
            //    }
            //    ++index;
            //}

            // finalResults is shown here

            List<string> results = new List<string>();
            GetTwoStringsRegex("213-304", "(213)555", results);
            return;
        }

        public static void GetTwoStringsRegex(string A, string B, List<string> results)
        {
            // do permutation and get all the results
            List<string> curRegex = new List<string>();
            List<List<string>> regexPerm = new List<List<string>>();
            Permutation(Cut(A), 0, curRegex, regexPerm);// the permutation don't add "/" to the beginning of special characters

            List<List<string>> candidates = new List<List<string>>();
            int distance = Int32.MaxValue;
            List<string> segB = Segmentize(B);
            foreach (var regex in regexPerm)
            {
                List<List<int>> direction;
                int curDistance = GetEditDistance(regex, segB, out direction);
                if (curDistance < distance)
                {
                    distance = curDistance;
                    results.Clear();
                }

                if (distance == curDistance)
                {
                    List<string> curResults;
                    ModifyRegex(regex, segB, direction, out curResults);
                    results.AddRange(curResults);
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
                            matrix[i][j] = Math.Min(Math.Min(matrix[i - 1][j] + DIS_DEL_INS, matrix[i][j - 1] + DIS_DEL_INS), matrix[i - 1][j - 1] + DIS_REP);
                            if (matrix[i][j] == matrix[i - 1][j] + DIS_DEL_INS)
                            {
                                direction[i][j] |= DIR_DOWN;
                            }
                            if (matrix[i][j] == matrix[i][j - 1] + DIS_DEL_INS)
                            {
                                direction[i][j] |= DIR_LEFT;
                            }
                            if (matrix[i][j] == matrix[i - 1][j - 1] + DIS_REP)
                            {
                                direction[i][j] |= DIR_DIAGONAL;
                            }
                        }
                    }
                }
            }

            return matrix[rows - 1][cols - 1];

        }

        static bool isEqual(string first, string second)
        {
            if (first == second) { return true; }
            if (first == @"\d" && second.All(char.IsDigit)) { return true; }
            if (first == @"[a-zA-Z]" && second.All(char.IsLetter)) { return true; }

            return false;
        }
        static List<string> Cut(string input)
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

        static List<string> Segmentize(string input)
        {
            List<string> list = new List<string>();

            foreach (char ch in input)
            {
                list.Add(ch.ToString());
            }

            return list;
        }

        static void Permutation(List<string> segments, int index, List<string> curRegex, List<List<string>> finalResults)
        {
            // use recursion
            // to each segment
            // add its reges representation
            // or original character to curRegex
            // then Recur Permutation
            // if there is no more segments, put curRegex to our final Results

            if (index >= segments.Count)
            {
                List<string> result = new List<string>(curRegex);
                finalResults.Add(result);
                return;
            }

            List<string> curSeg =  new List<string>();
            List<string> segRegex = new List<string>();
            foreach (var ch in segments[index])
            {
                curSeg.Add(ch.ToString());
                segRegex.Add(ConvertNumLetter(ch.ToString()));
            }


            curRegex.AddRange(curSeg);
            Permutation(segments, index + 1, curRegex, finalResults);
            curRegex.RemoveRange(curRegex.Count - curSeg.Count, curSeg.Count);

            if (curSeg.First().All(char.IsLetterOrDigit))
            {
                curRegex.AddRange(segRegex);
                Permutation(segments, index + 1, curRegex, finalResults);
                curRegex.RemoveRange(curRegex.Count - segRegex.Count, segRegex.Count);
            }

            return;
        }

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
            if (input.Count() == 1 && input.Contains(specialCharacters))
            {
                return  @"\" + input.ToString();
            }

            return input;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"> first is an regular expression</param>
        /// <param name="second">regular is a normal input without being regexed</param>
        /// <param name="direction">trace back direction matrix from dp results</param>
        /// <param name="results"> the results after we do trace back</param>
        static void ModifyRegex(List<string> first, List<string> second, List<List<int>> direction, out List<string> results)
        {
            int rows = first.Count + 1;
            int cols = second.Count + 1;
            results = new List<string>();
            if (direction.Count != rows) { return; }
            if (direction[0].Count != cols) { return; }
            Queue<Tuple<int, int, string>> queue = new Queue<Tuple<int, int, string>>();
            queue.Enqueue(new Tuple<int, int, string>(rows - 1, cols - 1, string.Empty)); // trace back from the last direction
            while (queue.Count != 0)
            {
                int size = queue.Count;
                for (int i = 0; i < size; ++i)
                {
                    var pos = queue.Dequeue();
                    int x = pos.Item1;
                    int y = pos.Item2;
                    string curStr = pos.Item3;
                    int locDir = direction[x][y];
                    // trace back ends
                    // add results to list
                    if (x == 0 && y == 0)
                    {
                        results.Add(curStr);
                        break;
                    }

                    string curFirst = string.Empty;
                    string curSecond = string.Empty;
                    if (x > 0) { curFirst = ConvertSpecial(first[x - 1]); }
                    if (y > 0) { curSecond = ConvertSpecial(second[y - 1]); }

                    if ((locDir & DIR_LEFT) != 0) // current second string character is optional in regex
                    {
                        queue.Enqueue(new Tuple<int, int, string>(x, y - 1, "(" + curSecond + ")?" + curStr));
                    }
                    if ((locDir & DIR_DOWN) != 0) // current first string character is optional in regex
                    {
                        queue.Enqueue(new Tuple<int, int, string>(x - 1, y, "(" + curFirst + ")?" + curStr));
                    }
                    if ((locDir & DIR_DIAGONAL) != 0) // second or first string is the same or can be replaced
                    {
                        if (isEqual(curFirst, curSecond))
                        {
                            queue.Enqueue(new Tuple<int, int, string>(x - 1, y - 1, curFirst + curStr));
                        }
                        else
                        {
                            queue.Enqueue(new Tuple<int, int, string>(x - 1, y - 1, "[" + curFirst + curSecond + "]" + curStr));
                        }
                    }
                }
            }

        }
    }
}
