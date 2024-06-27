using System.Collections.Generic;
using System.Linq;
using parser.ProgramacaoOrentadaAObjetos;
using System.Reflection;
using System;
using System.Security.Cryptography;
using static parser.SuiteClasseTestes;
using System.Net.Http.Headers;

namespace parser
{
    public class ProgramaEmVM
    {


        /// <summary>
        /// lista todas instruções do programa, rorando dentro da VM.
        /// </summary>
        private List<Instrucao> instrucoes = new List<Instrucao>();



        internal static Dictionary<int, HandlerInstrucao> dicHandlers = null;
        /// <summary>
        /// lista de tags das instruções a ser executadas.
        /// </summary>
        public static List<int> codesInstructions;


        public static int codeWhile = 0;
        public static int codeIfElse = 7;
        public static int codeFor = 3;
        public static int codeAtribution = 4;
        public static int codeCallerFunction = 5;
        public static int codeReturn = 2;
        public static int codeBlock = 6;
        public static int codeBreak = 9;
        public static int codeContinue = 10;
        public static int codeDefinitionFunction = 11; // a definição de função não é uma instrução, mas resultado da compilação.
        public static int codeGetObjeto = 14;
        public static int codeSetObjeto = 16;
        public static int codeOperadorBinario = 17;
        public static int codeOperadorUnario = 18;
        public static int codeCasesOfUse = 19;
        public static int codeCreateObject = 20;
        public static int codeImporter = 22;
        public static int codeCallerMethod = 25;
        public static int codeExpressionValid = 26;
        public static int codeConstructorUp = 27;
        public static int codeAspectos = 28;
        public static int codeRiseError = 29;

        public delegate object HandlerInstrucao(Instrucao umaInstrucao, Escopo escopo);

        private int IP_contador = 0; // guarda o id das sequencias.
        private bool isQuit = false;

        public object lastReturn;

        // inicia o programa na VM.
        public void Run(Escopo escopo)
        {

            IP_contador = 0; // inicia a primeira instrução do software.

            while (IP_contador < instrucoes.Count)
            {
                ExecutaUmaInstrucao(instrucoes[IP_contador], escopo);
                IP_contador++;

                if (isQuit)
                    break;
            } // while
        } // Run()


        /// <summary>
        /// inicializa uma instancia do programa virtual.
        /// </summary>
        /// <param name="instrucoesPrograma"></param>
        public ProgramaEmVM(List<Instrucao> instrucoesPrograma)
        {
         
            if (ProgramaEmVM.codesInstructions == null)
                ProgramaEmVM.codesInstructions = new List<int>();

            instrucoes = instrucoesPrograma.ToList<Instrucao>();

            if (dicHandlers == null)
            {
                dicHandlers = new Dictionary<int, HandlerInstrucao>();

                // nem todas instruções são acessíveis pelo dicionario: break, e continue ficam dentro das instruções de repetição. codeBlock não precisa ter handler.
                dicHandlers[codeAtribution] = this.InstrucaoAtribuicao;
                dicHandlers[codeCallerFunction] = this.InstrucaoChamadaDeFuncao;
                dicHandlers[codeCallerMethod] = this.InstrucaoChamadaDeMetodo;
                dicHandlers[codeDefinitionFunction] = this.InstrucaoDefinicaoDeFuncao;
                dicHandlers[codeIfElse] = this.InstrucaoIfElse;
                dicHandlers[codeFor] = InstrucaoFor;
                dicHandlers[codeWhile] = InstrucaoWhile;
                dicHandlers[codeGetObjeto] = InstrucaoGetObjeto;
                dicHandlers[codeSetObjeto] = InstrucaoSetObjeto;
                dicHandlers[codeOperadorUnario] = InstrucaoOperadorUnario;
                dicHandlers[codeOperadorBinario] = InstrucaoOperadorBinario;
                dicHandlers[codeCasesOfUse] = InstrucaoCasesOfUse;
                dicHandlers[codeCreateObject] = InstrucaoCreateObject;
                dicHandlers[codeImporter] = InstrucaoImporter;
                dicHandlers[codeReturn] = InstrucaoReturn;
                dicHandlers[codeExpressionValid] = InstrucaoExpressaoValida;
                dicHandlers[codeConstructorUp] = InstrucaoConstrutorUP;
                dicHandlers[codeAspectos] = InstrucaoAspecto;
            }
        } // InstrucoesVM()


        /// <summary>
        /// executa uma instrução dentro do programa contido na VM.
        /// </summary>
        /// <param name="umaInstrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns>retorna o resultado do processamento da instrucao.</returns>
        private object ExecutaUmaInstrucao(Instrucao umaInstrucao, Escopo escopo)
        {
            this.lastReturn = dicHandlers[umaInstrucao.code](umaInstrucao, escopo);
            return this.lastReturn;
        } // ExecutaUmaInstrucao()




        /// <summary>
        /// instrucao importer. vazio, porque foi utilizado na compilacao.
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoImporter(Instrucao instrucao, Escopo escopo)
        {
            // as classes já foram importadas no compilador..
            return null;

        } // InstrucaoCreateObject()


        /// <summary>
        /// instrucao chamadaDeMetodo. executa uma expressao chamada de metodo.
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoChamadaDeMetodo(Instrucao instrucao, Escopo escopo)
        {

            object resultChamada = null;
            Expressao ExpressaoObjetoChamadaDeMetodo = instrucao.expressoes[0];
            ExpressaoObjetoChamadaDeMetodo.isModify = true;

            EvalExpression eval = new EvalExpression();
            resultChamada = eval.EvalPosOrdem(ExpressaoObjetoChamadaDeMetodo, escopo);


            // retorna apenas o resultado da útltima chamada de método, ex.: "a.metodoB().metdoA(x)",
            // retorna o resultado de "metodoA(x)", o que segue a lógica de chamadas de métodos aninhadas...
            return resultChamada;

        }
    


        /// <summary>
        /// instrucao expressaoValida. executar uma expressao: int x=1; funcao(x,y), numero.tan(), texto.Replace(),
        /// tudo que não for comandos estruturados.
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoExpressaoValida(Instrucao instrucao, Escopo escopo)
        {
            if ((instrucao.expressoes != null) && (instrucao.expressoes.Count > 0))
            {
                
                
                Expressao expressao = instrucao.expressoes[0];
                EvalExpression eval = new EvalExpression();

              


                object result = eval.EvalPosOrdem(expressao, escopo);


                return result;
            }
            else
            {
                return null;
            }

        } 

        /// <summary>
        /// instrucao constructorUP. executa um construtor da classe base de uma classe herdeira.
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoConstrutorUP(Instrucao instrucao, Escopo escopo)
        {
            ///   Cabecalho de listas de expressoes:
            ///   0- nomeDaClasseHerdeira
            ///   1- nomeDaClasseHerdada.
            ///   2- indice do construtor da classe herdada.
            ///   3- expressao cujos elementos são os parametros do construtor.


            /// template: nomeDaClasseHerdeira.construtorUP(nomeClasseHerdada, List<Expressao> parametrosDoConstrutor).
            /// obsoleto, chamada direta do construtor up como um metodo comum.
            ///           



            string nomeDaClasseHerdeira = instrucao.expressoes[0].ToString();
            string nomeClasseHerdada = instrucao.expressoes[1].ToString();


            Objeto ObjetoAtual = escopo.tabela.GetObjeto("atual", nomeClasseHerdada, escopo); // obtem o objeto referenciado pelo construtor principal.
            if (ObjetoAtual == null)
                return null;



            Classe classeHerdada = RepositorioDeClassesOO.Instance().GetClasse(nomeClasseHerdada);  // obtem a classe do objeto a ser instanciado.

            int indexConstrutorClasseHerdada = int.Parse(instrucao.expressoes[2].ToString());
            Metodo construtor = classeHerdada.construtores[indexConstrutorClasseHerdada]; // obtem o construtor para instanciar o objeto a ser instanciado.



            List<Expressao> parametrosParaOConstrutor = instrucao.expressoes[3].Elementos; //obtm os parâmetros a serem passados para o construtor da classe herdada.



            Escopo escopoConstrutorUP = escopo.Clone();


            // executa o construtor, com o escopo que detém os valores dos objetos herdados.
            construtor.ExecuteAConstructor(ref ObjetoAtual, parametrosParaOConstrutor, nomeClasseHerdada, escopoConstrutorUP, indexConstrutorClasseHerdada);

            escopo = escopoConstrutorUP.Clone();

            return new object();

        }

