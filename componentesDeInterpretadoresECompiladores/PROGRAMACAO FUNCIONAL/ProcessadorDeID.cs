using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

using parser;
using Util;
using parser.ProgramacaoOrentadaAObjetos;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

using System.Globalization;
using parser.textoFormatado;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using MathNet.Numerics.Interpolation;
using ModuloTESTES;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Metadata;

namespace parser
{

    /// <summary>
    /// sistema de compilação de código orquidea.
    /// </summary>
    public class ProcessadorDeID : BuildInstrucoes
    {

        /// <summary>
        /// codigo de um processador id. lista de tokens para compilar
        /// </summary>
        public List<string> codigo;
        /// <summary>
        /// escopo do processador.
        /// </summary>
        public Escopo escopo;
        /// <summary>
        /// linguagem contendo classes, métodos, propriedades, funções, variáveis, e operadores.
        /// </summary>
        private static LinguagemOrquidea lng = null;


        /// <summary>
        /// guarda a linha de compilação com falha.
        /// </summary>
        public static int lineInCompilation = 0;



        /// <summary>
        ///  assinatura de uma função compiladora de uma instrucao da linguagem.
        /// </summary>
        /// <param name="sequencia">texto contendo uma sequencia possivel de commpilar.</param>
        /// <param name="escopo">contexto onde o texto está.</param>
        /// <returns></returns>
        public delegate List<Instrucao> MetodoTratador(UmaSequenciaID sequencia, Escopo escopo);


        /// <summary>
        /// lista de métodos tratadores para ordenação.
        /// </summary>
        public static List<MetodoTratadorOrdenacao> tratadores = new List<MetodoTratadorOrdenacao>();






        /// <summary>
        /// lista de instruções construidas na compilação.
        /// </summary>
        public List<Instrucao> instrucoes = new List<Instrucao>();


        // guarda as sequencias já mapeadas e resumidas.
        private static List<List<string>> sequenciasJaMapeadas = new List<List<string>>(); 


        // tipos de acesso a propriedades e metodos.
        private static List<string> acessorsValidos = new List<string>() { "public", "private", "protected" };



        /// <summary>
        /// lista de identifcadores de instanciacao de wrappers objects.
        /// </summary>
        private static List<string> tokensIdenficacaoWrappers = null;



        public List<Instrucao> GetInstrucoes()
        {
            return this.instrucoes;
        }

        public List<MetodoTratadorOrdenacao> GetHandlers()
        {
            return ProcessadorDeID.tratadores;
        }



        /// <summary>
        /// compila o codigo, na linha de codigo [code], para instruções orquidea.
        /// </summary>
        /// <param name="code">linha de codigo contendo todos os tokens do programa VM</param>
        public ProcessadorDeID(string code):base(new List<string>())
        {
        
            this.codigo = new Tokens(code).GetTokens();

            // inicializa os headers.
            Expressao.InitHeaders(code);
            // inicializa o mapeamento de sequencias id resumidas, afim de processamento de sequencias e instruções.
            this.InitMapeamentoProgramacaoEstruturada();
            // carrega os nomes que identificam uma instanciacao id.
            this.GetIdentifyWrappers();
            if (lng == null)
                lng = LinguagemOrquidea.Instance();

      
            this.instrucoes = new List<Instrucao>();
            this.escopo = new Escopo(codigo);
       
        }


        /// <summary>
        /// compila o codigo, da lista de tokens, para instruções orquidea.
        /// </summary>
        /// <param name="code">lista de tokens do programa VM.</param>
        public ProcessadorDeID(List<string> code) : base(code)
        {
         
            // inicializa os headers.
            Expressao.InitHeaders(code);
            // inicializa as sequencias id mapeadas resumidas.
            this.InitMapeamentoProgramacaoEstruturada();
            // inicializa a lista de identificadoes de instanciacao wrappers.
            this.GetIdentifyWrappers();


            // instancia a linguagem padrão..
            if (lng == null)
            {
                lng = LinguagemOrquidea.Instance();
            }
                

            this.instrucoes = new List<Instrucao>();
            codigo = new List<string>();
            codigo.AddRange(code);

            this.escopo = new Escopo(code); // obtém um escopo para o processador de sequencias ID.
            

        } 



        /// <summary>
        /// obtem uma lista de nomes de idenficação para uma instanciação wrapper.
        /// </summary>
        /// <returns></returns>
        private void GetIdentifyWrappers()
        {
            if (ProcessadorDeID.tokensIdenficacaoWrappers == null)
            {
                ProcessadorDeID.tokensIdenficacaoWrappers= new List<string>();
                List<WrapperData> wrappers = WrapperData.GetAllTypeWrappers();
                foreach (WrapperData w in wrappers)
                {
                    tokensIdenficacaoWrappers.AddRange(w.getNamesIDWrapperData());
                }

               
            }
            
        }

        
        
        /// <summary>
        /// adiciona a uma lista, um handler, que faz o processamento se o pattern parametro for encontrado.
        /// </summary>
        /// <param name="umTratador">função handler que fará o processamento.</param>
        /// <param name="patternResumedOfSequence">pattern para acionar o handler, se encontrado.</param>
        public static void LoadHandler(MetodoTratador umTratador, string patternResumedOfSequence)
        {
            tratadores.Add(new MetodoTratadorOrdenacao(umTratador, patternResumedOfSequence));
            
        } 


