using MathNet.Numerics.Statistics.Mcmc;
using parser.ProgramacaoOrentadaAObjetos;
using parser.PROLOG;
using ParserLinguagemOrquidea.Wrappers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.Marshalling;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Util;

using Wrappers;
using static parser.ExpressaoGrupos;
using static System.Formats.Asn1.AsnWriter;

namespace parser
{
    /// <summary>
    /// motor de processamento de expressoes.
    /// </summary>
    public class ExpressaoPorClassificacao
    {
        /// <summary>
        /// tabela hash de classificao de tokens.
        /// </summary>
        public static Classificador.TabelaHash TABLE;

        /// <summary>
        /// contador de ids para objetos estaticos.
        /// </summary>
        private static int countNamesObjectsStatic = 0;

        /// <summary>
        /// contador de ids para objetos-elemento de wrapper object.
        /// </summary>
        public static int countObjetosElementosDeWrapperObject = 0;

        private enum typeExpreession { none, propriedadeAninhada, chamadaDeMetodo };
        private enum typeOperator { binary, unary, binaryAndUnary, none };


        /// <summary>
        /// operadores binarios, dois operandos.
        /// </summary>
        private static List<string> opBinary = null;
        /// <summary>
        /// operadores unarios, um operando. pode ser pos ou pre.
        /// </summary>
        private static List<string> opUnary = null;
        /// <summary>
        /// operadores que são binarios e unarios, como [+,-].
        /// </summary>
        private static List<string> opUnaryAndBinary = null;

        /// <summary>
        /// lista de wrappers data processamento.
        /// </summary>
        private static List<WrapperData> wrappersDATA = null;

        /// <summary>
        /// contem a quantidade de expressoes independentes foramm processado.
        /// </summary>
        public int QUANTIDADE_EXPRESSOES_INDEPPENDENTES = 0;

       
        /// <summary>
        /// dados de operadores encontrados nos tokens da expressão,
        /// </summary>
        private List<DataOperators> dataOperadores = new List<DataOperators>();

        /// <summary>
        /// guarda os tokens de parametros de uma funcao.
        /// </summary>
        private List<string> tokensParametros = new List<string>();


        private bool isToContinue = false;

        /// <summary>
        /// faz o processamento de expressoes, construindo sub-expressoes da expressao representado pelos tokens parametros.
        /// </summary>
        /// <param name="tokens">lista de tokens da expressao.</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        /// <returns>retorna uma lista de sub-expressoes, ou null se houve falha no processamento.</returns>
        public List<Expressao> ExtraiExpressoes(List<string> tokens, Escopo escopo)
        {

            string code = Utils.OneLineTokens(tokens);
            return ExtraiExpressoes(code, escopo);
        }

        /// <summary>
        /// faz o processamento de expressoes, construindo sub-expressoes da expressao representado pelos tokens parametros.
        /// </summary>
        /// <param name="code">texto contendo os tokens da expressao.</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        /// <returns>retorna uma lista de sub-expressoes, ou null se houve falha no processamento.</returns>
        public List<Expressao> ExtraiExpressoes(string code, Escopo escopo)
        {



            if (escopo == null)
            {
                escopo = Escopo.escopoROOT;
            }

            List<Expressao> exprssRetorno = new List<Expressao>();
            TABLE = Classificador.TabelaHash.Instance(code);

            // header da classe currente.
            HeaderClass headerClasse = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == Escopo.nomeClasseCurrente);

            List<string> tokens = new Tokens(code).GetTokens();

           

            // PRE-PROCESSAMENTO de ANOTAÇÕES WRAPPER OBJECT.
            preProcessamentoCREATEWrappersObject(tokens, 0, escopo);
            preProcessamentoGET_SET_WrappersObject(ref tokens, 0, escopo);


            // PROCESSAMENTO DE MAIS DE 1 EXPRESSAO INDEPENDENTE.
            if ((tokens.FindAll(k => k.Equals(";")) != null) && (tokens.FindAll(k => k.Equals(";")).Count > 1))
            {
                return BuildExpressionsIndependenstes(tokens, escopo);
            }



