using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace Metric
{
    class Program
    {

        static string code;

        static void deleteLiterals()
        {
            string regularExpressionLiteral = @"((\'.*\')|(//.*|{[^}]*}))";
            string regularExpressionElse = @"\s*\bend\b\s*\belse\b\s*\bbegin\b";
            string replaceString = "";
            code = Regex.Replace(code, regularExpressionLiteral, replaceString);
            code = Regex.Replace(code, regularExpressionElse, replaceString, RegexOptions.IgnoreCase);
        }

        static int ToCountIf()
        {
            int CountOfIf = 0;
            string RegularExpressionIf = @"\bif\b";
            foreach (Match Count in Regex.Matches(code, RegularExpressionIf, RegexOptions.IgnoreCase | RegexOptions.Singleline))
                CountOfIf++;
            return CountOfIf;
        }

        static int ToCountNesting(string text)
        {
            string[] linesOfCode = text.Split('\r');
            var stackForCounting = new Stack<string>();
            int maximalNesting = 0;
            int currentNesting = 0;

            for (int countLines = 0; countLines < linesOfCode.Length; countLines++)
            {
                string[] wordsOfLine = linesOfCode[countLines].Split(new Char[] { ' ', '\n', '\r', '\t' });
                for (int arrayCount = 0; arrayCount < wordsOfLine.Length; arrayCount++)
                {
                    if (wordsOfLine[arrayCount] == "if" | wordsOfLine[arrayCount] == "for" | wordsOfLine[arrayCount] == "while" | wordsOfLine[arrayCount] == "repeat" | wordsOfLine[arrayCount] == "else")
                    {
                        if (wordsOfLine[arrayCount] == "if") currentNesting++;
                        stackForCounting.Push(wordsOfLine[arrayCount]);
                    }
                    if (stackForCounting.Count != 0)
                    {
                        if (wordsOfLine[arrayCount] == "end;" & (stackForCounting.Peek() == "if"))
                        {
                            stackForCounting.Pop();
                            currentNesting--;
                        }
                        else
                            if (wordsOfLine[arrayCount] == "end;" & (stackForCounting.Peek() != "if"))
                                stackForCounting.Pop();

                        if (wordsOfLine[arrayCount] == "end")
                            for (int stackElements = stackForCounting.Count; stackElements > 1; stackElements--)
                            {
                                if (stackForCounting.Peek() == "if") currentNesting--;
                                stackForCounting.Pop();
                            }
                    }
                    if (currentNesting > maximalNesting)
                        maximalNesting = currentNesting;
                }
            }
            return maximalNesting;
        }

        static int deleteVariableDeclarations()
        {
            List<string> listWithTypes = new List<string>();

            string RegularExpressionIdentifier = @"[a-z_]\w*(?=\s*\:\s*|\s*,\s*)";
            string search = @"(\bvar\b|\btype\b|\bconst\b)(.*?)(?=\b(begin|var|type|const|procedure|function)\b)";
            string RegularExpressionType = @"(?<=\:\s)([a-z_]\w*)|([a-z_]\w*)(?=\s*\=)";
            string RegularExpressionConstant = @"(?<=const)\s*([a-z_]\w*)";
            string replace = "";

            bool flagForCycle = true;

            while (flagForCycle)
            {
                Match ToFindVarSection = Regex.Match(code, search, RegexOptions.Singleline | RegexOptions.IgnoreCase);

                string section = ToFindVarSection.Value;
                string[] split = section.Split(';');

                code = code.Replace(section, replace);
                for (int arrayCount = 0; arrayCount < split.Length; arrayCount++)
                {
                    Match identifier = Regex.Match(split[arrayCount], RegularExpressionIdentifier, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    Match type = Regex.Match(split[arrayCount], RegularExpressionType, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    Match constant = Regex.Match(split[arrayCount], RegularExpressionConstant, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    if (identifier.Value != "")
                        code = Regex.Replace(code, @"\b" + identifier.Value + @"\b", replace, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    if (type.Value != "")
                        listWithTypes.Add(type.Value);
                    if (constant.Value != "")
                        code = Regex.Replace(code, @"\b" + constant.Value + @"\b", replace, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                }

                int varCount = 0;

                foreach (Match endOfText in Regex.Matches(code, @"\b(var|type|const)\b", RegexOptions.Singleline | RegexOptions.IgnoreCase))
                    varCount++;
                if (varCount == 0)
                    flagForCycle = false;
            }
            return listWithTypes.Count;
        }

        static int ToDeleteKeywords()
        {
            string RegularExpressionKeyword = @"((\b(for|while|repeat|break|continue|if|case|goto|with|in)\b))";
            string RegularExpressionAdditionalKeyword = @"((\b(uses)\b.*?(?=\;))\b(do|until|begin|end|then|downto|to|of)\b|(\b(function|procedure)\b.*?(?=\;))|(\b(uses)\b.*?(?=\;)))";
            string replace = "";
            int keywordCount = 0;

            foreach (Match count in Regex.Matches(code, RegularExpressionKeyword, RegexOptions.Singleline | RegexOptions.IgnoreCase))
                keywordCount++;

            code = Regex.Replace(code, RegularExpressionKeyword, replace, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            code = Regex.Replace(code, RegularExpressionAdditionalKeyword, replace, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            return keywordCount;
        }

        static int ToCountSubroutineCall()
        {
            int CountCallSubroutine = 0;
            string RegularExpressionSubroutineCall = @"[a-z_]\w*(?=\()";

            foreach (Match count in Regex.Matches(code, RegularExpressionSubroutineCall, RegexOptions.Singleline | RegexOptions.IgnoreCase))
                CountCallSubroutine++;

            return CountCallSubroutine;
        }

        static int countingOperators()
        {
            int resultCount = 0;
            resultCount = deleteVariableDeclarations() + ToDeleteKeywords();
            string operatorRegularExpression = @"[\+\-\/\*@^]|\b(mod|or|xor|not|shl|shr|and|div)\b|(\.\w*)|(\:\=)";
            string replace = "";
            foreach (Match count in Regex.Matches(code, operatorRegularExpression, RegexOptions.Singleline | RegexOptions.IgnoreCase))
                resultCount++;
            code = Regex.Replace(code, operatorRegularExpression, replace, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            resultCount = resultCount + ToCountSubroutineCall();

            return resultCount;
        }

        static void Main(string[] args)
        {
            StreamReader fileWithCode = new StreamReader("d:\\code.txt");
            code = fileWithCode.ReadToEnd();
            fileWithCode.Close();

            deleteLiterals();

            int ifCount = ToCountIf();
            Console.Write("Количество условных операторов: ");
            Console.WriteLine(ifCount);

            int maximalNesting = ToCountNesting(code);
            Console.Write("Максимальная вложенность: ");
            Console.WriteLine(maximalNesting);

            int operatorCount = countingOperators();
            Console.Write("Число операторов: ");
            Console.WriteLine(operatorCount);
            
            float cl = (float)ifCount / operatorCount;
            Console.Write("Результат(cl): ");
            Console.WriteLine("{0}", cl);
            
            Console.ReadLine();
         }
    }
}