        /// <summary>
        /// carrega os handlers para cada tipo de instrução.
        /// </summary>
        private void InitMapeamentoProgramacaoEstruturada()
        {
            if ((tratadores == null) || (tratadores.Count == 0))
            {



                // comando de  aspecto. 
                string rgx_definicaoAspecto = "aspecto";





          
                // comandos de importação de classes da classe base.
                string rgx_Importer = "importer";
                string rgx_moduleLibrary = "module";

                // comandos programacao estruturada.
                string rgx_DefinicaoInstrucaoCasesOfUse = "casesOfUse";
                string rgx_CreateNewObject = "create";
                string rgx_ConstrutorUp = "construtorUP";
                string rgx_tokensInstrucaoWhile = "while";
                string rgx_tokensInstrucaoFor = "for";
                string rgx_DefinicaoInsInstrucaoIfComOuSemElse = "if else";
                string rgx_DefinicaoInstrucaoSetVar = "SetVar";
                string rgx_DefinicaoInstrucaoGetObjeto = "GetObjeto";
                string rgx_DefinicaoInstrucaoReturn = "return";
                //____________________________________________________________________________________________________________________________
                // CARREGA OS METODO TRATADORES E AS SEQUENCIAS DE ID ASSOCIADAS.

                





                // SEQUENCIAS INTEROPABILIDADE:
                LoadHandler(BuildInstrucaoImporter, rgx_Importer);


                // sequencias de programacao estruturada.
                LoadHandler(BuildInstrucaoWhile, rgx_tokensInstrucaoWhile);
                LoadHandler(BuildInstrucaoFor, rgx_tokensInstrucaoFor);
                LoadHandler(BuildInstrucaoIFsComOuSemElse, rgx_DefinicaoInsInstrucaoIfComOuSemElse);
                LoadHandler(BuildInstrucaoReturn, rgx_DefinicaoInstrucaoReturn);
                LoadHandler(BuildInstrucaoCreate, rgx_CreateNewObject);
                LoadHandler(BuildInstrucaoGetObjeto, rgx_DefinicaoInstrucaoGetObjeto);
                LoadHandler(BuildInstrucaoSetVar, rgx_DefinicaoInstrucaoSetVar);
                LoadHandler(BuildInstrucaoCasesOfUse, rgx_DefinicaoInstrucaoCasesOfUse);
                LoadHandler(BuildInstrucaoConstrutorUP, rgx_ConstrutorUp);
                LoadHandler(BuildInstrucaoModule, rgx_moduleLibrary);


                // programacao orientada a aspectos.
                LoadHandler(BuildDefinicaoDeAspecto, rgx_definicaoAspecto);



                // ordena a lista de métodos tratadores, pelo cumprimento de seus testes de sequencias ID.            
                ProcessadorDeID.MetodoTratadorOrdenacao.ComparerMetodosTratador comparer = new MetodoTratadorOrdenacao.ComparerMetodosTratador();
                if (tratadores == null)
                {
                    tratadores = new List<MetodoTratadorOrdenacao>();
                }
                tratadores.Sort(comparer);
             

            } // if tratadores

        } // InitMapeamento()


        /// <summary>
        /// faz uma varredura nas classes, encontrando objetos e/ou metodos 
        /// monitorados, para inserir  aspectos no codigo.
        /// </summary>
        private void ProcessaAspectos()
        {
            
            if (lng.Aspectos != null)
            {
                for (int x = 0; x < lng.Aspectos.Count; x++)
                    lng.Aspectos[x].AnaliseAspecto(escopo);
            }
        }

      
        /// <summary>
        /// compila o codigo carregado no construtor.
        /// </summary>
        public void Compilar()
        {

            // obtem os tokens do codigo para compilar.
            List<string> tokens = new Tokens(codigo).GetTokens();


            // calcula uma vez os headers, para a compilação total do codigo. os headers auxilia no processamento de classes,
            // evitando instanciação posterior a atribuição de propriedades, metodos, operadores.
            InitExpressao(tokens);

            
            // faz a compilação dos tokens da entrada do processador.
            this.CompileEscopos(this.escopo, tokens); 

            // faz a compilação dos aspectos.
            this.ProcessaAspectos();

           
        }


