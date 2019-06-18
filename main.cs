using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSQL;
using TSQL.Statements;
using TSQL.Tokens;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            TSQLSelectStatement select = TSQLStatementReader.ParseStatements(@"
					SELECT OrderDateKey, SUM(SalesAmount) AS TotalSales
					FROM FactInternetSales
					WHERE a__Account = 'hello' or a__Account = 'hello2' and (a__clientaccount = 'sdfgsdf' or a__clientaccount = 'sdfgdfgfsdf' )
                    and teree in ('aaaa','bbbb')
					;")[0] as TSQLSelectStatement;

         
           

            if (select.Where != null)
            {
                Console.WriteLine("WHERE:");
                foreach (TSQLToken token in select.Where.Tokens)
                {
                    Console.WriteLine("\ttype: " + token.Type.ToString() + ", value: " + token.Text);
                }
            }


            var stack = new Stack<TSQLToken>();
            var stack2 = new Stack<TSQLToken>();
            foreach (TSQLToken token in select.Where.Tokens)
            {
                stack.Push(token);
                if(token.Type==TSQLTokenType.Character && token.Text == ")")
                {
                    //pop till you get (
                    TSQLToken t = null;
                    while(t?.Text!="(")
                    {
                        t = stack.Pop();
                        stack2.Push(t);
                    }

                    var processed = ProcessTokens(stack2.ToList());

                    stack.Push(processed);

                }
            }

            var final = ProcessTokens(stack.ToList());

             

            Console.Read();
        }

        private static TSQLToken ProcessTokens(List<TSQLToken> list)
        {
            var values =  list.Where(l => l.Type == TSQLTokenType.StringLiteral);
            var conjunction = list.FirstOrDefault(l => l.Type == TSQLTokenType.Keyword)?.Text ?? "AND";
            var operand = list.FirstOrDefault(l => l.Type == TSQLTokenType.Identifier)?.Text ?? "UNKNOWN";
            return new FillerToken(operand);

        }
    }


    public class FillerToken : TSQLToken
    {
        public override TSQLTokenType Type => TSQLTokenType.Identifier;

        public FillerToken(string text):base(0, text)
        {
            
        }

        

    }




}