            for (int x = 0; x < tokens.Count; x++)
            {
              

                // FINAL DE EXPRESSAO.
                if (tokens[x] == ";") 
                {
                    QUANTIDADE_EXPRESSOES_INDEPPENDENTES += 1;
                    // todas expressoes independente foram construidas.
                    if (x >= tokens.Count - 1) 
                    {
                        
                        break;
                    }
                        
                }



                // PROCESSAMENTO DO TOKEN TRUE.
                if (tokens[x] == "TRUE")
                {
                    Objeto objetoTrue = new Objeto("private", "bool", Objeto.TRUE, true);
                    ExpressaoObjeto exprssBoolTrue = new ExpressaoObjeto(objetoTrue);
                    exprssRetorno.Add(exprssBoolTrue);
                    escopo.tabela.RegistraObjeto(objetoTrue);
                    continue;
                }
                else
                // PROCESSAMENTO DO TOKEN FALSE.
                if (tokens[x] == "FALSE")
                {
                    Objeto objetoFalse = new Objeto("private", "bool", Objeto.FALSE, false);
                    ExpressaoObjeto exprssBoolFALSE= new ExpressaoObjeto(objetoFalse);
                    exprssRetorno.Add(exprssBoolFALSE);
                    escopo.tabela.RegistraObjeto(objetoFalse);
                    continue;
                }
                // OTIMIZAÇÃO DE EXPRESSOES DE ATRIBUICAO SIMPLES.
                if ((x + 3 < tokens.Count) && (tokens[x + 2] == "="))
                {
                    string possibleClass = tokens[x];
                    string possibObject = tokens[x + 1];

                    // EXPRESSAO BOOLEANA DE ATRIBUICAO.
                    if (possibleClass=="bool")
                    {
                        Objeto objBooleano = new Objeto("private", possibleClass, possibObject, null);

                        if ((x + 3 < tokens.Count) && (tokens[x+3]=="TRUE") || (tokens[x+3]=="FALSE"))
                        {
                            ExpressaoObjeto exprssATRIBUIR = null;
                            ExpressaoObjeto exprssRECEBE_ATRIBUICAO = new ExpressaoObjeto(objBooleano);
                            if (tokens[x + 3] == "TRUE")
                            {
                                objBooleano.valor = true;
                                exprssATRIBUIR = new ExpressaoObjeto(new Objeto("private", "bool", "TRUE", true));
                            }
                            else
                            if (tokens[x + 3] == "FALSE") 
                            {
                                objBooleano.valor = false;
                                exprssATRIBUIR = new ExpressaoObjeto(new Objeto("private", "bool", "FALSE", false));
                            }

                            // registra o objeto no escopo.
                            escopo.tabela.RegistraObjeto(objBooleano);
  
                            // constroi a expressao de atribuicao.
                            ExpressaoAtribuicao exprssAtrib = new ExpressaoAtribuicao(exprssRECEBE_ATRIBUICAO, exprssATRIBUIR, escopo);
                            exprssRetorno.Add(exprssAtrib);
                            return exprssRetorno;
                        }
                    }
                   // EXPRESSAO NUMERO, LITERAL DE ATRIBUICAO, E ATRIBUICOES MAIS COMPLEXAS.
                   if ((TABLE.isNomeClasse(possibleClass)) && (TABLE.isNomeObjeto(possibObject)))
                    {

                        if (x + 4 < tokens.Count)
                        {
                            // OBJETO NUMERO.
                            if ((ExpressaoNumero.isClasseNumero(possibleClass)) &&
                                (ExpressaoNumero.isNumero(tokens[x + 3])) &&
                                (tokens[x + 4] == ";"))
                            {
                                Objeto obj = new Objeto("private", possibleClass, possibObject, ExpressaoNumero.castingNumero(tokens[x + 3]));
                                escopo.tabela.RegistraObjeto(obj);
                                ExpressaoObjeto exprssObjetoRecebeAtribuicao = new ExpressaoObjeto(obj);
                                ExpressaoNumero exprssExpressaoAtribuir = new ExpressaoNumero(tokens[x + 3]);
                                ExpressaoAtribuicao exprss = new ExpressaoAtribuicao(exprssObjetoRecebeAtribuicao, exprssExpressaoAtribuir, escopo);
                                exprssRetorno.Add(exprss);

                                return exprssRetorno;

                            }
                            else
                            // OBJETO LITERAL.
                            if ((possibleClass == "string") &&
                                (tokens[x + 4] == ";") &&
                                (x + 3 < tokens.Count) &&
                                (ExpressaoLiteralText.isConstantLiteral(tokens[x + 3])))
                            {
                                string texto = tokens[x + 3];
                                if (texto[0] == ExpressaoLiteralText.aspas)
                                {
                                    if (texto.Length >= 2)
                                    {
                                        texto = texto.Substring(1, texto.Length - 2);
                                    }

                                }
                                Objeto obj = new Objeto("private", "string", possibObject, texto);
                                escopo.tabela.RegistraObjeto(obj);

                                ExpressaoObjeto exprssObjetoRecebeAtribuicao = new ExpressaoObjeto(obj);
                                ExpressaoLiteralText exprssExpressaoAtribuir = new ExpressaoLiteralText(texto);
                                ExpressaoAtribuicao exprss = new ExpressaoAtribuicao(exprssObjetoRecebeAtribuicao, exprssExpressaoAtribuir, escopo);
                                exprssRetorno.Add(exprss);

                                return exprssRetorno;

                            }
                            //  QUANDO O VALOR TEM MAIS DE 1 TOKEN.
                            else
                            if (tokens[x + 4] != ";")
                            {
                                int indexOperatorEquals = tokens.IndexOf("=");
                                int indexOperadorFinalExpressao = tokens.IndexOf(";", indexOperatorEquals + 1);
                                if ((indexOperatorEquals != -1) && (indexOperadorFinalExpressao != -1))
                                {
                                    // cria os tokens da expressao do valor a atribuir.
                                    List<string> tokensExpressaoValor = tokens.GetRange(indexOperatorEquals + 1, indexOperadorFinalExpressao - (indexOperatorEquals + 1) + 1);
                                    if (tokensExpressaoValor.Count > 0)
                                    {
                                        // criaa o objeto da otimização.
                                        Objeto objRecebeAtribuicao = new Objeto("private", possibleClass, possibObject, null);
                                        escopo.tabela.RegistraObjeto(objRecebeAtribuicao);

                                        // cria as expressoes para a expressao atribuicao.
                                        ExpressaoObjeto exprrsObj = new ExpressaoObjeto(objRecebeAtribuicao);
                                        Expressao expressaoAAtribuir = new Expressao(tokensExpressaoValor.ToArray(), escopo);


                                        ExpressaoObjeto exprssObjetoRecebeAtribuicao = new ExpressaoObjeto(objRecebeAtribuicao);
                                        ExpressaoAtribuicao exprssAtrib = new ExpressaoAtribuicao(exprrsObj, expressaoAAtribuir, escopo);

                                        exprssRetorno.Add(exprssAtrib);
                                        return exprssRetorno;


                                    }
                                }


                            }

                        }


                    }
                }


                // VERIFICACAO SE O TOKEN É UM NOME DE FUNCAO DA CLASSE CURRENTE.
                HeaderMethod headerMethod = null;
                if (headerClasse != null)
                {
                    headerMethod = headerClasse.methods.Find(k => k.name == tokens[x]);
                }


                // EXPRESSAO CHAMADA DE METODO COM NOME DE FUNCAO E SEM OBJETO CALLER DE CHAMADA.
                if ((headerMethod != null) && (TABLE.isNomeFuncao(tokens[x])) && (x - 1 >= 0) && (tokens[x - 1] != ".") && (tokens.Contains("("))) 
                {
                    // nome da funcao.
                    string nomeFuncao = tokens[x];
                    // nome da classe que a funcao pertence.
                    string classeDaFuncao = Escopo.nomeClasseCurrente;


                    Objeto OBJETO_CALLER = new Objeto("private", classeDaFuncao, "obj_actual" + countNamesObjectsStatic++, null);
                    escopo.tabela.RegistraObjeto(OBJETO_CALLER);
                   
                    List<string> tokensParametros5 = new List<string>();
                     // obtem a lista de parametros da chamada de metodo.
                    List<Expressao> parametros = BuildParameters(tokens, escopo, x, ref tokensParametros5);
                    if (parametros != null)
                    {
                        Metodo fnc = GetFunctionCompatible(parametros, nomeFuncao, classeDaFuncao, OBJETO_CALLER, escopo, OBJETO_CALLER.isStatic, false);
                        if (fnc != null)
                        {

                            Classe classeFnc = RepositorioDeClassesOO.Instance().GetClasse(classeDaFuncao);
                            bool validaAcessors = ValidaACESSOR_METODO(fnc, tokensParametros5, escopo);

                            if (validaAcessors)
                            {
                                ExpressaoChamadaDeMetodo exprssChamada = new ExpressaoChamadaDeMetodo(OBJETO_CALLER, fnc, parametros);
                                exprssRetorno.Add(exprssChamada);
                                if (tokensParametros != null)
                                {
                                    
                                    x += 1 + tokensParametros.Count; //+1 do nome da funcao, +x da contagem de tokens dos parametros.
                                }

                                continue;
                            }
                            else
                            {
                                UtilTokens.WriteAErrorMensage("function: "+nomeFuncao+"  sintax error, acessor a method private or protected, not valid class where is!"+Utils.OneLineTokens(tokens)+".", tokens, escopo);
                                return null;

                            }


                        }
                    }
                    else
                    {
                        UtilTokens.WriteAErrorMensage("function: " + nomeFuncao + " bad format for parameters list: " + OBJETO_CALLER.nome + "object.  " + Utils.OneLineTokens(tokens)+".", tokens, escopo);
                        return null;
                    }
                }

                // INSTANCIACAO DE OBJETO ESTATICO/OBJETO NAO ESTATICO.
                if (TABLE.isNomeClasse(tokens[x]))
                {
                    string nomeClasse = tokens[x];

                    // CONSTRUCAO DE OBJETO ESTATICO.
                    if (isNextToken(tokens, ".", x))
                    {

                        string acessor1 = "public";
                        string nameProp = "objStatic";

                        // NOME DA PROPRIEDADE ESTATICA.
                        if (x + 2 < tokens.Count)
                        {
                            nameProp = tokens[x + 2] + "_static";
                        }
                        


                        if (Escopo.nomeClasseCurrente == "")
                        {
                            // se o objeto estiver no escopo global, seta para [public] o acessor, é visivel para todos metodos de classes.
                            acessor1 = "public";
                        }

                        // DEFINICOES DA PROPRIEDADE ESTATICA.
                        object valorObjeto = Objeto.GetStaticObject(nomeClasse);
                        Objeto objSTATIC = Objeto.GetObjetoByDefinitionInHeaders(nomeClasse);
                        objSTATIC.nome = tokens[x + 2].ToString();
                        objSTATIC.valor = valorObjeto;
                        objSTATIC.isStatic = true;

                        // PROPRIEDADE ESTÁTICA.
                        if ((x + 2 < tokens.Count) && (TABLE.isNomePropriedade(tokens[x + 2])))
                        {



                            // VALOR DA PROPRIEDADE ESTATICA.
                            if ((valorObjeto == null) && (x + 4 < tokens.Count))
                            {
                                List<string> valorProp = tokens.GetRange(x + 4, tokens.Count - (x + 4));
                                Expressao exprssValor = new Expressao(valorProp.ToArray(), escopo);
                                if (exprssValor == null)
                                {
                                    UtilTokens.WriteAErrorMensage("invalid value of property:  " + tokens[x + 2] + " in static object:  " + objSTATIC.nome + "   " + Utils.OneLineTokens(tokens) + ".  ", tokens, escopo);
                                    return null;
                                }

                                EvalExpression eval = new EvalExpression();
                                object result = eval.EvalPosOrdem(exprssValor, escopo);


                                int indexProp = objSTATIC.propriedades.FindIndex(k => k.nome == nameProp);
                                if (indexProp != -1)
                                {
                                    objSTATIC.propriedades[indexProp].valor = result;
                                }

                            }


                            // PROCESSO PARA CLASSES STRING, DOUBLE.
                            Classe classe = RepositorioDeClassesOO.Instance().GetClasse(nomeClasse);
                            if (classe != null)
                            {

                                if (SystemInit.classeFirstArgumntAsObjectCaller.FindIndex(k => k.Equals(nomeClasse)) != -1)
                                {
                                    if (nomeClasse == "string")
                                    {
                                        Type type = Type.GetType(typeof(MetodosString).FullName);
                                        objSTATIC.valor = Metodo.BuildObjectFromConstrutorWithoutParameters(type);
                                    }
                                    else
                                    if (nomeClasse == "double")
                                    {
                                        Type type = Type.GetType(typeof(MetodosDouble).FullName);
                                        objSTATIC.valor = Metodo.BuildObjectFromConstrutorWithoutParameters(type);
                                    }
                                }

                            }
                            if (objSTATIC.valor == null)
                            {
                                objSTATIC.valor = objSTATIC;
                            }

                            if (objSTATIC == null)
                            {
                                UtilTokens.WriteAErrorMensage("class: " + nomeClasse + "of a static object dont registred!", tokens, escopo);
                                return null;
                            }

                            // REGISTRA O NOME DO OBJETO ESTATICO NA TABELA HASH de classificacao! NÃO REGISTRA NO ESCOPO, PORQUE JÁ FOI REGISTRADO EM EXTRATORESOO.
                            TABLE.WriteToken(tokens[x]);

                            // faz uma chamada/propriedade estática, substituindo o token currente de nome de classe,
                            // por um nome de objeto estático, que não gera efeitos na programação, justamente porque
                            // está fora do programa. 
                        }

                        // CHAMADA DE METODO ESTATICA.
                        if ((x + 2 < tokens.Count) && (tokens[x + 1] == ".") && (TABLE.isNomeFuncao(tokens[x + 2])))
                        {
                            
                            object valorObj = Objeto.GetStaticObject(nomeClasse);
                            objSTATIC = new Objeto(acessor1, nomeClasse, "objStatic" + countNamesObjectsStatic++, valorObj);
                            objSTATIC.nome = tokens[x + 2] + "_static".ToString();
                            objSTATIC.isStatic = true;
                            if (objSTATIC.valor == null)
                            {
                                objSTATIC.valor = objSTATIC;
                            }


                            string nomeFuncao = tokens[x + 2];

                            List<string> tokensParametros = new List<string>();
                            List<Expressao> exprss_params = BuildParameters(tokens, escopo, x, ref tokensParametros);
                            if (exprss_params != null)
                            {
                                Metodo fnc = UtilTokens.FindMethodCompatible(objSTATIC, nomeClasse, nomeFuncao, objSTATIC.tipo, exprss_params, escopo,true, false);
                                if (fnc != null)
                                {
                                    ExpressaoChamadaDeMetodo exprssChamadaEstatica = new ExpressaoChamadaDeMetodo(objSTATIC, fnc, exprss_params);
                                    exprssRetorno.Add(exprssChamadaEstatica);

                                    x += 1 + 1 + 1 + tokensParametros.Count + 2;//+1 do nome da classe, +1 do operador dot,+1 do nome da funcao,  +2 dos parenteses, +x dos tokens de parametros.
                                    x = x - 1; // -1 do ajuste da malha externo de tokens.
                                    continue;
                                }
                                else
                                {
                                    UtilTokens.WriteAErrorMensage("not found function: " + nomeFuncao + " with parameters list!"+Utils.OneLineTokens(tokens)+" .", tokens, escopo);
                                    return null;
                                }
                            }
                            else
                            {
                                UtilTokens.WriteAErrorMensage("error: static function without parameters interface! " + Utils.OneLineTokens(tokens) + ". ", tokens, escopo);
                                return null;
                            }
                        }
                        else
                        // CHAMADA DE PROPRIEDADE ESTATICA.
                        if ((x + 2 < tokens.Count) && (tokens[x + 1] == ".") && (TABLE.isNomePropriedade(tokens[x + 2])))
                        {
                            
                           
                            ExpressaoPropriedadesAninhadas exprssPropEstatica = new ExpressaoPropriedadesAninhadas(new List<Objeto>() { objSTATIC }, new List<string>() { nameProp });
                            exprssPropEstatica.tipoDaExpressao = objSTATIC.propriedades[0].tipo;
                            exprssRetorno.Add(exprssPropEstatica);

                            
                            
                            x += 1 + 1 + 1; // +1 do nome da classe, +1 do operador dot de POO, +1, +1 do nome da propriedade.
                            x -= 1; // -1 do ajuste da malha externa de tokens.

                            continue;

                        }

                        // se nao encontrar chamadas estaticas ou propriedades estaticas, passa para o proximo token
                        // afim de encontrar mais erros no codigo do programador.
                        continue;

                    } 
                    
                    // CONSTRUCAO DE OBJETO NAO ESTÁTICO, CHAMADAS DE METODO E PROPRIEDADES ANINHADAS.
                    if ((x + 1 < tokens.Count) && (TABLE.isNomeObjeto(tokens[x + 1])))
                    {
                        string nomeClasseDoObjeto = tokens[x];
                        string nomeDoObjeto = tokens[x + 1];

                        string acessor = "private";
                        if (Escopo.nomeClasseCurrente == "")
                        {
                            // se o objeto estiver no escopo global, seta para [public] o acessor, é visivel para todos metodos de classes.
                            acessor = "public";
                        }

                        Objeto objetoNAOestatico = new Objeto(acessor, nomeClasseDoObjeto, nomeDoObjeto, null);
                        Type type = Type.GetType(nomeClasseDoObjeto);
                        if (type != null)
                        {
                            Object obj = Activator.CreateInstance(type);
                            objetoNAOestatico.valor = obj;
                        }


                        escopo.tabela.RegistraObjeto(objetoNAOestatico);
                        exprssRetorno.Add(new ExpressaoObjeto(objetoNAOestatico));
                        x = x + 2 - 1; // +1 do nome do objeto, +1 do nome do oeprador dot de POO, -1 de ajuste da malha de processamento de tokens.
                        continue;
                    }

                }

                // EXPRESSAO NUMERO.
                if (TABLE.isNumero(tokens[x]))
                {
                    ExpressaoNumero exprssNumber = new ExpressaoNumero(tokens[x]);
                    exprssRetorno.Add(exprssNumber);

                }
                else
                // EXPRESSAO LITERAL.
                if (TABLE.isLiteral(tokens[x]))
                {
                    ExpressaoLiteralText exprssLiteral = null;
                    exprssLiteral = new ExpressaoLiteralText(tokens[x]);

                    exprssRetorno.Add(exprssLiteral);
                }
                else
                // EXPRESSAO ENTRE PARENTESES.
                if (tokens[x] == "(")
                {
                    Expressao exprssEntreParenteses = ProcessingExpressionsBetweenParaentesis(tokens, x, escopo);
                    if (exprssEntreParenteses != null)
                    {
                        exprssRetorno.Add(exprssEntreParenteses);
                        x += exprssEntreParenteses.tokens.Count - 1 + 2; // -1 para ajuste da malha de tokens, +2 dos parenteses.
                    }
                    else
                    {
                        UtilTokens.WriteAErrorMensage(
                            "bad format in expression between parentesis: " + Util.UtilString.UneLinhasLista(tokens), tokens, escopo);
                        return null;
                    }
                }
                else
                // EXPRESSAO OBJETO.
                if ((tokens[x] != ";") && (TABLE.isNomeObjeto(tokens[x]) || (escopo.tabela.GetObjeto(tokens[x], escopo) != null)))
                {
                    // processamento aqui de objetos: objeto somente, objetos com propriedades, objetos com metodos, actual objeto, wrapper data objetos.
                    Objeto obj = escopo.tabela.GetObjeto(tokens[x], escopo);
                    typeExpreession tipoExpressaoPrevia = typeExpreession.none;
                    typeExpreession tipoFirstExpressao = typeExpreession.none;

                    ExpressaoPropriedadesAninhadas exprssFIRST_PROPRIEDADE_ANINHADA = new ExpressaoPropriedadesAninhadas();
                    ExpressaoChamadaDeMetodo exprssFIRST_CHAMADA_METODO = null;

                    int countObjetosCaller = 0;
                    Objeto objFirstObject = null;
                    if (obj != null)
                    {
                        // guarda o 1o. objeto caller da expressao, propriedades aninhadas, e/ou chamadas de metodo.
                        objFirstObject = obj.Clone();

                        // EXPRESSAO 1 OBJETO SOMENTE.
                        if (!isNextToken(tokens, ".", x))
                        {
                            // caso de um objeto sem propriedades ou chamadas de metodo.
                            ExpressaoObjeto exprssObj = new ExpressaoObjeto(obj);
                            exprssRetorno.Add(exprssObj);
                            continue;
                        }
                        else
                        // PROCESSAMENTO DE PROPRIEDADES NAO ESTATICAS, E CHAMADAS DE METODO NAO ESTATICAS.
                        if ((x + 2 < tokens.Count) && ((TABLE.isNomePropriedade(tokens[x + 2])) || (TABLE.isNomeFuncao(tokens[x + 2]))))
                        {
                            x += 2; // O token currente ESTÁ NO NOME DA FUNÇÃO OU PROPRIEDADE!

                            // NOME DE FUNCAO.
                            if (TABLE.isNomeFuncao(tokens[x]))
                            {
                                // EXPRESSAO PRIMEIRA É UMA CHAMADA DE METODO.
                                tipoFirstExpressao = typeExpreession.chamadaDeMetodo;
                                tipoExpressaoPrevia = typeExpreession.chamadaDeMetodo;


                            }
                            else
                            // NOME DE PRORRIEDADE. 
                            if (TABLE.isNomePropriedade(tokens[x]))
                            {
                                // EXPRESSAO PRIMEIRA É UMA PROPRIEDADE ANINHADA.
                                tipoFirstExpressao = typeExpreession.propriedadeAninhada;
                                tipoExpressaoPrevia = typeExpreession.propriedadeAninhada;
                            }

                            // tipo da expressao resultante.
                            string tipoDaExpressao = "";

                            while ((x < tokens.Count) && ((TABLE.isNomeFuncao(tokens[x])) || (TABLE.isNomePropriedade(tokens[x]))))
                            {
                                string nomePropriedade = tokens[x];
                                // PROPRIEDADE ANINHADA.
                                if (TABLE.isNomePropriedade(nomePropriedade))
                                {
                                    if (tipoFirstExpressao == typeExpreession.propriedadeAninhada)
                                    {
                                        string nomeClasseCurrente = Escopo.nomeClasseCurrente;
                                        Classe classe = RepositorioDeClassesOO.Instance().GetClasse(nomeClasseCurrente);

                                        // EXPRESSAO ANINHADA APOS UMA EXPRESSAO ANINHADA.
                                        if (tipoExpressaoPrevia == typeExpreession.propriedadeAninhada)
                                        {
                                            if (countObjetosCaller == 0)
                                            {
                                                if (!ValidaAcessorPRIVATEdeUmaPropriedade(obj, nomePropriedade))
                                                {
                                                    UtilTokens.WriteAErrorMensage("property: " + nomePropriedade + "+acessor private not aloweed for an object: !" + Utils.OneLineTokens(tokens) + ". ", tokens, escopo);
                                                    return null;
                                                }

                                                if (!ValidaAcessorPROTECTEDDeUmaPropriedade(obj, nomePropriedade))
                                                {
                                                    UtilTokens.WriteAErrorMensage("property: " + nomePropriedade + "+acessor proteceted not aloweed for an object: !" + Utils.OneLineTokens(tokens) + ". ", tokens, escopo);
                                                    return null;
                                                }

                                                countObjetosCaller++;
                                            }


                                            //********************************************************************************************************************    
                                            // verificação implicita nesta função, para acessor [private].
                                            if (!SetPropertyAninhada(tokens, nomePropriedade, ref obj, exprssFIRST_PROPRIEDADE_ANINHADA, escopo, x))
                                            {
                                                UtilTokens.WriteAErrorMensage("property:  " + nomePropriedade + " properties sequence is invalid!   " + Utils.OneLineTokens(tokens), tokens, escopo);
                                                return null;
                                            }

                                            //********************************************************************************************************************    


                                            //  +1 do nome da propriedade.
                                            x += 1;
                                            if ((x < tokens.Count) && (tokens[x] == "."))
                                            {
                                                // +1 do operador dot, se tiver (se tiver haveara mais propriedades/funcoes aninhados.
                                                x += 1;
                                            }

                                            if ((x < tokens.Count) && ((tokens[x] == "=") || (TABLE.isNomeOperador(tokens[x]))))
                                            {
                                                x -= 1;
                                                break;
                                            }



                                            tipoExpressaoPrevia = typeExpreession.propriedadeAninhada;
                                            tipoDaExpressao = exprssFIRST_PROPRIEDADE_ANINHADA.tipoDaExpressao;

                                            // passa para o proximo item, sem ajustes da malha interna.
                                            continue;
                                        }
                                        else
                                        // EXPRESSAO PROPRIEDADE ANINHADA APOS UMA EXPRESSAO CHAMADA DE METODO 
                                        if (tipoExpressaoPrevia == typeExpreession.chamadaDeMetodo)
                                        {
                                            // cria uma nova expressao propriedade aninhada, e seta como uma sub-expressao
                                            // de chamada de metodo.
                                            ExpressaoPropriedadesAninhadas expressaoPropriedadeAninhada1 = new ExpressaoPropriedadesAninhadas();

                                            if (!SetPropertyAninhada(tokens, nomePropriedade, ref obj, expressaoPropriedadeAninhada1,
                                                escopo, x))
                                            {
                                                UtilTokens.WriteAErrorMensage("property:  " + nomePropriedade + ", sintax error.  " + Utils.OneLineTokens(tokens) + ".", tokens, escopo);
                                                return null;
                                            }

                                            x += 1;  //  +1 do pulo do nome da propriedade.
                                            if ((x < tokens.Count) && (tokens[x] == "."))
                                            {

                                                x += 1; // +1 do pulo do operador dot, se tiver (se tiver haveara mais propriedades/funcoes aninhados.
                                            }
                                            tipoExpressaoPrevia = typeExpreession.chamadaDeMetodo;


                                            exprssFIRST_CHAMADA_METODO.Elementos.Add(expressaoPropriedadeAninhada1);
                                            tipoDaExpressao = expressaoPropriedadeAninhada1.tipoDaExpressao;
                                            continue;
                                        }


                                    }
                                    else
                                    if (tipoFirstExpressao == typeExpreession.chamadaDeMetodo)
                                    {
                                        // PROCESSAMENTO DE  chamada de metodo seguida de propriedade aninhada.
                                        if (objFirstObject.isWrapperObject)
                                        {
                                            string tipoPROP = objFirstObject.tipoElemento;
                                            string nomePROP = nomePropriedade;
                                            Objeto objetoPROP = new Objeto("private", tipoPROP, nomePROP, null);
                                            List<String> lstProp = new List<string>() { tipoPROP };
                                            List<Objeto> lstAninhamento = new List<Objeto>() { objetoPROP };

                                            ExpressaoPropriedadesAninhadas exprssProp = new ExpressaoPropriedadesAninhadas(lstAninhamento, lstProp);
                                            exprssFIRST_CHAMADA_METODO.Elementos.Add(exprssProp);

                                        }
                                        x = x + 1;
                                        continue;
                                    }
                                }
                                else
                                // CHAMADA DE METODO.
                                if ((TABLE.isNomeFuncao(tokens[x])) && (obj != null))
                                {
                                    bool isToIncludeObjectCallerAsFirstParameter = false;
                                    string nomeFuncao = tokens[x];
                                    string classeDaFuncao = obj.tipo;



                                    Metodo fnc = null;
                                    List<string> tokensParametros3 = null;

                                    List<Expressao> parametros = BuildParameters(tokens, escopo, x, ref tokensParametros3);
                                    if (parametros == null)
                                    {
                                        UtilTokens.WriteAErrorMensage("function:" + nomeFuncao + " expression call without paraemeters interface" + Utils.OneLineTokens(tokens), tokens, escopo);
                                        return null;
                                    }

                                    if (parametros != null)
                                    {
                                        // verifica se a classe da funcao é uma classe que permite 1o. argumento o objeto caller.
                                        if ((SystemInit.classeFirstArgumntAsObjectCaller.FindIndex(k => k.Equals(classeDaFuncao)) != -1) && (!obj.isStatic))
                                        {
                                            isToIncludeObjectCallerAsFirstParameter = true;
                                        }


                                        fnc = GetFunctionCompatible(parametros, nomeFuncao, classeDaFuncao, obj, escopo, obj.isStatic, isToIncludeObjectCallerAsFirstParameter);
                                        if (fnc != null)
                                        {
                                            bool isValidAcessor = ValidaACESSOR_METODO(fnc, tokensParametros3, escopo);
                                            if (!isValidAcessor)
                                            {
                                                UtilTokens.WriteAErrorMensage("function: " + nomeFuncao + ", acessor this function is private, but the coding is out of home class!" + Utils.OneLineTokens(tokens) + ". ", tokens, escopo);
                                                return null;
                                            }

                                            if (tipoFirstExpressao == typeExpreession.propriedadeAninhada)
                                            {
                                                // CHAMADA DE METODO APOS UMA EXPRESSAO PROPRIEDADE ANINHADA.
                                                if (!SetUmaChamadaDeMetodoEmFirstExpressaoPropriedadeAninhada(ref obj, exprssFIRST_PROPRIEDADE_ANINHADA, parametros, fnc, tokensParametros3, tokens, escopo))
                                                {
                                                    UtilTokens.WriteAErrorMensage("function:  " + nomeFuncao + "invalid acessor for this method calling!" + Utils.OneLineTokens(tokens) + ". ", tokens, escopo);
                                                    return null;
                                                }

                                                tipoDaExpressao = exprssFIRST_PROPRIEDADE_ANINHADA.tipoDaExpressao;
                                                tipoExpressaoPrevia = typeExpreession.chamadaDeMetodo;

                                                x += 1 + tokensParametros3.Count + 2;  // +1 do nome da funcao,+2 todos tokens parenteses, +x dos tokens parametros.
                                                if ((x < tokens.Count) && (tokens[x] == "."))
                                                {
                                                    x += 1;
                                                }

                                                if ((x < tokens.Count) && (TABLE.isNomeOperador(tokens[x])))
                                                {
                                                    x -= 1;

                                                }
                                                continue;



                                            }
                                            else
                                            if (tipoFirstExpressao == typeExpreession.chamadaDeMetodo)
                                            {
                                                // CHAMADA DE METODO APOS UMA EXPRESSAO CHAMADA DE METODO.
                                                if (!SetChamadaDeMetodoFirstChamadaDeMetodo(ref obj, ref exprssFIRST_CHAMADA_METODO, parametros, fnc, tokensParametros3, tokens, escopo))
                                                {
                                                    UtilTokens.WriteAErrorMensage("acessor for method: " + fnc.nome + " is not valid!   " + Utils.OneLineTokens(tokens), tokens, escopo);
                                                    return null;
                                                }


                                                tipoDaExpressao = fnc.tipoReturn;
                                                tipoExpressaoPrevia = typeExpreession.chamadaDeMetodo;

                                                if ((objFirstObject != null) && (objFirstObject.isWrapperObject))
                                                {
                                                    tipoDaExpressao = objFirstObject.tipoElemento;
                                                }

                                                x += 1 + tokensParametros3.Count + 2;  //+1 do nome da funcao, +2 todos tokens parenteses, +x dos tokens parametros.
                                                if ((x != null) && (x < tokens.Count) && (tokens[x] == "."))
                                                {
                                                    x += 1;
                                                }
                                                if (isToIncludeObjectCallerAsFirstParameter)
                                                {
                                                    x += 1 + 1; //+1 do primeiro parametro (objeto caller),  +1 do token virgula.
                                                }
                                                if ((x < tokens.Count) && (!TABLE.isNomeFuncao(tokens[x])) && (tokens[x] != ";") && (!TABLE.isNomePropriedade(tokens[x])))
                                                {
                                                    x -= 1;
                                                    isToContinue = true;
                                                }

                                                continue;

                                            }



                                        }
                                        else
                                        {
                                            UtilTokens.WriteAErrorMensage("function:" + nomeFuncao + " : compatible not found!   " + Utils.OneLineTokens(tokens), tokens, escopo);
                                        }

                                    }
                                    // passa para o proximo token: propriedade aninhada ou chamada de metodo.
                                    x += 1;
                                }


                            }
                        }

                    }
                    else
                    {
                        UtilTokens.WriteAErrorMensage("object: " + tokens[x] + " not exists!+" + Utils.OneLineTokens(tokens), tokens, escopo);
                        return null;
                    }
                    if ((isToContinue) && (tipoFirstExpressao == typeExpreession.chamadaDeMetodo))
                    {
                        exprssRetorno.Add(exprssFIRST_CHAMADA_METODO);
                        isToContinue = false;
                        continue;
                    }

                    // CASO EM QUE A 1a. EXPRESSAOÉ UMA PROPRIEDADE ANINHADA.
                    if (tipoFirstExpressao == typeExpreession.propriedadeAninhada)
                    {
                        // adiciona a expressao aninhada a expressao de retorno.
                        exprssRetorno.Add(exprssFIRST_PROPRIEDADE_ANINHADA);

                        continue;
                    }

                    // CASO EM QUE A 1a. EXPRESSAO É UMA CHAMADA DE METODO.
                    if (tipoFirstExpressao == typeExpreession.chamadaDeMetodo)
                    {

                        exprssRetorno.Add(exprssFIRST_CHAMADA_METODO);
                        continue;

                    }


                }
                else
                // PROCESSAMENTO DE OBJETOS ESTATICOS COMO PROPRIEDADES ESTATICAS.
                if (Escopo.escopoROOT.tabela.GetObjeto(tokens[x], Escopo.escopoROOT) != null)
                {
                    ExpressaoObjeto exprssObJ = new ExpressaoObjeto(Escopo.escopoROOT.tabela.GetObjeto(tokens[x], Escopo.escopoROOT));
                    exprssRetorno.Add(exprssObJ);
                    continue;
                }
                else
                // PROCESSAMENTO DE OPERADORES.
                if (TABLE.isNomeOperador(tokens[x]))
                {
                    int indexInsercao = exprssRetorno.Count;
                    string nameOperator = tokens[x];
                    DataOperators.tipoComOperandos tipoOperador = GetTypeOperator(nameOperator);
                    DataOperators dadosOperador = new DataOperators(nameOperator, indexInsercao, tipoOperador);

                    this.dataOperadores.Add(dadosOperador);


                    continue;
                }
                else
                if (TABLE.isNomeFuncao(tokens[x]))
                {
                    int indexParams = tokens.IndexOf("(");
                    
                    // processamento das expressoes parame
                    List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(indexParams, "(", ")", tokens);
                    if ((tokensParametros==null) || (tokensParametros.Count < 2))
                    {
                        UtilTokens.WriteAErrorMensage("function: " + tokens[x] + " without parameters interface."+Utils.OneLineTokens(tokens), tokens, escopo);
                        return null;
                    }
                    tokensParametros.RemoveAt(0);
                    tokensParametros.RemoveAt(tokensParametros.Count - 1);
                    List<Expressao> exprssParametros = BuildParameters(tokens, escopo, x, ref tokensParametros);

                    if (exprssParametros == null)
                    {
                        UtilTokens.WriteAErrorMensage("function: " + tokens[x] + ", not valid parameters  " + Utils.OneLineTokens(tokens), tokens, escopo);
                        return null;
                    }
                    string nomeClasse = Escopo.nomeClasseCurrente;
                    string nomeFuncao = tokens[x];
                    Objeto objCaller = new Objeto("private", nomeClasse, "obj_function" + (countNamesObjectsStatic++).ToString(), null);
                    Metodo mt = UtilTokens.FindMethodCompatible(objCaller, nomeClasse, nomeFuncao, nomeClasse, exprssParametros, escopo, false, false);
                    if (mt == null)
                    {
                        UtilTokens.WriteAErrorMensage("function: " + nomeFuncao + ", not found a compatible function" + Utils.OneLineTokens(tokens), tokens, escopo);
                        return null;
                    }
                    escopo.tabela.RegistraObjeto(objCaller);

                    ExpressaoChamadaDeMetodo exprssChamada = new ExpressaoChamadaDeMetodo(objCaller, mt, exprssParametros);
                    exprssRetorno.Add(exprssChamada);
                    x += 2 + tokensParametros.Count + 1 - 1; // +2 dos parenteses dos parametros, +x dos tokens parametros, +1 do nome da funcao, -1 do ajuste da malha.
                    continue;
                }

            }

           

            // PROCESSAMENTO DE OPERADORES.
            this.ProcessingInsertingOperators(escopo, dataOperadores, exprssRetorno, tokens);


            return exprssRetorno;
        }