        /// <summary>
        /// faz a compilação da lista de tokens, transformando em instruções.
        /// </summary>
        /// <param name="escopo">contexto onde os tokens estão.</param>
        /// <param name="tokens">lista de tokens para compilação.</param>
        private void CompileEscopos(Escopo escopo, List<string> tokens)
        {

            // se 1a. compilacao,constroi o escopo raiz.
            if (Escopo.escopoROOT == null)
            {
                Escopo.escopoROOT = escopo;
                Escopo.escopoCURRENT = escopo;
            }



            int umToken = 0;
            while (umToken < tokens.Count)
            {

                UmaSequenciaID sequenciaCurrente = null;

                if (((umToken + 1) < tokens.Count) &&
                    (acessorsValidos.Find(k => k.Equals(tokens[umToken])) != null) &&
                    ((tokens[umToken + 1].Equals("class")) ||
                    (tokens[umToken + 1].Equals("interface"))))
                {
                    // PROCESSAMENTO DE INTERFACE.
                    string classeOuInterface = tokens[umToken + 1];

                    try
                    {
                        if (classeOuInterface == "interface")
                        {

                            List<string> tokensDaInterface = UtilTokens.GetCodigoEntreOperadores(umToken, "{", "}", tokens);


                            ExtratoresOO extratorDeClasses = new ExtratoresOO(tokensDaInterface, this.escopo);
                            Classe umaInterface = extratorDeClasses.ExtraiUmaInterface();
                            Escopo.nomeClasseCurrente = umaInterface.GetNome();
                            
                            if (umaInterface != null)
                            {

                                umToken += umaInterface.tokensDaClasse.Count;
                                continue;
                            }

                        }
                        else
                        // PROCESSAMENTO DE CLASSE.
                        if (classeOuInterface == "class")
                        {



                            List<string> tokensDaClasse = UtilTokens.GetCodigoEntreOperadores(umToken, "{", "}", tokens);
                            ExtratoresOO extratorDeClasses = new ExtratoresOO(tokensDaClasse, this.escopo);


                            Classe umaClasse = extratorDeClasses.ExtaiUmaClasse(Classe.tipoBluePrint.EH_CLASSE);
                            if (umaClasse != null)
                            {
                                

                                umToken += umaClasse.tokensDaClasse.Count;

                                // retransmite erros na extracao da classe, para a mensagem de erros do escopo.
                                if (extratorDeClasses.MsgErros.Count > 0)
                                {
                                    this.escopo.GetMsgErros().AddRange(extratorDeClasses.MsgErros);
                                }
                                     
                                continue;

                            } // if

                        }
                    }
                    catch
                    {
                        UtilTokens.WriteAErrorMensage("sintax error in " +classeOuInterface+"  class, or interface", tokens, escopo);
                        return;
                    }
                }
                else
                if (lng.IsID(tokens[umToken]) || (lng.isTermoChave(tokens[umToken])))
                {


                    try
                    {
                        

                        ProcessadorDeID.lineInCompilation += 1;

                        // obtem a sequencia  seguinte.
                        sequenciaCurrente = UmaSequenciaID.ObtemUmaSequenciaID(umToken, tokens, codigo); 

                       

                        if ((sequenciaCurrente != null) && (sequenciaCurrente.tokens.Count == 1) && (sequenciaCurrente.tokens[0].Trim(' ') == ";"))
                        {
                            umToken += 1;
                            continue;
                        }

                        if (sequenciaCurrente == null)
                        {
                            // continua o processamento, para verificar se há mais erros no codigo orquidea.
                            UtilTokens.WriteAErrorMensage("sequence error, tokens sequence not match in a one of instructions: " + Utils.OneLineTokens(sequenciaCurrente.tokens), sequenciaCurrente.tokens, escopo);
                        }



                        // obtém o indice de metodo tratador.
                        MatchSequencias(sequenciaCurrente, escopo);






                        if (sequenciaCurrente.indexHandler == -1)
                        {

                            try
                            {
                                // instrucao processamento de expressao: expressoes chamada de metodo, propriedades aninhadas, create/set/get wrappers object, atribuicao, instanciacao de objeto, etc...
                                List<Instrucao> instrucaoEXPRESSAO = BuildExpressaoValida(sequenciaCurrente, escopo);
                                if (instrucaoEXPRESSAO != null)
                                {
                                    // a sequencia id é uma expressao correta, processa e adiciona a instrucao de expressao correta.
                                    this.instrucoes.AddRange(instrucaoEXPRESSAO);
                                    umToken += sequenciaCurrente.tokens.Count;
                                    continue;
                                }
                                else
                                {
                                    // trata de problemas de sintaxe da sequencia currente, emitindo uma mensagem de erro.
                                    UtilTokens.WriteAErrorMensage("sequence not match to one of instructions possible: " + Utils.OneLineTokens(sequenciaCurrente.tokens) + ". ", sequenciaCurrente.tokens, escopo);

                                    // continua, para capturar mais erros em outras sequencias currente.   
                                    umToken += sequenciaCurrente.tokens.Count;
                                    continue;
                                }

                            } catch (Exception ex)
                            {
                                string msgError = ex.Message;
                                UtilTokens.WriteAErrorMensage("sequence:" + Utils.OneLineTokens(sequenciaCurrente.tokens) + " sintax error.", sequenciaCurrente.tokens, escopo);
                                return;
                            }


                        }
                        else
                        if (sequenciaCurrente.indexHandler != -1)
                        {

                            try
                            {
                                // chamada do método tratador para processar a costrução de escopos, da sequencia de entrada.
                                List<Instrucao> instrucaoTratada = tratadores[sequenciaCurrente.indexHandler].metodo(sequenciaCurrente, escopo);
                                if (instrucaoTratada != null)
                                {
                                    for (int i = 0; i < instrucaoTratada.Count; i++)
                                    {
                                        if ((instrucaoTratada[i] != null) && (instrucaoTratada[i].code != -1))
                                        {
                                            this.instrucoes.Add(instrucaoTratada[i]);
                                        }
                                        else
                                        {
                                            // a sequencia id pode ser uma expressao correta, há build para expressoes corretas.
                                            List<Instrucao> instrucaoExpressaoCorreta = BuildExpressaoValida(sequenciaCurrente, escopo); 
                                            if (instrucaoExpressaoCorreta != null)
                                            {
                                                for (int t = 0; t < instrucaoExpressaoCorreta.Count; t++)
                                                {
                                                    if ((instrucaoExpressaoCorreta[t] != null) && (instrucaoExpressaoCorreta[t].code != -1))
                                                    {
                                                        // a sequencia id é uma expressao correta, processa e adiciona a instrucao de expressao correta.
                                                        this.instrucoes.Add(instrucaoExpressaoCorreta[i]); 

                                                    }
                                                }
                                                umToken += sequenciaCurrente.tokens.Count;
                                                continue;
                                            }
                                            else
                                            {
                                                UtilTokens.WriteAErrorMensage("tokens sequence: " + Utils.OneLineTokens(sequenciaCurrente.tokens) + " not match. ", sequenciaCurrente.tokens, escopo);
                                                return;
                                            }


                                        }

                                    }
                                }
                                else
                                if (instrucaoTratada == null)
                                {
                                }
                                // atualiza o iterator de tokens, consumindo os tokens que foram utilizados no processamento da seuencia id currente.
                                umToken += sequenciaCurrente.tokens.Count; 
                                continue;

                            }
                            catch (Exception ex)
                            {
                                UtilTokens.WriteAErrorMensage("sintax error in:" + Utils.OneLineTokens(sequenciaCurrente.tokens)+" message error:  "+ex.Message, sequenciaCurrente.tokens, escopo);
                                return;
                            }
                        } // if tokensSequenciaOriginais.Count>0
                    }
                    catch(Exception ex)
                    {
                        LoggerTests.AddMessage("fatal error:   " + ex.Message + " in sequence:  " + Utils.OneLineTokens(sequenciaCurrente.tokens));
                        
                                              
                    }
                } // if linguagem.VerificaSeEID()
                umToken++;
            } // while

        }// CompileEcopos()

