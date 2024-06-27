using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using parser;
using parser.textoFormatado;

namespace Wrappers
{
    /// <summary>
    /// classe responsavel pela conversao de anotação wrapper para chamadas de metodo.
    /// </summary>
    public class WrapperDataDictionaryText : WrapperData
    {
       
        /// <summary>
        /// pattern resumed tipado para instanciacao wrapper.
        /// </summary>
        private string patternRigor = "DictionaryText id = { id }";
        /// <summary>
        /// regex para pattern rigor.
        /// </summary>
        private Regex regexRigor;

        //string str_getElement = "id {exprss}"; // pattern resumed para obter um elemento, em anotação wrapper.
        //string str_setElement = "id {exprss, exprss}"; // pattern resumed para setar um elemento, em anotação wrapper.

        // anotação wrapper instanciacao: //DictionaryText id = { id }
        // anotação wrapper getElement {key}.
        // anotação wrapper setElement {key}=value.


        /// <summary>
        /// construtor vazio.
        /// </summary>
        public WrapperDataDictionaryText()
        {
            // constroi a string da expressao regex.
            TextExpression textRegex = new TextExpression();
            string textPatternRigor = textRegex.FormaExpressaoRegularGenerica(patternRigor);
            regexRigor = new Regex(textPatternRigor);

            this.tipo = "DictionaryText";
        }

