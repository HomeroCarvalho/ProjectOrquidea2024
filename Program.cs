// See https://aka.ms/new-console-template for more information
using parser;

Console.WriteLine("Project Orquidea2024 demonstration");
Console.WriteLine("Choice to: ");
Console.WriteLine("1- run a game in this language");
Console.WriteLine("2- run tests");
string op = "";
while ((op!="1") && (op != "2"))
{
    Console.Write("you option? (1-2):  ");
    op = Console.ReadLine();
}
if (op == "1")
{
    Console.WriteLine("game code file is in: programa jogos\\CatchMeteors.txt");
    Console.WriteLine("compiling is slow, in 1.5 min");
    Console.WriteLine("compiling now...");
    ParserAFile.ExecuteAProgram(@"programa jogos\CatchMeteors.txt");
}
else
if (op == "2")
{
    Console.WriteLine("you will see the results in a file named: RelatorioTexto.txt");
    Console.WriteLine("run tests of EvalExpression now...");
    ClassesDeTestes testesContainer = new ClassesDeTestes();

}






