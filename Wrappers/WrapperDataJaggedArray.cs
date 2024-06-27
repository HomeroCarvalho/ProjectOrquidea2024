using parser;
using parser.textoFormatado;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;



namespace ParserLinguagemOrquidea.Wrappers
{
    /// <summary>
    /// classe wrapper data para jaggedArray, responsavel pela conversao de anotação wrapper, para chamadas de metodo.
    /// </summary>
    public class WrapperDataJaggedArray : WrapperData
    {


        /// <summary>
        /// padrado de instanciacao rigor.
        /// </summary>
        private string patternRigor = "JaggedArray id = [ exprss ] [ ]";
        /// <summary>
        /// regex de instanciacaao rigor.
        /// </summary>
        private Regex regexRigor;
        /// <summary>
        /// objeto WrapperData. 
        /// </summary>
        public JaggedArray array;                                               




        /// <summary>
        /// construtor vazio.
        /// </summary>
        public WrapperDataJaggedArray()
        {

            // constroi a string da expressao regex.
            TextExpression textRegex = new TextExpression();
            string textPatternRigor = textRegex.FormaExpressaoRegularGenerica(patternRigor);

            // instancia a regex para instanciacao rigor (tipada).
            regexRigor = new Regex(textPatternRigor);

            this.tipo = "JaggedArray";


        }


