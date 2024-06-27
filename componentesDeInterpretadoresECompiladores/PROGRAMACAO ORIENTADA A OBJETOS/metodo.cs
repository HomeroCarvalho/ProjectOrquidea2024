using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using parser.ProgramacaoOrentadaAObjetos;
using System.Reflection;
using Modulos;
using System.Diagnostics.Metrics;
using static System.Formats.Asn1.AsnWriter;
using System.Runtime.Intrinsics;
using System.ComponentModel.Design;

namespace parser
{

    public class Metodo: Objeto
    {
        /// <summary>
        /// acessor: public, protected, private. por default, setado para public
        /// </summary>
        public new string acessor = "public";


        /// <summary>
        /// nome do metodo.
        /// </summary>
        public new string nome;


        /// <summary>
        /// nome longo do metodo (classe.nomeMetodo). para diferenciar em caso de metodos herdados de
        /// mesmo nome, possibilitando chamar tanto 1 quanto o outro, utilizando o metodo longo.
        /// </summary>
        public string nomeLongo;

        /// <summary>
        /// nome da classe do metodo.
        /// </summary>
        public string nomeClasse;
        


        public bool isCompiled = false;


        /// <summary>
        /// prioridade de execução em expressões.
        /// </summary>
        public int prioridade;


        public new string tipo;

        /// <summary>
        /// id do scopo, dentro do escopo da familia.
        /// </summary>
        public int idEscopo = -1;

        /// <summary>
        /// parâmetros do metodo.
        /// </summary>
        public Objeto[] parametrosDaFuncao;
        
        


        /// <summary>
        /// nome do tipo do objeto de retorno da função.
        /// </summary>
        public string tipoReturn;

        /// <summary>
        /// se true, o objeto caller (que chamou o metodo) é incluido na lista de parâmetros.
        /// </summary>
        public bool isToIncludeCallerIntoParameters = false;



        /// <summary>
        /// se [true], ativa o modo de depuração de variaveis.
        /// </summary>
        public static bool isDEBUG = false;


        /// <summary>
        /// tokens das instrucoes do metodo, se for um metodo da linguagem orquidea, nao metodo importado.
        /// </summary>
        public List<string> tokens = new List<string>();

        /// <summary>
        /// retorna true se o metodo é importado.
        /// </summary>
        public bool isMethodImported
        {
            get
            {
                return InfoMethod != null;
            }
        }

        /// <summary>
        /// o indice deste metodo perante aos outros metodos da classe.
        /// </summary>
        public int indexInClass;


  

        //  PROTOTIPOS DE FUNÇÕES IMPLEMENTADORAS.

        /// <summary>
        ///  prototipo para funções genérica.
        /// </summary>
        /// <param name="parametros">lista de parametros object.</param>
        /// <returns></returns>
        public delegate object FuncaoGeral(params object[] parametros);


        /// <summary>
        /// prototipo de execução da função, implementada na liguagem utilizada na construção do compilador;
        /// </summary>
        public delegate void CallFunction();


        private int OBJETO_PARAMETER_INCLUSO = 1;
        private int NO_OBJETO_PARAMETER = 0;




        /// <summary>
        /// DADOS DE METODO IMPORTADO DA LINGUAGEM BASE;
        /// </summary>
        public MethodInfo InfoMethod { get; set; }

        /// <summary>
        /// se [true] a funcao é importada da Linguagem Base.
        /// </summary>
        private bool isImportingMethod
        {
            get
            {
                return this.InfoMethod != null;
            }
        }


        /// <summary>
        /// CONSTRUTOR DE CLASSES IMPORTADA DA LINGUAGEM BASE, SEM PARAMETROS.
        /// </summary>
        public ConstructorInfo InfoConstructor;



        /// <summary>
        /// contém as INSTRUÇÕES DE CODIGO ORQUIDEA.
        /// </summary>
        public List<Instrucao> instrucoesFuncao;
        /// <summary>
        /// OBJETO QUE ESTÁ INVOCANDO A FUNÇÃO: p.ex., obj1.metodo1(): obj1 é o objeto caller.
        /// </summary>
        public object caller = null; 


        /// <summary>
        /// escopo interno do metodo, contendo variaveis, objetos e funcoes dentro do corpo do metodo.
        /// </summary>
        public Escopo escopoCORPO_METODO;


        /// <summary>
        /// escopo total do metodo: escopo externo, escopo dos parametros, escopo do corpo do metodo.
        /// </summary>
        public Escopo escopoSeSSAODoMetodo;


        /// <summary>
        /// contem o objeto caller desta função.
        /// </summary>
        public new Objeto actual;


        /// <summary>
        /// adiciona um parametro na lista de parãmetros.
        /// util quando o parametro for um metodo, com tipo de retorno, classe, e parametros definidos.
        /// </summary>
        /// <param name="parameter">parametro a ser adicionado.</param>
        public void AddParameter(Objeto parameter)
        {
            if (this.parametrosDaFuncao == null)
            {
                this.parametrosDaFuncao = new Objeto[1];

            }
            List<Objeto> paramsList= this.parametrosDaFuncao.ToList();
            paramsList.RemoveAt(0);
            paramsList.Add(parameter);
            this.parametrosDaFuncao= paramsList.ToArray();
        }


        /// <summary>
        /// em implementações anteriores, para distinguir metodos herdados, de metodos atuais,
        /// foi de design setar o nome de metodos como nome longo (nomeClasse@nomeMetodo).
        /// se descontinuidade.
        /// </summary>
        /// <param name="classeDoMetodo">nome da classe a qual o metodo pertence,</param>
        public void SetNomeLongo(string classeDoMetodo)
        {
            // muda o nome do metodo para o nome longo (nomeDaClasseDoMetodo+"."+ nomeDoMetodo).
            this.nomeLongo = classeDoMetodo + "@" + nome;
            this.nome = this.nomeLongo;
        }

        /// <summary>
        /// executa o construtor de uma classe importada.
        /// </summary>
        /// <param name="tipoObjetoConstruir">tipo do objeto a construir, da classe importada</param>
        /// <param name="parametros">parametros ao construtor.</param>
        /// <param name="indexConstrutor">indice do construtor</param>
        /// 
        /// <returns>retorna um objeto construido da classe do objCaller.</returns>
        public static object ExecuteAConstructorImportado(Type tipoObjetoConstruir, List<object> parametros, int indexConstrutor)
        {
            if (tipoObjetoConstruir == null)
            {
                return null;
            }
            else
            {
                // obtem os construtores da classe do objeto a construir.
                ConstructorInfo[] construtores = tipoObjetoConstruir.GetConstructors();
                if (indexConstrutor >= 0)
                {
                    // chama o construtor.
                    object objectBuild = construtores[indexConstrutor].Invoke(parametros.ToArray());


                    // retorna o objeto construido.
                    return objectBuild;

                }
                else
                {
                    return null;
                }
            }
            
        }


        /// <summary>
        /// faz a avaliacao de uma funcao importada.
        /// </summary>
        /// <param name="objCaller">objeto que chamou a funcao/metodo.</param>
        /// <param name="valoresParametros">parametros da funcao.</param>
        /// <param name="escopo">contexto onde os paremetros e o objeto caller estão.</param>
        /// <returns>retorna a saida da funcao.</returns>
        public object ExecuteAFunctionImportada(ref Objeto objCaller, List<object> valoresParametros, Escopo escopo)
        {

           


            //  PROCESSAMENTO DE OBJETOS IMPORTADOS, QUE NAO SEJAM STRING OU DOUBLE.
            if ((objCaller != null) && (objCaller.tipo != "string") && (objCaller.tipo != "double") && (objCaller.valor != null))   
            {
                
                
                for (int i = 0; i < valoresParametros.Count; i++)
                {
                    if (valoresParametros[i] == null)
                    {
                        return null;
                    }
                    if (valoresParametros[i].GetType() == typeof(Objeto))
                    {
                        valoresParametros[i] = ((Objeto)valoresParametros[i]).valor;
                    }
                }


                if ((valoresParametros == null) || (valoresParametros.Count == 0))
                {
                    this.caller = objCaller.valor;
                    object result1 = this.InfoMethod.Invoke(this.caller, new object[] { });

                    return result1;
                }

                this.caller = objCaller.valor;
                
                
                // executa a função.
                object result = this.InfoMethod.Invoke(this.caller, valoresParametros.ToArray());

        

                return result;
            }
          
            
       
            // OBTEM CONSTRUTORES DA CLASSE IMPORTADA.
            Type classObjectCaller = this.InfoMethod.DeclaringType;
            ConstructorInfo[] construtores = classObjectCaller.GetConstructors();

