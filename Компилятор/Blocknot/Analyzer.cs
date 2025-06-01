using System;
using System.Collections.Generic;
using System.Text;

namespace Blocknot
{
    public class Analyzer
    {
        private static readonly HashSet<string> keywords = new HashSet<string> { "enum", "case" };
        private static readonly HashSet<char> separators = new HashSet<char> { ' ', '\t' };
        private static readonly HashSet<char> validSymbols = new HashSet<char>(
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_".ToCharArray()
        );

        private const int MaxErrors = 100;

        public static string Analyze(string input)
        {
            StringBuilder output = new StringBuilder();
            int i = 0;
            int length = input.Length;
            int errorCount = 0;
            bool AddError(string message)
            {
                if (errorCount >= MaxErrors)
                    return false;
                output.AppendLine(message);
                errorCount++;
                return errorCount < MaxErrors;
            }
            bool success = true;
            // enum
            bool enumKeywordOk = ExpectKeyword(ref i, "enum", ref output, input, AddError);
            if (!enumKeywordOk)
            {
                success = false;
                // При ошибке в enum не пропускаем весь блок до '{',
                // а просто пытаемся дальше распарсить идентификатор
            }
            // идентификатор (имя перечисления)
            if (!ExpectIdentifier(ref i, ref output, input, "имя перечисления", AddError))
            {
                success = false;

                // При ошибке в идентификаторе можно пропустить до '{', 
                // чтобы не застрять на ошибках.
                SkipToNext(ref i, input, new char[] { '{' });
            }
            // открывающая скобка {
            if (!ExpectSymbol(ref i, '{', ref output, input, AddError))
            {
                success = false;
                // при ошибке пропускаем до '}' для продолжения парсинга
                SkipToNext(ref i, input, new char[] { '}' });
            }

            // поля case
            while (i < length)
            {
                SkipWhitespace(ref i, input);

                if (PeekChar(i, input) == '}')
                    break;

                bool caseKeywordOk = ExpectKeyword(ref i, "case", ref output, input, AddError);
                if (!caseKeywordOk)
                {
                    // Ошибка в ключевом слове "case", но не синхронизируем полностью,
                    // пытаемся дальше найти идентификатор и символ ';'
                    success = false;

                    // Попытка распарсить идентификатор после ошибочного ключевого слова
                    if (!ExpectIdentifier(ref i, ref output, input, "значение case", AddError))
                    {
                        // ошибка в идентификаторе — пропускаем до ';' или '}'
                        SkipToNext(ref i, input, new char[] { ';', '}' });
                    }

                    // Попытка распарсить символ ';'
                    if (!ExpectSymbol(ref i, ';', ref output, input, AddError))
                    {
                        // Если нет ';', пропустим до следующего 'case' или '}'
                        SkipToNext(ref i, input, new char[] { ';', '}' });
                        if (PeekChar(i, input) == ';') i++;
                    }

                    // Переходим к следующему элементу цикла
                    continue;
                }

                // Если case ключевое слово ok, пытаемся распарсить идентификатор
                if (!ExpectIdentifier(ref i, ref output, input, "значение case", AddError))
                {
                    success = false;

                    // Пропускаем до ';' или '}'
                    SkipToNext(ref i, input, new char[] { ';', '}' });
                    if (PeekChar(i, input) == ';') i++;

                    continue;
                }

                // И пытаемся распарсить символ ';'
                if (!ExpectSymbol(ref i, ';', ref output, input, AddError))
                {
                    success = false;

                    // Пропускаем до ';' или '}' для синхронизации
                    SkipToNext(ref i, input, new char[] { ';', '}' });
                    if (PeekChar(i, input) == ';') i++;
                }
            }


            // Закрывающая скобка }
            if (!ExpectSymbol(ref i, '}', ref output, input, AddError))
            {
                SkipToNext(ref i, input, new char[] { ';' });
                success = false;
            }

            // Точка с запятой ;
            if (!ExpectSymbol(ref i, ';', ref output, input, AddError))
            {
                success = false;
            }

            if (errorCount >= MaxErrors)
            {
                output.AppendLine($"8\tERROR\t\"\" \t({i} - {i}) -> Достигнут лимит ошибок ({MaxErrors}). Разбор остановлен.");
            }

            return output.ToString();
        }

