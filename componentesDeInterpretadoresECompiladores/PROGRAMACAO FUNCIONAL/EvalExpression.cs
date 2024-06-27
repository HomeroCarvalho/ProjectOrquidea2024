using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using MathNet.Numerics;
using parser.ProgramacaoOrentadaAObjetos;
using static parser.SuiteClasseTestes;

namespace parser
{

    /// <summary>
    /// AVALIADOR de expressoes: expressoes aritmeticas, chamadas de metodo, atribuicoes, propriedades aninhadas,
    /// operadores binarios, unarios, condicionais, expressoes numero, literal, etc...
    /// </summary>
    public class EvalExpression
    {

        public static EvalExpression UM_EVAL = new EvalExpression();

        /// <summary>
        /// avalia uma expressao. colaca em pos-ordem a expressao, se não já estiver.
        /// </summary>
        /// <param name="expss_">expressao a ser avaliada.</param>
        /// <param name="escopo">contexto onde a expressão está. contem dados de objetos, funcoes, relacionados a expressao, também.</param>
        /// <returns></returns>
        public object EvalPosOrdem(Expressao expss_, Escopo escopo)
        {
            // converte a expressao para pos-ordem.
            if (!expss_.isPosOrdem)
            {

                expss_.PosOrdemExpressao();
                expss_.isPosOrdem = true;
            }



            expss_.isModify = true;
            object valorExpressao = null;

            valorExpressao = this.Eval(expss_, escopo);

            expss_.oldValue = valorExpressao;

            return valorExpressao;


        } // EvalPosOrdem()


        /// <summary>
        /// avaliador de expressoes em pos-ordem.
        /// </summary>
        /// <param name="expss">expressao a avaliar.</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        /// <returns></returns>
        protected object Eval(Expressao expss, Escopo escopo)
        {
            

            if (Expressao.Instance.IsNumero(expss.ToString()))
                return Expressao.Instance.ConverteParaNumero(expss.ToString(), escopo);


            object result1 = 0;
            Pilha<object> pilhaOperandos = new Pilha<object>("pilhaOperandos");


            Pilha<Objeto> pilhaObjetos = new Pilha<Objeto>("pilha de Objetos em processamento.");
            