        /// <summary>
        /// constroi as expressoes parametros, a partir de uma lista de tokens.
        /// </summary>
        /// <param name="tokens">lista de tokens contendo as expressoes.</param>
        /// <param name="escopo">contexto onde as expressoes estão.</param>
        /// <param name="x">indice currente na malha de processamento de tokens.</param>
        /// <param name="tokensParametros">tokens de todas expressoes parametros, retirados desta funcao. podendo ser um parametro vazio ou mesmo nulo</param>
        /// <returns>retorna uma lista de expressoes, que formam os parametros de uma chamada de método.</returns>
        private List<Expressao> BuildParameters(List<string> tokens, Escopo escopo, int x, ref List<string> tokensParametros)
        {
            if ((tokens == null) || (tokens.Count == 0))
            {
                return new List<Expressao>();
            }
            if (x - 1 < 0)
            {
                x = 1;
            }
            int indexParentesesAbre = tokens.IndexOf("(", x - 1);




            // obtem toda interface de parenteses, incluindo os parenteses.
            tokensParametros = UtilTokens.GetCodigoEntreOperadores(indexParentesesAbre, "(", ")", tokens);
            if ((tokensParametros == null) || (tokensParametros.Count < 2))
            {
                UtilTokens.WriteAErrorMensage("bad format for function calling.   "+Utils.OneLineTokens(tokens), tokens, escopo);
                return null;
            }
            tokensParametros.RemoveAt(0);
            tokensParametros.RemoveAt(tokensParametros.Count - 1);

            List<Expressao> parametros = GetListExpressionsParamters(tokens, indexParentesesAbre, escopo);
            return parametros;

        }



     
      

        /// <summary>
        /// faz a validação de acessores de métodos, em relação a classe currente sendo compilada.
        /// </summary>
        /// <param name="fnc">função a validar.</param>
        /// <param name="tokens">lista de tokens onde a expressão que contém o método está.</param>
        /// <param name="escopo">contexto onde os tokens estão.</param>
        /// <returns></returns>
        private static bool ValidaACESSOR_METODO(Metodo fnc, List<string> tokens, Escopo escopo)
        {
            if (fnc.acessor == "public")
            {
                return true;
            }
            if (!ValidaAcessorProtectedDeUmMetodo(fnc))
            {
                UtilTokens.WriteAErrorMensage("acess to protected method: " + fnc.nome + " of: " + fnc.nomeClasse + " is not allowed!" + Utils.OneLineTokens(tokens), tokens, escopo);
                return false;
            }
            if (!ValidaAcessorPrivateDeUmMetodo(fnc))
            {
                UtilTokens.WriteAErrorMensage("acess to private method: " + fnc.nome + " of: " + fnc.nomeClasse + " is not allowed!" + Utils.OneLineTokens(tokens), tokens, escopo);
                return false;
            }


            return true;
        }


        /// <summary>
        /// processamento de expressoes independentes.
        /// </summary>
        /// <param name="tokens">tokens de toddas expressoes independentes.</param>
        /// <param name="escopo">contexto onde as expressão estão.</param>
        /// <returns></returns>
        private List<Expressao> BuildExpressionsIndependenstes(List<string> tokens, Escopo escopo)
        {
            if ((tokens.FindAll(k => k.Equals(";")) != null) && (tokens.FindAll(k => k.Equals(";")).Count > 1))
            {
                List<Expressao> expressoes = new List<Expressao>();
                List<string> tokensUMA_EXPRESSAO = new List<string>();
                if ((tokens != null) && (tokens.Count > 0))
                {
                    for (int i = 0; i < tokens.Count; i++)
                    {
                        tokensUMA_EXPRESSAO.Add(tokens[i]);

                        if (tokens[i] == ";")
                        {
                            // constroi a expressao independente.
                            Expressao exprssIndependente = new Expressao(Utils.OneLineTokens(tokensUMA_EXPRESSAO), escopo);
                            // adiciona a expressa a lista de retorno.
                            expressoes.Add(exprssIndependente);

                            // esvazia a lista de tokens de uma expressao independente.
                            tokensUMA_EXPRESSAO.Clear();
                        }
                    }
                    return expressoes;
                }
            }
            return null;

        }