        private static bool ExpectKeyword(ref int pos, string keyword, ref StringBuilder output, string input, Func<string, bool> AddError)
        {
            SkipWhitespace(ref pos, input);

            int start = pos;
            while (pos < input.Length && char.IsLetter(input[pos]))
                pos++;

            string word = input.Substring(start, pos - start);

            // Проверяем на наличие недопустимых символов
            List<char> invalidChars = new List<char>();
            for (int i = 0; i < word.Length; i++)
            {
                char c = word[i];
                if (!char.IsLetter(c)) // Проверяем, является ли символ буквой
                {
                    invalidChars.Add(c);
                }
            }

            // Если есть недопустимые символы, выводим ошибку
            if (invalidChars.Count > 0)
            {
                foreach (char invalidChar in invalidChars)
                {
                    int errPos = start + word.IndexOf(invalidChar);
                    if (!AddError($"8\tERROR\t\"{invalidChar}\"\t({errPos} - {errPos}) -> Недопустимый символ в ключевом слове '{keyword}'."))
                        return false;
                }
                return false; // Возвращаем false, так как слово недопустимо
            }

            // Если слово не совпадает с ожидаемым ключевым словом
            if (word != keyword)
            {
                if (!AddError($"8\tERROR\t\"{word}\"\t({start} - {pos - 1}) -> Ожидается ключевое слово '{keyword}'"))
                    return false;
                return false;
            }

            output.AppendLine($"2\tключевое слово\t\"{word}\"\t({start} - {pos - 1})");
            return true;
        }



        private static bool ExpectIdentifier(ref int pos, ref StringBuilder output, string input, string expected, Func<string, bool> AddError)
        {
            SkipWhitespace(ref pos, input);

            int start = pos;
            while (pos < input.Length && validSymbols.Contains(input[pos]))
                pos++;

            if (start == pos)
            {
                string errChar = (start < input.Length) ? input[start].ToString() : "";
                if (!AddError($"8\tERROR\t\"{errChar}\"\t({start} - {start}) -> Ожидается {expected}"))
                    return false;
                return false;
            }

            string identifier = input.Substring(start, pos - start);
            if (!IsValidIdentifier(identifier))
            {
                if (!AddError($"8\tERROR\t\"{identifier}\"\t({start} - {pos - 1}) -> Недопустимый идентификатор."))
                    return false;
                return false;
            }

            output.AppendLine($"3\tидентификатор\t\"{identifier}\"\t({start} - {pos - 1})");
            return true;
        }

        private static bool ExpectSymbol(ref int pos, char symbol, ref StringBuilder output, string input, Func<string, bool> AddError)
        {
            SkipWhitespace(ref pos, input);

            if (pos >= input.Length || input[pos] != symbol)
            {
                string errChar = (pos < input.Length) ? input[pos].ToString() : "";
                if (!AddError($"8\tERROR\t\"{errChar}\"\t({pos} - {pos}) -> Ожидается символ '{symbol}'"))
                    return false;
                return false;
            }

            output.AppendLine($"5\tсимвол\t\"{symbol}\"\t({pos} - {pos})");
            pos++;
            return true;
        }

        private static void SkipWhitespace(ref int pos, string input)
        {
            while (pos < input.Length && (separators.Contains(input[pos]) || input[pos] == '\n' || input[pos] == '\r'))
                pos++;
        }

        private static char PeekChar(int pos, string input)
        {
            return pos < input.Length ? input[pos] : '\0';
        }

        private static bool IsValidIdentifier(string identifier)
        {
            return !string.IsNullOrEmpty(identifier) && char.IsLetter(identifier[0]);
        }

        private static void SkipToNext(ref int pos, string input, char[] syncSymbols)
        {
            while (pos < input.Length && Array.IndexOf(syncSymbols, input[pos]) == -1)
                pos++;
        }
    }
}