            // PROCESSAMENTO EXCEPCIONAL DE EXPRESSÕES SEM WRAPPER DE EXPRESSAO.
            if (expss.Elementos.Count == 0)
            {
                if (expss.typeExprss==Expressao.typeATRIBUICAO)
                {

                    Expressao exprssWrapper = new Expressao();
                    exprssWrapper.Elementos.Add(expss);

                    EvalExpression eval = new EvalExpression();
                    return eval.EvalPosOrdem(exprssWrapper, escopo);
                }

                if ((expss.typeExprss == Expressao.typeEXPRESSAO) && (expss.tokens != null) && (expss.tokens.Count > 0)) 
                {
                    if (escopo.tabela.GetObjeto(expss.tokens[0].ToString(), escopo) != null)
                    {
                        return escopo.tabela.GetObjeto(expss.tokens[0].ToString(), escopo);
                    }
                    else
                    {
                        return expss.tokens[0];
                    }
                    
                }
                
                // constante string formando um unico token.
                if (expss.typeExprss == Expressao.typeLITERAL)
                {
                    return ((ExpressaoLiteralText)expss).literalText;
                }
                else
                // constante numero formando um unico token.
                if (ExpressaoNumero.isNumero(expss.ToString()))
                {
                    bool isFoundANumber = false;
                    object numero = null;
                    string str_numero = expss.ToString();
                    GetNumber(ref numero, ref isFoundANumber, str_numero, escopo);
                    if (isFoundANumber)
                    {
                        pilhaOperandos.Push(numero);                        
                    }

                }
                else 
                // um objeto somente, sem sub-expressoes.
                if (expss.typeExprss == Expressao.typeOBJETO)
                {
                    return ((ExpressaoObjeto)expss).objectCaller.GetValor();
                }
                
                    
            }

            

            
            for (int x = 0; x < expss.Elementos.Count; x++)
            { if (expss.Elementos[x] != null)
                {
                    // EXPRESSAO LITERAL TEXT.
                    if (expss.Elementos[x].typeExprss== Expressao.typeLITERAL)
                    {
                        ExpressaoLiteralText exprssLiteral = (ExpressaoLiteralText)expss.Elementos[x];
                        pilhaOperandos.Push(exprssLiteral.literalText);
                    }

                    // EXPRESSAO NILL.
                    if (expss.Elementos[x].typeExprss == Expressao.typeNILL)
                    {
                        object elementoAVerificar = pilhaOperandos.Pop();
                        if (elementoAVerificar == null)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    }
                    else
                    // EXPRESSAO ATRIBUICAO.
                    if (expss.Elementos[x].typeExprss == Expressao.typeATRIBUICAO)
                    {
                        ExpressaoAtribuicao exprssAtribui = ((ExpressaoAtribuicao)expss.Elementos[x]);

                        // valor a atribuir, calculado.
                        object valorAtribuir = EvalPosOrdem(exprssAtribui.ATRIBUICAO, escopo);
                        // objeto a receber a atribuição.
                        Objeto objetoAtribuicao = exprssAtribui.objetoAtribuir;

                        // objeto resultado da operação de atribuição.
                        Objeto objetoResult = null;

                        // o objeto A RECEBER ATRIBUICAO é UMA PROPRIEDADE ESTÁTICA.
                        if ((objetoAtribuicao != null) && (objetoAtribuicao.isStatic))
                        {
                            Objeto umaPropriedadeEstatica = Escopo.escopoROOT.tabela.GetObjeto(objetoAtribuicao.nome, Escopo.escopoROOT);
                            if (umaPropriedadeEstatica != null)
                            {
                                umaPropriedadeEstatica.valor = valorAtribuir;
                                Escopo.escopoROOT.tabela.UpdateObjeto(umaPropriedadeEstatica);
                            }
                        }
                        else
                        /// o objeto A RECEBER ATRIBUICAO é um  WRAPPER DATA OBJECT.
                        if ((objetoAtribuicao != null) && (objetoAtribuicao.isWrapperObject))
                        {
                            objetoResult = WrapperData.SetValor(objetoAtribuicao, valorAtribuir);

                            // se o valor do objeto result.
                            Objeto obj = escopo.tabela.GetObjeto(objetoResult.nome, escopo);
                            if (obj != null)
                            {
                                obj.SetValor(valorAtribuir);
                            }
                            
                            //  atualiza o escopo.
                            escopo.tabela.UpdateObjeto(objetoAtribuicao);

                        }
                        else
                        // o objeto A RECEBER ATRIBUICAO É do tipo PROPRIEDADES ANINHADAS.
                        if (exprssAtribui.exprssObjetoAAtribuir.typeExprss == Expressao.typePROPRIEDADES_ANINHADADAS)
                        {
                            ExpressaoPropriedadesAninhadas exprsAninhadas = (ExpressaoPropriedadesAninhadas)exprssAtribui.exprssObjetoAAtribuir;

                            if ((exprsAninhadas != null) && (exprsAninhadas.objectCaller != null))
                            {
                                Objeto objetoCaller = escopo.tabela.GetObjeto(exprsAninhadas.objectCaller.nome, escopo);
                                if ((objetoCaller != null) && (exprsAninhadas.aninhamento != null) && (exprsAninhadas.aninhamento.Count > 0)) 
                                {
                                    string nomeProp = exprsAninhadas.aninhamento[exprsAninhadas.aninhamento.Count - 1].nome;
                                    string tipoProp = exprsAninhadas.aninhamento[exprsAninhadas.aninhamento.Count - 1].tipo;
                                    objetoCaller.UpdatePropriedade(nomeProp, tipoProp, valorAtribuir);
                                    
                                    // atualiza o objeto caller.
                                    escopo.tabela.UpdateObjeto(objetoCaller);
                                }
                            }

                            // obtem qual propriedadade será atualizada.
                            objetoAtribuicao = exprsAninhadas.aninhamento[exprsAninhadas.aninhamento.Count - 1];
                            objetoAtribuicao.valor = valorAtribuir;


                            //  atualiza o escopo.
                            escopo.tabela.UpdateObjeto(objetoAtribuicao);






                        }
                        else
                        // O objeto de RETORNO é do tipo OBJETO.
                        if (exprssAtribui.exprssObjetoAAtribuir.typeExprss == Expressao.typeOBJETO)
                        {
                            objetoAtribuicao.valor = valorAtribuir;

                            if (escopo.tabela.GetObjeto(objetoAtribuicao.nome, escopo) != null)
                            {
                                escopo.tabela.UpdateObjeto(objetoAtribuicao);
                            }
                            else
                            {
                                escopo.tabela.RegistraObjeto(objetoAtribuicao);
                            }
                            objetoResult = objetoAtribuicao;
                       
                        }
                        else
                        // O objeto VEM DE OBJETO mas não exatamente um Objeto.
                        if (objetoAtribuicao is Objeto)
                        {
                            objetoResult.valor = valorAtribuir;
                            escopo.tabela.UpdateObjeto(objetoResult);
                        }

                        // guarda na pilha de processamento de expressoes.
                        if (objetoResult != null)
                        {
                            pilhaOperandos.Push(objetoResult);
                        }
                        // SENAO, retorna o valor sem atribuir a algum objeto.
                        else
                        {
                            return valorAtribuir;
                        }
                    }
                    // EXPRESSAO PROPRIEDADES ANINHADAS.
                    if (expss.Elementos[x].typeExprss == Expressao.typePROPRIEDADES_ANINHADADAS)
                    {
                        // faz o processamento basico de uma propriedade aninhada, ex.: "a.propriedadeB";
                        ExpressaoPropriedadesAninhadas exprss_aninhadas = (ExpressaoPropriedadesAninhadas)expss.Elementos[x];


                        // PROCESSAMENTO DE PROPRIEDADES ANINHADAS ESTÁTICAS.
                        if ((exprss_aninhadas != null) && (exprss_aninhadas.objectCaller.isStatic))
                        {
                            string nomePropEstatica = exprss_aninhadas.aninhamento[exprss_aninhadas.aninhamento.Count - 1].nome;
                            Objeto objPropEstatica = Escopo.escopoROOT.tabela.GetObjeto(nomePropEstatica, Escopo.escopoROOT);
                            if (objPropEstatica != null)
                            {
                                pilhaOperandos.Push(objPropEstatica.valor);
                            }
                            
                        }

                        // objeto actual pode conter dados atualizados das propriedades aninhadas, se for o objeto caller do aninhamento


                        int countAnimnhamento = exprss_aninhadas.aninhamento.Count - 1;
                        //_________________________________________________________
                        // obtem a ultima propriedade do aninhamento. a fim de atualizar a propria expressao propriedades.
                        Objeto propriedade = exprss_aninhadas.aninhamento[countAnimnhamento];
                        // ATUALIZA a [propriedade]
                        propriedade = escopo.tabela.GetObjeto(propriedade.nome, escopo);

                        //_________________________________________________________


                     

                        if (propriedade != null)
                        {

                            // PROCESSAMENTO DE PROPRIEDADES ANINHADAS A PARTIR DO OBJETO CALLER.
                            Objeto propriedadeVindaDoEscopo = escopo.tabela.GetObjeto(propriedade.nome, escopo);
                            if ((propriedadeVindaDoEscopo != null) && (propriedadeVindaDoEscopo.valor != null))
                            {

                                pilhaOperandos.Push(propriedadeVindaDoEscopo.valor);


                            }




                        }
                     

                        Objeto objOBjCallerNEXT = null;
                        bool isVectorObjectCaller = false;
                        Expressao INDEX_VECTOR = null;
                        Vector vtRESULT = null;

                        //  processamento de  UMA  CHAMADA DE METODO ANINHADA com PROPRIEDADES ANINHADAS, com objeto caller do aninhamento de propriedades aninhadas.
                        if ((exprss_aninhadas.Elementos != null) &&
                         (exprss_aninhadas.Elementos.Count > 0) &&
                         (exprss_aninhadas.Elementos[0].typeExprss == Expressao.typeCHAMADA_METODO))
                        {

                            // PROCESSAMENTO DE CHAMADAS DE METODO, ANINHADOS A PROPRIEDADE ANINHADA.
                            for (int k = 0; k < exprss_aninhadas.Elementos.Count; k++)
                            {
                                ExpressaoChamadaDeMetodo exprssAdicional = (ExpressaoChamadaDeMetodo)exprss_aninhadas.Elementos[k];

                                if (k == 0)
                                {
                                    // condicao especial de uma chamada de metodo adicional, envolvendo elementos de um vector.
                                    isVectorObjectCaller = exprssAdicional.objectCaller.tipo == "Vector"; 
                                    objOBjCallerNEXT = exprssAdicional.objectCaller;
                                    if ((exprssAdicional.parametros != null) && (exprssAdicional.parametros.Count > 0))
                                    {
                                        INDEX_VECTOR = exprssAdicional.parametros[0];
                                    }

                                    if ((exprssAdicional.objectCaller != null) && (exprssAdicional.objectCaller.GetType() == typeof(Vector)))
                                    {

                                        vtRESULT = (Vector)escopo.tabela.GetObjeto(exprssAdicional.objectCaller.nome, escopo).valor;
                                    }
                                }

                                Metodo fnc = exprssAdicional.funcao;
                                if ((fnc.instrucoesFuncao == null) || (fnc.instrucoesFuncao.Count == 0))
                                {
                                    Classe classeDaFuncao = RepositorioDeClassesOO.Instance().GetClasse(fnc.nomeClasse);
                                    List<Metodo> metodo = classeDaFuncao.GetMetodo(fnc.nome);
                                    fnc.instrucoesFuncao = metodo[0].instrucoesFuncao;
                                }



                                List<Expressao> parametros = exprssAdicional.parametros;
                                if ((parametros == null) || (parametros.Count == 0))
                                {
                                    parametros = new List<Expressao>();
                                }

                                object result = fnc.ExecuteAMethod(parametros, escopo, ref objOBjCallerNEXT, Metodo.IS_METHOD);
                                pilhaOperandos.Push(result);


                                
                                if ((result != null) && (result.GetType() == typeof(Objeto)))
                                {
                                    objOBjCallerNEXT = (Objeto)result;
                                }

                                

                                // é preciso atualizar o vector, cujo elemento foi objeto caller da chamada de metodo adicional.
                                if ((k == 1) && (isVectorObjectCaller))  
                                {
                                    // OBTEM O INDICE DO ELEMENTO VECTOR, que é o objeto caller da chamada de metodo adicional.
                                    object indexElementVector = EvalExpression.UM_EVAL.EvalPosOrdem(INDEX_VECTOR, escopo);
                                    // OBTEM O VETOR DA CHAMADA DE METODO PRIMEIRA.
                                    // ATUALIZA O VETOR, CUJO ELMEENTO FOI OBJETO CALLER DA CHAMADA ADICIONAL.
                                    vtRESULT.SetElement(indexElementVector, objOBjCallerNEXT);


                                    // ATUALIZA O OBJETO CALLER DA PROPRIEDADE ANINHADA
                                    Objeto objCaller = exprss_aninhadas.objectCaller;
                                    if ((objCaller.propriedades!=null) && (objCaller.propriedades.Count > 0))
                                    {
                                        for (int i = 0; i < objCaller.propriedades.Count; i++)
                                        {
                                            if ((objCaller.propriedades[i] != null) && (objCaller.propriedades[i].nome == vtRESULT.nome))
                                            {
                                                objCaller.propriedades[i].valor = vtRESULT;
                                            }
                                        }
                                        escopo.tabela.UpdateObjeto(objCaller);
                                    }
                                    // atualiza o escopo.
                                    escopo.tabela.UpdateObjeto(objOBjCallerNEXT);


                                }


                            }

                            continue;

                        }


                    }
                    else
                    // EXPRESSAO CHAMADA DE METODO.
                    if (expss.Elementos[x].typeExprss == Expressao.typeCHAMADA_METODO)
                    {


                        // forma a chamada de metodo inicial.
                        ExpressaoChamadaDeMetodo umaChamadaDeMetodo = (ExpressaoChamadaDeMetodo)expss.Elementos[x];



                        List<Expressao> exprssParametros = umaChamadaDeMetodo.parametros;
                        if (exprssParametros == null)
                        {
                            exprssParametros = new List<Expressao>();
                        }


                        // condicao especial de uma chamada de metodo adicional, envolvendo elementos de um vector.
                        bool isVectorObjectCaller = umaChamadaDeMetodo.objectCaller.tipo == "Vector";



                        // executa a chamada de metodo.
                        Metodo fnc = umaChamadaDeMetodo.funcao;

                        


                        object result = fnc.ExecuteAMethod(exprssParametros, escopo, ref umaChamadaDeMetodo.objectCaller, Metodo.IS_METHOD);
                        pilhaOperandos.Push(result);

                        


                        // PROCESSAMENTO DE CHAMADAS DE METODO ANINHADOS,. ex: "a.metodoA(x,y).metodoB(a)", ".metodoB" é uma chamada de metodo adicional. 
                        if ((umaChamadaDeMetodo.Elementos != null) && (umaChamadaDeMetodo.Elementos.Count > 0))
                        {

                            Objeto objRESULT = null;
                            if ((result != null) && (result.GetType() == typeof(Objeto)))
                            {
                                objRESULT = ((Objeto)result).Clone();
                                escopo.tabela.UpdateObjeto(objRESULT);
                            }

                            int i = 0;
                            // faz o processamento de chamadas de metodos adicionais.
                            while (i < umaChamadaDeMetodo.Elementos.Count)
                            {
                                // PROPRIEDADE ANINHADA ADICIONAIS:  caso primeiro: uma chamada GetElement de Vector, seguida de propriedade.
                                if (umaChamadaDeMetodo.Elementos[i].typeExprss == Expressao.typePROPRIEDADES_ANINHADADAS)
                                {
                                    ExpressaoPropriedadesAninhadas exprssProp = (ExpressaoPropriedadesAninhadas)umaChamadaDeMetodo.Elementos[i];

                                    

                                    if ((objRESULT!=null) && (exprssProp.aninhamento != null) && (exprssProp.aninhamento.Count > 0))
                                    {
                                        string nomePROP = exprssProp.aninhamento[0].nome;
                                        Objeto objPropriedade= new Objeto("private",objRESULT.tipo,nomePROP, objRESULT.GET(nomePROP));


                                        // remove o elmeento wrapper temporario, do escopo.
                                        escopo.tabela.RemoveObjeto(objRESULT.nome);

                                        return objPropriedade.valor;


                                    } 

                                }
                                else
                                // CHAMADAS DE METODO ADICIONAIS: caso primeiro: uma chamada GetElement de Vector, seguida de uma função
                                // chamada com o elemento do vector.
                                if (umaChamadaDeMetodo.Elementos[i].typeExprss == Expressao.typeCHAMADA_METODO)
                                {



                                    // atualiza o escopo, afim de processamento da chamada de metodo adicional.
                                    escopo.tabela.UpdateObjeto(objRESULT);

                                    // instancia a chamada de metodo aninhada.
                                    ExpressaoChamadaDeMetodo chamadaAdicional = (ExpressaoChamadaDeMetodo)umaChamadaDeMetodo.Elementos[i];
                                    // levantamento dos parametros da chamada.
                                    List<Expressao> exprssParametros2 = chamadaAdicional.parametros;


                                    // executa o metodo. o caller sai atualizado.
                                    object result2 = chamadaAdicional.funcao.ExecuteAMethod(exprssParametros2, escopo, ref objRESULT, Metodo.IS_METHOD);
                                    pilhaOperandos.Push(result2);




                                    // é preciso atualizar o vector, cujo elemento foi objeto caller da chamada de metodo adicional.
                                    if ((i == 0) && (isVectorObjectCaller))
                                    {
                                        // OBTEM O INDICE DO ELEMENTO VECTOR, que é o objeto caller da chamada de metodo adicional.
                                        object indexElementVector = EvalExpression.UM_EVAL.EvalPosOrdem(exprssParametros[0], escopo);
                                        // OBTEM O VETOR DA CHAMADA DE METODO PRIMEIRA.
                                        Vector vt = (Vector)umaChamadaDeMetodo.objectCaller;
                                        // ATUALIZA O VETOR, CUJO ELMEENTO FOI OBJETO CALLER DA CHAMADA ADICIONAL.
                                        vt.SetElement(indexElementVector, objRESULT);

                                        if (objRESULT != null)
                                        {
                                            // remove o objeto-elemento anonimo (definido pelo seu container vetor+um indice.
                                            escopo.tabela.RemoveObjeto(objRESULT.nome);
                                        }

                                    }


                                    // passa para o objeto caller da proxima chamada de metodo, se tiver.
                                    if ((result2 != null) && (result2.GetType() == typeof(Objeto)))
                                    {
                                        objRESULT = (Objeto)result2;
                                    }
                                }
                                i++;


                            }

                        }


                    }
                    else
                    // [Expressao] DENTRO DE [Expressao]
                    if (expss.Elementos[x].typeExprss == Expressao.typeEXPRESSAO)
                    {
                        // o elemento da expressão é outra expressão.
                        EvalExpression evalExpressaoElemento = new EvalExpression();
                        object result = evalExpressaoElemento.EvalPosOrdem(expss.Elementos[x], escopo); // avalia a expressão elemento.
                        pilhaOperandos.Push(result);
                    }
                    else
                    // EXPRESSAO NUMERO.
                    if (expss.Elementos[x].typeExprss == Expressao.typeNUMERO)
                    {
                        object numero = ((ExpressaoNumero)expss.Elementos[x]).numero;
                        pilhaOperandos.Push(numero);

                    }
                    else
                    // EXPRESSAO OBJETO.
                    if (expss.Elementos[x].typeExprss == Expressao.typeOBJETO)
                    {
                        Objeto v = escopo.tabela.GetObjeto(((ExpressaoObjeto)expss.Elementos[x]).nomeObjeto, escopo);

                        if ((v != null) && (v.valor != null) && (ExpressaoNumero.isNumero(v.valor.ToString())))
                        {
                            pilhaOperandos.Push(ExpressaoNumero.castingNumero(v.valor.ToString()));
                            pilhaObjetos.Push(v);
                        }
                        else
                        if ((v != null) && (v.valor != null))
                        {
                            pilhaOperandos.Push(v.valor);
                            pilhaObjetos.Push(v);
                        }
                        else
                        if (((ExpressaoObjeto)expss.Elementos[x]).nomeObjeto == "TRUE")
                        {
                            pilhaOperandos.Push(true);
                        }
                        else
                        if (((ExpressaoObjeto)expss.Elementos[x]).nomeObjeto == "FALSE")
                        {
                            pilhaOperandos.Push(false);
                        }
                        else
                        {
                            pilhaOperandos.Push(v);

                        }
                    }
                    else
                    // EXPRESSAO ELEMENTO.
                    if (expss.Elementos[x].typeExprss == Expressao.typeELEMENT)
                    {

                        // nova funcionalidade: elementos que podem ser o nome de variáveis.
                        Objeto v = escopo.tabela.GetObjeto(((ExpressaoElemento)expss.Elementos[x]).GetElemento().ToString(), escopo);
                        if (v != null)
                        {
                            pilhaOperandos.Push(v.GetValor());
                        }

                    }
                    else
                    // PROCESSAMENTO DE OPERADORES CONDICIONAIS && e ||, com shortcut de ifs, p.ex,
                    // OU PROCESSAMENTO DE EXPRESOES ENTRE PARENTESES NAO CONDICIONAIS.
                    if (expss.Elementos[x].typeExprss == Expressao.typeENTRE_PARENTESES)
                    {



                        Expressao exprssContainer = (((ExpressaoEntreParenteses)expss.Elementos[x]).exprssParenteses);

                        bool operadorCondicionalMaior = exprssContainer.tokens.Contains(">");
                        bool operadorCondicionalMenor = exprssContainer.tokens.Contains("<");
                        bool operadorCondicionalMaiorOuIgual = exprssContainer.tokens.Contains(">=");
                        bool operadorCondicionalMenorOuIgual = exprssContainer.tokens.Contains("<=");
                        bool operadorCondicionalNOT = exprssContainer.tokens.Contains("!");
                        bool operadorCondicionalIgualComparativo = exprssContainer.tokens.Contains("==");
                        bool constanteBooleanaTRUE = exprssContainer.tokens.Contains("TRUE");
                        bool constanteBooleanaFALSE = exprssContainer.tokens.Contains("FALSE");

                        // PROCESSAMAENTO DE EXPRESSOES CONDICIONAIS.
                        if ((operadorCondicionalMaior) || (operadorCondicionalMenor) || (operadorCondicionalMaiorOuIgual) ||
                            (operadorCondicionalMenorOuIgual) || (operadorCondicionalIgualComparativo) ||
                            (operadorCondicionalNOT) || (constanteBooleanaTRUE) || (constanteBooleanaFALSE))
                        {




                            bool operadorConectivoAND = expss.Elementos[x].tokens.Contains("&&");
                            bool operadorConectivoOR = expss.Elementos[x].tokens.Contains("||");

                            // processamento SEM CONECTIVOS CONDICIONAIS. há somente uma exprssao condicional, 
                            // entre parenteses, que foi retirado, para processamento regular da expressao.
                            if ((!operadorConectivoOR) && (!operadorConectivoAND))
                            {
                                // AVALIA a condicional. guardando o resultado na pilha de operandos.
                                object resultCondicional = EvalExpression.UM_EVAL.EvalPosOrdem(exprssContainer, escopo);

                                if (operadorCondicionalNOT)
                                {
                                    operadorCondicionalNOT = false;
                                    if (resultCondicional.GetType() == typeof(bool))
                                    {
                                        resultCondicional = !(bool)resultCondicional;
                                    }


                                }
                                pilhaOperandos.Push(resultCondicional);

                            }
                            else
                            {


                                int begin = 0;
                                Expressao condicional2 = null;
                                object resultCurr = true;
                                // processamento COM CONECTIVOS  CONDICIONAIS.
                                // processa sequencialmente, uma por uma as expressoes condicionais, eliminando a expressao condicional currente, da
                                // expressao container entre parenteses principal.
                                while (begin + 1 <= exprssContainer.Elementos.Count)
                                {


                                    // obtem a proxima expressao condicional.
                                    condicional2 = exprssContainer.Elementos[begin];

                                    // processamento da expressao condicional currente.
                                    if (condicional2.typeExprss == Expressao.typeENTRE_PARENTESES)
                                    {
                                        ExpressaoEntreParenteses condicionalSemParenteses = ((ExpressaoEntreParenteses)condicional2);
                                        Expressao exprssWrapper = new Expressao();
                                        exprssWrapper.Elementos.Add(condicionalSemParenteses);


                                        // avalia a expressao condicional curremte.
                                        resultCurr = EvalExpression.UM_EVAL.EvalPosOrdem(exprssWrapper, escopo);
                                        pilhaOperandos.Push(resultCurr);

                                    }
                                    // obtem o proximo condicional conectivo, a partir de t, indice de malha conectivo.
                                    string proximoCondicionalConectivo = ObtemNesimoOperadorConectivo(exprssContainer, begin);

                                    if ((exprssContainer.Elementos[begin].tokens != null) &&
                                        (exprssContainer.Elementos[begin].tokens.Count == 1) &&
                                        ((exprssContainer.Elementos[begin].tokens[0] == "&&") || (exprssContainer.Elementos[begin].tokens[0] == "||")))
                                    {


                                        // PROCESSAMENTO DE OPERADOR NOT
                                        if ((operadorCondicionalNOT) && (resultCurr != null))
                                        {
                                            resultCurr = !(bool)resultCurr;
                                            operadorCondicionalNOT = false;
                                        }



                                        // ANALISE DE SHORT CUTS DE IFS:
                                        if (((bool)resultCurr == true) && (proximoCondicionalConectivo == "||"))
                                        {
                                            return true;
                                        }
                                        else
                                        if (((bool)resultCurr == false) && (proximoCondicionalConectivo == "&&"))
                                        {
                                            return false;
                                        }

                                        // faz o processamento de 2 expressoes condicionais, guardadas na pilha de operandos seus resultados.
                                        object operando2 = pilhaOperandos.Pop();
                                        object operando1 = pilhaOperandos.Pop();
                                        object result2 = null;


                                        if (exprssContainer.Elementos[begin].tokens.Contains("&&"))
                                        {
                                            result2 = (bool)operando2 && (bool)operando1;
                                        }

                                        if (exprssContainer.Elementos[begin].tokens.Contains("||"))
                                        {
                                            result2 = (bool)operando2 || (bool)operando1;
                                        }


                                        if (operadorCondicionalNOT)
                                        {
                                            result2 = !(bool)result2;
                                            operadorCondicionalNOT = false;
                                        }


                                        pilhaOperandos.Push(result2);

                                        begin += 1;
                                        continue;
                                    }

                                    // passa para a proxima expressao condicional.
                                    begin += 1;






                                }
                            }


                        }

                        else
                        {
                            // PROCESSAMENTO DE EXPRESSOES ENTRE PARENTESES NAO CONDICIONAIS.
                            // expressao já sem o wrapper de expressao entre parenteses, procede o processamento da expressao com sub-expressoes em seus Elementos.
                            object resultExprssEntreParenteses = EvalExpression.UM_EVAL.EvalPosOrdem(exprssContainer, escopo);
                            pilhaOperandos.Push(resultExprssEntreParenteses);

                            continue;

                        }

                    }
                    else
                    // EXPRESSAO OPERADOR.
                    if (expss.Elementos[x].typeExprss == Expressao.typeOPERADOR)
                    {
                        Operador operador = ((ExpressaoOperador)expss.Elementos[x]).operador;

                        if (operador.tipo == "BINARIO")
                        {
                            if (operador.nome == "&&")
                            {
                                bool operando2 = (bool)pilhaOperandos.Pop();
                                bool operando1 = (bool)pilhaOperandos.Pop();

                                return operando1 && operando2;
                            }

                            if (operador.nome == "||")
                            {
                                bool operando2 = (bool)pilhaOperandos.Pop();
                                bool operando1 = (bool)pilhaOperandos.Pop();

                                return operando1 || operando2;
                            }

                            if (operador != null)
                            {
                                if (operador.nome == "=")
                                {
                                    object novoValor = pilhaOperandos.Pop();

                                    // RETORNO com atribuicao OBJETO.
                                    if (expss.Elementos[0].typeExprss == Expressao.typeOBJETO)
                                    {
                                        escopo.tabela.GetObjeto(((ExpressaoObjeto)expss.Elementos[0]).nomeObjeto, escopo).SetValor(novoValor);
                                    }
                                    else
                                    // RETORNO com atribuicao PROPRIEDADES ANINHADAS.
                                    if (expss.Elementos[0].typeExprss == Expressao.typePROPRIEDADES_ANINHADADAS)
                                    {
                                        ExpressaoPropriedadesAninhadas epxrssRetornoPropriedades = (ExpressaoPropriedadesAninhadas)expss.Elementos[0];
                                        epxrssRetornoPropriedades.aninhamento[epxrssRetornoPropriedades.aninhamento.Count - 1].SetValor(novoValor);

                                    }

                                }
                                else
                                {
                                    object oprnd2 = pilhaOperandos.Pop();
                                    object oprnd1 = pilhaOperandos.Pop();


                                    // execução de operador nativo.
                                    result1 = operador.ExecuteOperador(operador.nome, escopo, oprnd1, oprnd2);
                                    pilhaOperandos.Push(result1);
                                } // else
                            } // if
                        }
                        else
                        if (operador.tipo.Contains("UNARIO"))
                        {



                            object opndPOS = pilhaOperandos.Pop();

                            if (opndPOS == null)
                            {
                                continue;
                            }



                            if (operador.tipo.Contains("POS"))
                            {
                                // primeiro guarda o valor, depois faz a operacao do operador.
                                pilhaOperandos.Push(opndPOS);
                                // ex.: c++. (unario pos)
                                object valorPOS_UNARIO = operador.ExecuteOperador(operador.nome, escopo, opndPOS);


                                Objeto variavelUnario = pilhaObjetos.Pop();

                                // atualiza o valor do operando, cumprindo o script de operador unario.
                                if ((variavelUnario != null) && (variavelUnario.GetType() == typeof(Objeto)))
                                {
                                    variavelUnario.valor = valorPOS_UNARIO;
                                    escopo.tabela.UpdateObjeto(variavelUnario);
                                }



                            }
                            else
                            if (operador.tipo.Contains("PRE"))
                            {
                                // primeiro faz a operacao do operador, depois guarda o valor.
                                // ex.: ++c. (unario pre)
                                object valorPRE_UNARIO = operador.ExecuteOperador(operador.nome, escopo, opndPOS);
                                pilhaOperandos.Push(valorPRE_UNARIO);
                                if (valorPRE_UNARIO.GetType() == typeof(Objeto))
                                {
                                    pilhaObjetos.Push((Objeto)valorPRE_UNARIO);
                                }

                                Objeto variavelUnario = pilhaObjetos.Pop();
                                /// atualiza o valor do operando.
                                if ((variavelUnario != null) && (variavelUnario.GetType() == typeof(Objeto)))
                                {
                                    variavelUnario.valor = valorPRE_UNARIO;

                                    escopo.tabela.UpdateObjeto(variavelUnario);
                                }
                            }





                        }
                    }
                }
            } //  for x, malha principal de sub-expressoes.

