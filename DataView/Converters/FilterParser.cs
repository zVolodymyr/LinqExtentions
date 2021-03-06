﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuncanApps.DataView.Converters
{
    class FilterParser
    {
        enum TokenType
        {
            OpenGroup,
            CloseGroup,
            Word,
            Number,
            String,
            EoF
        }

        private const string nullValue = "null";

        private string _text;
        private int _pos = 0;
        private readonly StringBuilder _buf = new StringBuilder();
        private readonly bool _handleNullValue;


        public FilterParser(string text, bool handleNullValue = false)
        {
            _text = text;
            _handleNullValue = handleNullValue;
        }

        public IWhereClause Parse()
        {
            return ParseInternal(false);
        }

        public IWhereClause ParseInternal(bool isGroup)
        {
            IWhereClause result = null;
            KeyValuePair<TokenType, string> token;
            WhereLogic logic = WhereLogic.And;

            do
            {
                if (result != null)
                {
                    token = ExpectToken(TokenType.CloseGroup, TokenType.Word, TokenType.EoF);

                    if (token.Key == TokenType.CloseGroup)
                        if (isGroup)
                            break;
                        else
                            throw new ArgumentException($"Unexpected token: {token.Key}");
                    else if (token.Key == TokenType.Word)
                        logic = ParseHelper.ParseWhereLogic(token.Value);
                }

                token = ExpectToken(TokenType.OpenGroup, TokenType.Word, TokenType.EoF);

                IWhereClause clause = null;

                if (token.Key == TokenType.OpenGroup)
                {
                    clause = ParseInternal(true);
                }
                else if (token.Key == TokenType.Word)
                {
                    var opValue = ParseHelper.ParseWhereOperator(ExpectToken(TokenType.Word).Value);
                    var valueToken = ExpectToken(TokenType.Word, TokenType.Number, TokenType.String);

                    clause = new WhereClause
                    {
                        Field = token.Value,
                        Operator = opValue,
                        Value = (_handleNullValue && valueToken.Key == TokenType.Word && valueToken.Value == nullValue) ? null : valueToken.Value
                    };
                }

                if (clause != null)
                    result = result == null ? clause : result.Combine(logic, clause);
            }
            while (token.Key != TokenType.EoF);

            return result;
        }

        private KeyValuePair<TokenType, string> ExpectToken(params TokenType[] tokens)
        {
            var result = ReadToken();

            if (!tokens.Contains(result.Key))
                throw new ArgumentException($"Unexpected token: {result.Key}");

            return result;
        }

        private KeyValuePair<TokenType, string> ReadToken()
        {
            ReadWhile(char.IsWhiteSpace);

            if (_pos >= _text.Length)
                return new KeyValuePair<TokenType, string>(TokenType.EoF, null);

            var c = _text[_pos];

            if (c == '(')
            {
                _pos++;
                return new KeyValuePair<TokenType, string>(TokenType.OpenGroup, null);
            }
            else if (c == ')')
            {
                _pos++;
                return new KeyValuePair<TokenType, string>(TokenType.CloseGroup, null);
            }
            else if (c == '\"')
                return new KeyValuePair<TokenType, string>(TokenType.String, ReadString());
            else if (IsNumberChar(c))
                return new KeyValuePair<TokenType, string>(TokenType.Number, ReadWhile(IsNumberChar));

            return new KeyValuePair<TokenType, string>(TokenType.Word, ReadWhile(IsWordChar));
        }

        private string ReadWhile(Func<char, bool> isEnd)
        {
            _buf.Clear();

            while (_pos < _text.Length && isEnd(_text[_pos]))
                _buf.Append(_text[_pos++]);

            return _buf.Length == 0 ? null : _buf.ToString();
        }

        private string ReadString()
        {
            _pos++; // double quotes 

            return ReadWhile(c => {
                if (c == '\\' || c == '"')
                    _pos++;

                return c != '"';
            });
        }

        private static bool IsNumberChar(char c)
        {
            return char.IsDigit(c) || c == '.' || c == '-';
        }

        private static bool IsWordChar(char c)
        {
            return c == '@' || c == '_' || char.IsLetterOrDigit(c); 
        }
    }
}
