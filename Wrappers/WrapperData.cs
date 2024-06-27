using ParserLinguagemOrquidea.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wrappers;


namespace parser
{


    /// <summary>
    /// classe base para tipos de WrappersData. Provê métodos que deverão ser implementados
    /// para que uma estrutura de dados Wrapper seja funcional.
    /// </summary>
    public abstract class WrapperData:Objeto
    {

        /// <summary>
        /// retorna o processador wrapper data, para processamento do objeto parametro.
        /// </summary>
        /// <param name="wrapperObjeto">objeto wrapper para processamento.</param>
        /// <returns>[o wrapper data apropriado]</returns>
        /// <exception cref="Exception">lança uma exceção quando o tipo de wrapper não foi reconhecido.</exception>
        public static WrapperData GetWrapperData(Objeto wrapperObjeto)
        {
            switch (wrapperObjeto.tipo)
            {
                case "Matriz": return new WrapperDataMatriz();
                case "JaggedArray": return new WrapperDataJaggedArray();
                case "DictionaryText": return new WrapperDataDictionaryText();
                case "Vector": return new WrapperDataVector();
                default:
                    throw new Exception("wrapper data type unknown");
            }
        }

        
        


        /// <summary>
        /// obtem todos tipos de wrapper.
        /// deve conter TODOS tipos de wrapper.
        /// </summary>
        /// <returns>retorna uma lista de todos wrapper data.</returns>
        public static List<WrapperData> GetAllTypeWrappers()
        {
            WrapperDataVector dataVector = new WrapperDataVector();
            WrapperDataDictionaryText dataDictionaryText = new WrapperDataDictionaryText();
            WrapperDataJaggedArray dataJaggedArray = new WrapperDataJaggedArray();
            WrapperDataMatriz dataMatriz = new WrapperDataMatriz();

            return new List<WrapperData>() { dataVector, dataDictionaryText, dataMatriz, dataJaggedArray };
        }


        /// <summary>
        /// obtem todos tipos de wrapper data gerenciadores.
        /// </summary>
        /// <returns></returns>
        public static List<WrapperData> Get_ALL_WrapperData()
        {
            return new List<WrapperData>() { new WrapperDataMatriz(),
                                             new WrapperDataVector(),
                                             new WrapperDataJaggedArray(),
                                             new WrapperDataDictionaryText()};
        }

        /// <summary>
        /// faz a copia POR VALOR do objeto [valor], para o objeto [objWrapperObject]
        /// </summary>
        /// <param name="objWrapperObject">objeto wrapper a receber valor.</param>
        /// <param name="valor">valor a atribuir.</param>
        public static Objeto SetValor(object objWrapperObject, object valor)
        {
            if (objWrapperObject == null)
            {
                return null;
            }

            string tipoWrapper = objWrapperObject.GetType().Name;
            switch (tipoWrapper)
            {
                case "Vector":
                    Vector vValorVector = (Vector)valor;
                    Vector vAAtribuir = Vector.Clone(vValorVector);
                    Vector vResult = (Vector)objWrapperObject;
                    vResult = vAAtribuir;
                    vResult.valor= vResult;
                    return vResult;
                case "Matriz":
                    Matriz mValorMatriz=(Matriz)valor;
                    Matriz mAtribuir= Matriz.Clone(mValorMatriz);
                    Matriz mResult = (Matriz)objWrapperObject;
                    mResult = mAtribuir;
                    mResult.valor= mResult;
                    return mResult;

                case "DictionaryText":
                    DictionaryText dictValorDicionario= (DictionaryText)valor;
                    DictionaryText dicAtribuir= DictionaryText.Clone(dictValorDicionario);
                    DictionaryText dicResult = (DictionaryText)objWrapperObject;
                    dicResult = dicAtribuir;
                    dicResult.valor= dicResult;
                    return dicResult;

                case "JaggedArray":
                    JaggedArray jValorJagged = (JaggedArray)valor;
                    JaggedArray jAtribuir= JaggedArray.Clone(jValorJagged);
                    JaggedArray jResult= (JaggedArray)objWrapperObject;
                    jResult = jAtribuir;
                    jResult.valor= jResult;
                    return jResult;

                case "Objeto":
                    Objeto oResult= (Objeto)objWrapperObject;
                    oResult.valor = valor;
                    return oResult;
            }

            return null;
        }

        /// <summary>
        /// instancia um Wrapper Object.
        /// </summary>
        /// <param name="tipo">classe WrapperObject</param>
        /// <returns></returns>
        public static object InstantiateWrapperObject(string tipo)
        {
            switch (tipo)
            {
                case "Vector": return new Vector();
                case "Matriz": return new Matriz();
                case "JaggedArrray": return new JaggedArray();
                case "DictionaryText": return new DictionaryText();

            }
            return null;
        }

