using System;

namespace Whirlpool
{
    class Program
    {
        static void Main(string[] args)
        {
            Whirlpool whirlpool = new Whirlpool("infile.txt");

            Console.WriteLine(whirlpool.GetHash());
        }
    }
}
