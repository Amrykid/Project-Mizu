// Generated by TinyPG v1.3 available at www.codeproject.com

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Mizu.Parser
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
            Skipped = new List<Token>();

            SkipList = new List<TokenType>();
            SkipList.Add(TokenType.COMMENTBLOCK);

            regex = new Regex(@"(\-)?[0-9]+", RegexOptions.Compiled);
            Patterns.Add(TokenType.NUMBER, regex);
            Tokens.Add(TokenType.NUMBER);

            regex = new Regex(@"(\-)?[0-9]+\.[0-9]+", RegexOptions.Compiled);
            Patterns.Add(TokenType.FLOAT, regex);
            Tokens.Add(TokenType.FLOAT);

            regex = new Regex(@"(\+|-)", RegexOptions.Compiled);
            Patterns.Add(TokenType.PLUSMINUS, regex);
            Tokens.Add(TokenType.PLUSMINUS);

            regex = new Regex(@"\*|/", RegexOptions.Compiled);
            Patterns.Add(TokenType.MULTDIV, regex);
            Tokens.Add(TokenType.MULTDIV);

            regex = new Regex(@"\(", RegexOptions.Compiled);
            Patterns.Add(TokenType.BROPEN, regex);
            Tokens.Add(TokenType.BROPEN);

            regex = new Regex(@"\)", RegexOptions.Compiled);
            Patterns.Add(TokenType.BRCLOSE, regex);
            Tokens.Add(TokenType.BRCLOSE);

            regex = new Regex(@"^\s*$", RegexOptions.Compiled);
            Patterns.Add(TokenType.EOF, regex);
            Tokens.Add(TokenType.EOF);

            regex = new Regex(@"[a-zA-Z_][a-zA-Z0-9_]*", RegexOptions.Compiled);
            Patterns.Add(TokenType.IDENTIFIER, regex);
            Tokens.Add(TokenType.IDENTIFIER);

            regex = new Regex(@"\`", RegexOptions.Compiled);
            Patterns.Add(TokenType.SET, regex);
            Tokens.Add(TokenType.SET);

            regex = new Regex(@"\s+", RegexOptions.Compiled);
            Patterns.Add(TokenType.WHITESPACE, regex);
            Tokens.Add(TokenType.WHITESPACE);

            regex = new Regex(@"\[", RegexOptions.Compiled);
            Patterns.Add(TokenType.OPENBR, regex);
            Tokens.Add(TokenType.OPENBR);

            regex = new Regex(@"\]", RegexOptions.Compiled);
            Patterns.Add(TokenType.CLOSEBR, regex);
            Tokens.Add(TokenType.CLOSEBR);

            regex = new Regex(@"\.", RegexOptions.Compiled);
            Patterns.Add(TokenType.PERIOD, regex);
            Tokens.Add(TokenType.PERIOD);

            regex = new Regex(@"\|", RegexOptions.Compiled);
            Patterns.Add(TokenType.STATEMENTSEP, regex);
            Tokens.Add(TokenType.STATEMENTSEP);

            regex = new Regex(@"\?", RegexOptions.Compiled);
            Patterns.Add(TokenType.QUESTION, regex);
            Tokens.Add(TokenType.QUESTION);

            regex = new Regex(@"\:", RegexOptions.Compiled);
            Patterns.Add(TokenType.COLON, regex);
            Tokens.Add(TokenType.COLON);

            regex = new Regex(@"ABS", RegexOptions.Compiled);
            Patterns.Add(TokenType.COMMAND, regex);
            Tokens.Add(TokenType.COMMAND);

            regex = new Regex(@"\@", RegexOptions.Compiled);
            Patterns.Add(TokenType.ABS, regex);
            Tokens.Add(TokenType.ABS);

            regex = new Regex(@"\$", RegexOptions.Compiled);
            Patterns.Add(TokenType.SIN, regex);
            Tokens.Add(TokenType.SIN);

            regex = new Regex(@"\&", RegexOptions.Compiled);
            Patterns.Add(TokenType.COS, regex);
            Tokens.Add(TokenType.COS);

            regex = new Regex(@"\#", RegexOptions.Compiled);
            Patterns.Add(TokenType.TAN, regex);
            Tokens.Add(TokenType.TAN);

            regex = new Regex(@"\%", RegexOptions.Compiled);
            Patterns.Add(TokenType.SQRT, regex);
            Tokens.Add(TokenType.SQRT);

            regex = new Regex(@"\=", RegexOptions.Compiled);
            Patterns.Add(TokenType.EQUAL, regex);
            Tokens.Add(TokenType.EQUAL);

            regex = new Regex(@"\^", RegexOptions.Compiled);
            Patterns.Add(TokenType.UPPER, regex);
            Tokens.Add(TokenType.UPPER);

            regex = new Regex(@"(\r\n|\n)", RegexOptions.Compiled);
            Patterns.Add(TokenType.NEWLINE, regex);
            Tokens.Add(TokenType.NEWLINE);

            regex = new Regex(@"(\r\n|\n)+", RegexOptions.Compiled);
            Patterns.Add(TokenType.MULTINEWLINE, regex);
            Tokens.Add(TokenType.MULTINEWLINE);

            regex = new Regex(@"\~", RegexOptions.Compiled);
            Patterns.Add(TokenType.WAVEY, regex);
            Tokens.Add(TokenType.WAVEY);

            regex = new Regex(@"\"".+?\""", RegexOptions.Compiled);
            Patterns.Add(TokenType.STRING, regex);
            Tokens.Add(TokenType.STRING);

            regex = new Regex(@"/\*[^*]*\*+(?:[^/*][^*]*\*+)*/", RegexOptions.Compiled);
            Patterns.Add(TokenType.COMMENTBLOCK, regex);
            Tokens.Add(TokenType.COMMENTBLOCK);


        }

        public void Init(string input)
        {
            this.Input = input;
            StartPos = 0;
            EndPos = 0;
            CurrentLine = 0;
            CurrentColumn = 0;
            CurrentPosition = 0;
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
                else if (tok.StartPos < tok.EndPos - 1)
                {
                    tok.Text = Input.Substring(tok.StartPos, 1);
                }

                if (SkipList.Contains(tok.Type))
                {
                    startpos = tok.EndPos;
                    Skipped.Add(tok);
                }
                else
                {
                    // only assign to non-skipped tokens
                    tok.Skipped = Skipped; // assign prior skips to this token
                    Skipped = new List<Token>(); //reset skips
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
            EvalStatement= 5,
            VarStatement= 6,
            PrintStatement= 7,
            MathCMDStatement= 8,
            AddExpr = 9,
            MultExpr= 10,
            Atom    = 11,

            //Terminal tokens:
            NUMBER  = 12,
            FLOAT   = 13,
            PLUSMINUS= 14,
            MULTDIV = 15,
            BROPEN  = 16,
            BRCLOSE = 17,
            EOF     = 18,
            IDENTIFIER= 19,
            SET     = 20,
            WHITESPACE= 21,
            OPENBR  = 22,
            CLOSEBR = 23,
            PERIOD  = 24,
            STATEMENTSEP= 25,
            QUESTION= 26,
            COLON   = 27,
            COMMAND = 28,
            ABS     = 29,
            SIN     = 30,
            COS     = 31,
            TAN     = 32,
            SQRT    = 33,
            EQUAL   = 34,
            UPPER   = 35,
            NEWLINE = 36,
            MULTINEWLINE= 37,
            WAVEY   = 38,
            STRING  = 39,
            COMMENTBLOCK= 40
    }

    public class Token
    {
        private int startpos;
        private int endpos;
        private string text;
        private object value;

        // contains all prior skipped symbols
        private List<Token> skipped;

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

        public List<Token> Skipped { 
            get { return skipped;} 
            set { skipped = value; }
        }
        public object Value { 
            get { return value;} 
            set { this.value = value; }
        }

        [XmlAttribute]
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