        /// <summary>
        /// inicializa a compilação.
        /// </summary>
        /// <param name="tokensClasses"></param>
        private static void InitExpressao(List<string> tokensClasses)
        {
            
            if (SystemInit.isResetLoadClasseCode)
            {
                SystemInit.isResetLoadClasseCode= false;
                // constroi os headers de Expressao.
                Expressao.headers = new FileHeader();
                List<Classe> classesDaLinguagem = LinguagemOrquidea.Instance().GetClasses();
                List<HeaderClass> headersDaLinguagem = new List<HeaderClass>();
                Expressao.headers.ExtractHeadersClassFromClassesOrquidea(classesDaLinguagem, headersDaLinguagem);


                Expressao.headers.ExtractCabecalhoDeClasses(tokensClasses);
                
            }
        }







        /// <summary>
        /// encontra um indice de handler que fará a compilação da sequencia de tokens parametro.
        /// </summary>
        /// <param name="sequencia">sequencia pre-agrupada, com tokens relacionados.</param>
        /// <param name="escopo">contexto onde a sequencia está.</param>
        public void MatchSequencias(UmaSequenciaID sequencia, Escopo escopo)
        {

            for (int seqMapeada = 0; seqMapeada < tratadores.Count; seqMapeada++)
            {
                if (sequencia.tokens.IndexOf("{") != -1)
                {
                    if ((sequencia.tokens.Contains(tratadores[seqMapeada].tokens[0])) &&
                        (sequencia.tokens.IndexOf(tratadores[seqMapeada].tokens[0]) < sequencia.tokens.IndexOf("{"))) 
                    {
                     
                        sequencia.indexHandler = seqMapeada;
                        MatchBlocos(sequencia, escopo);
                        return;
                    }
    
                }
                else
                {
                    if (sequencia.tokens.Contains(tratadores[seqMapeada].tokens[0])) 
                    {
                        sequencia.indexHandler = seqMapeada;
                        MatchBlocos(sequencia, escopo);
                        return;
                    }
                 
                }

            
            } // seqMapeada

            // adiciona o indice -1, pois não encontrou uma sequencia mapeada, nem expressão complexa.
            sequencia.indexHandler = -1;

        }





        /// <summary>
        /// a ideia deste método é identificar blocos, e colocar os tokens de blocos, na sequencia de entrada.
        /// </summary>
        /// <param name="sequencia">sequencia a verificar.</param>
        /// <param name="escopo">contexto onde a sequencia está.</param>
        private static void MatchBlocos(UmaSequenciaID sequencia, Escopo escopo)
        {
            int indexSearchBlocks = sequencia.tokens.IndexOf("{");

            while (indexSearchBlocks != -1)
            {
                // retira um bloco a partir dos tokens sem modificações (originais).
                List<string> umBloco = UtilTokens.GetCodigoEntreOperadores(indexSearchBlocks, "{", "}", sequencia.tokens);
                // encontrou um bloco de tokens, adiciona à sequencia de entrada.
                if ((umBloco != null) && (umBloco.Count > 0))
                {
                    umBloco.RemoveAt(0);
                    umBloco.RemoveAt(umBloco.Count - 1);
                    sequencia.sequenciasDeBlocos.Add(new List<UmaSequenciaID>() { new UmaSequenciaID(umBloco.ToArray(), escopo.codigo) });
                } // if
                indexSearchBlocks = sequencia.tokens.IndexOf("{", indexSearchBlocks + 1);
            } // while
        } // MatchBlocos()


       
        

        /// <summary>
        /// constroi a instrucao [ExpressaoValida], que não contem comandos estruturados, como atribuicao, chamadas de metodo, operacoes
        /// aritmeticas, etc...
        /// </summary>
        /// <param name="sequencia">sequencia a processar.</param>
        /// <param name="escopo">contexto onde a sequencia está.</param>
        /// <returns>retorna instruções [ExpressaoValida], como resultado do processamento.</returns>
        protected List<Instrucao> BuildExpressaoValida(UmaSequenciaID sequencia, Escopo escopo)
        {

            

            if (sequencia.tokens == null)
            {
                return null;
            }
            Expressao expressaoCorreta = new Expressao(sequencia.tokens.ToArray(), escopo);
            List<Instrucao> instrucoesCORRETA = new List<Instrucao>();
            if ((expressaoCorreta != null) && (expressaoCorreta.Elementos.Count > 0))
            {
                int indexFinalExpressaoEsperada = expressaoCorreta.tokens.IndexOf(";");
                // caso que não há multiplas expressoes independentes.
                if (indexFinalExpressaoEsperada + 1 >= expressaoCorreta.tokens.Count - 1) 
                {
                    Instrucao umaInstrucaoUnica = new Instrucao(ProgramaEmVM.codeExpressionValid, new List<Expressao>(){ expressaoCorreta }, new List<List<Instrucao>>());
                    instrucoesCORRETA.Add(umaInstrucaoUnica);
                    return instrucoesCORRETA;
                }
                if ((expressaoCorreta != null) && (expressaoCorreta.Elementos != null) && (expressaoCorreta.Elementos.Count > 1)) 
                {
                    // caso que HÀ multiplas expressoes independentes.
                    for (int i = 0;i<expressaoCorreta.Elementos.Count;i++)
                    {
                        if (expressaoCorreta.Elementos[i] != null)
                        {
                            Instrucao umaInstrucao = new Instrucao(ProgramaEmVM.codeExpressionValid, new List<Expressao>() { expressaoCorreta.Elementos[i] }, new List<List<Instrucao>>());
                            instrucoesCORRETA.Add(umaInstrucao);
                        }
                    }
                    return instrucoesCORRETA;
                }
                else
                {
                    // cria a instrucao para a expressao correta.
                    Instrucao instrucaoExpressaoCorreta = new Instrucao(ProgramaEmVM.codeExpressionValid, new List<Expressao>() { expressaoCorreta }, new List<List<Instrucao>>());
                    return new List<Instrucao>() { instrucaoExpressaoCorreta };
                }
                
            }
            else
            {
                UtilTokens.WriteAErrorMensage("instruction ValidExpress,  expression in bad format: " + UtilString.UneLinhasLista(sequencia.tokens), sequencia.tokens, escopo);
                return null;
            }
            
        }

  