            // construtores para classe string e double. essas classes não tem seus metodos dentro da propria classe, estão em
            // classes diferentes.
            if   ((objCaller.tipo == "string") || (objCaller.tipo == "double"))    
            {


                // A FUNCAO É UM CONSTRUTOR.
                if (objCaller.nome == this.nome) 
                {

                    // constroi a lista de types dos parametros, para executar o construtor.
                    List<Type> tiposParametros= new List<Type>();
                    for (int i = 0; i < parametrosDaFuncao.Length; i++)
                    {
                        tiposParametros.Add(parametrosDaFuncao[i].GetType());
                        
                    }

                    // chama o construtor, com uma lista de parametros.
                    if (tiposParametros.Count > 0)
                    {
                        this.caller = classObjectCaller.GetConstructor(tiposParametros.ToArray()).Invoke(valoresParametros.ToArray());
                        
                    }
                    else
                    {
                        // caso de nao tiver parametros, chama um construtor sem lista de parametros.
                        this.caller = classObjectCaller.GetConstructor(new Type[] { }).Invoke(null);
                    }
                }
                else
                {
                  
                    // obtem um objeto da classe que construiu o operador.
                    this.caller = classObjectCaller.GetConstructor(new Type[] { }).Invoke(null);
                }
                
            }
           
            if (this.caller != null)
            {
                object result1 = this.InfoMethod.Invoke(this.caller, valoresParametros.ToArray());
                objCaller.valor = result1;
                return result1;
            }
            return null;

        }


        private List<Objeto> stackActual= new List<Objeto>();


        /// <summary>
        /// executa uma funcao orquidea, parametros estão dentro do escopo sessao do metodo, assim como
        /// variaveis globais, variaveis do corpo do metodo, propriedades do objeto caller.
        /// </summary>
        /// <param name="escopoFunctionOrquidea">escopo onde o metodo está.</param>
        /// <returns></returns>
        public object ExecuteAFunctionOrquidea(Escopo escopoFunctionOrquidea, Objeto objCaller)
        {
            if (nome == "")
            {
                return null;
            }

            

            if ((this.instrucoesFuncao == null) || (this.instrucoesFuncao.Count == 0))
            {
                Classe classeDaFuncao = RepositorioDeClassesOO.Instance().GetClasse(this.nomeClasse);
                List<Metodo> metodo = classeDaFuncao.GetMetodo(this.nome);
                this.instrucoesFuncao = metodo[0].instrucoesFuncao;
            }




            // se o objeto caller nao estiver registrado no escopo sessao, registra.
            if ((objCaller!=null) && (escopoFunctionOrquidea.tabela.GetObjeto(objCaller.nome, escopoFunctionOrquidea) == null))
            {
                escopoFunctionOrquidea.tabela.RegistraObjeto(objCaller);
            }





            // avaliação da função via instruções da linguagem orquidea.
            if ((this.instrucoesFuncao != null) && (this.instrucoesFuncao.Count > 0))
            {
                // cria uma instancia de processamento de codigo, com as instruções da função.
                ProgramaEmVM program = new ProgramaEmVM(this.instrucoesFuncao);
                // CHAMA A FUNCAO.
                program.Run(escopoFunctionOrquidea);




                if (objCaller != null)
                {
                    // ATUALIZACAO DO OBJETO CALLER, DE SUAS PROPRIEDADES PROCESSADO NA FUNCAO.
                    // atualiza o objeto caller, com as suas propriedades atualizadas presente no escopo.
                    List<Objeto> propriedades = objCaller.propriedades;
                    if (propriedades != null)
                    {
                        for (int i = 0; i < propriedades.Count; i++)
                        {
                            int indexProp = escopoFunctionOrquidea.tabela.objetos.FindIndex(k => k.nome == propriedades[i].nome);
                            if (indexProp >= 0)
                            {
                                objCaller.propriedades[i] = escopoFunctionOrquidea.tabela.objetos[indexProp];
                                objCaller.propriedades[i].valor = escopoFunctionOrquidea.tabela.objetos[indexProp].valor;
                            }
                        }

                        escopoFunctionOrquidea.tabela.UpdateObjeto(objCaller);
                    }


                }




                object result = program.lastReturn;

                return result;
            } // if
            else
            {
                return null;
            }
        }

        /// <summary>
        ///  avalia uma função, tendo como parâmetros expressões, 
        /// </summary>
        public object ExecuteAFunction(List<Expressao> parametros, object caller, Escopo escopoDaFuncao)
        {

            if (this.InfoMethod == null)
            {
                Objeto objCaller= (Objeto)caller;
                return ExecuteAMethod(parametros, escopoDaFuncao, ref objCaller, Metodo.IS_METHOD);
            }
                      

            List<object> valoresParametro = new List<object>();
            EvalExpression eval = new EvalExpression();

            for (int x = 0; x < parametros.Count; x++)
            {
                // possivel inclusao do escopo como um dos parametros.
                if (parametrosDaFuncao[x].GetTipo()=="Escopo")
                {
                    valoresParametro.Add(escopoDaFuncao);
                }
                else
                {


                    parametros[x].isModify = true;
                    object umValor = eval.EvalPosOrdem(parametros[x], escopoDaFuncao);

                   

                    if ((umValor != null) && (umValor.GetType() == typeof(Objeto)))
                    {
                        valoresParametro.Add(((Objeto)umValor).GetValor());
                    }
                    else
                    {
                        valoresParametro.Add(umValor);
                    }
                        
                  
                        
                }
            } // for x

            // constroi a lista de parametros object, para UMA FUNCAO IMPORTADA.
            List<object> parametrosValoresAtuais = new List<object>();
            if (this.parametrosDaFuncao != null)
            {
                int parametrosDaChamada = 0;
                for (int x = 0; x < parametrosDaFuncao.Length;  x++, parametrosDaChamada++)
                {
                    // processamento de multi-argumentos.
                    if (parametrosDaFuncao[x].isMultArgument)
                    {
                        
                        string tipoDoMultiArgumento = parametrosDaFuncao[x].GetTipo();
                        List<object> paramsMult = new List<object>();
                        int indexBegin = parametrosDaChamada;
                        
                        // armazena os valores dos multi-argumentos numa lista-array
                        while ((indexBegin<parametros.Count) && (parametros[indexBegin].tipoDaExpressao == tipoDoMultiArgumento))
                        {
                            // calculo do valor da expressao parametro, afim de adicionar este valor numa lista contendo objetos multi-argumento.
                            object valorParam = eval.EvalPosOrdem(parametros[indexBegin], escopoCORPO_METODO);

                            paramsMult.Add(valorParam);
                            indexBegin++;
                        }

                        
                        // construcao de um vetor como container dos parametros multi-argumento.
                        parser.Vector arrayParametrosMultiArgumentos = new parser.Vector(tipoDoMultiArgumento);
                        arrayParametrosMultiArgumentos.SetNome(parametrosDaFuncao[x].GetNome());
                        for (int p = 0; p < paramsMult.Count; p++)
                        {
                            arrayParametrosMultiArgumentos.pushBack(paramsMult[p]);
                        }

                        // remove o parametro curretne, a fim de inserção do array de parametros multi-argumento. é provável que o parametro currente seja um vetor também.
                        parametrosDaFuncao.ToList<Objeto>().RemoveAt(x);
                        parametrosDaFuncao.ToList<Objeto>().Insert(x, arrayParametrosMultiArgumentos);


                        // adiciona o parametro para o escopo do metodo.
                        escopoDaFuncao.tabela.GetObjetos().Add(parametrosDaFuncao[x]);
                        // adiciona oo parametro para a lista de valores atuais.
                        parametrosValoresAtuais.Add(parametrosDaFuncao[x]);


                        // atualiza o indice de malha de parametros da chamada de metodo.
                        parametrosDaChamada += paramsMult.Count;


                        // se todos parametros da chamada foram processados, para a malha.
                        if (parametrosDaChamada >= parametros.Count)
                        {
                            break;
                        }
                    }
                    else
                    {   // caso em que não é um parametro multi-argumento.
                        parametrosDaFuncao[x].SetValor(valoresParametro[x]);
                        escopoDaFuncao.tabela.GetObjetos().Add(parametrosDaFuncao[x]);

                        parametrosValoresAtuais.Add(parametrosDaFuncao[x]);

                    }

                }

            }
                

            object resultCalcFuncao = null;
            // EXECUTA UMA FUNCAO IMPORTADA.
            if ((this.InfoMethod != null) && (caller != null))

            {
                resultCalcFuncao = this.InfoMethod.Invoke(caller, valoresParametro.ToArray());
            }
                




            return resultCalcFuncao;
        }


