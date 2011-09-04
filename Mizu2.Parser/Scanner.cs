// Generated by TinyPG v1.3 available at www.codeproject.com

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Mizu2.Parser
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
            SkipList.Add(TokenType.TAB);
            SkipList.Add(TokenType.FOURSPACE);

            regex = new Regex(@"\<", RegexOptions.Compiled);
            Patterns.Add(TokenType.GT, regex);
            Tokens.Add(TokenType.GT);

            regex = new Regex(@"\<\=", RegexOptions.Compiled);
            Patterns.Add(TokenType.GTE, regex);
            Tokens.Add(TokenType.GTE);

            regex = new Regex(@"\=\=", RegexOptions.Compiled);
            Patterns.Add(TokenType.DEQUAL, regex);
            Tokens.Add(TokenType.DEQUAL);

            regex = new Regex(@"\>", RegexOptions.Compiled);
            Patterns.Add(TokenType.LT, regex);
            Tokens.Add(TokenType.LT);

            regex = new Regex(@"\>\=", RegexOptions.Compiled);
            Patterns.Add(TokenType.LTE, regex);
            Tokens.Add(TokenType.LTE);

            regex = new Regex(@"(\!\=|\<\>)", RegexOptions.Compiled);
            Patterns.Add(TokenType.NOTEQUAL, regex);
            Tokens.Add(TokenType.NOTEQUAL);

            regex = new Regex(@"\{", RegexOptions.Compiled);
            Patterns.Add(TokenType.OPENBRCK, regex);
            Tokens.Add(TokenType.OPENBRCK);

            regex = new Regex(@"\}", RegexOptions.Compiled);
            Patterns.Add(TokenType.CLOSEBRCK, regex);
            Tokens.Add(TokenType.CLOSEBRCK);

            regex = new Regex(@"\"".+?\""", RegexOptions.Compiled);
            Patterns.Add(TokenType.STRING, regex);
            Tokens.Add(TokenType.STRING);

            regex = new Regex(@"\,", RegexOptions.Compiled);
            Patterns.Add(TokenType.COMMA, regex);
            Tokens.Add(TokenType.COMMA);

            regex = new Regex(@"(\r\n|\n)", RegexOptions.Compiled);
            Patterns.Add(TokenType.NEWLINE, regex);
            Tokens.Add(TokenType.NEWLINE);

            regex = new Regex(@"\=", RegexOptions.Compiled);
            Patterns.Add(TokenType.EQUAL, regex);
            Tokens.Add(TokenType.EQUAL);

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

            regex = new Regex(@"\:", RegexOptions.Compiled);
            Patterns.Add(TokenType.COLON, regex);
            Tokens.Add(TokenType.COLON);

            regex = new Regex(@"^\s*$", RegexOptions.Compiled);
            Patterns.Add(TokenType.EOF, regex);
            Tokens.Add(TokenType.EOF);

            regex = new Regex(@"\t", RegexOptions.Compiled);
            Patterns.Add(TokenType.TAB, regex);
            Tokens.Add(TokenType.TAB);

            regex = new Regex(@"\s\s\s\s", RegexOptions.Compiled);
            Patterns.Add(TokenType.FOURSPACE, regex);
            Tokens.Add(TokenType.FOURSPACE);

            regex = new Regex(@"[a-zA-Z_][a-zA-Z0-9_]*", RegexOptions.Compiled);
            Patterns.Add(TokenType.IDENTIFIER, regex);
            Tokens.Add(TokenType.IDENTIFIER);

            regex = new Regex(@"(\-)?[0-9]+", RegexOptions.Compiled);
            Patterns.Add(TokenType.NUMBER, regex);
            Tokens.Add(TokenType.NUMBER);

            regex = new Regex(@"(\-)?[0-9]+\.[0-9]+", RegexOptions.Compiled);
            Patterns.Add(TokenType.FLOAT, regex);
            Tokens.Add(TokenType.FLOAT);

            regex = new Regex(@"[a-zA-Z_]*(\.[a-zA-Z0-9_]*)+", RegexOptions.Compiled);
            Patterns.Add(TokenType.TYPE, regex);
            Tokens.Add(TokenType.TYPE);

            regex = new Regex(@"(null|nil)", RegexOptions.Compiled);
            Patterns.Add(TokenType.NULLKW, regex);
            Tokens.Add(TokenType.NULLKW);

            regex = new Regex(@"class", RegexOptions.Compiled);
            Patterns.Add(TokenType.CLASSKW, regex);
            Tokens.Add(TokenType.CLASSKW);

            regex = new Regex(@"public", RegexOptions.Compiled);
            Patterns.Add(TokenType.PUBLICKW, regex);
            Tokens.Add(TokenType.PUBLICKW);

            regex = new Regex(@"private", RegexOptions.Compiled);
            Patterns.Add(TokenType.PRIVATEKW, regex);
            Tokens.Add(TokenType.PRIVATEKW);

            regex = new Regex(@"(P|p)ublic class", RegexOptions.Compiled);
            Patterns.Add(TokenType.PUBLICCLASSKW, regex);
            Tokens.Add(TokenType.PUBLICCLASSKW);

            regex = new Regex(@"(P|p)rivate class", RegexOptions.Compiled);
            Patterns.Add(TokenType.PRIVATECLASSKW, regex);
            Tokens.Add(TokenType.PRIVATECLASSKW);

            regex = new Regex(@"def", RegexOptions.Compiled);
            Patterns.Add(TokenType.DEFKW, regex);
            Tokens.Add(TokenType.DEFKW);

            regex = new Regex(@"(P|p)ublic def", RegexOptions.Compiled);
            Patterns.Add(TokenType.PUBLICDEFKW, regex);
            Tokens.Add(TokenType.PUBLICDEFKW);

            regex = new Regex(@"(P|p)rivate def", RegexOptions.Compiled);
            Patterns.Add(TokenType.PRIVATEDEFKW, regex);
            Tokens.Add(TokenType.PRIVATEDEFKW);

            regex = new Regex(@"\(", RegexOptions.Compiled);
            Patterns.Add(TokenType.BROPEN, regex);
            Tokens.Add(TokenType.BROPEN);

            regex = new Regex(@"\)", RegexOptions.Compiled);
            Patterns.Add(TokenType.BRCLOSE, regex);
            Tokens.Add(TokenType.BRCLOSE);

            regex = new Regex(@"if", RegexOptions.Compiled);
            Patterns.Add(TokenType.IFKW, regex);
            Tokens.Add(TokenType.IFKW);

            regex = new Regex(@"true", RegexOptions.Compiled);
            Patterns.Add(TokenType.TRUE, regex);
            Tokens.Add(TokenType.TRUE);

            regex = new Regex(@"false", RegexOptions.Compiled);
            Patterns.Add(TokenType.FALSE, regex);
            Tokens.Add(TokenType.FALSE);

            regex = new Regex(@"var", RegexOptions.Compiled);
            Patterns.Add(TokenType.VAR, regex);
            Tokens.Add(TokenType.VAR);

            regex = new Regex(@"new", RegexOptions.Compiled);
            Patterns.Add(TokenType.NEW, regex);
            Tokens.Add(TokenType.NEW);

            regex = new Regex(@"as", RegexOptions.Compiled);
            Patterns.Add(TokenType.AS, regex);
            Tokens.Add(TokenType.AS);

            regex = new Regex(@"else", RegexOptions.Compiled);
            Patterns.Add(TokenType.ELSE, regex);
            Tokens.Add(TokenType.ELSE);


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
            Boolean = 3,
            Assignment= 4,
            VariableAssignment= 5,
            VariableExpr= 6,
            Statement= 7,
            IfStatement= 8,
            IfStmtIFBody= 9,
            IfStmtELSEBody= 10,
            FuncCall= 11,

            //Terminal tokens:
            GT      = 12,
            GTE     = 13,
            DEQUAL  = 14,
            LT      = 15,
            LTE     = 16,
            NOTEQUAL= 17,
            OPENBRCK= 18,
            CLOSEBRCK= 19,
            STRING  = 20,
            COMMA   = 21,
            NEWLINE = 22,
            EQUAL   = 23,
            WHITESPACE= 24,
            OPENBR  = 25,
            CLOSEBR = 26,
            PERIOD  = 27,
            COLON   = 28,
            EOF     = 29,
            TAB     = 30,
            FOURSPACE= 31,
            IDENTIFIER= 32,
            NUMBER  = 33,
            FLOAT   = 34,
            TYPE    = 35,
            NULLKW  = 36,
            CLASSKW = 37,
            PUBLICKW= 38,
            PRIVATEKW= 39,
            PUBLICCLASSKW= 40,
            PRIVATECLASSKW= 41,
            DEFKW   = 42,
            PUBLICDEFKW= 43,
            PRIVATEDEFKW= 44,
            BROPEN  = 45,
            BRCLOSE = 46,
            IFKW    = 47,
            TRUE    = 48,
            FALSE   = 49,
            VAR     = 50,
            NEW     = 51,
            AS      = 52,
            ELSE    = 53
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