        /// <summary>
        /// obtem um valor numero. LEGADO.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="expressaoNumero"></param>
        /// <param name="escopo"></param>
        internal static void SetValorNumero(Objeto v, Expressao expressaoNumero, Escopo escopo)
        {
            if (expressaoNumero == null)
                return;

            string possivelNumero = expressaoNumero.ToString().Replace(" ", "");

            if (Expressao.Instance.IsTipoInteiro(possivelNumero))
                v.SetValorObjeto(int.Parse(possivelNumero)); // seta o valor previamente, pois em modo de depuracao, é necessario este valor. Em um programa, a atribuicao é feito pela instrucao atribuicao.
            else
            if (Expressao.Instance.IsTipoFloat(possivelNumero))
                v.SetValorObjeto(float.Parse(possivelNumero)); // seta o valor previamente, pois em modo de depuracao, é necessario este valor. Em um programa, a atribuicao é feito pela instrucao atribuicao.
            else
            if (Expressao.Instance.IsTipoDouble(possivelNumero))
                v.SetValorObjeto(double.Parse(possivelNumero));
        }

   
 


        /// <summary>
        /// constroi uma definição de aspecto.
        /// </summary>
        /// <param name="sequencia">sequencia gatilho.</param>
        /// <param name="escopo">contexto geral de sequencias.</param>
        /// <returns></returns>
        protected List<Instrucao> BuildDefinicaoDeAspecto(UmaSequenciaID sequencia, Escopo escopo)
        {
            /// template: aspecto NameId typeInsertId (TipoOjbeto:string, string, NomeMetodo: string ) { funcaoCorte(Objeto x){}}.
            int indexNameAspect = 1;
            string nomeAspecto = sequencia.tokens[indexNameAspect];

            int indexTypeInsert = sequencia.tokens.IndexOf(nomeAspecto) + 1;
            List<string> tiposInsercao = new List<string>() { "before", "after", "all" };
            if (tiposInsercao.IndexOf(sequencia.tokens[indexTypeInsert]) == -1)
            {
                UtilTokens.WriteAErrorMensage("instruction aspect, insertion type error:  before, after ou all", sequencia.tokens, escopo);
                return null;
            }

            string typeInserction = sequencia.tokens[indexTypeInsert];

            int indexStartInterface = sequencia.tokens.IndexOf("(");
            if (indexStartInterface == -1)
            {
                UtilTokens.WriteAErrorMensage("instruction aspect, sintax error.", sequencia.tokens, escopo);
                return null;
            }
            List<string> tokensInterface = UtilTokens.GetCodigoEntreOperadores(indexStartInterface, "(", ")", sequencia.tokens);
            if ((tokensInterface == null) || (tokensInterface.Count == 0))
            {
                UtilTokens.WriteAErrorMensage("instruction aspect: sintax error, parameter not build right", sequencia.tokens, escopo);
                return null;
            }


            int indexTypeObjectName = tokensInterface.IndexOf("(") + 1;
            if (indexTypeObjectName < 0)
            {
                UtilTokens.WriteAErrorMensage("instruction aspect: sintax error, parameter not build right", sequencia.tokens, escopo);
                return null;
            }


            string tipoObjetoAMonitorar = tokensInterface[indexTypeObjectName];

            string metodoAMonitorar = null;

            int indexMethodName = indexTypeObjectName + 2; // +1 do typeObject, +1 do operador virgula.
            if ((indexMethodName >= 2) && (indexMethodName < tokensInterface.Count))
                metodoAMonitorar = tokensInterface[indexMethodName];



            int indexStartInstructionsAspect = sequencia.tokens.IndexOf("{");
            if (indexStartInstructionsAspect == -1)
            {
                UtilTokens.WriteAErrorMensage("instruction aspect: sintax error, not block of instruction, defined.", sequencia.tokens, escopo);
                return null;
            }


            List<string> tokensDaFuncaoCorte = UtilTokens.GetCodigoEntreOperadores(indexStartInstructionsAspect, "{", "}", sequencia.tokens);
            if ((tokensDaFuncaoCorte == null) || (tokensDaFuncaoCorte.Count == 0))
            {
                UtilTokens.WriteAErrorMensage("instruction aspect: sintax error, not block of instruction, defined", sequencia.tokens, escopo);
                return null;
            }
            tokensDaFuncaoCorte.RemoveAt(0);
            tokensDaFuncaoCorte.RemoveAt(tokensDaFuncaoCorte.Count - 1);




            ProcessadorDeID processador = new ProcessadorDeID(tokensDaFuncaoCorte);
            processador.Compilar();

            Metodo funcaoCorte = null;
            if ((processador.escopo.tabela.GetFuncoes() != null) && (processador.escopo.tabela.GetFuncoes().Count == 1))
            {
                funcaoCorte = processador.escopo.tabela.GetFuncoes()[0];
                if ((funcaoCorte.parametrosDaFuncao == null) || (funcaoCorte.parametrosDaFuncao.Length != 1))
                {
                    UtilTokens.WriteAErrorMensage("the cut funcion must be only one parameter, and a object type,  "+ Utils.OneLineTokens(sequencia.tokens)+".", sequencia.tokens, escopo);
                    return null;
                }
            }
            Random rnd = new Random();
            List<Expressao> expressoesDaInstrucao = new List<Expressao>();
            Objeto obj_caller = new Objeto("public", funcaoCorte.nomeClasse, "objTemp" + rnd.Next(100000), null);
            expressoesDaInstrucao.Add(new ExpressaoChamadaDeMetodo(obj_caller, funcaoCorte, new List<Expressao>()));
            expressoesDaInstrucao.Add(new ExpressaoElemento(tipoObjetoAMonitorar));


            Aspecto aspecto = new Aspecto(nomeAspecto, tipoObjetoAMonitorar, metodoAMonitorar, funcaoCorte, escopo, Aspecto.TypeAlgoritmInsertion.ByObject, typeInserction);
            lng.Aspectos.Add(aspecto);

            return new List<Instrucao>() { new Instrucao(-1, null, null) };

        }

   
        /// <summary>
        /// classe para handlers de instrucoes de comandos de programação estruturada.
        /// </summary>
        public class MetodoTratadorOrdenacao
        {
            public MetodoTratador metodo 
            {
                get;
                set;
            }