        /// <summary>
        /// faz o pre-processamento de criação de wrapper data objects.
        /// </summary>
        /// <param name="tokens">tokens da expressao.</param>
        /// <param name="index"></param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        /// <returns>retorna [true] se houve processamento bem sucedido.</returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool preProcessamentoCREATEWrappersObject(List<string> tokens, int index, Escopo escopo)
        {
            // é razoável supor que a expressão currente é toda uma expressao para criar um wrapper object.
            string expression_in_one_line = Utils.OneLineTokens(tokens);

            if (ExpressaoPorClassificacao.wrappersDATA == null)
            {
                wrappersDATA = WrapperData.Get_ALL_WrapperData();
            }
            for (int i = 0; i < wrappersDATA.Count; i++)
            {
                List<string> tokensProcessed = new List<string>();
                List<string> tokensCreate = wrappersDATA[i].CREATE(ref expression_in_one_line, escopo, ref tokensProcessed);

                if ((tokensCreate != null) && (tokensCreate.Count > 0))
                {
                    // remove os tokens utilizados no processo de instanciacao do wrapper object (função create).
                    tokens.RemoveRange(0, tokensProcessed.Count);
                    // insere os tokens gerados na instanciacao.
                    tokens.InsertRange(0, tokensCreate);
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// FAZ PROCESSAMENTO DE ANOTAÇÕES WRAPPER GET e SET, INSERINDO OS TOKENS DA CHAMADA DE METODO RESULTANTE
        /// PARA DENTRO DA LISTA DE TOKENS.
        /// </summary>
        /// <param name="tokens">lista de tokens da expressao principal.</param>
        /// <param name="index">indice currente da malha de tokens principal.</param>
        /// <param name="escopo">contexto em que a expressao principal está.</param>
        /// <returns>[true] se houve processamento bem sucedido, [false] se houve falham ou não houve processamento</returns>
        private bool preProcessamentoGET_SET_WrappersObject(ref List<string> tokensRAW, int index, Escopo escopo)
        {

           

            List<string> tokens = tokensRAW.ToList<string>();


            if ((tokens == null) || (tokens.Count == 0))
            {
                return false;
            }
            int qtdobjectsWrapper = 0;
            int countObjectsWrapper = 0;
            List<string> nomesWrapperObjectsPresentes = new List<string>();
            for (int j = 0; j < tokens.Count; j++)
            {
                Objeto objetoWrapper = escopo.tabela.GetObjeto(tokens[j], escopo);
                if ((objetoWrapper != null) && (objetoWrapper.isWrapperObject))
                {
                    nomesWrapperObjectsPresentes.Add(objetoWrapper.nome);
                    qtdobjectsWrapper++;
                }
            }

          
            int i = 0;

        

            
            bool isFoundSETElement = false;
            
            while ((i >= 0) && (i < tokens.Count) && (countObjectsWrapper < qtdobjectsWrapper))  
            {


             

                Objeto umObjeto = escopo.tabela.GetObjeto(tokens[i], escopo);



                if ((umObjeto != null) && ((umObjeto.isWrapperObject)))
                {
                    

                    if (!umObjeto.isWrapperObject)
                    {
                        continue;
                    }
                    // obtem o gerenciador wrapper data para o objeto wrapper.
                    WrapperData wrapperData = WrapperData.GetWrapperData(umObjeto);
                    int indexWrapperObject = i;

                    // PROCESSAMENTO DE SET.
                    if ((!isFoundSETElement) && (tokens.IndexOf("=") != -1) && (tokens.IndexOf("=") > i)) 
                    {


                     
                        int indexBegin = tokens.IndexOf(umObjeto.nome);

                        List<string> tokensProcessed = new List<string>();
                        List<string> tokensSET = wrapperData.SETChamadaDeMetodo(ref tokens, escopo, ref tokensProcessed, indexBegin);

                        if (tokensSET != null)
                        {
                            tokens.RemoveRange(0, tokensProcessed.Count);
                            tokens.InsertRange(0, tokensSET);
                            i += tokensSET.Count;

                            int indexMore = -1;
                            // verifica se há mais wrapper objects nos tokens da expressao.
                            if (this.isMoreWrapperObjects(tokens, indexBegin + 1, escopo, ref indexMore)) 
                            {
                                // se tiver, volta para a posicao do proximo wrapper object.
                                i = indexMore;
                                // seta o indice de um wrapper object encontrado.
                                indexWrapperObject = indexMore;
                                isFoundSETElement = true;

                                
                                int indexSignalEquals = tokens.IndexOf("=");
                                if (indexSignalEquals >= 0)
                                {
                                    tokens= tokens.GetRange(0, indexSignalEquals);
                                }
                                if (this.isMoreWrapperObjects(tokens, indexWrapperObject + 1, escopo, ref indexMore))
                                {
                                    i = indexMore;
                                    indexWrapperObject = indexMore;
                                    isFoundSETElement = true;
                                }

                                continue;
                            }
                            

                            countObjectsWrapper++;
                            if (countObjectsWrapper == qtdobjectsWrapper)
                            {
                                tokensRAW = tokens.ToList<string>();
                                
                                return true;
                            }
                            else
                            {
                                
                                if (i < 0) 
                                {
                                    tokensRAW = tokens.ToList<string>();
                                    return true;
                                }


                                i -= 1;
                                
                             
                            }
                        }
                    }
                    else
                    // PROCESSAMENTO DE GET, COM/SEM OPERADOR IGUAL.
                    if ((tokens.IndexOf("=") == -1) ||
                        ((tokens.IndexOf("=") != -1) && (tokens.IndexOf("=") < i)) ||
                        ((tokens.IndexOf("=") != -1) && (tokens.IndexOf(".") != -1)))
                    {
                 


                        List<string> tokensProcessed = new List<string>();
                        List<string> tokensGET = wrapperData.GETChamadaDeMetodo(
                                                        ref tokens, escopo, ref tokensProcessed, i);

                        if (tokensGET != null)
                        {
                            if ((i + 1 < tokens.Count) && (tokens[i + 1] != "["))
                            {
                                i = tokens.IndexOf("[", i + 1) - 1;
                                if (i <= -1)
                                {
                                    i = 0;
                                }
                            } 

                            tokens.RemoveRange(i, tokensProcessed.Count);
                            tokens.InsertRange(i, tokensGET);
                            i += tokensGET.Count;  // -1 do ajuste de malha interno.

                            countObjectsWrapper++;
                            if (countObjectsWrapper == qtdobjectsWrapper)
                            {
                                tokensRAW = tokens.ToList<string>();

                                return true;
                            }
                            else
                            {
                                
                                if (i < 0)
                                {
                                    tokensRAW = tokens.ToList<string>();
                                    return true;
                                }
                                else
                                {

                                    i -= 1;
                                    
                                }
                            }
                        }


                    }

                }

                i++;
            }
            if (tokens != null)
            {
                tokensRAW = tokens.ToList<string>();
            }
            
            return true;
        }

        /// <summary>
        /// funca para verificar se há mais wrappers object.
        /// </summary>
        /// <param name="tokens">tokens da expressao.</param>
        /// <param name="indexBegin">indice de comeco da busca.</param>
        /// <param name="escopo">contexto de onde a expressao está.</param>
        /// <param name="indexNextWrapperObject">indice do tokens do proximo wrapper object.</param>
        /// <returns></returns>
        private bool isMoreWrapperObjects(List<string> tokens, int indexBegin,Escopo escopo, ref int indexNextWrapperObject)
        {
            if ((tokens == null) || (tokens.Count == 0) || (indexBegin >= tokens.Count - 1))
            {
                return false;
            }
            else
            {
                for (int i=indexBegin;i<tokens.Count;i++)
                {
                    if ((escopo.tabela.GetObjeto(tokens[i], escopo) != null) && (escopo.tabela.GetObjeto(tokens[i], escopo).isWrapperObject))
                    {
                        indexNextWrapperObject = i;
                        return true;
                    }
                
                }
            }
            return false;
        }
        /// <summary>
        /// /adiciona a expressao chamada de metodo construida, como sub-expressao de outra expressao chamada de metodo.
        /// </summary>
        /// <param name="obj">objeto caller que chama o metodo.</param>
        /// <param name="exprssFirstPropriedadesAninhadas">expressao chamada de metodo 1o, dentro das sub-expressoes.</param>
        /// <param name="parametros">lista de expressoes parâmetros.</param>
        /// <param name="fnc">metodo da chamada de metodo a adicionar.</param>
        /// <param name="tokensDosParametros">lista de tokens de todas expressões parâmetros.</param>
        /// <param name="tokens">lista de tokens da expressão principal, todo codigo da expressão a construir.</param>
        /// <param name="escopo">contexto onde a expressao chamada de metodo está.</param>
        /// <returns>retorna [true] se o processamento resultou nenhum erro.</returns>
        private static bool SetUmaChamadaDeMetodoEmFirstExpressaoPropriedadeAninhada(ref Objeto obj,
            ExpressaoPropriedadesAninhadas exprssFirstPropriedadesAninhadas, List<Expressao> parametros, Metodo fnc, List<string> tokensDosParametros, List<string> tokens, Escopo escopo)
        {
            
            bool isValidAcessor = ValidaACESSOR_METODO(fnc, tokens, escopo);
            if (!isValidAcessor)
            {
                return false;
            }
            ExpressaoChamadaDeMetodo exprssChamada = new ExpressaoChamadaDeMetodo(obj, fnc, parametros);

            // caso em que o objeto caller for um wrapper object.
            if (obj.isWrapperObject)
            {
                exprssChamada.tipoDaExpressao = obj.tipoElemento;
            }
            exprssFirstPropriedadesAninhadas.Elementos.Add(exprssChamada);

            // constroi um objeto caller para a proxima expressao.
            obj = new Objeto("private", exprssChamada.tipoDaExpressao, "obj" + ExpressaoPorClassificacao.countNamesObjectsStatic++, null);


            // seta o tipo da expressao da expressão primeiro.
            exprssFirstPropriedadesAninhadas.tipoDaExpressao = exprssChamada.tipoDaExpressao;

            return true;
        }





        /// <summary>
        /// adiciona uma expressao chamada de metodo, como sub-expressao da expressao chamada de metodo 1a.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="exprssFirstIsChamadaDeMetodo"></param>
        /// <param name="parametros"></param>
        /// <param name="fnc"></param>
        /// <param name="tokensDeTodosParametros"></param>
        /// <param name="tokens"></param>
        /// <param name="escopo"></param>
        /// <returns></returns>
        private bool SetChamadaDeMetodoFirstChamadaDeMetodo(ref Objeto obj, ref ExpressaoChamadaDeMetodo exprssFirstIsChamadaDeMetodo, List<Expressao> parametros, Metodo fnc,
            List<string> tokensDeTodosParametros, List<string> tokens, Escopo escopo)
        {

            bool isValidAcessor = ValidaACESSOR_METODO(fnc, tokens, escopo);
            if (!isValidAcessor)
            {
                UtilTokens.WriteAErrorMensage("acessor invalid!   " + Utils.OneLineTokens(tokens), tokens, escopo);
                return false;
            }

            ExpressaoChamadaDeMetodo exprssChamada = new ExpressaoChamadaDeMetodo(obj, fnc, parametros);
            // caso do objeto caller for um wrapper object.
            if (obj.isWrapperObject)
            {
                exprssChamada.tipoDaExpressao = obj.tipoElemento;
            }

            // caso em que as funcoes de wrapper object não sejam da anotação wrapper envolvido.
            if ((obj.isWrapperObject) && (fnc.nome != "GetElement") && (fnc.nome != "SetElement") && (fnc.nome != "Create"))
            {
                exprssChamada.tipoDaExpressao = fnc.tipoReturn;
            }


            if (exprssFirstIsChamadaDeMetodo == null)
            {
                exprssFirstIsChamadaDeMetodo = new ExpressaoChamadaDeMetodo(exprssChamada.objectCaller, exprssChamada.funcao, exprssChamada.parametros);
                exprssFirstIsChamadaDeMetodo.tipoDaExpressao = exprssChamada.tipoDaExpressao;
            }
            else
            {
                // atualiza o tipo da expressao: se obj.prop.metodo(), então o tipo da expressao é o tipo de retorno do metodo, ou seja seu tipo de expressao.
                // em uma expressao chamada de metodo, já vem calculado o tipo da expressão, como sendo o tipo de retorno do metodo da chamada.
                exprssFirstIsChamadaDeMetodo.tipoDaExpressao = exprssChamada.tipoDaExpressao;
                exprssFirstIsChamadaDeMetodo.Elementos.Add(exprssChamada);

            }


            // obtem o tipo do proximo objeto caller..
            string nomeClasseDoObjetoCaller = obj.tipo;
            if ((fnc != null) && (!obj.isWrapperObject))
            {
                nomeClasseDoObjetoCaller = fnc.tipoReturn;
            }
            else
            if ((fnc!=null) && (obj.isWrapperObject))
            {
                nomeClasseDoObjetoCaller = obj.tipoElemento;
            }
            


            // constroi um objeto caller para A PROXIMA EXPRESSAO. o objeto não é registrado, porque é uma extensão de metodo!
            obj = new Objeto("private", nomeClasseDoObjetoCaller, "obj" + ExpressaoPorClassificacao.countNamesObjectsStatic++, null);


            
            return true;
        }



        /// <summary>
        /// constroi a expressao de propriedade aninhadas.
        /// </summary>
        /// <param name="tokens">lista de tokens totais da expressao.</param>
        /// <param name="nomePropriedade"></param>
        /// <param name="obj">objeto caller que faz a chamada a propriedades aninhadas.</param>
        /// <param name="exprssProp">expressao propriedades aninhadas a construir, mas ja instanciada.</param>
        /// <param name="escopo">contexto onde a expressao está,</param>
        /// <param name="indexNextPropriedade"></param>
        /// <returns></returns>
        private bool SetPropertyAninhada(List<string> tokens, string nomePropriedade, ref Objeto obj, ExpressaoPropriedadesAninhadas exprssProp, Escopo escopo,int indexNextPropriedade)
        {
          

            string tipo = obj.tipo;

         
           // constroi um objeto para a proxmima propriedade aninhada.
            Objeto objProp = GetNextPropriedade(tokens, tipo, indexNextPropriedade, TABLE);
            bool isWrapperObject = false;
            if (objProp == null)
            {

                return false;
            }


           

            Objeto propriedade = obj.propriedades.Find(k => k.nome == nomePropriedade);
            if ((propriedade == null) && (!isWrapperObject))
            {
                UtilTokens.WriteAErrorMensage("property: " + nomePropriedade + " of class: " + tipo + " not found!" + Utils.OneLineTokens(tokens), tokens, escopo);
                return false;
            }




            if (objProp != null)
            {
                if (exprssProp.aninhamento == null)
                {
                    exprssProp.aninhamento = new List<Objeto>();
                }
                exprssProp.tokens = new List<string>() { nomePropriedade };
                // seta o aninhamento de objetos da propriedade aninhada.
                exprssProp.aninhamento.Add(objProp);
                // seta o tipo da expressao, porque a propriedade aninhada do codigo foi feita sem parametros!
                exprssProp.tipoDaExpressao = objProp.tipo;

                // seta o objeto caller.
                exprssProp.objectCaller = obj.Clone();



            }
            else
            {
                UtilTokens.WriteAErrorMensage("error in properties in: " +Utils.OneLineTokens(tokens), tokens, escopo);
                return false;
            }
            obj = objProp.Clone();
            obj.classePertence = propriedade.classePertence;
            

            return true;

        }



        /// <summary>
        /// obtem um metodo compativel com a lista de expressoes parametro de uma chamada de funcao/metodo.
        /// </summary>
        /// <param name="parametros">lista de expressoes parametro da chamada de metodo.</param>
        /// <param name="nomeFuncao">nome do metodo/funcao.</param>
        /// <param name="nameClasseDaFuncao">nome da classe da funcao.</param>
        /// <param name="objCaller">objeto que faz a chamada de metodo.</param>
        /// <param name="escopo">contexto onde a expressao chamada de metodo está.</param>
        /// <param name="isStatic">[true] se é uma chamada de metodo estatica.</param>
        /// <param name="isToIncludeObjectCallerAsParameter">[true] se é para incluir o objeto caller como 1o. parametro.</param>
        /// <returns>retorna um metodo compativel, null se não.</returns>
        private Metodo GetFunctionCompatible(List<Expressao> parametros, string nomeFuncao, string nameClasseDaFuncao, Objeto objCaller,
            Escopo escopo, bool isStatic, bool isToIncludeObjectCallerAsParameter)
        {
            return UtilTokens.FindMethodCompatible(objCaller, objCaller.GetTipo(), nomeFuncao, nameClasseDaFuncao, parametros, escopo, isStatic, isToIncludeObjectCallerAsParameter);

        }



        /// <summary>
        /// verifica se o proximo token na lista de tokens, é o token esperado do parametro.
        /// </summary>
        /// <param name="tokens">lista de tokens da expressao,</param>
        /// <param name="tokenEspected">token esperado.</param>
        /// <param name="indexTokenCurrent">indice do token currente na malha de tokens.</param>
        /// <returns>retorna [true] se o proximo token é o token esperado.</returns>
        private bool isNextToken(List<string> tokens, string tokenEspected, int indexTokenCurrent)
        {
            if ((indexTokenCurrent + 1) < tokens.Count)
            {
                return (tokens[indexTokenCurrent + 1] == tokenEspected);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// obtem a lista de expressos parametros de uma chamada de funcao/metodo.
        ///
        /// </summary>
        /// <param name="tokens">lista de tokens da expressao.</param>
        /// <param name="indexParentesOpen">indice do parenteses abre, inicio do parametro.</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        /// <returns>retorna uma lista de expressoes, ou null.</returns>
        private List<Expressao> GetListExpressionsParamters(List<string> tokens, int indexParentesOpen, Escopo escopo)
        {

            List<Expressao> expressoesParametros = new List<Expressao>();
            List<List<string>> parametrosTokens = ExtraiParametrosDeExpressoesChamadaDeMetodo(tokens, escopo, ref indexParentesOpen);
            if ((parametrosTokens != null) && parametrosTokens.Count > 0)
            {
                for (int i = 0; i < parametrosTokens.Count; i++)
                {
                    Expressao umaExpressaoParametro = new Expressao(parametrosTokens[i].ToArray(), escopo);
                    if (umaExpressaoParametro != null)
                    {
                        /// EXPRESSAO CONTAINER.
                        Expressao exprssContainer = new Expressao();
                        exprssContainer.Elementos.AddRange(umaExpressaoParametro.Elementos);
                        // seta o tipo da expressao:  todas sub-expressoes são do mesmo tipo. poderia ser do mesmo tipo herdado.
                        exprssContainer.tipoDaExpressao = umaExpressaoParametro.Elementos[0].tipoDaExpressao;
                        exprssContainer.tokens.AddRange(umaExpressaoParametro.tokens);


                        expressoesParametros.Add(exprssContainer);
                    }
                    else
                    {
                        UtilTokens.WriteAErrorMensage("sintax error in parameter:  " + parametrosTokens[i] + "   " + Utils.OneLineTokens(tokens), tokens, escopo);
                        return null;
                    }
                }
            }
            return expressoesParametros;


        }





        /// <summary>
        /// extrai listas de parametros, de uma chamada de metodo.
        /// </summary>
        /// <param name="tokensExpressao">tokens total da expressao chamada de metodo.</param>
        /// <param name="escopo">contexto onde a expressao esta.</param>
        /// <param name="offsetChamadaDeExpessao">contador de tokens de n. chamadas de metodo, se p.ex. for metodoA(1,1)+metodoB(1,5), ha previsao para distinguir a 2a. 
        /// chamda da 1a. chamada.</param>
        /// <returns>retorna uma lista contendo cada parametro extraido, ou null se algo der errado.</returns>
        private List<List<string>> ExtraiParametrosDeExpressoesChamadaDeMetodo(List<string> tokensExpressao, Escopo escopo, ref int offsetChamadaDeExpessao)
        {

            // obtem o indice do operador parenteses "(";
            int indexBegin = tokensExpressao.IndexOf("(", offsetChamadaDeExpessao);
            if (indexBegin == -1)
            {
                UtilTokens.WriteAErrorMensage("parameters interface in bad sintax, ausent the parentesis.  " + Utils.OneLineTokens(tokensExpressao), tokensExpressao, escopo);
                return null;
            }

            // obtem a lista de tokens de parametros de funcao.
            List<string> tokens = UtilTokens.GetCodigoEntreOperadores(indexBegin, "(", ")", tokensExpressao);
            if ((tokens == null) || (tokens.Count < 2))
            {

                UtilTokens.WriteAErrorMensage("parameters interface in bad sintax, ausent the parentesis.  " + Utils.OneLineTokens(tokens), tokens, escopo);
                return null;
            }

            // remove os tokens de parenteses que formam a interface de parametros.
            tokens.RemoveAt(0);
            tokens.RemoveAt(tokens.Count - 1);

            List<List<string>> parametros = new List<List<string>>();
            List<string> umParametro = new List<string>();


            int pilhaParenteses = 0;
            int pilhaColchetetes = 0;
            int pilhaBracas = 0;
            if (tokens != null)
            {
                // malha de procesamento de tokens.
                for (int i = 0; i < tokens.Count; i++)
                {
                    string tokenCurr = tokens[i].ToString();
                    if ((tokenCurr == ",") && (pilhaParenteses == 0) && (pilhaColchetetes == 0) && (pilhaBracas == 0))
                    {
                        // adiciona o parametro formado, na lista de parametros.
                        parametros.Add(umParametro.ToList<string>());
                        umParametro.Clear();
                        // pula o token virgula de separacao entre parametros.
                        continue;
                    }
                    if (tokenCurr == "(")
                    {
                        pilhaParenteses++;
                    }
                    else
                    if (tokenCurr == ")")
                    {
                        pilhaParenteses--;
                    }
                    else
                    if (tokenCurr == "[")
                    {
                        pilhaColchetetes++;
                    }
                    else
                    if (tokenCurr == "]")
                    {
                        pilhaColchetetes--;
                    }
                    else
                    if (tokenCurr == "{")
                    {
                        pilhaBracas++;
                    }
                    else
                    if (tokenCurr == "}")
                    {
                        pilhaBracas--;
                    }

                    umParametro.Add(tokenCurr);

                    offsetChamadaDeExpessao++;
                }
            }
            if (umParametro.Count > 0)
            {
                parametros.Add(umParametro);
            }

            return parametros;


        }




        /// <summary>
        /// verifica se o proximo token na lista de tokens é um token de propriedade.
        /// </summary>
        /// <param name="tokens">lista de tokens da expressao.</param>
        /// <param name="classeObjeto">classe do objeto</param>
        /// <param name="classeDaPropriedade">classe a que a propriedade é.</param>
        /// <param name="indexTokenCurrent">indice do token currente, na malha de tokens.</param>
        /// <param name="table">tabela hash de elementos-token extraidos.</param>
        /// 
        /// <returns></returns>
        private Objeto GetNextPropriedade(List<string> tokens, string classeObjeto, int indexTokenCurrent, Classificador.TabelaHash table)
        {
            int indexClass = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == classeObjeto);
            if (indexClass == -1)
            {
                return null;
            }
            HeaderClass headerClasse = Expressao.headers.cabecalhoDeClasses[indexClass];
            int indexProp1 = headerClasse.properties.FindIndex(k => k.name == tokens[indexTokenCurrent]);
            if (indexProp1 == -1)
            {
                return null;
            }
            else
            if (WrapperData.isWrapperData(headerClasse.properties[indexProp1].typeOfProperty) != null)
            {   
                {
                    HeaderProperty propWrappeer= headerClasse.properties[indexProp1];
                    classeObjeto= headerClasse.properties[indexProp1].typeOfProperty;

                    if (classeObjeto == "Vector")
                    {
                        Vector vt = new Vector();
                        vt.nome = propWrappeer.name;
                        vt.tipoElemento= propWrappeer.tipoElemento;
                        vt.tipo = "Vector";

                        return vt;
                    }
                    else
                    if (classeObjeto == "Matriz")
                    {
                        Matriz mt = new Matriz();
                        mt.nome = propWrappeer.name;
                        mt.tipoElemento = propWrappeer.tipoElemento;
                        mt.tipo = "Matriz";

                        return mt;
                    }
                    else
                    if (classeObjeto=="JaggedArray")
                    {
                        JaggedArray jt= new JaggedArray();
                        jt.nome = propWrappeer.name;
                        jt.tipoElemento=propWrappeer.tipoElemento;
                        jt.tipo = "JaggedArray";

                        return jt;
                    }
                    else
                    if (classeObjeto == "DictionaryText")
                    {
                        DictionaryText dt = new DictionaryText();
                        dt.nome = propWrappeer.name;
                        dt.tipoElemento = propWrappeer.tipoElemento;
                        dt.tipo = "DictionaryText";

                        return dt;
                    }
                }
                
            }
            List<ElementToken> elementos = table.ReadToken(tokens[indexTokenCurrent]);
            if (elementos != null)
            {
                for (int i = 0; i < elementos.Count; i++)
                {
                    string nomeClasse = elementos[i].nomeClasse;
                    string nomeProp = elementos[i].nomeToken;
                    

                    List<ElementToken> tokensTodasPropriedadesClasseObjetoCaller = table.ReadToken(classeObjeto);
                    ElementToken elementProperty = elementos.Find(k => k.nomeClasse == nomeClasse && k.nomeToken == nomeProp && k.tipoClassificacao == ElementToken.typeToken.nameProperty);
                    if (elementProperty != null)
                    {

                        string acessorProp = "private";
                        if (indexClass != -1)
                        {
                            int indexProp = Expressao.headers.cabecalhoDeClasses[indexClass].properties.FindIndex(k => k.name == nomeProp);
                            if (indexProp != -1)
                            {
                                acessorProp = Expressao.headers.cabecalhoDeClasses[indexClass].properties[indexProp].acessor;

                            }
                        }

                        return new Objeto(acessorProp, elementProperty.nomeTipo, nomeProp, null);
                        
                        
                    }

                }
                return null;
            }
            else
            {
                return null;
            }


        }

        /// <summary>
        /// obtem o acessor da propriedade: public, private, ou protected.
        /// busca pelo [Headers], do nome da classe, e o nome da propriedade.
        /// </summary>
        /// <param name="nomeProp">nome da propriedade.</param>
        /// <param name="nomeClasseProp">nome da classe que a propriedade pertence.</param>
        /// <returns>retorna o acessor da propriedade.</returns>
        private string GetAcessorProperty(string nomeProp, string nomeClasseProp)
        {
            int indexClass = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == nomeClasseProp);
            string acessorProp = "private";
            if (indexClass != -1)
            {
                int indexProp = Expressao.headers.cabecalhoDeClasses[indexClass].properties.FindIndex(k => k.name == nomeProp);
                if (indexProp != -1)
                {
                    acessorProp = Expressao.headers.cabecalhoDeClasses[indexClass].properties[indexProp].acessor;

                   
                }
            }
            return acessorProp;
        }



        

        /// <summary>
        /// valida um metodo com acessor private.
        /// </summary>
        /// <param name="metodo">metodo a validar o acessor.</param>
        /// <returns>retorna [true] se o acessor é [private] e a classe onde a chamada do metodo está for a classe currente.</returns>
        private static bool ValidaAcessorPrivateDeUmMetodo(Metodo metodo)
        {
            if (metodo.acessor != "private")
            {
                return true;

            }
            else
            {
                return ((metodo.acessor == "private") && (metodo.nomeClasse == Escopo.nomeClasseCurrente));
            }
        }


        /// <summary>
        /// valida um metodo proteceted, acessado de um objeto caller.
        /// </summary>
        /// <param name="metodo">funcao protected.</param>
        /// <returns>[false] se o acessor do metodo não for [protected]; [false] se o escopo for global; [true] se a chamada do metodo parametro for dentro de uma classe que herda
        /// a classe do metodo.[false] se não tiver nenhuma classe que herda o metodo parametro.</returns>
        private static bool ValidaAcessorProtectedDeUmMetodo(Metodo metodo)
        {
            string acessorMetodo = metodo.acessor;
            if (acessorMetodo == "protected")
            {
                string nomeClasseCurrente = Escopo.nomeClasseCurrente;
                List<string> classesQueHerdamMetodoParametro = GetClassesHerdeiras(metodo.nomeClasse);
                if ((classesQueHerdamMetodoParametro == null) || (classesQueHerdamMetodoParametro.Count == 0))
                {
                    return true;
                }
                return classesQueHerdamMetodoParametro.FindIndex(k => k.Equals(metodo.nomeClasse)) != -1;
                
            }
            return true;
        }



        /// <summary>
        /// valida um acessor private de uma propriedade.
        /// </summary>
        /// <param name="objCaller">objeto que contem a propriedade.</param>
        /// <param name="nomeProp">nome da propriedade.</param>
        /// <returns></returns>
        private static bool ValidaAcessorPRIVATEdeUmaPropriedade(Objeto objCaller, string nomeProp)
        {
            if (Escopo.nomeClasseCurrente == "")
            {
                return true;
            }
            int index = objCaller.propriedades.FindIndex(k => k.nome == nomeProp);
            if (index == -1)
            {
                return true;
            }
            if ((objCaller.propriedades[index].acessor == "public") || (objCaller.propriedades[index].acessor == "protected"))
            {
                return true;
            }

            if ((objCaller.propriedades[index].acessor == "private") && (objCaller.propriedades[index].classePertence == objCaller.classePertence))
            {
                return true;
            }

            return false;
        }


        private static bool ValidaAcessorPROTECTEDDeUmaPropriedade(Objeto objetoCaller, string nomeProp)
        {
            int index = objetoCaller.propriedades.FindIndex(k => k.nome == nomeProp);
            if (index == -1)
            {
                return true;
            }

            if (objetoCaller.classePertence == "")
            {
                return true;
            }
            List<string> classesQueHerdam = GetClassesHerdeiras(objetoCaller.propriedades[index].classePertence);
            if ((classesQueHerdam != null) && (classesQueHerdam.Count > 0) && (classesQueHerdam.FindIndex(k => k.Equals(objetoCaller.propriedades[index].classePertence)) != -1)) 
            {
                return true;
            }

            if (objetoCaller.classePertence == objetoCaller.propriedades[index].classePertence)
            {
                return true;
            }

            return false;
        }



        /// <summary>
        /// retorna uma lista de classes que herdam o item parametro (propriedade, metodo).
        /// </summary>
        /// <param name="nomeClasseComItemProtected">nome de classe com o item (propriedade, metodo).</param>
        /// <returns></returns>
        private static List<string> GetClassesHerdeiras(string nomeClasseComItemProtected)
        {
            HeaderClass classITEM_protected = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == nomeClasseComItemProtected);
            List<HeaderClass> classeQueHERDAMClasseItem = Expressao.headers.cabecalhoDeClasses;
            List<string> classesQUEHerdam = new List<string>();
            if ((classeQueHERDAMClasseItem != null) && (classeQueHERDAMClasseItem.Count > 0))
            {
                
                for (int i = 0;i<classeQueHERDAMClasseItem.Count;i++)
                {
                    if ((classeQueHERDAMClasseItem[i].heranca!=null) && (classeQueHERDAMClasseItem[i].heranca.Count>0))
                    {
                        for (int j = 0; j < classeQueHERDAMClasseItem[i].heranca.Count; j++)
                        {
                            if (classeQueHERDAMClasseItem[i].heranca[j].Equals(nomeClasseComItemProtected))
                            {
                                classesQUEHerdam.Add(classeQueHERDAMClasseItem[i].heranca[j]);
                            }
                        }
                    }
                }
            }
            return classesQUEHerdam;

        }

        /// <summary>
        /// extrai dos headers, todos nome de operadores das classes presente no codigo.
        /// </summary>
        /// <param name="opBinary">lista de operadores binarios.</param>
        /// <param name="opUnary">lista de operadores unarios.</param>
        /// <param name="opBinaryAndToUnary">lista de operadores que sao unarios e binarios.</param>
        public static void GetNomesOperadoresBinariosEUnarios(ref List<string> opBinary, ref List<string> opUnary, ref List<string> opBinaryAndToUnary)
        {
            if ((opBinary != null) && (opBinary.Count > 0))
            {
                return;
            }
            else
            {
                opBinary = new List<string>();
                opUnary = new List<string>();


                // encontra todos nomes de operadores, registrados nas classes base e classe do codigo.
                List<HeaderClass> headers = Expressao.headers.cabecalhoDeClasses;




                for (int x = 0; x < headers.Count; x++)
                {
                    List<HeaderOperator> todosOperadoresDeUmaClasse = headers[x].operators;
                    if (todosOperadoresDeUmaClasse != null)
                    {
                        for (int i = 0; i < todosOperadoresDeUmaClasse.Count; i++)
                        {
                            // INCLUSAO DE OPERADORES CONDICIONAIS CONECTIVOS UNIVERSAIS.
                            opBinary.Add("&&");
                            opBinary.Add("||");

                            // operadores EXCLUSIVAMENTE BINARIOS.
                            if (todosOperadoresDeUmaClasse[i].tipoDoOperador == HeaderOperator.typeOperator.binary)
                            {
                                if (opBinary.IndexOf(todosOperadoresDeUmaClasse[i].name) == -1)
                                {
                                    opBinary.Add(todosOperadoresDeUmaClasse[i].name);
                                }

                            }
                            // operadores EXCLUSIVAMENTE UNARIOS.
                            if (((todosOperadoresDeUmaClasse[i].tipoDoOperador == HeaderOperator.typeOperator.unary_pos) ||
                                (todosOperadoresDeUmaClasse[i].tipoDoOperador == HeaderOperator.typeOperator.unary_pre)))
                            {
                                if (opUnary.IndexOf(todosOperadoresDeUmaClasse[i].name) == -1)
                                {
                                    opUnary.Add(todosOperadoresDeUmaClasse[i].name);
                                }

                            }
                        }


                    }
                }

                // OPERADORES BINARIOS E UNARIOS AO MESMO TEMPO
                if ((opBinary != null) && (opUnary != null))
                {
                    for (int i = 0; i < opBinary.Count; i++)
                    {
                        string operador = opBinary[i];
                        int indexOperadorUnario = opUnary.FindIndex(k => k == operador);
                        if ((opUnary.FindIndex(k => k == operador) != -1) && (opBinaryAndToUnary.FindIndex(k => k.Equals(operador)) == -1))
                        {
                            opBinaryAndToUnary.Add(opBinary[i]);
                            for (int op = 0; op < opBinary.Count; op++)
                            {
                                if (opBinary[op] == operador)
                                {
                                    opBinary.RemoveAt(op);
                                    op--;
                                }
                            }
                            for (int op = 0; op < opUnary.Count; op++)
                            {
                                if (opUnary[op] == operador)
                                {
                                    opUnary.RemoveAt(op);
                                    op--;
                                }

                            }
                            i--;
                        }
                    }
                }

                // remove o operador "=", da lista de operadores, pois é um operador de instanciação,
                // não operador matemático, condicional ou booleano.
                opBinary.Remove("=");
                opUnary.Remove("=");

            }
        }


        /// <summary>
        /// classe para processamento de operadores em uma expressao.
        /// </summary>
        public class DataOperators
        {
            public string nameOperator;
            public int indexInListOfExpressions;

            public enum tipoComOperandos { binary, unary, binaryAndUnary, none };
            public tipoComOperandos tipo;

            /// <summary>
            /// construtor.
            /// </summary>
            /// <param name="nameOperator">nome do operador.</param>
            /// <param name="indexInListOfExpressions">indice do token, com relacao a lista de expressoes extraidas.</param>
            /// <param name="typeOperator">tipo do operador: binario, unario binarioEUnario.</param>
            public DataOperators(string nameOperator, int indexInListOfExpressions, tipoComOperandos typeOperator)
            {
                this.nameOperator = nameOperator;
                this.indexInListOfExpressions = indexInListOfExpressions;
                this.tipo = typeOperator;

            }


            /// <summary>
            /// atualiza a lista de operadores, ante a insercao de um operador na lista de expressoes.
            /// </summary>
            /// <param name="datas">lista de dados contendo operadores.</param>
            public static void UpdateIndex(List<DataOperators> datas)
            {

                if ((datas == null) || (datas.Count == 0))
                {
                    return;
                }
                for (int i = 0; i < datas.Count; i++)
                {
                    if (datas[i].nameOperator != "=")
                    {
                        datas[i].indexInListOfExpressions++;
                    }

                }
            }

            public override string ToString()
            {
                string type = "";
                switch (this.tipo)
                {
                    case tipoComOperandos.binary:
                        type = "BINARIO";
                        break;
                    case tipoComOperandos.unary:
                        type = "UNARIO";
                        break;
                    case tipoComOperandos.binaryAndUnary:
                        type = "BINARIO_E_UNARIO";
                        break;
                }


                return nameOperator + ": " + type;
            }
        }
        /// <summary>
        /// obtem o tipo de operador, quanto ao numero de operandos.
        /// </summary>
        /// <param name="nameOperator">nome do operador.</param>
        /// <returns>retorna binario, unario ou binarioEUnario.</returns>
        private DataOperators.tipoComOperandos GetTypeOperator(string nameOperator)
        {
            // inicializa a lista de operadores, se precisar.
            if (opBinary == null)
            {
                opBinary = new List<string>();
                opUnary = new List<string>();
                opUnaryAndBinary = new List<string>();

                GetNomesOperadoresBinariosEUnarios(ref opBinary, ref opUnary, ref opUnaryAndBinary);
            }

            // faz a classificação.
            if (opUnaryAndBinary.FindIndex(k => k.Equals(nameOperator)) != -1)
            {
                return DataOperators.tipoComOperandos.binaryAndUnary;
            }
            else
            if (opBinary.FindIndex(k => k.Equals(nameOperator)) != -1)
            {
                return DataOperators.tipoComOperandos.binary;
            }
            else
            if (opUnary.FindIndex(k => k.Equals(nameOperator)) != -1)
            {
                return DataOperators.tipoComOperandos.unary;
            }
            else
            {
                return DataOperators.tipoComOperandos.none;
            }

        }

        /// <summary>
        /// faz o processamento de expressao entre parenteses.
        /// </summary>
        /// <param name="tokens">tokens da expressao.</param>
        /// <param name="index">indice da malha principal de processamento de tokens.</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        /// <returns>retorna uma Expressao entre parenteses, ou null se falhar.</returns>
        private Expressao ProcessingExpressionsBetweenParaentesis(List<string> tokens, int index, Escopo escopo)
        {
            if (tokens[index] == "(")
            {
                List<string> tokensDeExprssParenteses = UtilTokens.GetCodigoEntreOperadores(index, "(", ")", tokens);
                if ((tokensDeExprssParenteses != null) && (tokensDeExprssParenteses.Count >= 2))
                {
                    tokensDeExprssParenteses.RemoveAt(0);
                    tokensDeExprssParenteses.RemoveAt(tokensDeExprssParenteses.Count - 1);

                    Expressao exprssContainer = new Expressao(tokensDeExprssParenteses.ToArray(), escopo);
                    if (exprssContainer != null)
                    {
                        ExpressaoEntreParenteses expressaoEntreParenteses = new ExpressaoEntreParenteses(exprssContainer, escopo);
                        if (exprssContainer != null)
                        {
                            return expressaoEntreParenteses;
                        }
                    }
                    UtilTokens.WriteAErrorMensage("bad format for expression beetween paraentesis.   " + Utils.OneLineTokens(tokens), tokens, escopo);
                    return null;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// extrai expressoes, que nao tem nada a ver um com outro.
        /// </summary>
        /// <param name="codigo">codigo com as expressoes.</param>
        /// <param name="escopo">contexto onde o codigo está,</param>
        /// <returns>retorna uma lista de expressoes ou null se resultar em erros.</returns>
        public List<Expressao> ExtraiMultipasExpressoesIndependentes(string codigo, Escopo escopo)
        {
            List<string> tokens = new Tokens(codigo).GetTokens();


            int offsetTokensVirgula = 0;

            // lista de tokens de cada expressao extraidas.
            List<List<string>> tokensEXPRESSOES_INDEPENDENTES = new List<List<string>>();

            // indices de comeco, fim, de tokens de uma expressao.
            int indexBEGINTokensINDPENDENTES = 0;
            int indexENDtokensINDEPENDENTES = -1;

            // pilhas de operadores duplo.
            int pilhaParenteses = 0;
            int pilhaColchetes = 0;
            int pilhaOperadorBlocos = 0;


            List<Expressao> exprssRetorno = new List<Expressao>();
            int x = 0;
            bool isTerminadoExtracao = false;
            int indexBeginTokensExpressao = 0;
            while ((x < tokens.Count) && (!isTerminadoExtracao))
            {

                offsetTokensVirgula = +1;


                // TOKEN PONTO-E-VIRGULA, EXTRAI UMA EXPRESSAO.
                if (tokens[x] == ";")
                {
                    ExtraiUMAExpressao(tokens, tokensEXPRESSOES_INDEPENDENTES, ref indexBeginTokensExpressao, ref indexBEGINTokensINDPENDENTES, ref indexENDtokensINDEPENDENTES, ref x, ref offsetTokensVirgula);
                    continue;
                }
                else
                // SE A PILHA DE OPERADORES = 0, E TOKEN VIRGULA, EXTRAI UMA EXPRESSAO.
                if ((tokens[x] == ",") && (pilhaParenteses == 0))
                {
                    ExtraiUMAExpressao(tokens, tokensEXPRESSOES_INDEPENDENTES,
                        ref indexBeginTokensExpressao,
                        ref indexBEGINTokensINDPENDENTES,
                        ref indexENDtokensINDEPENDENTES,
                        ref x, ref offsetTokensVirgula);

                    continue;
                }
                else
                if ((tokens[x] == "(") &&
                    (x - 1 >= 0) &&
                    (x < tokens.Count) &&
                    (GruposEntreParenteses.IsID(tokens[x - 1])))
                {

                    // atingiu uma area de parametros de uma chamada de metodo;
                    List<string> tokensParametrosFuncao = UtilTokens.GetCodigoEntreOperadores(x, "(", ")", tokens);
                    if ((tokensParametrosFuncao != null) && (tokensParametrosFuncao.Count > 0))
                    {
                        // faz o incremento do indice de malha, abrangendo os tokens parametros de uma expressao (chamada de metodo).
                        x += tokensParametrosFuncao.Count;
                        continue;
                    }
                    else
                    {
                        x++;
                    }
                    continue;
                }
                else
                if (tokens[x] == "(")
                {
                    pilhaParenteses++;
                }
                else
                if (tokens[x] == ")")
                {
                    pilhaParenteses--;
                }
                else
                if (tokens[x] == "[")
                {
                    pilhaColchetes++;
                }
                else
                if (tokens[x] == "]")
                {
                    pilhaColchetes--;
                }
                else
                if (tokens[x] == "{")
                {
                    pilhaOperadorBlocos++;
                }
                else
                if (tokens[x] == "}")
                {
                    pilhaOperadorBlocos--;
                }


                x++;
            }
            // PROCESSAMENTO DA ULTIMA EXPRESSAO, PENDENTE DENTRO DA LISTA DE TOKENS currente;
            if ((indexBEGINTokensINDPENDENTES != -1) && (indexENDtokensINDEPENDENTES < tokens.Count))
            {
                List<string> tokensIND = tokens.GetRange(indexBEGINTokensINDPENDENTES, tokens.Count - indexBEGINTokensINDPENDENTES);
                tokensEXPRESSOES_INDEPENDENTES.Add(new List<string>());
                tokensEXPRESSOES_INDEPENDENTES[tokensEXPRESSOES_INDEPENDENTES.Count - 1].AddRange(tokensIND);
            }

            if (tokensEXPRESSOES_INDEPENDENTES.Count > 0)
            {


                for (int n = 0; n < tokensEXPRESSOES_INDEPENDENTES.Count; n++)
                {
                    Expressao umaExpressao = new Expressao(tokensEXPRESSOES_INDEPENDENTES[n].ToArray(), escopo);
                    if (umaExpressao == null)
                    {
                        UtilTokens.WriteAErrorMensage("bad format in group of independents expressions.  " + Utils.OneLineTokens(tokens), tokens, escopo);
                        return null;
                    }
                    else
                    {

                        exprssRetorno.Add(umaExpressao);
                    }

                }
                return exprssRetorno;
            }



            return exprssRetorno;
        }


        /// <summary>
        /// faz o processamento de tokens de uma expressao independente.
        /// </summary>
        /// <param name="tokensEXPRESSOES_INDEPENDENTES">lista de todas expressoes encontradas, seus tokens.</param>
        /// <param name="indexBeginTokensExpressao">indice de comeco da expressao.</param>
        /// <param name="indexBEGINTokensINDPENDENTES">indice da lista de expressao independentes;</param>
        /// <param name="indexENDtokensINDEPENDENTES">indice da lista de expressao independentes;</param>
        /// <param name="x">variavel de malha de tokens.</param>
        /// <param name="offsetTokensVirgula">offset para contabilizar o operador [;].</param>
        private void ExtraiUMAExpressao(List<string> tokensNAOResumidos, List<List<string>> tokensEXPRESSOES_INDEPENDENTES, ref int indexBeginTokensExpressao, ref int indexBEGINTokensINDPENDENTES, ref int indexENDtokensINDEPENDENTES, ref int x, ref int offsetTokensVirgula)
        {
            List<string> tokensIND = tokensNAOResumidos.GetRange(indexBeginTokensExpressao, x - indexBeginTokensExpressao);
            if ((tokensIND != null) && (tokensIND.Count == 1) && (tokensIND[0] == ","))
            {
                x += 1;
                return;
            }

            tokensEXPRESSOES_INDEPENDENTES.Add(new List<string>());
            tokensEXPRESSOES_INDEPENDENTES[tokensEXPRESSOES_INDEPENDENTES.Count - 1].AddRange(tokensIND);

            indexBeginTokensExpressao = x + 1;


            indexBEGINTokensINDPENDENTES = x + 1;
            offsetTokensVirgula = indexBEGINTokensINDPENDENTES;






            x = indexBEGINTokensINDPENDENTES;
        }



        /// <summary>
        /// faz o processamento de insercao de operadores.
        /// </summary>
        /// <param name="escopo">contexto onde a expressao principal está.</param>
        /// <param name="dadosDeOperadores">lista de operadores extraidos do processamento da expressao principal.</param>
        /// <param name="exprssRetorno">lista de sub-expressoes da expressao de retorno.</param>
        /// <param name="tokens">tokens da expressao.</param>
        /// <returns>[true] se o processamento foi bem sucedido.</returns>
        private bool ProcessingInsertingOperators(Escopo escopo, List<DataOperators> dadosDeOperadores, List<Expressao> exprssRetorno, List<string> tokens)
        {

           

            List<string> operadoresCONDICIONAIS_BINARIOS = new List<string>() { ">", "<", ">=", "<=", "==" };
            bool isAtribuitionPresent = false;
            int opAtribution = 0;
            if ((dadosDeOperadores != null) && (dadosDeOperadores.Count > 0))
            {
                // em expressoes de mais de um operador, certo, a expressao operador anterior se comporta como operando da proxima
                // expressao operador.
                for (int op = 0; op < dadosDeOperadores.Count; op++)
                {
                    string nameOperator = dadosDeOperadores[op].nameOperator;
                    // operador igual já foi extraido.
                    if (nameOperator == "=")
                    {
                        isAtribuitionPresent = true;
                        opAtribution = op;
                        continue;
                    }

                    // OPERADOR EXCLUSIVAMENTE BINARIO. processamento de OPERANDOS.
                    if (dadosDeOperadores[op].tipo == DataOperators.tipoComOperandos.binary)
                    {
                        // fazer processamento aqui dos operandos do operador binario.
                        int indexPrimeiroOperando = dadosDeOperadores[op].indexInListOfExpressions - 1;
                        int indexSegundoOperando = dadosDeOperadores[op].indexInListOfExpressions;

                        // PROCESSAMENTO DE OPERADORES CONDICIONAIS CONECTIVOS.
                        string nomeOperador = dadosDeOperadores[op].nameOperator;
                        if ((nomeOperador == "||") || (nomeOperador == "&&"))
                        {
                            Operador operadorCondicionalConectivo = new Operador("", nomeOperador, -11, null, new List<Instrucao>() { }, new Objeto[2] { null, null }, escopo);
                            operadorCondicionalConectivo.tipo = "BINARIO";
                            ExpressaoOperador exprssOperador = new ExpressaoOperador(operadorCondicionalConectivo);


                            // insere o operador na lista de expressoes.
                            exprssRetorno.Insert(dadosDeOperadores[op].indexInListOfExpressions, exprssOperador);
                            // atualiza a lista de operadores.
                            DataOperators.UpdateIndex(dadosDeOperadores);


                            exprssOperador.tipoDaExpressao = "bool";

                            continue;
                        }




                        if ((indexPrimeiroOperando >= exprssRetorno.Count) || (indexSegundoOperando >= exprssRetorno.Count))
                        {
                            return false;
                        }
                        if ((indexPrimeiroOperando < 0) || (indexSegundoOperando < 0))
                        {
                            UtilTokens.WriteAErrorMensage("binary operator: " + nameOperator + " without first or second parameter", tokens, escopo);
                            return false;
                        }

                        Expressao exprssOperando1 = exprssRetorno[indexPrimeiroOperando];
                        Expressao exprssOperando2 = exprssRetorno[indexSegundoOperando];
                        bool isBinaryAdnUnary = false;
                        // processamento de encontrar operador compativel com os tipos do operando.
                        Operador operatorCompatible = UtilTokens.FindOperatorCompatible(dadosDeOperadores[op].nameOperator, exprssOperando1.tipoDaExpressao, exprssOperando2.tipoDaExpressao, ref isBinaryAdnUnary);
                        if (operatorCompatible != null)
                        {



                            ExpressaoOperador exprssOperador = new ExpressaoOperador(operatorCompatible);
                            exprssOperador.tipoDaExpressao = operatorCompatible.tipoRetorno;
                            exprssOperador.typeOperandos = HeaderOperator.typeOperator.binary;

                            // insere o operador na lista de expressoes.
                            exprssRetorno.Insert(dadosDeOperadores[op].indexInListOfExpressions, exprssOperador);


                            // EXPRESSAO CONDICIONAL.
                            if (operadoresCONDICIONAIS_BINARIOS.FindIndex(k => k.Equals(dadosDeOperadores[op].nameOperator)) != -1)
                            {
                                exprssOperador.tipoDaExpressao = "bool";

                            }


                            // atualiza a lista de operadores.
                            DataOperators.UpdateIndex(dadosDeOperadores);



                            continue;
                        }
                        if (operatorCompatible == null)
                        {
                            UtilTokens.WriteAErrorMensage("operator: " + nameOperator + " not match with parameters list for it", tokens, escopo);
                            return false;
                        }


                    }
                    else
                    // OPERADOR EXCLUSIVAMENTE UNARIO. processamento de OPERANDOS.
                    if (dadosDeOperadores[op].tipo == DataOperators.tipoComOperandos.unary)
                    {
                        int indexPrimeiroOperando = dadosDeOperadores[op].indexInListOfExpressions - 1;
                        int indexSegundoOperando = dadosDeOperadores[op].indexInListOfExpressions;

                        string nomeOperador = dadosDeOperadores[op].nameOperator;
                        bool isBinaryAndUnary = false;



                        if ((indexPrimeiroOperando >= 0) && (UtilTokens.IsUnaryOperator(nomeOperador, exprssRetorno[indexPrimeiroOperando])))
                        {
                            // operador UNARIO POS.
                            Expressao exprssOperando = exprssRetorno[indexPrimeiroOperando];
                            Operador operadorCompatible = UtilTokens.FindOperatorCompatible(nomeOperador, exprssOperando.tipoDaExpressao, null, ref isBinaryAndUnary);

                            if (operadorCompatible != null)
                            {
                                ExpressaoOperador exprssOperatorUnary = new ExpressaoOperador(operadorCompatible);
                                exprssOperatorUnary.tipoDaExpressao = operadorCompatible.tipoRetorno;
                                exprssOperatorUnary.typeOperandos = HeaderOperator.typeOperator.unary_pos;

                                // insere o operador na lista de expressoes.
                                exprssRetorno.Insert(dadosDeOperadores[op].indexInListOfExpressions, exprssOperatorUnary);


                                // atualiza a insercao de operadores.
                                DataOperators.UpdateIndex(dadosDeOperadores);


                                continue;
                            }

                            if (operadorCompatible == null)
                            {
                                UtilTokens.WriteAErrorMensage("operator: " + nomeOperador + " not match with parameters for it", tokens, escopo);
                                return false;
                            }

                        }
                        else
                        if ((indexSegundoOperando < exprssRetorno.Count) && (UtilTokens.IsUnaryOperator(nomeOperador, exprssRetorno[indexSegundoOperando])))
                        {
                            //operador UNARIO PRE
                            Expressao exprssOperando2 = exprssRetorno[indexSegundoOperando];
                            Operador operadorCompatible = UtilTokens.FindOperatorCompatible(nomeOperador, null, exprssOperando2.tipoDaExpressao, ref isBinaryAndUnary);
                            if (operadorCompatible == null)
                            {
                                UtilTokens.WriteAErrorMensage("operator: " + nomeOperador + " not match with parameters list for it", tokens, escopo);
                                return false;
                            }
                            if (operadorCompatible != null)
                            {
                                ExpressaoOperador exprssOperator = new ExpressaoOperador(operadorCompatible);
                                exprssOperator.tipoDaExpressao = operadorCompatible.tipoRetorno;
                                exprssOperator.typeOperandos = HeaderOperator.typeOperator.unary_pre;

                                // faz o processamneto de expressoes condicionais.
                                if (nomeOperador == "!")
                                {
                                    exprssOperator.tipoDaExpressao = "bool";
                                }



                                // insere o operador na lista de expressoes.
                                exprssRetorno.Insert(dadosDeOperadores[op].indexInListOfExpressions, exprssOperator);
                                // atualiza a insercao de operadores.
                                DataOperators.UpdateIndex(dadosDeOperadores);

                                continue;
                            }

                        }
                        else
                        {   // nao validou o operador unario.
                            UtilTokens.WriteAErrorMensage("operator:" + nomeOperador + " is invalid name, or dont match with parameters for it", tokens, escopo);
                            return false;
                        }


                    }
                    else
                    // operador UNARIO E BINARIO. processamento de OPERANDOS.
                    if (dadosDeOperadores[op].tipo == DataOperators.tipoComOperandos.binaryAndUnary)
                    {


                        string nomeOperador = dadosDeOperadores[op].nameOperator;
                        int indexOperador = dadosDeOperadores[op].indexInListOfExpressions;
                        int indexPrimeiroOperando = dadosDeOperadores[op].indexInListOfExpressions - 1;
                        int indexSegundoOperando = dadosDeOperadores[op].indexInListOfExpressions;

                        bool isValidFirstOperand = false;
                        bool isValidSecondOperand = false;
                        // operador UNARIO E BINARIO funcionando como BINARIO.
                        if (IsValidBinaryOperands(exprssRetorno, indexOperador, ref isValidFirstOperand, ref isValidSecondOperand))
                        {


                            Expressao operando1 = exprssRetorno[indexPrimeiroOperando];
                            Expressao operando2 = exprssRetorno[indexSegundoOperando];

                            


                            // fazer processamento aqui dos operandos do operador unario e binario.
                            Operador operadorCompatibleBinary = UtilTokens.FindOperatorBinarioEUnarioMasComoBINARIO(
                                 operando1.tipoDaExpressao, nomeOperador, operando1.tipoDaExpressao, operando2.tipoDaExpressao);

                            if (operadorCompatibleBinary != null)
                            {
                                ExpressaoOperador exprssOperatorBinario = new ExpressaoOperador(operadorCompatibleBinary);
                                exprssOperatorBinario.tipoDaExpressao = operadorCompatibleBinary.tipoRetorno;
                                exprssOperatorBinario.typeOperandos = HeaderOperator.typeOperator.binary;

                                // insere o operador na lista de expressoes.
                                exprssRetorno.Insert(dadosDeOperadores[op].indexInListOfExpressions, exprssOperatorBinario);
                                // atualiza a insercao de operadores.
                                DataOperators.UpdateIndex(dadosDeOperadores);

                                continue;

                            }
                            if (operadorCompatibleBinary == null)
                            {
                                UtilTokens.WriteAErrorMensage("binary operator: " + nomeOperador + " not found, or dont match with parameters list for it." + Utils.OneLineTokens(tokens), tokens, escopo);
                                return false;
                            }



                        }
                        else
                        // operador UNARIO E BINARIO, funcionando como UNARIO PRE.
                        if ((!isValidFirstOperand) && (isValidSecondOperand))
                        {
                            Expressao operando = exprssRetorno[indexSegundoOperando];
                            Operador operadorCompatibleUnary = UtilTokens.FindOperatorBinarioUnarioMasComoUNARIO(
                                nomeOperador, null, operando.tipoDaExpressao);


                            if (operadorCompatibleUnary != null)
                            {
                                ExpressaoOperador exprssOperadorUnarioPRE = new ExpressaoOperador(operadorCompatibleUnary);
                                exprssOperadorUnarioPRE.typeOperandos = HeaderOperator.typeOperator.unary_pre;
                                exprssOperadorUnarioPRE.tipoDaExpressao = operadorCompatibleUnary.tipoRetorno;

                                // insere o operador na lista de expressoes.
                                exprssRetorno.Insert(dadosDeOperadores[op].indexInListOfExpressions, exprssOperadorUnarioPRE);
                                // atualiza a insercao de operadores.
                                DataOperators.UpdateIndex(dadosDeOperadores);

                                continue;
                            }
                            if (operadorCompatibleUnary == null)
                            {
                                UtilTokens.WriteAErrorMensage("binary and unary operator, not match for your type as unary, or dont match with parameters list for it", tokens, escopo);
                                return false;
                            }



                        }
                        else
                        // operador UNARIO E BINARIO, funcionando como UNARIO POS.
                        if ((isValidFirstOperand) && (!isValidSecondOperand))
                        {
                            Expressao operando = exprssRetorno[indexPrimeiroOperando];
                            Operador operadorCompatibleUnary = UtilTokens.FindOperatorBinarioUnarioMasComoUNARIO(
                                                                     nomeOperador, operando.tipoDaExpressao, null);

                            if (operadorCompatibleUnary != null)
                            {
                                ExpressaoOperador exprssOperadorUnarioPOS = new ExpressaoOperador(operadorCompatibleUnary);

                                exprssOperadorUnarioPOS.tipoDaExpressao = operadorCompatibleUnary.tipoRetorno;
                                exprssOperadorUnarioPOS.typeOperandos = HeaderOperator.typeOperator.unary_pos;

                                // insere o operador na lista de expressoes.
                                exprssRetorno.Insert(dadosDeOperadores[op].indexInListOfExpressions, exprssOperadorUnarioPOS);
                                // atualiza a insercao de operadores.
                                DataOperators.UpdateIndex(dadosDeOperadores);

                                continue;
                            }
                            if (operadorCompatibleUnary == null)
                            {
                                UtilTokens.WriteAErrorMensage("binary and unary operator, not match for your type as unary, or dont match with parameters list for it", tokens, escopo);
                                return false;
                            }


                        }



                    }
                }
            }
            if (isAtribuitionPresent)
            {
                ProcessingSignalEqualsOperator(escopo, exprssRetorno, dadosDeOperadores, tokens, opAtribution);
            }
            return true;
        }



     
        /// <summary>
        /// verifica se os operandos para um operador binario sao operandos validos para operador binario.
        /// </summary>
        /// <param name="elementos">lista de expressoes contendo operandos, operadores,...</param>
        /// <param name="indexOperator">indice da expressao operador, dentro da lista de expressoes.</param>
        /// <param name="isValidFirst">true se o 1o. operando eh valido.</param>
        /// <param name="isValidSecond">true se o 2o. operando eh valido.</param>
        /// <returns>retorna true se os dois operandos do operador sao operandos validos.</returns>
        private bool IsValidBinaryOperands(List<Expressao> elementos, int indexOperator, ref bool isValidFirst, ref bool isValidSecond)
        {
            isValidSecond = false;
            isValidFirst = false;

            if (elementos == null)
            {
                return false;
            }
            List<Type> tiposValidosOperand = new List<Type>();
            tiposValidosOperand.Add(typeof(ExpressaoObjeto));
            tiposValidosOperand.Add(typeof(ExpressaoChamadaDeMetodo));
            tiposValidosOperand.Add(typeof(ExpressaoPropriedadesAninhadas));
            tiposValidosOperand.Add(typeof(ExpressaoNumero));
            tiposValidosOperand.Add(typeof(ExpressaoLiteralText));
            tiposValidosOperand.Add(typeof(ExpressaoEntreParenteses));
            tiposValidosOperand.Add(typeof(Expressao));
            if (indexOperator - 1 < 0)
            {


                if (indexOperator < elementos.Count)
                {
                    isValidSecond = tiposValidosOperand.FindIndex(k => k == elementos[indexOperator].GetType()) != -1;
                }
                return false;
            }
            if (indexOperator >= elementos.Count)
            {
                if (indexOperator - 1 > 0)
                {
                    isValidFirst = tiposValidosOperand.FindIndex(k => k == elementos[indexOperator - 1].GetType()) != -1;
                }
                return false;
            }



            isValidFirst = tiposValidosOperand.FindIndex(k => k == elementos[indexOperator - 1].GetType()) != -1;
            isValidSecond = tiposValidosOperand.FindIndex(k => k == elementos[indexOperator].GetType()) != -1;

            return (isValidFirst && isValidSecond);
        }


        /// <summary>
        /// PROCESSAMENTO DE EXPRESSAO DE ATRIBUICAO, COM OPERADOR = (IGUAL).
        /// </summary>
        /// <param name="indexTokenCurrent">indice da malha de tokens principal.</param>
        /// <param name="escopo">contexto onde a expressao principal está.</param>
        /// <param name="exprssRetorno">sub-expressoes resultantes do processamento.</param>
        /// <param name="dadosDeOperadores">lista com dados de operadores extraidos da expressao principal.</param>
        /// <param name="tokens">tokens da expressao principal.</param>
        /// <param name="op">indice do operador.</param>
        /// <returns>[true] se o processamento foi bem sucedido.</returns>
        private bool ProcessingSignalEqualsOperator(Escopo escopo, List<Expressao> exprssRetorno, List<DataOperators> dadosDeOperadores, List<string> tokens, int op)
        {


         

            int indexOperator = dadosDeOperadores[op].indexInListOfExpressions;

            if ((indexOperator <= exprssRetorno.Count) && (indexOperator - 1) >= 0)
            {


                // TIPO DO OBJETO A RECEBER É UM [OBJETO].
                if (exprssRetorno[indexOperator - 1].GetType() == typeof(ExpressaoObjeto))
                {
                    Objeto objRecebeAtribuicao = ((ExpressaoObjeto)exprssRetorno[indexOperator - 1]).objectCaller;
                    if ((objRecebeAtribuicao != null) && (!objRecebeAtribuicao.isWrapperObject))
                    {
                        BuildExpressaoAtribuicaoExpressaoObjeto(false, tokens, ref exprssRetorno, dadosDeOperadores, objRecebeAtribuicao, escopo);
                    }
                    else
                    // CASO EM QUE O OBJETO A ATRIBUIR É UM WRAPPER OBJECT.
                    if (objRecebeAtribuicao.isWrapperObject)
                    {
                        BuildExpressaoAtribuicaoExpressaoObjeto(true, tokens, ref exprssRetorno, dadosDeOperadores, objRecebeAtribuicao, escopo);
                    }

                }
                else
                // TIPO DO OBJETO A RECEBER É UMA [PROPRIEDADE_ANINHADA].
                if (exprssRetorno[indexOperator - 1].GetType() == typeof(ExpressaoPropriedadesAninhadas))
                {
                    ExpressaoPropriedadesAninhadas exprssProps = ((ExpressaoPropriedadesAninhadas)exprssRetorno[indexOperator - 1]);
                    List<Objeto> objetosAninhados = exprssProps.aninhamento;
                    if ((objetosAninhados == null) || (objetosAninhados.Count == 0))
                    {
                        UtilTokens.WriteAErrorMensage("properties:bad format.  " + Utils.OneLineTokens(tokens), tokens, escopo);
                        return false;
                    }

                    int indexSginalEquals = tokens.IndexOf("=");
                    BuildExpressaoAtribuicaoExpressaoPropriedadesAninhadas(
                                    tokens, ref exprssRetorno, indexSginalEquals, dadosDeOperadores, exprssProps, escopo);





                }
            }


            return false;
        }


        /// <summary>
        /// constroi uma expressao de atribuicao, a partir de uma propriedade aninhada a receber a atribuicao.
        /// </summary>
        /// <param name="tokens">tokens da expressao.</param>
        /// <param name="exprssRetorno">expressao contendo todas sub-expressoes de retorno.</param>
        /// <param name="indexSignalEquals">indice na malha de tokens, do signal de atribuicao igual.</param>
        /// <param name="dadosDeOperadores">dados de operaddores.</param>
        /// <param name="expressaoANINHADO_ReceberAtribuicao">propriedade aninhada a receber a atribuicao.</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        /// <returns></returns>
        private bool BuildExpressaoAtribuicaoExpressaoPropriedadesAninhadas(
            List<string> tokens,
            ref List<Expressao> exprssRetorno,
            int indexSignalEquals,
            List<DataOperators> dadosDeOperadores,
            ExpressaoPropriedadesAninhadas expressaoANINHADO_ReceberAtribuicao, Escopo escopo)
        {

          


            if (expressaoANINHADO_ReceberAtribuicao == null)
            {
                UtilTokens.WriteAErrorMensage("object caller to set the atribution is null.   " + Utils.OneLineTokens(tokens), tokens, escopo);
                return false;
            }
            DataOperators operadorIgual = dadosDeOperadores.Find(k => k.nameOperator == "=");
            if (operadorIgual == null)
            {
                UtilTokens.WriteAErrorMensage("operator = data, not found. " + Utils.OneLineTokens(tokens), tokens, escopo);
            }


            // obtem os tokens da expressao aninhada.
            int signalEqualsTOKENS = tokens.IndexOf("=");
            List<string> tokensTotalExpressaoAtribuicao = tokens.GetRange(signalEqualsTOKENS + 1, tokens.Count - (signalEqualsTOKENS + 1));




            Expressao exprssRECEBE_A_ATRIBUICAO = new Expressao(tokensTotalExpressaoAtribuicao.ToArray(), escopo);
            if (exprssRECEBE_A_ATRIBUICAO != null)
            {
                ExpressaoAtribuicao exprssAtribui = new ExpressaoAtribuicao(expressaoANINHADO_ReceberAtribuicao, exprssRECEBE_A_ATRIBUICAO, escopo);

                // remove as expressoes que foram utilizadas na expressao de atribuicao.
                if (exprssRetorno.Count >= 2)
                {
                    exprssRetorno.Clear();
                }


                // adiciona a expressao de atribuicao, para a expressao principal.
                exprssRetorno.Add(exprssAtribui);

                DataOperators.UpdateIndex(dadosDeOperadores);


                return true;

            }


            if ((exprssRECEBE_A_ATRIBUICAO != null) && (exprssRECEBE_A_ATRIBUICAO.tipoDaExpressao != expressaoANINHADO_ReceberAtribuicao.tipoDaExpressao))
            {
                UtilTokens.WriteAErrorMensage("types of object of atribution and expression atribution dont match!  " + Utils.OneLineTokens(tokens), tokens, escopo);
                return false;
            }
            else
            if (exprssRECEBE_A_ATRIBUICAO == null)
            {
                UtilTokens.WriteAErrorMensage("invalid expression in atributation.   "+Utils.OneLineTokens(tokensTotalExpressaoAtribuicao), tokensTotalExpressaoAtribuicao, escopo);
                return false;
            }


            return false;
        }




        /// <summary>
        /// constroi uma expressao de retorno.
        /// </summary>
        /// <param name="isWrapperObject">[true] se o objeto atribuir é wrapper object.</param>
        /// <param name="tokens">lista de tokens da expressao.</param>
        /// <param name="exprssRetorno">todas sub-expressoes.</param>
        /// <param name="dadosDeOperadores">dados de operadores encontrados.</param>
        /// <param name="objRecebeAtribuicao">objeto a receber a atribuicao.</param>
        /// <param name="escopo">contexto onde a expressao total está.</param>
        /// <returns></returns>
        private bool BuildExpressaoAtribuicaoExpressaoObjeto(bool isWrapperObject, List<string> tokens, ref List<Expressao> exprssRetorno, List<DataOperators> dadosDeOperadores, Objeto objRecebeAtribuicao, Escopo escopo)
        {


            if (objRecebeAtribuicao == null)
            {
                UtilTokens.WriteAErrorMensage("object caller to set the atribution is null.  " + Utils.OneLineTokens(tokens), tokens, escopo);
                return false;
            }
            int indexExpressaoAtribuicao = tokens.IndexOf(objRecebeAtribuicao.GetNome()) + 2;
            List<string> tokensTotalExpressaoAtribuicao = tokens.GetRange(indexExpressaoAtribuicao, tokens.Count - indexExpressaoAtribuicao);


            DataOperators dataSignalEquals = dataOperadores.Find(k => k.nameOperator == "=");
            if (dataSignalEquals == null)
            {
                UtilTokens.WriteAErrorMensage("bad format in expression, expected operator =, not found.   " + Utils.OneLineTokens(tokens), tokens, escopo);
                return false;
            }

            int indexSignalEquals = dataSignalEquals.indexInListOfExpressions;        
    

            Expressao exprssATRIBUICAO = new Expressao(tokensTotalExpressaoAtribuicao.ToArray(), escopo);
            if (exprssATRIBUICAO != null)
            {
                ExpressaoObjeto exprssObjeto = new ExpressaoObjeto(objRecebeAtribuicao);
                ExpressaoAtribuicao exprssAtribui = new ExpressaoAtribuicao(exprssObjeto, exprssATRIBUICAO, escopo);
                if (isWrapperObject)
                {
                    exprssAtribui.objetoAtribuir = objRecebeAtribuicao;

                }
                else
                {
                    exprssAtribui.objetoAtribuir = objRecebeAtribuicao;
                    exprssAtribui.objetoAtribuir.valor = objRecebeAtribuicao.valor;
                }
                // remove as expressoes que foram utilizadas na expressao de atribuicao.
                if (exprssRetorno.Count >= 2)
                {
                    exprssRetorno.RemoveRange(0, exprssRetorno.Count - indexSignalEquals + 1); // -1 da expressao antes do sinal de igual, +1 porque é contador, não indice.
                }

                // adiciona a expressao de atribuicao, para a expressao principal..
                exprssRetorno.Add(exprssAtribui);

                DataOperators.UpdateIndex(dadosDeOperadores);


                return true;

            }
            if ((exprssATRIBUICAO != null) && (exprssATRIBUICAO.tipoDaExpressao != objRecebeAtribuicao.GetTipo()))
            {
                UtilTokens.WriteAErrorMensage("types of object of atribution and expression atribution dont match!  " + Utils.OneLineTokens(tokens), tokens, escopo);
                return false;
            }
            else
            if (exprssATRIBUICAO == null)
            {
                UtilTokens.WriteAErrorMensage("invalid expression in atributation!  " + Utils.OneLineTokens(tokensTotalExpressaoAtribuicao), tokensTotalExpressaoAtribuicao, escopo);
                return false;
            }

            return false;
        }

    
    
        public class Testes : SuiteClasseTestes
        {
            private char aspas = '\"';
            public Testes() : base("testes processamento de expressoes pelo classificador")
            {
            }


            public void TestesPropriedadesAninhadasEchamadasDeMetodo(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                try
                {

                    string codigo_Pixel = "public class Pixel{public Pixel(){}; public int propriedadePixel; public int metodoPixel(){};}";
                    string codigo_MyImage = "public class MyImage{ public Pixel propriedadeA; public int metodoA(){}; public MyImage(){};};";
                    string codigo_classeB = "public class classeB{ public MyImage imageB; public MyImage metodoB(); public classeB(){};};";

                    string codigo_create = "MyImage image1= create(); classeB objB= create();";

                    ProcessadorDeID compilador = new ProcessadorDeID(codigo_Pixel + codigo_MyImage + codigo_classeB + codigo_create);
                    compilador.Compilar();

                    string codigo_umaPropriedadeAninhada = "image1.propriedadeA;";
                    string codigo_duasPropriedadesAninhadas = "objB.imageB.propriedadeA;";
                    string codigo_umaChamadaDeMetodo = "image1.metodoA();";
                    string codigo_duasChamadasDeMetodo = "objB.metodoB().metodoA()";
                    string codigo_duasPropriedadesEUmaChamadaDeMetodo = "objB.imageB.propriedadeA.metodoPixel();";


                    Expressao exprss_duasChamadaDeMetodo = new Expressao(codigo_duasChamadasDeMetodo, compilador.escopo);
                    assercao.IsTrue(exprss_duasChamadaDeMetodo.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo) && (exprss_duasChamadaDeMetodo.Elementos[0].Elementos.Count == 1), codigo_duasChamadasDeMetodo);


                    Expressao exprss_umChamadaDeMetodo = new Expressao(codigo_umaChamadaDeMetodo, compilador.escopo);
                    assercao.IsTrue(exprss_umChamadaDeMetodo.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo_umaChamadaDeMetodo);


                    Expressao exprss_duasPropEUmaChamada = new Expressao(codigo_duasPropriedadesEUmaChamadaDeMetodo, compilador.escopo);
                    assercao.IsTrue(exprss_duasPropEUmaChamada.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas) &&
                    (((ExpressaoPropriedadesAninhadas)exprss_duasPropEUmaChamada.Elementos[0]).aninhamento.Count == 2) &&
                    (exprss_duasPropEUmaChamada.Elementos[0].Elementos.Count == 1), codigo_duasPropriedadesEUmaChamadaDeMetodo);


                    Expressao exprss_duasProp = new Expressao(codigo_duasPropriedadesAninhadas, compilador.escopo);
                    assercao.IsTrue(exprss_duasProp.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas) && ((ExpressaoPropriedadesAninhadas)exprss_duasProp.Elementos[0]).aninhamento.Count == 2, codigo_duasPropriedadesAninhadas);

                    Expressao exprss_umaProp = new Expressao(codigo_umaPropriedadeAninhada, compilador.escopo);
                    assercao.IsTrue(exprss_umaProp.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas), codigo_umaPropriedadeAninhada);




                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }




            }


            public void TestesFuncoesAninhadasDictionrayText(AssercaoSuiteClasse assercao)
            {

                try
                {
                    SystemInit.InitSystem();
                    string code_0_0_create = "DictionaryText d1= { string }";
                    string code_0_exrpss = "d1{" + aspas + "abacaxi" + aspas + "}.Hash()";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_create);
                    compilador.Compilar();

                    Expressao epxrss_0_0 = new Expressao(code_0_exrpss, compilador.escopo);

                    assercao.IsTrue(epxrss_0_0.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo) &&
                                    epxrss_0_0.Elementos[0].Elementos.Count == 1, code_0_exrpss);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }


            }