        /// <summary>
        /// obtem o tipo de elemento do dictionary text, e o contador de tokens utilizados na definição.
        /// </summary>
        /// <param name="tokens">tokens contendo a definição do wrapper object.</param>
        /// <param name="countTokensWrapper">contador de tokens utilizados na instanciacao.</param>
        /// <returns></returns>
        public override string GetTipoElemento(List<string> tokens, ref int countTokensWrapper)
        {
            ////DictionaryText id  { id } ---> DictionaryText id  { id } (como parametro).
            int indexTypeData = tokens.IndexOf("DictionaryText");
            if (indexTypeData != -1) 
            {
                countTokensWrapper = 5;
                return tokens[indexTypeData + 3];
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// obtem o nome do wrapper data object, dentro de tokens de definição de objeto.
        /// </summary>
        /// <param name="tokens">tokens contendo a definicao do wrapper data object.</param>
        /// <returns></returns>
        public override string GetNameWrapperObject(List<string> tokens, int index)
        {
            // DictionaryText id = { id }   -----> como parametro: DictionaryText id { id } 
            int indexTypeData = tokens.IndexOf("DictionaryText", index);
            if ((indexTypeData >= 0) && (indexTypeData + 1 < tokens.Count)) 
            {
                return tokens[indexTypeData + 1];
            }
            else
            {
                return null;
            }
           
        }



        /// <summary>
        /// retorna true, se há definição de um dictionary text wrapper object.
        /// </summary>
        /// <param name="tokens">tokens contendo a definicao do wrapper objetc.</param>
        /// <param name="index">indice onde estpa o nome do wrapper object.</param>
        /// <returns></returns>
        public override List<string> isThisTypeWrapperParameter(List<string> tokens, int index)
        {
            // DictionaryText id = { id } ---> DictionaryText id { id };
            if ((index>=0) && (index<tokens.Count))
            {
   
                if ((index + 5 < tokens.Count) && (tokens[index] == "DictionaryText")) 
                {
                    List<string> tokensWrapper = new List<string>();
                    tokensWrapper.AddRange(tokens.GetRange(index, 5));
                    return tokensWrapper;
                }
                

            }

            return null;

        }




        /// <summary>
        /// cria uma chamada de metodo estatica, para instanciar um objeto wrapper [DictionaryText].
        /// </summary>
        /// <param name="exprssInstanciacaoEmNotacaoWrapper">expressao wrapper da instanciacao.</param>
        /// <param name="escopo">contexto onde a expressao esta.</param>
        /// <param name="tokensProcessed">tokens consumidos durante a criacao do wrapper object.</param>
        /// <returns>retorna uma lista de tokens da chamada de metodo estatica de instanciacao.</returns>
        public override List<string> CREATE(ref string exprssInstanciacaoEmNotacaoWrapper, Escopo escopo, ref List<string> tokensProcessed)
        {
            string[] str_expressao = new Tokens(exprssInstanciacaoEmNotacaoWrapper).GetTokens().ToArray();

            string pattern = new TextExpression().FormaPatternResumed(Utils.OneLineTokens(str_expressao.ToList<string>()));

            List<string> tokensPattern = new Tokens(pattern).GetTokens();
            if ((tokensPattern == null) || (tokensPattern.Count == 0))
            {
                return null;
            }


            if (str_expressao[0] == "DictionaryText")
            {
                tokensPattern[0] = "DictionaryText";
            }

            if (MatchElement(tokensPattern.ToArray(), regexRigor))
            {
                //DictionaryText id = { id }
                // obtem o nome do objeto a ser instanciado.
                string nomeObjeto = str_expressao[1];
                string nomeTipoElemento = str_expressao[4];

                List<string> exprssRetorno = new List<string>();
                exprssRetorno.Add(nomeObjeto);
                exprssRetorno.Add(".");
                exprssRetorno.Add("Create");
                exprssRetorno.Add("(");
                exprssRetorno.Add(")");
                if (exprssInstanciacaoEmNotacaoWrapper.IndexOf(";") > -1)
                {
                    exprssRetorno.Add(";");
                }

                // "DictionaryText id = { id }";
                tokensProcessed = new List<string>();
                tokensProcessed.Add("DictionaryText");
                tokensProcessed.Add(nomeObjeto);
                tokensProcessed.Add("=");
                tokensProcessed.Add("{");
                tokensProcessed.Add(nomeTipoElemento);
                tokensProcessed.Add("}");
              


                // instancia um objeto [DictionaryText].
                DictionaryText dictObj = new DictionaryText();
                dictObj.acessor = "private";
                dictObj.SetNome(nomeObjeto);
                dictObj.SetTipoElement(nomeTipoElemento);
                dictObj.isWrapperObject = true;

                // registra o objeto em tempo de compilacao, para que possa fazer validacoes de expressoes, e ser idenficado neste processamento.
                escopo.tabela.GetObjetos().Add(dictObj);

                return exprssRetorno;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// constroi uma expressao chamada de metodo, para o metodo DictionaryText.GetElement(key).
        /// anotação wrapper: id {exprss}.
        /// </summary>
        /// <param name="tokensExpressaoRAW">tokens em anotacao wrapper.</param>
        /// <param name="escopo">contexto onde a anotacao wrapper está.</param>
        /// <param name="tokensProcessed">tokens consumidos na chamada de metodo GetElement.</param>
        /// <param name="indexBegin">indice de começo do tokens da anotação wrapper.</param>
        /// <returns>retorna tokens da chamada de metodo GetElement.</returns>
        public override List<string> GETChamadaDeMetodo(ref List<string> tokensExpressaoRAW, Escopo escopo, ref List<string> tokensProcessed, int indexBegin)
        {
            if ((tokensExpressaoRAW == null) || (tokensExpressaoRAW.Count == 0))
            {
                return null;
            }


            if (tokensExpressaoRAW.Count < 4)
            {
                return null;
            }


            List<string> tokensOriginal= tokensExpressaoRAW.ToList();
            List<string> tokensOriginais = tokensExpressaoRAW.GetRange(indexBegin, tokensExpressaoRAW.Count - indexBegin);
            
                


            string nomeObjeto = this.GetNameOfFirstObjectWrapper(escopo, Utils.OneLineTokens(tokensExpressaoRAW));
            if (nomeObjeto == null)
            {
                return null;
            }


            Objeto objWrapper = escopo.tabela.GetObjeto(nomeObjeto, escopo);

        

            if (!objWrapper.GetTipo().Equals("DictionaryText"))
            {
                return null;
            }
                




            int indexBeginParametros = tokensExpressaoRAW.IndexOf("{");
            if (indexBeginParametros == -1)
            {
                return null;
            }
                

            List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(indexBeginParametros, "{", "}", tokensExpressaoRAW);
            if ((tokensParametros == null) || (tokensParametros.Count == 0))
            {
                return null;
            }
                

            tokensParametros.RemoveAt(0);
            tokensParametros.RemoveAt(tokensParametros.Count - 1);

            if (tokensParametros.Count != 1)
            {
                return null;
            }



            // constroi a lista de tokens da chamada de metodo a ser feita.
            List<string> tokens_retorno = new List<string>();
            tokens_retorno.Add(nomeObjeto);
            tokens_retorno.Add(".");
            tokens_retorno.Add("GetElement");
            tokens_retorno.Add("(");
            tokens_retorno.AddRange(tokensParametros);
            tokens_retorno.Add(")");


            tokensProcessed = tokensOriginal.ToList<string>();



            int indexBracketsFecha = tokensProcessed.IndexOf("}");

            // PROCESSAMENTO de casos como: actual.vetorA[0].metodoB();
            List<string> tokensOperadorDOT = tokensProcessed.FindAll(k => k.Equals("."));
            int indexBeginNameObject = tokensProcessed.IndexOf(nomeObjeto);
            if ((tokensOperadorDOT != null) && (tokensOperadorDOT.Count > 1))
            {
                int indexOpertorDot = tokensProcessed.LastIndexOf(".");
                if ((indexOpertorDot != -1) && (indexOpertorDot + 1 <= tokensProcessed.Count - 1))
                {

                    tokensProcessed = tokensProcessed.GetRange(indexBeginNameObject, indexOpertorDot - indexBeginNameObject);
                    return tokens_retorno;
                }

            }
            else
            if ((indexBracketsFecha >= 0) && (indexBracketsFecha + 1 < tokensProcessed.Count))
            {
                tokensProcessed = tokensProcessed.GetRange(0, indexBracketsFecha + 1);
            }

            return tokens_retorno;


        }


        /// <summary>
        /// constroi uma expressao chamada de metodo, para o metodo DictionaryText.SetElement(key,value).
        /// anotaçao wrapper: id{key, value}
        /// </summary>
        /// <param name="tokens">tokens em anotacao wrapper.</param>
        /// <param name="escopo">contexto onde a anotacao wrapper está.</param>
        /// <param name="tokensProcessed">tokens consumidos para formar a chamada de metodo.</param>
        /// <param name="indexBegin">indice de começo da anotação wrapper.</param>
        /// <returns></returns>
        public override List<string> SETChamadaDeMetodo(ref List<string> tokens, Escopo escopo, ref List<string> tokensProcessed, int indexBegin)
        {
            List<string> tokensOriginal= tokens.ToList();   


            // obtem o nome do objeto wrapper.
            string nomeObjeto = tokens[0];
            Objeto objWrapper = escopo.tabela.GetObjeto(nomeObjeto, escopo);
            if ((objWrapper == null) || (!objWrapper.GetTipo().Equals("DictionaryText"))) 
                return null;
           




            int indiceSignalEquals = tokens.IndexOf("=");
            if (indiceSignalEquals == -1)
            {
                return null;
            }
                

            int indexOperadorBracas= tokens.IndexOf("{");
            if (indexOperadorBracas == -1)
            {
                return null;
            }
                


            List<string> key;
            List<string> value;

            try
            {
                key = tokens.GetRange(indexOperadorBracas + 1, indiceSignalEquals - indexOperadorBracas - 2);
                value = tokens.GetRange(indiceSignalEquals + 1, tokens.Count - (indiceSignalEquals + 1));

            }
            catch (Exception e)
            {
                UtilTokens.WriteAErrorMensage("error in set element to a wrapper object dictionary text. " + e.Message, tokensOriginal, escopo);
                return null;
            }

            // ex.: dict{key: value}
            tokensProcessed = tokensOriginal.ToList<string>();


            // ex.: dict{key:value} ----> dict.SetElement(key, value)
            List<string> tokens_retorno = new List<string>();
            tokens_retorno.Add(nomeObjeto);
            tokens_retorno.Add(".");
            tokens_retorno.Add("SetElement");
            tokens_retorno.Add("(");
            tokens_retorno.AddRange(key);
            tokens_retorno.Add(",");
            tokens_retorno.AddRange(value);
            tokens_retorno.Add(")");

            if (tokens.IndexOf(";") != -1)
            {
                tokens_retorno.Add(";");
            }


            return tokens_retorno;
                
        }




        /// <summary>
        /// tenta reconhecer o acesso a um elemento do wrapper data.
        /// </summary>
        /// <param name="tokens_expressao">tokens da expressão.</param>
        /// <param name="umaRegex">expressão regex.</param>
        /// <returns></returns>
        private static bool MatchElement(string[] tokens_expressao, Regex umaRegex)
        {
            if ((tokens_expressao != null) && (tokens_expressao.Length > 0))
            {
                string textExpression = UtilTokens.FormataEntrada(Utils.OneLineTokens(tokens_expressao.ToList<string>()));


                if (umaRegex.IsMatch(textExpression))
                {
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// retorna true se a lista de tokens parametro contem um anotacao wrapper.
        /// </summary>
        /// <param name="str_exprss">lista de tokens a investigar.</param>
        /// <returns></returns>
        public override bool IsInstantiateWrapperData(List<string> str_exprss)
        {
            if (MatchElement(str_exprss.ToArray(), regexRigor))
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// retorna true se o tipo parametro, é um wrapper data objects [DictionaryText].
        /// </summary>
        /// <param name="tipoObjeto">tipo a verificar.</param>
        /// <returns></returns>
        public override bool isWrapper(string tipoObjeto)
        {
            return tipoObjeto.Equals("DictionaryText");
        }

      

        /// <summary>
        /// retorna true se a chamada de metodo for set element.
        /// </summary>
        /// <param name="nomeObjeto"></param>
        /// <param name="tokensNotacaoWrapper"></param>
        /// <returns></returns>
        public override bool IsSetElement(string nomeObjeto, List<string> tokensNotacaoWrapper)
        {
            return tokensNotacaoWrapper.IndexOf(",") != -1;
        }


        /// <summary>
        /// faz a conversao entre um object e um DictionaryText.
        /// </summary>
        /// <param name="objtFromCasting">object contendo o valor do casting.</param>
        /// <param name="ObjToReceiveCast">objeto a receber o casting.</param>
        public override bool Casting(object objtFromCasting, Objeto ObjToReceiveCast)
        {
            if ((objtFromCasting == null) || (ObjToReceiveCast == null))
            {
                return false;
            }

            if (ObjToReceiveCast.valor.GetType() == typeof(DictionaryText)) 
            {
                DictionaryText dict1 = (DictionaryText)ObjToReceiveCast.valor;
                dict1.Casting(objtFromCasting);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// retorna nomes que identificam a instanciacao de um [DictionaryText]
        /// </summary>
        /// <returns></returns>
        public override List<string> getNamesIDWrapperData()
        {
            return new List<string>() { "DictionaryText" };
        }

        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para wrapper de dictionary text.")
            {
            }

            
            public void TesteChamadaDeMetodoCreate(AssercaoSuiteClasse assercao)
            {
                //DictionaryText id = { id }
                string exprssInstanciacao = "DictionaryText dict1 = { string }";
                Escopo escopo = new Escopo(exprssInstanciacao);
                
                WrapperDataDictionaryText wrapperData= new WrapperDataDictionaryText();
                List<string> tokensProcessed = new List<string>();
                List<string> tokensCreate = wrapperData.CREATE(ref exprssInstanciacao, escopo, ref tokensProcessed);

                assercao.IsTrue(tokensCreate != null && tokensCreate.Count > 0 && tokensCreate.Contains("Create"));
            }

        
    
         
        }
    }
}