        /// <summary>
        /// retorna os tokens parametros se há definicao de wrapper data  jagged array.
        /// </summary>
        /// <param name="tokens">tokens contendo a definicao de jagged array.</param>
        /// <param name="index">indice de começo da definicao do wrapper data.</param>
        /// <returns></returns>
        public override List<string> isThisTypeWrapperParameter(List<string> tokens, int index)
        {
            //"JaggedArray id = [ exprss ] [ ]"---> "JaggedArray id" (como parametro);
            if (index < 0)
            {
                return null;
            }
            if ((index + 2 < tokens.Count) && (tokens[index] == "JaggedArray")) 
            {
                List<string> tokensWrapper = tokens.GetRange(index, 2);
                return tokensWrapper;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// retorna o nome do wrapper data, contido em tokens de definição.
        /// </summary>
        /// <param name="tokens">tokens contendo a definicao do wrapper data.</param>
        /// <param name="index">inicio dos tokens da definicao do wrapper object.</param>
        /// <returns></returns>
        public override string GetNameWrapperObject(List<string> tokens, int index)
        {
            // JaggedArray id = [ exprss ] [ ] ----> JaggedArray id (como parametro).
            int indexTypeData = tokens.IndexOf("JaggedArray", index);
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
        /// obtem o tipo elemento, e o contador de tokens utilizados na definição do objeto wrapper.
        /// </summary>
        /// <param name="tokens">tokens onde está a definição do objeto wrapper.</param>
        /// <param name="countTokensWrapper">contador de tokens utilizados na definição do objeto wrapper.</param>
        /// <returns>retorna o tipo elemento, e o contador de tokens utilizados.</returns>
        public override string GetTipoElemento(List<string> tokens, ref int countTokensWrapper)
        {
            /// JaggedArray id = [ exprss ] [ ]"  -----> JaggedArray id (como parametro)
        
            countTokensWrapper = 2;
            return "Object";
        }



        /// <summary>
        /// constroi uma chamada de metodo estatica que instancia um objeto wrapper jagged array.
        /// </summary>
        /// <param name="exprssInstanciacaoEmNotacaoWrapper">expressao wrapper de instanciacao.</param>
        /// <param name="escopo">contexto onde a expressao esta.</param>
        /// <param name="tokensProcessed">tokens consumidos na conversão da anotação wrapper.</param>
        /// <returns>retorna uma lista de tokens da chamada de metodo estatica.</returns>
        public override List<string> CREATE(ref string exprssInstanciacaoEmNotacaoWrapper, Escopo escopo, ref List<string> tokensProcessed)
        {

            string[] str_expressao = new Tokens(exprssInstanciacaoEmNotacaoWrapper).GetTokens().ToArray();

            if (MatchElement(str_expressao, regexRigor))
            {
                // JaggedArray id = [ exprss ] [ ]" 
                if (str_expressao.Length > 5)
                {
                    string nomeObjeto = str_expressao[1];



                    List<string> tokensOriginais = new Tokens(str_expressao.ToList<string>()).GetTokens();

                    int indexOperadorAbre = tokensOriginais.IndexOf("[");
                    List<string> tkParamsIndex_01 = UtilTokens.GetCodigoEntreOperadores(indexOperadorAbre, "[", "]", tokensOriginais);

                    if ((tkParamsIndex_01 == null) || (tkParamsIndex_01.Count == 0) || (indexOperadorAbre == -1))
                    {
                        UtilTokens.WriteAErrorMensage("error of sintax, brackets not found in a jagged array create", tokensOriginais, escopo);
                        return null;
                    }

                    tkParamsIndex_01.RemoveAt(0);
                    tkParamsIndex_01.RemoveAt(tkParamsIndex_01.Count - 1);



                    // extrai a expressao das dimensões do jagged array, que pode ser expressao, nao um numero constante.
                    List<Expressao> exprssaoSize = Expressao.ExtraiExpressoes(tkParamsIndex_01, escopo);

                    try
                    {
                        if (exprssaoSize[0].Elementos[0].tipoDaExpressao != "int")
                        {
                            UtilTokens.WriteAErrorMensage("error in extract 1o. index to a jagged array object", tokensOriginais, escopo);
                            return null;
                        }
                    }
                    catch (Exception e)
                    {
                        UtilTokens.WriteAErrorMensage("error in extract 1o. index to a jagged array object" + e.Message, tokensOriginais, escopo);
                        return null;
                    }



                    // constroi a lista de tokens da instanciacao.
                    List<string> exprssRetorno = new List<string>();
                    exprssRetorno.Add(nomeObjeto);
                    exprssRetorno.Add(".");
                    exprssRetorno.Add("Create");
                    exprssRetorno.Add("(");
                    exprssRetorno.AddRange(exprssaoSize[0].tokens);
                    exprssRetorno.Add(")");
                    if (exprssInstanciacaoEmNotacaoWrapper.IndexOf(";") > -1)
                    {
                        exprssRetorno.Add(";");
                    }

                    //JaggedArray id = [ exprssSize ] [ ]"
                    tokensProcessed = new List<string>();
                    tokensProcessed.Add("JaggedArray");
                    tokensProcessed.Add(nomeObjeto);
                    tokensProcessed.Add("=");
                    tokensProcessed.Add("[");
                    tokensProcessed.AddRange(exprssaoSize[0].tokens);
                    tokensProcessed.Add("]");
                    tokensProcessed.Add("[");
                    tokensProcessed.Add("]");
                    if (exprssInstanciacaoEmNotacaoWrapper.IndexOf(";") != -1)
                    {
                        tokensProcessed.Add(";");
                    }


                    // instancia um  objeto [JaggedArray].
                    JaggedArray jagObj = new JaggedArray();
                    jagObj.acessor = "private";
                    jagObj.SetNome(nomeObjeto);
                    jagObj.isWrapperObject = true;

                    // registra o objeto, para na compilacao de expressoes, ser identificado.
                    escopo.tabela.GetObjetos().Add(jagObj);

                    return exprssRetorno;
                }
                else
                {
                    return null;
                }
            }

            return null;


        }



        /// <summary>
        ///  converte uma anotação wrapper numa chamada de metodo "GetElement"
        ///  // anotaçao wrapper: a[indexRow][indexCol]
        /// </summary>
        /// <param name="tokensExpressao">tokens contendo a anotação wrapper.</param>
        /// <param name="exprssEmNotacaoWrapper">texto contendo uma anotação wrapper.</param>
        /// <param name="escopo">contexto onde o objeto wrapper está.</param>
        /// <param name="tokensProcessed">tokens consumido na anotacao wrapper.</param>
        /// <param name="indexBegin">indice de começo na lista de tokens, da anotação wrapper.</param>
        /// 
        /// <returns>retorna os tokens de uma chamada de metodo contendo a criacao de um jagged array.</returns>
        public override List<string> GETChamadaDeMetodo(ref List<string> tokensExpressao, Escopo escopo, ref List<string> tokensProcessed, int indexBegin)
        {
            if ((tokensExpressao == null) || (tokensExpressao.Count == 0))
            {
                return null;
            }

            // obtem os tokens da anotação wrapper para jagged array.
            List<string> tokensOriginal= tokensExpressao.ToList();
            List<string> tokensOriginais = tokensExpressao.GetRange(indexBegin, tokensExpressao.Count - indexBegin);

            

            /// obtem o nome do objeto jagged array.
            string nomeObjeto = this.GetNameOfFirstObjectWrapper(escopo, Utils.OneLineTokens(tokensExpressao));
            if (nomeObjeto == null)
            {
                return null;
            }

            Objeto objCaller = escopo.tabela.GetObjeto(nomeObjeto, escopo);
            

         
            int indexFirstIndex = tokensExpressao.IndexOf("[");
           

            if (indexFirstIndex == -1)
            {
                // ausencia de expressao [] primeiro indice.
                return null;
            }
                

            List<string> firstIndex = UtilTokens.GetCodigoEntreOperadores(indexFirstIndex, "[", "]", tokensExpressao);
            if ((firstIndex == null) || (firstIndex.Count < 2))
            {
                UtilTokens.WriteAErrorMensage("sintaxe error, brackets not found", tokensOriginal, escopo);
                return null;
            }
                

           

            int  indexSecondIndex = tokensExpressao.IndexOf("[");
            if (indexSecondIndex == -1)
            {
                UtilTokens.WriteAErrorMensage("sintaxe error, brackets not found", tokensOriginal, escopo);
                return null;
            }
                

            List<string> secondIndex= UtilTokens.GetCodigoEntreOperadores(indexSecondIndex,"[", "]", tokensExpressao);
            if ((secondIndex == null) || (secondIndex.Count < 2))
            {
                return null;
            }
                

            firstIndex.RemoveAt(0);
            firstIndex.RemoveAt(firstIndex.Count - 1);


            secondIndex.RemoveAt(0);
            secondIndex.RemoveAt(secondIndex.Count - 1);

            Expressao exprssINDICE1 = new Expressao(firstIndex.ToArray(), escopo);
            Expressao exprssINDICE2= new Expressao(secondIndex.ToArray(), escopo);

            try
            {
                if ((exprssINDICE1.Elementos[0].tipoDaExpressao != "int") ||
                    (exprssINDICE2.Elementos[0].tipoDaExpressao != "int"))
                {
                    UtilTokens.WriteAErrorMensage("error in extract indexes to jaggedArray object", tokensOriginal, escopo);
                    return null;
                }
            }
            catch (Exception e)
            {
                UtilTokens.WriteAErrorMensage("error in extract indexes to jaggedArray object: " + e.Message, tokensOriginal, escopo);
                return null;
            }


            // ex.: m[1][5] --> m.GetElement(1,5);
            List<string> tokens_retorno = new List<string>();
            tokens_retorno.Add(nomeObjeto);
            tokens_retorno.Add(".");
            tokens_retorno.Add("GetElement");
            tokens_retorno.Add("(");
            tokens_retorno.AddRange(exprssINDICE1.tokens);
            tokens_retorno.Add(",");
            tokens_retorno.AddRange(exprssINDICE2.tokens);
            tokens_retorno.Add(")");
            
            

            // ex.: m[1][5]
            tokensProcessed = tokensOriginal.ToList<string>();


            int indexFirstColchetesFecha = tokensProcessed.LastIndexOf("]");
            if ((indexFirstColchetesFecha >= 0) && (indexFirstColchetesFecha + 1 < tokensProcessed.Count))
            {
                tokensProcessed = tokensProcessed.GetRange(0, indexFirstColchetesFecha + 1);
            }


            return tokens_retorno;

           
        }
        /// <summary>
        /// converte uma anotação wrapper para uma expressao chamada de metodo.
        /// ex.; a[row][col]= valor.
        /// </summary>
        /// <param name="tokens">lista de tokens contendo anotação wrapper SET.</param>
        /// <param name="escopo">escopo onde o codigo wrapper está.</param>
        /// <param name="tokensProcessed">tokens consumido na anotacao wrapper.</param>
        /// <param name="indexBegin">indice de começo da anotação, dentro dos tokens parametro.</param>
        /// <returns>retorna os tokens de uma chamada de metodo de setar um elemento de jagged array.</returns>
        public override List<string> SETChamadaDeMetodo(ref List<string> tokens, Escopo escopo, ref List<string> tokensProcessed, int indexBegin)
        {

            List<string> tokensOriginal = tokens.ToList();

            if ((tokens == null) || (tokens.Count == 0))
                return null;



            string nomeObjeto = tokens[0];
            Objeto objCaller = escopo.tabela.GetObjeto(nomeObjeto, escopo);
            if ((objCaller == null) || (!objCaller.GetTipo().Equals("JaggedArray")))
                return null;



            int indexNomeObjeto = tokensOriginal.IndexOf(nomeObjeto);
            int indexEquals = tokens.IndexOf("=");
            if ((indexEquals == -1) || (indexEquals == -1))
            {
                return null;
            }




            int indexFirstIndex = tokens.IndexOf("[");
            if (indexFirstIndex == -1)
            {
                return null;
            }






            List<string> firstIndex = UtilTokens.GetCodigoEntreOperadores(indexFirstIndex, "[", "]", tokens);
            if ((firstIndex == null) || (firstIndex.Count == 0))
            {
                return null;
            }


            firstIndex.RemoveAt(0);
            firstIndex.RemoveAt(firstIndex.Count - 1);




            int indexSecondIndex = tokens.IndexOf("[");
            if (indexSecondIndex == -1)
            {
                return null;
            }


            List<string> secondIndex = UtilTokens.GetCodigoEntreOperadores(indexSecondIndex, "[", "]", tokens);
            if ((secondIndex == null) || (secondIndex.Count == 0))
                return null;


            secondIndex.RemoveAt(0);
            secondIndex.RemoveAt(secondIndex.Count - 1);


            Expressao exprssINDICE1 = new Expressao(firstIndex.ToArray(), escopo);
            Expressao exprssINDICE2 = new Expressao(secondIndex.ToArray(), escopo);

            try
            {
                if ((exprssINDICE1.Elementos[0].tipoDaExpressao != "int") ||
                    (exprssINDICE2.Elementos[0].tipoDaExpressao != "int"))
                {
                    UtilTokens.WriteAErrorMensage("error in extract indexes in jagged array object", tokensOriginal, escopo);
                    return null;
                }
            }
            catch (Exception e)
            {
                UtilTokens.WriteAErrorMensage("error in extract indexes in jagged array object: " + e.Message, tokensOriginal, escopo);
                return null;
            }



            indexEquals = tokens.IndexOf("=");

            // obtem os tokens da expressao que representa o valor a ser setado.
            List<string> tokensValor = tokens.GetRange(indexEquals + 1, tokens.Count - (indexEquals + 1));
            tokensValor.Remove(";");

            Expressao exprssVALOR = new Expressao(tokensValor.ToArray(), escopo);
            try
            {
                if (exprssVALOR.Elementos[0] == null)
                {
                    UtilTokens.WriteAErrorMensage("error in extract a expression of value, for a jagged array object", tokensOriginal, escopo);
                    return null;
                }
            }
            catch
            {
                UtilTokens.WriteAErrorMensage("error in extract a expression of value, for a jagged array object", tokensOriginal, escopo);
                return null;
            }




            // ex.: m[1][5] ---> m.GetElement(1,5).

            List<string> tokens_retorno = new List<string>();
            tokens_retorno.Add(nomeObjeto);
            tokens_retorno.Add(".");
            tokens_retorno.Add("SetElement");
            tokens_retorno.Add("(");
            tokens_retorno.AddRange(exprssINDICE1.tokens);
            tokens_retorno.Add(",");
            tokens_retorno.AddRange(exprssINDICE2.tokens);
            tokens_retorno.Add(",");
            tokens_retorno.AddRange(tokensValor);
            tokens_retorno.Add(")");
            if (tokens.IndexOf(";") != -1)
            {
                tokens_retorno.Add(";");
            }

            // ex.: m[1][2].
            tokensProcessed = tokensOriginal.ToList<string>();

            
            
            


           
            return tokens_retorno;
        }



   


        
        
        /// <summary>
        /// retorna true se os tokens contem uma anotacao wrapper. LEGADO.
        /// </summary>
        /// <param name="tokens_expressao"></param>
        /// <param name="umaRegex"></param>
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
        /// retorna true se a lista de tokens contem uma anotação wrapper.
        /// </summary>
        /// <param name="str_exprss">lista de tokens a verificar.</param>
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
        /// retorna true se o tipo parametro é um JaggedArray tipo.
        /// </summary>
        /// <param name="tipoObjeto">tipo parametro a investigar.</param>
        /// <returns></returns>
        public override bool isWrapper(string tipoObjeto)
        {
            return tipoObjeto.Equals("JaggedArray");
        }

        /// <summary>
        /// verifica se os tokens da anotação é de uma chamada de metodo set element.
        /// </summary>
        /// <param name="nomeObjeto">nome do objeto wrapper.</param>
        /// <param name="tokensNotacaoWrapper">tokens da anotação wrapper, a investigar.</param>
        /// <returns>[true] se a anotação é de set element.</returns>
        public override bool IsSetElement(string nomeObjeto, List<string> tokensNotacaoWrapper)
        {
            int indexNomeObjeto = tokensNotacaoWrapper.IndexOf(nomeObjeto);
            int indexSinalIgual = tokensNotacaoWrapper.IndexOf("=");
            if (indexSinalIgual == -1)
            {
                return false;
            }
            if (indexNomeObjeto < indexSinalIgual)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// faz o casting entre um object e um JaggedArray.
        /// </summary>
        /// <param name="objtFromCasting">object contendo o valor do casting.</param>
        /// <param name="ObjToReceiveCast">JaggedArray a receber o casting.</param>
        public override bool Casting(object objtFromCasting, Objeto ObjToReceiveCast)
        {
            if ((objtFromCasting == null) || (ObjToReceiveCast == null))
            {
                return false;
            }

            if (ObjToReceiveCast.valor.GetType() == typeof(JaggedArray)) 
            {
                JaggedArray j1 = (JaggedArray)ObjToReceiveCast.valor;
                j1.Casting(objtFromCasting);

                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// retorna nomes que identificam uma instanciacao de um jagged array.
        /// </summary>
        /// <returns></returns>
        public override List<string> getNamesIDWrapperData()
        {
            return new List<string>() { "JaggedArray" };
        }


        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("Testes para WrapperJaggedArray")
            {
            }


            public void TesteChamadaDeMetodoCreate(AssercaoSuiteClasse assercao)
            {
                // JaggedArray id = [ exprss ] [ ]".
                string exprssInstanciacao = "JaggedArray j1 = [ 20 ] [ ]";
                Escopo escopo = new Escopo(exprssInstanciacao);

                List<string> tokensProcessed= new List<string>();
                WrapperDataJaggedArray wrapperData = new WrapperDataJaggedArray();
                List<string> tokensCreate = wrapperData.CREATE(ref exprssInstanciacao, escopo, ref tokensProcessed);

                assercao.IsTrue(tokensCreate != null && tokensCreate.Count > 0 && tokensCreate.Contains("Create"));
            }


         

            public void TesteConstrucaoDeChamadaDeMetodoGET(AssercaoSuiteClasse assercao)
            {
                string str_acessoAElemento = "m[1][2]";
                List<string> tokensGet = new Tokens(str_acessoAElemento).GetTokens();
                JaggedArray array = new JaggedArray(20);
                array.SetNome("m");
               

                List<string> tokensProcessed = new List<string>();

                Escopo escopoTeste = new Escopo(str_acessoAElemento);
                escopoTeste.tabela.RegistraObjeto(array);

                WrapperDataJaggedArray wrapperJagged = new WrapperDataJaggedArray();
                List<string> exprssChamdaGET = wrapperJagged.GETChamadaDeMetodo(ref tokensGet, escopoTeste, ref tokensProcessed, 0);


                UtilTokens.PrintTokens(exprssChamdaGET, "tokens da chamada de metodo get element.");
                UtilTokens.PrintTokens(tokensProcessed, "tokens anotação wrapper retirados.");



                 assercao.IsTrue(exprssChamdaGET != null, "validacao de chamada de metodo GetElement");
          

            }

            public void TesteConstrucaoDeChamadaDeMetodoSETWrapper(AssercaoSuiteClasse assercao)
            {
                
                JaggedArray array = new JaggedArray(15);
                array.SetNome("m");




                string str_acessoAElemento = "m[1][1]=5;";
                Escopo escopoTeste = new Escopo(str_acessoAElemento);
                escopoTeste.tabela.RegistraObjeto(array);

                List<string> tokensGet = new Tokens(str_acessoAElemento).GetTokens();
                List<string> tokensProcessed = new List<string>();

                WrapperDataJaggedArray wrapperJagged = new WrapperDataJaggedArray();
                List<string> exprssChamadaSET = wrapperJagged.SETChamadaDeMetodo(ref tokensGet, escopoTeste, ref tokensProcessed, 0);


                UtilTokens.PrintTokens(exprssChamadaSET, "tokens chamada de metodo set elemenet: ");
                UtilTokens.PrintTokens(tokensProcessed, "tokens anotação wrapper retirados: ");



                assercao.IsTrue(exprssChamadaSET != null, "validacao de chamada de metodo setElement");




               
             

            }

        }


    }
}
