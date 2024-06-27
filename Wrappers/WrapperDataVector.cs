using parser.ProgramacaoOrentadaAObjetos;
using parser.textoFormatado;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text.RegularExpressions;


namespace parser
{
    /// <summary>
    /// classe responsavel pela conversão de uma anotação wrapper vector, em chamadas de metodo.
    /// </summary>
    public class WrapperDataVector : WrapperData
    {
        // objeto wrapper data.

        public Vector vectorElements;                                   


        


        /// instanciacao tipada.  
        private string codigoInstanciacaoRigor = "Vector id [ exprss ]";
        // instanciacao classica.
        private string coddigoInstanciacaoClassica = "id [ ] id [ exprss ]";


        // regex para instanciacao rigor.
        protected static Regex regexInstanciacaoResumidaRigor;
        // regex para instanciacao classica. 
        protected static Regex regexInstanciacaoResumidaClassica;       


     
        /// <summary>
        /// construtor vazio.
        /// </summary>
        public WrapperDataVector()
        {

            // constroi a string da expressao regex.
            TextExpression textRegex = new TextExpression();
            string textRigor = textRegex.FormaExpressaoRegularGenerica(this.codigoInstanciacaoRigor);
            string textClassico = textRegex.FormaExpressaoRegularGenerica(this.coddigoInstanciacaoClassica);



            // instancia a expressao regex, para tipos de  instanciação resumida.
            regexInstanciacaoResumidaRigor = new Regex(textRigor);
            regexInstanciacaoResumidaClassica = new Regex(textClassico);


            this.tipo = "Vector";


        }

        /// <summary>
        /// obtem o nome de um wrrapper data object.
        /// </summary>
        /// <param name="tokens">tokens contendo a definicao do wrapper data.</param>
        /// <param name="index">indice de começo da anotação wrapper.</param>
        /// <returns>retorna o nome do objeto, null se não for uma anotação wrapper.</returns>
        public override string GetNameWrapperObject(List<string> tokens, int index)
        {
           

            if ((tokens[index] == "Vector") && (tokens.Count >= 1))
            {
                // instanciacao tipada.
                // codigoInstanciacaoRigor = "Vector id [ exprss ]" ---> Vector id (como parametro).
                return tokens[index + 1];
            }
            else
            if ((index + 2 < tokens.Count) && (tokens[index + 1] == "[") && (tokens[index + 2] == "]") && (TextExpression.IsID(tokens[index])) && (TextExpression.IsID(tokens[index + 3]))) 
            {
                // instanciacao classica.
                // coddigoInstanciacaoClassica = "id [ ] id [ exprss ]";--> id [ ] id (como parametro).  
                return tokens[tokens.IndexOf("]") + 1];
            }
            else
            {
                return null;
            }
           
        }