        /// <summary>
        /// Modos de execucao da função:
        ///    1- via instruções da linguagem orquidea.
        ///         é criado uma copia do escopo da classe, na chamada de método. como só há chamadas de métodos,
        ///         o escopo é sempre clonado, evitando chamadas intercaladas do método, resultando em erros de instanciação
        ///         dos objets do escopo.
        ///    2- via metodo via reflexao (é preciso setar o objeto que fará a chamada do método).
        /// </summary>
        public object ExecuteAFunction(Objeto objCaller, List<object> valoresDosParametros, Escopo escopo)
        {

           

            // avaliação da função via instruções da linguagem orquidea.
            if ((this.instrucoesFuncao != null) && (this.instrucoesFuncao.Count > 0))
            {
                // cria uma instancia de processamento de codigo, com as instruções da função.
                ProgramaEmVM program = new ProgramaEmVM(this.instrucoesFuncao);
                program.Run(escopo);


                object result = program.lastReturn;

                return result;
            } // if 
            else
            // avaliação de função via método importado com API Reflexão.
            if (this.InfoMethod != null)
            {



                object result1 = new object();
                Type classObjectCaller = this.InfoMethod.DeclaringType;
                ConstructorInfo[] construtores = classObjectCaller.GetConstructors();
                if ((construtores.Length > 0) && (!(caller is Objeto)))
                {
                    // obtem um objeto da classe que construiu o operador.
                    this.caller = classObjectCaller.GetConstructor(new Type[] { }).Invoke(null);
                }
                if ((valoresDosParametros != null) && (valoresDosParametros.Count > 0))
                {

                    result1 = this.InfoMethod.Invoke(this.caller, valoresDosParametros.ToArray());
                    
                    // se o objeto caller for um wrapper object, seta o seu valor.
                    if (WrapperData.isWrapperObject(this.caller))
                    {
                        Objeto objetoWrapper = null;
                        WrapperData.CastingObjeto(this.caller, ref objetoWrapper);

                        string nomeObjetoCaller = objCaller.nome;
                        objCaller = objetoWrapper;
                        objCaller.nome = nomeObjetoCaller;
                        List<Objeto> objetosNoEscopo = escopo.tabela.objetos;
                        if (objetosNoEscopo != null)
                        {
                            for (int i = 0; i < objetosNoEscopo.Count; i++)
                            {
                                if (objetosNoEscopo[i].nome == objCaller.nome)
                                {
                                    escopo.tabela.objetos[i] = objCaller;
                                    break;
                                }
                            }
                        }
                    }
                  
                }
                else
                {
                    result1 = this.InfoMethod.Invoke(this.caller, null);
                }

                return result1;
            }
            else
                return null;

        } // ExecuteAFunction()



        /// <summary>
        /// constroi um objeto com um construtor sem parametros. utilidade para objetos estáticos.
        /// </summary>
        /// <param name="tipoDoObjeto">tipo do objeto a construir.</param>
        /// <returns></returns>
        public static object BuildObjectFromConstrutorWithoutParameters(Type tipoDoObjeto)
        {

            if (tipoDoObjeto == null)
            {
                return null;
            }

            ConstructorInfo[] construtores= tipoDoObjeto.GetConstructors();
            if ((construtores==null) || (construtores.Length==0)) { return null; }
            else
            {
                for (int x=0;x<construtores.Length;x++)
                {
                    if ((construtores[x].GetParameters() == null) || (construtores[x].GetParameters().Length == 0))
                    {
                        return construtores[x].Invoke(null); 
                    }
                }
            }

            return null;

        }

       

        public static int IS_CONSTRUCTOR = 1;
        public static int IS_METHOD = 2;

   

        /// <summary>
        /// executa um metodo, com parametros e objeto caller que invoca a expressao chamada de metodo.
        /// </summary>
        /// <param name="paramsChamadaDeMetodo">parametros vindos da chamada de metodo, 
        /// ex.: a.metodoB(1,5), 1, e 5 são parametros da chamada de metodo metodB..</param>
        /// <param name="escopoExterno">contexto onde os parametros expressao estão.</param>
        /// <param name="objetoCaller">objeto que fez a chamada de metodo, ex.: a.metodoA(1), "a" é o objeto caller.</param>
        /// <returns></returns>
        public object ExecuteAMethod(List<Expressao> paramsChamadaDeMetodo, Escopo escopoExterno, ref Objeto objetoCaller, int flagBuild)
        {

          

            Escopo.escopoCURRENT = escopoExterno;


            
            

            // contem os objetos parametros.
            // valotes dos parametros, calculado após a avaliação das expressões parâmetros.
            List<object> valoresDosParametros = new List<object>();
            List<Objeto> PARAMETROS = new List<Objeto>();




