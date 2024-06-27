using MathNet.Numerics.Interpolation;
using parser;
using parser.ProgramacaoOrentadaAObjetos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace parser
{

    /// <summary>
    /// classe para processamento de colisoes entre dois objetos imagens.
    /// </summary>
    public class RectangleCollision:Objeto
    {
        private System.Drawing.Rectangle rect;

        /// <summary>
        /// construtor vazio.
        /// </summary>
        public RectangleCollision()
        {
            this.rect = new System.Drawing.Rectangle(-1, -1, 1, 1);
        }

        /// <summary>
        /// contrutor, aceita coordenadas e dimensoes do retangulo de colisao.
        /// </summary>
        /// <param name="x">coordenada X.</param>
        /// <param name="y">coordenada Y.</param>
        /// <param name="width">dimensao do retangulo.</param>
        /// <param name="height">dimensao do retangulo.</param>
        public RectangleCollision(double x, double y, int width, int height)
        {
            this.rect = new System.Drawing.Rectangle((int)x, (int)y, width, height);
        }

        /// <summary>
        /// atualiza a coordenada X do retangulo.
        /// </summary>
        /// <param name="newX">nova coordenada X.</param>
        public void SetX(double newX)
        {
            this.SET("X", "int", newX);
        }


        /// <summary>
        /// atualiza a coordenada Y do retangulo.
        /// </summary>
        /// <param name="newY">nova coordenada Y.</param>
        public void SetY(double newY)
        {
            this.SET("Y", "double", newY);
        }

        /// <summary>
        /// seta o cumprimento do retangulo de colisão.
        /// </summary>
        /// <param name="w">novo cumprimento.</param>
        public void SetW(int w)
        {
            this.SET("width", "int", w);
          
        }

        /// <summary>
        /// seta a largua do retangulo de colisao.
        /// </summary>
        /// <param name="h">largura do retangulo.</param>
        public void SetH(int h)
        {
            this.SET("height", "int", h);
          
        }


        /// <summary>
        /// verifica se houve colisão entre os retangulos objetos de jogo.
        /// </summary>
        /// <param name="rect1">rectangulo de um objeto de jogo.</param>
        /// <param name="rect2">rectangulo de outro objeto de jogo.</param>
        /// <returns>[true] se houve intersecção dos retangulos (colisão).</returns>
        public static bool isCollide(RectangleCollision rect1, RectangleCollision rect2)
        {
            return rect1.rect.IntersectsWith(rect2.rect);
        }

        /// <summary>
        /// colisao entre dois objetos, pelas coordenadas e dimensoes.
        /// </summary>
        /// <param name="x1">coordenada de desenho do objeto1</param>
        /// <param name="y1">coordenada de desenho do objeto1</param>
        /// <param name="x2">coordenada de desenho do objeto2</param>
        /// <param name="y2">coordenada de desenho do objeto2</param>
        /// <param name="wdth1">dimensao X do objeto1</param>
        /// <param name="heght1">dimensao Y do objeto1</param>
        /// <param name="wdth2">dimensao  X do objeto2</param>
        /// <param name="height2">dimensao Y do objeto2</param>
        /// <returns></returns>
        public static bool isCollideImagens(double x1, double y1, double x2, double y2, int wdth1, int heght1, int wdth2, int height2)
        {

            System.Drawing.Rectangle rect1 = new System.Drawing.Rectangle((int)x1, (int)y1, wdth1, heght1);
            System.Drawing.Rectangle rect2 = new System.Drawing.Rectangle((int)x2, (int)y2, wdth2, height2);


            return rect1.IntersectsWith(rect2);

        }


    }

    /// <summary>
    /// funcoes uteis para jogos: random, clamp,...
    /// </summary>
    public class FunctionsGame
    {
        public FunctionsGame()
        {

        }

        public string GetTextRandom()
        {
            return "ola";
        }

        /// <summary>
        /// retorna um numero randomico na faixa dos parametros init, e end.
        /// </summary>
        /// <param name="init">menor numero a randomizar.</param>
        /// <param name="end">maior numero a randomizar.</param>
        /// <returns>retorna um inteiro entre [init] e [end].</returns>
        public int RandomInt(int init, int end)
        {
            return new Random().Next(init, end);
        }

        /// <summary>
        /// retorna um numero ponto-flutuante entre 0..1;
        /// </summary>
        /// <returns>retornao um numero entre 0..1.</returns>
        public double RandomDouble()
        {
            return new Random().NextDouble();
        }

        /// <summary>
        /// retorna um numero ponto-flutuante entre [init]..[end].
        /// </summary>
        /// <param name="init">valor minimo.</param>
        /// <param name="end">valor maximo.</param>
        /// <returns>retorna um numero double na faixa dos parametros.</returns>
        public double RandomDouble(double init, double end)
        {
            return new Random().NextDouble() * (end - init) + init;

        }


        /// <summary>
        /// retorna um numero randomico, para uma distribuicao normal.
        /// </summary>
        /// <param name="media">media dos valores da distribuicao normal.</param>
        /// <param name="variancia">variancia da distribuicao normal.</param>
        /// <returns></returns>
        public double RandomDistributionNormal(double media, double variancia)
        {
            double dx = new Random().NextDouble() * variancia * 2 - 1;
            return media + dx;
        }


        /// <summary>
        /// funcao clamp.
        /// </summary>
        /// <param name="min">valor minimo.</param>
        /// <param name="max">valor maximo.</param>
        /// <param name="value">valor currente.</param>
        /// <returns></returns>
        public double ClampDouble(double min, double max, double value)
        {
            return Math.Clamp(value, min, max);
        }

        /// <summary>
        /// funcao clamp.
        /// </summary>
        /// <param name="min">valor minimo.</param>
        /// <param name="max">valor maximo.</param>
        /// <param name="value">valor currente.</param>
        /// <returns></returns>
        public int ClampInt(int min, int max, int value)
        {
            return Math.Clamp(value, min, max);
        }

        /// <summary>
        /// funcao de converte um double para um int.
        /// </summary>
        /// <param name="valor">numero double.</param>
        /// <returns></returns>
        public int ParseToInt(double numero)
        {
            return (int)numero;
        }

        /// <summary>
        /// funcao que converte um int para um double.
        /// </summary>
        /// <param name="numero">numero int.</param>
        /// <returns></returns>
        public double ParseToDouble(int numero)
        {
            return (double)numero;
        }

        /// <summary>
        /// converte um numero para um texto string.
        /// </summary>
        /// <param name="numero">numero a ser convertido.</param>
        /// <returns>retorna um texto string do numero.</returns>
        public string ParseFromIntToString(int numero)
        {
            return numero.ToString();
        }

        /// <summary>
        /// converte um numero para um texto string.
        /// </summary>
        /// <param name="numero">numero ponto-flutuante a ser convertido.</param>
        /// <returns>retorna um texto string representando o numero.</returns>
        public string ParseDoubleIntToString(double numero)
        {
            return numero.ToString();
        }
    }

    

    public class UtilsGame
    {

        private static char aspas = '\u0022';
       
        
       

        /// <summary>
        /// formata textos onde há a ocorrencia de aspas, como um texto de uma entrada de ExpressionBase.
        /// </summary>
        /// <param name="textToFormat"></param>
        /// <returns></returns>
        public static string FormataTexto(string textToFormat)
        {
            if (textToFormat[0] == aspas)
            {
                textToFormat = textToFormat.Substring(1);
            }
            if (textToFormat[textToFormat.Length - 1] == aspas)
            {
                textToFormat = textToFormat.Substring(0, textToFormat.Length - 1);
            }

            return textToFormat;
        }

        



        /// <summary>
        /// classe de importacao de classes do kit de desenvolvimento de games.
        /// </summary>
        public class ImportarClassesKitDesenvolvedor
        {
            List<Classe> classesDoKit = new List<Classe>();
            public ImportarClassesKitDesenvolvedor()
            {
                      
            }

            /// <summary>
            /// importa as classes da biblioteca de games, afim de poder
            /// instanciar objetos de classes importado.
            /// </summary>
            public void KitDevGame()
            {
           
                
                ImportAClass(typeof(TimeReaction));
                ImportAClass(typeof(FunctionsGame));
                ImportAClass(typeof(System.Drawing.Rectangle));
                ImportAClass(typeof(RectangleCollision));

                ImportAClass(typeof(Vector2D));
                ImportAClass(typeof(SFML.Graphics.Color));
                ImportAClass(typeof(LoopGame));
                ImportAClass(typeof(Imagem));
                ImportAClass(typeof(Text));
                ImportAClass(typeof(Input));
                ImportAClass(typeof(Sound));



                if (RepositorioDeClassesOO.Instance().GetClasse("ExpressionBase") != null)
                {
                    RepositorioDeClassesOO.Instance().GetClasse("ExpressionBase").isImport = true;
                    
                    
                }
                else
                {
                    ImportAClass(typeof(ExpressionBase));
                }

            }



            /// <summary>
            /// importa metodos e propriedades, de uma classe da Linguagem Base.
            /// </summary>
            /// <param name="nameClass">nome da classe a importar.</param>
            private void ImportAClass(Type classImportada)
            {

                string nameClass = classImportada.Name;


                // registra a classe no repositorio de classes.
                Classe classeEmRepositorio = new Classe();
                classeEmRepositorio.nome = classImportada.Name;
                RepositorioDeClassesOO.Instance().RegistraUmaClasse(classeEmRepositorio);


          
                // importa TODOS METODOS da classe importada.
                LoadMethods(classImportada.Name, classImportada);







                // importa TODO CONSTRUTORES da classe importada.
                ConstructorInfo[] construtores = classImportada.GetConstructors();
                if ((construtores!=null) && (construtores.Length > 0))
                {
                    for (int x=0;x< construtores.Length;x++)
                    {
                        ParameterInfo[] parametros = construtores[x].GetParameters();
                        List<Objeto> parametrosOBJETO= new List<Objeto>();  

                        if ((parametros != null) && (parametros.Length > 0))
                        {
                            for (int p = 0; p < parametros.Length; p++)
                            {
                                string nomeParametro = parametros[p].Name;
                                string classParametro = parametros[p].ParameterType.Name;

                                Objeto umParametro = new Objeto("public", classParametro, nomeParametro, null);
                                parametrosOBJETO.Add(umParametro);
                            }
                        }

                        // seta o info do construtor, e seus parametros.
                        Metodo fncConstrutor = new Metodo();
                        fncConstrutor.InfoConstructor = construtores[x];
                        fncConstrutor.parametrosDaFuncao = parametrosOBJETO.ToArray();
                        fncConstrutor.nomeClasse = nameClass;
                      
                        // adiciona o construtor importado, na lista de construtores da classe no repositorio de classes.
                        RepositorioDeClassesOO.Instance().GetClasse(nameClass).construtores.Add(fncConstrutor); 
                    }
                }
               

                // importa TODAS PROPRIEDADES DA CLASSE IMPORTADA.
                PropertyInfo[] propriedades = classImportada.GetProperties();
                if ((propriedades != null) && (propriedades.Length > 0))
                {
                    for (int x = 0; x < propriedades.Length; x++)
                    {
                        string name = propriedades[x].Name;
                        string type = propriedades[x].PropertyType.Name;

                        RepositorioDeClassesOO.Instance().GetClasse(nameClass).propriedades.Add(new Objeto("public", type, name, null));
                    }
                }

              


                // seta a classe importada como isImport=true, SINALIZANDO ao compilador a instanciar objetos importados, E que não são classe estruturais como string, double, int, char...
                RepositorioDeClassesOO.Instance().GetClasse(nameClass).isImport = true;

                // seta a classe para classe estrutural, onde nao é necessário parsear novamente em cenarios de testes com mesmas classes com nomes iguais.
                RepositorioDeClassesOO.Instance().GetClasse(nameClass).isEstructuralClasse = true;


                // importa para as classes basicas da linguagem orquidea.
                LinguagemOrquidea.Instance().adicionaClasse(RepositorioDeClassesOO.Instance().GetClasse(nameClass));
            }
        }



        /// <summary>
        /// registra os metodos da classe importada da Linguagem Base.
        /// </summary>
        /// <param name="nomeClasseBasica">nome da classe.</param>
        /// <param name="classeImportada">Type contendo a classe.</param>
        public static void LoadMethods(string nomeClasseBasica, Type classeImportada)
        {

            MethodInfo[] metodos = classeImportada.GetMethods();
            if (metodos != null)
            {
                for (int x = 0; x < metodos.Length; x++)
                {

                   

                    ParameterInfo[] parametrosImportados = metodos[x].GetParameters();
                    List<Objeto> parametrosObjeto = new List<Objeto>();


                    if (parametrosImportados != null)
                    {

                        /// constroi os parâmetros objetos, dos parâmetros do método importado.
                        for (int i = 0; i < parametrosImportados.Length; i++)
                        {
                            Objeto umObjParametro = new Objeto("private", UtilTokens.Casting(parametrosImportados[i].ParameterType.Name), parametrosImportados[i].Name, null);
                            parametrosObjeto.Add(umObjParametro);
                        }

                        // constroi o metodo importado.
                        Metodo umMetodo = new Metodo(nomeClasseBasica, "public", metodos[x].Name, metodos[x], UtilTokens.Casting(metodos[x].ReturnType.Name), parametrosObjeto.ToArray());
                    
                        // sinaliza que deve incluir na lista de parâmetros do método, o objeto caller (o que chamou o metodo, uma chamada de metodo).
                        umMetodo.isToIncludeCallerIntoParameters = false;

                        /// adiciona o metodo à classe "string" orquidea.
                        RepositorioDeClassesOO.Instance().GetClasse(nomeClasseBasica).GetMetodos().Add(umMetodo);
                    }


                }
            }


        }
    }
    public class GameLibrary
    {
        

        public class Testes: SuiteClasseTestes
        {
            char aspas = '\u0022';
            public Testes():base("testes da classe game library")
            {


               
            
            
            }

            public void TestesDeInput(AssercaoSuiteClasse assercao)
            {
                string pathFile = @"programasTestes\programaInputKeyDown.txt";
                ParserAFile.ExecuteAProgram(pathFile);
            }


            public void TestesJogoSpaceInvaders(AssercaoSuiteClasse assercao)
            {
                string pathFileCodigoJogo = @"programa jogos\SpaceInvaders.txt";
                ParserAFile.ExecuteAProgram(pathFileCodigoJogo);

                System.Environment.Exit(0);
            }


            public void TesteImagensEInput(AssercaoSuiteClasse assercao)
            {
                string pathFileProgram = @"programasTestesJogos\programaOrquideaInputMouse.DAT";
                ParserAFile.ExecuteAProgram(pathFileProgram);
            }
            public void TesteProgramaColisao(AssercaoSuiteClasse assercao)
            {

                string pathFileJogo = @"programasTestesJogos\jogoColisao.DAT";
                ParserAFile.ExecuteAProgram(pathFileJogo);
            }

    
            
     
           
            public void TesteProgramaTextos(AssercaoSuiteClasse assercao)
            {
                string pathFileProgram = @"programasTestesJogos\programaJogoDesenhosDeTextos.DAT";
                ParserAFile.ExecuteAProgram(pathFileProgram);
            }


            


   
            public void TesteJogoOrquideaUnits(AssercaoSuiteClasse assercao)
            {
                string pathFileJogo = @"programasTestesJogos\programaVectorUnits.txt";
                ParserAFile.ExecuteAProgram(pathFileJogo);
            }


            public void TesteClassOrquideaComTeclado(AssercaoSuiteClasse assercao)
            {
                string pathFileJogo = @"programasTestesJogos\jogoEsbocoVectorsEInputDados.DAT";
                ParserAFile.ExecuteAProgram(pathFileJogo);
            }


           




            public void TesteProgramaTeclado(AssercaoSuiteClasse assercao)
            {
                string pathFileProgram = @"programasTestesJogos\programaTeclado.DAT";
                ParserAFile.ExecuteAProgram(pathFileProgram);

            }
       

   

            public void TesteEsbocoCriacaoVector(AssercaoSuiteClasse assercao)
            {
                string pathFile = @"programasTestesJogos\programaEstudoJogos.DAT";
                ParserAFile.ExecuteAProgram(pathFile);
            }




            public void TesteScreenEmOrquidea(AssercaoSuiteClasse assercao)
            {
                string pathFileProgram = @"programasTestesJogos\programaStartSScreenOrquidea.DAT";
                ParserAFile.ExecuteAProgram(pathFileProgram);
            }


            public void TestesFuncoesUtils(AssercaoSuiteClasse assercao)
            {

                string code_0_0_program = "txtHello.ModifyText(newText);";
                string code_0_0_create = "double xPlayer=1.0; Text txtHello= create(" + aspas + "Jogo 6!" + aspas + "); string newText=" + aspas + "Ola" + aspas + ";";

                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_create + code_0_0_program);
                compilador.Compilar();


                ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                programa.Run(compilador.escopo);

            }




        


        

        
            public void TesteProgramaEsbocoJogo1(AssercaoSuiteClasse assercao)
            {
                string pathFile = @"programasTestes\programaJogo_HelloWorld.txt";
                ParserAFile.ExecuteAProgram(pathFile);


            }


            public void TesteLoopComCodigoOrquideaEExpressionBase(AssercaoSuiteClasse assercao)
            {
                try
                {
                    string textDesenho = "string textDraw= Prompt.sWrite(" + aspas + "draw..." + aspas + ");";
                    string textAtualizacao = "string textUpdate= Prompt.sWrite(" + aspas + "update..." + aspas + ");";

                    string codigoJogo = "ObjectGame obj1 = create(textUpdate);" +
                        "LoopGame loop = create();" + "loop.AddObjectGame(obj1);" + "loop.Run();";
                                                            


                

                    ProcessadorDeID compilador= new ProcessadorDeID(textDesenho+textAtualizacao+codigoJogo);
                    compilador.Compilar();

                    assercao.IsTrue(compilador.GetInstrucoes().Count > 0, textDesenho + "  " + textAtualizacao + "  " + codigoJogo);

                    Escopo.escopoCURRENT = compilador.escopo;
                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    System.Environment.Exit(5);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }

            }


            public void TesteLoopComCodigoOrquidea(AssercaoSuiteClasse assercao)
            {
                try
                {
                    UtilsGame.ImportarClassesKitDesenvolvedor kit = new UtilsGame.ImportarClassesKitDesenvolvedor();
                    kit.KitDevGame();

                    string code_0_0 = "LoopGame loop= create(); loop.Run();";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                    compilador.Compilar();

                    assercao.IsTrue(compilador.GetInstrucoes().Count == 2);

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);

                    
                }
                catch(Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }

            }

  
            public void TesteExpressionBase2(AssercaoSuiteClasse assercao)
            {
                string code_0_0 = "Prompt.sWrite(" + aspas + "hello games worlds" + aspas + ")";
                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                compilador.Compilar();

                Escopo.escopoCURRENT = compilador.escopo;

                ExpressionBase exprss_0_0 = new ExpressionBase();
                exprss_0_0.Compile(code_0_0);

            }

            public void TesteFuncoesString(AssercaoSuiteClasse assercao)
            {
                /// TESTE COM LOOP INFINITO. PARA VALIDAR FUNCOES STRING DE [FunctionsGame].
                string pathFileGame = @"programasTestesJogos\programaTextosFuncoesString.DAT";
                ParserAFile.ExecuteAProgram(pathFileGame);
            }


            public void TestesProgramVMCreateInstrucao(AssercaoSuiteClasse assercao)
            {
                UtilsGame.ImportarClassesKitDesenvolvedor kit = new UtilsGame.ImportarClassesKitDesenvolvedor();
                kit.KitDevGame();

                
                string nomeImagem = @"batship (1).png";
                string code_0_0 = "Imagem obj= create(" + aspas + nomeImagem + aspas + ")";

                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                compilador.Compilar();

                try
                {
                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("obj", compilador.escopo).valor != null, code_0_0);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
                
            }

            public void TestesKitDesenvolvedor(AssercaoSuiteClasse assercao)
            {
                try
                {
                    UtilsGame.ImportarClassesKitDesenvolvedor kit = new UtilsGame.ImportarClassesKitDesenvolvedor();
                    kit.KitDevGame();

                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasse("Imagem") != null, "TESTE DE KIT GAME");
                }
                catch (Exception ex)
                {

                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }



            }

            public void TestesInstanciacaoObjetoImportado(AssercaoSuiteClasse assercao)
            {
                try
                {

                    UtilsGame.ImportarClassesKitDesenvolvedor kit = new UtilsGame.ImportarClassesKitDesenvolvedor();
                    kit.KitDevGame();

                    char aspas = '\u0022';
                    string nomeImagem = @"batship (1).png";
                    string code_0_0 = "Imagem obj= create(" + aspas + nomeImagem + aspas + ")";

                    ProcessadorDeID compilador= new ProcessadorDeID(code_0_0);
                    compilador.Compilar();

                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("obj", compilador.escopo) != null, code_0_0);
                }
                catch (Exception ex)
                {

                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }


            }
    
        }
    }

   
}