            public void TesteChamadasDeMetodoAninhadasComWrapperObject(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_0_0_program = "string[] v1[20];";
                    string code_0_0_GET = "v1[1].Hash();";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_program);
                    compilador.Compilar();

                    Expressao exprss_0_0 = new Expressao(code_0_0_GET, compilador.escopo);

                    assercao.IsTrue(
                                   (exprss_0_0.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo)) &&
                                   (exprss_0_0.Elementos[0].Elementos.Count == 1) &&
                                   (exprss_0_0.Elementos[0].Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo)));

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }


            }


            public void TestePropriedadeAninhadaSeguidaDePropriedadeAninhada(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoClasseX = "public class classeX { public int propriedadeX;  public classeX() { int y; }  public classeA metodoX(int x, int y) { int x; }; }";
                string codigoClasseA = "public class classeA { public classeX propriedadeA; public classeA() { int x=1; }  public int metodoA(){ int y= 1; }; };";
                string codigoProgram = "classeA objA= create();";

                string codigoExpressao = "objA.propriedadeA.propriedadeX;";


                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseX + codigoClasseA + codigoProgram);
                compilador.Compilar();
                Escopo.nomeClasseCurrente = "";

                Expressao expressao = new Expressao(codigoExpressao, compilador.escopo);

                try
                {
                    assercao.IsTrue(
                        (expressao.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas)) &&
                        (((ExpressaoPropriedadesAninhadas)expressao.Elementos[0]).aninhamento.Count == 2));

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }


            public void TestePropriedadeProtected(AssercaoSuiteClasse assercao)
            {

                // teste de verificação de erro de acesso a uma propriedade protected.
                try
                {
                    SystemInit.InitSystem();
                    string code_classB = "public class classeB { protected int propriedadeA; public classeB() { propriedadeA=5; propriedadeA= propriedadeA+1;};};";
                    string code_classA = "public class classeA { public int propriedadeB; public classeA() { classeB obj1= create(); obj1.propriedadeA; };};";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_classB + code_classA);
                    compilador.Compilar();



                    assercao.IsTrue(SystemInit.errorsInCopiling.Count > 0, code_classB + "  " + code_classA);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }


            public void TestesFuncoesAninhadasVector(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_0_0_create = "double[] v1[20];";
                    string code_0_0_exprss = "v1[1].tan()";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_create);
                    compilador.Compilar();

                    Expressao exprss_0_0 = new Expressao(code_0_0_exprss, compilador.escopo);

                    assercao.IsTrue(exprss_0_0.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo) &&
                                     exprss_0_0.Elementos[0].Elementos.Count == 1, code_0_0_exprss);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }

            }


            public void TesteAtribuicaoBooleanaSemInstanciacao(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_0_0_create = "bool n= TRUE;";
                    string code_0_0_exprss = "n=FALSE;";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_create);
                    compilador.Compilar();

                    Expressao exprss_0_0 = new Expressao(code_0_0_exprss, compilador.escopo);
                    assercao.IsTrue(exprss_0_0.Elementos.Count == 1 && exprss_0_0.Elementos[0].GetType() == typeof(ExpressaoAtribuicao), code_0_0_create + "  " + code_0_0_exprss);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }

            }

            public void TesteAtribuicaoSemInstanciacao(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_0_0_create = "int n=1;";
                    string code_0_0_exprss = "n=2;";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_create);
                    compilador.Compilar();

                    Expressao exprss_0_0 = new Expressao(code_0_0_exprss, compilador.escopo);
                    assercao.IsTrue(exprss_0_0.Elementos.Count == 1, code_0_0_create + "  " + code_0_0_exprss);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }


            public void TesteConstantesBooleanas(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_0_0_true = "bool b = TRUE;";
                    string code_0_0_false = "bool c= FALSE";
                    string code_0_0_exprss = "bool d=TRUE;";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_true + code_0_0_false);
                    compilador.Compilar();


                    Expressao exprss_0_0_boolean = new Expressao(code_0_0_exprss, compilador.escopo);
                    EvalExpression eval = new EvalExpression();
                    object result = eval.EvalPosOrdem(exprss_0_0_boolean, compilador.escopo);

                    assercao.IsTrue(compilador.instrucoes.Count == 2, code_0_0_true + "  " + code_0_0_false);
                    assercao.IsTrue((bool)((Objeto)result).valor == true, code_0_0_exprss);



                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }


       

            public void TestesFuncoesAninhadasJaggedArray(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                try
                {

                    // "JaggedArray id = [ exprss ] [ ]"
                    string code_create_0_0 = "JaggedArray j1=[20][]; j1.SetTipoElemento(\"string\");";
                    string code_exprss_0_0 = "j1[1][1].Hash();";



                    ProcessadorDeID compilador = new ProcessadorDeID(code_create_0_0);
                    compilador.Compilar();

                    compilador.escopo.tabela.GetObjeto("j1", compilador.escopo).tipoElemento = "string";

                    Expressao exprss_0_0 = new Expressao(code_exprss_0_0, compilador.escopo);

                    assercao.IsTrue(exprss_0_0.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo) &&
                                    exprss_0_0.Elementos[0].Elementos.Count == 1, code_exprss_0_0);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }


            }


   
    
            public void TesteFuncaoOperacoesAritmeticasComObjetoActual(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                try
                {
                    string code_0_0_class = "public class classeB { public int propriedadeA; public classeB() { propriedadeA=5; propriedadeA= propriedadeA+1;};};";
                    string code_0_0_create = "classeB obj1= create();";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_class + code_0_0_create);
                    compilador.Compilar();

                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);


                    Objeto objTeste = compilador.escopo.tabela.GetObjeto("obj1", compilador.escopo);
                    assercao.IsTrue(
                        objTeste.propriedades[0] != null &&
                        objTeste.propriedades[0].valor.ToString() == "6", code_0_0_class);


                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }


       





    
            public void TesteOperacoesAritmeticasComVectors(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                try
                {
                    string code_0_0 = "double[] coordsXInvader[15]; double velInvaders=5.0; double direction=1.0; for (int i2=0;i2<11;i2++){coordsXInvader[i2]=2.0; coordsXInvader[i2]= coordsXInvader[i2]+velInvaders*direction;};";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                    compilador.Compilar();

                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    assercao.IsTrue(((Vector)(compilador.escopo.tabela.GetObjeto("coordsXInvader", compilador.escopo).valor)).GetElement(0).ToString() == "7");
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }

            
     
            public void TesteClasseElementoVector(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                try
                {
                    string codde_0_0_class = "public class Entity{ public int xx;  public void Entity(int i){xx=i;}; public int metodoB(){xx=1;};}; ";
                    string code_0_0_program = "Entity e= create(3); Entity[] v1[20]; v1[1]=e; v1[1].metodoB();";

                    ProcessadorDeID compilador = new ProcessadorDeID(codde_0_0_class + "  " + code_0_0_program);
                    compilador.Compilar();

                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    Vector vetor = (Vector)(compilador.escopo.tabela.GetObjeto("v1", compilador.escopo).valor);
                    Objeto elementoEnity = (Objeto)vetor.Get(1);

                    assercao.IsTrue(elementoEnity != null && elementoEnity.propriedades[0].valor.ToString() == "1", codde_0_0_class + "   " + code_0_0_program);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }


            public void TesteVetoresParametros(AssercaoSuiteClasse assercao)
            {
                try
                {

                    SystemInit.InitSystem();
                    string code_create_0_0 = "double[] vx[5]; double[] vy[5]; vx[0]=0.0; vy[0]=0.0; LoopGame window= create(900,600,\"esboco space invaders\");Imagem imgPlayer= create(\"programa jogos\\assets space invaders\\player.png\");";
                    string code_exprss = "window.Draw(imgPlayer,vx[0], vy[0]);";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_create_0_0 + code_exprss);
                    compilador.Compilar();

                    assercao.IsTrue(SystemInit.errorsInCopiling != null && SystemInit.errorsInCopiling.Count == 0, code_create_0_0 + "  " + code_exprss);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }


        



          
            public void TesteAtribuicaoPropriedadesAninhadasObjetoImportado(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string code_create_0_0 = " double xPlayer= 1.0; Vector2D positionMouse= create(50.0,0.0);";
                string code_expression_0_0 = "xPlayer = positionMouse.getX();";


                ProcessadorDeID compilador = new ProcessadorDeID(code_create_0_0 + code_expression_0_0);
                compilador.Compilar();

                ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                program.Run(compilador.escopo);

                try
                {
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("xPlayer", compilador.escopo).valor.ToString() == "50");
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }


     
            public void TesteAtribuicaoDeUmaFuncaoPorUmaVariavel(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_0_0_create = "string newText=" + aspas + "Ola" + aspas + "; int saida=1;";
                    string code_0_0_program = "newText=Castings.IntToText(saida);";


                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_create + code_0_0_program);
                    compilador.Compilar();


                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("newText", compilador.escopo).valor.ToString() == "1");
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }

     


            
            public void TestesFuncoesAninhadasMatriz(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_0_0_create = "Matriz m1[50,50];";
                    string code_0_0_express = "m1[1,1].tan()";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_create);
                    compilador.Compilar();

                    Expressao exprrs_0_0 = new Expressao(code_0_0_express, compilador.escopo);

                    assercao.IsTrue(
                        exprrs_0_0.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo) &&
                        exprrs_0_0.Elementos[0].Elementos.Count == 1, code_0_0_express);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }


   

            public void TesteExpressaoTexto(AssercaoSuiteClasse assercao)
            {
                string code_0_0 = "string newText=" + aspas + "umTexto" + aspas + "; int contador=1;";
                string code_program_0_0 = "newText= " + aspas + "contando:" + aspas + "+Castings.IntToText(contador);";

                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                compilador.Compilar();


                Expressao exprss_0_0 = new Expressao(code_program_0_0, compilador.escopo);
                EvalExpression eval = new EvalExpression();
                object result = eval.EvalPosOrdem(exprss_0_0, compilador.escopo);


                try
                {
                    assercao.IsTrue(
                         compilador.escopo.tabela.GetObjeto("newText", compilador.escopo).valor != null &&
                         compilador.escopo.tabela.GetObjeto("newText", compilador.escopo).valor.ToString() == "contando:1", code_program_0_0);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }

            public void TesteFuncaoImportada(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                try
                {
                    string code_0_0_create = "string contando=" + aspas + "contando...:" + aspas + "; int count=1; string texto=" + aspas + "txt inicial" + aspas + ";";
                    string code_exprss_0_0 = "texto = contando+ Castings.IntToText(count);";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_create);
                    compilador.Compilar();

                    Expressao exprss_0_0 = new Expressao(code_exprss_0_0, compilador.escopo);
                    EvalExpression eval = new EvalExpression();
                    object result = eval.EvalPosOrdem(exprss_0_0, compilador.escopo);

                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("texto", compilador.escopo).valor.ToString() == "contando...:1", code_0_0_create + "   " + code_exprss_0_0);

                }
                catch(Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }

            }


 
            public void TesteExpressaoMatematicaEntreParenteses(AssercaoSuiteClasse assercao)
            {
                string code_0_0_create = "double x= 100.0; double y= 50.0; double t=1.0";
                string code_0_0_exprss = "t= (x+y)*0.5;";




                try
                {
                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_create);
                    compilador.Compilar();

                    Expressao exrpss_0_0 = new Expressao(code_0_0_exprss, compilador.escopo);

                    EvalExpression eval = new EvalExpression();
                    object result = eval.EvalPosOrdem(exrpss_0_0, compilador.escopo);

                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("t", compilador.escopo).valor.ToString() == "75");
                }
                catch (Exception ex)
                {

                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }

            public void TestePropriedadesAninhadas(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoClasse = "public class classeX { public int propriedadeX; public classeX() { int y; }  public int metodoX(int x, int y) { int x; }; };";
                string codigoCreate = "classeX obj1= create();";
                string codigoExpressao = "obj1.propriedadeX;";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasse+codigoCreate);
                compilador.Compilar();

                Expressao exprssResult = new Expressao(codigoExpressao, compilador.escopo);    

                try
                {
                    assercao.IsTrue(exprssResult.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas), codigoClasse + codigoCreate + codigoExpressao);
                }
                catch (Exception ex)
                {
                    string msgError = ex.Message;
                }

            }


            public void TesteInstanciacaoWrapperObjectVector(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoVetor1 = "int[] v1[20];";
                string codigoVetor2 = "Vector v2 [ 20 ]";
                string codigoMatriz1 = "Matriz m1 [ 15, 15 ]";
                string codigoMatriz2 = "double [ , ] m2 = [ 11 , 15 ]";
                string codigoDictionaryText = "DictionaryText d1 = { string }";
                string codigoJaggedArray = "JaggedArray j1 = [ 11 ] [ ]";


                Escopo escopo = new Escopo(codigoVetor1);


                Expressao exprssReturnVetor2 = new Expressao(codigoVetor2, escopo);
                Expressao exprssReturnVetor1 = new Expressao(codigoVetor1, escopo);


                Expressao exprssMatriz_0_1 = new Expressao(codigoMatriz1, escopo);
                Expressao exprssMatriz_0_2 = new Expressao(codigoMatriz2, escopo);

                Expressao exprssJaggedArray = new Expressao(codigoJaggedArray, escopo);

                Expressao exprssDictionaryText = new Expressao(codigoDictionaryText, escopo);





                try

                {

                    assercao.IsTrue(exprssMatriz_0_1.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                    assercao.IsTrue(exprssMatriz_0_1.Elementos[0].tokens.Contains("Create"), codigoMatriz1);


                    assercao.IsTrue(exprssMatriz_0_2.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                    assercao.IsTrue(exprssMatriz_0_2.Elementos[0].tokens.Contains("Create"), codigoMatriz2);

                    assercao.IsTrue(exprssJaggedArray.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                    assercao.IsTrue(exprssJaggedArray.Elementos[0].tokens.Contains("Create"), codigoJaggedArray);

                    assercao.IsTrue(exprssDictionaryText.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                    assercao.IsTrue(exprssDictionaryText.Elementos[0].tokens.Contains("Create"), codigoDictionaryText);

                    assercao.IsTrue(exprssReturnVetor1.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                    assercao.IsTrue(exprssReturnVetor1.Elementos[0].tokens.Contains("Create"), codigoVetor1);


                    assercao.IsTrue(exprssReturnVetor2.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                    assercao.IsTrue(exprssReturnVetor2.Elementos[0].tokens.Contains("Create"), codigoVetor2);

                    assercao.IsTrue(escopo.tabela.GetObjetos().Count == 6, "objetos instanciados");


                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                }
            }


            public void TesteSetterEGettter(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                try
                {


                    string classe_0_0 = "public class MyImage{public Imagem img; public MyImage(){ img=create(" + aspas + "18.png" + aspas + ");img.SetX(img.GetX()+ 7.0);};};";
                    string codigo_0_0 = "MyImage objA= create();";
                    ProcessadorDeID compilador = new ProcessadorDeID(classe_0_0 + codigo_0_0);
                    compilador.Compilar();
                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);

                    assercao.IsTrue(((Objeto)compilador.escopo.tabela.GetObjeto("objA", compilador.escopo).valor != null));
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }

            }
       
          public void TestePropriedadePrivate(AssercaoSuiteClasse assercao)
            {
                

                // teste de verificação de erro de acesso a uma propriedade private.
                try
                {
                    SystemInit.InitSystem();
                    string code_0_0_class = "public class classeB { private int propriedadeA; public classeB() { propriedadeA=5; propriedadeA= propriedadeA+1;};};";
                    string code_0_0_class1 = "public class classeA { public classeB() { classeB obj1= create(); obj1.propriedadeA; };};";


                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_class + code_0_0_class1);
                    compilador.Compilar();

                    assercao.IsTrue(SystemInit.errorsInCopiling.Count > 0, code_0_0_class + "  " + code_0_0_class1);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }


            }

            public void TesteWrapperObjectJaggedArray(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoGet = "m[1][2]";
                string codigoSet = "m[1][1]=5;";

                JaggedArray m = new JaggedArray(20);
                m.SetNome("m");

                Escopo escopo = new Escopo(codigoGet + codigoSet);
                escopo.tabela.RegistraObjeto(m);

                try
                {
                    ExpressaoPorClassificacao expressao = new ExpressaoPorClassificacao();
                    Expressao exprssResultGET = new Expressao(codigoGet, escopo);
                    Expressao exprssResultSET = new Expressao(codigoSet, escopo);


                    assercao.IsTrue(exprssResultGET.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigoGet);
                    assercao.IsTrue(exprssResultSET.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigoSet);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }

            public void TesteChamadasDeMetodosComOperadorAritmetico(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_classeA = "public class classeA{ public int propriedadeA; public classeA(){int b=3;};public int metodoA(int x){ return x;};};";
                    string code_classeB = "public class classeB{public classeA propriedadeB; public classeB(){int y=1;};};";
                    string code_classeC = "public class classeC{public classeB propriedadeC; public classeC(){int x=1;};};";
                    string code_create = "classeC obj1= create();  obj1.propriedadeC.propriedadeB=create();  int x= obj1.propriedadeC.propriedadeB.metodoA(1) + obj1.propriedadeC.propriedadeB.metodoA(2);";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_classeA + code_classeB + code_classeC + code_create);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);

                    

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
                
            }

            public void TestePropriedadeAninhadaEstatica(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                try
                {
                    string code_classeA = "public class classeA{ public classeA(){}; public static int propriedadeEstatica; };";
                    string code_create = "classeA obj1= create(); classeA.propriedadeEstatica;";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_classeA + code_create);
                    compilador.Compilar();

                    assercao.IsTrue(compilador.GetInstrucoes()[1].expressoes[0].Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas), code_classeA + "  " + code_create);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }

            }

            public void TesteMetodoEstatico(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                try
                {
                    string code_classeA = "public class classeA{ public classeA(){}; public static int metodoEstatico(int x){};};";
                    string code_create = "classeA obj1= create(); classeA.metodoEstatico(1);";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_classeA + code_create);
                    compilador.Compilar();

                    assercao.IsTrue(compilador.GetInstrucoes()[1].expressoes[0].Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), code_classeA + "  " + code_create);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }



            }
            public void TesteTextExpressionBase(AssercaoSuiteClasse assercao)
            {
                string code_0_0 = "string textDraw= Prompt.sWrite(" + aspas + "draw..." + aspas + ");";
                Escopo escopo = new Escopo(code_0_0);


                Expressao eprss_0_0 = new Expressao(code_0_0, escopo);

                assercao.IsTrue(eprss_0_0.Elementos.Count == 1 && eprss_0_0.Elementos[0].GetType() == typeof(ExpressaoAtribuicao));
            }


            public void TestesParametrosGetter(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                try
                {
                    string classe_0_0 = "public class MyImage{public Imagem img; public MyImage(){ double xxx= img.GetX()+ 7.0; };};";
                    string codigo_0_0 = "MyImage objA= create();";
                    ProcessadorDeID compilador = new ProcessadorDeID(classe_0_0 + codigo_0_0);
                    compilador.Compilar();

                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasse("MyImage").construtores[0].instrucoesFuncao.Count == 1, classe_0_0);
                }
                catch(Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

            }
    

            public void TesteObjetoActualNoConstrutor(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string codigo_classe1 = "public class MyImage{ public double propriedadeA; public MyImage(){propriedadeA=1.0;};};";
                string codigo_create = "MyImage objA= create();";

                try
                {
                    ProcessadorDeID compilador = new ProcessadorDeID(codigo_classe1 + codigo_create);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("objA", compilador.escopo).propriedades[0].valor.ToString() == "1", codigo_classe1);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }

            }

            public void TesteChamadaDeMetodoSeguidaDeUmaChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoClasseA = "public class classeA { public classeX propriedadeA; public classeA() { int x=1; }  public int metodoA(){ int y= 1; }; };";
                string codigoClasseX = "public class classeX { public classeX() { int y=1; }  public classeA metodoX(int x, int y) { x=2; }; };";
                string codigo_0_0_create = "classeX obj1= create();";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseX + codigoClasseA + codigo_0_0_create);
                compilador.Compilar();

                string codigoExpressao = "obj1.metodoX(1,1).metodoA();";

                Expressao exprssResult = new Expressao(codigoExpressao, compilador.escopo);

                try
                {
                    assercao.IsTrue(exprssResult.Elementos != null &&
                                    exprssResult.Elementos.Count > 0 &&
                                    exprssResult.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo) &&
                                    exprssResult.Elementos[0].Elementos[0].GetType()==typeof(ExpressaoChamadaDeMetodo), 
                                    codigoExpressao);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }


            public void TestePropridadesAninhadasSeguidasDeChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoClasseX = "public class classeX { public classeX() { int y; }  public int metodoX(int x, int y) { int x; }; };";
                string codigoClasseA = "public class classeF { public classeX propriedadeA; public classeF() { int x=1; }  public int metodoA(){ int y= 1; }; };";


                string codeChamadaDeMetodo = "objA.propriedadeA.metodoX(1,1);";




                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseA + codigoClasseX);
                compilador.Compilar();

                Objeto objA = new Objeto("private", "classeF", "objA", null);
                compilador.escopo.tabela.RegistraObjeto(objA);


                Expressao exprssRetorno = new Expressao(codeChamadaDeMetodo, compilador.escopo);

                try
                {
                    assercao.IsTrue(
                        exprssRetorno.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas), codigoClasseA + codigoClasseX + codeChamadaDeMetodo);
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message;
                }
            }


            public void TesteSobrecargaFuncao(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigo_classeA_0_0 = "public class classeA { public int propriedadeA;  public classeA(){int x=1; } public int metodoA(int x){ int i=1;}  public int metodoA(int x, int y){int i=2;} };";
                string codigo_create = "classeA obj1= create();obj1.metodoA();";
               

                ProcessadorDeID compilador = new ProcessadorDeID(codigo_classeA_0_0 + codigo_create);
                compilador.Compilar();

                ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                program.Run(compilador.escopo);



            }

    
            public void TesteOperadorIgual(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string code_0_0_create = "int a=1;int b=1;";
                string code_0_0_express = "a=b+1";
                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_create);
                compilador.Compilar();


                try
                {
                    Expressao eprss_0_0 = new Expressao(code_0_0_express, compilador.escopo);

                    assercao.IsTrue(eprss_0_0.Elementos[0].GetType() == typeof(ExpressaoAtribuicao) &&
                        ((ExpressaoAtribuicao)eprss_0_0.Elementos[0]).exprssObjetoAAtribuir.GetType() == typeof(ExpressaoObjeto));
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

            }
            public void TesteTipoRetornoExpressaoChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {
                string code_0_0_program = "newText=util.ParseFromIntToString(saida);";
                string code_0_0_create = "string newText=" + aspas + "Ola" + aspas + "; int saida=1; FunctionsGame util= create();";

                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_create + code_0_0_program);
                compilador.Compilar();


            }

            public void TesteExpressaoComChamadaDeFuncaoEOperadores(AssercaoSuiteClasse assercao)
            {
                string code_0_0_create = "double inc=1.0; Imagem img= create(" + aspas + "images\\set1\\Human_Church_6.png" + aspas + ");";
                string code_0_0_exprss = "img.GetX()+1.0;";

                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_create);
                compilador.Compilar();

                Expressao exprss_0_0 = new Expressao(code_0_0_exprss, compilador.escopo);

            }
   
   


            public void TesteWrappersObjectVector(AssercaoSuiteClasse assercao)
            {

                SystemInit.InitSystem();

                string codigoGetElement = "v5[1];";
                string codigoSetElement = "v5[1]=5;";

                Vector vector1 = new Vector("int");
                vector1.SetNome("v5");
                vector1.isWrapperObject = true;

                Escopo escopo = new Escopo(codigoGetElement + codigoSetElement);
                escopo.tabela.RegistraObjeto(vector1);

                try
                {


                    Expressao exprssGET = new Expressao(codigoGetElement, escopo);
                    Expressao exprssSET= new Expressao(codigoSetElement, escopo);



                    assercao.IsTrue(exprssGET.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigoGetElement);
                    assercao.IsTrue(exprssSET.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigoSetElement);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

            }
            public void TesteAtribuicaoNumeroNegativoAPropriedadeAninhada(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string code_0_0_class = "public class classeA { public int propriedadeB;  public classeA(int x) { propriedadeB= -1; } };";
                string code_0_0_create = " classeA obj = create (1);";


                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_class + code_0_0_create);
                compilador.Compilar();




            }
   

   

            public void TesteWrapperDictionaryText(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                char aspas = '\u0022';
                string key = aspas + "fruta" + aspas;
                string value = aspas + "maca" + aspas;
                string codigoGet = "m{" + key + "}";
                string codigoSet = "m{" + key + "} =" + value;

                string codigoProgram = "DictionaryText m= {string};";


                ProcessadorDeID compilador = new ProcessadorDeID(codigoProgram);
                compilador.Compilar();

                try
                {
                    Expressao expressaoGET = new Expressao(codigoGet, compilador.escopo);
                    Expressao expressaoSET = new Expressao(codigoSet, compilador.escopo);

                    assercao.IsTrue(expressaoGET.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigoSet);
                    assercao.IsTrue(expressaoSET.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigoGet);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }


            }



   

            public void TesteAtribuicaoPorExpressaoChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_compile_0_0 = " int saidaParams= 5; LoopGame loop= create()";
                    string code_exprss_0_0 = "saidaParams= loop.CloseWindow();";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_compile_0_0);
                    compilador.Compilar();


                    Expressao exprss_0_0 = new Expressao(code_exprss_0_0, compilador.escopo);

                    EvalExpression eval = new EvalExpression();
                    object result = eval.EvalPosOrdem(exprss_0_0, compilador.escopo);


                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }


            }



            public void TesteExpressaoOperadoresEmParameetros(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                //preparacao para testes unitarios em massa.
                string codigoClasseA = "public class classeM { public int propriedadeA;  public classeM() { };  public int metodoA(int a, int b ){ int x =1; x=x+1;}; }";
                string codigoCreate = "int x=1; int y= 1; classeM objA= create();";
                string codigoExpressao = "objA.metodoA(1,1)";

                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseA + codigoCreate);
                compilador.Compilar();


                Expressao exprss = new Expressao(codigoExpressao, compilador.escopo);
                try
                {
                    assercao.IsTrue(exprss.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "FATAL ERROR em validar os resultados." + e.Message);
                }

            }



 




            public void TesteExpressaoCondicionalComConectivos(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoExpressao = "((a<1) && (b>2) || (a==2))";
                Escopo escopoTeste = new Escopo(codigoExpressao);
                escopoTeste.tabela.AddObjeto("private", "a", "int", 1, escopoTeste);
                escopoTeste.tabela.AddObjeto("private", "b", "int", 2, escopoTeste);

                Expressao exprssResult = new Expressao(codigoExpressao, escopoTeste);
                try
                {

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

            }


            public void TesteExpressaoEstaticaComAtribuicao(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string codigo_0_0 = "x = string.Index(" + aspas + "numero" + aspas + ", " + aspas + "num" + aspas + ")";
                Objeto x = new Objeto("private", "int", "x", 0);

                Escopo escopo = new Escopo(codigo_0_0);
                escopo.tabela.RegistraObjeto(x);


                Expressao exprssAtribuicao = new Expressao(codigo_0_0, escopo);
                try
                {
                    ExpressaoAtribuicao exprssAtribui = (ExpressaoAtribuicao)exprssAtribuicao.Elementos[0];
                    assercao.IsTrue(((ExpressaoChamadaDeMetodo)exprssAtribui.ATRIBUICAO.Elementos[0]).parametros.Count == 2);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }







            public void TesteExpressaoOperadores(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigo = "a+b*c";
                Objeto a = new Objeto("private", "int", "a", 1);
                Objeto b = new Objeto("private", "int", "b", 1);
                Objeto c = new Objeto("private", "int", "c", 1);
                Escopo escopo = new Escopo(codigo);
                escopo.tabela.RegistraObjeto(a);
                escopo.tabela.RegistraObjeto(b);
                escopo.tabela.RegistraObjeto(c);


                try
                {

                    Expressao exprssResult = new Expressao(codigo, escopo);

                    assercao.IsTrue(exprssResult.Elementos.Count == 5, codigo);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

            }




            public void TesteExpressaoEntreParentesis(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigo = "(a)";
                Objeto obj1 = new Objeto("private", "int", "a", 1);
                Escopo escopo = new Escopo(codigo);
                escopo.tabela.RegistraObjeto(obj1);

                try
                {
                    ExpressaoPorClassificacao expressao = new ExpressaoPorClassificacao();
                    List<Expressao> exprssResult = expressao.ExtraiExpressoes(codigo, escopo);
                    assercao.IsTrue(((ExpressaoEntreParenteses)exprssResult[0]).exprssParenteses.tokens[0] == "a");
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }


            }


            public void TesteWrapperObjectMatriz(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoGet = "m1[1,5]";
                string codigoSet = "m1[1,5]=1";

                Matriz matriz1 = new Matriz(10, 10);
                matriz1.SetNome("m1");
                matriz1.isWrapperObject = true;

                Objeto x = new Objeto("private", "int", "x", 1);
                Objeto y = new Objeto("private", "int", "y", 1);

                Escopo escopo = new Escopo(codigoGet + codigoSet);
                escopo.tabela.RegistraObjeto(matriz1);
                escopo.tabela.RegistraObjeto(x);
                escopo.tabela.RegistraObjeto(y);

                try
                {
                    ExpressaoPorClassificacao expressao = new ExpressaoPorClassificacao();
                    List<Expressao> exprssResultSET = expressao.ExtraiExpressoes(codigoSet, escopo);
                    List<Expressao> exprssResultGET = expressao.ExtraiExpressoes(codigoGet, escopo);

                    assercao.IsTrue(exprssResultGET[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                    assercao.IsTrue(exprssResultSET[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }

  
            public void TesteExtracaoDeExpressoesIndependentes(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string codigo = "a+b(x,y), b+1, a++; x; (x+1)";
                Escopo escopo = new Escopo(codigo);

                Objeto a = new Objeto("private", "int", "a", 1);
                Objeto b = new Objeto("private", "int", "b", 1);
                Objeto x = new Objeto("private", "int", "x", 1);
                Objeto y = new Objeto("private", "int", "y", 1);

                escopo.tabela.RegistraObjeto(a);
                escopo.tabela.RegistraObjeto(b);
                escopo.tabela.RegistraObjeto(x);
                escopo.tabela.RegistraObjeto(y);



                ExpressaoPorClassificacao expressao = new ExpressaoPorClassificacao();
                List<Expressao> expressoes = expressao.ExtraiMultipasExpressoesIndependentes(codigo, escopo);

                try
                {
                    assercao.IsTrue(expressoes.Count == 5);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                }
            }





   


            public void TesteClassesStringEDouble(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string code = "a.tan();";
                Escopo escopo = new Escopo(code);
                Objeto obj1 = new Objeto("private", "double", "a", 1.0);
                escopo.tabela.RegistraObjeto(obj1);

                try
                {
                    ExpressaoPorClassificacao expressao = new ExpressaoPorClassificacao();
                    List<Expressao> exprssResult = expressao.ExtraiExpressoes(code, escopo);
                    assercao.IsTrue(exprssResult[0].GetType() == typeof(ExpressaoChamadaDeMetodo) && ((ExpressaoChamadaDeMetodo)exprssResult[0]).objectCaller.nome == "a", code);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:" + ex.Message);
                }



            }









            public void TesteChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoClasseX = "public class classeX { public classeX() { int y; }  public int metodoX(int x, int y) { int x; }; }";
                string codigoClasseA = "public class classeA { public classeX propriedadeA; public classeA() { int x=1; }  public int metodoA(){ int y= 1; }; };";

                string codeExpression = "obj.metodoX(1,1);";



                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseX + codigoClasseA);
                compilador.Compilar();

                Objeto obj = Objeto.BuildObject("obj", "classeX");
                Escopo escopo = new Escopo(codeExpression);
                escopo.tabela.RegistraObjeto(obj);


                ExpressaoPorClassificacao expressao = new ExpressaoPorClassificacao();
                List<Expressao> exprssResult = expressao.ExtraiExpressoes(codeExpression, escopo);

                try
                {
                    assercao.IsTrue(exprssResult[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                }
                catch (Exception ex)
                {
                    string msgError = ex.Message;
                }
            }


            public void TesteClasse(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoCreate = "int obj1=5;";
                string codigoExpressao = "obj1;";

                ProcessadorDeID compilador = new ProcessadorDeID(codigoCreate);
                compilador.Compilar();

                Expressao exprssRetorno = new Expressao(codigoExpressao, compilador.escopo);

                try
                {
                    assercao.IsTrue(exprssRetorno.Elementos[0].GetType() == typeof(ExpressaoObjeto), codigoCreate + "  " + codigoExpressao);
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }

            }

            public void TesteLiteral(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string codigoCreate = "int x=5;";
                string codeExpressao = aspas + "isso e texto" + aspas;

                ProcessadorDeID compilador = new ProcessadorDeID(codigoCreate);
                compilador.Compilar();

                Expressao exprss = new Expressao(codeExpressao, compilador.escopo);
                try
                {
                    assercao.IsTrue(exprss.Elementos[0].GetType() == typeof(ExpressaoLiteralText), codigoCreate + "  " + codeExpressao);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }


            public void TesteNumero(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string codigoCreate = "double x=1.0";
                string codigoExpressao = "1000.5";

                ProcessadorDeID compilador = new ProcessadorDeID(codigoCreate);
                compilador.Compilar();

                Expressao exprss = new Expressao(codigoExpressao, compilador.escopo);

                try
                {
                    assercao.IsTrue(exprss.Elementos[0].GetType() == typeof(ExpressaoNumero), codigoCreate + "  " + codigoExpressao);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }



      
        }


        public class Testes2 : SuiteClasseTestes
        {
            private string aspas = "\"";
            public Testes2() : base("new tests for expresions")
            {

            }


            // plano de testes para getter/setters, em classes orquidea.
            public void PlanoDeTestesGetterSettersComClassesEmVector(AssercaoSuiteClasse assercao)
            {

                try
                {
                    SystemInit.InitSystem();

                    string code_class_entity = "public class entity{" +
                    " public int prop1;" +
                    " public entity(int x) {prop1= x;};" +
                     "public int getProp(){ return prop1;};" +
                     "public void setProp(int x){prop1=x;};" +
                     "};";

                    string code_class_planet = "public class planet{" +
                         "public entity[] elems; " +
                         "public int actualProp;" +
                         "public planet(){ " +
                                    "elems.Clear();" +
                                    "entity e= create(3);" +
                                    "elems.Append(e);" +
                                    "actualProp=3;" +
                                    "};" +
                        "public void setPlanetProp(int x)" +
                        "{" +
                            "elems[0].setProp(x);" +
                        "}" +
                        "};";


                    string code_create = "planet p= create();";
                    string code_create_2 = "int y= p.elems[0].getProp(); p.setPlanetProp(11);";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_class_entity + code_class_planet + code_create + code_create_2);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);


                    Vector vtResult = (Vector)compilador.escopo.tabela.GetObjeto("p", compilador.escopo).propriedades[0].valor;
                    Objeto elems = (Objeto)vtResult.GetElement(0);



                    // teste getter .
                    assercao.IsTrue(
                        compilador.escopo.tabela.GetObjeto("p", compilador.escopo).propriedades[1].valor != null &&
                        compilador.escopo.tabela.GetObjeto("p", compilador.escopo).propriedades[1].valor.ToString() == "3", code_class_entity + code_class_planet + code_create + code_create_2);


                    // teste setter com elementos de vector;    
                    assercao.IsTrue(((Objeto)elems).propriedades[0].valor.ToString() == "11", code_class_entity + code_class_planet + code_create + code_create_2);


                    // teste de getter via elemento de vector.
                    assercao.IsTrue(
                        compilador.escopo.tabela.GetObjeto("y", compilador.escopo).valor.ToString() == "3", code_class_entity + code_class_planet + code_create + code_create_2);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }

            }



            // plano de testes para propriedades estáticas.
            public void PlanoDeTestesPropriedadesEstaticas(AssercaoSuiteClasse assercao)
            {

                try
                {
                    SystemInit.InitSystem();
                    /// 1- definicao de uma proppriedade estatica;(v)
                    /// 2- setar um valor de propriedade estatica; (v) 
                    /// 3- operacoes aritmeticas,
                    /// 4- condicionais (com if/else); (falha em operacao aritmetica);
                    /// 5- ler o valor de uma propriedade estática; (v);

                    string code_class_entity = "public class entity{" +
                        "public static int prop1;" +
                        "public static int prop2;" +
                        " public entity(){" +
                            "prop1=1;" +
                            "prop2=4;" +
                            "};" +
                        "};";

                    string code_class_factory = "public class factory{" +
                        " public factory(){entity.prop1=5;};" +
                        "public int add(){ " +
                                "return entity.prop1+entity.prop2;" +
                                " };" +
                        "public int getProp1(){" +
                            "return entity.prop1;" +
                                            "};" +
                        "public void setProp2(int x){" +
                                "entity.prop2=x;" +
                                "};" +
                        "public int getEstado()" +
                           "{" +
                                "if (entity.prop2>=4){" +
                                    "return 1;" +
                                    "}" +
                                 "else {" +
                                 "return 0;};" +
                            "};" +
                        "};";

                    string code_create = "factory f= create();int yt=f.getEstado(); int x= f.getProp1(); int soma= f.add(); int t=entity.prop1; f.setProp2(7); ";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_class_entity + code_class_factory + code_create);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);


                    // teste de operacoes condicionais com propriedades estaticas.
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("yt", compilador.escopo).valor.ToString() == "1", code_class_entity + code_class_factory + code_create);

                    // teste de instanciacao de propriedades estáticas.
                    assercao.IsTrue(Escopo.escopoROOT.tabela.GetObjeto("prop1", Escopo.escopoROOT) != null, code_class_entity + code_class_factory + code_create);

                    // teste de ler uma propriedade estática.
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("t", compilador.escopo).valor.ToString() == "5", code_class_entity + code_class_factory + code_create);

                    // teste de setar uma propriedade estática.
                    assercao.IsTrue(Escopo.escopoROOT.tabela.GetObjeto("prop2", Escopo.escopoROOT).valor.ToString() == "7", code_class_entity + code_class_factory + code_create);

                    // teste de operacoes aritmeticas com propriedades estáticas.
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("soma", compilador.escopo).valor.ToString() == "8", code_class_entity + code_class_factory + code_create);



                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }




            // plano de testes para chamadas de metodo.
            public void PlanoDeTestesChamadasDeMetodo(AssercaoSuiteClasse assercao)
            {
                /*
                 *  (x)1 chamada de metodo;
			        (x) operacoes aritmeticas, condicionais (principalmente isso), com chamadas de metodo operacoes condicionais.
			        (x) chamadas de metodo em expressoes condicionais if/else;
			        (x) chamadas de metodo  com parametro de outra chamada de metodo.
			        (x) parametros de umObjeto+umaChamadaDeMetodo, em outra chamada de metodo.
			        (x) parametros de um numero,string+ umaChamadaDeMetodo, em outra chamada de metodo.
                 */
                SystemInit.InitSystem();
                try
                {
                    string code_class = "public class entity" +
                        " {" +
                        " public int x;" +
                        " public int y; " +
                        " public entity(){x=1; y=5;};" +
                        " public string GetTexto(){ return" + aspas + "homero" + aspas + "};" +
                        " public void metodoB(){};" +
                        " public int add(int x){return x;};" +
                        " public int mult(int w){return w*x;};" +
                        " public int getState(){ return 1;};" +
                        " public int updateEntity(){ if (getState()==1){ return 1;} return 0;};" +
                        "};";
                    string code_create = "entity e= create(); " +
                                         "string texto=" + aspas + "ola " + aspas + "+ e.GetTexto();" +
                                         "int tx=e.add(e.mult(5)+7);" +
                                         "int by=e.add(e.updateEntity());" +
                                         "int j=e.add(1); int t=e.mult(5)+e.add(1);" +
                                         "int b=t+j; int ty=e.updateEntity(); ";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_class + code_create);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);

                    // texto com constante+ chamada de metodo.
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("texto", compilador.escopo).valor.ToString() == "ola homero", code_class + code_create);
                    // parametros: 1 chamada de metodo + 1 objeto.
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("tx", compilador.escopo).valor.ToString() == "12", code_class + code_create);
                    // chamadas de metodo como parametro de outra chamada de metodo.
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("by", compilador.escopo).valor.ToString() == "1", code_class + code_create);
                    // 1 chamada de metodo.
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("j", compilador.escopo).valor.ToString() == "1", code_class + code_create);
                    // expressoes aritmeticas com chamada de metodo.
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("t", compilador.escopo).valor.ToString() == "6", code_class + code_create);
                    // resultados com expressoes aritmeticas feitas com chamada de metodos.
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("b", compilador.escopo).valor.ToString() == "7", code_class + code_create);
                    // expressoes codificionais com chamadas de metodo.
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("ty", compilador.escopo).valor.ToString() == "1", code_class + code_create);


                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }


            // plano de testes para cenario com propriedades aninhadas.
            public void PlanoDeTestesPropriedadesAninhadas(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                try
                {
                    string code_class = "public class entity" +
                        " {" +
                        " public static int b;" +
                        " public int x;" +
                        " public int y; " +
                        " public entity(){x=1; y=5;};" +
                        " public void metodoB(){};" +
                        " public int add(){return 1;};" +
                        "};";
                    string code_create = "entity e= create(); e.x=6; int w= e.x+e.y; entity.b=5; int t=e.add()+5;";


                    ProcessadorDeID compilador = new ProcessadorDeID(code_class + code_create);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("t", compilador.escopo).valor.ToString() == "6", code_class + code_create);

                    // teste de propriedade estatica.
                    assercao.IsTrue(Escopo.escopoROOT.tabela.GetObjeto("b", Escopo.escopoROOT).valor.ToString() == "5", code_class + code_create);

                    // teste de instanciacao de propriedades. 
                    assercao.IsTrue(((Objeto)compilador.escopo.tabela.GetObjeto("e", compilador.escopo)).propriedades[1].valor.ToString() == "5", code_class + code_create);

                    // teste de setar uma propriedde aninhada pelo objeto instanciado.
                    assercao.IsTrue(((Objeto)compilador.escopo.tabela.GetObjeto("e", compilador.escopo)).propriedades[0].valor.ToString() == "6", code_class + code_create);

                    // teste de retorno de tipo de propriedade.
                    assercao.IsTrue(((Objeto)compilador.escopo.tabela.GetObjeto("e", compilador.escopo)).propriedades[0].tipo == "int", code_class + code_create);

                    // teste de operacao matemattica de string.
                    assercao.IsTrue(((Objeto)compilador.escopo.tabela.GetObjeto("w", compilador.escopo)).valor.ToString() == "11", code_class + code_create);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }


            }


            public void TestePropriedadeDeElementoDeVector(AssercaoSuiteClasse assercao)
            {

                try
                {
                    SystemInit.InitSystem();
                    string code_class_meteor = "public class meteor {" +
                        "public int prop1;" +
                        " public meteor(int t){prop1=4;};" +
                        "public void Update(){ prop1=5;};" +
                        "};";

                    string code_class_ship = "public class ship {" +
                        "public meteor[] elementos;" +
                        "public ship(){" +
                                "elementos.Clear();" +
                                "meteor mt= create(1);" +
                                "elementos.Append(mt);" +
                            "};" +
                        "public int updateShip(){" +
                                "return elementos[0].prop1;" +
                            "};" +
                          "};";

                    string code_create = "ship objShip= create(); int y= objShip.updateShip();";



                    ProcessadorDeID compilador = new ProcessadorDeID(code_class_meteor + code_class_ship + code_create);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);

                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("y", compilador.escopo).valor.ToString() == "4", code_class_meteor + code_class_ship + code_create);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }


            }





            public void TestePropriedadesEstaticasCom2Classes(AssercaoSuiteClasse assercao)
            {

                SystemInit.InitSystem();
                try
                {
                    string code_classMeteor = "public class Meteor {" +
                    "public int isDestroyed; " +
                    "public Meteor(){isDestroyed=1;};};";

                    string code_classShip = "public class Ship{ " +
                        "public int crash= 3; " +
                        "public Ship(){crash=0;};" +
                        "public void UpdateShip()" +
                        "{" +
                        "   Meteor m= create(); if (m.isDestroyed==1){crash=1;};" +
                        "};" +

                        "};";

                    string code_create = "Ship e= create(); for (int i=0;i<1000;i++){ e.UpdateShip();};";



                    ProcessadorDeID compilador = new ProcessadorDeID(code_classMeteor + code_classShip + code_create);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);

                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("e", compilador.escopo).propriedades[0].valor.ToString() == "1", code_classMeteor + code_classShip + code_create);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }


            }

 
    

  


        }

    }

}