        /// <summary>
        /// obtem o tipo elemento do vetor.
        /// </summary>
        /// <param name="tokens">tokens contendo a definicao do wrapper object.</param>
        /// <param name="countTokens">contador de tokens utilizado na definicao do wrapper vector.</param>
        /// <returns></returns>
        public override string GetTipoElemento(List<string> tokens, ref int countTokens)
        {
            // "Vector id [ exprss ]" ---> Vector id (como parametro;  
            if (tokens.IndexOf("Vector") != -1)
            {
                countTokens = 2;
                return "Object";
            }
            else
            // "id [ ] id [ exprss ]   ---> id [ ]  id  (como parametro).
            {
                int indexTipoElemento = tokens.IndexOf("[");
                if (indexTipoElemento - 1 >= 0) 
                {
                    countTokens = 4;
                    return tokens[indexTipoElemento - 1];
                }
            }

            return null;

            

        }

      
        /// <summary>
        /// verifica se ha um wrapper data vector.
        /// </summary>
        /// <param name="tokens">tokens onde está contido o wrapper data.</param>
        /// <param name="index">indice de começo da definição do wrapper vector.</param>
        /// <returns></returns>
        public override List<string> isThisTypeWrapperParameter(List<string> tokens, int index)
        {

            // codigoInstanciacaoRigor = "Vector id [ exprss ]";  ----> "Vector id" (como parametro) ;
            // coddigoInstanciacaoClassica = "id [ ] id [ exprss ]";  ----> id[] id (como parametro); 

            if ((index + 2 <= tokens.Count) && (tokens[index] == "Vector"))
            {

                if (TextExpression.IsID(tokens[index + 1])) 
                {
                    List<string> tokensWrapper = tokens.GetRange(index, 2);
                    return tokensWrapper;
                }
                else
                {
                    return null;
                }

            }
            else
            if ((index + 3 < tokens.Count) && (tokens[index + 1] == "[") && (tokens[index + 2] == "]") &&
                (TextExpression.IsID(tokens[index])) && (TextExpression.IsID(tokens[index + 3]))) 
            {
                List<string> tokensWrapper = tokens.GetRange(index, 4);
                return tokensWrapper;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// obtem uma lista de todos valores iniciais de instanciacao de um vetor, com formato de expressoes.
        /// </summary>
        /// <param name="tokensOriginais">tokens de instanciacao, CREATE.</param>
        /// <param name="nameVector">nome do vector wrapper.</param>
        /// <param name="escopo">contexto onde os tokens estao.</param>
        /// <returns>retorna uma lista de valores, no formato de expressoes.</returns>
        private List<List<string>> InitiateValues(List<string> tokensOriginais,string nameVector, Escopo escopo)
        {
            List<List<string>> lstValoresIniciais= new List<List<string>>();
            List<List<string>> lstChamadasDeMetodoSet = new List<List<string>>();

            int indexSignalEquals = tokensOriginais.IndexOf("=");
            if (indexSignalEquals != -1)
            {
                int indexColcheteAbre = tokensOriginais.IndexOf("[", indexSignalEquals);
                if (indexColcheteAbre != -1)
                {
                    // obtem os tokens de todos valores iniciais da instanciacao do vector.
                    List<string> tokensValores = UtilTokens.GetCodigoEntreOperadores(indexColcheteAbre, "[", "]", tokensOriginais);
                    if ((tokensValores!=null) && (tokensValores.Count >= 3))
                    {
                        tokensValores.RemoveAt(0);
                        tokensValores.RemoveAt((int)tokensValores.Count - 1);

                        List<Expressao> exprssValores = Expressao.ExtraiExpressoes(tokensValores, escopo);
                        if ((exprssValores!=null) && (exprssValores.Count > 0))
                        {
                            for (int i = 0; i < exprssValores.Count; i++)
                            {
                                // monta a anotacao wrapper para o valor expressao.
                                List<string> tokensProcessedOneValue = new List<string>();
                                tokensProcessedOneValue.Add(nameVector);
                                tokensProcessedOneValue.Add("[");
                                tokensProcessedOneValue.Add(i.ToString());
                                tokensProcessedOneValue.Add("]");
                                tokensProcessedOneValue.Add("=");
                                tokensProcessedOneValue.AddRange(exprssValores[i].tokens);
                                tokensProcessedOneValue.Add(";");
                                
                                // obtem os tokens da chamada de metodo SET.
                                List<string> tokensConsumidos = new List<string>();
                                List<string> tokensChamadaDeMetodo1Valorr = SETChamadaDeMetodo(ref tokensProcessedOneValue, escopo, ref tokensConsumidos, 0);

                                // adiciona os tokens da chamada de metodo SET currente.
                                lstValoresIniciais.Add(tokensChamadaDeMetodo1Valorr);
                            }
                            
                           
                        }

                    }
                }
            }
            return lstValoresIniciais;
        }

        /// <summary>
        /// retorna uma chamada de metodo estatica que faz a construcao do objeto vetor em tempo de execução do codigo.
        /// </summary>
        /// <param name="exprssInstanciacaoEmNotacaoWrapper">expressao da instanciacao, em objetos wrapper.</param>
        /// <param name="escopo">contexto onde está a expressao.</param>
        /// <param name="tokensProcessed">tokens utilizados na construcao da instanciacao do create chamada de metodo.</param>
        /// <returns>retorna a lista de tokens da chamada de metodo de criação de objeto wrapper vector.</returns>
        public override List<string> CREATE(ref string exprssInstanciacaoEmNotacaoWrapper, Escopo escopo, ref List<string>tokensProcessed)
        {

            // obtem a lista de tokens da expressao.
            List<string> tokensOriginais = new Tokens(exprssInstanciacaoEmNotacaoWrapper).GetTokens();
            if ((tokensOriginais == null) || (tokensOriginais.Count < 5)) 
                return null;
            List<string> tokensInicializacao = tokensOriginais.ToList<string>();


            string nomeObjetoVetor = null;
            string tipoObjetoElemento = null;

            string pattern = new TextExpression().FormaPatternResumed(Utils.OneLineTokens(tokensOriginais));
            pattern = pattern.Replace("number", "exprss");
            List<string> tokensPattern = new Tokens(pattern).GetTokens();
            if ((tokensPattern == null) || (tokensPattern.Count == 0))
            {
                return null;
            }

            if (tokensPattern[0] == "id")
            {
                tokensPattern[0] = "Vector";
            }


            // INSTANCIACAO CLASSICA:   // id[] id[exprs]   
            string pattern1 = new TextExpression().FormaPatternResumed(Utils.OneLineTokens(tokensOriginais));
            pattern1 = pattern1.Replace("number", "exprss");

            List<string> tokensPatternClassic = new Tokens(pattern1).GetTokens();
            tipoObjetoElemento = tokensOriginais[0];
            nomeObjetoVetor = tokensOriginais[3];


            
            // CREATE NOTACAO RIGOR: "Vector id [ exprss ]".
            // codigoInstanciacaoRigor = "Vector id [ exprss ]";  ----> "Vector id" (como parametro) ;
            if (tokensOriginais[0]=="Vector")
            {

                List<Expressao> exprssSIZE = new List<Expressao>();
                string nomeVector = tokensOriginais[1];
                int indexTokenVector = tokensOriginais.IndexOf("Vector");
                if (indexTokenVector != -1)
                {
                    if (indexTokenVector + 1 < tokensOriginais.Count)
                    {
                        nomeVector = tokensOriginais[indexTokenVector + 1];
                        int indexSizeExpress = tokensOriginais.IndexOf("[", indexTokenVector);
                        if (indexSizeExpress != -1)
                        {
                            List<string> tokensSize = UtilTokens.GetCodigoEntreOperadores(indexSizeExpress, "[", "]", tokensOriginais);
                            if ((tokensSize != null) && (tokensSize.Count > 2))
                            {
                                tokensSize.RemoveAt(0);
                                tokensSize.RemoveAt(tokensSize.Count - 1);
                                exprssSIZE = Expressao.ExtraiExpressoes(tokensSize, escopo);
                                if ((exprssSIZE != null) && (exprssSIZE.Count > 0) &&
                                    (exprssSIZE[0].Elementos != null) &&
                                    (exprssSIZE[0].Elementos.Count > 0) &&
                                    (exprssSIZE[0].Elementos[0].tipoDaExpressao == "int")) 
                                {

                                    // CREATE NOTACAO RIGOR: "Vector id [ exprss ]".
                                    List<string> tokensRetorno = new List<string>();
                                    tokensRetorno.Add(nomeVector);
                                    tokensRetorno.Add(".");
                                    tokensRetorno.Add("Create");
                                    tokensRetorno.Add("(");
                                    tokensRetorno.AddRange(exprssSIZE[0].tokens);
                                    tokensRetorno.Add(")");
                                    if (tokensOriginais.IndexOf(";") != -1)
                                    {
                                        tokensRetorno.Add(";");
                                    }

                                    
                                    // instancia um vetor, com o tipo do elemento vindo da instanciacao.
                                    Vector vecObj = new Vector("Object");
                                    vecObj.acessor = "private";
                                    vecObj.SetNome(nomeVector);
                                    vecObj.SetTipoElement("Object");
                                    vecObj.isWrapperObject = true;
                                    // registra o vetor criado.
                                    escopo.tabela.GetObjetos().Add(vecObj);

                                    // obtem, se tiver, valores iniciais de elementos de vetor.
                                    GetChamadasDeMetodoSETParaValoresIniciais(escopo, tokensInicializacao, nomeVector, tokensRetorno);


                                    // CREATE NOTACAO RIGOR: "Vector id [ exprss ]".
                                    tokensProcessed = new List<string>();
                                    tokensProcessed.Add("Vector");
                                    tokensProcessed.Add(nomeVector);
                                    tokensProcessed.Add("[");
                                    tokensProcessed.AddRange(exprssSIZE[0].tokens);
                                    tokensProcessed.Add("]");
                                    if (tokensOriginais.IndexOf(";") != -1)
                                    {
                                        tokensProcessed.Add(";");
                                    }




                                    return tokensRetorno;

                                }


                            }
                            else
                            {
                                EvalExpression eval = new EvalExpression();
                                object size = (int)20;
                                try
                                {

                                    size = eval.EvalPosOrdem(exprssSIZE[0], escopo);

                                }
                                catch (Exception ex)
                                {
                                    string msg = ex.ToString();
                                }

                                // caso de um parametro, sem definicao de tamanho!.
                                Vector vectorCreated = new Vector("Object", (int)size);
                                vectorCreated.acessor = "private";
                                vectorCreated.SetNome(nomeVector);
                                vectorCreated.SetTipoElement("Object");
                                vectorCreated.isWrapperObject = true;
                                escopo.tabela.RegistraObjeto(vectorCreated);

                                

                                int indexSizeExpress1 = tokensOriginais.IndexOf("[", indexTokenVector);
                                List<string> tokensSize1 = UtilTokens.GetCodigoEntreOperadores(indexSizeExpress1, "[", "]", tokensOriginais);

                                List<string> tokensRetorno = new List<string>();
                                tokensRetorno.Add(nomeObjetoVetor);
                                tokensRetorno.Add(".");
                                tokensRetorno.Add("Create");
                                tokensRetorno.Add("(");
                                tokensRetorno.AddRange(tokensSize1);
                                tokensRetorno.Add(")");
                                if (tokensOriginais.IndexOf(";") > -1)
                                {
                                    tokensRetorno.Add(";");
                                }

                                // obtem, se tiver, valores iniciais de elementos de vetor.
                                GetChamadasDeMetodoSETParaValoresIniciais(escopo, tokensInicializacao, nomeVector, tokensRetorno);

                          
                                string tipoElemento = tokensOriginais[0];
                                string nomeVetor1 = tokensOriginais[3];
                                // INSTANCIACAO CLASSICA:   // id[] id[exprs] 
                                tokensProcessed = tokensOriginais.ToList<string>();

                                return tokensRetorno;

                            }
                        }
                    }
                }
            }
            // CREATE NOTACAO CLASSICA: "id[] id[exprs]".   
            if (MatchElement(tokensPatternClassic.ToArray(), WrapperDataVector.regexInstanciacaoResumidaClassica))
            {
                if ((tokensOriginais.Count >= 4) && (TextExpression.IsID(tokensOriginais[0])) && (tokensOriginais[1] == "[") && (tokensOriginais[2] == "]") && (TextExpression.IsID(tokensOriginais[3])))
                {

                    int indexOperatorBacket = tokensOriginais.IndexOf("[");
                    indexOperatorBacket = tokensOriginais.IndexOf("[", indexOperatorBacket + 1);


                    List<string> tokensParametrosConstrutor = UtilTokens.GetCodigoEntreOperadores(indexOperatorBacket, "[", "]", tokensOriginais);
                    if ((tokensParametrosConstrutor == null) || (tokensParametrosConstrutor.Count == 0))
                    {
                        return null;
                    }
                    tokensParametrosConstrutor.RemoveAt(0);
                    tokensParametrosConstrutor.RemoveAt(tokensParametrosConstrutor.Count - 1);


                    try
                    {

                        // constroi o parametro de tamanho do vetor.
                        List<Expressao> parametrosConstrutorResumido2 = Expressao.ExtraiExpressoes(tokensParametrosConstrutor, escopo);
                        if ((parametrosConstrutorResumido2[0].Elementos[0].tipoDaExpressao == "int"))
                        {
                            // constroi a chamada de metodo [Create], contendos os tokens processado na instanciacao.


                            List<string> tokensRetorno = new List<string>();
                            tokensRetorno.Add(nomeObjetoVetor);
                            tokensRetorno.Add(".");
                            tokensRetorno.Add("Create");
                            tokensRetorno.Add("(");
                            tokensRetorno.AddRange(parametrosConstrutorResumido2[0].tokens);
                            tokensRetorno.Add(")");
                            if (tokensOriginais.IndexOf(";") != -1) 
                            {
                                tokensRetorno.Add(";");
                            }

                            EvalExpression eval = new EvalExpression();
                            object size = (int)20;
                            try
                            {

                                 size = eval.EvalPosOrdem(parametrosConstrutorResumido2[0], escopo);

                            }
                            catch(Exception ex)
                            {
                                string msg = ex.ToString();
                            }

                            // instancia um vetor, com o tipo do elemento vindo da instanciacao.
                            Vector vecObj = new Vector(tipoObjetoElemento);
                            vecObj.acessor = "private";
                            vecObj.reSize((int)size);
                            vecObj.SetNome(nomeObjetoVetor);
                            vecObj.SetTipoElement(tipoObjetoElemento);
                            vecObj.isWrapperObject = true;
                            // registra o vetor criado.
                            escopo.tabela.GetObjetos().Add(vecObj);

                            // obtem, se tiver, valores iniciais de elementos de vetor.
                            GetChamadasDeMetodoSETParaValoresIniciais(escopo, tokensInicializacao, nomeObjetoVetor, tokensRetorno);


                            // CREATE NOTACAO CLASSICA: "id[] id[exprs]".   
                            tokensProcessed = tokensOriginais.ToList<string>();
                            return tokensRetorno;


                        }
                        else
                        {
                            UtilTokens.WriteAErrorMensage("error in index vector object", tokensOriginais, escopo);
                            return null;
                        }
                    }
                    catch
                    {
                        UtilTokens.WriteAErrorMensage("error in create a vector object", tokensOriginais, escopo);
                        return null;
                    }

                }

            }



            return null;

        }

        /// <summary>
        /// constroi chamadas de metodo Set, para valores iniciais passados na construcao do vector.
        /// </summary>
        /// <param name="escopo">contexto onde a anotacao wrapper esta.</param>
        /// <param name="tokensOriginais">tokens de todos valores iniciais.</param>
        /// <param name="nomeVector">nome do vetor.</param>
        /// <param name="tokensRetorno">tokens de retorno.</param>
        private void GetChamadasDeMetodoSETParaValoresIniciais(Escopo escopo, List<string> tokensOriginais, string nomeVector, List<string> tokensRetorno)
        {
            if (tokensOriginais.IndexOf("=") != -1)
            {
                // obtem tokens de chamadas de metodo SET para os valores iniciais.
                List<List<string>> tokensChamadaDeMetodosValoresIniciais = InitiateValues(tokensOriginais, nomeVector, escopo);
                if ((tokensChamadaDeMetodosValoresIniciais != null) && (tokensChamadaDeMetodosValoresIniciais.Count > 0))
                {
                    // adiciona cada chamada de metodo SET, para os tokens de retorno.
                    for (int i = 0; i < tokensChamadaDeMetodosValoresIniciais.Count; i++)
                    {
                        tokensRetorno.AddRange(tokensChamadaDeMetodosValoresIniciais[i]);
                    }
                }
            }
        }

        /// <summary>
        /// verifica se os tokens da anotação é de uma chamada de metodo set element.
        /// </summary>
        /// <param name="nomeObjeto">nome do objeto.</param>
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
        /// em tempo de compilação, é convertido uma expressão em anotação do wrapper, ex.: M[1,5]= valor,
        /// para uma expressão de instanciacao: M.SetElement(1,5, valor).
        /// extrai também dos tokens da expressão de entrada, os tokens consumidos pela anotação wrapper.
        /// </summary>
        /// <param name="tokensOriginais">tokens em anotacao wrapper.</param>
        /// <param name="escopo">contexto onde a expressão wrapper está.</param>
        /// <param name="tokensProcessed">tokens consumidos da anotacao wrapper.</param>
        /// <returns>retorna uma expressão chamada de metodo, contendo dados para setar um elemento, no indice calculado.</returns>
        public override List<string> SETChamadaDeMetodo(ref List<string> tokensOriginais, Escopo escopo, ref List<string> tokensProcessed, int indexBegin)
        {


            if ((tokensOriginais != null) && (tokensOriginais.Count > 0))
            {

 
                Objeto obj_caller = escopo.tabela.GetObjeto(tokensOriginais[indexBegin], escopo);
                if (obj_caller == null) 
                {
                    return null;
                
                }
                if (!obj_caller.isWrapperObject)
                {
                    return null;
                }

                if ((obj_caller != null) || (obj_caller.GetTipo().Equals("Vector")))
                {




                    int indexComplemento = tokensOriginais.IndexOf("=");
                    int indexOperadorAbre = tokensOriginais.IndexOf("[");

                    if ((indexOperadorAbre == -1) || (indexComplemento == -1))
                        return null;


                    // obtem os tokens que formam os parãmetros da chamada de metodo.
                    List<string> tokens_parametros = UtilTokens.GetCodigoEntreOperadores(indexOperadorAbre, "[", "]", tokensOriginais);

                    if ((tokens_parametros == null) || (tokens_parametros.Count == 0))
                    {
                        return null;
                    }
                    // obtem os tokens do indice. m[indice].
                    tokens_parametros.RemoveAt(0);
                    tokens_parametros.RemoveAt(tokens_parametros.Count - 1);

                    // EXTRAI AS EXPRESSOES PARAMETROS.
                    List<Expressao> exprssParametros = Expressao.ExtraiExpressoes(tokens_parametros, escopo);
                    try
                    {

                        if (exprssParametros[0].Elementos[0].tipoDaExpressao != "int")
                        {
                            UtilTokens.WriteAErrorMensage("error in extract index of a vector object ", tokensOriginais, escopo);
                            return null;
                        }
                    }
                    catch (Exception e)
                    {
                        UtilTokens.WriteAErrorMensage("error in extract index of a vector object " + e.Message, tokensOriginais, escopo);
                        return null;
                    }




                    int indexBeginValor = tokensOriginais.IndexOf("=");
                    if (indexBeginValor == -1)
                    {
                        return null;
                    }

                    List<string> tokensValor = tokensOriginais.GetRange(indexBeginValor, tokensOriginais.Count - indexBeginValor);
                    tokensValor.Remove("=");
                    if (tokensValor.IndexOf(";") != -1)
                    {
                        tokensValor.Remove(";");
                    }
                    Expressao exprssValor = new Expressao(tokensValor.ToArray(), escopo);

                    if ((exprssValor == null) || (exprssValor.Elementos == null || (exprssValor.Elementos.Count < 1)))
                    {
                        return null;
                    }





                    tokensProcessed = tokensOriginais.GetRange(0, tokensOriginais.Count);

                    // m[1]=5 --- > m.setElement(1,5);
                    List<string> tokens_retorno = new List<string>();
                    for (int i = 0; i < indexBegin; i++)
                    {
                        tokens_retorno.Add(tokensOriginais[i]);
                    }
                    
                    tokens_retorno.Add(obj_caller.GetNome());
                    tokens_retorno.Add(".");
                    tokens_retorno.Add("SetElement");
                    tokens_retorno.Add("(");
                    tokens_retorno.AddRange(exprssParametros[0].tokens);
                    tokens_retorno.Add(",");
                    tokens_retorno.AddRange(exprssValor.tokens);
                    tokens_retorno.Add(")");


                    indexComplemento = tokensOriginais.IndexOf("=");
                    List<string> tokensProximoWrapperObject = NextWrapperObject(tokensOriginais, indexComplemento, escopo);
                    if (tokensProximoWrapperObject.Count > 0)
                    {
                        tokensProcessed = tokensOriginais.GetRange(0, tokensOriginais.Count - tokensProximoWrapperObject.Count - 1);
                    }
                    



                    return tokens_retorno;


                }
            }
            return null;
        }



        /// <summary>
        /// converte uma expressao em notação Wrapper, para uma chamada de método.
        /// </summary>
        /// <param name="tokens_originais">tokens contendo a anotação wrapper.</param>
        /// <param name="escopo">contexto onde a notação Wrapper está.</param>
        /// <param name="tokensProcessed">tokens de retorno, que foram consumidos com o processamento para chamada de metodo.</param>
        /// <param name="indexBegin">indice de começo da notação, dentro de tokens originais.</param>
        /// <returns>retorna uma lista de tokens, contendo uma chamada de metodo de GET elemento wrapper vector.</returns>
        public override List<string> GETChamadaDeMetodo(ref List<string> tokens_originais, Escopo escopo, ref List<string> tokensProcessed, int indexBegin)
        {
            
            if ((tokens_originais != null) && (tokens_originais.Count > 0))
            {
                List<string> tokens_expressao = tokens_originais.ToList<string>();

                tokens_expressao = tokens_expressao.GetRange(indexBegin, tokens_expressao.Count - indexBegin);

                string nomeObjeto = this.GetNameOfFirstObjectWrapper(escopo, Utils.OneLineTokens(tokens_expressao));
                if (nomeObjeto == null)
                {
                    return null;
                }


                Objeto obj_caller = escopo.tabela.GetObjeto(nomeObjeto, escopo);

                if ((obj_caller != null) && (obj_caller.GetTipo().Equals("Vector")))
                {




                    int indexBeginParametros = tokens_expressao.IndexOf("[");

                    // obtem os tokens que formam os parãmetros da chamada de metodo.
                    // permite que ademais tokens não anotação wrapper, sejam processados fora do wrapper
                    List<string> tokens_parametros = UtilTokens.GetCodigoEntreOperadores(indexBeginParametros, "[", "]", tokens_expressao);

                    if ((tokens_parametros == null) || (tokens_parametros.Count < 0))
                    {
                        return null;
                    }

                    tokens_parametros.RemoveAt(0);
                    tokens_parametros.RemoveAt(tokens_parametros.Count - 1);

                    List<Expressao> expressaoIndice = Expressao.ExtraiExpressoes(tokens_parametros, escopo);
                    try
                    {
                        if (expressaoIndice[0].Elementos[0].tipoDaExpressao != "int")
                        {
                            UtilTokens.WriteAErrorMensage("error in extract index of a vector object ", tokens_expressao, escopo);
                            return null;
                        }
                    }
                    catch (Exception e)
                    {
                        UtilTokens.WriteAErrorMensage("error in extract index of a vector object:  " + e.Message, tokens_expressao, escopo);
                        return null;
                    }
                    int indexNomeObjeto = tokens_expressao.IndexOf(obj_caller.GetNome());




                    // forma os tokens processado.
                    tokensProcessed = tokens_expressao.ToList<string>();




                    // composicao dos TOKENS DE RETORNO, ORIGINALMENTE PARA 1 GetElement
                    List<string> tokens_retorno = new List<string>();
                    tokens_retorno.Add(obj_caller.GetNome());
                    tokens_retorno.Add(".");
                    tokens_retorno.Add("GetElement");
                    tokens_retorno.Add("(");
                    tokens_retorno.AddRange(expressaoIndice[0].tokens);
                    tokens_retorno.Add(")");


                    // PROCESSAMENTO de casos como: v[v[0]+3];
                    int indexFirstColchetesFecha = tokensProcessed.IndexOf("]");
                    int indexBeginNameObject = tokensProcessed.IndexOf(obj_caller.GetNome());
                    if ((indexBeginNameObject != -1) &&
                        (indexBeginNameObject+1<tokensProcessed.Count) &&
                        (tokensProcessed[ indexBeginNameObject+1]!="["))
                    {
                        
                        indexBeginNameObject = tokensProcessed.IndexOf(obj_caller.GetNome(), indexBeginNameObject + 1);
                        tokensProcessed = tokensProcessed.GetRange(indexBeginNameObject, indexFirstColchetesFecha - indexBeginNameObject + 1);
                        return tokens_retorno;
                       
                    }

                    // PROCESSAMENTO de casos como: actual.vetorA[0].metodoB();
                    List<string> tokensOperadorDOT = tokensProcessed.FindAll(k => k.Equals("."));
                    if ((tokensOperadorDOT != null) && (tokensOperadorDOT.Count > 1))
                    {
                        int indexOpertorDot = tokensProcessed.LastIndexOf(".");
                        if ((indexOpertorDot != -1) && (indexOpertorDot + 1 <= tokensProcessed.Count - 1))
                        {

                            tokensProcessed = tokensProcessed.GetRange(indexBeginNameObject, indexOpertorDot - indexBeginNameObject);
                            return tokens_retorno;
                        }

                    }


                    // PROCESSAMENTO NORMAL.
                    if ((indexFirstColchetesFecha >= 0) && (indexFirstColchetesFecha + 1 < tokensProcessed.Count))
                    {
                        tokensProcessed = tokensProcessed.GetRange(indexBeginNameObject, indexFirstColchetesFecha + 1);
                        return tokens_retorno;
                    }

                    return tokens_retorno;

                }
            }

            return null;


        }

        /// <summary>
        /// // tenta reconhecer o acesso a um elemento do wrapper data. LEGADO.
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
        /// retorna true se é uma anotação wrapper vector. LEGADO, porem ainda em utilizacao, a ser substituido.
        /// </summary>
        /// <param name="str_exprss">lista de tokens contendo a anotação wrapper de criacao de wrapper vector.</param>
        /// <returns></returns>
        public override bool IsInstantiateWrapperData(List<string> str_exprss)
        {
            if (MatchElement(str_exprss.ToArray(), regexInstanciacaoResumidaRigor))
            {
                return true;
            }
            else
            if (MatchElement(str_exprss.ToArray(), regexInstanciacaoResumidaClassica))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// retorna true se o tipo parametro, é de um wrapper vector.
        /// </summary>
        /// <param name="tipoObjeto">tipo a investigar.</param>
        /// <returns></returns>
        public override bool isWrapper(string tipoObjeto)
        {
            return tipoObjeto.Equals("Vector");
        }



        /// <summary>
        /// casting entre um object e um Vector.
        /// Faz uma copia em profundidade, caso utilização, alertar para a possivel perda de desempenho.
        /// </summary>
        /// <param name="objtFromCasting">object contendo o conteudo do casting.</param>
        /// <param name="objToReceiveCast">Objeto a receber o casting.</param>
        public override bool Casting(object objtFromCasting, Objeto objToReceiveCast)
        {
            if ((objtFromCasting == null) || (objToReceiveCast == null))
            {
                return false;
            }


            if ((objtFromCasting.GetType() == typeof(Vector)) && (objToReceiveCast.GetType() == typeof(Vector))) 
            {
                Vector vtFrom = (Vector)objToReceiveCast;
                vtFrom.Casting(objtFromCasting);
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// obtem nomes de identificam uma instanciacao wrapper object.
        /// </summary>
        /// <returns></returns>
        public override List<string> getNamesIDWrapperData()
        {
            return new List<string>() { "Vector", "[ ]" };
        }


        public new class Testes : SuiteClasseTestes
        {
            char aspas = '\u0022';
            public Testes() : base("testes wrapper vector")
            {
            }


            public void TesteClasseElementoVector(AssercaoSuiteClasse assercao)
            {
                string codde_0_0_class = "public class Entity{ public int xx;  public void Entity(int i){actual.xx=i;}; " +
                    "public Draw(){Prompt.sWrite(" + aspas + "Desenhando..." + aspas + ");};" +
                    "public Update(){ Prompt.sWrite(" + aspas + "Atualizando..." + aspas + ");};};";
                string code_0_0_create = "Entity e= create(3); Entity[] v1[20]; v1[1]= e;v1[0]=e; for (int i=0;i<5;i++){ v1[1].Update(); v1[1].Draw();};";

                ProcessadorDeID compilador = new ProcessadorDeID(codde_0_0_class + code_0_0_create);
                compilador.Compilar();

                ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                program.Run(compilador.escopo);


            }


            public void TesteExtensaoVectorFuncao(AssercaoSuiteClasse assercao)
            {
                try
                {
                    string code_0_0_program = "string[] v1[20];";
                    string code_0_0_GET = "v1[1].Hash();";

                    Classificador.TabelaHash.Instance(code_0_0_GET);

                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_program);
                    compilador.Compilar();

                    Expressao exprss_GET = new Expressao(code_0_0_GET, compilador.escopo);

                    assercao.IsTrue(exprss_GET.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo) && exprss_GET.Elementos[0].Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }

            }


 
            public void TesteExtensaoVectorPropriedadeAninhada(AssercaoSuiteClasse assercao)
            {
                string code_0_0_program = "Vector2D[] v1[20];";
                string code_0_0_GET = "v1[1].XX2D=1;";

                Classificador.TabelaHash.Instance(code_0_0_GET);

                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_program);
                compilador.Compilar();

                Expressao exprss_GET = new Expressao(code_0_0_GET, compilador.escopo);

            }

 
 
 
            public void TesteInicializacaoDeVetorElementoString(AssercaoSuiteClasse assercao)
            {
                string valor1 = aspas + "salada" + aspas;
                string valor2 = aspas + "churrasco" + aspas;
                string valor3 = aspas + "macarrao" + aspas;

                string codigoInicializacao = "string[] v1[20]= [" + valor1 + "," + valor2 + "," + valor3 + "];";
                Escopo escopo1 = new Escopo(codigoInicializacao);

                WrapperDataVector wrapper = new WrapperDataVector();


                List<string> tokensProcessed1 = new List<string>();
                List<string> tokensInicializacao = wrapper.CREATE(ref codigoInicializacao, escopo1, ref tokensProcessed1);

                try
                {
                    assercao.IsTrue(tokensInicializacao.IndexOf("SetElement") != -1, codigoInicializacao);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

            }
            public void TesteInicializacaoDeVector(AssercaoSuiteClasse assercao)
            {
                string codigoInicializacao = "int[] v1[20]=[1,2,3,4,5,6];";
                Escopo escopo1 = new Escopo(codigoInicializacao);


                WrapperDataVector wrapper = new WrapperDataVector();


                List<string> tokensProcessed1 = new List<string>();
                List<string> tokensInicializacao = wrapper.CREATE(ref codigoInicializacao, escopo1, ref tokensProcessed1);

                try
                {
                    assercao.IsTrue(tokensInicializacao.IndexOf("SetElement") != -1, codigoInicializacao);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

            }
            public void TesteCREATE(AssercaoSuiteClasse assercao)
            {
                //"Vector id [ exprss ]";  // instanciacao tipada.
                //"id [ ] id [ exprss ]";  // instanciacao classica.

                string codeCREATE1 = "Vector v1 [ 20 ]";
                Escopo escopo1 = new Escopo(codeCREATE1);
                

                WrapperDataVector wrapper = new WrapperDataVector();


                List<string> tokensProcessed1= new List<string>();
                List<string> tokensCREATE1 = wrapper.CREATE(ref codeCREATE1, escopo1, ref tokensProcessed1);


                string codeCREATE2 = "int [ ] v2 [ 20 ]";
                List<string> tokensProcessed2 = new List<string>();
                List<string> tokensCREATE2 = wrapper.CREATE(ref codeCREATE2, escopo1, ref tokensProcessed2);

                try
                {
                    assercao.IsTrue(escopo1.tabela.GetObjetos().Count == 2, tokensCREATE1 + "   " + tokensCREATE2);
                }
                catch(Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:" + ex.Message);
                }

        }

        public void TesteCasting(AssercaoSuiteClasse assercao)
        {

                Vector v1= new Vector();
                v1.nome = "v1";
                v1.SetElement(0, "2");

                Vector v2 = new Vector();
                v2.nome = "v2";
                v2.SetElement(0, "1");

                object objSim = (object)v2;

                WrapperDataVector wrapperData = new WrapperDataVector();
                wrapperData.Casting(objSim, v1);

                assercao.IsTrue(v1.GetElement(0).ToString() == "1");
            }

            public void TesteCreateChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {
                // id[] id[exprs]
                string exprssInstanciacao = "int[] vetor1[20]";
                Escopo escopo = new Escopo(exprssInstanciacao);
                WrapperDataVector wrapper = new WrapperDataVector();
                List<string> tokensProcessed = new List<string>();
                List<string> tokensChamadaCreate = wrapper.CREATE(ref exprssInstanciacao, escopo,ref tokensProcessed);

                assercao.IsTrue(tokensChamadaCreate != null && tokensChamadaCreate.Contains("Create"));



                string exprssInstanciacao2 = "Vector m[20]";
                Escopo escopo2 = new Escopo(exprssInstanciacao2);
                WrapperDataVector wrapper2 = new WrapperDataVector();
                tokensProcessed = new List<string>();
                List<string> tokensChamadaCreate2 = wrapper2.CREATE(ref exprssInstanciacao2, escopo, ref tokensProcessed);

                assercao.IsTrue(tokensChamadaCreate2 != null && tokensChamadaCreate2.Contains("Create"));

            }
        }

    }
}
