using System;

namespace Proiect2
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = new LLk();
            x.ReadFromFile("D:\\Projects\\Laboratoare an III sem II\\Tehnici de Compilare\\Proiect2\\input_data.txt");
            var k = x.VerifyLLk();
            Console.WriteLine("Grammar LL(" + k.ToString() + ") strong!");
            x.ParseWordsFromFile("D:\\Projects\\Laboratoare an III sem II\\Tehnici de Compilare\\Proiect2\\input_words.txt", k);
        }
    }
}
