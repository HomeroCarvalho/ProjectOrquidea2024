using parser.ProgramacaoOrentadaAObjetos;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;

namespace parser
{

    /// <summary>
    /// converte uma arquivo de texto, em uma lista de tokens da linguagem orquidea.
    /// funcionalidades extras: executar um programa orquidea contido num arquivo de texto.
    /// </summary>
    public class ParserAFile
    {
        /// <summary>
        /// lista dos tokens do codigo.
        /// </summary>
        private List<string> tokens;

        /// <summary>
        /// lista de linhas de codigo, como está no texto do programa.
        /// </summary>
        private List<string> code;


        /// <summary>
        /// contador de linhas de codigo, afim de sinalizar eventuais falhas e erros no codigo, sinalizando linhas de falha.
        /// começa em 1, porque é uma linha de codigo, começa em 1.
        /// </summary>
        private static int linesPureCode = 0;

        /// <summary>
        /// lista de caracteres a remover, a fim de obter apenas o codigo, dentro de um arquivo de texto.
        /// </summary>
        private List<char> caracteresRemover = new List<char>() { '\t', '\n' };


        /// <summary>
        /// linhas vazias, ou linhas de comentarios.
        /// </summary>
        public static List<int> LINE_TEXT_FILE = new List<int>();
        /// <summary>
        /// linhas contendo codigo.
        /// </summary>
        public static List<string> LINE_CODE = new List<string>();




        
        /// <summary>
        /// modo de depuracao para encontrar falhas no algoritmo de contagem de linhas.
        /// </summary>
        public static bool DEBUG = false;



        /// <summary>
        /// retorna as linhas de codigo.
        /// </summary>
        /// <returns></returns>
        public List<string> GetCode()
        {
            return this.code;
        }

        
        /// <summary>
        /// retorna os tokens do codigo.
        /// </summary>
        /// <returns></returns>
        public List<string> GetTokens()
        {
            return this.tokens;
        }



        /// <summary>
        /// lista de erros no carregamento do arquivo de codigo.
        /// </summary>
        public List<string> msgErros = new List<string>();



       

        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="pathFile">caminho do arquivo que contem o codigo a executar.</param>
        public ParserAFile(string pathFile)
        {
            this.Parser(pathFile);
        }


        /// <summary>
        /// inicialia o sistema utilizado.
        /// </summary>
        public static void InitSystem()
        {
            TablelaDeValores.expressoes = new List<Expressao>();
            LinguagemOrquidea.Instance().Aspectos = new List<Aspecto>();


        }

        /// <summary>
        /// le, obtem tokens, compila e executa um programa orqquidea.
        /// </summary>
        /// <param name="fileName">nome do arquivo contendo o programa.</param>
        /// <exception cref="Exception">lança uma exceção se nenhum token foi encontrado.</exception>
        public static void ExecuteAProgram(string fileName)
        {
            SystemInit.InitSystem();

            ParserAFile parser = new ParserAFile(fileName);
            if ((parser != null) && (parser.GetTokens() != null) && (parser.GetTokens().Count > 0))
            {
                ProcessadorDeID compilador = new ProcessadorDeID(parser.GetTokens());
                compilador.Compilar();


                // VERIFICA SE HÁ FALHAS, E DÁ AO DEV OPÇÃO DE RODAR O PROGRAMA COM ERROS, OU NÃO.
                if ((SystemInit.errorsInCopiling != null) && (SystemInit.errorsInCopiling.Count > 0))
                {
                    // MOSTRA AS FALHAS E O MENU DE CONTINUAR OU NAO.
                    Console.WriteLine("There is fails in yoour program: ");
                    for (int i = 0; i < SystemInit.errorsInCopiling.Count; i++)
                    {
                        try
                        {
                            Console.WriteLine("   ---->" + SystemInit.errorsInCopiling[i]);
                        }
                        catch (Exception e)
                        {
                            string errorMsg = e.Message;
                        }
                    }

                    // menu de opcoes: "s/S": roda o propgrama, "n/N": encerra a compilação, afim de fixar os erros. 
                    string op = "";
                    while ((op.ToLower() != "s") && (op.ToLower() != "n"))
                    {
                        Console.Write("You want running de program? (s/n) (threre are errors) ");
                        op = Console.ReadLine();
                    }
                    if (op == "n")
                    {
                        System.Environment.Exit(1);
                    }


                }
                else
                {
                    // EXECUTAR O PROGRAMA.
                    if ((compilador.GetInstrucoes() != null) && (compilador.GetInstrucoes().Count > 0))
                    {
                        ProgramaEmVM programaVM = new ProgramaEmVM(compilador.GetInstrucoes());
                        programaVM.Run(compilador.escopo);
                    }
                }
            }
            else
            {
                throw new Exception("error in parse, not found tokens valid.");
            }
        }