        /// <summary>
        /// cria uma instancia de um objeto, se já não foi criado (em tempo de compilação).
        /// pode ser um objeto oquidea, ou um objeto importado.
        /// </summary>
        /// <param name="instrucao">dados da instrução.</param>
        /// <param name="escopo">contexto onde as expressoes da instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoCreateObject(Instrucao instrucao, Escopo escopo)
        {


            /*
             *  ESTRUTURA DE DADOS CONTIDA NA LISTA DE EXPRESSOES.
             * 
               ELemento 0: token "create";
               ELemento 1: tipo do objeto instanciado.
               ELemento 2: nome do objeto instanciado.
               ELemento 3: token "Objeto" ou "Vetor" (deprecado).
               ELemento 4: tipo de um elemento do vetor.
               ELemento 5: lista de parametros, para o create, se for Objeto, ou parametros que compoe os indices matriciais se for Vetor.
               ELemento 6: indice do construtor compativel.
               Elemento 7: nome do objeto caller.
             * 
             */

          
            // INSTANCIACAO DE OBJETO IMPORTADO.
            if (instrucao.expressoes[0].Elementos[0].ToString() != "create")
            {
                return null;
            }



            string tipoDoObjeto = instrucao.expressoes[0].Elementos[1].ToString();
            string nomeDoObjeto = instrucao.expressoes[0].Elementos[2].ToString();
            int indexConstructor = int.Parse(instrucao.expressoes[0].Elementos[6].ToString());


            Expressao expressoesParametros;
            if (instrucao.expressoes[0].Elementos[5] != null)
            {
                expressoesParametros = instrucao.expressoes[0].Elementos[5];
            }
            else
            {
                expressoesParametros = new Expressao();
            }

//______________________________________________________________________________________________________________________________________________________
            // O OBJETO A SER CRIADO É UM OBJETO IMPORTADO, constroi o valor do objeto com um construtor também importado.
            if ((RepositorioDeClassesOO.Instance().GetClasse(tipoDoObjeto) != null) && ((RepositorioDeClassesOO.Instance().GetClasse(tipoDoObjeto).isImport)))
            {
                List<object> parametrosObject = new List<object>();
                EvalExpression eval = new EvalExpression();


                if ((expressoesParametros != null) && (expressoesParametros.Elementos != null) && (expressoesParametros.Elementos.Count > 0))
                {
                    // avalia as expressoes dos parametros, guardando na lista de parametros do construtor.
                    for (int x = 0; x < expressoesParametros.Elementos.Count; x++)
                    {
                        object umParametroObject = eval.EvalPosOrdem(expressoesParametros.Elementos[x], escopo);
                        if (umParametroObject == null)
                        {
                            umParametroObject = eval.EvalPosOrdem(expressoesParametros.Elementos[x].Elementos[0], escopo);
                        }
                        parametrosObject.Add(umParametroObject);
                    }
                }

                
                // PROCESSAMENTO DE CLASSE IMPORTADA.
                if ((RepositorioDeClassesOO.Instance().GetClasse(tipoDoObjeto) != null) &&
                    (RepositorioDeClassesOO.Instance().GetClasse(tipoDoObjeto).isImport))
                {

                    // procura o tipo do objeto, via reflexao.
                    Type typeObjeto = Type.GetType(tipoDoObjeto);
                    if (typeObjeto == null)
                    {
                        // o tipo pode estar no namespace [parser]
                        typeObjeto = Type.GetType("parser." + tipoDoObjeto);
                    }

                    // se null o tipo estara nas bibliotecas externas.
                    if (typeObjeto == null)
                    {
                        // obtem o tipo importado do objeto.
                        if ((LinguagemOrquidea.libraries != null) && (LinguagemOrquidea.libraries.Count > 0))
                        {
                            for (int i = 0; i < LinguagemOrquidea.libraries.Count; i++)
                            {
                                if ((LinguagemOrquidea.libraries[i].GetTypes() != null) &&
                                    (LinguagemOrquidea.libraries[i].GetTypes().Length > 0))
                                {
                                    for (int j = 0; j < LinguagemOrquidea.libraries[i].GetTypes().Length; j++)
                                    {
                                        if (LinguagemOrquidea.libraries[i].GetTypes()[j].Name == tipoDoObjeto)
                                        {
                                            typeObjeto = LinguagemOrquidea.libraries[i].GetTypes()[j];
                                            break;
                                        }
                                    }

                                }


                            }

                        }

                    }




                    object valorObjeto = Metodo.ExecuteAConstructorImportado(typeObjeto, parametrosObject, indexConstructor);
                    if (valorObjeto != null)
                    {
                        Objeto objCriado = new Objeto("private",tipoDoObjeto, nomeDoObjeto, valorObjeto);
                 
                        // atualiza o valor do objeto.
                        escopo.tabela.UpdateObjeto(objCriado);
                        if (valorObjeto == null)
                        {
                            objCriado.valor= objCriado;
                        }




                        return objCriado;
                    }
                }
                else
                {
                    return null;
                }
            }
            //_____________________________________________________________________________________________________________________________________________________
            // O OBJETO A INSTANCIAR É UM OBJETO ORQUIDEA [Objeto]
            if (tipoDoObjeto == "METEORO")
            {
                int k = 0;
                k++;
            }

            Objeto objJaInstanciado = new Objeto("private", tipoDoObjeto, nomeDoObjeto, null);
            objJaInstanciado.indexConstrutor = indexConstructor;
            

            // obtem um construtor, de acordo com os parametros de construtor.
            indexConstructor = objJaInstanciado.indexConstrutor;
            

            Metodo construtor = null;
            if (indexConstructor >= 0)
            {
                construtor = RepositorioDeClassesOO.Instance().GetClasse(tipoDoObjeto).construtores[indexConstructor];
                if (construtor == null)
                {
                    escopo.tabela.UpdateObjeto(objJaInstanciado);
                    return objJaInstanciado;
                }
            }
            else
            {
                // construtor nao construido antes da instanciacao do objeto a instanciar.
                indexConstructor = ProcessadorDeID.FoundACompatibleConstructor(objJaInstanciado.tipo, expressoesParametros.Elementos);
                if (indexConstructor >= 0)
                {
                    construtor = RepositorioDeClassesOO.Instance().GetClasse(tipoDoObjeto).construtores[indexConstructor];
                }
                
            }
            

            
            Escopo escopoCreate = escopo;


          
            List<Expressao> parametrosOrquideaConstrutor= new List<Expressao>();
            if ((expressoesParametros != null) && (expressoesParametros.Elementos != null) && (expressoesParametros.Elementos.Count > 0))
            {
                for (int i = 0; i < expressoesParametros.Elementos.Count; i++)
                {
                    parametrosOrquideaConstrutor.Add(expressoesParametros.Elementos[i]);
                }
            }

            if (construtor != null)
            {


                ///__________________________________________________________________________________________________________________________________________________
                // CONSTROI UMA NOVA INSTANCIA DO OBJETO, o objeto construido está no [escopoCreate].
                object objetoResult = construtor.ExecuteAConstructor(ref objJaInstanciado, parametrosOrquideaConstrutor, tipoDoObjeto, escopoCreate, indexConstructor);

                // seta o valor propriedade, com o próprio objeto.
                objJaInstanciado = ((Objeto)objetoResult).Clone();
                objJaInstanciado.valor = objJaInstanciado;

                if (escopo.tabela.GetObjeto(objJaInstanciado.nome, escopo) != null)
                {
                    // atualiza o escopo.            
                    escopo.tabela.UpdateObjeto(objJaInstanciado);

                }
                else
                {
                    escopo.tabela.RegistraObjeto(objJaInstanciado);
                }






            }
            return objJaInstanciado;
        }