            // A FUNCAO NAO É IMPORTADA. (FUNCAO ORQUIDEA)
            if (!isImportingMethod)
            {

               

                // INSTANCIA O ESCOPO DE SESSAO DO METODO. uniao de todos escopos: escopo externo, escopo do corpo do metodo,
                // escopo de parametros da função, escopo de propriedades estáticas.
                 if (escopoSeSSAODoMetodo == null)
                {
                    escopoSeSSAODoMetodo = new Escopo("");
                }

                escopoSeSSAODoMetodo.tabela.objetos.Clear();
               



               

                // COPIA POR REFERENCIA O ESCOPO DA FUNCAO, PARA O ESCOPO ESTÁTICO, a fim de avaliar expressoes sem o parametro Escopo.
                Escopo.escopoCURRENT = escopoSeSSAODoMetodo;


                // parametros 1o. pois será o objeto que vale, em caso de objetos de mesmo nome.
                if ((parametrosDaFuncao != null) && (parametrosDaFuncao.Length > 0))
                {
                    for (int i = 0; i < parametrosDaFuncao.Length; i++)
                    {
                        if (escopoExterno.tabela.GetObjeto(parametrosDaFuncao[i].nome, escopoExterno) != null)
                        {
                            parametrosDaFuncao[i].valor = escopoExterno.tabela.GetObjeto(parametrosDaFuncao[i].nome, escopoExterno).valor;
                        }
                        else
                        {
                            escopoExterno.tabela.RegistraObjeto(parametrosDaFuncao[i]);
                        }
                        
                        escopoSeSSAODoMetodo.tabela.RegistraObjeto(parametrosDaFuncao[i]); 
                    }
                }

                // escopo contendo propriedades estáticas.
                JoinEscopoEstatico(escopoSeSSAODoMetodo);

                // escopo contendo os objetos do corpo do metodo.
                JoinEscopos(escopoSeSSAODoMetodo, escopoCORPO_METODO);
              
                // escopo contexto vindo da chamada do metodo.
                JoinEscopos(escopoSeSSAODoMetodo, escopoExterno);

                
               


                // obtem dados atualizados do objeto caller, presente no escopo externo.
                if ((objetoCaller!=null) && (escopoExterno.tabela.GetObjeto(objetoCaller.nome, escopoExterno) != null))
                {
                    objetoCaller = escopoExterno.tabela.GetObjeto(objetoCaller.nome, escopoExterno);
                }


                // obtem as propriedades do objeto caller, afim de colocar no escopo.
                if ((objetoCaller != null)  && (objetoCaller.propriedades != null))
                {
                    for (int i = 0; i < objetoCaller.propriedades.Count; i++)
                    {
                        if (escopoSeSSAODoMetodo.tabela.GetObjeto(objetoCaller.propriedades[i].nome, escopoSeSSAODoMetodo) == null)
                        {
                            escopoSeSSAODoMetodo.tabela.RegistraObjeto(objetoCaller.propriedades[i]);
                        }
                        else
                        {
                            escopoSeSSAODoMetodo.tabela.UpdateObjeto(objetoCaller.propriedades[i]);
                        }
                    }
                }




                

                // CONSTROI os parametros com valores atualizados.
                this.BuildParametros(objetoCaller,
                            paramsChamadaDeMetodo,
                            this.parametrosDaFuncao,
                            ref valoresDosParametros,
                            ref PARAMETROS, this.NO_OBJETO_PARAMETER, escopoSeSSAODoMetodo);


                if (nome == "DrawMeteoro")
                {
                    int k = 0;
                    k++;
                }

                if ((PARAMETROS != null) && (PARAMETROS.Count > 0))
                {
                    escopoSeSSAODoMetodo.tabela.objetos.AddRange(PARAMETROS);
                   
                }




                Escopo.escopoCURRENT = escopoSeSSAODoMetodo;

                //******************************************************************************************************************
                // EXECUTA A FUNCAO.
                object objetoValor = ExecuteAFunctionOrquidea(escopoSeSSAODoMetodo, objetoCaller);
                //*******************************************************************************************************************


                // atualiza o escopo externo, dos parametros da funcao.
                if (parametrosDaFuncao != null)
                {
                    for (int i = 0; i < parametrosDaFuncao.Length; i++)
                    {
                        Objeto umParam= escopoSeSSAODoMetodo.tabela.GetObjeto(parametrosDaFuncao[i].nome, escopoSeSSAODoMetodo);
                        if (umParam!= null)
                        {
                            if (escopoExterno.tabela.GetObjeto(umParam.nome, escopoExterno) != null)
                            {
                                escopoExterno.tabela.UpdateObjeto(umParam);
                            }
                            else
                            {
                                escopoExterno.tabela.RegistraObjeto(umParam);
                            }
                        }
                    }
                }

                if (objetoCaller != null)
                {


                    // ATUALIZA AS PROPRIEDADES DO OBJETO CALLER, COM DADOS ARMAZENADOS NO ESCOPO.
                    for (int i = 0; i < objetoCaller.propriedades.Count; i++)
                    {
                        Objeto obCallerCopia = objetoCaller;
                        int index = escopoSeSSAODoMetodo.tabela.objetos.FindIndex(k => k.nome == obCallerCopia.propriedades[i].nome);
                        if (index >= 0)
                        {
                            objetoCaller.propriedades[i] = escopoSeSSAODoMetodo.tabela.objetos[index];
                        }
                    }
                }

                // RETIRADA dos objetos do corpo do metodo.        
                if ((escopoCORPO_METODO!=null) && (escopoCORPO_METODO.tabela.objetos != null))
                {
                    
                    List<Objeto> objetosCorpoDoMetodo = escopoCORPO_METODO.tabela.GetObjetos();
                    List<Objeto> objetosSESSAO_doMetodo = escopoSeSSAODoMetodo.tabela.GetObjetos();
                    if ((escopoSeSSAODoMetodo.tabela.objetos != null) && (escopoSeSSAODoMetodo.tabela.objetos.Count > 0))
                    {

                        for (int i = 0; i < objetosCorpoDoMetodo.Count; i++)
                        {
                            int index = objetosSESSAO_doMetodo.FindIndex(k => k.nome == objetosCorpoDoMetodo[i].nome);
                            if (index != -1)
                            {
                                escopoSeSSAODoMetodo.tabela.objetos.RemoveAt(index);

                            }
                        }


                    }
                }
                
                // ATUALIZA OS OBJETOS DO ESCOPO EXTERNO, a partir do escopo sessao do metodo
                if ((escopoSeSSAODoMetodo.tabela.objetos != null) && (escopoSeSSAODoMetodo.tabela.objetos.Count > 0))
                {
                    List<Objeto> objetosEscopo = escopoSeSSAODoMetodo.tabela.objetos;
                    for (int i = 0; i < objetosEscopo.Count; i++)
                    {

                        if (escopoExterno.tabela.GetObjeto(objetosEscopo[i].nome, escopoExterno) != null)
                        {
                            escopoExterno.tabela.UpdateObjeto(objetosEscopo[i]);
                        }

                    }
                }


                
                        
                

               
                // atualiza a situação de propriedades estaticas, que podem ter sidos modificados com o codigo do metodo.
                UpdatePropriedadesEstaticas(escopoSeSSAODoMetodo);



              
                
                Escopo.escopoCURRENT = escopoExterno;


                // se for um construtor, retorna o objeto caller que é o objeto construido na funcao orquidea com instrucoes em orquidea.               
                if (flagBuild == IS_CONSTRUCTOR)
                {
                    return objetoCaller;
                }
                else
                {
                    return objetoValor;
                }

                
            }
            else
            {

                // A FUNCAO É IMPORTADA.
                // atualiza o objeto caller, com dados do escopo.
                if (escopoExterno.tabela.GetObjeto(objetoCaller.nome, escopoExterno) != null)  
                {
                    objetoCaller = escopoExterno.tabela.GetObjeto(objetoCaller.nome, escopoExterno);
                    
                }



                // otimizacao para vector, matriz, jagged array, dictionary text.
                if ((nome == "GetElement") && (parametrosDaFuncao != null) && (parametrosDaFuncao.Length == 1))
                {
                    if ((paramsChamadaDeMetodo[0] != null) && (paramsChamadaDeMetodo[0].Elementos[0].typeExprss == Expressao.typeOBJETO)) 
                    {
                        ExpressaoObjeto exprss = (ExpressaoObjeto)paramsChamadaDeMetodo[0].Elementos[0];
                        Objeto paramsIndex = escopoExterno.tabela.GetObjeto(exprss.objectCaller.nome, escopoExterno);

                        if (paramsIndex != null)
                        {

                            Vector vt = (Vector)escopoExterno.tabela.GetObjeto(objetoCaller.nome, escopoExterno).valor;
                            if (((int)paramsIndex.valor < vt.size()) && ((int)paramsIndex.valor >= 0)) 
                            {
                                return vt.VectorObjects[(int)paramsIndex.valor];
                            }
                            


                        }

                    }
                }
                else
                if ((nome == "SetElement") && (parametrosDaFuncao != null) && (parametrosDaFuncao.Length == 2))
                {
                    if ((paramsChamadaDeMetodo[0] != null) && (paramsChamadaDeMetodo[0].Elementos[0].typeExprss == Expressao.typeOBJETO)) 
                    {
                        ExpressaoObjeto exprss = (ExpressaoObjeto)paramsChamadaDeMetodo[0].Elementos[0];

                        // indice de array.
                        Objeto paramsIndex = escopoExterno.tabela.GetObjeto(exprss.objectCaller.nome, escopoExterno);
                        // valor a guardar no vector[index].
                        object valor = EvalExpression.UM_EVAL.EvalPosOrdem(paramsChamadaDeMetodo[1], escopoExterno);

                        // obtem ovetor, afim de acesso direto ao array do vector.
                        Vector vt = (Vector)escopoExterno.tabela.GetObjeto(objetoCaller.nome, escopoExterno).valor;

                        if (((int)paramsIndex.valor < vt.size()) && ((int)paramsIndex.valor >= 0)) 
                        {
                            vt.VectorObjects[(int)paramsIndex.valor] = valor;
                        }
                        


                        // atualiza o objeto dentro do escopo.
                        escopoExterno.tabela.UpdateObjeto(objetoCaller);

                        return null;

                    }
                }
                    
                // escopo para processamento de expressoes sem escopo como parametro. como [ExpressionBase].
                Escopo.escopoCURRENT = escopoExterno;

                


                // CONSTROI os parametros com valores atualizados.
                this.BuildParametros(objetoCaller,
                            paramsChamadaDeMetodo,
                            this.parametrosDaFuncao,
                            ref valoresDosParametros,
                            ref PARAMETROS,OBJETO_PARAMETER_INCLUSO, escopoExterno);




                //************************************************************************************************
                // faz a chamada do metodo, com parametros, escopo, retornando um object.
                object objetoValor = ExecuteAFunctionImportada(ref objetoCaller, valoresDosParametros, escopoExterno);
                //***********************************************************************************************

                // atualiza o objeto caller.
                escopoExterno.tabela.UpdateObjeto(objetoCaller);
                
               
               
               


                return objetoValor;
            }



            
        }

