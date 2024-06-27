using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;

using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Integration;
using Microsoft.SqlServer.Server;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    /// <summary>
    /// classe com funções uteis na implementação de codigo no compilador.
    /// </summary>
    public class UtilTokens
    {
        private static LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();


        /// <summary>
        /// retorna o tipo de uma classe.
        /// </summary>
        /// <param name="typeName">nome da classe.</param>
        /// <returns></returns>
        public static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        /// <summary>
        /// registra uma mensagem de erro de compilação, no escopo parâmetro, e também numa lista de errros estatica.
        /// </summary>
        /// <param name="mensagemDeErro">mensagem a registrar.</param>
        /// <param name="tokensDOProcessamento">tokens para localizar a linha e coluna da falha.</param>
        /// <param name="escopo">contexto onde os tokens estão, e também uma das formas de armazenar a mensage, num dos seus campos.</param>
        public static void WriteAErrorMensage(string mensagemDeErro, List<string> tokensDOProcessamento, Escopo escopo)
        {

            // encontra o numero da linha de onde surgiu a falha.
            int numeroDaLinhaFalha = ProcessadorDeID.lineInCompilation - 1;



            string linhaDaFalha = ParserAFile.GetLine(numeroDaLinhaFalha);
            try
            {
                string lineInfo = ParserAFile.GetLine(numeroDaLinhaFalha);
                SystemInit.errorsInCopiling.Add(lineInfo + ":      error: " + mensagemDeErro);
                return;
            }
            catch (Exception ex)
            {
                string codeError = ex.Message;
            }


        }

      

        /// <summary>
        ///  obtem uma lista de tokens, entre operador abre e operador fecha, com criterio que a quantidade da diferenca dos operadores abre/fecha seja = 0.
        /// </summary>
        /// <param name="indiceInicio">indice inicial, cujo token deste indice é um operador abre.</param>
        /// <param name="operadorAbre">operador inicial.</param>
        /// <param name="operadorFecha">operador final.</param>
        /// <param name="tokensEntreOperadores">tokens contendo inclusive a lista de tokens resultante.</param>
        /// <returns>retorna uma lista de tokens entre os parenteses, ou null se não resultar em tokens, ou se o numero de tokens < 2</returns>
        public static List<string> GetCodigoEntreOperadores(int indiceInicio, string operadorAbre, string operadorFecha, List<string> tokensEntreOperadores)
        {
            if (indiceInicio == -1)
                return null;


            List<string> tokens = new List<string>();
            int pilhaInteiros = 0;
            
            
            int indexToken = indiceInicio;
            
            while (indexToken < tokensEntreOperadores.Count)
            {
                if (tokensEntreOperadores[indexToken] == operadorAbre)
                {
                    tokens.Add(operadorAbre);
                    pilhaInteiros++;
                }
                else
                if (tokensEntreOperadores[indexToken] == operadorFecha)
                {
                    tokens.Add(operadorFecha);
                    pilhaInteiros--;
                    if (pilhaInteiros == 0)
                        return tokens;

                } // if
                else
                    tokens.Add(tokensEntreOperadores[indexToken]);
                indexToken++;
            } // While

            return tokens;
        } // GetCodigoEntreOperadores()


        /// <summary>
        /// imprime no terminal, a lista de tokens parametro.
        /// </summary>
        /// <param name="tokens">lista de tokens a imprimir.</param>
        /// <param name="caption">texto caption-info da impressão.</param>
        public static void PrintTokens(List<string> tokens, string caption)
        {
            if ((tokens == null) || (tokens.Count == 0))
            {
                System.Console.WriteLine("without tokens to print!");
                return;
            }
            else
            {
                System.Console.Write(caption + "     ");
                for (int x = 0; x < tokens.Count; x++)
                {
                    System.Console.Write(tokens[x] + " ");
                }
                System.Console.WriteLine();
            }

        }
        /// <summary>
        /// imprime numa linha de texto somente, no terminal, a lista de tokens parametro.
        /// </summary>
        /// <param name="tokens">lista de tokens a imprimir.</param>
        /// <param name="caption">texto info-caption de impressao.</param>
        public static void PrintTokensWithoutSpaces(List<string> tokens, string caption)
        {
            if ((tokens == null) || (tokens.Count == 0))
            {
                System.Console.WriteLine("without tokens to print!");
                return;
            }
            else
            {
                System.Console.Write(caption + "     ");
                for (int x = 0; x < tokens.Count; x++)
                {
                    System.Console.Write(tokens[x]);
                }
                System.Console.WriteLine();
            }
        }


        /// <summary>
        /// extracao de tokens de expressoes para instrução case.
        /// </summary>
        /// <param name="tokens1">lista de tokens para processamento.</param>
        /// <returns></returns>
        public static List<List<List<string>>> GetCodigoEntreOperadoresCases(List<string> tokens1)
        {
            // sintaxe: "casesOfUse y:  (case < x): { y = 2; }; ";


            List<string> tokens = tokens1.ToList<string>();
            List<List<List<string>>> tokensRetorno = new List<List<List<string>>>();
            int x = tokens.IndexOf("(");

            int firstOperadorAbre = tokens.IndexOf("{");
            int lastOperadorFecha = tokens.LastIndexOf("}");

            if ((firstOperadorAbre == -1) && (lastOperadorFecha == -1))
            {
                return null;
            }
            tokens.RemoveAt(firstOperadorAbre);
            lastOperadorFecha = tokens.LastIndexOf("}");
            tokens.RemoveAt(lastOperadorFecha);


            int offsetMarcadores = 0;
            int offsetProcessamento = tokens.IndexOf("{");
            while ((x >= 0) && (x < tokens.Count))
            {
                // faz o processamento do cabecalho do case.
                int indexCabecalhoCase = tokens.IndexOf("(", offsetMarcadores);
                if (indexCabecalhoCase == -1)
                {
                    return tokensRetorno;
                }
                List<string> cabecalhoUmCase = GetCodigoEntreOperadores(indexCabecalhoCase, "(", ")", tokens);

            
                if ((cabecalhoUmCase == null) || (cabecalhoUmCase.Count== 0))
                {
                    return tokensRetorno;
                }


                // faz o processamento do corpo do case.
                // obtem o primeiro token abre, apos o token abre de delimitação dos cases.
                int indexProcessamentoCase = tokens.IndexOf("{", offsetProcessamento);
                List<string> tokensCorpoDeUnCase = UtilTokens.GetCodigoEntreOperadores(indexProcessamentoCase, "{", "}", tokens);
                if ((tokensCorpoDeUnCase!=null) && (tokensCorpoDeUnCase.Count > 0))
                {
                    // se encontrou tokens validos, adiciona o case completo para a lista de cases de retorno.    
                    List<List<string>> umCaseCompleto = new List<List<string>>() { cabecalhoUmCase, tokensCorpoDeUnCase };

                    tokensRetorno.Add(umCaseCompleto);
                        
                       
                }
                else
                {
                    return null;
                }

                // atualiza os offsets de procura de tokens.
                offsetMarcadores += cabecalhoUmCase.Count;
                offsetProcessamento += tokensCorpoDeUnCase.Count;
                x = offsetProcessamento + offsetMarcadores;
                    
            }

            return tokensRetorno;

        }  





        /// <summary>
        /// faz a conversao de tipos basicos de classes importado, para o sistema de tipos da linguagem orquidea.
        /// </summary>
        /// <param name="tipo">tipo importado.</param>
        /// <returns>retorna o tipo correpondente, ou o tipo parametro se nao houver correspondente.</returns>
        public static string Casting(string tipo)
        {
            if ((tipo.Contains("Single")) || (tipo.Contains("single")))
            {
                return "float";
            }
            else
            if ((tipo.Contains("Int32")) || (tipo.Contains("Int16")))
            {
                return "int";
            }
            else
            if ((tipo.Contains( "Float")) || (tipo.Contains("float")))
            {
                return "float";
            }
            else
            if ((tipo.Contains("Double")) || (tipo.Contains("double")))
            {
                return "double";
            }
            else
            if (tipo.Contains ("Boolean"))
            {
                return "bool";
            }
            else
            if ((tipo.Contains("string")) || (tipo.Contains("String")))
            {
                return "string";
            }
            else
            if ((tipo.Contains("Char")) || (tipo.Contains("char")))
            {
                return "char";
            }
            else
            if (tipo.Contains("Wrappers"))
            {
                tipo = tipo.Replace("Wrappers.DataStructures.", "");
            }
            return tipo;
        }

        /// <summary>
        /// encontra um operador que é binario e unario ao mesmo tempo, mas funcionando com binario.
        /// </summary>
        /// <param name="nameOperator">nome do operador</param>
        /// <param name="tipoOperando1">tipo do operando 1.</param>
        /// <param name="tipoOperando2">tipo do operando 2.</param>
        /// <returns>retorna a funcao que implementa o operador.</returns>
        public static Operador FindOperatorBinarioEUnarioMasComoBINARIO(string classOperator, string nameOperator, string tipoOperando1, string tipoOperando2)
        {
            bool isFoundOperator = false;

            Operador operadorComoBinario = null;
            List<HeaderClass> classesOperadores = Expressao.headers.cabecalhoDeClasses;
            HeaderClass headerClassOperator = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == classOperator);

            if (headerClassOperator != null)
            {
                string nomeClasse= headerClassOperator.nomeClasse;
                List<HeaderOperator> operators = headerClassOperator.operators;
                HeaderOperator headerOperador = operators.Find(k => k.name == nameOperator && k.operands.Count==2 && k.operands[0] == tipoOperando1 && k.operands[1] == tipoOperando2 && k.tipoDoOperador == HeaderOperator.typeOperator.binary);
                if (headerOperador != null)
                {
                    return RepositorioDeClassesOO.Instance().GetClasse(nomeClasse).GetOperadores().Find(k => k.nome == nameOperator && k.tipo == "BINARIO");
                }

            }
            if (classesOperadores != null)
            {
                for (int x = 0; x < classesOperadores.Count; x++)
                {
                    List<HeaderOperator> operators = classesOperadores[x].operators;
                    if ((operators != null) && (operators.Count > 0))
                    {
                        // verifica se o operador é binario, com operandos compativeis com os parametros de entrada.
                        for (int op = 0; op < operators.Count; op++)
                        {
                            if (operators[op].tipoDoOperador == HeaderOperator.typeOperator.binary)
                            {
                                if ((operators[op].name == nameOperator) && (operators[op].operands[0] == tipoOperando1) && (operators[op].operands[1] == tipoOperando2))
                                {
                                    return RepositorioDeClassesOO.Instance().GetClasse(classesOperadores[x].nomeClasse).GetOperadores().Find(k => k.nome == nameOperator && k.tipo.Contains("BINARIO"));
                                    
                                  
                                }
                            }
                        }


                    }

                }
            }
            // nao encontrou nenhum operador.
            if (!isFoundOperator)
            {
                return null;
            }
            else
            {
                return operadorComoBinario;
            }
           
        }



        /// <summary>
        /// encontra um operador unario e binario ao mesmo tempo, mas funcionando como unario.
        /// </summary>
        /// <param name="nameOperator">nome do operador.</param>
        /// <param name="tipoOperando1">tipo do operando 1.</param>
        /// <param name="tipoOperando2">tipo do operando 2,dentro da expressao (operador unario tem apenas um operando).</param>
        /// <returns>retorna a funcao que implementa o operador.</returns>
        public static Operador FindOperatorBinarioUnarioMasComoUNARIO(string nameOperator, string tipoOperando1, string tipoOperando2)
        {
            bool isFoundOperator = false;

            Operador operatorCompatible = null;

            List<HeaderClass> classesOperadores = Expressao.headers.cabecalhoDeClasses;
            int indexClass = -1;
            if (tipoOperando1 != null)
            {
                indexClass = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == tipoOperando1);
            }
            else
            if (tipoOperando2 != null)
            {
                indexClass = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == tipoOperando2);
            }

            if (indexClass == -1)
            {
                return null;
            }

            string nameClassOperator = classesOperadores[indexClass].nomeClasse;

            List<HeaderOperator> operators = classesOperadores[indexClass].operators;
            if ((operators != null) && (operators.Count > 0))
            {

                // verifica se o operador é unario.
                for (int op = 0; op < operators.Count; op++)
                {
                    if ((tipoOperando1 != null) && (operators[op].name == nameOperator) && (operators[op].tipoDoOperador == HeaderOperator.typeOperator.unary_pre) &&
                        (operators[op].operands[0] == tipoOperando1))
                    {
                        operatorCompatible = RepositorioDeClassesOO.Instance().GetClasse(nameClassOperator).GetOperadores().Find(k => k.nome == nameOperator && k.tipo.Contains("UNARIO"));
                        operatorCompatible.tipo = "UNARIO POS";

                        isFoundOperator = true;

                    }
                    else
                    if ((tipoOperando2 != null) && (operators[op].name == nameOperator) && (operators[op].tipoDoOperador == HeaderOperator.typeOperator.unary_pos) &&
                        (operators[op].operands[0] == tipoOperando2))
                    {
                        operatorCompatible = RepositorioDeClassesOO.Instance().GetClasse(nameClassOperator).GetOperadores().Find(k => k.nome == nameOperator && k.tipo.Contains("UNARIO"));
                        operatorCompatible.tipo = "UNARIO PRE";
                        isFoundOperator = true;

                    }

                }
            }
            // nao encontrou nenhum operador.
            if (!isFoundOperator)
            {
                return null;
            }
            return operatorCompatible;

        }
        /// <summary>
        /// verficia se um  nome é um token de operador unario.
        /// </summary>
        /// <param name="nameOperator">nome do tokens dito operador unario a investigar.</param>
        /// <param name="operand">expressao que contem o tipo de operando do operador unario.</param>
        /// <returns></returns>
        public static bool IsUnaryOperator(string nameOperator, Expressao operand)
        {
            string nameClassOperator = operand.tipoDaExpressao;
            List<HeaderOperator> operators = null;

            HeaderClass classHeader = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == operand.tipoDaExpressao);
            if (classHeader == null)
            {
                return false;
            }
            operators = classHeader.operators;

            if ((operators == null) || (operators.Count == 0))
            {
                return false;
            }

            int indexOP = operators.FindIndex(k => k.name == nameOperator);


            if (indexOP != -1)
            {
                if ((operators[indexOP].tipoDoOperador == HeaderOperator.typeOperator.unary_pos) &&
                    (operators[indexOP].operands.Count > 0) && (operators[indexOP].operands[0] == operand.tipoDaExpressao))
                {
                    return true;
                }

                if ((operators[indexOP].tipoDoOperador == HeaderOperator.typeOperator.unary_pos) &&
                    (operators[indexOP].operands.Count > 1) && (operators[indexOP].operands[1] == operand.tipoDaExpressao))
                {
                    return true;
                }

                if ((operators[indexOP].tipoDoOperador == HeaderOperator.typeOperator.unary_pre) &&
                    (operators[indexOP].operands.Count > 0) && (operators[indexOP].operands[0] == operand.tipoDaExpressao))
                {
                    return true;
                }

            }

            return false;
        }

        /// <summary>
        /// encontra um operador, binario, unario pos, ou unario pre, de nome de entrada, e com tipos de operandos de entrada.
        /// </summary>
        /// <param name="nameOperator">nome do operador.</param>
        /// <param name="tipoOperando1">tipo do operando 1.</param>
        /// <param name="tipoOperando2">tipo do operando 2.</param>
        /// <returns></returns>
        public static Operador FindOperatorCompatible(string nameOperator, string tipoOperando1, string tipoOperando2, ref bool isBinaryAndUnary)
        {
            
            isBinaryAndUnary = false;
            bool isBinary = false;
            bool isUnary = false;
            bool isFoundOperator = false;

            Operador operatorCompatible = null;

            List<HeaderClass> classesOperadores = Expressao.headers.cabecalhoDeClasses;
            if (classesOperadores == null)
            {
                return null;
            }
            classesOperadores = Expressao.headers.cabecalhoDeClasses;
            int indexClass = -1;
            if (tipoOperando1 != null)
            {
                indexClass = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == tipoOperando1);
            }
            else
            if (tipoOperando2 != null)
            {
                indexClass = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == tipoOperando2);
            }

            if (indexClass == -1)
            {
                return null;
            }

            string nameClassOperator = classesOperadores[indexClass].nomeClasse;
            List<HeaderOperator> operators = classesOperadores[indexClass].operators;

            // verifica se o operador é binario, com operandos compativeis com os parametros de entrada.

            // verifica se o operador é unario.
            for (int op = 0; op < operators.Count; op++)
            {
                if (operators[op].tipoDoOperador == HeaderOperator.typeOperator.binary)
                {
                    if ((operators[op].name == nameOperator) && (operators[op].operands[0] == tipoOperando1) && (operators[op].operands[1] == tipoOperando2))
                    {
                        operatorCompatible = RepositorioDeClassesOO.Instance().GetClasse(nameClassOperator).GetOperador(operators[op].name);
                        operatorCompatible.tipo = "BINARIO";
                        isBinary = true;
                        isFoundOperator = true;
                    }
                }

                if ((operators[op].name == nameOperator) && (operators[op].tipoDoOperador == HeaderOperator.typeOperator.unary_pos) &&
                    (tipoOperando2 != null) && (operators[op].operands[0] == tipoOperando2))
                {
                    operatorCompatible = RepositorioDeClassesOO.Instance().GetClasse(nameClassOperator).GetOperador(operators[op].name);
                    operatorCompatible.tipo = "UNARIO POS";

                    isUnary = true;
                    isFoundOperator = true;

                }
                else
                if ((operators[op].name == nameOperator) && (operators[op].tipoDoOperador == HeaderOperator.typeOperator.unary_pos) &&
                    (tipoOperando1 != null) && (operators[op].operands[0] == tipoOperando1))
                {
                    operatorCompatible = RepositorioDeClassesOO.Instance().GetClasse(nameClassOperator).GetOperador(operators[op].name);
                    operatorCompatible.tipo = "UNARIO PRE";
                    isUnary = true;
                    isFoundOperator = true;

                }


            }
            // nao encontrou nenhum operador.
            if (!isFoundOperator)
            {
                return null;
            }
            // encontrou operador, verifica se é binario E unario.
            isBinaryAndUnary = isBinary && isUnary;
            return operatorCompatible;

        }

        /// <summary>
        /// encontra um metodo que seja compativel com a lista de parâmetros de uma chamada de método.
        /// se for para incluir o objeto caller na lista de parâmetros da chamada, faz e valida com a lista de parametros do metodo.
        /// </summary>
        /// <param name="objCaller">objeto que chama a funcao.</param>
        /// <param name="classObjCaller">classe do objeto da chamada de metodo.</param>
        /// <param name="nameMethod">nome do metodo</param>
        /// <param name="nameClass">classe do metodo</param>
        /// <param name="parameters">lista de parâmetros da chamada de metodo, que o metodo tenha para validar.</param>
        /// <param name="escopo">contexto onde a chamada de método está.</param>
        /// <param name="isStatic">chamada estatica ou nao.</param>
        /// <param name="isToIncludeFirstParameterObjectCaller">se [true], inclui o objeto caller como primeiro parametro.</param>
        /// <returns>retorna um metodo, com nome, e lista de parâmetros compativel aos parâmetros da chamada.</returns>
        public static Metodo FindMethodCompatible(Objeto objCaller, string classObjCaller, string nameMethod, string nameClass, List<Expressao> parameters, Escopo escopo, bool isStatic, bool isToIncludeFirstParameterObjectCaller)
        {

           
            Classe classeDoMetodo = RepositorioDeClassesOO.Instance().GetClasse(nameClass);
           
            if (classeDoMetodo == null)
            {
                return null;
            }

            // obtem os metodos polimorticos, com o mesmo nome e classe, dos parametros de entrada.
            List<Metodo> lst_metodos = classeDoMetodo.GetMetodos();


            // obtem metodos de classe que herdaram a classe parametro.
            List<Classe> todasClasses = RepositorioDeClassesOO.Instance().GetClasses();
            if ((todasClasses != null) && (todasClasses.Count > 0)) 
            {
                for (int i = 0; i < todasClasses.Count; i++) 
                {
                    if ((todasClasses[i].classesHerdadas != null) && (todasClasses[i].classesHerdadas.FindIndex(k => k.nome == nameClass)) != -1)
                    {
                        int index = todasClasses[i].classesHerdadas.FindIndex(k => k.nome == nameClass);
                        List<Metodo> metodosQueHerdaramAClasseParam = todasClasses[i].classesHerdadas[index].GetMetodos().FindAll(k=>k.nome == nameClass);
                        if ((metodosQueHerdaramAClasseParam != null) && (metodosQueHerdaramAClasseParam.Count > 0))
                        {
                            lst_metodos.AddRange(metodosQueHerdaramAClasseParam);
                        }
                    }
                }
            }


            

            // obtem uma lista de metodos da classe do objeto caller, cujo nome é o mesmo do nome da chamada de metodo parametro.
            List<Metodo> metodos = lst_metodos.FindAll(k => k.nome == nameMethod).ToList<Metodo>();


            // obtem uma lista de funções do escopo, cujo nome é o mesmo do nome de metodo da chamada de metodo parametro.
            List<Metodo> funcoes = escopo.tabela.GetFuncoes();


            // adiciona as funcoes, na lista de metodos a investigar a compatibilidade com a chamada de metodo que invocou este metodo.
            if ((funcoes != null) && (funcoes.Count > 0))
            {
                // se a lista de metodos resultou em uma lista vazia, pode ser que o metodo compativel esteja na lista de funcoes,
                // entao inicializa a lista de metodos, afim de investigar as funcoes.
                if (metodos == null)
                {
                    metodos = new List<Metodo>();
                }

                // adiciona as funcoes que tem o mesmo nome do metodo parametro.
                List<Metodo> funcoesComNomeDoMetodo = funcoes.FindAll(k => k.nome == nameMethod).ToList<Metodo>();
                if ((funcoesComNomeDoMetodo != null) && (funcoesComNomeDoMetodo.Count > 0))
                {
                    metodos.AddRange(funcoesComNomeDoMetodo);
                }

            }


            if ((metodos == null) || (metodos.Count == 0))
            {
                return null;
            }


            for (int x = 0; x < metodos.Count; x++)
            {
                bool isFound = true;
                // casos de metodos sem parametros.
                if ((metodos[x].parametrosDaFuncao == null) && (parameters == null))
                {
                    return metodos[x].Clone();
                }
                else
                if ((metodos[x].parametrosDaFuncao.Length == 0) && (parameters != null) && (parameters.Count == 0))
                {
                    return metodos[x].Clone();
                }
                else


                    if ((metodos[x].parametrosDaFuncao == null) && (parameters != null) && (parameters.Count > 0))
                {
                    return null;
                }
                else
                    if ((metodos[x].parametrosDaFuncao == null) && (parameters != null))
                {
                    return null;
                }
                else
                    if ((metodos[x].parametrosDaFuncao != null) && (parameters == null))
                {
                    return null;
                }
                else
                {


                    int indexParamsFunction = 0;
                    int indexParamsCHAMADADeMetodo = 0;
                    if (isToIncludeFirstParameterObjectCaller)
                    {
                        parameters.Insert(0, new ExpressaoObjeto(objCaller));

                    }

                    if (((metodos[x].parametrosDaFuncao == null) || (metodos[x].parametrosDaFuncao.Length == 0)) &&
                        ((parameters != null && parameters.Count == 1 && parameters[0].Elementos.Count == 0))) 
                    {
                        return metodos[x];
                    }


                    // verificacao se os parametros da funcao não tem parametro multi-argumento.
                    if (!HasAMultiArgumentParameter(metodos[x].parametrosDaFuncao))
                    {
                        // se nao tiver, a interface de parametros é fixo, e se a contagem de parâmetros não for igual, passa para outro metodo.
                        if (!CompareCountParameters(metodos[x].parametrosDaFuncao, parameters))
                        {
                            continue;
                        }


                    }
                   
                    for (indexParamsFunction = 0, indexParamsCHAMADADeMetodo = 0; indexParamsFunction < metodos[x].parametrosDaFuncao.Length; indexParamsFunction++, indexParamsCHAMADADeMetodo++)
                    {
                        

                        // obtem o tipo do parametro currente do metodo investigado.
                        string tipoParamsFnc = metodos[x].parametrosDaFuncao[indexParamsFunction].tipo;
                        // CASTING DE PARAMETROS NUMEROS.
                        CastingExpressao(x, indexParamsCHAMADADeMetodo, indexParamsFunction, parameters, metodos, tipoParamsFnc);


                        // VERIFICACAO SE O OBJETO-CALLER DEVE SER INCLUIDO NOS PARAMETROS.
                        if ((LinguagemOrquidea.Instance().isClassToIncludeObjectCallerAsParameter(classeDoMetodo.GetNome())) && (!isStatic)) 
                        {
                            if ((classObjCaller == metodos[x].parametrosDaFuncao[indexParamsFunction].tipo) &&
                                (indexParamsCHAMADADeMetodo < parameters.Count) &&
                                (indexParamsFunction < metodos[x].parametrosDaFuncao.Length)) 
                            {
                                if (parameters[indexParamsCHAMADADeMetodo].tipoDaExpressao == metodos[x].parametrosDaFuncao[indexParamsFunction].tipo)
                                {
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                           
                        }


                        // VERIFICACAO DE O PARAMETRO DO METODO É DO TIPO [OBJECT].
                        if (metodos[x].parametrosDaFuncao[indexParamsFunction].tipo == "Object")
                        {
                            continue;
                        }
                        else
                        /// PROCESSAMENTO DE  PARAMETRO-METODO.
                        if ((metodos[x].parametrosDaFuncao[indexParamsFunction].isFunctionParameter) && (parameters[indexParamsCHAMADADeMetodo].GetType() == typeof(ExpressaoChamadaDeMetodo)))
                        {
                            ExpressaoChamadaDeMetodo expressaoChamada = (ExpressaoChamadaDeMetodo)parameters[indexParamsCHAMADADeMetodo];

                            // obtem o metodo parametro na expressao chamada de metodo.
                            Metodo metodoChamada = expressaoChamada.funcao;

                            HeaderClass headerParametroFuncao = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == metodos[x].nomeClasse);
                            if (headerParametroFuncao != null)
                            {


                                HeaderMethod headerMethod = headerParametroFuncao.methods.Find(k => k.name == metodos[x].nome);
                                if (headerMethod != null)
                                {
                                    List<HeaderProperty> paramtersOfFuncionParameter = headerMethod.parameters;
                                    if ((paramtersOfFuncionParameter.Count == 0) && (metodos[x].parametrosDaFuncao.Length == 0))
                                    {
                                        continue;
                                    }
                                    if ((paramtersOfFuncionParameter.Count != 0) && (metodos[x].parametrosDaFuncao.Length == 0))
                                    {
                                        break;
                                    }
                                    if ((paramtersOfFuncionParameter.Count == 0) && (metodos[x].parametrosDaFuncao.Length != 0))
                                    {
                                        break;
                                    }
                                    // validacao de tipos de retorno.
                                    if (metodos[x].tipoReturn != headerMethod.typeReturn)
                                    {
                                        break;
                                    }
                                    // validacao dos tipos de parametros.
                                    for (int p = 0; p < paramtersOfFuncionParameter.Count; p++)
                                    {
                                        if (metodos[x].parametrosDaFuncao[p].tipo != ((ObjectHeader)paramtersOfFuncionParameter[p]).typeOfProperty)
                                        {
                                            break;
                                        }
                                    }

                                }
                                else
                                {
                                    break;
                                }
                            }
                
                        }
                        else
                        // O PARAMETRO É UM PARAMETRO NORMAL.
                        if ((indexParamsCHAMADADeMetodo < parameters.Count) &&
                            (parameters[indexParamsCHAMADADeMetodo].Elementos!=null) &&
                            (parameters[indexParamsCHAMADADeMetodo].Elementos.Count>0) &&
                            (!metodos[x].parametrosDaFuncao[indexParamsFunction].isFunctionParameter) &&(!metodos[x].parametrosDaFuncao[indexParamsFunction].isMultArgument) && (!metodos[x].isToIncludeCallerIntoParameters)) 
                        {
                            string tipoDoParametroDaFuncao = metodos[x].parametrosDaFuncao[indexParamsFunction].tipo;
                            string tipoParamertroCHAMADA = parameters[indexParamsCHAMADADeMetodo].Elementos[0].tipoDaExpressao;
                            if (tipoDoParametroDaFuncao.IndexOf(".") != -1)
                            {
                                int indexLastDot = tipoDoParametroDaFuncao.LastIndexOf(".");
                                tipoDoParametroDaFuncao= tipoDoParametroDaFuncao.Substring(indexLastDot + 1);
                            }




                            if (tipoDoParametroDaFuncao == tipoParamertroCHAMADA) 
                            {
                                continue;
                            }
                            else
                            {
                                isFound = false;
                                break;
                            }
                        }


                        // PARAMETRO É MULTI-ARGUMENTO.
                        // o parametro é um multi-argumento (lista variavel de parametros), e é do tipo [Vector], que armazenará os valores em quantidade variavel.
                        if (metodos[x].parametrosDaFuncao[indexParamsFunction].isMultArgument)
                        {
                            if (metodos[x].parametrosDaFuncao[indexParamsFunction].GetTipo() == "Vector")
                            {
                                // faz a validacao de parametro multi-argumento.
                                if (!ValidatingParametersMultiArguments(metodos[x]))
                                {
                                    int i_multi = indexParamsCHAMADADeMetodo;
                                    int contadorArgs = 0;
                                    string tipoDoMultiArgumento = metodos[x].parametrosDaFuncao[indexParamsFunction].tipoElemento;

                                    // obtem a faixa de objetos dentro de multi-argumento.
                                    while ((i_multi < parameters.Count) && (parameters[i_multi].GetTipoExpressao() == tipoDoMultiArgumento))
                                    {
                                        i_multi++;
                                        contadorArgs++;
                                    }

                                    // verifica se atingiu tokens de parametro multi-agumento;
                                    if (i_multi <= parameters.Count)
                                    {

                                        // acerta o indice de malha de expressoes parametro.
                                        indexParamsCHAMADADeMetodo += contadorArgs;
                                        continue;
                                    }
                                    else
                                    {
                                        isFound = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    isFound = false;
                                    break;
                                }

                            }
                            else
                            {
                                isFound = false;
                                break;
                            }
                        }

                    }

                    if (isFound)
                    {
                        return metodos[x].Clone();
                    }

                    else
                        continue;

                }




            }

            return null;
        }


        /// <summary>
        /// compara se a contagem de parâmetros é igual.
        /// </summary>
        /// <param name="parametrosDeFuncao">array de parametros de um metodo.</param>
        /// <param name="parametrosChamadaDeMetodo">lista de parametros de uma chamada de método.</param>
        /// <returns>[true] se tem a mesma contagem de parametros, ou [true] não tem lista e array vazios, [false] senão.</returns>
        private static bool CompareCountParameters(Objeto[] parametrosDeFuncao, List<Expressao> parametrosChamadaDeMetodo)
        {
            if ((parametrosDeFuncao == null) || ((parametrosDeFuncao.Length == 0)) && ((parametrosChamadaDeMetodo == null) || (parametrosChamadaDeMetodo.Count == 0)))
            {
                return true;
            }
            else
            {
                if ((parametrosDeFuncao != null) && ((parametrosChamadaDeMetodo == null) || (parametrosChamadaDeMetodo.Count == 0)))
                {
                    return false;
                }
                else
                if ((parametrosDeFuncao == null) && (((parametrosChamadaDeMetodo != null) && (parametrosChamadaDeMetodo.Count > 0))))
                {
                    return false;
                }
                else
                if (parametrosDeFuncao.Length!= parametrosChamadaDeMetodo.Count)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
      

        /// <summary>
        /// verifica se parametros de uma função há um parametro multi-argumento,
        /// que tem composição para uma lista de parâmetros de cumprimento variável.
        /// </summary>
        /// <param name="parametros">array de parâmetros de uma função.</param>
        /// <returns>[true] se há um ou mais parâmetro multi-argumento, [false] se não tiver parametros
        /// ou for um array vazio, ou sem parâmetros multi-argumento.</returns>
        private static bool HasAMultiArgumentParameter(Objeto[] parametros)
        {
            if ((parametros == null) || (parametros.Length == 0))
            {
                return false;
            }
            else
            {
                for (int i = 0; i < parametros.Length; i++)
                {
                    if (parametros[i].isMultArgument)
                    {
                        return true;
                    }
                }

                return false;
            }
        }


        /// <summary>
        /// encontra um construtor registrado, compativel com os parametros expressoes.
        /// </summary>
        /// <param name="nameClass">nome da classe do construtor.</param>
        /// <param name="parametrosExpressao">expressoes que formam os parametros.</param>
        /// <returns>retorna o metodo do construtor compativel, ou null se nao encontrar um construtor compativel.</returns>
        public static Metodo FindConstructorCompatible(string nameClass,List<Expressao> parametrosExpressao, ref int indexConstrutor)
        {

            Classe classeDoMetodo = RepositorioDeClassesOO.Instance().GetClasse(nameClass);

            if (classeDoMetodo == null)
            {
                return null;
                
            }
            

            Classe tipo = RepositorioDeClassesOO.Instance().GetClasse(nameClass);
            List<Metodo> lst_construtores = new List<Metodo>();

            if ((tipo != null) && (tipo.construtores != null) && (tipo.construtores.Count > 0)) 
            {

                lst_construtores.AddRange(tipo.construtores);
            }

            if ((lst_construtores != null) && (lst_construtores.Count > 0))
            {
                for (int i = 0; i < lst_construtores.Count; i++)
                {
                    if (((lst_construtores[i].parametrosDaFuncao == null) || (lst_construtores[i].parametrosDaFuncao.Length == 0)) &&
                         (parametrosExpressao == null || (parametrosExpressao.Count == 0)))
                    {
                        indexConstrutor = i;
                        return lst_construtores[i];
                    }
                    else
                    if ((lst_construtores[i].parametrosDaFuncao != null) && (lst_construtores[i].parametrosDaFuncao.Length > 0))
                    {
                        bool isFound = true;
                        if (lst_construtores[i].parametrosDaFuncao.Length != parametrosExpressao.Count)
                        {
                            continue;
                        }
                        for (int c = 0; c < lst_construtores[i].parametrosDaFuncao.Length; c++)
                        {
                            string tipoParamsEXPRESSAO_CHAMDA = parametrosExpressao[c].Elementos[0].tipoDaExpressao;
                            string tipoParamsFUNCAO = lst_construtores[i].parametrosDaFuncao[c].tipo;

                            if ((tipoParamsFUNCAO == "Int32") && (tipoParamsEXPRESSAO_CHAMDA == "int"))
                            {
                                continue;
                            }

                            if ((tipoParamsFUNCAO == "int") && (tipoParamsEXPRESSAO_CHAMDA == "Int32"))
                            {
                                continue;
                            }

                            if ((ExpressaoNumero.isClasseNumero(tipoParamsFUNCAO)) && (ExpressaoNumero.isClasseNumero(tipoParamsEXPRESSAO_CHAMDA)))
                            {


                                if ((tipoParamsEXPRESSAO_CHAMDA == "double") && (tipoParamsFUNCAO.ToLower() == "single"))
                                {
                                    continue;
                                }

                                if ((tipoParamsEXPRESSAO_CHAMDA.ToLower() == "single") && (tipoParamsFUNCAO == "double"))
                                {
                                    continue;
                                }
                            }

                            if (parametrosExpressao[c].Elementos[0].tipoDaExpressao.ToLower() == lst_construtores[i].parametrosDaFuncao[c].tipo.ToLower()) 
                            {
                                continue;
                            }
                            else
                            {
                                isFound = false;
                                break;
                            }
                        }

                        if (isFound)
                        {
                            indexConstrutor = i;
                            return lst_construtores[i];
                        }
                    }
                }
            }
            

            

            return null;
        }

        /// <summary>
        /// faz o casting entre numeros: double,float,int, convertendo o numero da expressao para o numero do parametro da funcao.
        /// </summary>
        /// <param name="x">indice do metodo currente.</param>
        /// <param name="indexParamsCHAMADADeMetodo">indice do parametro da chamada de metodo.</param>
        /// <param name="indexParamsFunction">indice do parametro da lista de parametros do metodo compativel.</param>
        /// <param name="parameters">lista de expressoes parametros da chamada de metodo.</param>
        /// <param name="metodos">lista de metodos compativeis ou nao.</param>
        /// <param name="tipoParamsFnc">tipo do parametro da função, como base para a conversão.</param>
        private static void CastingExpressao(int x, int indexParamsCHAMADADeMetodo,int indexParamsFunction, List<Expressao> parameters, List<Metodo>metodos, string tipoParamsFnc)
        {

            // VEFIRICA SE O PARAMETRO É UMA CONSTANTE NUMERO, E FAZ CASTING ENTRE OS NUMEROS DA FUNCAO E DA CHAMADA, SE NECESSÁRIO.
            if ((indexParamsCHAMADADeMetodo < parameters.Count) &&
                (parameters[indexParamsCHAMADADeMetodo].Elementos!=null) && (parameters[indexParamsCHAMADADeMetodo].Elementos.Count>0) &&  
                ((parameters[indexParamsCHAMADADeMetodo].Elementos[0].GetType() == typeof(ExpressaoNumero)) || 
                (parameters[indexParamsCHAMADADeMetodo].Elementos[0].GetType() == typeof(ExpressaoObjeto))) && ((tipoParamsFnc == "double") || (tipoParamsFnc == "float") || (tipoParamsFnc == "int")))
            {
            
                string tipoNumberRequired = metodos[x].parametrosDaFuncao[indexParamsFunction].tipo;
                string tipoNumberChamada = parameters[indexParamsCHAMADADeMetodo].Elementos[0].tipoDaExpressao;


                // conversao de int/float para double.
                if (tipoNumberRequired == "double") 
                {
                    parameters[indexParamsCHAMADADeMetodo].Elementos[0].tipoDaExpressao = "double";
                    if (parameters[indexParamsCHAMADADeMetodo].Elementos[0].GetType() == typeof(ExpressaoObjeto))
                    {
                        ((ExpressaoObjeto)parameters[indexParamsCHAMADADeMetodo].Elementos[0]).objectCaller.tipo = "double";
                    }
                    
                    return;
                }


                // conversao de float/double para int.
                if (tipoNumberRequired == "int")
                {
                    if (tipoNumberChamada == "double")
                    {
                        parameters[indexParamsCHAMADADeMetodo].Elementos[0].tipoDaExpressao = "int";
                        if (parameters[indexParamsCHAMADADeMetodo].Elementos[0].GetType() == typeof(ExpressaoObjeto))
                        {
                            ((ExpressaoObjeto)parameters[indexParamsCHAMADADeMetodo].Elementos[0]).objectCaller.tipo = "int";
                        }

                        return;
                    }
                    else
                    if (tipoNumberChamada == "float")
                    {
                        parameters[indexParamsCHAMADADeMetodo].Elementos[0].tipoDaExpressao = "int";

                        if (parameters[indexParamsCHAMADADeMetodo].Elementos[0].GetType() == typeof(ExpressaoObjeto)) 
                        {
                            ((ExpressaoObjeto)parameters[indexParamsCHAMADADeMetodo].Elementos[0]).objectCaller.tipo = "int";
                        }

                        return;
                    }

                }


            }


        }



         /// <summary>
        /// retorna true se há parametros multi-argummentos invalidados.
        /// 2 parametros multi-argumentos em sequencia não podem ter o mesmo tipo,
        /// pois torna impossivel saber se um parametro pertence a qual parametro-argumento.
        /// </summary>
        /// <param name="method">metodo com os parmaetros multi-argumentos.</param>
        /// <returns>[true] se é invalido o metodo com parametros multi-argumentos, [false] se houve irregularidade.</returns>
        public static bool ValidatingParametersMultiArguments(Metodo method)
        {
            if ((method.parametrosDaFuncao != null) && (method.parametrosDaFuncao.Length >= 2))
            {
                for (int i = 1; i < method.parametrosDaFuncao.Length; i++)
                {
                    if ((method.parametrosDaFuncao[i].isMultArgument) &&
                       (method.parametrosDaFuncao[i - 1].isMultArgument))
                    {
                        if (method.parametrosDaFuncao[i].tipo == method.parametrosDaFuncao[i - 1].tipo)
                        {
                            return true;
                        }
                        else
                        if ((WrapperData.isWrapperData(method.parametrosDaFuncao[i].tipo) != null) &&
                            (method.parametrosDaFuncao[i].tipoElemento == method.parametrosDaFuncao[i - 1].tipoElemento)) 
                        {
                            return true;
                        }
                    }


                }
            }
            return false;

        }

        
        /// <summary>
        /// faz a liguação entre um escopo principal, com um sub-escopo.
        /// </summary>
        /// <param name="escopoPai">escopo principal.</param>
        /// <param name="escopoFilho">sub-escopo.</param>
        public static void LinkEscopoPaiEscopoFilhos(Escopo escopoPai, Escopo escopoFilho)
        {
            if ((escopoPai != null) && (escopoFilho != null))
            {
                escopoFilho.escopoPai = escopoPai;
                escopoPai.escopoFolhas.Add(escopoFilho);
            }
            
        }


        /// <summary>
        /// formata um texto de entrada, colocando espaçamento de 1 unidade, nos tokens do texto.
        /// </summary>
        /// <param name="input">texto a ser formatado.</param>
        public static string FormataEntrada(string input)
        {
            string inputOut = "";
            List<string> tokensInput = new Tokens(input).GetTokens();
            if (tokensInput != null)
                for (int x = 0; x < tokensInput.Count; x++)
                    inputOut += tokensInput[x] + " ";
            return inputOut.Trim(' ');
        }

  
        public class Testes: SuiteClasseTestes
        {
            public Testes():base("testes de classe util tokens")
            {

            }


            public void TesteProgramaComFalhas2(AssercaoSuiteClasse assercao)
            {

                string pathFile = @"programasTestes\programHelloWorldComFalha.txt";
                ParserAFile.DEBUG = false;
                ParserAFile.ExecuteAProgram(pathFile);
            }

            public void TesteModulesAndLibraries(AssercaoSuiteClasse assercao)
            {
                ParserAFile.DEBUG = false;
                ParserAFile.ExecuteAProgram(@"programasTestes\programContagensModules.txt");


            }



            public void TesteCalculoDeLinhas(AssercaoSuiteClasse assercao)
            {
                string pathFile = @"programa jogos\SpaceInvadersSFML_ComImagens.txt";
                ParserAFile.DEBUG = true;
                ParserAFile.ExecuteAProgram(pathFile);
            }


            public void TesteParametrosMetodos(AssercaoSuiteClasse assercao)
            {
                string codigo_classeA_0_0 = "public class classeA { public int propriedadeA; public int metodoA(int y){int x=2;};  public classeA(){int x=1; }; public int metodoB(int funcaco(int x), a) {int m=1;}};";
                string codigo_create = "classeA obj1= create();";
                string codigo_expressaoChamada = "obj1.metodoB(obj1.metodoA(1),1)";


                ProcessadorDeID compilador = new ProcessadorDeID(codigo_classeA_0_0 + codigo_create);
                compilador.Compilar();

                Expressao exprssChamada = new Expressao(codigo_expressaoChamada, compilador.escopo);

                try
                {
                    assercao.IsTrue(exprssChamada.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo_expressaoChamada);
                    assercao.IsTrue(((ExpressaoChamadaDeMetodo)exprssChamada.Elementos[0]).parametros[0].Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo_expressaoChamada);
                }
                catch (Exception e)
                {
                    string codeError = e.Message;
                    assercao.IsTrue(false, "FALHA NO TESTE:");
                }

            }


        
            public void TesteMetodosParametrosComChamadaDeFuncao(AssercaoSuiteClasse assercao)
            {
                string codigo_classeA_0_0 = "public class classeA { public int propriedadeA; public int metodoB(int funcaco(int x), int a) { funcao(1);} public classeA(){int x=1; };  public int metodoA(int y){int x=2;}; };";
                string codigo_create = "classeA obj1= create();";
                string codigo_expressaoChamada = "obj1.metodoB(obj1.metodoA(1),1)";


                ProcessadorDeID compilador = new ProcessadorDeID(codigo_classeA_0_0 + codigo_create);
                compilador.Compilar();

                Expressao exprssChamada = new Expressao(codigo_expressaoChamada, compilador.escopo);

                try
                {
                    assercao.IsTrue(exprssChamada.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo_expressaoChamada);
                    assercao.IsTrue(((ExpressaoChamadaDeMetodo)exprssChamada.Elementos[0]).parametros[0].Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo_expressaoChamada);
                   
                }
                catch (Exception e)
                {
                    string codeError = e.Message;
                    assercao.IsTrue(false, "FALHA NO TESTE:");
                }

            }
     


        }
    }


}