        /// <summary>
        /// execução da instrução de construção do casesOfUses. 
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoCasesOfUse(Instrucao instrucao, Escopo escopo)
        {

            // formato:
            /// exprss[umCaseIndex]:a expressao condicional do case.
            /// instrucao[umCaseIndex]: instrucoes.


            List<Expressao> expressoes = instrucao.expressoes;
            

            List<Expressao> exprssCodicionaisDoCase = instrucao.expressoes;

            EvalExpression eval = new EvalExpression(); // inicializa o avaliador de expressões.

            // percorre os cases da instrução, se a expressão condicional do case for true, roda a lista de instruções do case.
            for (int x = 0; x < exprssCodicionaisDoCase.Count; x++)
            {
                exprssCodicionaisDoCase[x].isModify = true;



                bool resultCondicionalDoCase = (bool)eval.EvalPosOrdem(exprssCodicionaisDoCase[x], escopo);
                if (resultCondicionalDoCase)
                {
                    // obtém o bloco de instruções do case avaliado.
                    List<Instrucao> instrucoesDoCase = instrucao.blocos[x];
                    if ((escopo.escopoFolhas != null) && (escopo.escopoFolhas.Count - 1 >= x))
                    {
                        LoadObjectsBloco(escopo, escopo.escopoFolhas[x]);
                    }
                    for (int i = 0; i < instrucoesDoCase.Count; i++)
                    {
                        this.ExecutaUmaInstrucao(instrucoesDoCase[i], escopo);
                    }
                        
                    if ((escopo.escopoFolhas != null) && (escopo.escopoFolhas.Count - 1 >= x))
                    {
                        UnloadObjectsBloco(escopo.escopoFolhas[x], escopo);
                    }
                } // if
            } // for x
            return null;
        } 


        /// <summary>
        /// instrucao de construcao de um operador binario costumizado.
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoOperadorBinario(Instrucao instrucao, Escopo escopo)
        {
            // obtém alguns dados do novo operador binario.
            string tipoRetornoDoOperador = ((ExpressaoElemento)instrucao.expressoes[0]).GetElemento().ToString();
            string nomeOperador = ((ExpressaoElemento)instrucao.expressoes[1]).GetElemento().ToString();
            string tipoOperando1 = ((ExpressaoElemento)instrucao.expressoes[2]).GetElemento().ToString();
            string nomeOpérando1 = ((ExpressaoElemento)instrucao.expressoes[3]).GetElemento().ToString();
            string tipoOperando2 = ((ExpressaoElemento)instrucao.expressoes[4]).GetElemento().ToString();
            string nomeOperando2 = ((ExpressaoElemento)instrucao.expressoes[5]).GetElemento().ToString();
            int prioridade = int.Parse(((ExpressaoElemento)instrucao.expressoes[6]).GetElemento().ToString());
            Metodo metodoDeImplantacaoDoOperador = ((ExpressaoChamadaDeMetodo)instrucao.expressoes[7]).funcao;


            // encontra a classe em que se acrescentará o novo operador binnario.
            Classe classeDoOperador = escopo.tabela.GetClasses().Find(k => k.GetNome() == tipoRetornoDoOperador);
            if (classeDoOperador == null)
            {
                return null;
            }
                
            // consroi o novo operador binario, a partir dos dados coletados.
            Operador novoOperadorBinario = new Operador(classeDoOperador.GetNome(), nomeOperador, prioridade, new string[] { tipoOperando1, tipoOperando2 }, tipoRetornoDoOperador, metodoDeImplantacaoDoOperador, escopo);
            novoOperadorBinario.funcaoImplementadoraDoOperador = metodoDeImplantacaoDoOperador; // seta a função de cálculo do operador.

            if (novoOperadorBinario == null)
            {
                return null;
            }
                

            // adiciona o novo operador binario para a classe do tipo de retorno do operador.
            classeDoOperador.GetOperadores().Add(novoOperadorBinario);

            LinguagemOrquidea.Instance().AddOperator(novoOperadorBinario);

            // atualiza a classe no repositório.
            Classe classeRepositorio = RepositorioDeClassesOO.Instance().GetClasse(tipoRetornoDoOperador);
            if (classeRepositorio != null)
            {
                RepositorioDeClassesOO.Instance().GetClasse(tipoRetornoDoOperador).GetOperadores().Add(novoOperadorBinario);
            }
                
            return null;
        }

        /// <summary>
        /// instrução de construção de operador binario. 
        /// 
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrução está.</param>
        /// <returns></returns>
        private object InstrucaoOperadorUnario(Instrucao instrucao, Escopo escopo)
        {
            // obtém alguns dados do novo operador unario.
            string tipoRetornoDoOperador = ((ExpressaoElemento)instrucao.expressoes[0]).GetElemento().ToString();
            string nomeOperador = ((ExpressaoElemento)instrucao.expressoes[1]).GetElemento().ToString();
            string tipoOperando1 = ((ExpressaoElemento)instrucao.expressoes[2]).GetElemento().ToString();
            string nomeOpérando1 = ((ExpressaoElemento)instrucao.expressoes[3]).GetElemento().ToString();
            int prioridade = int.Parse(((ExpressaoElemento)instrucao.expressoes[4]).GetElemento().ToString());
            Metodo funcaoOperador = ((ExpressaoChamadaDeMetodo)instrucao.expressoes[5]).funcao;


            // encontra a classe em que se acrescentará o novo operador unario.
            Classe classeDoOperador = escopo.tabela.GetClasses().Find(k => k.GetNome() == tipoRetornoDoOperador);
            if (classeDoOperador == null)
            {
                return null;
            }
                
            // consroi o novo operador unario, a partir dos dados coletados.
            Operador novoOperadorUnario = new Operador(classeDoOperador.GetNome(), nomeOperador, prioridade, new string[] { tipoOperando1 }, tipoRetornoDoOperador, ((ExpressaoChamadaDeMetodo)instrucao.expressoes[5]).funcao, escopo);

            novoOperadorUnario.funcaoImplementadoraDoOperador = funcaoOperador;
            if (novoOperadorUnario == null)
            {

            }
             

            // atualiza a classe no escopo.
            classeDoOperador.GetOperadores().Add(novoOperadorUnario); // adiciona o novo operador unario para a classe do tipo de retorno do operador.

            LinguagemOrquidea.Instance().AddOperator(novoOperadorUnario);

            // atualiza a classe no repositorio.
            Classe classeRepositorioOperador = RepositorioDeClassesOO.Instance().GetClasse(tipoRetornoDoOperador);
            if (classeRepositorioOperador != null)
            {
                RepositorioDeClassesOO.Instance().GetClasse(tipoRetornoDoOperador).GetOperadores().Add(novoOperadorUnario);
            }
                
            return null;
        }


