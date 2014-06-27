using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShoNS.Array;
namespace TestCodes
{
    class Program
    {
        static void Main(string[] args)
        {
            var M = new SparseDoubleArray(5, 5);
            M[0, 0] = 5;
            M[0, 0] += 5;
            M[4, 2] = 3;
            M[1, 1] = 5;
            M[2, 2] = 3;
            M[2, 2] = 4;
            M.Clear();
            System.Console.WriteLine((M).ToString());
            System.Console.Read();
        }
    }
}
