using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KlipCompiler
{
    class Program
    {
        public static List<string> imports;

        static void Main(string[] args)
        {
            imports = new List<string>();

            StreamReader sr = new StreamReader(args[0]);
            string code = sr.ReadToEnd();

            Lexer lexer = new KlipCompiler.Lexer();
            lexer.InputString = code;

            List<Token> tokens = new List<Token>();

            while (true)
            {
                try
                {
                    Token t = lexer.GetToken();

                    if (t.TokenName.ToString() != "Whitespace" && t.TokenName.ToString() != "NewLine" && t.TokenName.ToString() != "Undefined")
                    {
                        tokens.Add(t);
                    }
                }
                catch
                {
                    break;
                }
            }

            Token tok = new Token(Lexer.Tokens.EOF, "EOF");
            tokens.Add(tok);

            TokenList tokenlist = new TokenList(tokens);

            Parser parser = new Parser(tokenlist);
            List<Stmt> tree = parser.GetTree();

            Compiler compiler = new Compiler(tree);
            string c = compiler.GetCode();

            string path = Path.GetDirectoryName(args[0]);

            foreach (string p in imports)
            {
                StreamReader s = new StreamReader(path + "\\" + p + ".txt");
                c += "\n" + s.ReadToEnd();
            }

            FileStream fs = new FileStream(Path.GetFileNameWithoutExtension(args[0]) + ".krt", FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(c);
        }
    }
}