        /// <summary>
        /// executa uma instrucao GetObjeto.
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoGetObjeto(Instrucao instrucao, Escopo escopo)
        {
            object valor = ((ExpressaoObjeto)instrucao.expressoes[0]).objectCaller.GetValor();
            return valor;
        }

        
        /// <summary>
        /// executa uma instrucao SetObjeto.
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoSetObjeto(Instrucao instrucao, Escopo escopo)
        {
            Objeto v = ((ExpressaoObjeto)instrucao.expressoes[0]).objectCaller;
            v.SetValor(((ExpressaoElemento)instrucao.expressoes[1]).elemento);
            return null;
        }

        /// <summary>
        /// executa uma instrucao return.
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoReturn(Instrucao instrucao, Escopo escopo)
        {
            if ((instrucao.expressoes == null) || (instrucao.expressoes.Count == 0))
                return null;

            instrucao.expressoes[0].isModify = true;
            EvalExpression eval = new EvalExpression();
            object objRetorno = eval.EvalPosOrdem(instrucao.expressoes[0], escopo);

            return objRetorno;
          

        }

        /// <summary>
        /// executa uma instrucao while.
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoWhile(Instrucao instrucao, Escopo escopo)
        {

            Expressao exprssControle = instrucao.expressoes[0];

         
            EvalExpression eval = new EvalExpression();
            exprssControle.isModify = true;
            if ((instrucao.escoposDeBloco!=null) && (instrucao.escoposDeBloco.Count > 0))
            {
                LoadObjectsBloco(escopo, instrucao.escoposDeBloco[0]);
            }

            while ((bool)eval.EvalPosOrdem(exprssControle, escopo))
            {
                
                Executa_bloco(instrucao, escopo, 0);
                exprssControle.isModify = true;
            }
            if ((instrucao.escoposDeBloco != null) && (instrucao.escoposDeBloco.Count > 0))
            {
                UnloadObjectsBloco(instrucao.escoposDeBloco[0], escopo);
            }
            return null;

        } 

        /// <summary>
        /// executa uma instrucao if/else.
        /// </summary>
        /// <param name="instrucao">instrucao if/else.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoIfElse(Instrucao instrucao, Escopo escopo)
        {

            // expresssao controle: expressao[0]
            // blocos: instrucoes.Blocos. (2 para else).

            Expressao exprssControle = instrucao.expressoes[0];
            exprssControle.isModify = true;

            object result = null;
            EvalExpression eval = new EvalExpression();
            object resultBool = eval.EvalPosOrdem(exprssControle, escopo);
            if ((resultBool != null) && (resultBool.GetType() == typeof(bool)) && ((bool)resultBool == true)) 
            {

                LoadObjectsBloco(escopo, instrucao.escoposDeBloco[0]);
                


                result = Executa_bloco(instrucao, escopo, 0);
                exprssControle.isModify = true;

                if ((instrucao.escoposDeBloco != null) && (instrucao.escoposDeBloco.Count > 0))
                {
                    UnloadObjectsBloco(instrucao.escoposDeBloco[0], escopo);
                }

                return result;
            }

            else
            {
                if ((instrucao.escoposDeBloco != null) && (instrucao.escoposDeBloco.Count > 1))
                {
                    LoadObjectsBloco(escopo, instrucao.escoposDeBloco[1]);
                }

                if (instrucao.blocos.Count > 1)  //procesamento da instrução else. O segundo bloco é para instrução else.
                {
                    exprssControle.isModify = true;
                    result = Executa_bloco(instrucao, escopo, 1);
                }

                if ((instrucao.escoposDeBloco != null) && (instrucao.escoposDeBloco.Count > 1))
                {
                    UnloadObjectsBloco(instrucao.escoposDeBloco[1], escopo);
                }

                return result;
            }
            
         

        } // IfElseInstrucao()

        /// <summary>
        /// executa um bloco de instrucoes.
        /// </summary>
        /// <param name="instrucao">instrucao base.</param>
        /// <param name="escopo">contexto onde a instrucao base está.</param>
        /// <param name="bloco">indice do bloco.</param>
        /// <returns></returns>
        private object Executa_bloco(Instrucao instrucao, Escopo escopo, int bloco)
        {
            object result = null;
            // transfere o escopo do bloco para o escopo estático [EscopoCurrent].
            Escopo.escopoCURRENT = escopo;
          

            for (int umaInstrucao = 0; umaInstrucao < instrucao.blocos[bloco].Count; umaInstrucao++)
            {
                if (instrucao.blocos[bloco][umaInstrucao].code == codeBreak)
                {
                    break;
                }
                    
                if (instrucao.blocos[bloco][umaInstrucao].code == codeContinue)
                {
                    continue;
                }

                if (instrucao.blocos[bloco][umaInstrucao].code != codeReturn) 
                {
                    // processamento de instruções do bloco, com o escopo currente, que é copia do escopo do bloco.
                    result = ExecutaUmaInstrucao(instrucao.blocos[bloco][umaInstrucao], escopo);
                }
                else
                {
                    object resultReturn = new EvalExpression().EvalPosOrdem(instrucao.blocos[bloco][umaInstrucao].expressoes[0], escopo);
                    return resultReturn;
                }

           
            } // for bloco
            return result;
        }

        /// <summary>
        /// descarrega os objetos do escopoBloco, que estão presentes no escopoTO.
        /// </summary>
        /// <param name="escopoBloco">escopo de bloco de instrucoes, contendo objetos a registrar no escopo acima.</param>
        /// <param name="escopoTO">escopo pai do escopo bloco.</param>
        private void UnloadObjectsBloco(Escopo escopoBloco, Escopo escopoTO)
        {
            if ((escopoBloco != null) && (escopoBloco.tabela != null) && (escopoBloco.tabela.objetos != null) && (escopoBloco.tabela.objetos.Count > 0))
            {

                List<Objeto> objBLOCO = escopoBloco.tabela.getObjetos().ToList<Objeto>();
                // if estiver no escopo bloco, mas nao no escopoTO, remove do escopo bloco.
                for (int i = 0; i < objBLOCO.Count; i++)
                {
                    int index = escopoTO.tabela.objetos.FindIndex(k => k.nome == objBLOCO[i].nome);
                    if (index == -1)
                    {
                        escopoBloco.tabela.objetos.RemoveAt(i);
                    }
                }



            }
        }

        /// <summary>
        /// carrega para o escopo bloco, os objetos do escopoFrom
        /// </summary>
        /// <param name="escopoOrigem">escopo origem.</param>
        /// <param name="escopoBloco">escopo destino.</param>
        private void LoadObjectsBloco(Escopo escopoOrigem, Escopo escopoBloco)
        {
            if (escopoBloco.tabela.objetos != null)
            {


                List<Objeto> objORIGEM = escopoOrigem.tabela.getObjetos().ToList<Objeto>();
                for (int i = 0; i < objORIGEM.Count; i++)
                {
                    int index = escopoBloco.tabela.objetos.FindIndex(k => k.nome == objORIGEM[i].nome);
                    if (index == -1)
                    {
                        // adiciona o objeto do escopo origem, que nao esta no escopo do bloco.
                        escopoBloco.tabela.objetos.Add(objORIGEM[i]);
                    }
                    else
                    {   // ou atualiza o objeto presente no escopo bloco.
                        escopoBloco.tabela.UpdateObjeto(objORIGEM[i]);
                    }
                }
            }

        }