            /// <summary>
            /// tokens mapeados.
            /// </summary>
            public List<string> tokens = new List<string>();


            /// <summary>
            /// pattern de identificacao de sequencias mapeadas.
            /// </summary>
            public string patterResumed;


            /// <summary>
            /// regex de sequecia. LEGADO.
            /// </summary>
            public Regex regex_sequencia;
            
            /// <summary>
            /// texto expressao. LEGADO.
            /// </summary>
            private static TextExpression textExprss;

            /// <summary>
            /// construtor.
            /// </summary>
            /// <param name="umMetodo">uma handler para uma sequencia.</param>
            /// <param name="patternResumedDaSequencia"> pattern de sequencia</param>
            public MetodoTratadorOrdenacao(MetodoTratador umMetodo, string patternResumedDaSequencia)
            {
                if (textExprss == null)
                    textExprss = new TextExpression();
                this.metodo = umMetodo;

                this.patterResumed = textExprss.FormaPatternResumed(patternResumedDaSequencia);
                this.regex_sequencia = new Regex(textExprss.FormaExpressaoRegularGenerica(this.patterResumed));
                this.tokens = new Tokens(patternResumedDaSequencia).GetTokens();


            }
            /// <summary>
            /// retorna um texto identificacao do metodo tratador.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                string str = "";
                str += patterResumed;
                return str;
            }

           
            /// <summary>
            /// ordena decrescentemente pelo cumprimento da sequencia do metodo tratador,
            /// e priorizando sequencias que contem termos-chaves de comando.
            /// </summary>
            public class ComparerMetodosTratador : IComparer<MetodoTratadorOrdenacao>
            {
                private static List<string> termos_chaves=new List<string>();

                public ComparerMetodosTratador()
                {
                    if (termos_chaves == null)
                    {
                        termos_chaves = TextExpression.GetTodosTermosChavesIniciais();
                    }
                    termos_chaves.Add(".");    
                }

                private bool ContainsTermoChave(MetodoTratadorOrdenacao x)
                {
                    List<string> tokens = x.tokens;
                    for (int i = 0; i < tokens.Count; i++)
                    {
                        if (termos_chaves.IndexOf(tokens[i]) != -1)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                public int Compare(MetodoTratadorOrdenacao ?x, MetodoTratadorOrdenacao ?y)
                {
                    int c1 = x.patterResumed.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length;
                    int c2 = y.patterResumed.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length;

                    

                    if ((ContainsTermoChave(x)) && (!ContainsTermoChave(y)))
                        return -1;
                    if ((!ContainsTermoChave(x)) && (ContainsTermoChave(y)))
                        return +1;
                  

                    if (c1 > c2)
                        return -1;
                    if (c1 < c2)
                        return +1;
                    return 0;
                } // Compare()
            }
        }