            if (pilhaOperandos.lenghtPilha > 0)
            {
                result1 = pilhaOperandos.Pop();
            }
            return result1;
        } // EvalPosOrdem()

        


        /// <summary>
        /// obtem o n-esimo operador condicional conectivo.
        /// </summary>
        /// <param name="exprrsContainer">expressao condicional contendo operadores conecctivos.</param>
        /// <param name="indexNesimo">contador para o n-esimo operador conectivo que se quer obter.</param>
        /// <returns></returns>
        private static string ObtemNesimoOperadorConectivo(Expressao exprrsContainer, int indexNesimo)
        {
            if ((exprrsContainer == null) || (exprrsContainer.Elementos == null) || (exprrsContainer.Elementos.Count == 0))
            {
                return "";
            }
            else
            {
                int contadorOperadores = 0;
                int contadorIndice = indexNesimo;
                for (int i = 0; i < exprrsContainer.Elementos.Count; i++)
                {
                    if ((exprrsContainer.Elementos[i] != null) &&
                        (exprrsContainer.Elementos[i].tokens != null) &&
                        (exprrsContainer.Elementos[i].tokens.Count > 0) &&
                        ((exprrsContainer.Elementos[i].tokens[0] == "&&") || (exprrsContainer.Elementos[i].tokens[0] == "||")))
                    {
                        contadorOperadores++;
                        if (contadorOperadores == contadorIndice)
                        {
                            return exprrsContainer.Elementos[i].tokens[0];
                        }
                    }
                }
            }
            return "";
        }







 
        /// <summary>
        /// obtem um numero a partir de um token [str_numero].
        /// </summary>
        /// <param name="numero">numero da conversao.</param>
        /// <param name="isFoundANumber">[true] se converteu o token para um numero: int, float, double.</param>
        /// <param name="str_numero">token a ser convertido em numero.</param>
        /// <param name="escopo">contexto onde o numero está.</param>
        public static void GetNumber(ref object numero, ref bool isFoundANumber, string str_numero, Escopo escopo)
        {
            // o objeto tem valor como numero.
            if (ExpressaoNumero.isNumero(str_numero))
            {
                isFoundANumber = true;
                ExpressaoNumero exprss = new ExpressaoNumero();
                numero= exprss.ConverteParaNumero(str_numero, escopo);
            }
            else
            {
                isFoundANumber = false;
            }
        }






