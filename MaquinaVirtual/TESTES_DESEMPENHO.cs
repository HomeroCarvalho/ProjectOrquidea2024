using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
namespace parser
{
    /// <summary>
    /// classe de verificação de codigo implementado, se é o melhor desempenho, ou não.
    /// </summary>
    public class TESTES_DESEMPENHO
    {


        public class Testes: SuiteClasseTestes
        {
        
            public Testes():base("testes de melhoria de desempenho")
            {

            }

            // teste de desempenho se dictionary é melhor que List.FindIndex.
            public void TesteDesempenhoGetObjetoFuncao(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_class = "public class entity{" +
                        "public int x; " +
                        "public int y; " +
                        "public entity(){x=1;y=2;};};";
                    string code_create = "entity e= create();  entity[] v[1]; v.Clear(); for (int i=0;i<100;i++){v.Append(e);};";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_class + code_create);
                    compilador.Compilar();

                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    int countNames = 0;
                    Vector v1 = (Vector)compilador.escopo.tabela.GetObjeto("v", compilador.escopo);


                    // dicionario contendo todos objetos, tem que ter cuidado com objetos de mesmo nome,
                    // se não tratar isso, e nao utilizar a funcao certa, pode lançar uma exceção
                    Dictionary<string, Objeto> dict_objects = new Dictionary<string, Objeto>();
                    for (int i = 0;i < v1.size(); i++)
                    {
                        if (v1.Get(i) != null)
                        {
                            dict_objects.Add("obj_" + countNames++, (Objeto)v1.Get(i));
                        }
                    }

                    // lista de objetos.
                    List<Objeto> lst_objects = new List<Objeto>();
                    for (int i = 0; i < v1.size(); i++)
                    {
                        lst_objects.Add((Objeto)v1.Get(i));
                    }

                    List<Objeto> lstOBJETOS_TO_FIND = lst_objects.ToList<Objeto>();
                    Random rnd= new Random();


                    Stopwatch lstTimer= new Stopwatch();
                    lstTimer.Start();

                    // teste de desempenho para listas, e funcao FindIndex.
                    for (int i = 0; i < 40000; i++)
                    {
                        Objeto objToSearch = lstOBJETOS_TO_FIND[rnd.Next(lstOBJETOS_TO_FIND.Count)];
                        int index = lst_objects.FindIndex(k => k.nome == objToSearch.nome);
                        Objeto objFound = lst_objects[index];
                    }
                    lstTimer.Stop();


                    Stopwatch dictTimer= new Stopwatch();
                    dictTimer.Start();
                    // teste de desempenho para dicionarios.
                    for (int i = 0; i < 40000; i++)
                    {
                        Objeto objToSearch = lstOBJETOS_TO_FIND[rnd.Next(lstOBJETOS_TO_FIND.Count)];
                        Objeto objFound = null;
                        dict_objects.TryGetValue(objToSearch.nome, out objFound);

                    }
                    dictTimer.Stop();
                        


                    long timeLST = lstTimer.ElapsedMilliseconds;
                    long timeDICT= dictTimer.ElapsedMilliseconds;

                    Console.WriteLine("tempo de acesso a lista: {0} ", timeLST);
                    Console.WriteLine("tempo de acesso em dicionario {0}", timeDICT);

                    Console.WriteLine("tecle [ENTER para terminar o cenario.");
                    Console.ReadLine();


                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }


            }



            public void TesteDesempenhoVector(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string code_0_0= "int[] v1[10];  v1[0]=1; int i=0; for (int j=0;j<100;j++){v1[i];};";
                string code_0_1 = "int[]  v2[10]; v2[0]=1; int i=0; for (int j=0;j<100;j++){v2.Get(i));};";
                string code_0_2 = "int[]  v3[10]; v3[0]=1; int i=0; for (int j=0;j<1;j++){v3.VectorObjects[0];};";



                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                compilador.Compilar();

                Stopwatch timer1 = new Stopwatch();
                timer1.Start();
                ProgramaEmVM programa= new ProgramaEmVM(compilador.GetInstrucoes());
                programa.Run(compilador.escopo);
                timer1.Stop();


                long timerTotalGetElement= timer1.ElapsedMilliseconds;



                ProcessadorDeID compilador2 = new ProcessadorDeID(code_0_1);
                compilador2.Compilar(); 

                Stopwatch timer2= new Stopwatch();
                timer2.Start();
                ProgramaEmVM programa2 = new ProgramaEmVM(compilador.GetInstrucoes());
                programa2.Run(compilador2.escopo);
                timer2.Stop();

                long timerTotalGet = timer2.ElapsedMilliseconds;



                ProcessadorDeID compilador3 = new ProcessadorDeID(code_0_2);
                compilador3.Compilar();


                Stopwatch timer3 = new Stopwatch();
                timer3.Start();

                Vector v3 = (Vector)compilador3.escopo.tabela.GetObjeto("v3", compilador3.escopo);
                v3.Set(0, 1);
                for (int x = 0; x < 100; x++)
                {
                    v3.VectorObjects[0] = 1;
                }
                
                
                ProgramaEmVM programaAcessoDireto= new ProgramaEmVM(compilador3.GetInstrucoes());
                programaAcessoDireto.Run(compilador3.escopo);
                timer3.Stop();

                long timerTotalAcessoDireto=timer3.ElapsedMilliseconds; 

                Console.WriteLine("tempo em GetElement funcao: {0}", timerTotalGetElement);
                Console.WriteLine("tempo em Get função:  {0}", timerTotalGet);
                Console.WriteLine("tempo em Acesso Dirto: {0}", timerTotalAcessoDireto);
                Console.WriteLine("desempenho: {0} %", (float)timerTotalGet / (float)timerTotalGetElement);

                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("tecle ENTER para terminar.");
                Console.ReadLine();

                System.Environment.Exit(0);

            }

            public void TesteTypeTypeOf(AssercaoSuiteClasse assercao)
            {

                int contadorStress = 10000000;

                Stopwatch timerGetType = new Stopwatch();



                timerGetType.Start();
                int contador = 0;
                for (int i=0; i < contadorStress; i++)
                {

                    Expressao expressao= new Expressao();
                    if (expressao.GetType() == typeof(Expressao))
                    {
                        contador++;
                    }

                }

                timerGetType.Stop();

                long tempoGastoGetType = timerGetType.ElapsedMilliseconds;




                Stopwatch timerConstanteInt= new Stopwatch();
                timerConstanteInt.Start();

                int constProcessing = 0;
                for (int i = 0; i < contadorStress; i++)
                {
                    if (i< contadorStress)
                    {
                        constProcessing++;
                    }
                    

                }

                timerConstanteInt.Stop();



                long tempoGastoConstanteInteira = timerConstanteInt.ElapsedMilliseconds;

                Console.WriteLine("tempo GetType():  {0}    tempo constante inteiro: {1}", tempoGastoGetType, tempoGastoConstanteInteira);
                Console.WriteLine("% de desemepnho: {0}", ((1.0 - (double)tempoGastoConstanteInteira / (double)contadorStress) * 100).ToString());
                Console.Write("PRESS ENTER TO QUIT");
                Console.ReadLine();



            }


        }

    }
}