        /// <summary>
        /// executa uma instrucao for.
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoFor(Instrucao instrucao, Escopo escopo)
        {

            /// template: for (int x=0;x.controleLimite; x++)
            /// template instruções:
            ///    instrucao.expressoes[0]: expressão de atribuição da variável controle. 
            ///    instrucao.expressoes[1]; expressão de controle para a instrução.
            ///    instrucao.expressoes[2]: expressão de incremento da instrucao.
            ///    

            Expressao exprssAtribuicao = instrucao.expressoes[0];
            Expressao exprsCondicional = instrucao.expressoes[1];
            Expressao exprsIncremento = instrucao.expressoes[2];

            EvalExpression eval = new EvalExpression();

            object valorAtribuicao = (((Objeto)eval.EvalPosOrdem(exprssAtribuicao, escopo)).valor);

            // obtem a variavel de controle da malha.
            Objeto objVarMalha = ((ExpressaoAtribuicao)exprssAtribuicao.Elementos[0]).objetoAtribuir;
            objVarMalha.valor= valorAtribuicao;


            escopo.tabela.UpdateObjeto(objVarMalha);


            
        
            // calcula o limite do contador para o "for".
            Expressao exprssControle = exprsCondicional.Clone();


            if ((instrucao.blocos != null) && (instrucao.blocos.Count > 0))
            {
                // carrega objeto do escopo do bloco 0.
                LoadObjectsBloco(escopo, instrucao.escoposDeBloco[0]);

            }





            while ((bool)eval.EvalPosOrdem(exprsCondicional, escopo))  // avalia a expressão de controle.
            {
                
                if ((instrucao.blocos[0] != null) && (instrucao.blocos[0].Count > 0))
                {


                    // executa as instrucoes do operador bloco.
                    object result = Executa_bloco(instrucao, escopo, 0);


                  
                    // calcula o proximo valor da variavel de malha.
                    valorAtribuicao = eval.EvalPosOrdem(exprsIncremento, escopo);

                    
                    objVarMalha.valor = valorAtribuicao;
                    escopo.tabela.UpdateObjeto(objVarMalha);
               
                
                }
                else
                {
                    break;
                }
                    

                


            } // for

            if ((instrucao.blocos != null) && (instrucao.blocos.Count > 0))
            {
                // descarrega os objetos do escopo do bloco, do escopo pai.
                UnloadObjectsBloco(instrucao.escoposDeBloco[0], escopo);

            }

            return null;
        } // ForInstrucao()

        /// <summary>
        /// executa uma instrucao de atribuicao.
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoAtribuicao(Instrucao instrucao, Escopo escopo)
        {

            /// estrutura de dados para atribuicao:
            /// 0- Elemento[0]: tipo do objeto.
            /// 1- Elemento[1]: nome do objeto.
            /// 2- Elemento[2]: se tiver propriedades/metodos aninhados: expressao de aninhamento. Se não tiver, ExpressaoElemento("") ".
            /// 3- Elemento[3]: expressao da atribuicao ao objeto. (se nao tiver: ExpressaoELemento("")




            string tipoObjetoAAtribuir = ((ExpressaoElemento)instrucao.expressoes[0].Elementos[0]).elemento;
            string nomeObjetoAAtribuir = ((ExpressaoElemento)instrucao.expressoes[0].Elementos[1]).elemento;


            EvalExpression eval = new EvalExpression();
            Expressao atribuicao = instrucao.expressoes[0].Elementos[3];
            
            object novoValorObjeto = null;
            if (atribuicao != null)
            {   

                novoValorObjeto = eval.EvalPosOrdem(atribuicao, escopo);
                if ((novoValorObjeto != null) && (novoValorObjeto.GetType() == typeof(Objeto)))
                {
                    Objeto objValor= (Objeto)novoValorObjeto;
                    novoValorObjeto = objValor.valor;
                }
                
            }


            // PROCESSAMENTO DE OBJETOS PROPRIEDADES ESTATICAS.
            if ((instrucao.expressoes[0].Elementos.Count >= 5) && (((ExpressaoElemento)instrucao.expressoes[0].Elementos[4]).elemento == "estatica"))
            {

                string tipoDaPropriedadeEstatica = tipoObjetoAAtribuir;
                string nomeDaPropriedadeEstatica = nomeObjetoAAtribuir;

                Classe classe = escopo.tabela.GetClasse(tipoDaPropriedadeEstatica, escopo);
                if (classe != null)
                {
                    object novoValorB = new EvalExpression().EvalPosOrdem(instrucao.expressoes[0].Elementos[3], escopo);

                    Objeto objPropEstatica = classe.propriedadesEstaticas.Find(k => k.GetNome().Equals(nomeDaPropriedadeEstatica));
                    if (objPropEstatica != null)
                    {
                        objPropEstatica.valor = novoValorB;
                    }


                    return classe.propriedadesEstaticas.Find(k => k.GetNome().Equals(nomeDaPropriedadeEstatica));

                }


            }
            // PROCESSAMENTO DE OBJETOS EM EXPRESSAO OBJETO.
            if (instrucao.expressoes[0].Elementos[1].GetType() == typeof(ExpressaoObjeto))
            {


                Objeto objAtribuir = escopo.tabela.GetObjeto(nomeObjetoAAtribuir, escopo);

                if (objAtribuir != null)
                {
                    objAtribuir.valor = novoValorObjeto;
                    escopo.tabela.RemoveObjeto(nomeObjetoAAtribuir);
                    escopo.tabela.RegistraObjeto(objAtribuir);
                    return objAtribuir;
                }
                else
                {
                    Objeto objCreated = new Objeto("private", tipoObjetoAAtribuir, nomeObjetoAAtribuir, novoValorObjeto);
                    escopo.tabela.RegistraObjeto(objCreated);

                    return objCreated;
                }

            }
            else
            // PROCESSAMENTO DE PROPRIEDADES ANINHADAS NAO ESTATICAS.
            if (instrucao.expressoes[0].Elementos[1].GetType() == typeof(ExpressaoPropriedadesAninhadas))
            {
                ExpressaoPropriedadesAninhadas exprssAninhamento = (ExpressaoPropriedadesAninhadas)instrucao.expressoes[0].Elementos[1];

                Objeto objCaller = exprssAninhamento.objectCaller;
                objCaller.SetValorField(nomeObjetoAAtribuir, novoValorObjeto);

            }
            // PROCESSAMENTO DE VARIAVEIS, QUE NAO SAO OBJETOS! SAO TIPOS DE BASE: STRING, INT,... QUE NAO SAO DO TIPO OBJETO!
            else
            {
                Objeto objAtribui = escopo.tabela.GetObjeto(nomeObjetoAAtribuir, escopo);
                if ((objAtribui != null) && (novoValorObjeto != null))
                {
                    if (novoValorObjeto.GetType() != typeof(Objeto))
                    {
                        if (escopo.tabela.GetObjeto(nomeObjetoAAtribuir, escopo) != null)
                        {
                            escopo.tabela.GetObjeto(nomeObjetoAAtribuir, escopo).valor = novoValorObjeto;
                        }

                    }
                    else
                    if (novoValorObjeto.GetType() == typeof(Objeto))
                    {
                        Objeto objValor = (Objeto)novoValorObjeto;
                        objAtribui.valor = objValor.valor;
                    }



                    return objAtribui;
                }
            
    }
            return null;
        } // InstrucaoAtribuicao()


        private static string ObtemTipoRecursivamente(Escopo escopo, string nomePropriedadeProcurada, Objeto objetoAtribuicaoCampo)
        {
            if (objetoAtribuicaoCampo == null)
            {
                return null;
            }
                

            if (objetoAtribuicaoCampo.GetNome() == nomePropriedadeProcurada)
            {
                return objetoAtribuicaoCampo.GetTipo();
            }
                
            else
            {
                string tipoPropriedadeProcurada = null;
                for (int i = 0; i < objetoAtribuicaoCampo.GetFields().Count; i++)
                {

                    tipoPropriedadeProcurada = ObtemTipoRecursivamente(escopo, nomePropriedadeProcurada, objetoAtribuicaoCampo.GetFields()[i]);
                    if (tipoPropriedadeProcurada != null)
                    {
                        return tipoPropriedadeProcurada;
                    }
                        
                }
            }
            return null;
        }