        /// <summary>
        /// verifica se um dos tokens da sequencia é um wrapper object.
        /// </summary>
        /// <param name="sequenciaCurrente">sequencia com tokens.</param>
        /// <param name="escopo">contexto da sequencia.</param>
        /// <returns>[true] se há wrapper object na sequencia.</returns>
        private bool HasWrapperObjectInSequence(UmaSequenciaID sequenciaCurrente, Escopo escopo)
        {

            for (int x = 0; x < sequenciaCurrente.tokens.Count; x++)
            {
                string nomeObjeto = sequenciaCurrente.tokens[x];
                if (escopo.tabela.GetObjeto(nomeObjeto, escopo) != null)
                {
                    if (escopo.tabela.GetObjeto(nomeObjeto, escopo).isWrapperObject)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public new class Testes : SuiteClasseTestes
        {
            

            public Testes() : base("testes para classe ProcessadorID")
            {
            }

            public void TesteIF_Else(AssercaoSuiteClasse assercao)
            {
                string codigo = "int a=1; int b=5; if (a>b){a=3*b;int d=3;} else {a=6; int k=0; a=a+k;}";
                ProcessadorDeID processador = new ProcessadorDeID(codigo);
                processador.Compilar();

                try
                {
                    assercao.IsTrue(
                        processador.instrucoes.Count == 3 &&
                        processador.instrucoes[2].code == ProgramaEmVM.codeIfElse, codigo);


                    assercao.IsTrue
                        (processador.instrucoes[2].blocos[0][0].code == ProgramaEmVM.codeExpressionValid, codigo);


                    assercao.IsTrue(
                        processador.instrucoes[2].blocos[1][0].code == ProgramaEmVM.codeExpressionValid, codigo);


                }
                catch (Exception ex)
                {
                    string codeMessage = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
            }


            public void TesteWhile(AssercaoSuiteClasse assercao)
            {
                string codigo = "int x=1; int dx=5; while (x<=4){dx=dx+1; x=x+1;}";
                ProcessadorDeID processador = new ProcessadorDeID(codigo);
                processador.Compilar();

                try
                {
                    assercao.IsTrue(
                        processador.instrucoes.Count == 3 &&
                        processador.instrucoes[2].code == ProgramaEmVM.codeWhile, codigo);
                }
                catch (Exception ex)
                {
                    string codeMessage = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }

            }






            public void TesteFor(AssercaoSuiteClasse assercao)
            {
                string codigo = "int a=0; int b=5; for (int x=0;x< 3;x++) {a= a+ x;};";

                ProcessadorDeID processador = new ProcessadorDeID(codigo);
                processador.Compilar();

                try
                {
                    assercao.IsTrue(
                    processador.instrucoes[2].expressoes.Count == 3 &&
                    processador.instrucoes[1].expressoes.Count == 1 &&
                    processador.instrucoes[0].expressoes.Count == 1, codigo);
                }
                catch (Exception ex)
                {
                    string codeMessage = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }


            }

            public void TesteGeraErrors(AssercaoSuiteClasse assercao)
            {
                try
                {
                    string code_0_0 = "for(int i=0;i<5;i++){y=1;};";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                    compilador.Compilar();

                    assercao.IsTrue(compilador.escopo.GetMsgErros().Count > 0, code_0_0);
                }
                catch(Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }
            public void TesteAtribuicaoDeValoresEntreVariaveis(AssercaoSuiteClasse assercao)
            {
                string codigo_create = "int x= 1; int y=2;";
                ProcessadorDeID compilador = new ProcessadorDeID(codigo_create);
                compilador.Compilar();
                string codigo_exprss_0_0 = "x=y";
                string codigo_exprss_0_1 = "y=5";

                Expressao expressao_0_0 = new Expressao(codigo_exprss_0_0, compilador.escopo);
                Expressao expressao_0_1 = new Expressao(codigo_exprss_0_1, compilador.escopo);
                EvalExpression eval = new EvalExpression();
                eval.EvalPosOrdem(expressao_0_0, compilador.escopo);
                eval.EvalPosOrdem(expressao_0_1 , compilador.escopo);

                try
                {
                
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("x", compilador.escopo).valor.ToString() == "2", codigo_create);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }


            }

            public void TesteInstanciacaoWrapperObjects(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoClasse1 = "public class classeWrapper { public classWrapper(){}; public int metodoB(){ int[] vetor1[15]; vetor1[0]=1; }};";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasse1);
                compilador.Compilar();
                try
                {
                    List<Metodo> metodos = RepositorioDeClassesOO.Instance().GetClasse("classeWrapper").GetMetodos();
                    assercao.IsTrue(metodos[1].instrucoesFuncao.Count == 2, codigoClasse1);
                    assercao.IsTrue(metodos[1].instrucoesFuncao[0].code == ProgramaEmVM.codeExpressionValid, codigoClasse1);
                    assercao.IsTrue(metodos[1].instrucoesFuncao[1].code == ProgramaEmVM.codeExpressionValid, codigoClasse1);

                }
                catch (Exception e)
                {
                    string msgError = e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
            }

            public void TesteCompilarCorpoDeMetodos(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string codigoClasseA = "public class classeA { public classeA() { int b=6;  };  public int metodoA(){ int x =1; x=x+1;}; }; int a=1; int b=2;";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseA);
                compilador.Compilar();

                try
                {
                    Instrucao instrucaoMetodo1 = RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodos()[0].instrucoesFuncao[0];
                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodos()[0].instrucoesFuncao.Count == 1, codigoClasseA);
                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodos()[1].instrucoesFuncao.Count == 2, codigoClasseA);

                }
                catch (Exception e)
                {
                    string codeMessage = e.Message;
                    assercao.IsTrue(false, "Teste Falhou");
                }



            }

            public void TesteCasesOfUse_2(AssercaoSuiteClasse assercao)
            {
                // sintaxe: casesOfUse id : {( case operador exprss ) : {} (case operador exprss): {}}...
                string codigo = "int b=1; int a=5; casesOfUse b: { (case  < a): { b = 1;}; }";

                // "template: casesOfUse y: { (case < x): { y = 2; }; } ";
                ProcessadorDeID processador = new ProcessadorDeID(codigo);
                processador.Compilar();
                try
                {
                    assercao.IsTrue(
                        processador.instrucoes.Count == 3 &&
                        processador.instrucoes[2].code == ProgramaEmVM.codeCasesOfUse, codigo);
                }
                catch (Exception ex)
                {
                    string codeMessage = ex.Message;
                }

            }




     
            public void TestePromptWrite(AssercaoSuiteClasse assercao)
           {
                SystemInit.InitSystem();
                LinguagemOrquidea lng = LinguagemOrquidea.Instance();
                try
                {
                    Classe prompt= RepositorioDeClassesOO.Instance().GetClasse("Prompt");
                    List<Metodo> fnc = prompt.GetMetodo("xWrite");
                    for (int i=0;i<fnc.Count; i++)
                    {
                        if (fnc[i].parametrosDaFuncao.Length == 2)
                        {
                            assercao.IsTrue(fnc[i].parametrosDaFuncao[1].isMultArgument);
                            return;
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    string codeMsg= ex.Message; 
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
            }



            public void TesteExpressaoValida(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string pathProgramaFatorialRecursivo = "programasTestes\\programaFatorialRecursivo.txt";
                ParserAFile parser = new ParserAFile(pathProgramaFatorialRecursivo);

                ProcessadorDeID compilador = new ProcessadorDeID(parser.GetTokens());
                compilador.Compilar();


            }    
 
   
            public void TesteIfElse(AssercaoSuiteClasse assercao)
            {
                string pathFile = @"programasTestes\programaFatorialRecursivo.txt";
                ParserAFile parser = new ParserAFile(pathFile);
                ProcessadorDeID compilador = new ProcessadorDeID(parser.GetTokens());
                compilador.Compilar();

                List<Metodo> metodos = RepositorioDeClassesOO.Instance().GetClasse("classeFatorial").metodos; 

                try
                {
                    assercao.IsTrue(metodos[1].instrucoesFuncao.Count == 1);
                }
                catch (Exception e)
                {
                    string errorMessage = e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
                
            }

            public void TestesFuncoes(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string codigoClasseA = "public class classeA { public classeA() { int b=metodoA();  };  public int metodoA(){ int x =1; x=x+1;}; }; int a=1; int b=2;";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseA);
                compilador.Compilar();


                Classe classe1 = RepositorioDeClassesOO.Instance().GetClasse("classeA");
                List<Metodo> metodosClasse1 = classe1.metodos;

                try
                {
                    assercao.IsTrue(metodosClasse1[0].instrucoesFuncao.Count == 1 && metodosClasse1[0].instrucoesFuncao[0].expressoes.Count == 1);
                }catch(Exception e)
                {
                    string messageError = e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }


            }


            public void TestePrintTratadores(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string codigoClasseA = "public class classeA { public classeA() { int b=6;  };  public int metodoA(){ int x =1; x=x+1;}; }; int a=1; int b=2;";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseA);
                compilador.Compilar();

                List<MetodoTratadorOrdenacao> handlers = compilador.GetHandlers();
                for (int i = 0; i < handlers.Count; i++)
                {
                    System.Console.WriteLine((i + 1).ToString() + "-  " + handlers[i].patterResumed + "    regex: " + handlers[i].regex_sequencia.ToString());
                }

                System.Console.ReadLine();

            }



            public void TestesPequenosProgramas(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string pathProgram = @"programasTestes\programContagens.txt";
                ParserAFile program = new ParserAFile(pathProgram);



                ProcessadorDeID compilador = new ProcessadorDeID(program.GetTokens());
                compilador.Compilar();

                try
                {
                    assercao.IsTrue(true, "programa rodado sem erros fatais.");
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }




            }

   
            public void TesteInstanciacaoVariavel(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigo_0_1 = "int x=1;";
                string codigo_0_2 = "int x=1; int y=5;";
                string codigo0_3 = "int x=1+1;int y=5; x=x+y;";

                Escopo escopo1 = new Escopo(codigo_0_1 + codigo_0_2 + codigo0_3 + codigo0_3);

                ProcessadorDeID compilador = new ProcessadorDeID(codigo_0_1+ codigo_0_2);
                compilador.Compilar();

                try
                {
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("x", compilador.escopo) != null, codigo_0_1);
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("y", compilador.escopo) != null, codigo_0_2);
                }
                catch
                {
                    assercao.IsTrue(false, "falha na compilacao.");
                }
            }


 
            public void TesteCompilarUmaClasse(AssercaoSuiteClasse assercao)
            {

                SystemInit.InitSystem();

                string codigoClasseA = "public class classeA { public classeA() { };  public int metodoA(){}; }";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseA);
                compilador.Compilar();


                try
                {
                   
                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodos().Count == 2);

                }
                catch (Exception e)
                {
                    string codeMessage = e.Message;
                    assercao.IsTrue(false, "Teste Falhou");
                }
                


            }