        public class Testes : SuiteClasseTestes
        {
            char aspas = ExpressaoLiteralText.aspas;
            private EvalExpression eval = new EvalExpression();


            public Testes() : base("testes para availiacao de expressoes.")
            {
            }


            public void TestesWrapperObjectsExpressaoAtribuicao(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigo_create = "int[] v1[15]; int[] v2[15];";
                string codigo_expressao_0_1 = "v1[0]=4";
                string codigo_expressao_0_2 = "v2[0]=1";
                string codigo_expressao_0_3 = "v1=v2";

                ProcessadorDeID compilador = new ProcessadorDeID(codigo_create);
                compilador.Compilar();

                Expressao exprss_0_1 = new Expressao(codigo_expressao_0_1, compilador.escopo);
                Expressao exprss_0_2 = new Expressao(codigo_expressao_0_2, compilador.escopo);
                Expressao exprss_0_3 = new Expressao(codigo_expressao_0_3, compilador.escopo);

                try
                {
                    assercao.IsTrue(exprss_0_1.Elementos[0].typeExprss == Expressao.typeCHAMADA_METODO, codigo_expressao_0_1);
                    assercao.IsTrue(exprss_0_2.Elementos[0].typeExprss == Expressao.typeCHAMADA_METODO, codigo_expressao_0_2);
                    assercao.IsTrue(exprss_0_3.Elementos[0].typeExprss == Expressao.typeATRIBUICAO, codigo_expressao_0_3);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
                try
                {
                    EvalExpression eval = new EvalExpression();

                    object result_0_1 = eval.EvalPosOrdem(exprss_0_1, compilador.escopo);
                    object result_0_2 = eval.EvalPosOrdem(exprss_0_2, compilador.escopo);
                    object result_0_3 = eval.EvalPosOrdem(exprss_0_3, compilador.escopo);  // v1= v2, atriuicao.


                    Vector v2 = (Vector)((Vector)result_0_3).valor;
                    Vector v1 = (Vector)((Vector)(compilador.escopo.tabela.GetObjeto("v2", compilador.escopo)).valor);


                    assercao.IsTrue(v1.GetElement(0).ToString() == "1", codigo_expressao_0_3);
                    assercao.IsTrue(v2.GetElement(0) != null && v2.GetElement(0).ToString() == "1", codigo_expressao_0_2);


                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }

            public void TesteExpressaoCondificionalComTrueFalse(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_0_0_create = "bool b=FALSE;";
                    string code_0_0_program = "if ((b==FALSE)){b=TRUE;};";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_create + code_0_0_program);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);

                    assercao.IsTrue((bool)compilador.escopo.tabela.GetObjeto("b", compilador.escopo).valor == true, code_0_0_create + "  " + code_0_0_program);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }

            }