        /// <summary>
        /// constroi listas de parametros atualizados, para uma funcao.
        /// </summary>
        /// <param name="objetoCaller">objeto que invoca a funcao/metodo</param>
        /// <param name="paramsChamadaDeMetodo">lista de expressoes parametros da chamada de metodo.</param>
        /// <param name="parametrosDaFuncao">lista de nomes e tipos,  de parametros da função.(tipos compativeis verificados em compilação) </param>
        /// <param name="valoresDosParametros">lista de object contendo os valores de parametros da funcao.</param>
        /// <param name="PARAMETROS"></param>
        /// <param name="escopoDeParametros"></param>
        private void BuildParametros(Objeto objetoCaller,
            List<Expressao> paramsChamadaDeMetodo, Objeto[] parametrosDaFuncao,
            ref List<object> valoresDosParametros, ref List<Objeto> PARAMETROS, int INDEX_TYPE, Escopo escopoTOTAL)
        {
            


            if ((paramsChamadaDeMetodo == null) || (paramsChamadaDeMetodo.Count == 0))
            {
                valoresDosParametros = new List<object>();
                PARAMETROS = new List<Objeto>();
                return;
            }

            object valorParametroChamada = null;

            if ((parametrosDaFuncao != null) && (parametrosDaFuncao.Length > 0))
            {
                EvalExpression evalP = new EvalExpression();
                int indexParamsCHAMADA_Metodo = 0;
                for (int i = 0; i < parametrosDaFuncao.Length; i++)
                {

                    // PROCESSAMENTO DE OBJETO CALLER INCLUIDO NA LISTA DE PARAMETOS.                   
                    if ((i == 0) && (isToIncludeCallerIntoParameters))
                    {
                        // passagem por valor, para evitar alterações no objeto caller.
                        if (objetoCaller.isReadOnly)
                        {
                            parametrosDaFuncao[i] = objetoCaller.Clone();
                            parametrosDaFuncao[i].valor = objetoCaller.Clone();
                        }
                        // passagem por referência, para passar modificações no parametro quando a função for executada.
                        else
                        {
                            parametrosDaFuncao[i] = objetoCaller;
                            parametrosDaFuncao[i].valor = objetoCaller;
                        }
                        valorParametroChamada = objetoCaller;
                        paramsChamadaDeMetodo.Insert(0, new ExpressaoObjeto(objetoCaller));

                    }
                    else
                    // PROCESSAMENTO DE PROPRIEDADES ANINHADAS, COM OBJETO CALLER PRESENTE NO ESCOPO, como p.ex.: actual.xximage;
                    if ((paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo] != null) &&
                        (paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos != null) &&
                        (paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos.Count > 0) &&
                        (paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos[0].typeExprss == Expressao.typePROPRIEDADES_ANINHADADAS))
                    {
                        ExpressaoPropriedadesAninhadas exprssPropAninhados = (ExpressaoPropriedadesAninhadas)paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos[0];
                        if ((exprssPropAninhados.aninhamento == null) || (exprssPropAninhados.aninhamento.Count == 0))
                        {
                            // caso de lançamento de exceção NullException.
                        }
                        string nomeProp = exprssPropAninhados.aninhamento[exprssPropAninhados.aninhamento.Count - 1].nome;
                        string tipoProp = exprssPropAninhados.aninhamento[exprssPropAninhados.aninhamento.Count - 1].tipo;

                        Objeto prop = escopoTOTAL.tabela.GetObjeto(exprssPropAninhados.objectCaller.nome, escopoTOTAL);
                        int indexProp = -1;
                        if (prop != null)
                        {

                            int sizeProps = exprssPropAninhados.aninhamento.Count;
                            int b = 0;
                            while (b < sizeProps)
                            {
                                // verifica se a propriedade currente está no objeto de propriedade caller.
                                indexProp = prop.propriedades.FindIndex(k => k.nome == nomeProp && k.tipo == tipoProp);
                                if (indexProp != -1)
                                {
                                    break;
                                }
                                // passa para a proxima propriedade aninhada.
                                prop = exprssPropAninhados.aninhamento[b];

                                b++;
                            }
                            if (indexProp != -1)
                            {
                                valorParametroChamada= escopoTOTAL.tabela.GetObjeto(prop.propriedades[indexProp].nome, escopoTOTAL).valor;    
                            }
                            else
                            {   // caso de lançar uma exceção NullExecption;
                                valorParametroChamada = null;
                            }

                        }
                    }
                    else
                    // PROCESSAMENTO DE PARAMETROS MULTI-ARGUMENTOS.
                    if (parametrosDaFuncao[indexParamsCHAMADA_Metodo].isMultArgument)
                    {
                        string TIPOParamsMultiArg = parametrosDaFuncao[i].tipoElemento;
                        List<Expressao> paramsMultiArgumentos = new List<Expressao>();
                        int indexParamsBegin = i;
                        if ((parametrosDaFuncao[i] != null) && (parametrosDaFuncao[indexParamsCHAMADA_Metodo].isMultArgument))
                        {
                            int indexParams = i;
                            while ((indexParams < paramsChamadaDeMetodo.Count) &&
                                ((paramsChamadaDeMetodo[indexParams].tipoDaExpressao == TIPOParamsMultiArg))) 
                            {
                                paramsMultiArgumentos.Add(paramsChamadaDeMetodo[indexParams]);
                                indexParams++;
                                indexParamsCHAMADA_Metodo++;
                            }

                            // INSTANCIA O VETOR MULTI-ARGUMENTO, contendo os parametros multi-argumento.
                            Vector vetorMultiArgument = new Vector(TIPOParamsMultiArg);
                            vetorMultiArgument.nome = "vectorMultiArgument_" + (ExpressaoPorClassificacao.countObjetosElementosDeWrapperObject++).ToString();
                            vetorMultiArgument.isMultArgument = true;
                            vetorMultiArgument.Clear();
                            // obtem os valores de cada elemento do vetor multi-argumento.
                            for (int p = 0; p < paramsMultiArgumentos.Count; p++)
                            {
                                object objElemento = evalP.EvalPosOrdem(paramsMultiArgumentos[p], escopoTOTAL);
                                vetorMultiArgument.Append(objElemento);
                            }
                            //atualiza a lista de expressoes parametros.
                            paramsChamadaDeMetodo.RemoveRange(indexParamsBegin, paramsMultiArgumentos.Count);
                            // atualiza a malha de expressoes parametros.
                            indexParamsCHAMADA_Metodo -= paramsMultiArgumentos.Count;
                            paramsChamadaDeMetodo.Insert(indexParamsBegin, new ExpressaoObjeto(vetorMultiArgument));
                            if (vetorMultiArgument.isReadOnly)
                            {
                                parametrosDaFuncao[i] = vetorMultiArgument.Clone();
                                parametrosDaFuncao[i].valor = vetorMultiArgument.Clone();
                            }
                            else
                            {
                                // seta o i-esimo parametro do método, como um vector, que contem os elementos multi-argumentos.
                                parametrosDaFuncao[i] = vetorMultiArgument;
                                parametrosDaFuncao[i].valor = vetorMultiArgument;
                            }

                            valorParametroChamada = parametrosDaFuncao[i];

                            if (indexParamsCHAMADA_Metodo <= 0)
                            {
                                break;
                            }
                        }

                    }

                    else
                    // PROCESSAMENTO DE PARAMETROS-METODO.
                    if (paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].typeExprss == Expressao.typeCHAMADA_METODO)
                    {
                        ExpressaoChamadaDeMetodo exprssChamada = (ExpressaoChamadaDeMetodo)paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo];
                        if (exprssChamada.isMethodParameter)
                        {
                            Metodo metodoParametro = exprssChamada.funcao;
                            escopoTOTAL.tabela.RegistraFuncao(metodoParametro);
                           indexParamsCHAMADA_Metodo++;
                        }

                    }
                    else
                    // PROCESSAMENTO de EXPRESSAO NUMERO como parametro, p.ex., 1, 5.0, 6.7...
                    if (((paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos.Count > 0)) &&
                            (paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos[0].typeExprss == Expressao.typeNUMERO))
                    {
                        valorParametroChamada = ((ExpressaoNumero)paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos[0]).numero;
                        if (parametrosDaFuncao[i].tipo == "Objeto")
                        {
                            string nomeParametroNumero = parametrosDaFuncao[i].nome;
                            ExpressaoNumero exprssNumber = ((ExpressaoNumero)paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos[0]);
                            Objeto objNumero = new Objeto("private", exprssNumber.tipoDaExpressao, nomeParametroNumero, valorParametroChamada);

                            valorParametroChamada = objNumero;
                        }
                    }
                    else
                        // processamento de EXPRESSAO LITERAL como parametro, p.ex., "isto eh um texto";
                        if ((paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos.Count > 0) &&
                           (paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos[0].typeExprss == Expressao.typeLITERAL))
                    {
                        valorParametroChamada = ((ExpressaoLiteralText)paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos[0]).literalText;
                        if (parametrosDaFuncao[i].tipo == "Objeto")
                        {
                            string nomeParametroTexto = parametrosDaFuncao[i].nome;
                            ExpressaoLiteralText exprssTexto = ((ExpressaoLiteralText)paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos[0]);
                            Objeto objTexto = new Objeto("private", "string", nomeParametroTexto, valorParametroChamada);
                        }
                    }
                    else
                    // PROCESSAMENTO DE EXPRESSAO OBJETO, com atualizacao do valor presente no escopo.
                     if ((paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos.Count== 1) &&
                          (paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos[0].typeExprss ==Expressao.typeOBJETO))
                    {
                      
                        ExpressaoObjeto exprssObjeto = (ExpressaoObjeto)paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos[0];
                        if ((exprssObjeto != null) && (escopoTOTAL.tabela.GetObjeto(exprssObjeto.objectCaller.nome, escopoTOTAL) != null))
                        {
                            valorParametroChamada = escopoTOTAL.tabela.GetObjeto(exprssObjeto.objectCaller.nome, escopoTOTAL).valor;
                        }
                        else
                        if (exprssObjeto != null)
                        {
                            valorParametroChamada = exprssObjeto.objectCaller;
                        }

                    }
                    else
                    // PROCESSAMENTO DE EXPRESSAO CHAMADA DE METODO.
                    if ((paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos.Count==1) &&
                        (paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].Elementos[0].typeExprss==Expressao.typeCHAMADA_METODO))
                    {
                        valorParametroChamada = evalP.EvalPosOrdem(paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo], escopoTOTAL);
                    }
                    else
                    {
                        // PROCESSAMENTO de EXPRESSAO COMPLEXA, comoo x+1, x+y+5, etc...
                        valorParametroChamada = evalP.EvalPosOrdem(paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo], escopoTOTAL);


                    }
                    /// FINAL DE CASOS DE PARAMETROS.
                    //// passagens por valor.
                    if ((parametrosDaFuncao[i].tipo == "int") ||
                        (parametrosDaFuncao[i].tipo == "double") ||
                        (parametrosDaFuncao[i].tipo == "string"))
                    {
                        parametrosDaFuncao[i].isReadOnly = true;
                    }

                    indexParamsCHAMADA_Metodo++;

                    // ADICIONA o valor object do parametro, para a lista de objects.
                    valoresDosParametros.Add(valorParametroChamada);

                    // seta o valor do parametro, valor atualizado.
                    parametrosDaFuncao[i].valor = valorParametroChamada;

                    
                    // ADICIONA o parametro para a lista de parametros de tipo Objeto.
                    if (parametrosDaFuncao[i].isReadOnly)
                    {

                        // passagem por valor.
                        PARAMETROS.Add(parametrosDaFuncao[i].Clone());


                    }
                    else
                    {
                        // passagem por referencia.
                        PARAMETROS.Add(parametrosDaFuncao[i]);

                    }




                }

            }


        }



        /// <summary>
        /// atualiza as propriedades estáticas com o valor de escopo sessao de metodo.
        /// </summary>
        /// <param name="EscopoSessaoDoMetodo">escopo contendo todos objetos do processamento de ExecuteAMethod função.</param>
        private void UpdatePropriedadesEstaticas(Escopo EscopoSessaoDoMetodo)
        {
            if ((Escopo.escopoROOT.tabela.objetos.Count > 0) && (EscopoSessaoDoMetodo.tabela.objetos.Count>0))
            {
                List<Objeto> objetosEstaticos = Escopo.escopoROOT.tabela.objetos;
                for (int i = 0; i < objetosEstaticos.Count; i++)
                {
                    int index = EscopoSessaoDoMetodo.tabela.objetos.FindIndex(
                        k => k.nome == objetosEstaticos[i].nome &&
                        k.tipo == objetosEstaticos[i].tipo);
                    
                    if (index != -1)
                    {
                        Escopo.escopoROOT.tabela.objetos[i].valor = EscopoSessaoDoMetodo.tabela.objetos[index].valor;
                    }
                }
            }

        }
        /// <summary>
        /// junta os objetos do escopo de propriedades estaticas para o [escopoTO] parametro.
        /// </summary>
        /// <param name="escopoTO">escopo final.</param>
        private void JoinEscopoEstatico(Escopo escopoTO)
        {
            if (escopoTO == null)
            {
                return;
            }

            // ADICIONA OS OBJETOS DO ESCOPO FROM PARA O ESCOPO TO.
            if ((Escopo.escopoROOT.tabela.objetos != null) && (Escopo.escopoROOT.tabela.objetos.Count > 0))
            {
                for (int i = 0; i < Escopo.escopoROOT.tabela.objetos.Count; i++)
                {
                    Objeto umOBJ = Escopo.escopoROOT.tabela.objetos[i];
                    if ((umOBJ!=null) && (umOBJ.isStatic))
                    {
                        escopoTO.tabela.RegistraObjeto(umOBJ);
                    }

                }
            }

        }



        /// <summary>
        /// junta os objetos de dois escopos.
        /// </summary>
        /// <param name="escopoTO">escopo final.</param>
        /// <param name="escopoFrom">escopo a adicionar.</param>
        private void JoinEscopos(Escopo escopoTO, Escopo escopoFrom)
        {
            if ((escopoFrom == null) || (escopoTO == null))
            {
                return;
            }
            // ADICIONA OS OBJETOS DO ESCOPO FROM PARA O ESCOPO TO.
            if ((escopoFrom.tabela.objetos != null) && (escopoFrom.tabela.objetos.Count > 0))
            {
                for (int i = 0; i < escopoFrom.tabela.objetos.Count; i++)
                {
                    string nomeUmObj = escopoFrom.tabela.objetos[i].GetNome();
                    if (escopoTO.tabela.objetos.FindLastIndex(k => k.GetNome() == nomeUmObj) == -1)
                    {
                        escopoTO.tabela.objetos.Add(escopoFrom.tabela.objetos[i]);
                    }
                }
            }

        }



        /// <summary>
        /// executa um construtor de uma classe orquidea.
        /// </summary>
        /// <param name="objCaller">objeto que chamou o construtor. é o objeto construido.</param>
        /// <param name="parametros">lista de parametros.</param>
        /// <param name="nomeClasse">nome da classe do construtor.</param>
        /// <param name="escopoFuncao">escopo externo ao escopo da funcao construtor.</param>
        /// <param name="indexConstrutor">indice do metodo construtor, dentre a lista de construtores.</param>
        /// <returns></returns>
        public object ExecuteAConstructor(ref Objeto objCaller, List<Expressao> parametros, string nomeClasse, Escopo escopoFuncao, int indexConstrutor)
        {

            
            // executa a funcao do construtor, com lista de parametros, e escopo.
            object result = RepositorioDeClassesOO.Instance().GetClasse(nomeClasse).construtores[indexConstrutor].ExecuteAMethod(parametros, escopoFuncao, ref objCaller, Metodo.IS_CONSTRUCTOR);

            if (escopoFuncao.tabela.GetObjeto(objCaller.nome, escopoFuncao) != null)
            {
                objCaller = escopoFuncao.tabela.GetObjeto(objCaller.nome, escopoFuncao).Clone();
            }
          

            return result;

        }


   
        /// <summary>
        /// seta propriedades particulares de metodos de certo tipo de classes.
        /// </summary>
        /// <param name="isExpressionEstatic">a expressao que contem o metodo é uma chamada estatica.</param>
        public void SetAtributesMethod(bool isExpressionEstatic)
        {
            if ((this.nomeClasse == "double") || (this.nomeClasse == "string"))
            {
                this.isStatic = isExpressionEstatic;
                this.isToIncludeCallerIntoParameters = true;
            }
            else
            {
                this.isStatic = false;
                this.isToIncludeCallerIntoParameters = false;
            }
            
            
        }

        public new Metodo Clone()
        {
            Metodo metodoCopy = new Metodo();
            metodoCopy.acessor = acessor;
            metodoCopy.nome = nome;
            metodoCopy.nomeClasse = nomeClasse;
            metodoCopy.isCompiled = isCompiled;
            metodoCopy.prioridade = prioridade;
            metodoCopy.tipo = tipo;
            metodoCopy.idEscopo = idEscopo;
            metodoCopy.parametrosDaFuncao = new Objeto[] { };
            if ((parametrosDaFuncao != null) && (parametrosDaFuncao.Length > 0))
            {
                metodoCopy.parametrosDaFuncao = new Objeto[parametrosDaFuncao.Length];
                for (int i = 0; i < parametrosDaFuncao.Length; i++)
                {
                    metodoCopy.parametrosDaFuncao[i] = parametrosDaFuncao[i].Clone();
                }
            }
            metodoCopy.tipoReturn = tipoReturn;
            metodoCopy.isToIncludeCallerIntoParameters = isToIncludeCallerIntoParameters;
            metodoCopy.tokens = tokens;
            metodoCopy.InfoMethod = InfoMethod;
            metodoCopy.InfoConstructor = InfoConstructor;
            metodoCopy.indexInClass = indexInClass;
            if ((this.instrucoesFuncao != null) && (this.instrucoesFuncao.Count > 0))
            {
                metodoCopy.instrucoesFuncao = this.instrucoesFuncao;

            }
            else
            {
                this.instrucoesFuncao = new List<Instrucao>();
            }
            metodoCopy.caller = caller;
            if (escopoCORPO_METODO == null)
            {
                metodoCopy.escopoCORPO_METODO = new Escopo("");
            }
            else
            {
                metodoCopy.escopoCORPO_METODO = escopoCORPO_METODO.Clone();

            }
            metodoCopy.stackActual = stackActual;

            return metodoCopy;
        }

  

        public Metodo()
        {
            this.escopoCORPO_METODO = null;
            this.acessor = "protected"; // valor default para o acessor da função.
            this.nome = "";
            this.tipoReturn = null;

            this.prioridade = 300;  // seta a prioridade da função em avaliação de expressões. A regra de negócio é que a função sempre tem prioridade sobre os operadores.

            this.instrucoesFuncao = new List<Instrucao>();
        } //Funcao()

        public Metodo(string acessor, string nome, FuncaoGeral fncImplementa, string tipoRetorno, params Objeto[] parametrosMetodo)
        {
            this.escopoCORPO_METODO = null;
            this.InfoMethod = null;
            this.InfoConstructor = null;
            this.acessor = acessor;
            if (acessor == null)
                this.acessor = "protected";
            this.nome = nome;
            this.tipoReturn = tipoRetorno;
            if (parametrosMetodo != null)
                this.parametrosDaFuncao = parametrosMetodo.ToArray<Objeto>();
            this.instrucoesFuncao = new List<Instrucao>();
        }


        public Metodo(string classe, string acessor, string nome, Objeto[] parametrosMetodo, string tipoRetorno, List<Instrucao> instrucoesCorpo, Escopo escopoDaFuncao)
        {
            
            this.InfoMethod = null;
            this.InfoConstructor = null;
            if (acessor == null)
                acessor = "protected"; // se nao tiver acessor, é uma função estruturada, seta o acessor para protected.
            else
                this.acessor = acessor; // acessor da função.
            this.nome = nome; // nome da função.
            this.parametrosDaFuncao = new Objeto[parametrosMetodo.Length]; // inicializa a lista de parâmetros da função.

            if ((parametrosMetodo != null) && (parametrosMetodo.Length > 0)) // obtém uma lista dos parâmetros da função. 
                this.parametrosDaFuncao = parametrosMetodo.ToArray<Objeto>();


            this.instrucoesFuncao = new List<Instrucao>(); // sem instruções (sem corpo de função).
            this.tipoReturn = tipoRetorno; // tipo do retorno da função.


            this.escopoCORPO_METODO = escopoDaFuncao.Clone();
    
            for (int x = 0; x < this.parametrosDaFuncao.Length; x++)
                escopoCORPO_METODO.tabela.GetObjetos().Add(new Objeto("private", parametrosDaFuncao[x].GetTipo(), parametrosDaFuncao[x].GetNome(), null, escopoCORPO_METODO, false));


            if (instrucoesCorpo != null)
                this.instrucoesFuncao = instrucoesCorpo.ToList<Instrucao>();

            this.nomeClasse = classe;
        } // Funcao()


        ///  construtor com método importado via API Reflexao.
        public Metodo(string nomeClasse, string acessor, string nome, MethodInfo metodoImportado, string tipoRetorno, params Objeto[] parametrosMetodo)
        {
            this.escopoCORPO_METODO = null;
            this.acessor = acessor;
            this.nome = nome;
            this.tipoReturn = tipoRetorno;
            this.parametrosDaFuncao = parametrosMetodo.ToArray<Objeto>();

            this.InfoMethod = metodoImportado;
            this.InfoConstructor = null;
            this.instrucoesFuncao = new List<Instrucao>();
            this.nomeClasse = nomeClasse;
     
            List<Type> tiposDosParametros = new List<Type>();
            List<object> nomesDosParametros = new List<object>();

        }

      
        public Metodo(string nomeClasse, string acessor, string nome, ConstructorInfo construtorImportado, string tipoRetorno, Escopo escopoDaFuncao, params Objeto[] parametrosMetodo)
        {
            this.acessor = acessor;
            this.nome = nome;
            this.InfoMethod = null;
            this.InfoConstructor = construtorImportado;
            this.tipoReturn = tipoRetorno;
            this.nomeClasse = nomeClasse;
            this.instrucoesFuncao = new List<Instrucao>();
            this.parametrosDaFuncao = parametrosMetodo.ToArray<Objeto>();
            if (escopoDaFuncao != null)
                this.escopoCORPO_METODO = escopoDaFuncao.Clone();
            
        }


        public static bool IguaisFuncoes(Metodo fncA, Metodo fncB)
        {
            if (fncA.nome != fncB.nome)
                return false;

            if ((fncA.parametrosDaFuncao == null) && (fncB.parametrosDaFuncao == null))
                return true;

            if ((fncA.parametrosDaFuncao == null) && (fncB.parametrosDaFuncao != null))
                return false;

            if ((fncA.parametrosDaFuncao != null) && (fncB.parametrosDaFuncao == null))
                return false;

            if (fncA.parametrosDaFuncao.Length != fncB.parametrosDaFuncao.Length)
                return false;

          
            for (int x = 0; x < fncA.parametrosDaFuncao.Length; x++)
                if (fncA.parametrosDaFuncao[x].GetTipo() != fncB.parametrosDaFuncao[x].GetTipo())
                    return false;

            if (fncA.tipoReturn != fncB.tipoReturn)
                return false;

            return true;
        }


        public override string ToString()
        {
            string str = "";
            if ((this.tipoReturn != null) && (this.tipoReturn != ""))
                str += this.tipoReturn.ToString() + "  ";

            if ((this.nome != null) && (this.nome != ""))
                str += this.nome + "( ";
            if ((this.parametrosDaFuncao != null) && (this.parametrosDaFuncao.Length > 0))
            {
                for (int x = 0; x < this.parametrosDaFuncao.Length; x++)
                {
                    str += this.parametrosDaFuncao[x] + " ";
                    if (x < (parametrosDaFuncao.Length - 1))
                        str += ",";
                } // for x
            } // if
            str += ")";
            return str;
        } // ToString()



        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes de chamada de metodos, e funcionalidades classe funcao.")
            {
            }


            public void TestePropriedadesEstaticas(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string code_0_0_class = "public class classeB{public static int propriedade1; public classeB(){int x=1;};};";
                string code_0_0_create = "classeB.propriedade1=1;";

                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_class);
                compilador.Compilar();

                Expressao exprss_0_0 = new Expressao(code_0_0_create, compilador.escopo);

                ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                programa.Run(compilador.escopo);

                List<Objeto> objetosEstaticos = Escopo.escopoROOT.tabela.objetos;
            }
            // teste de estudo de variaveis em diferentes escopos, se não estão interferindo 1 nas outras,
            // este teste comprova algo meio grave: bagunça quando se tem duas ou + objetos com mesmo nome.
            // do jeito que está, não há separação em combos de escopos!
            public void TesteVariaveisMesmoNomeEscoposDiferentes(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
          
                string code_class = "public class entity{ public int x; public entity(){ x=3;}; public int metodoB(){return x;};};";
                string code_create = "int x=1; entity e= create(); e.metodoB();";

                ProcessadorDeID compilador = new ProcessadorDeID(code_class + code_create);
                compilador.Compilar();


                ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                program.Run(compilador.escopo);
            }   

            // teste para correcao de nomes de parametros.
            public void TestePassagemDeParametros(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_class = "public class entity{" +
                        " pubilc entity()" +
                                "{int x=1;};" +
                        " public void funcaoB(double xPlayer, double yPlayer)" +
                                "{xPlayer=2.0; yPlayer=5.0;};};";

                    string code_create = "double xPlayer=1.0; double yPlayer= 3.0; entity e= create(); e.funcaoB(xPlayer,yPlayer);";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_class + code_create);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);

                    assercao.IsTrue(
                        compilador.escopo.tabela.GetObjeto("xPlayer", compilador.escopo).valor.ToString() == "1" &&
                        compilador.escopo.tabela.GetObjeto("yPlayer", compilador.escopo).valor.ToString() == "3", code_class + code_create);
                }
                catch(Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }


            


    
    
            public void TesteExecucaoMetodoOrquidea(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                
                string codigoClasse = "public class classe1 {  public classe1(){int a=1; }; public int fatorial(:r int x){if (x<=0){ return 1;};int y=1;for (int n=1;n<=x;n++){y=y*n;};return y;};};";
                string codigoCreateObjects = "classe1 obj1= create();int b = obj1.fatorial(5);";

                try
                {
                    ProcessadorDeID compilador = new ProcessadorDeID(codigoClasse + codigoCreateObjects);
                    compilador.Compilar();
                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);
                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasse("classe1").GetMetodos()[1].parametrosDaFuncao[0].isReadOnly == true, codigoCreateObjects);
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("b", compilador.escopo).valor.ToString() == "120");

                }
                catch (Exception ex)
                {
                    string msgError = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU:  " + msgError);
                }


            }

            public void TesteObterParametros(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoCompilar = "double a=9;";
                string codigoExprss_0_0 = "double.root2(9)";
                string codigoExprss_0_1 = "double.root2(a)";

                EvalExpression eval = new EvalExpression();

                try
                {
                    ProcessadorDeID compilador = new ProcessadorDeID(codigoCompilar);
                    compilador.Compilar();

                    Expressao exprss_0_0 = new Expressao(codigoExprss_0_0, compilador.escopo);
                    object result_0_0 = eval.EvalPosOrdem(exprss_0_0, compilador.escopo);

                    Expressao exprss_0_1= new Expressao(codigoExprss_0_1,compilador.escopo);
                    object result_0_1 = eval.EvalPosOrdem(exprss_0_1, compilador.escopo);
                        

                    assercao.IsTrue(result_0_0.ToString() == "3");
                    assercao.IsTrue(result_0_1.ToString() == "3");
                }
                catch (Exception ex)
                {
                    string msgErro = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }


            }

      
        

            
            

        }
    } // class Funcao

    public class Operador : Metodo
    {
        public new int prioridade { get;  set; } // prioridade do operador nas expressões.
        public string tipoRetorno { get; set; } // tipo de retorno da função

        internal int indexPosOrdem = 0; // utilizada para processamento de PosOrdem().


      
    

        // função com instrucoes orquidea.
        public Metodo funcaoImplementadoraDoOperador { get; set; }
    
        public new string GetNome()
        {
            return this.nome;
        }
        public int GetPrioridade()
        {
            return prioridade;
        }
    

        public string GetTipoFuncao()
        {
            return tipo;
        }

        public void SetTipo(string tipoNovo)
        {
            this.tipo = tipoNovo;
        }



        private Random aleatorizador = new Random(1000);

      
        public Operador(string nomeClasse, string nomeOperador, int prioridade, Objeto[] parametros, string tipoOperador, MethodInfo metodoImpl, Escopo escopoDoOperador):base()
        {
            this.nome = nomeOperador;
            this.nomeClasse = nomeClasse;
            if (parametros != null)
                this.parametrosDaFuncao = parametros.ToArray<Objeto>(); // faz uma copia em profundidade nos parametros.
            else
                this.parametrosDaFuncao = new Objeto[0];
            
            this.tipo = tipoOperador;
            this.tipoReturn = UtilTokens.Casting(metodoImpl.ReflectedType.Name.ToLower());
            
            this.InfoMethod = metodoImpl;
            this.caller = new object();



            this.instrucoesFuncao = null;
            this.prioridade = prioridade;
            LinguagemOrquidea.RegistraOperador(this); // adiciona o operador criado, para a lista de operadores da linguagem, atualizando a lista de operadores para processamento.

        }

        public Operador(string nomeClasse, string nomeOperador, int prioridade, string[] tiposParametros, string tipoOperador, Metodo funcaoDeImplementacaoDoOperador, Escopo escopoDoOperador) : base()
        {
            this.nome = nomeOperador;
            this.nomeClasse = nomeClasse;
            this.prioridade = prioridade;


            Objeto[] operandos = new Objeto[2];
            if (tiposParametros[0] != null)
            {
                if (tiposParametros.Length > 0)
                    operandos[0] = new Objeto("A", tiposParametros[0], null, false);

                if (tiposParametros.Length > 1)
                    operandos[1] = new Objeto("B", tiposParametros[1], null, false);

            }

            this.parametrosDaFuncao = operandos;

            this.tipo = tipoOperador;
            if (funcaoDeImplementacaoDoOperador != null)
            {
                this.funcaoImplementadoraDoOperador = funcaoDeImplementacaoDoOperador;
                this.instrucoesFuncao = funcaoDeImplementacaoDoOperador.instrucoesFuncao;

                // adiciona o operador criado, para a lista de operadores da linguagem, atualizando a lista de operadores para processamento.
                LinguagemOrquidea.RegistraOperador(this); 
            }

          
            
            

        } // Operador()

        public Operador(string nomeClase, string nomeOperador, int prioridade, string tipoRetorno, List<Instrucao> instrucoesCorpo, Objeto[] parametros, Escopo escopoDoOperador):base()
        {
            this.nome = nomeOperador;
            this.tipoRetorno = tipoRetorno;
            this.prioridade = prioridade;
            this.nomeClasse = nomeClase;
            this.instrucoesFuncao = instrucoesCorpo.ToList<Instrucao>();
            this.parametrosDaFuncao = parametros;
            LinguagemOrquidea.RegistraOperador(this); // adiciona o operador criado, para a lista de operadores da linguagem, atualizando a lista de operadores para processamento.

        }

        public new Operador Clone()
        {
            Operador operador = new Operador(this.nomeClasse, this.nome, this.prioridade, this.parametrosDaFuncao, this.tipo, this.InfoMethod, this.escopoCORPO_METODO);
            operador.tipoRetorno = this.tipoRetorno;
            operador.tipoReturn = this.tipoReturn;
            return operador;
        }

        public static bool IguaisOperadores(Operador op1, Operador op2)
        {
            if ((op1.parametrosDaFuncao == null) && (op2.parametrosDaFuncao == null))
                return true;
             
            if ((op1.parametrosDaFuncao == null) && (op2.parametrosDaFuncao != null))
                return false;

            if ((op1.parametrosDaFuncao != null) && (op2.parametrosDaFuncao == null))
                return false;

            if (op1.parametrosDaFuncao.Length != op2.parametrosDaFuncao.Length)
                return false;

            for (int x = 0; x < op1.parametrosDaFuncao.Length; x++)
                if (op1.parametrosDaFuncao[x].GetTipo() != op2.parametrosDaFuncao[x].GetTipo())
                    return false;

            return true;
        }


        public static Operador GetOperador(string nomeOperador, string classeOperador, string tipo, UmaGramaticaComputacional lng)
        {
            Classe classe = RepositorioDeClassesOO.Instance().GetClasse(classeOperador);
            if (classe == null)
                return null;
            int index = classe.GetOperadores().FindIndex(k => k.nome.Equals(nomeOperador));

            if (index != -1)
            {
                Operador op = classe.GetOperadores().Find(k => k.GetTipo().Contains(tipo));
                return classe.GetOperadores()[index];
            } // if
            return null;
        }


        public object ExecuteOperador(string nomeDoOperador, Escopo escopo, params object[] valoresParametros)
        {
     
            // atualiza com dados do objeto actual.
            if (escopo.isPresentObject("actual"))
            {
                Objeto actual = escopo.tabela.GetObjeto("actual", escopo);
                if ((actual != null) && (actual.propriedades != null) && (actual.propriedades.Count > 0))
                {
                    for (int x = 0; x < valoresParametros.Length; x++)
                    {

                        if ((valoresParametros[x] != null) && (valoresParametros[x].GetType() == typeof(Objeto)))
                        {
                            Objeto parametro = (Objeto)valoresParametros[x];
                            int index = actual.propriedades.FindIndex(k => k.nome == parametro.nome && k.tipo == parametro.tipo);
                            if (index != -1)
                            {
                                valoresParametros[x] = actual.propriedades[index].valor;
                            }
                        }
                    }


                }
            }
            
            object result = null;
     
            if (this.InfoMethod != null)
            {
                for (int x = 0; x < valoresParametros.Length; x++)
                {

                    if ((valoresParametros[x] != null) && (valoresParametros[x].GetType() == typeof(Objeto)))
                    {
                        Objeto parametro = (Objeto)valoresParametros[x];
                        Objeto OBJ_parametro = escopo.tabela.GetObjeto(parametro.nome, escopo);
                        if (OBJ_parametro != null)
                        {
                            valoresParametros[x] = OBJ_parametro.valor;
                        }
                        
                        
                    }
                }

                Type classeOperador = this.InfoMethod.DeclaringType;
                // obtem um objeto da classe que construiu o operador.
                this.caller = classeOperador.GetConstructor(new Type[] { }).Invoke(null);
                
                result = InfoMethod.Invoke(this.caller, valoresParametros);
            }
            else
            if (this.instrucoesFuncao != null)
            {
                result = this.ExecuteAFunction(null, valoresParametros.ToList<object>(), escopo);
            }
            else
            {
                return Expressao.Instance.ConverteParaNumero(result.ToString(), escopo);
            }
                


            return result;
        } // ExecuteOperador()

        public override string ToString()
        {
            string str = "";
            if (this.nome != null)
                str += "Nome: " + this.nome + "  pri: " + this.prioridade.ToString();
            return str;
        }// ToString()

 
      
    } // class
   
} //namespace