        /// <summary>
        /// faz uma copia do wrapper object parametro.
        /// </summary>
        /// <param name="objAClonar"></param>
        /// <returns></returns>
        public static Objeto Clone(Objeto objAClonar)
        {
            if (objAClonar == null)
            {
                return null;
            }
            string classeObjetoWrapper = objAClonar.GetTipo();
            Objeto objClone = null; ;
            switch (classeObjetoWrapper)
            {
                case "Vector":
                    objClone = Vector.Clone(objAClonar);
                    break;
                case "Matriz":
                    objClone= Matriz.Clone(objAClonar);
                    break;
                case "JaggedArray":
                    objClone = JaggedArray.Clone(objAClonar);
                    break;
                case "DictionaryText":
                    objClone = DictionaryText.Clone(objAClonar);
                    break;

            }
            if (objClone != null)
            {
                objClone.acessor = objAClonar.acessor;
                objClone.nome = objAClonar.nome;
                objClone.tipo =  objAClonar.tipo;
                objClone.valor = objAClonar.valor;
                objClone.tipoElemento = objAClonar.tipoElemento;
                objClone.isWrapperObject = objAClonar.isWrapperObject;
                objClone.isStatic = objAClonar.isStatic;
                objClone.isMultArgument = objAClonar.isMultArgument;
                objClone.isMethod = objAClonar.isMethod;
                objClone.actual= objAClonar.actual;
            }
            return objClone;
          
        }

        /// <summary>
        /// retorna true se objeto do tipo object é um Wrapper Object.
        /// </summary>
        /// <param name="objetoWrapper">object podendo ser Wrapper Object.</param>
        /// <returns></returns>
        public static new bool isWrapperObject(object objetoWrapper)
        {
            if (objetoWrapper == null)
            {
                return false;
            }
            else
            {
                return ((objetoWrapper.GetType() == typeof(Vector)) || (objetoWrapper.GetType() == typeof(Matriz)) ||
                    (objetoWrapper.GetType() == typeof(JaggedArray)) || (objetoWrapper.GetType() == typeof(DictionaryText)));
            }
            
        }
       
        /// <summary>
        /// retorna true se é um dos tipos de wrapper data object.
        /// </summary>
        /// <param name="tipo">nome da classe a investigar.</param>
        /// <returns></returns>
        public static string isWrapperData(string tipo)
        {
            if ((tipo == "Vector") || (tipo == "Matriz") || (tipo == "DictionaryText") || (tipo == "JaggedArray"))
            {
                return tipo;
            }
            else
            {
                return null;
            }
            
        }

        /// <summary>
        /// retornam tokens que identificam a definicao de um wrapper data. p.ex. Vector vetor1, o id=Vector.
        /// </summary>
        /// <returns>retorna uma lista de tokens idenficadores.</returns>
        public abstract List<string> getNamesIDWrapperData();
        


        /// <summary>
        /// verifica se um determinado objeto é um tipo wrapper data.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns>retorna a lista de tokens do wrapper data, se for wrapper, ou null se nao for wrapper data.</returns>
        public abstract List<string> isThisTypeWrapperParameter(List<string> tokens, int index);

        /// <summary>
        /// obtem o nome do objeto wrapper contido nos tokens.
        /// </summary>
        /// <param name="tokens">tokens da definicao do wrapper data object.</param>
        /// <returns></returns>
        public abstract string GetNameWrapperObject(List<string> tokens, int index);

       
        /// <summary>
        /// obtem o tipo de elemento constituinte do objeto wrapper.
        /// </summary>
        /// <param name="tokens">tokens contendo a definicao do objeto wrapper.</param>
        /// <param name="countTokensWrapper">contador de tokens utilizado na definicao do wrapper object.</param>
        /// 
        /// <returns>retorna o tipo do elemento, se houver, ou null se não for um objeto wrapper ou nao tiver tipo elemento especifico.</returns>
        public abstract string GetTipoElemento(List<string> tokens, ref int countTokensWrapper);


        /// <summary>
        /// verifica se os tokens contem uma instanciacao de objeto wrapper.
        /// </summary>
        /// <param name="str_exprss"></param>
        /// <returns></returns>
        public abstract bool IsInstantiateWrapperData(List<string> str_exprss);

        /// <summary>
        /// verifica se é uma estrutura de dados wrapper.
        /// </summary>
        /// <param name="tipoObjeto">tipo do objeto, wrapper ou nao.</param>
        /// <returns></returns>
        public abstract bool isWrapper(string tipoObjeto);