        private object InstrucaoChamadaDeFuncao(Instrucao instrucao, Escopo escopo)
        {

            if (instrucao.expressoes[1].GetType() == typeof(ExpressaoChamadaDeMetodo))
            {
                ExpressaoChamadaDeMetodo funcaoExpressao = (ExpressaoChamadaDeMetodo)instrucao.expressoes[1];
                Metodo funcaoDaChamada = funcaoExpressao.funcao;
                List<Expressao> expressoesParametros = funcaoExpressao.Elementos;
                return funcaoDaChamada.ExecuteAFunction(expressoesParametros, funcaoDaChamada.caller, escopo);
            } // if
            return null;
        }



        /// <summary>
        /// instrucao de definicao de instrucao. LEGADO. era executado durante a compilacao.
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoDefinicaoDeFuncao(Instrucao instrucao, Escopo escopo)
        {
            // a instrucao de definicao nao eh executada no programa VM
            return null;
        }

        /// <summary>
        /// executa um bloco de instrucoes, dentro de um try/catch.
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoRiseError(Instrucao instrucao, Escopo escopo)
        {
            if (instrucao.blocos.Count > 0)
            {
                try
                {
                    if ((instrucao.blocos[0] != null) && (instrucao.blocos[0] != null) && (instrucao.blocos[0].Count > 0)) 
                    {
                        List<Instrucao> umBlocoTry = instrucao.blocos[0];
                        for (int x = 0; x < umBlocoTry.Count; x++)
                            ExecutaUmaInstrucao(umBlocoTry[x], escopo);

                        return null;
                    }
                }
                catch
                {
                    if ((instrucao.blocos.Count > 1) && (instrucao.blocos[1] != null) && (instrucao.blocos[1].Count > 0))
                    {
                        List<Instrucao> umBlocoRiseError = instrucao.blocos[0];
                        for (int x = 0; x < umBlocoRiseError.Count; x++)
                            ExecutaUmaInstrucao(umBlocoRiseError[x], escopo);

                        return null;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// executa uma instrucao [aspecto].
        /// </summary>
        /// <param name="instrucao">instrucao.</param>
        /// <param name="escopo">contexto onde a instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoAspecto(Instrucao instrucao, Escopo escopo)
        {

            if (instrucao.expressoes[0].GetType() == typeof(ExpressaoChamadaDeMetodo))
            {
                // obtem a funcao do corte aspecto.
                ExpressaoChamadaDeMetodo umaExpressaoObjetoChamadaDeMetodo = (ExpressaoChamadaDeMetodo)instrucao.expressoes[0];
                Metodo funcaoDaChamada = umaExpressaoObjetoChamadaDeMetodo.funcao;

                // obtem o nome do objeto parametro da funcao corte.
                string nomeObjetoMonitorado = ((ExpressaoElemento)instrucao.expressoes[1]).ToString();
                
                
                if (escopo.tabela.GetObjeto(nomeObjetoMonitorado, escopo) != null)
                {
                    /// int b; 
                    /// int funcao(int x);
                    /// x.valor= b.valor;
                    /// b.valor= funcao(x);
                    
                    Objeto objetoSobAspecto = escopo.tabela.GetObjeto(nomeObjetoMonitorado, escopo);// obtem o objeto sob aspecto, no escopo.
                    funcaoDaChamada.parametrosDaFuncao[0].SetValor(objetoSobAspecto.GetValor());

                    // obtem o valor da funcao corte, tendo parametro com valor do objeto monitorado.
                    object novoValor = funcaoDaChamada.ExecuteAFunction(
                        new List<Expressao>() { new ExpressaoObjeto(funcaoDaChamada.parametrosDaFuncao[0]) },
                        funcaoDaChamada.caller, escopo);

                    // seta o valor do objeto monitorado, com o valor de retorno da função.
                    escopo.tabela.GetObjeto(nomeObjetoMonitorado, escopo).SetValor(novoValor);

                    
                    if (escopo.tabela.GetObjeto(funcaoDaChamada.parametrosDaFuncao[0].GetNome(), escopo) != null)
                    {
                        escopo.tabela.GetObjeto(funcaoDaChamada.parametrosDaFuncao[0].GetNome(), escopo).SetValor(novoValor);
                        
                    }

                    return novoValor;
                }
                
            } // if
            return null;
        }



    } // class ProcessamentoInstrucoes()


    /// <summary>
    /// // uma instrução da linguagem orquidea  tem 4 objetos:
    /// 1- um id do tipo int, para controle de chamadas de métodos/funções,
    /// 2- o codigo da instrução,
    /// 3- a lista de expressões utilizadas,
    /// 4- a lista de blocos de sequencias que comporarão bloos associados à instrução.
    /// </summary>
    public class Instrucao
    {
        public int code; // tipo da instrução.

        /// <summary>
        /// expressões da instrução.
        /// </summary>
        public List<Expressao> expressoes = new List<Expressao>();

        /// <summary>
        /// lista de blocos instrucoes relacionado a blocos.
        /// </summary>
        public List<List<Instrucao>> blocos = new List<List<Instrucao>>();
        
        /// <summary>
        /// lista de escopos de blocos de instruções de bloco;
        /// </summary>
        public List<Escopo> escoposDeBloco = new List<Escopo>();
        public List<int> flags { get; set; }

        public delegate void BuildInstruction(int code, List<Expressao> expressoesDaInstrucao, List<List<Instrucao>> blocos, UmaSequenciaID sequencia);


        public const int EH_OBJETO = 1; //: a atribuica é feita sobre um objeto.
        public const int EH_VETOR = 7; // a atribuicao é feita sobre uma variavel vetor.
        public const int EH_PRPOPRIEDADE_ESTATICA = 4; //a atribuição é feita sobre uma propriedade estatica.
     
        public const int EH_DEFINICAO = 5; //é definição (criação)
        public const int EH_MODIFICACAO = 6; //sem definicao, apenas modificacao do valor.

        private static Dictionary<int, string> dicNamesOfInstructions;
        private static System.Random random = new System.Random();

        public void InitNames()
        {
            
            dicNamesOfInstructions = new Dictionary<int, string>();
            dicNamesOfInstructions = new Dictionary<int, string>();
            dicNamesOfInstructions[ProgramaEmVM.codeAtribution] = "Atribution";
            dicNamesOfInstructions[ProgramaEmVM.codeCallerFunction] = "Caller of a Function";
            dicNamesOfInstructions[ProgramaEmVM.codeDefinitionFunction] = "Definition of a Function";
            dicNamesOfInstructions[ProgramaEmVM.codeIfElse] = "if/else";
            dicNamesOfInstructions[ProgramaEmVM.codeFor] = "for";
            dicNamesOfInstructions[ProgramaEmVM.codeWhile] = "while";
            dicNamesOfInstructions[ProgramaEmVM.codeReturn] = "return";
            dicNamesOfInstructions[ProgramaEmVM.codeContinue] = "continue flux";
            dicNamesOfInstructions[ProgramaEmVM.codeBreak] = "break flux";
            dicNamesOfInstructions[ProgramaEmVM.codeGetObjeto] = "GetObjeto";
            dicNamesOfInstructions[ProgramaEmVM.codeSetObjeto] = "SetVar";
            dicNamesOfInstructions[ProgramaEmVM.codeOperadorBinario] = "operador binario";
            dicNamesOfInstructions[ProgramaEmVM.codeOperadorUnario] = "operador unario";
            dicNamesOfInstructions[ProgramaEmVM.codeCasesOfUse] = "casesOfUse";
            dicNamesOfInstructions[ProgramaEmVM.codeCreateObject] = "Create a Object";
            dicNamesOfInstructions[ProgramaEmVM.codeCallerMethod] = "Call a method";
            dicNamesOfInstructions[ProgramaEmVM.codeExpressionValid] = "Valid Express";
            dicNamesOfInstructions[ProgramaEmVM.codeConstructorUp] = "Constructor base";
            dicNamesOfInstructions[ProgramaEmVM.codeAspectos] = "aspect insertion";

    
        }


        public delegate Instrucao handlerCompilador(UmaSequenciaID sequencia, Escopo escopo);
        
        /// POSSIBILITA A EXTENSÃO DE INSTRUÇÕES DA LINGUAGEM, REUNINDO EM UM SÓ LUGAR TODOS OBJETOS NECESSÁRIOS PARA IMPLEMENTAR UMA NOVA INSTRUÇÃO.

        
        /// adiciona um novo tipo de instrução, com id identificador, texto contendo a sintaxe da instrução, e um método para processamento da instrução, e
        /// um metodo para comiplar a instrucao, e também uma sequencia id para reconhecer a instrucao, no compilador.
        public void AddNewTypeOfInstruction(int code, string templateInstruction,ProgramaEmVM.HandlerInstrucao instruction, string sequenciaID_mapeada, handlerCompilador buildCompilador)
        {
            dicNamesOfInstructions[code] = templateInstruction;
            ProgramaEmVM.dicHandlers[code] = instruction;
        }

   
        public Instrucao()
        {
            // do nothing, for instructions processed, but not needs execution in run-time.
        }

        /// <summary>
        /// construtor. contém os elementos de uma instrução VM: codigo ID, expressoes associadas, e blocos de instruções.
        /// </summary>
        /// <param name="code">id de identificacao da instrucao.</param>
        /// <param name="expressoesDaInstrucao">lista de expressoes da instrucao.</param>
        /// <param name="blocos">blocos de instrucao da instrucao.</param>
        public Instrucao(int code, List<Expressao> expressoesDaInstrucao, List<List<Instrucao>> blocos)
        {
            this.flags = new List<int>();
            if (dicNamesOfInstructions == null)
                this.InitNames();
         
            this.code = code;
            if ((expressoesDaInstrucao != null) && (blocos != null))
            {
                this.expressoes = expressoesDaInstrucao.ToList<Expressao>();
                this.blocos = blocos.ToList<List<Instrucao>>();
            }  // if
            else
            {
                this.expressoes = new List<Expressao>();
                this.blocos = new List<List<Instrucao>>();
            }

        } // Instrucao()


        /// <summary>
        /// construtor. contém os elementos de uma instrução VM: codigo ID, expressoes associadas, e blocos de instruções.
        /// </summary>
        /// <param name="code">id de identificacao de tipo da instrucao.</param>
        /// <param name="expressoesDaInstrucao">lista de expressoes da instrucao.</param>
        /// <param name="blocos">blocos de instrucoes da instrucao.</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        public Instrucao(int code, List<Expressao> expressoesDaInstrucao, List<List<Instrucao>> blocos, Escopo escopo)
        {
            this.flags = new List<int>();
            if (dicNamesOfInstructions == null)
                this.InitNames();
            this.code = code;
            if ((expressoesDaInstrucao != null) && (blocos != null))
            {
                this.expressoes = expressoesDaInstrucao;
                this.blocos = blocos.ToList<List<Instrucao>>();
            }  // if

        } // Instrucao()


        /// <summary>
        /// funcao aditiva, permitindo que a lista de instruções da VM possa ser extendida.
        /// </summary>
        /// <param name="novoTipoInstrucao"></param>
        /// <param name="code"></param>
        public void AddTipoInstrucao(ProgramaEmVM.HandlerInstrucao novoTipoInstrucao, int code)
        {
            // para inserir um novo comando, construa a string de definicao, o metodo Build, o indice de codigo, e o metodo de execução do comando.
            ProgramaEmVM.dicHandlers[code] = novoTipoInstrucao;
        }

        
        public override string ToString()
        {
            string str = dicNamesOfInstructions[this.code].ToString() + "   ";
            if ((this.expressoes != null) && (this.expressoes.Count > 0))
            {
                for (int i = 0; i < this.expressoes.Count; i++)
                {
                    if (this.expressoes[i] != null)
                    {
                        str+= this.expressoes[i] + "  ";
                    }
                }
            }
            return str;
            
            
        }






        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("teste de execução de instruçoes orquidea")
            {
            }


            public void TestesProgramaComFuncoesEClasseOrquideaEObjetoActual(AssercaoSuiteClasse assercao)
            {
                string pathFile = @"programasTestes\programaClasseContador.txt";
                ParserAFile.ExecuteAProgram(pathFile);
            }
                 


            public void TestesUnitariosInstrucaoCasesOfUse(AssercaoSuiteClasse assercao)
            {
                // sintaxe: "casesOfUse y: { (case < x): { y = 2; }; } ";
                // instrucao cases of use.                           
                string code_0_0 = "int x = 5;  int y= 1;  casesOfUse y:  { (case  < x): { y = 2; }}";
                string code_0_1 = "int x = 5;  int y= 1;  casesOfUse y:  { (case >= x): { y = 2; }}";
                string code_0_2 = "int x = -5; int y= 1;  casesOfUse y:  { (case == x): { y = 2; }}";
                string code_0_3 = "int x = 1;  int y= 1;  casesOfUse y:  { (case == x): { y = 2; }}";

                string value_expected_0_0 = "2";
                string value_expected_0_1 = "1";
                string value_expected_0_2 = "1";
                string value_expected_0_3 = "2";

                List<string> codesCaseOfUse = new List<string>() { code_0_1, code_0_0, code_0_2, code_0_3 };
                List<string> values_expected = new List<string>() {value_expected_0_1, value_expected_0_0, value_expected_0_2, value_expected_0_3 };

                for (int i = 0; i < codesCaseOfUse.Count; i++)
                {
                    SystemInit.InitSystem();
                    ProcessadorDeID compilador = new ProcessadorDeID(codesCaseOfUse[i]);
                    compilador.Compilar();


                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    try
                    {
                        assercao.IsTrue(ValidateValueObjects(compilador.escopo, "y", values_expected[i]));
                    }
                    catch (Exception ex)
                    {
                        string codeError = ex.Message;
                        assercao.IsTrue(false, "TESTE FALHOU");
                    }
                }
            }



            public void TestesUnitariosFor(AssercaoSuiteClasse assercao)
            {


                string code_0_2 = "int x=-2; for (int i = 0; i< 5 ; i++){ x=x+1;}";
                string code_0_0 = "int x=1; for (int i = 0; i< -5 ; i++){ x=x+1;}";
                string code_0_1 = "int x=0; for (int i = 0; i< 5 ; i++){ x=x+1;}";


                string valueExpected_0_2 = "3";
                string valueExpected_0_0 = "1";
                string valueExpected_0_1 = "5";

                List<string> codesFor = new List<string>() { code_0_2, code_0_1, code_0_0 };
                List<string> valuesExpected = new List<string> { valueExpected_0_2, valueExpected_0_1, valueExpected_0_0 };

                for (int i = 0; i < codesFor.Count; i++)
                {
                    SystemInit.InitSystem();
                    ProcessadorDeID compilador = new ProcessadorDeID(codesFor[i]);
                    compilador.Compilar();




                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    try
                    {
                        assercao.IsTrue(ValidateValueObjects(compilador.escopo, "x", valuesExpected[i]), codesFor[i]);
                    }
                    catch (Exception e)
                    {
                        assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                    }
                }
            }



            public void TesteInstrucaoWhile(AssercaoSuiteClasse assercao)
            {

                string code_0_0 = "int x=3; while (x<4){x=x+1;}";
                string code_0_1 = "int x=3; while (x>4){x=x+1;}";
                string code_0_2 = "int x=5; while (x<=6){x=x+5;}";
                string code_0_3 = "int x=6; while (x>4){x=x-1;}";

                string result_0_0 = "4";
                string result_0_1 = "3";
                string result_0_2 = "10";
                string result_0_3 = "4";


                List<string> codes = new List<string>() { code_0_1, code_0_0, code_0_2, code_0_3 };
                List<string> resultExpected = new List<string>() { result_0_1, result_0_0, result_0_2, result_0_3 };


                for (int i = 0; i < codes.Count; i++)
                {
                    SystemInit.InitSystem();
                    ProcessadorDeID compilador = new ProcessadorDeID(codes[i]);
                    compilador.Compilar();

                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);
                    try
                    {
                        assercao.IsTrue(ValidateValueObjects(compilador.escopo, "x", resultExpected[i]), codes[i]);

                    }
                    catch (Exception ex)
                    {
                        assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                    }

                }



            }


            public void TesteInstrucaoIf(AssercaoSuiteClasse assercao)
            {

                string code_0_0 = "int x=1; if (x<2){x=5;}else{x=6;}";
                string code_0_1 = "int x=5; if (x>6){x=-1;} else {x=7;}";
                string code_0_2 = "int x=-1; if (x<-1){x=3;}else{x=5;}";
                string code_0_3 = "int x=5; if (x<6){x=-1;} else {x=2;}";

                string result_0_0 = "5";
                string result_0_1 = "7";
                string result_0_2 = "5";
                string result_0_3 = "-1";


                List<string> codes = new List<string>() { code_0_2, code_0_0, code_0_1, code_0_3 };
                List<string> result_expected = new List<string>() { result_0_2, result_0_0, result_0_1, result_0_3 };

                for (int x = 0; x < codes.Count; x++)
                {
                    SystemInit.InitSystem();
                    ProcessadorDeID compilador = new ProcessadorDeID(codes[x]);
                    compilador.Compilar();

                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);


                    try
                    {
                        assercao.IsTrue(ValidateValueObjects(compilador.escopo, "x", result_expected[x]));
                    }
                    catch (Exception ex)
                    {
                        assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                    }
                }
            }

 

            /// <summary>
            /// funcao de validacao, tendo como verificacao de uma variavel que é modificada no cenario de teste.
            /// </summary>
            /// <param name="escopo">contexto onde a variavel esta.</param>
            /// <param name="nameObject">nome da variavel.</param>
            /// <param name="valueExpected">valor esperado</param>
            /// <returns>[true] se o valor esperado é o mesmo valor da variavel</returns>
            private bool ValidateValueObjects(Escopo escopo, string nameObject, string valueExpected)
            {
                return escopo.tabela.GetObjeto(nameObject, escopo).valor.ToString() == valueExpected;
            }


    
      
            public void TesteInstrucaoContrutorUP(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigo1 = "public class classeA { public classeA(int x, int y){int z=1;}}";
                string codigo2 = "public class classeB: + classeA { public classeB(){ int b=1; } }";
                string codigoInstrucao = "classeB.construtorUP(classeA, 1, 5);";

                ProcessadorDeID compilador = new ProcessadorDeID(codigo1 + " " + codigo2 + " " + codigoInstrucao);

                compilador.Compilar();


                try
                {
                    assercao.IsTrue(
                    RepositorioDeClassesOO.Instance().GetClasse("classeA") != null &&
                    RepositorioDeClassesOO.Instance().GetClasse("classeB") != null &&
                    compilador.GetInstrucoes()[0].code == ProgramaEmVM.codeConstructorUp, codigo1+"  "+codigo2);

                }
                catch(Exception ex)
                {
                    assercao.IsTrue(false, "teste falhou: " + ex.Message);
                }




                ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                programa.Run(compilador.escopo);

                // teste executado sem erros fatais.
                assercao.IsTrue(true);
            }

            public void TesteDefinicaoDeMetodoEVerificacaoDeConstrucaoDoEscopoDoMetodo(AssercaoSuiteClasse assercao)
            {
                string codigo = "public class classeB { public classeB(){int x= 1;};  public int metodoB(int x, int y){ int z=1;  return x+y;} }";

                SystemInit.InitSystem();

                ProcessadorDeID compilador = new ProcessadorDeID(codigo);
                compilador.Compilar();

                try
                {
                    assercao.IsTrue(
                        RepositorioDeClassesOO.Instance().GetClasse("classeB").GetMetodo("metodoB")[0].escopoCORPO_METODO.tabela.GetObjeto(
                        "z", RepositorioDeClassesOO.Instance().GetClasse("classeB").GetMetodo("metodoB")[0].escopoCORPO_METODO) != null, codigo);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: "+ex.Message);
                }

            }



  
            public void TesteInstrucaoImporter(AssercaoSuiteClasse assercao)
            {
                // verifica se as classes base importadas foram realmente importadas, durante a compilacao.
                // numero de classes importada diminuida, devido melhor escopo da linguagem.
                string codigo = "public class classeB { public classeB(){int x= 1;};  public int metodoB(int x, int y){ int z=1;  return x+y;} }; importer (ParserLinguagemOrquidea.exe);";

                SystemInit.InitSystem();

                ProcessadorDeID compilador = new ProcessadorDeID(codigo);
                compilador.Compilar();

                try
                {
                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasses().Count > 8, codigo);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "teste falhou: "+ex.Message);
                }

            }

    
  

           

  

        } // class Testes