            public void Atribuicao(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigo = "int a=2; int b=1; int c= 3*b;";
            
                ProcessadorDeID processador = new ProcessadorDeID(codigo);
                processador.Compilar();

                try
                {
                    assercao.IsTrue(processador.escopo.tabela.GetObjeto("a", processador.escopo) != null, codigo);
                    assercao.IsTrue(processador.escopo.tabela.GetObjeto("b", processador.escopo) != null, codigo);
                    assercao.IsTrue(processador.escopo.tabela.GetObjeto("c", processador.escopo) != null, codigo);

                }
                catch (Exception e)
                {
                    string codeMessage = e.Message;
                    assercao.IsTrue(false, "Teste Falhou");
                }
                
            }
            

            public void TesteCompilacaoPropriedade(AssercaoSuiteClasse assercao)
            {

                SystemInit.InitSystem();

                // codigo das classes do cenario de teste.
                string codigoClasseE = "public class classeE { public classeE() { }; public int propriedade1 ;  }";

                List<string> codigoTeste = new Tokens(new List<string>() { codigoClasseE }).GetTokens();


                ProcessadorDeID compilador = new ProcessadorDeID(codigoTeste);
                compilador.Compilar();

                try
                {
                    assercao.IsTrue(
                       RepositorioDeClassesOO.Instance().GetClasse("classeE").GetPropriedade("propriedade1") != null, codigoClasseE);

                }
                catch (Exception e)
                {
                    string msgError= e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }

            }




            public void TesteCasesOfUse(AssercaoSuiteClasse assercao)
            {

                // sintaxe: casesOfUse id : {( case operador exprss ) : {} (case operador exprss): {}}...
                string codigo = "int b=1; int a=5; casesOfUse a: {(case < b): { a = 2; }};";
                ProcessadorDeID processador = new ProcessadorDeID(codigo);
                processador.Compilar();
                try
                {
                    assercao.IsTrue(processador.instrucoes.Count >= 3, codigo);
                }
                catch(Exception ex)
                {
                    string codeMessage = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU.");
                }
                
            }


   


            public void TesteChamadaDeObjetoImportado(AssercaoSuiteClasse assercao)
            {
                CultureInfo.CurrentCulture = CultureInfo.CurrentCulture; // para compatibilizar os numeros float como: 1.0.

                /// ID ID = create ( ID , ID ) --> exemplo: int m= create(1,1).
                /// importer ( nomeAssembly).

                string codigoImportar = "importer (ParserLinguagemOrquidea.exe);";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoImportar);
                compilador.Compilar();

                try
                {
                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasses().Count > 8, codigoImportar);
                }
                catch (Exception ex)
                {
                    string codeMessage = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU.");
                }
                
            }

            public void TesteIF(AssercaoSuiteClasse assercao)
            {
                string codigo = "int a=1; int b=5; if (a<b){a=3*b;}";
                ProcessadorDeID compilador = new ProcessadorDeID(codigo);

                compilador.Compilar();

                try
                {
                    assercao.IsTrue(
                        compilador.instrucoes.Count == 3 &&
                        compilador.instrucoes[2].code == ProgramaEmVM.codeIfElse, codigo);
                }
                catch(Exception e)
                {
                    string codeMessage = e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
                




            }


        }

    } // class ProcessadorDeID

} // namespace