            public void TesteObjetosBooleans(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_0_0 = "bool op=FALSE; if (op==FALSE){op=TRUE;};";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);

                    assercao.IsTrue((bool)compilador.escopo.tabela.GetObjeto("op", compilador.escopo).valor == true, code_0_0);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }


            }

            public void TesteClasseOrquideaComBooleans(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                   
                    string code_class = "public class Ship {" +
                        "public bool op; " +
                        "public Ship(){ op=TRUE};" +
                        "public metodoB(){" +
                                "if (op==TRUE){op=FALSE;}}" +
                        ";};";
                    string code_create = "Ship umShip=create(); umShip.metodoB();";


                    ProcessadorDeID compilador = new ProcessadorDeID(code_class + code_create);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);

                    assercao.IsTrue((bool)compilador.escopo.tabela.GetObjeto("umShip", compilador.escopo).propriedades[0].valor == false, code_class + "  " + code_create);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }

            }

            public void TestesFuncoesClasseString(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoCreate_0_0 = "string s;";
                string codigoCreate_0_1 = "string texto= " + aspas + "este e um texto literal" + aspas + ";";
                string codigoCreate_0_2 = "bool b= true;";
                string codigoCreate_0_3 = "string textoToLower=" + aspas + "MARTE" + aspas + ";";
                string codigoCreate_0_5 = "string textoToReplace=" + aspas + "Terra Azul" + aspas + ";";
                string codigoCreate_0_6 = "int x=0;";

                string codigo_0_0 = "string.textFromInt(5)";
                string codigo_0_1 = "string.Contains(" + aspas + "este e um texto literal" + aspas + "," + aspas + "literal" + aspas + " );";
                string codigo_0_2 = "string.Start(" + aspas + "comeco eh tudo" + aspas + "," + aspas + "comeco" + aspas + ")";
                string codigo_0_3 = "string.EqualsText(" + aspas + "Terra" + aspas + "," + aspas + "Marte" + aspas + ")";
                string codigo_0_5 = "string.Index(" + aspas + "numero" + aspas + "," + aspas + "num" + aspas + ")";
                string codigo_0_6 = "string.ReplaceFor(" + aspas + "Terra Azul" + aspas + "," + aspas + "Azul" + aspas + "," + aspas + "Vermelha" + aspas + ");";
                string codigo_0_7 = "string._Upper(" + aspas + "terra" + aspas + ");";
                string codigo_0_8 = "textoToLower._Lower()";
                string codigo_0_9 = "textoToReplace.ReplaceFor(" + aspas + "Azul" + aspas + "," + aspas + "Verde" + aspas + ");";
                string codigo_0_9_1 = "x = string.Index(" + aspas + "numero" + aspas + ", " + aspas + "num" + aspas + ")";



                EvalExpression eval = new EvalExpression();

                ProcessadorDeID compilador = new ProcessadorDeID(codigoCreate_0_0 + codigoCreate_0_1 + codigoCreate_0_2 + codigoCreate_0_3 + codigoCreate_0_5 + codigoCreate_0_6);
                compilador.Compilar();



                Expressao exprss_0_9_1 = new Expressao(codigo_0_9_1, compilador.escopo);
                Expressao exprss_0_9 = new Expressao(codigo_0_9, compilador.escopo);
                Expressao exprss_0_8 = new Expressao(codigo_0_8, compilador.escopo);
                Expressao express_0_7 = new Expressao(codigo_0_7, compilador.escopo);
                Expressao express_0_6 = new Expressao(codigo_0_6, compilador.escopo);
                Expressao express_0_5 = new Expressao(codigo_0_5, compilador.escopo);
                Expressao express_0_3 = new Expressao(codigo_0_3, compilador.escopo);
                Expressao express_0_2 = new Expressao(codigo_0_2, compilador.escopo);
                Expressao express_0_0 = new Expressao(codigo_0_0, compilador.escopo);
                Expressao express_0_1 = new Expressao(codigo_0_1, compilador.escopo);

                object result_0_9_1 = eval.EvalPosOrdem(exprss_0_9_1, compilador.escopo);
                object result_0_9 = eval.EvalPosOrdem(exprss_0_9, compilador.escopo);
                object result_0_8 = eval.EvalPosOrdem(exprss_0_8, compilador.escopo);
                object result_0_7 = eval.EvalPosOrdem(express_0_7, compilador.escopo);
                object result_0_6 = eval.EvalPosOrdem(express_0_6, compilador.escopo);
                object result_0_5 = eval.EvalPosOrdem(express_0_5, compilador.escopo);
                object result_0_3 = eval.EvalPosOrdem(express_0_3, compilador.escopo);
                object result_0_2 = eval.EvalPosOrdem(express_0_2, compilador.escopo);
                object result_0_0 = eval.EvalPosOrdem(express_0_0, compilador.escopo);
                object result_0_1 = eval.EvalPosOrdem(express_0_1, compilador.escopo);


                try
                {
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("x", compilador.escopo).valor.ToString() == "0", codigo_0_9_1);
                    assercao.IsTrue(result_0_9.ToString().Contains("Verde"));
                    assercao.IsTrue(result_0_8.ToString() == "marte", codigo_0_8);
                    assercao.IsTrue(result_0_7.ToString() == "TERRA", codigo_0_7);
                    assercao.IsTrue(result_0_6.ToString().Contains("Vermelha"), codigo_0_6);
                    assercao.IsTrue(result_0_5.ToString() == "0", codigo_0_5);
                    assercao.IsTrue(result_0_3.ToString() == "False", codigo_0_3);
                    assercao.IsTrue(result_0_2.ToString() == "True", codigo_0_2);
                    assercao.IsTrue(result_0_1.ToString() == "True", codigo_0_1);
                    assercao.IsTrue(result_0_0.ToString() == "5", codigo_0_0);

                }
                catch (Exception e)
                {
                    string codigoError = e.Message;
                    assercao.IsTrue(false, "FALHA NO TESTE");
                }


            }




            public void TesteAtribuicao(AssercaoSuiteClasse assercao)
            {
                string code_0_0 = "Imagem img1=create(\"programa jogos\\assets space invaders\\missel_1.png\");" +
                    "Imagem img2=create(\"programa jogos\\assets space invaders\\invader1.png\"); img1= img2;";

                ProcessadorDeID compilador= new ProcessadorDeID(code_0_0);
                compilador.Compilar();

                ProgramaEmVM program = new ProgramaEmVM(compilador.instrucoes);
                program.Run(compilador.escopo);




            }

            public void TestsSplitFunction(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoCreate = "string[] separadores_1[5];";
                string codeText = "string text=" + aspas + "A Terra eh verde da Amazonia" + aspas + ";";
                string codigoSETElement = "separadores_1[0]=" + aspas + " " + aspas;
                string codigo_0_0 = "string.CuttWords(text, separadores_1);";


                EvalExpression eval = new EvalExpression();

                ProcessadorDeID compilador = new ProcessadorDeID(codeText + codigoCreate);
                compilador.Compilar();

                try
                {

                    Expressao exprssVectorSetElement = new Expressao(codigoSETElement, compilador.escopo);
                    object result_setElement = eval.EvalPosOrdem(exprssVectorSetElement, compilador.escopo);


                    Expressao exprss_0_0 = new Expressao(codigo_0_0, compilador.escopo);
                    object result__1 = eval.EvalPosOrdem(exprss_0_0, compilador.escopo);



                    // vector criado no compilador;
                    Vector vtResult = (Vector)result__1;

                    assercao.IsTrue(vtResult.GetElement(0).ToString() == "A");

                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "Teste Falhou: " + e.Message);

                }


            }


  


            public void TesteExpressaoAtribuicaoBool(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_0_0 = "bool n= TRUE;";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.instrucoes);
                    programa.Run(compilador.escopo);

                    assercao.IsTrue((bool)compilador.escopo.tabela.objetos[0].valor == true, code_0_0);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }

            }
            public void TesteOperadorNOT(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                try
                {
                    string code_0_0 = "int x=1; int y=2;";
                    string code_exprss_0_0 = " (!(x<y))";

                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                    compilador.Compilar();

                    Expressao exprss_0_0 = new Expressao(code_exprss_0_0, compilador.escopo);
                    EvalExpression eval = new EvalExpression();
                    object result = eval.EvalPosOrdem(exprss_0_0, compilador.escopo);

                    assercao.IsTrue((bool)result == false, code_0_0 + "   " + code_exprss_0_0);


                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }


            }
            public void TesteOperadorUnario(AssercaoSuiteClasse assercao)
            {

                try
                {
                    SystemInit.InitSystem();

                    string code_0_0 = "int x=1;";
                    string exprss_0_0 = "x++"; ;


                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                    compilador.Compilar();

                    Expressao exprssOpUnario = new Expressao(exprss_0_0, compilador.escopo);
                    EvalExpression eval = new EvalExpression();
                    object result = eval.EvalPosOrdem(exprssOpUnario, compilador.escopo);

                    assercao.IsTrue(result.ToString().ToString() == "2", code_0_0 + "   " + exprss_0_0);
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("x", compilador.escopo).valor.ToString() == "2", code_0_0+"  "+exprss_0_0);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
                
            }



            public void TesteAvaliacaoChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoMetodo_0_abs = "double.abs(x);";
                string codigoMetodo_1_abs = "x.abs();";
                string codigoMetodo_2_abs = "double.abs(-1.0);";

                string codigoCreate = "double x=1.0;";


                string codigo_minus_1 = "x=-1.0";
                string codigo_sqrt_0 = "double.root2(9.0)";
                string codigo_sqrt_1 = "double.root2(2.0)";

                EvalExpression eval = new EvalExpression();

                ProcessadorDeID compilador = new ProcessadorDeID(codigoCreate);
                compilador.Compilar();

                try
                {
                    Expressao exprss_0_abs = new Expressao(codigoMetodo_0_abs, compilador.escopo);
                    object result_0_0_abs = eval.EvalPosOrdem(exprss_0_abs, compilador.escopo);

                    Expressao exprss_1_abs = new Expressao(codigoMetodo_1_abs, compilador.escopo);
                    object result_0_1_abs = eval.EvalPosOrdem(exprss_1_abs, compilador.escopo);

                    Expressao exprss_2_abs = new Expressao(codigoMetodo_2_abs, compilador.escopo);
                    object result_0_2_abs = eval.EvalPosOrdem(exprss_2_abs, compilador.escopo);


                    Expressao exprss_0_minus_1 = new Expressao(codigo_minus_1, compilador.escopo);
                    Expressao exprss_0_1_sqrt_0 = new Expressao(codigo_sqrt_0, compilador.escopo);
                    Expressao exprss_0_2_sqrt_1 = new Expressao(codigo_sqrt_1, compilador.escopo);

                    object result_0_minus1 = eval.EvalPosOrdem(exprss_0_minus_1, compilador.escopo);
                    object result_0_sqrt_0 = eval.EvalPosOrdem(exprss_0_1_sqrt_0, compilador.escopo);
                    object result_0_sqrt_1 = eval.EvalPosOrdem(exprss_0_2_sqrt_1, compilador.escopo);



                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("x", compilador.escopo).valor.ToString() == "-1", codigo_minus_1);
                    assercao.IsTrue(result_0_sqrt_0.ToString() == "3", codigo_sqrt_0);
                    assercao.IsTrue(result_0_sqrt_1.ToString().Contains("1.41"), codigo_sqrt_1);
                    assercao.IsTrue(result_0_2_abs.ToString() == "1", codigoMetodo_2_abs);
                    assercao.IsTrue(result_0_1_abs.ToString() == "1", codigoMetodo_1_abs);
                    assercao.IsTrue(result_0_0_abs.ToString() == "1", codigoMetodo_0_abs);
                    assercao.IsTrue(((Objeto)result_0_minus1).valor.ToString() == "-1", codigo_minus_1);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "falha no teste: " + e.Message);


                }
            }


   

            public void TesteAvaliacaoExpressaoCondicionalComConectivos(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string code_0_0 = "(  ((a<1) && (b>2)) || (a==2))";
                string code_0_1 = "(  ((a>1) && (b>2)) || (a==2))";
                string code_0_2 = "(  ((a<2) && (b>=2)) || (a==2))";
                string code_0_3 = "(((a<1) && (b>2)) || (a==2))";

                Escopo escopoTeste = new Escopo(code_0_0);
                escopoTeste.tabela.AddObjeto("private", "a", "int", 1, escopoTeste);
                escopoTeste.tabela.AddObjeto("private", "b", "int", 2, escopoTeste);


                Expressao exprss_0_2 = new Expressao(code_0_2, escopoTeste);
                Expressao exprss_0_0 = new Expressao(code_0_0, escopoTeste);
                Expressao exprss_0_1 = new Expressao(code_0_1, escopoTeste);
                Expressao exprss_0_3 = new Expressao(code_0_3, escopoTeste);


                EvalExpression eval = new EvalExpression();
                try
                {
                    object result_0_2 = eval.EvalPosOrdem(exprss_0_2, escopoTeste);
                    object result_0_0 = eval.EvalPosOrdem(exprss_0_0, escopoTeste);

                    object result_0_1 = eval.EvalPosOrdem(exprss_0_1, escopoTeste);
                    object result_0_3 = eval.EvalPosOrdem(exprss_0_3, escopoTeste);


                    assercao.IsTrue((bool)result_0_2 == true, code_0_2);
                    assercao.IsTrue((bool)result_0_0 == false, code_0_0);
                    assercao.IsTrue((bool)result_0_1 == false, code_0_1);

                    assercao.IsTrue(result_0_3.ToString() == "False", code_0_3);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

            }


            public void TestsVectorAtribution(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                EvalExpression eval = new EvalExpression();

                string code_create = "int[] v1[15]; int[] v2[15];";
                string code_init_0_0 = "v1[0]=5";
                string code_init_0_1 = "v2[0]=1";
                string code_0_0 = "v1=v2";

                ProcessadorDeID compilador = new ProcessadorDeID(code_create);
                compilador.Compilar();


                try
                {
                    Expressao exprss_init_0_0 = new Expressao(code_init_0_0, compilador.escopo);
                    Expressao exprss_init_0_1 = new Expressao(code_init_0_1, compilador.escopo);

                    object result_0_0 = eval.Eval(exprss_init_0_0, compilador.escopo);
                    object result_0_1 = eval.Eval(exprss_init_0_1, compilador.escopo);
                    Expressao exprss_0_0 = new Expressao(code_0_0, compilador.escopo);


                    object result = eval.EvalPosOrdem(exprss_0_0, compilador.escopo);
                    Vector vtResult = (Vector)((Objeto)result);
                    assercao.IsTrue(vtResult.GetElement(0) != null && vtResult.GetElement(0).ToString() == "1", code_0_0);

                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                }

            }



            public void TesteParametrosMultiArgumentos(AssercaoSuiteClasse assercao)
            {

                SystemInit.InitSystem();
                string codigoClasse = "public class classeT { public int propriedadeA = 1;  public classeT(){ int x=1; }; public int metodoB(double x, ! int[] y){ return 5;};};";
                string codigoCreate = "classeT objA= create(); double x=1;";
                string codigoChamadaDeMetodo_pass = "objA.metodoB(x,1,1,1);";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasse + codigoCreate);
                compilador.Compilar();


                Expressao exprssChamadaDeMetodo_pass = new Expressao(codigoChamadaDeMetodo_pass, compilador.escopo);



                // avalia as expressoes chamada de metodo.
                EvalExpression eval = new EvalExpression();
                object result_pass = eval.Eval(exprssChamadaDeMetodo_pass, compilador.escopo);

                try
                {
                    assercao.IsTrue(result_pass != null && result_pass.ToString() == "5", codigoChamadaDeMetodo_pass);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message + ":    " + codigoChamadaDeMetodo_pass);
                }

            }






   





 
 
    
            public void TesteAvaliacaoExpressaoCondicional(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoExprss_0_0 = "((a<=1) && (b>=2))";
                string codigoExprss_0_1 = "(((a<=1) && (b>=2)) || (a<=1))";
                EvalExpression eval = new EvalExpression();

                Escopo escopoTeste = new Escopo(codigoExprss_0_0);
                escopoTeste.tabela.AddObjeto("private", "a", "int", 1, escopoTeste);
                escopoTeste.tabela.AddObjeto("private", "b", "int", 2, escopoTeste);

                try
                {

                    Expressao exprssCondicional_0_1 = new Expressao(codigoExprss_0_1, escopoTeste);
                    object result_0_1 = eval.EvalPosOrdem(exprssCondicional_0_1, escopoTeste);



                    Expressao exprssCondicional_0_0 = new Expressao(codigoExprss_0_0, escopoTeste);
                    object result_0_0 = eval.EvalPosOrdem(exprssCondicional_0_0, escopoTeste);




                    assercao.IsTrue((bool)result_0_0 == true, codigoExprss_0_0);
                    assercao.IsTrue((bool)result_0_1 == true, codigoExprss_0_1);


                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

            }


            public void TesteAvaliacaoExpressaCondicionalComplexaDeConectivos(AssercaoSuiteClasse assercao)
            {

                SystemInit.InitSystem();

                string code_0_0 = "(((a>b)&& (c<1))  || (a>5))";
                Escopo escopo = new Escopo(code_0_0);
                escopo.tabela.AddObjeto("private", "a", "int", 6, escopo);
                escopo.tabela.AddObjeto("private", "b", "int", 5, escopo);
                escopo.tabela.AddObjeto("private", "c", "int", 2, escopo);
                EvalExpression eval = new EvalExpression();

                try
                {
                    Expressao exprss_0_0 = new Expressao(code_0_0, escopo);
                    object result_0_0 = eval.EvalPosOrdem(exprss_0_0, escopo);

                    assercao.IsTrue((bool)result_0_0 == true, code_0_0);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }



 
       

            public void TesteInstanciacao(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string code_0_0 = "double x=1.0;";
                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                compilador.Compilar();
                try
                {
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("x", compilador.escopo) != null);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }
           


            public void TesteExpressaoCondicionalSEMconectivos(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string code_0_0 = "(a<b)";

                Escopo escopo = new Escopo(code_0_0);
                escopo.tabela.AddObjeto("private", "a", "int", 1, escopo);
                escopo.tabela.AddObjeto("private", "b", "int", 2, escopo);

                EvalExpression eval = new EvalExpression();
                Expressao exprss_0_0 = new Expressao(code_0_0, escopo);
                try
                {
                    object result_0_0 = eval.EvalPosOrdem(exprss_0_0, escopo);
                    assercao.IsTrue(result_0_0.ToString() == "True", code_0_0);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }

            }









 



            public void TesteAvaliacaoNumeros(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigoCreate = "int a=1; int b=5; int c=3;";
                string codigo_0_0 = "c= a + b;";
                string codigo_0_1 = "a=2*b;";
                string codigo_0_2 = "b=b-c*a";


                string codigo_0_0_sem_atribuicao = "a + b;";
                string codigo_0_1_sem_atribuicao = "2*b;";
                string codigo_0_2_sem_atribuicao = "b-c*a";


                ProcessadorDeID compilador = new ProcessadorDeID(codigoCreate);
                compilador.Compilar();

                Expressao exprss_0_0_sem_atribuicao = new Expressao(codigo_0_0_sem_atribuicao, compilador.escopo);
                Expressao exprss_0_1_sem_atribuicao = new Expressao(codigo_0_1_sem_atribuicao, compilador.escopo);
                Expressao exprss_0_2_sem_atribuicao = new Expressao(codigo_0_2_sem_atribuicao, compilador.escopo);

                try
                {

                    object valor_0_0_sem_atribuicao = eval.EvalPosOrdem(exprss_0_0_sem_atribuicao, compilador.escopo);
                    object valor_0_1_sem_atribuicao = eval.EvalPosOrdem(exprss_0_1_sem_atribuicao, compilador.escopo);
                    object valor_0_2_sem_atribuicao = eval.EvalPosOrdem(exprss_0_2_sem_atribuicao, compilador.escopo);

                    assercao.IsTrue(valor_0_0_sem_atribuicao.ToString() == "6", codigo_0_0_sem_atribuicao);
                    assercao.IsTrue(valor_0_1_sem_atribuicao.ToString() == "10", codigo_0_1_sem_atribuicao);
                    assercao.IsTrue(valor_0_2_sem_atribuicao.ToString() == "2", codigo_0_2_sem_atribuicao);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }




                Expressao exprss_0_0 = new Expressao(codigo_0_0, compilador.escopo);
                Expressao exprss_0_1 = new Expressao(codigo_0_1, compilador.escopo);
                Expressao exprss_0_2 = new Expressao(codigo_0_2, compilador.escopo);






                try
                {


                    EvalExpression eval = new EvalExpression();


                    object valor_0_0 = eval.EvalPosOrdem(exprss_0_0, compilador.escopo);
                    object valor_0_1 = eval.EvalPosOrdem(exprss_0_1, compilador.escopo);
                    object valor_0_2 = eval.EvalPosOrdem(exprss_0_2, compilador.escopo);

                    Objeto result_0_0 = (Objeto)valor_0_0;
                    Objeto result_0_1 = (Objeto)valor_0_1;
                    Objeto result_0_2 = (Objeto)valor_0_2;

                    assercao.IsTrue(result_0_0.valor.ToString() == "6");
                    assercao.IsTrue(result_0_1.valor.ToString() == "10");
                    assercao.IsTrue(result_0_2.valor.ToString() == "-55");


                }
                catch (Exception ex)
                {
                    string codeError = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }


            }






   










    



        }

    } //class EvalExpression

} // namespace
