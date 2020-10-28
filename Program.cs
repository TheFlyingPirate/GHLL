using System;
using System.Collections.Generic;
using System.Linq;

namespace GHLL
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("GHLL Compiler");
     
            string input = ".\\main.g";
            while (true)
            {
                Console.Write(">");
                var line = Console.ReadLine();
                
                var parser = new Parser(line);
                var expresion = parser.Parse();
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                
                PrettyPrint(expresion);
                Console.ForegroundColor = color;
                Lexer lexer = new Lexer(line);
            /*    while (true)
                {
                    var token = lexer.NextToken();
                    if (token.Kind == SyntaxKind.EOFToken)
                        break;
                    Console.Write($"{token.Kind}: '{token.Text}'");
                    if (token.Value != null)
                    {
                        Console.Write($" {token.Value}");
                    }
                    Console.WriteLine();
                }        */
                Console.WriteLine();
                
            }
        }

        static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {
            //└──
            //│
            //├── 
            var marker = isLast ? "└──" : "├── ";
            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Kind);
            
            
            if (node is SyntaxToken t && t.Value != null)
            {
                Console.Write(" ");
                Console.Write(t.Value);
            }
            Console.WriteLine();
            //indent += "    ";
            indent += isLast ? "    " : "│   ";
            var lastChild = node.GetChildren().LastOrDefault();
            foreach (var child in node.GetChildren())
            {
                PrettyPrint(child,indent, child == lastChild);
            }
        }
    }

    public class Compiler
    {
        
    }
    
    public enum SyntaxKind
    {
        NumberToken,
        WhiteSpaceToken,
        PlusToken,
        MinusToken,
        TimesToken,
        DividedToken,
        ModuloToken,
        LBracketsToken,
        RBracketsToken,
        BadToken,
        EOFToken,
        BinaryExpression,
        NumberExpresion
    }
    
    public class SyntaxToken:SyntaxNode
    {
        public override SyntaxKind Kind { get; }
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Enumerable.Empty<SyntaxNode>();        
        }

        public int Position { get; }
        public string Text { get; }
        
        public object Value { get; }
        public SyntaxToken(SyntaxKind kind,int position,string text,object value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }
    }
    
    public class Lexer
    {
        private readonly string _text;
        private int _position;
        
        public Lexer(string text)
        {
            _text = text;
           
        }

        private char Current
        {
            get
            {
                if (_position >= _text.Length)
                {
                    return '\0';
                    
                }
                else
                {
                    return _text[_position];
                }
            }
        }

        private void Next()
        {
            _position++;
        }
        
        
        public SyntaxToken NextToken()
        {
            //<numbers>
            // + - * / ( ) %
            // WhiteSpace
            if(_position >= _text.Length)
                return new SyntaxToken(SyntaxKind.EOFToken,_position, "\0", null);
            if (char.IsDigit(Current))
            {
                var start = _position;
                while(char.IsDigit(Current))
                    Next();
                var length = _position - start;
                var text = _text.Substring(start, length);
                int.TryParse(text, out var value);
                return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
            }

            if (char.IsWhiteSpace(Current))
            {
                var start = _position;
                while(char.IsWhiteSpace(Current))
                    Next();
                var length = _position - start;
                var text = _text.Substring(start, length);
                return new SyntaxToken(SyntaxKind.WhiteSpaceToken, start, text, null);
            }

            if (Current == '+')
                return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null);
            if (Current == '-')
                return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", null);
            if (Current == '*')
                return new SyntaxToken(SyntaxKind.TimesToken, _position++, "*", null);
            if (Current == '/')
                return new SyntaxToken(SyntaxKind.DividedToken, _position++, "/", null);
            if (Current == '(')
                return new SyntaxToken(SyntaxKind.LBracketsToken, _position++, "(", null);
            if (Current == ')')
                return new SyntaxToken(SyntaxKind.RBracketsToken, _position++, ")", null);
            if (Current == '%')
                return new SyntaxToken(SyntaxKind.ModuloToken, _position++, "%", null);
            return new SyntaxToken(SyntaxKind.BadToken, _position++,_text[_position-1].ToString(), null);
        }
        
    }

    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }
        public abstract IEnumerable<SyntaxNode> GetChildren();
    }

    public abstract class ExpresionSyntax : SyntaxNode
    {
    }

    public sealed class NumberExpressionSyntax : ExpresionSyntax
    {
     
        public SyntaxToken NumberToken { get; }
        public override SyntaxKind Kind => SyntaxKind.NumberExpresion;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
           yield return NumberToken;
        }

        public NumberExpressionSyntax(SyntaxToken numberToken)
        {
            NumberToken = numberToken;
        }

  
    }

    public sealed class BinaryExpresionSyntax : ExpresionSyntax
    {
        public ExpresionSyntax Left { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpresionSyntax Right { get; }
        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }

        public BinaryExpresionSyntax(ExpresionSyntax left, SyntaxToken operatorToken, ExpresionSyntax right)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }

    
    }

    public class Parser
    {
        private readonly SyntaxToken[] _tokens;
        private int _position;
        public Parser(string text)
        {
            var tokens = new List<SyntaxToken>();
            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.NextToken();
                if (token.Kind != SyntaxKind.WhiteSpaceToken && token.Kind != SyntaxKind.BadToken)
                    tokens.Add(token);
                
            } while (token.Kind != SyntaxKind.EOFToken);

            _tokens = tokens.ToArray();
        }

        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _tokens.Length)
                return _tokens[_tokens.Length - 1];
            return _tokens[index];
        }

        private SyntaxToken Current => Peek(0);

        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }

        public ExpresionSyntax Parse()
        {
            var left = ParsePrimaryExpresion();
            while (Current.Kind == SyntaxKind.PlusToken ||
                   Current.Kind == SyntaxKind.MinusToken)
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpresion();
                left = new BinaryExpresionSyntax(left,operatorToken,right);
            }

            return left;
        }

        private SyntaxToken Match(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();
            return new SyntaxToken(kind, Current.Position, null, null);;
        }
        private ExpresionSyntax ParsePrimaryExpresion()
        {
            var numberToken = Match(SyntaxKind.NumberToken);
            return new NumberExpressionSyntax(numberToken);
        }
    }
}