        /// <summary>
        /// converte linhas de um aqrquivo, para linhas de puro codigo, 
        /// e linhas de texto, contendo linhas de codigo e linhas vazias.
        /// </summary>
        /// <param name="fileName">nome do arquivo contendo codigo orquidea.</param>
        private void Parser(string fileName)
        {

            this.code = new List<string>();
            this.tokens = new List<string>();


            InitSystem();

            // calcula as linas de codigo.
            CalcLinesOfCode(fileName);

            // essencial para se contar a posicao de tokens, é preciso inicializar algumas propriedades estáticas.
            PosicaoECodigo.InitCalculoPosicoes();   

        
           

            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            if ((code != null) && (code.Count > 0))
            {
                // converte o código para uma lista de tokens.
                this.tokens = new Tokens(code).GetTokens(); 
            }
                

        } // Parser()






        /// <summary>
        /// funcao de averigação de caracteres especiais, num arquivo de texto.
        /// </summary>
        /// <param name="pathFile">path do arquivo de texto a inspecionar.</param>
        private static void InspecaoLinhasDeCodigo(string pathFile)
        {
            StreamReader sr = new StreamReader(pathFile);
            string line = "";
            while (!sr.EndOfStream)
            {
                char[] buffer = new char[1];

                sr.Read(buffer, 0, 1);
                if (buffer[0] == '\n')
                {
                    Console.WriteLine(line);

                }
                else
                {
                    if (buffer[0] == '\r')
                    {
                        Console.Write("<r>");
                    }
                    else
                    if (buffer[0] == '\t')
                    {
                        Console.Write("<t>");
                    }
                    else
                    if (buffer[0] == '\b')
                    {
                        Console.Write("<b>");
                    }
                    else
                    if (buffer[0] == '\f')
                    {
                        Console.Write("<f>");
                    }
                    else
                    if (buffer[0] == '\v')
                    {
                        Console.Write("<v>");
                    }
                    else
                    {
                        Console.Write(buffer[0]);
                    }
                }
            }
            sr.Close();
            Console.WriteLine("ENTER PARA TERMINAR");
            Console.ReadLine();
        }


        /// <summary>
        /// calcula as linhas de codigo puro, somente codigo, e linhas de espaço vazio (para fins de linha de texto).
        /// </summary>
        /// <param name="pathFileProgram">path para o aquivo de programa.</param>
        public  void CalcLinesOfCode(string pathFileProgram)
        {


            ParserAFile.linesPureCode = 1;


           


            int countSpacing = 0;
            int indexLinesCode = 0;


            StreamReader stream = new StreamReader(pathFileProgram);
            while (stream.EndOfStream != null)
            {
                string line=stream.ReadLine();
               
                
                if (line == null)
                {
                    break;
                }
                line = line.Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace("\b","").Replace("/f","").Replace("/v",""). Trim(' ');
                if (line == "")
                {
                    countSpacing++;
                }
                else
                if (line.Contains("//"))
                {
                    countSpacing++;
                }
                else
                {
                    ParserAFile.linesPureCode++;
                    LINE_CODE.Add(line);
                    LINE_TEXT_FILE.Add(countSpacing + LINE_CODE.Count);

                    // lista de codigo, para fins de obter tokens do programa.
                    this.code.Add(line);

                    indexLinesCode++;
                    if (DEBUG)
                    {
                        Console.WriteLine("line {0}:  {1},   text line: {2}", indexLinesCode, line, LINE_TEXT_FILE[LINE_TEXT_FILE.Count - 1]);
                    }
                    
               }
            }

            stream.Close();
        }
       