        public void TestesPequenosProgramasTextos(AssercaoSuiteClasse assercao)
        {
            string pathProgramaLeNome = "programasTestes\\programaLeNome.txt";

            List<string> lstPrograms = new List<string>() { pathProgramaLeNome };

            for (int i = 0; i < lstPrograms.Count; i++)
            {
                SystemInit.InitSystem();

                ParserAFile program = new ParserAFile(lstPrograms[i]);
                ProcessadorDeID compilador = new ProcessadorDeID(program.GetTokens());
                compilador.Compilar();



                ProgramaEmVM programaVM = new ProgramaEmVM(compilador.GetInstrucoes());
                Escopo escopo = compilador.escopo.Clone();
                programaVM.Run(escopo);

            }


        }

        public void TestesPequenosProgramas(AssercaoSuiteClasse assercao)
        {
            string pathProgramaNumeroParOuImpar = @"programasTestes\programaNumeroParOuImpar.txt";
            string pathProgramaFatorialRecursivo = "programasTestes\\programaFatorialRecursivo.txt";
            string pahtProgramaFatorial = "programasTestes\\programaFatorial.txt";
            string pathProgramaContagens = "programasTestes\\programContagens.txt";


            List<string> fileProgramasTestes = new List<string>() { pathProgramaContagens, pathProgramaNumeroParOuImpar, pathProgramaFatorialRecursivo, pahtProgramaFatorial };
            List<string> resultsExpected = new List<string>() { "5", "1", "120", "120" };
            List<string> objectsResult = new List<string>() { "y", "y", "y", "y" };



            for (int x = 0; x < fileProgramasTestes.Count; x++)
            {
                try
                {
                    SystemInit.InitSystem();

                    ParserAFile program = new ParserAFile(fileProgramasTestes[x]);
                    ProcessadorDeID compilador = new ProcessadorDeID(program.GetTokens());
                    compilador.Compilar();


                    ProgramaEmVM programaVM = new ProgramaEmVM(compilador.GetInstrucoes());
                    programaVM.Run(compilador.escopo);


                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto(objectsResult[x], compilador.escopo).valor.ToString() == resultsExpected[x].ToString());

                }
                catch (Exception e)
                {
                    string msgError = e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU!");
                }
            }

        }




    } // class Instrucao



} //  namespace paser
