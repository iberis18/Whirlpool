using System;

namespace Whirlpool
{
    class Program
    {
        static void Main(string[] args)
        {
            Whirlpool whirlpool = new Whirlpool("infile.txt");
            //Whirlpool whirlpool = new Whirlpool("textInfile.txt");
            //Whirlpool whirlpool = new Whirlpool("emptyInfile.txt");
            Console.WriteLine(whirlpool.GetHash());
        }
    }
}