        /// <summary>
        /// retorna a linha de codigo, e a linha de texto do arquivo do programa,
        /// UTILIZADO PARA OBTER LINHAS DE CODIGO EM UtilTokens.Write_AMensageError().
        /// </summary>
        /// <param name="indexLine">linha de codigo, somente codigo.</param>
        public static string GetLine(int indexLine)
        {
            if ((LINE_CODE.Count == 0) || (LINE_TEXT_FILE.Count == 0))
            {
                return "";
            }
            else
            {
                return "code:   " + LINE_CODE[indexLine] + "  line text file: " + LINE_TEXT_FILE[indexLine];
            }
            
            
      
        }




        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes de parser file")
            {
            }

            public void TesteInspecaoProgramaComFalhas(AssercaoSuiteClasse assercao)
            {
                string pathFile = @"programasTestes\programaClasseContadorComFalhas.txt";
                ParserAFile.InspecaoLinhasDeCodigo(pathFile);
            }
            public void TesteProgramaContagemComFalhas(AssercaoSuiteClasse assercao)
            {
                string pathFile = @"programasTestes\programContagensModulesComFalha.txt";
                ParserAFile.ExecuteAProgram(pathFile);

            }
            public void TesteProgramaContadorComFalhas(AssercaoSuiteClasse assercao)
            {
                string pathFile = @"programasTestes\programaClasseContadorComFalhas.txt";
                ParserAFile.ExecuteAProgram(pathFile);
            }

            
           

           

            public void TesteModulesAndLibraries(AssercaoSuiteClasse assercao)
            {
                ParserAFile parser = new ParserAFile(@"programasTestes\programContagensModules.txt");
                List<string> linesOfCode = parser.GetCode();
                for (int i = 0; i < linesOfCode.Count; i++)
                {
                    System.Console.WriteLine(linesOfCode[i]);
                }


                System.Console.WriteLine("Pressione ÈNTER para terminar o teste");
                System.Console.ReadLine();

            }

            public void TesteParserCompilacao2(AssercaoSuiteClasse assercao)
            {
                string pathFile = @"programasTestes\HelloWorldComFalhas.txt";
                ParserAFile.ExecuteAProgram(pathFile);

                System.Console.WriteLine("Pressione ÈNTER para terminar o teste");
                System.Console.ReadLine();
            }

            public void TesteParserCompilacaoExecucao(AssercaoSuiteClasse assercao)
            {

                string nameFile = @"programasTestes\programHelloWorldComFalha.txt";
                ParserAFile.ExecuteAProgram(nameFile);

                System.Console.WriteLine("Pressione ÈNTER para terminar o teste");
                System.Console.ReadLine();
            }

            public void TesteContagemDeLinha(AssercaoSuiteClasse assercao)
            {
                
                string pathFile = @"esbocos testes\funcoesGameRnd.txt";
                ParserAFile parser = new ParserAFile(pathFile);
                parser.CalcLinesOfCode(pathFile);


                Console.WriteLine("ENTER para terminar.");
                Console.ReadLine();
            }

            public void TesteParserProgramFuncoesGame(AssercaoSuiteClasse assercao)
            {
                string pathFile = @"esbocos testes\funcoesGameRnd.txt";
                ParserAFile.ExecuteAProgram(pathFile);
            }
            
            public void TesteLinhasDeCometarios(AssercaoSuiteClasse assercao)
            {
                ParserAFile parser = new ParserAFile(@"programasTestes\programaFatorialComComentarios.txt");
                List<string> linesOfCode = parser.GetCode();
                for (int i = 0; i < linesOfCode.Count; i++)
                {
                    System.Console.WriteLine(linesOfCode[i]);
                }

                System.Console.WriteLine("Pressione ÈNTER para terminar o teste");
                System.Console.ReadLine();

            }

        }
    }
}
