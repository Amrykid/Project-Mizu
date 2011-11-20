// Generated by TinyPG v1.2 available at www.codeproject.com

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mizu3.Parser
{
    #region Scanner

    public partial class Scanner
    {
        public string Input;
        public int StartPos = 0;
        public int EndPos = 0;
        public int CurrentLine;
        public int CurrentColumn;
        public int CurrentPosition;
        public List<Token> Skipped; // tokens that were skipped
        public Dictionary<TokenType, Regex> Patterns;

        private Token LookAheadToken;
        private List<TokenType> Tokens;
        private List<TokenType> SkipList; // tokens to be skipped

        public Scanner()
        {
            Regex regex;
            Patterns = new Dictionary<TokenType, Regex>();
            Tokens = new List<TokenType>();
            LookAheadToken = null;

            SkipList = new List<TokenType>();
            SkipList.Add(TokenType.EMPTYLINE);
            SkipList.Add(TokenType.WHITESPACE);
            SkipList.Add(TokenType.TAB);
            SkipList.Add(TokenType.NEWLINE);
            SkipList.Add(TokenType.COMMENTBLOCK);

            regex = new Regex(@"(?!(itr|fun|let|new|import|from))[a-zA-Z][a-zA-Z0-9_]*", RegexOptions.Compiled);
            Patterns.Add(TokenType.IDENTIFIER, regex);
            Tokens.Add(TokenType.IDENTIFIER);

            regex = new Regex(@"\b[a-zA-Z0-9_]*((\.[a-zA-Z0-9_]*))*\b", RegexOptions.Compiled);
            Patterns.Add(TokenType.TYPE, regex);
            Tokens.Add(TokenType.TYPE);

            regex = new Regex(@"let", RegexOptions.Compiled);
            Patterns.Add(TokenType.LET, regex);
            Tokens.Add(TokenType.LET);

            regex = new Regex(@"itr", RegexOptions.Compiled);
            Patterns.Add(TokenType.ITER, regex);
            Tokens.Add(TokenType.ITER);

            regex = new Regex(@"\"".+?\""", RegexOptions.Compiled);
            Patterns.Add(TokenType.STRING, regex);
            Tokens.Add(TokenType.STRING);

            regex = new Regex(@"(\r\n|\n)\s(?=\S)", RegexOptions.Compiled);
            Patterns.Add(TokenType.EMPTYLINE, regex);
            Tokens.Add(TokenType.EMPTYLINE);

            regex = new Regex(@"\s+", RegexOptions.Compiled);
            Patterns.Add(TokenType.WHITESPACE, regex);
            Tokens.Add(TokenType.WHITESPACE);

            regex = new Regex(@"\t+", RegexOptions.Compiled);
            Patterns.Add(TokenType.TAB, regex);
            Tokens.Add(TokenType.TAB);

            regex = new Regex(@"(\r\n|\n)+", RegexOptions.Compiled);
            Patterns.Add(TokenType.NEWLINE, regex);
            Tokens.Add(TokenType.NEWLINE);

            regex = new Regex(@"/\*[^*]*\*+(?:[^/*][^*]*\*+)*/", RegexOptions.Compiled);
            Patterns.Add(TokenType.COMMENTBLOCK, regex);
            Tokens.Add(TokenType.COMMENTBLOCK);

            regex = new Regex(@"^\s*$", RegexOptions.Compiled);
            Patterns.Add(TokenType.EOF, regex);
            Tokens.Add(TokenType.EOF);

            regex = new Regex(@"\=", RegexOptions.Compiled);
            Patterns.Add(TokenType.EQUAL, regex);
            Tokens.Add(TokenType.EQUAL);

            regex = new Regex(@"\+", RegexOptions.Compiled);
            Patterns.Add(TokenType.PLUS, regex);
            Tokens.Add(TokenType.PLUS);

            regex = new Regex(@"\-", RegexOptions.Compiled);
            Patterns.Add(TokenType.MINUS, regex);
            Tokens.Add(TokenType.MINUS);

            regex = new Regex(@"\*", RegexOptions.Compiled);
            Patterns.Add(TokenType.MULTI, regex);
            Tokens.Add(TokenType.MULTI);

            regex = new Regex(@"\/", RegexOptions.Compiled);
            Patterns.Add(TokenType.DIV, regex);
            Tokens.Add(TokenType.DIV);

            regex = new Regex(@"\;", RegexOptions.Compiled);
            Patterns.Add(TokenType.SEMICOLON, regex);
            Tokens.Add(TokenType.SEMICOLON);

            regex = new Regex(@"(\-)?[0-9]+", RegexOptions.Compiled);
            Patterns.Add(TokenType.NUMBER, regex);
            Tokens.Add(TokenType.NUMBER);

            regex = new Regex(@"(\-)?[0-9]+\.[0-9]+", RegexOptions.Compiled);
            Patterns.Add(TokenType.FLOAT, regex);
            Tokens.Add(TokenType.FLOAT);

            regex = new Regex(@"\-\>", RegexOptions.Compiled);
            Patterns.Add(TokenType.ARROW, regex);
            Tokens.Add(TokenType.ARROW);

            regex = new Regex(@"\{", RegexOptions.Compiled);
            Patterns.Add(TokenType.BRCKOPEN, regex);
            Tokens.Add(TokenType.BRCKOPEN);

            regex = new Regex(@"\}", RegexOptions.Compiled);
            Patterns.Add(TokenType.BRCKCLOSE, regex);
            Tokens.Add(TokenType.BRCKCLOSE);

            regex = new Regex(@"\(", RegexOptions.Compiled);
            Patterns.Add(TokenType.BROPEN, regex);
            Tokens.Add(TokenType.BROPEN);

            regex = new Regex(@"\)", RegexOptions.Compiled);
            Patterns.Add(TokenType.BRCLOSE, regex);
            Tokens.Add(TokenType.BRCLOSE);

            regex = new Regex(@"\,", RegexOptions.Compiled);
            Patterns.Add(TokenType.COMMA, regex);
            Tokens.Add(TokenType.COMMA);

            regex = new Regex(@"fun", RegexOptions.Compiled);
            Patterns.Add(TokenType.FUNC, regex);
            Tokens.Add(TokenType.FUNC);

            regex = new Regex(@"\:", RegexOptions.Compiled);
            Patterns.Add(TokenType.COLON, regex);
            Tokens.Add(TokenType.COLON);

            regex = new Regex(@"\[", RegexOptions.Compiled);
            Patterns.Add(TokenType.OPENBR, regex);
            Tokens.Add(TokenType.OPENBR);

            regex = new Regex(@"\]", RegexOptions.Compiled);
            Patterns.Add(TokenType.CLOSEBR, regex);
            Tokens.Add(TokenType.CLOSEBR);

            regex = new Regex(@"ret", RegexOptions.Compiled);
            Patterns.Add(TokenType.RETURN, regex);
            Tokens.Add(TokenType.RETURN);

            regex = new Regex(@"new", RegexOptions.Compiled);
            Patterns.Add(TokenType.NEW, regex);
            Tokens.Add(TokenType.NEW);

            regex = new Regex(@"import", RegexOptions.Compiled);
            Patterns.Add(TokenType.IMPORT, regex);
            Tokens.Add(TokenType.IMPORT);

            regex = new Regex(@"from", RegexOptions.Compiled);
            Patterns.Add(TokenType.FROM, regex);
            Tokens.Add(TokenType.FROM);

            regex = new Regex(@"out", RegexOptions.Compiled);
            Patterns.Add(TokenType.OUT, regex);
            Tokens.Add(TokenType.OUT);


        }

        public void Init(string input)
        {
            this.Input = input;
            StartPos = 0;
            EndPos = 0;
            CurrentLine = 0;
            CurrentColumn = 0;
            CurrentPosition = 0;
            Skipped = new List<Token>();
            LookAheadToken = null;
        }

        public Token GetToken(TokenType type)
        {
            Token t = new Token(this.StartPos, this.EndPos);
            t.Type = type;
            return t;
        }

         /// <summary>
        /// executes a lookahead of the next token
        /// and will advance the scan on the input string
        /// </summary>
        /// <returns></returns>
        public Token Scan(params TokenType[] expectedtokens)
        {
            Token tok = LookAhead(expectedtokens); // temporarely retrieve the lookahead
            LookAheadToken = null; // reset lookahead token, so scanning will continue
            StartPos = tok.EndPos;
            EndPos = tok.EndPos; // set the tokenizer to the new scan position
            return tok;
        }

        /// <summary>
        /// returns token with longest best match
        /// </summary>
        /// <returns></returns>
        public Token LookAhead(params TokenType[] expectedtokens)
        {
            int i;
            int startpos = StartPos;
            Token tok = null;
            List<TokenType> scantokens;


            // this prevents double scanning and matching
            // increased performance
            if (LookAheadToken != null 
                && LookAheadToken.Type != TokenType._UNDETERMINED_ 
                && LookAheadToken.Type != TokenType._NONE_) return LookAheadToken;

            // if no scantokens specified, then scan for all of them (= backward compatible)
            if (expectedtokens.Length == 0)
                scantokens = Tokens;
            else
            {
                scantokens = new List<TokenType>(expectedtokens);
                scantokens.AddRange(SkipList);
            }

            do
            {

                int len = -1;
                TokenType index = (TokenType)int.MaxValue;
                string input = Input.Substring(startpos);

                tok = new Token(startpos, EndPos);

                for (i = 0; i < scantokens.Count; i++)
                {
                    Regex r = Patterns[scantokens[i]];
                    Match m = r.Match(input);
                    if (m.Success && m.Index == 0 && ((m.Length > len) || (scantokens[i] < index && m.Length == len )))
                    {
                        len = m.Length;
                        index = scantokens[i];  
                    }
                }

                if (index >= 0 && len >= 0)
                {
                    tok.EndPos = startpos + len;
                    tok.Text = Input.Substring(tok.StartPos, len);
                    tok.Type = index;
                }
                else
                {
                    if (tok.StartPos < tok.EndPos - 1)
                        tok.Text = Input.Substring(tok.StartPos, 1);
                }

                if (SkipList.Contains(tok.Type))
                {
                    startpos = tok.EndPos;
                    Skipped.Add(tok);
                }
            }
            while (SkipList.Contains(tok.Type));

            LookAheadToken = tok;
            return tok;
        }
    }

    #endregion

    #region Token

    public enum TokenType
    {

            //Non terminal tokens:
            _NONE_  = 0,
            _UNDETERMINED_= 1,

            //Non terminal tokens:
            Start   = 2,
            Statements= 3,
            Statement= 4,
            ImportStatement= 5,
            ReAssignmentStatement= 6,
            LetStatement= 7,
            IterStatement= 8,
            FuncStatement= 9,
            TypeFuncDeclartion= 10,
            RetStatement= 11,
            OutStatement= 12,
            Argument= 13,
            FuncCall= 14,
            ArrayIndexExpr= 15,
            ObjectCreatetion= 16,
            MathExpr= 17,
            OPERATOR= 18,

            //Terminal tokens:
            IDENTIFIER= 19,
            TYPE    = 20,
            LET     = 21,
            ITER    = 22,
            STRING  = 23,
            EMPTYLINE= 24,
            WHITESPACE= 25,
            TAB     = 26,
            NEWLINE = 27,
            COMMENTBLOCK= 28,
            EOF     = 29,
            EQUAL   = 30,
            PLUS    = 31,
            MINUS   = 32,
            MULTI   = 33,
            DIV     = 34,
            SEMICOLON= 35,
            NUMBER  = 36,
            FLOAT   = 37,
            ARROW   = 38,
            BRCKOPEN= 39,
            BRCKCLOSE= 40,
            BROPEN  = 41,
            BRCLOSE = 42,
            COMMA   = 43,
            FUNC    = 44,
            COLON   = 45,
            OPENBR  = 46,
            CLOSEBR = 47,
            RETURN  = 48,
            NEW     = 49,
            IMPORT  = 50,
            FROM    = 51,
            OUT     = 52
    }

    public class Token
    {
        private int startpos;
        private int endpos;
        private string text;
        private object value;

        public int StartPos { 
            get { return startpos;} 
            set { startpos = value; }
        }

        public int Length { 
            get { return endpos - startpos;} 
        }

        public int EndPos { 
            get { return endpos;} 
            set { endpos = value; }
        }

        public string Text { 
            get { return text;} 
            set { text = value; }
        }

        public object Value { 
            get { return value;} 
            set { this.value = value; }
        }

        public TokenType Type;

        public Token()
            : this(0, 0)
        {
        }

        public Token(int start, int end)
        {
            Type = TokenType._UNDETERMINED_;
            startpos = start;
            endpos = end;
            Text = ""; // must initialize with empty string, may cause null reference exceptions otherwise
            Value = null;
        }

        public void UpdateRange(Token token)
        {
            if (token.StartPos < startpos) startpos = token.StartPos;
            if (token.EndPos > endpos) endpos = token.EndPos;
        }

        public override string ToString()
        {
            if (Text != null)
                return Type.ToString() + " '" + Text + "'";
            else
                return Type.ToString();
        }
    }

    #endregion
}
