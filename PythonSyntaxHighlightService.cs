using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraRichEdit.Services;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RichEditSyntax
{
    public class PythonSyntaxHighlightService : ISyntaxHighlightService
    {
        #region #parsetokens
        readonly Document document;
        SyntaxHighlightProperties defaultSettings = new SyntaxHighlightProperties() { ForeColor = Color.Black };
        SyntaxHighlightProperties keywordSettings = new SyntaxHighlightProperties() { ForeColor = Color.Blue };
        SyntaxHighlightProperties stringSettings = new SyntaxHighlightProperties() { ForeColor = Color.Green };

        string[] keywords = new string[] {
            "ArithmeticError", "AssertionError", "AttributeError", "BaseException", "BufferError", "BytesWarning",
            "DeprecationWarning", "EOFError", "Ellipsis", "EnvironmentError", "Exception", "False", "FloatingPointError",
            "FutureWarning", "GeneratorExit", "IOError", "ImportError", "ImportWarning", "IndentationError",
            "IndexError", "KeyError", "KeyboardInterrupt", "LookupError", "MemoryError", "NameError", "None",
            "NotImplemented", "NotImplementedError", "OSError", "OverflowError", "PendingDeprecationWarning",
            "ReferenceError", "RuntimeError", "RuntimeWarning", "StandardError", "StopIteration", "SyntaxError",
            "SyntaxWarning", "SystemError", "SystemExit", "TabError", "True", "TypeError", "UnboundLocalError",
            "UnicodeDecodeError", "UnicodeEncodeError", "UnicodeError", "UnicodeTranslateError", "UnicodeWarning",
            "UserWarning", "ValueError", "Warning", "WindowsError", "ZeroDivisionError", "__debug__", "__doc__",
            "__import__", "__name__", "__package__", "abs", "all", "any", "apply", "basestring", "bin", "bool", "buffer",
            "bytearray", "bytes", "callable", "chr", "classmethod", "cmp", "coerce", "compile", "complex", "copyright",
            "credits", "delattr", "dict", "dir", "divmod", "enumerate", "eval", "execfile", "exit", "file", "filter",
            "float", "format", "frozenset", "getattr", "globals", "hasattr", "hash", "help", "hex", "id", "input", "int",
            "intern", "isinstance", "issubclass", "iter", "len", "license", "list", "locals", "long", "map", "max",
            "memoryview", "min", "next", "object", "oct", "open", "ord", "pow", "print", "property", "quit", "range",
            "raw_input", "reduce", "reload", "repr", "reversed", "round", "set", "setattr", "slice", "sorted",
            "staticmethod", "str", "sum", "super", "tuple", "type", "unichr", "unicode", "vars", "xrange", "zip",

            "and", "as", "assert", "break", "class", "continue", "def", "del", "elif", "else", "except", "exec",
            "finally", "for", "from", "global", "if", "import", "in", "is", "lambda", "not", "or", "pass", "print",
            "raise", "return", "try", "while", "with", "yield"
        };

        public PythonSyntaxHighlightService(Document document)
        {
            this.document = document;
        }

        private List<SyntaxHighlightToken> ParseTokens()
        {
            List<SyntaxHighlightToken> tokens = new List<SyntaxHighlightToken>();
            DocumentRange[] ranges = null;
            // search for quotation marks
            ranges = document.FindAll("\"", SearchOptions.None);
            for (int i = 0; i < ranges.Length / 2; i++)
            {
                tokens.Add(new SyntaxHighlightToken(ranges[i * 2].Start.ToInt(),
                    ranges[i * 2 + 1].Start.ToInt() - ranges[i * 2].Start.ToInt() + 1, stringSettings));
            }
            // search for keywords
            for (int i = 0; i < keywords.Length; i++)
            {
                ranges = document.FindAll(keywords[i], SearchOptions.CaseSensitive | SearchOptions.WholeWord);

                for (int j = 0; j < ranges.Length; j++)
                {
                    if (!IsRangeInTokens(ranges[j], tokens))
                        tokens.Add(new SyntaxHighlightToken(ranges[j].Start.ToInt(), ranges[j].Length, keywordSettings));
                }
            }
            // order tokens by their start position
            tokens.Sort(new SyntaxHighlightTokenComparer());
            // fill in gaps in document coverage
            AddPlainTextTokens(tokens);
            return tokens;
        }

        private void AddPlainTextTokens(List<SyntaxHighlightToken> tokens)
        {
            int count = tokens.Count;
            if (count == 0)
            {
                tokens.Add(new SyntaxHighlightToken(0, document.Range.End.ToInt(), defaultSettings));
                return;
            }
            tokens.Insert(0, new SyntaxHighlightToken(0, tokens[0].Start, defaultSettings));
            for (int i = 1; i < count; i++)
            {
                tokens.Insert(i * 2, new SyntaxHighlightToken(tokens[i * 2 - 1].End,
 tokens[i * 2].Start - tokens[i * 2 - 1].End, defaultSettings));
            }
            tokens.Add(new SyntaxHighlightToken(tokens[count * 2 - 1].End,
 document.Range.End.ToInt() - tokens[count * 2 - 1].End, defaultSettings));
        }

        private bool IsRangeInTokens(DocumentRange range, List<SyntaxHighlightToken> tokens)
        {
                return tokens.Any(t => IsIntersect(range, t));
        }
        bool IsIntersect(DocumentRange range, SyntaxHighlightToken token) {
            int start = range.Start.ToInt();
            if (start >= token.Start && start < token.End)
                return true;
            int end = range.End.ToInt() - 1;
            if (end >= token.Start && end < token.End)
                return true;
            return false;
        }
        #endregion #parsetokens

        #region #ISyntaxHighlightServiceMembers
        public void ForceExecute()
        {
            Execute();
        }
        public void Execute()
        {
            document.ApplySyntaxHighlight(ParseTokens());
        }
        #endregion #ISyntaxHighlightServiceMembers
    }

      #region #SyntaxHighlightTokenComparer
    public class SyntaxHighlightTokenComparer : IComparer<SyntaxHighlightToken> {
        public int Compare(SyntaxHighlightToken x, SyntaxHighlightToken y) {
            return x.Start - y.Start;
        }
    }
    #endregion #SyntaxHighlightTokenComparer
}