        /// <summary>
        /// verifica se os tokens da anotação é de uma chamada de metodo set element.
        /// </summary>
        /// <param name="tokensNotacaoWrapper">tokens da anotação wrapper, a investigar.</param>
        /// <returns>[true] se a anotação é de set element.</returns>
        public abstract bool IsSetElement(string nomeObjeto, List<string> tokensNotacaoWrapper);




        /// <summary>
        /// converte uma instanciação wrapper, em um construtor via chamada de metodo [Create],
        /// </summary>
        /// <param name="exprssInstanciacaoEmNotacaoWrapper">anotação da instanciação do objeto wrapper data.</param>
        /// <param name="escopo">contexto onde está o objeto a ser instanciado.</param>
        /// <returns></returns>
        public abstract List<string> CREATE(ref string exprssInstanciacaoEmNotacaoWrapper, Escopo escopo, ref List<string> tokensProcessed);


        /// <summary>
        /// converte uma anotação wrapper em uma lista de tokens para uma expressao chamada de metodo, em metodo getElement..
        /// </summary>
        /// <param name="exprssEmNotacaoWrapper">expressao contendo os dados do elemento, ex.: M[1],M[1,4,8], M[x,y,z+1].</param>
        /// <param name="escopo">contexto onde a expressão está.</param>
        /// <param name="tokensProcessed">tokens consumidos da chamada de objeto.</param>
        /// <returns>retorna uma expressao chamada de metodo contendo os dados da expressao wrapper de getElement.</returns>
        public abstract List<string> GETChamadaDeMetodo(ref List<string> tokens, Escopo escopo, ref List<string> tokensProcessed, int indexBegin);



        /// <summary>
        /// converte uma anotação wrapper em uma lista de tokens para uma expressao chamada de metodo.
        /// </summary>
        /// <param name="exprssEmNotacaoWrapper">expressao em anotação wrapper. ex>: M[1,4,8]=5.</param>
        /// <param name="escopo">contexto onde a expressão wrapper está.</param>
        /// <param name="tokensProcessed">tokens consumidos na chamada de metodo.</param>
        /// <returns>retorna uma expressao chamada de metodo contendo os dados da expressao wraper de setElement.</returns>
        public abstract List<string> SETChamadaDeMetodo(ref List<string> tokens, Escopo escopo, ref List<string> tokensProcessed, int indexBegin);




        /// <summary>
        /// econtra o proximo wrapper object.
        /// </summary>
        /// <param name="tokensOriginais">tokens da expressao inteira.</param>
        /// <param name="offset">offset</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        /// <returns></returns>
        public List<string> NextWrapperObject(List<string> tokensOriginais, int offset, Escopo escopo)
        {
            for (int i = offset; i < tokensOriginais.Count; i++)
            {
                if ((escopo.tabela.GetObjeto(tokensOriginais[i], escopo) != null) &&
                    (escopo.tabela.GetObjeto(tokensOriginais[i], escopo).isWrapperObject))
                {
                    return tokensOriginais.GetRange(i, tokensOriginais.Count - i);
                }
            }
            return new List<string>();
        }



        /// <summary>
        /// faz o casting DE VALOR com um object contendo o valor do casting.
        /// </summary>
        /// <param name="objToCasting">object com o valor do casting.</param>
        public static void CastingObjeto(object objToCasting, ref Objeto objReceive)
        {
            if (objToCasting != null)
            {

                if (objToCasting.GetType() == typeof(Vector))
                {
                    objReceive = ((Vector)objToCasting).Clone();
                }
                else
                if (objToCasting.GetType() == typeof(Matriz))
                {
                    objReceive = ((Matriz)objToCasting).Clone();
                }
                else
                if (objToCasting.GetType() == typeof(DictionaryText))
                {
                    objReceive = ((DictionaryText)objToCasting).Clone();
                }
                else
                if (objToCasting.GetType() == typeof(JaggedArray))
                {
                    objReceive = ((JaggedArray)objToCasting).Clone();
                }
                else
                if (objToCasting.GetType() == typeof(Objeto))
                {
                    objReceive.valor = ((Objeto)objToCasting).valor;
                    return;
                }



            }
        }


        /// <summary>
        /// retorna o indice do primeiro objeto wrapper, dentro do escopo.
        /// </summary>
        /// <param name="escopo">contexto onde os objetos wrapper está,</param>
        /// <param name="exprssEmNotacaoWWrapper">texto contendo a expressao wrapper.</param>
        /// <returns></returns>
        public string GetNameOfFirstObjectWrapper(Escopo escopo, string exprssEmNotacaoWWrapper)
        {
            List<Objeto> objWrapper = escopo.tabela.GetObjetos().FindAll(k => k.isWrapperObject == true);
            if ((objWrapper == null) || (objWrapper.Count == 0))
            {
                return null;
            }

            List<string> tokensNotacaoWrapper = new Tokens(exprssEmNotacaoWWrapper).GetTokens();
            if ((tokensNotacaoWrapper == null) || (tokensNotacaoWrapper.Count == 0))
            {
                return null;
            }

            for (int i = 0; i < objWrapper.Count; i++)
            {
                int indexObjWrapper = tokensNotacaoWrapper.FindIndex(k => k.Equals(objWrapper[i].GetNome()));
                if (indexObjWrapper != -1)
                {
                    return objWrapper[i].GetNome();
                }
            }

            return null;
        }

        /// <summary>
        /// faz a conversao de um object [objToCasting] de um wrapper object, para um Objeto.
        /// </summary>
        /// <param name="objtFromCasting">objeto contendo o conteudo do casting.</param>
        /// <param name="ObjToReceiveCast">objeto a receber o casting.</param>
        public abstract bool Casting(object objtFromCasting, Objeto ObjToReceiveCast);
    
        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes de wrappers data funcoes")
            {
            }

            public void TesteGetNomeWrapperDataObject(AssercaoSuiteClasse assercao)
            {
                string code_0_1_Vector = "int[] vetor1[20]";
                string code_0_2_Matriz = "int [,] mt_1= [20,20]";
                string code_0_3_JaggedArray = "JaggedArray j1 = [ 20 ] [ ]";
                string code_0_4_DictionaryText = "DictionaryText dict1 = { string }";

                List<string> tokens_0_1_vector = new Tokens(code_0_1_Vector).GetTokens();
                List<string> tokens_0_1_matriz = new Tokens(code_0_2_Matriz).GetTokens();
                List<string> tokens_0_1_jagged = new Tokens(code_0_3_JaggedArray).GetTokens();
                List<string> tokens_0_1_dictionary = new Tokens(code_0_4_DictionaryText).GetTokens();

                List<WrapperData> wrappersDefinition = new List<WrapperData>() {
                    new WrapperDataVector(),
                    new WrapperDataMatriz(),
                    new WrapperDataDictionaryText(),
                    new WrapperDataJaggedArray()};

                int countAssercoes = 0;
                int countTokens = 0;
                string tipoElemento = null;
                for (int i = 0; i < wrappersDefinition.Count; i++)
                {
                    if (wrappersDefinition[i].isThisTypeWrapperParameter(tokens_0_1_vector, 0) != null) 
                    {

                       
                        string name = wrappersDefinition[i].GetNameWrapperObject(tokens_0_1_vector, 0);
                        tipoElemento = wrappersDefinition[i].GetTipoElemento(tokens_0_1_vector, ref countTokens);
                        if ((name == "vetor1") && (tipoElemento=="int"))
                        {
                            countAssercoes++;
                        }
                        assercao.IsTrue(name == "vetor1" && tipoElemento=="int", code_0_1_Vector);
                        
                    }

                    if (wrappersDefinition[i].isThisTypeWrapperParameter(tokens_0_1_matriz,0) != null)
                    {
                        string name = wrappersDefinition[i].GetNameWrapperObject(tokens_0_1_matriz, 0);
                        tipoElemento = wrappersDefinition[i].GetTipoElemento(tokens_0_1_matriz, ref countTokens);

                        if (name == "mt_1" && tipoElemento=="int")
                        {
                            countAssercoes++;
                        }
                        assercao.IsTrue(name == "mt_1" && tipoElemento == "int", code_0_2_Matriz);

                    }

                    if (wrappersDefinition[i].isThisTypeWrapperParameter(tokens_0_1_jagged, 0) != null)
                    {
                        string name = wrappersDefinition[i].GetNameWrapperObject(tokens_0_1_jagged, 0);
                        if (name == "j1")
                        {
                            countAssercoes++;
                        }
                        assercao.IsTrue(name == "j1", code_0_3_JaggedArray);
                        
                    }

                    if (wrappersDefinition[i].isThisTypeWrapperParameter(tokens_0_1_dictionary, 0) != null)
                    {
                        string name = wrappersDefinition[i].GetNameWrapperObject(tokens_0_1_dictionary, 0);
                        tipoElemento= wrappersDefinition[i].GetTipoElemento (tokens_0_1_dictionary, ref countTokens);
                        if (name == "dict1" && tipoElemento == "string")
                        {
                            countAssercoes++;
                        }
                        assercao.IsTrue(name == "dict1", code_0_4_DictionaryText);
                        
                    }
                }
            
                assercao.IsTrue(countAssercoes == 4, "contador de assercoes true feito.");
            }

          
        }

    }

}
