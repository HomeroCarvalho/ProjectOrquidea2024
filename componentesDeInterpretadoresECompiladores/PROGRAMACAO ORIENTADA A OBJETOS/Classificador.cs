using META_GAME.Testes;
using parser.ProgramacaoOrentadaAObjetos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    /// <summary>
    /// um elemento extraido do classificador de tokens.
    /// </summary>
    public class ElementToken
    {
        /// <summary>
        /// nome da classe a qual pertence o token.
        /// </summary>
        public string nomeClasse;

        /// <summary>
        /// nome do tipo de retorno do objeto.
        /// </summary>
        public string nomeTipo;
        /// <summary>
        /// nome do token.
        /// </summary>
        public string nomeToken;

        /// <summary>
        /// tipo do token.
        /// </summary>
        public typeToken tipoClassificacao;

        /// <summary>
        /// lista de parametro de função, ou operador.
        /// </summary>
        public List<ElementToken> parameters= new List<ElementToken>(); 
        public enum typeToken { nameClasse, nameObject, nameFunction, nameParameter, nameProperty,
                                nameOperator, nameOperand, nameKeyTerm, notClassificate, nameNumber, nameLiteral,
                                nameOperatorKeyTerm}

      

        /// <summary>
        /// instancia um elemento de classificacao.
        /// </summary>
        /// <param name="nomeClasse">nome da classe do elemento-token.</param>
        /// <param name="tipoToken">tipo do elemento.</param>
        /// <param name="nomeToken">nome do elemento.</param>
        /// <param name="nomeTipo">nome do tipo de elemento, utilizado em funcoes, metodos.</param>
        public ElementToken(string nomeClasse, typeToken tipoToken, string nomeToken, string nomeTipo)
        {
            this.nomeClasse = nomeClasse;
            this.tipoClassificacao = tipoToken;
            this.nomeToken = nomeToken;
            this.nomeTipo = nomeTipo;
        }
      
        /// <summary>
        /// adiciona um elemento parametro de uma funcao, metodo.
        /// </summary>
        /// <param name="param">elemento parametro a adicionar.</param>
        public void AddParameter(ElementToken param)
        {
            parameters.Add(param);
        }

        public override string ToString()
        {
            string res = "";
            if (nomeToken != null)
            {
                res += "token name: " + nomeToken;
            }
            res += "  type: " + tipoClassificacao;
            if (nomeClasse != null)
            {
                res += "  class: " + nomeClasse;

            }
            if (nomeTipo != null)
            {
                res += " type: " + nomeTipo;
            }

            return res;
        }

    }

    /// <summary>
    /// classe que classifica tokens.
    /// </summary>
     public class Classificador
    {

        private FileHeader headers;

        /// <summary>
        /// lista de nomes de classes verificadas.
        /// </summary>
        public List<string> nomesDeClasses= new List<string>();

        /// <summary>
        /// lista de nomes de metodos verificados.
        /// </summary>
        public List<string> nomesDeFuncoes= new List<string>();

        /// <summary>
        /// lista de nomes de propriedades verificados.
        /// </summary>
        public List<string> nomesDePropriedades= new List<string>();

        /// <summary>
        /// lista de nomes de objetos.
        /// </summary>
        public List<string> nomesDeObjetos= new List<string>();

        /// <summary>
        /// lista de nomes de operadores.
        /// </summary>
        public List<string> nomesDeOperadores= new List<string>();

        /// <summary>
        /// lista de nomes de termos-chave.
        /// </summary>
        public List<string> nomesDeTermosChave= new List<string>();

        /// <summary>
        /// lista de nomes de operadores, contidos em classes orquidea, classes importados.
        /// </summary>
        public List<string> nomesOperadoresTermosChave= new List<string>();

        /// <summary>
        /// lista de elementos de tokens, contendo dados como nomes de:  tipos, classes, metodos, propriedades, operadores, objetos.
        /// </summary>
        public List<ElementToken> elementos= new List<ElementToken>();

        /// <summary>
        /// lista de elementos de tokens de classes orquidea.
        /// </summary>
        public List<ElementToken> elementosLinguagemOrquidea = new List<ElementToken>();

        /// <summary>
        /// singleton desta classe.
        /// </summary>
        private static Classificador classificadorSingleton = null;

        /// <summary>
        /// se true, classes orquidea já foram processados.
        /// </summary>
        public static bool isLoadCLassOrquidea = true;
        
        /// <summary>
        /// headers de classes base da linguagem orquidea.
        /// </summary>
        private List<HeaderClass> classesLinguagemOrquidea = new List<HeaderClass>();

        /// <summary>
        /// lista de nomes de classes a remover, ante a novo processamento de header;
        /// </summary>
        public List<string> classesARemover = new List<string>();


        /// <summary>
        /// singleton do classificador. o classificador é um objeto único no código.
        /// </summary>
        /// <returns></returns>
        public static Classificador Instance()
        {
            if (classificadorSingleton == null)
            {
                classificadorSingleton = new Classificador();
            }
            return classificadorSingleton;
        }


        /// <summary>
        /// carrega termos-chave de comandos de programacao estruturada, e termos-chave de POO.
        /// </summary>
        private void LoadTermosChave()
        {
            this.nomesDeTermosChave = new List<string>() { "public","private","protected", "actual","class","interface",
             "for","if","else","while","casesOfUse","break","continue","Module","Library",
            "return","create", "construtorUP","SetVar","GetObjeto","operador","prioridade","metodo"};


            // adiciona os termos-chave na lista de elementos do classificador.
            for (int x = 0; x < this.nomesDeTermosChave.Count; x++)
            {
                this.elementos.Add(new ElementToken("", ElementToken.typeToken.nameKeyTerm, this.nomesDeTermosChave[x], ""));
            }


            this.nomesOperadoresTermosChave = new List<string>() { "(", ")", "[", "]", "{", "}", ".", ";", "_" };
            // adicionna os operadores termo-chave a lista de elementos do classificador.
            for (int x = 0; x < this.nomesOperadoresTermosChave.Count; x++)
            {
                this.elementos.Add(new ElementToken("", ElementToken.typeToken.nameOperatorKeyTerm, this.nomesOperadoresTermosChave[x], ""));
            }
        }

        /// <summary>
        /// obtem classes a remover do classificador.
        /// </summary>
        public void GetClassToRemove()
        {
         
       
            classesARemover = new List<string>();

            if ((RepositorioDeClassesOO.Instance().GetClasses() != null) && (RepositorioDeClassesOO.Instance().GetClasses().Count > 0))
            {
                List<Classe> classesNoRepositorio = RepositorioDeClassesOO.Instance().GetClasses();
                for (int i = 0; i < classesNoRepositorio.Count; i++)
                {

                    if ((!classesNoRepositorio[i].isEstructuralClasse) &&
                        (!classesNoRepositorio[i].isImport) &&
                        (nomesDeClasses.FindIndex(k => k.Equals(classesNoRepositorio[i].nome)) != -1)) 
                    {
                        classesARemover.Add(classesNoRepositorio[i].nome);
                    }
                }
            }

           

        }

        /// <summary>
        /// remove todos tokens da classe de nome parametro, presentes no classificador.
        /// </summary>
        /// <param name="nomeClasse">nome da classe a remover.</param>
        private void RemoverClassesAntigas(string nomeClasse)
        {
            
            // remove os elementos do classificador que pertencem a classe de nome parametro.
            if ((elementos != null) && (elementos.Count > 0))
            {
                for (int i = 0; i < elementos.Count; i++)
                {
                    if ((elementos[i] != null) && (elementos[i].nomeClasse == nomeClasse)) 
                    {
                        
                        elementos.RemoveAt(i);
                        i--;
                    }
                }
                
                
            }
            if ((RepositorioDeClassesOO.Instance() != null) && (RepositorioDeClassesOO.Instance().GetClasses().Count > 0))
            {
                RepositorioDeClassesOO.Instance().RemoveClasse(nomeClasse);
            }
            
      

        }


        /// <summary>
        /// processamento de nomes de classes, metodos, propriedades, operadores.
        /// </summary>
        /// <param name="code">texto contendo definicoes de classes.</param>
        public void LoadCodeFromHeaders(string code)
        {
            List<string> allTokens = new Tokens(code).GetTokens();
            LoadCodeFromHeaders(allTokens);
        }

        ///	1- nao le classes orquidea base, se ja estão nos elementos do classificador. se não estão, faz o processamento.
        /// 2- remove classes de mesmo nome, anteriores, armazenada nos elementos do classificador.
        /// 3- le a classes dos headers vindo do cenario de teste currente, e dá processamento, armazenando no classificador.


        /// <summary>
        /// processamento de nomes de classes, metodos, propriedades, operadores.
        /// </summary>
        /// <param name="tokens">lista de tokens contendo definicoes de classes.</param>
        public void LoadCodeFromHeaders(List<string> tokens)
        {

            


            Expressao.InitHeaders(tokens);
            this.headers = Expressao.headers;
           


            List<HeaderClass> classesHeaders = this.headers.cabecalhoDeClasses;
            if ((classesHeaders != null) && (classesHeaders.Count > 0))
            {
                for (int i=0;i< classesHeaders.Count;i++) { 
                    for (int j=i+1;j<classesHeaders.Count;j++)
                    {
                        if ((classesHeaders[j].nomeClasse == classesHeaders[i].nomeClasse) && (i != j))
                        {

                            RemoverClassesAntigas(classesHeaders[i].nomeClasse);
                            nomesDeClasses.Remove(classesHeaders[i].nomeClasse);

                            classesHeaders.RemoveAt(i);
                        }
                    }
                
                }

            }
            classesARemover.Clear();


            // classifica tokens vindo do codigo de programa VM.
            ProcessingTokensFromHeadersClass(classesHeaders);

            // classifica tokens de classes base da linguagem orquidea, se ja nao estiver classificados.
            if (isLoadCLassOrquidea)
            {
                isLoadCLassOrquidea = false;

                // REGISTRO DE TOKENS DE OPERADORES CONECTIVOS UNIVERSAIS. 
                this.elementos.Add(new ElementToken("object", ElementToken.typeToken.nameOperator, "&&", "object"));
                this.elementos.Add(new ElementToken("object", ElementToken.typeToken.nameOperator, "||", "object"));

                // REGISTRO DOS OBJETOS TRUE, FALSE.
                this.elementos.Add(new ElementToken("bool", ElementToken.typeToken.nameObject, "TRUE", "bool"));
                this.elementos.Add(new ElementToken("bool", ElementToken.typeToken.nameObject, "FALSE", "bool"));

                List<Classe> classesRepositorio = new List<Classe>();

                // VERIFICA SE AS CLASSES ORQUIDEA BASE JÁ ESTÃO NOS headers, se tiver, não faz o processaomento
                if (headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == "int") == -1)
                {
                    // extrai as definicoes de classes do header.
                    this.headers.ExtractHeadersClassFromClassesOrquidea(LinguagemOrquidea.Instance().GetClasses(), classesLinguagemOrquidea);
                    // adiciona as definicoes de classes, funcoes, propriedades, operadores, na lista de elementos do classificador.
                    ProcessingTokensFromHeadersClass(classesLinguagemOrquidea);


                }


            }

            ProcessingTokensOutClass(tokens);
            // adiciona os termos-chave para a lista de elementos do classificador.
            this.LoadTermosChave();

            // obtem classes do codigo currente, afim de na proxima instanciacao, remover estas classes.
            if ((classesARemover == null) || (classesARemover.Count == 0))
            {
                // verifica se há classes já armazenadas nos headers, ou seja, houve processamento anterior.
                if ((TabelaHash.isRemoveCLassRepeat) && (Expressao.headers != null) && (Expressao.headers.cabecalhoDeClasses != null) && (Expressao.headers.cabecalhoDeClasses.Count > 0))
                {

                    this.GetClassToRemove();
                }
            }
        }

        /// <summary>
        /// remove todos tokens de definicao de classe, retornando tokens fora de classes,
        /// para processamento de tokens pelo classificador.
        /// </summary>
        /// <param name="tokens">todos tokens do codigo.</param>
        private void ProcessingTokensOutClass(List<string> tokens)
        {
            List<string> tokensTotalCodigo = tokens.ToList<string>();
            int offset = 0;
            while (tokensTotalCodigo.IndexOf("class", offset) != -1)
            {
                int initTokensClass = tokensTotalCodigo.IndexOf("class", offset);
                if (initTokensClass - 1 >= 0)
                {
                    if (acessors().FindIndex(k => k.Equals(tokensTotalCodigo[initTokensClass - 1])) >= 0)
                    {
                        initTokensClass--;
                    }
                }




                // remove os tokens do corpo da classe.
                int initBodyClass = tokensTotalCodigo.IndexOf("{", offset);
                List<string> tokensBodyClass = UtilTokens.GetCodigoEntreOperadores(initBodyClass, "{", "}", tokensTotalCodigo);
                if ((tokensBodyClass != null) && (tokensBodyClass.Count > 0)) 
                {
                    tokensTotalCodigo.RemoveRange(initBodyClass, tokensBodyClass.Count);
                }
                
                // remove o acessor, o token [class], o nome da classe, e mais tokens de heranca, deseranca.
                if (initBodyClass != -1)
                {
                    int countHeader = initBodyClass - initTokensClass; // +1 porque é contador, nao indices.
                    tokensTotalCodigo.RemoveRange(initTokensClass, countHeader);
                    

                }
                if ((tokensTotalCodigo != null) && (tokensTotalCodigo.Count > 0) && (tokensTotalCodigo[0] == ";")) 
                {
                    tokensTotalCodigo.RemoveAt(0);
                }
                
            }

            if (tokensTotalCodigo != null)
            {
                for (int i = 0; i < tokensTotalCodigo.Count; i++) 
                {
                    ElementToken.typeToken tipoDoToken= GetTypeToken(tokensTotalCodigo[i]);
                    ElementToken umElemento = new ElementToken("", tipoDoToken, tokensTotalCodigo[i], "");
                    this.elementos.Add(umElemento);
                }
            }
        }

        private List<string> acessors()
        {
            return new List<string>() { "public", "protected", "private" };
        }


        /// <summary>
        /// faz a classificacao de classes header.
        /// </summary>
        /// <param name="classesHeaders">lista de classes header com os tokens a classificar.</param>
        private void ProcessingTokensFromHeadersClass(List<HeaderClass> classesHeaders)
        {
            // reseta a lista de nomes de classes.
            this.nomesDeClasses = new List<string>();
            
            if (classesHeaders != null)
            {
                for (int x = 0; x < classesHeaders.Count; x++)
                {
                    
                    
                    string nomeClasseEmProcessamento = classesHeaders[x].nomeClasse;
                    // obtem nomes de classes.
                    this.nomesDeClasses.Add(classesHeaders[x].nomeClasse);
                    
                    
                    
                    ElementToken elementoNomeClasse = new ElementToken(
                        nomeClasseEmProcessamento, ElementToken.typeToken.nameClasse, classesHeaders[x].nomeClasse, classesHeaders[x].nomeClasse);
                    
                    
                    this.elementos.Add(elementoNomeClasse);

                    // extrai nomes de propriedades.
                    if (classesHeaders[x].properties != null)
                    {
                        
                        for (int p = 0; p < classesHeaders[x].properties.Count; p++)
                        {
                            this.nomesDePropriedades.Add(classesHeaders[x].properties[p].name);
                            ElementToken elementoNomeDePropriedade = new ElementToken(
                                nomeClasseEmProcessamento, ElementToken.typeToken.nameProperty, classesHeaders[x].properties[p].name, classesHeaders[x].properties[p].typeOfProperty);
                           

                            
                            this.elementos.Add(elementoNomeDePropriedade);
                            
                                                       
                        }
                    }
                    // extrai nomes de funcoes.
                    if (classesHeaders[x].methods != null)
                    {
                        for (int m = 0; m < classesHeaders[x].methods.Count; m++)
                        {
                            this.nomesDeFuncoes.Add(classesHeaders[x].methods[m].name);
                            
                            
                            ElementToken elementoFuncao = new ElementToken(
                              classesHeaders[x].methods[m].typeOfProperty, ElementToken.typeToken.nameFunction, classesHeaders[x].methods[m].name, classesHeaders[x].methods[m].typeReturn);
                           
                            // adiciona os parametros da funcao.
                            if (classesHeaders[x].methods[m].parameters != null)
                            {
                                for (int p = 0; p < classesHeaders[x].methods[m].parameters.Count; p++)
                                {
                                    ElementToken umParametro = new ElementToken(
                                        nomeClasseEmProcessamento, ElementToken.typeToken.nameParameter, 
                                        classesHeaders[x].methods[m].parameters[p].name, classesHeaders[x].methods[m].parameters[p].classNameOfProperty);

                                    elementoFuncao.AddParameter(umParametro);   
                                }
                            }
                            // adiciona o elemento funcao.
                            this.elementos.Add(elementoFuncao);
                        }
                    }
                    // extrai nomes de operadores.
                    if (classesHeaders[x].operators != null)
                    {
                        for (int op = 0; op < classesHeaders[x].operators.Count; op++)
                        {
                            this.nomesDeOperadores.Add(classesHeaders[x].operators[op].name);

                            ElementToken elementoOperador = new ElementToken(
                                nomeClasseEmProcessamento, ElementToken.typeToken.nameOperator, classesHeaders[x].operators[op].name, nomeClasseEmProcessamento);

                            if (classesHeaders[x].operators[op].operands != null)
                            {
                                for (int p = 0; p < classesHeaders[x].operators[op].operands.Count; p++)
                                {
                                    ElementToken umOperando = new ElementToken(
                                        "", ElementToken.typeToken.nameOperand, classesHeaders[x].operators[op].operands[p].Trim(' '), "");
                    
                                    elementoOperador.AddParameter(umOperando);
                                }
                            }
                            this.elementos.Add(elementoOperador);

                        }
                    }
                }
            }
        }

        /// <summary>
        /// obtem dados do token parametro.
        /// </summary>
        /// <param name="token">token a analisar.</param>
        /// <returns></returns>
        public List<ElementToken> GetDataToken(string token)
        {
            if (token != null)
            {
                return this.elementos.FindAll(k=>k.nomeToken.Equals(token));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// obtem todos nomes de classes base da linguagem orquidea.
        /// </summary>
        /// <returns>retorna uma lista de todos nomes de classes orquidea.</returns>
        private List<string> GetClassBaseOrquidea()
        {
            List<Classe> classesOrquidea = LinguagemOrquidea.Instance().GetClasses();
            List<string> nomesClassesOrquidea = new List<string>();
            if (classesOrquidea != null)
            {
                for (int j = 0; j < classesOrquidea.Count; j++)
                {
                    nomesClassesOrquidea.Add(classesOrquidea[j].nome);
                }
            }

            return nomesClassesOrquidea;
        }



        /// <summary>
        /// retorna o tipo de token vindo da classificação.
        /// se um token nao for nome de classe, nome de metodo, nome de propriedade, ou nome de operador,
        /// entao é um nome de objeto.
        /// </summary>
        /// <param name="token">token a obter classficacao.</param>
        /// <returns>retorna o tipo do token, ou nao-classificado, se o token for null, ou os elementos nao foram retirados.</returns>
        public ElementToken.typeToken GetTypeToken(string token)
        {
            // verifica se o token é um número.
            if (isNumero(token))
            {
                return ElementToken.typeToken.nameNumber;
            }
            else
            if (isLiteral(token))
            {
                return ElementToken.typeToken.nameLiteral;
            }
            else
            {
                // obtem o tipo do token dentro da classficacao de tokens.
                if ((this.elementos != null) && (this.elementos.Count > 0))
                {
                    ElementToken umElemento = this.elementos.Find(k => k.nomeToken == token);
                    if (umElemento != null)
                    {
                        return umElemento.tipoClassificacao;
                    }
                    else
                    {
                        return ElementToken.typeToken.nameObject;
                    }
                }
                else
                {
                    return ElementToken.typeToken.notClassificate;
                }
            }
        }

        /// <summary>
        /// resumo no terminal dos tokens das classes no classificador.
        /// </summary>
        public void PrintResults()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("nomes de classes:");
            for (int x=0;x< nomesDeClasses.Count;x++)
            {
                System.Console.Write(nomesDeClasses[x].ToString()+" ");
            }
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine("nomes de metodos:");
            for (int x = 0; x < nomesDeFuncoes.Count; x++)
            {
                System.Console.Write(nomesDeFuncoes[x].ToString()+" ");
            }

            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine("nomes de propriedades:");
            for (int x=0;x<nomesDePropriedades.Count;x++)
            {
                System.Console.Write(nomesDePropriedades[x].ToString() + " ");
            }

            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine("nome de operadores: ");
            for (int x = 0; x < nomesDeOperadores.Count; x++)
            {
                System.Console.Write(nomesDeOperadores[x].ToString() + " ");
            }
        }

        /// <summary>
        /// verifica se um token é um número.
        /// </summary>
        /// <param name="token">token a verificar se é numero.</param>
        /// <returns>retorna [true] se o token é um numero.</returns>
        private static bool isNumero(string token)
        {
            int n_int;
            float n_float;
            double n_double;

            if (int.TryParse(token, out n_int))
                return true;
            else
            if (float.TryParse(token, out n_float))
                return true;
            else
            if (double.TryParse(token, out n_double))
                return true;

            else
                return false;

        }


        /// <summary>
        /// verifica se um token é uma literal (constante de texto).
        /// </summary>
        /// <param name="token">token a verificar.</param>
        /// <returns>[true] se o token é uma literal.</returns>
        public static bool isLiteral(string token)
        {
            if ((token == null) || (token.Length == 0))
            {
                return false;
            }
            else
            {
                return (token.IndexOf("\"") == 0) && (token.LastIndexOf("\"") == token.Length - 1);
            }
        }


        /// <summary>
        /// tabela hash de procura de tipos de tokens.
        /// </summary>
        public class TabelaHash
        {

            /// <summary>
            /// entrada para uma linha de elementos com mesmo numero hash.
            /// </summary>
            public class RowHashTable
            {
                public List<ElementToken> row = new List<ElementToken>();
                
            }


            private static int countTiposChar = 0;
            private RowHashTable[] table;
            private int countCharsHash = 3;
            private static char hashNeutral = '#';

            private static char aspasLiteral = '\"';
            private static char singleQuote = '\"';


            public Classificador classificador;
            private static Dictionary<char, int> valoresHash;

            /// <summary>
            /// se [true] reconstroi a tabela hash.
            /// </summary>
            public static bool isReset = true;

            /// <summary>
            /// se true, remove classes vindo de outros cenarios, quando executar varios cenarios de testes.
            /// </summary>
            public static bool isRemoveCLassRepeat = false;
            private static TabelaHash TableSingleton;
           

            /// <summary>
            /// objeto unico de Tabela Hash. Tabela Hash é um objeto unico no codigo.
            /// </summary>
            /// <param name="code">trecho de codigo</param>
            /// <returns></returns>
            public static TabelaHash Instance(string code)
            {
                if ((TableSingleton == null) || (isReset))
                {
                    
                    TableSingleton = new TabelaHash(code);
                    InstanceWithouCode = TableSingleton;
                    isReset = false;
                }

                return TableSingleton;
            }

            /// <summary>
            /// instancia da tabela codigo. utiliza-se quando o codigo foi carregado.
            /// </summary>
            public static TabelaHash InstanceWithouCode;

            /// <summary>
            /// construtor. inicio do processamento otimizado de classificação de tokens.
            /// </summary>
            /// <param name="code">código do programa VM.</param>
            public TabelaHash(string code)
            {
                if (!isReset)
                {
                    return;
                }
                if (valoresHash == null)
                {
                    valoresHash = BuildHasthValues();
                }

                
                table = new RowHashTable[countTiposChar * countTiposChar * countTiposChar];
               

                // remove da tabela classes anteriores, do 2o. programa/cenario de teste em diante.
                if (isRemoveCLassRepeat)
                {
                    
                    RemoveClassRepeat(isRemoveCLassRepeat);
                    
                }
                else
                {
                    isRemoveCLassRepeat = true;
                }
               

                // inicializa o classificador e carrega os elementos de tipos de tokens.
                this.classificador = Classificador.Instance();
                this.classificador.LoadCodeFromHeaders(code);




                // registra todos tokens presentes no classificador de tokens.
                List<ElementToken> tokensDoClassificador = classificador.elementos;
                if (tokensDoClassificador != null)
                {
                    for (int i = 0; i < tokensDoClassificador.Count; i++)
                    {
                        WriteToken(tokensDoClassificador[i].nomeToken);
                    }
                }
            }

            /// <summary>
            /// remove classes repetidas, causado por processamento de outros cenarios, ou mesmo de um programa VM.
            /// </summary>
            private void RemoveClassRepeat(bool isRemoveClass)
            {
                if ((isRemoveClass) &&  ((Expressao.headers != null)) && (Expressao.headers.cabecalhoDeClasses != null) && (Expressao.headers.cabecalhoDeClasses.Count > 0)) 
                {
                    Classificador.Instance().GetClassToRemove();
                    List<string> classesAntigasARemover = Classificador.Instance().classesARemover;

                    if ((classesAntigasARemover != null) && (classesAntigasARemover.Count > 0))
                    {
                        for (int i = 0; i < classesAntigasARemover.Count; i++)
                        {
                            RemoveClasseNaTabelaHash(classesAntigasARemover[i]);
                        }
                    }
                }
            }

            /// <summary>
            /// remove uma classe da tabelaHash.
            /// </summary>
            /// <param name="nomeClasse">nome da tabelaHash.</param>
            public bool RemoveClasseNaTabelaHash(string nomeClasse)
            {
                List<ElementToken> elementosTotal = Classificador.Instance().elementos;
                List<ElementToken> elementosREMOVE = new List<ElementToken>();

                // obtem todos tokens da classe a remover.
                if ((elementosTotal != null) && (elementosTotal.Count > 0)) 
                {
                    for (int i = 0; i < elementosTotal.Count; i++)
                    {
                        if ((elementosTotal[i] != null) && (elementosTotal[i].nomeClasse != null) && (elementosTotal[i].nomeClasse == nomeClasse)) 
                        {
                            elementosREMOVE.Add(elementosTotal[i]);
                        }
                    }
                }
                // REMOVE OS TOKENS da classe a remover, QUE ESTÃO NA TABELA HASH.
                if (elementosREMOVE.Count > 0)
                {
                    for (int i = 0; i < elementosREMOVE.Count; i++)
                    {
                        this.RemoveToken(elementosREMOVE[i].nomeToken);
                    }


                 
                }
                return false;     
            }

            /// <summary>
            /// obtem todos nomes de classes base da linguagem orquidea.
            /// </summary>
            /// <returns>retorna uma lista de todos nomes de classes orquidea.</returns>
            private List<string> GetClassBaseOrquidea()
            {
                List<Classe> classesOrquidea = LinguagemOrquidea.Instance().GetClasses();
                List<string> nomesClassesOrquidea = new List<string>();
                if (classesOrquidea != null)
                {
                    for (int j = 0; j < classesOrquidea.Count; j++)
                    {
                        nomesClassesOrquidea.Add(classesOrquidea[j].nome);
                    }
                }

                return nomesClassesOrquidea;
            }
       
            /// <summary>
            /// verifica se o token pertence a classe parametro.
            /// </summary>
            /// <param name="token">token a verificar.</param>
            /// <param name="possibleClass">possivel class a qual o token pertence, ou não.</param>
            /// <returns>retorna [true] se o token pertence a classe parametro, outro caso [false].</returns>
            public bool GetClassToken(string token, string possibleClass)
            {
                if (token != null)
                {
                    return Classificador.Instance().elementos.Find(k => k.nomeToken == token && k.nomeClasse == possibleClass) != null;
                }
                else
                {
                    return false;
                }
                
            }


            /// <summary>
            /// retorna true se eh nome de classe.
            /// </summary>
            /// <param name="token">token a verificar.</param>
            /// <returns></returns>
            public bool isNomeClasse(string token)
            {
                return GetTipoToken(token, ElementToken.typeToken.nameClasse);
            }

            /// <summary>
            /// retorna true se eh nome de objeto.
            /// </summary>
            /// <param name="token">token a verificar</param>
            /// <returns></returns>
            public bool isNomeObjeto(string token)
            {
                return GetTipoToken(token, ElementToken.typeToken.nameObject); 
            }

            /// <summary>
            /// retorna true se eh nome de funcao.
            /// </summary>
            /// <param name="token">token a verificar.</param>
            /// <returns></returns>
            public bool isNomeFuncao(string token)
            {
                return GetTipoToken(token, ElementToken.typeToken.nameFunction);
            }

            /// <summary>
            /// retorna true se eh nome de propriedade (variavel de classe).
            /// </summary>
            /// <param name="token">token a verificar.</param>
            /// <returns></returns>
            public bool isNomePropriedade(string token)
            {
                return GetTipoToken(token, ElementToken.typeToken.nameProperty);
            }

            /// <summary>
            /// retorna true se o token eh nome de um operador.
            /// </summary>
            /// <param name="token">token a verificar.</param>
            /// <returns></returns>
            public bool isNomeOperador(string token)
            {
                return GetTipoToken(token, ElementToken.typeToken.nameOperator);
            }

            /// <summary>
            /// retorna true se o token eh nome de uma palavra reservada da linguagem.
            /// </summary>
            /// <param name="token"></param>
            /// <returns></returns>
            public bool isNomeTermoChave(string token)
            {
                return GetTipoToken(token, ElementToken.typeToken.nameKeyTerm);
            }

            /// <summary>
            /// retorna true se eh numero.
            /// </summary>
            /// <param name="token">token a verificar.</param>
            /// <returns></returns>
            public bool isNumero(string token)
            {
                return GetTipoToken(token, ElementToken.typeToken.nameNumber);
            }

            /// <summary>
            /// retorna true se é literal.
            /// </summary>
            /// <param name="token">token a verificar.</param>
            /// <returns></returns>
            public bool isLiteral(string token)
            {
                return classificador.GetTypeToken(token) == ElementToken.typeToken.nameLiteral;
                
            }

            /// <summary>
            /// retorna true se o token eh operador estrutural da linguagem.
            /// </summary>
            /// <param name="token">token a verificar.</param>
            /// <returns></returns>
            public bool isOperadorTermoChave(string token)
            {
                return GetTipoToken(token, ElementToken.typeToken.nameOperatorKeyTerm);
            }



            /// <summary>
            /// retorna true se o token eh do tipo de token do parametro.
            /// </summary>
            /// <param name="token">token a verificar.</param>
            /// <param name="tipoDoToken">tipo do token a verificar.</param>
            /// <returns></returns>
            private bool GetTipoToken(string token, ElementToken.typeToken tipoDoToken)
            {
                List<ElementToken> elementos = ReadToken(token);
                if ((elementos == null) || (elementos.Count == 0))
                {
                    return classificador.GetTypeToken(token) == tipoDoToken;
                }
                else
                {
                    return elementos.FindIndex(k => k.nomeToken == token && k.tipoClassificacao == tipoDoToken) != -1;
                }
            }



            /// <summary>
            /// registra tokens na tabela hash.
            /// </summary>
            /// <param name="token">token a ser registrado</param>
            public void WriteToken(string token)
            {
                try
                {
                    List<ElementToken> tipo = classificador.GetDataToken(token);

                    int hash = HashNumber(token);
                    if (table[hash] == null)
                    {
                        table[hash] = new RowHashTable();
                    }

                    table[hash].row.AddRange(tipo);
                }
                catch(Exception ex)
                {
                    string codeErro = ex.Message;
                }
            }
            
            /// <summary>
            /// remove um token da tabela hash.
            /// </summary>
            /// <param name="token">nome do token a remover.</param>
            public void RemoveToken(string token)
            {
                try
                {
                    int hashNumero = HashNumber(token);
                    if (table[hashNumero] == null)
                    {
                        return;
                    }
                    else
                    {
                        List<ElementToken> elementos = table[hashNumero].row.FindAll(k => k.nomeToken.Equals(token));
                        if (elementos != null)
                        {
                            for (int i = 0; i < elementos.Count; i++)
                            {
                                table[hashNumero].row.Remove(elementos[i]);
                            }
                        }
                       
                    }
                }
                catch (Exception ex)
                {
                    string errorCode = ex.Message;
                }
            }

            /// <summary>
            /// le dados de token, na tabela hash.
            /// </summary>
            /// <param name="token">tokens a verificar.</param>
            /// <returns></returns>
            public List<ElementToken> ReadToken(string token)
            {
                try
                {
                    int hashNumero = HashNumber(token);
                    if (table[hashNumero] == null)
                    {
                        return null;
                    }
                    else
                    {
                        List<ElementToken> elementos = table[hashNumero].row.FindAll(k => k.nomeToken.Equals(token));
                        return elementos;
                    }
                }
                catch (Exception ex)
                {
                    string errorCode = ex.Message;
                    return new List<ElementToken>();
                }
            }


            /// <summary>
            /// retorna um valor de hash para procura de tipos de tokens, na tabela hash.
            /// </summary>
            /// <param name="token">token para o valor hash.</param>
            /// <returns></returns>
            public int HashNumber(string token)
            {

                int numberHash = 1;
                if (token.Length <countCharsHash) 
                {
                    int sizeToken = token.Length;
                    int dx = countCharsHash - sizeToken;
                    for (int i = 0; i < sizeToken; i++)
                    {
                        numberHash += valoresHash[token[i]];
                    }

                    for (int i = 0; i < dx; i++)
                    {
                        numberHash += valoresHash['#'];
                    }
                }
                else
                {
                    for (int x = 0; x < countCharsHash; x++)
                    {
                        string lower_x = token[0].ToString().ToLower();
                        int num_x = 0;
                        num_x = valoresHash[lower_x[0]];
                        numberHash *= num_x;
                    }
                }
                



                return numberHash;

            }

            /// <summary>
            /// constroi os caracteres-texto de entrada da tabela hash.
            /// </summary>
            /// <returns></returns>
            private static Dictionary<char, int> BuildHasthValues()
            {
            
                // faltando simbolos NAO ALFA-NUMERICOS. SIMBOLO empty='#, para tokens length<3;
                Dictionary<char, int> caracteresHash = new Dictionary<char, int>();
                List<char> valoresLetras = new List<char>() { '_','a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'x', 'y', 'w', 'z',
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'X', 'Y', 'W', 'Z'};
                List<char> valoresNumericos = new List<char>() { '0', '1', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                List<char> valoresOperadores = new List<char>() { '+', '-', '*', '/', '%', '<', '>', '=', '!', '?', '[', ']', '{', '}', '(', ')', '.', ':', ';', ' ', ',', '"', '^', '|', '&', aspasLiteral, singleQuote };


                int countChars = 1;
                // adiciona textos de letras.
                for (int i = 0; i < valoresLetras.Count; i++)
                {
                    caracteresHash[valoresLetras[i]] = countChars++;
                }
                // adiciona textos de numeros.
                for (int i = 0; i < valoresNumericos.Count; i++)
                {
                    caracteresHash[valoresNumericos[i]] = countChars++; 
                }
                // adiciona textos de operadores.
                for (int i = 0; i < valoresOperadores.Count; i++)
                {
                    caracteresHash[valoresOperadores[i]] = countChars++;
                }
                // adiciona o caracter de completar tokens com length<3;
                caracteresHash[hashNeutral] = countChars++;
                countTiposChar = countChars + 1;
                return caracteresHash;
            }
        }


        public class Testes : SuiteClasseTestes
        {
            char aspas = '\"';

            public Testes():base("testes de classificador de tokens")
            {


            }

            public void TesteRemoveUmaClasse(AssercaoSuiteClasse assercao)
            {
                string codigoClasseA = "public class classeA { public classeA() { int x=1; }  public int metodoA(){ int y= 1; }; };";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseA);
                compilador.Compilar();


                string codigoNovaClasseA = "public class classeA { public classeA() { int x=1; }  public int metodoA(int x){ int y= -1; }; };";
                SystemInit.InitSystem();
                ProcessadorDeID compilador2 = new ProcessadorDeID(codigoNovaClasseA);
                compilador2.Compilar(); 

            }


            public void TesteRemoveClasses(AssercaoSuiteClasse assercao)
            {
                LinguagemOrquidea.Instance();

                LinguagemOrquidea.Instance().GetClasses();
                string codigoClasseX = "public class classeX { public classeX() { int y; }  public int metodoX(int x, int y) { int x; }; };";
                string codigoClasseA = "public class classeA { public classeA() { int x=1; }  public int metodoA(){ int y= 1; }; };";



                string codigoClasseC = "public class classeC { public int propriedadeC; public classeC() { int x =0; }  };";
                string codigoClasseD = "public class classeD {  public int propriedadeD;  public classeD() { int y=0; } };";


                // simula o 1o. cenario de teste.
                string codigoTodasClasses = codigoClasseX + codigoClasseA + codigoClasseC + codigoClasseD;


                ProcessadorDeID compilador = new ProcessadorDeID(codigoTodasClasses);
                compilador.Compilar();

                TabelaHash.Instance(codigoTodasClasses);


                // simula o 2o. cenario de test, com uma classe de nome repetido.
                SystemInit.InitSystem();
                string codigoNovaClasseA = "public class classeA { public classeA() { int x=1; }  public int metodoB(){ int y= 1; }; };";


                TabelaHash.isReset = true;
                // remove a classeA anterior.
                TabelaHash table = TabelaHash.Instance(codigoNovaClasseA);


                System.Console.WriteLine("classes no classificador:");
                for (int x = 0; x < table.classificador.nomesDeClasses.Count; x++)
                {
                    System.Console.WriteLine(table.classificador.nomesDeClasses[x]);
                }

                // verifica se o nome da funcao da classe repetida anterior foi removido, e se
                // o nome da funcao da classe repetida atual está registrado.
                try
                {
                    assercao.IsTrue(table.isNomeFuncao("metodoA") == false && table.isNomeFuncao("metodoB") == true);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

                System.Console.ReadLine();
            }

            public void TesteResetClassificador(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string codigoClasseX = "public class classeX { public classeX() { int y; }  public int metodoX(int x, int y) { int x; }; };";
                string codigoClasseA = "public class classeA { public classeA() { int x=1; }  public int metodoA(){ int y= 1; }; };";



                string codigoClasseC = "public class classeC { public int propriedadeC; public classeC() { int x =0; }  };";
                string codigoClasseD = "public class classeD {  public int propriedadeD;  public classeD() { int y=0; } };";


                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseX + codigoClasseA + codigoClasseC + codigoClasseD);
                compilador.Compilar();

                // simula o 1o. cenario de teste.
                string codigoTodasClasses = codigoClasseX + codigoClasseA + codigoClasseC + codigoClasseD;
                TabelaHash.Instance(codigoTodasClasses);

                // simula o reset do cenario seguinte.
                TabelaHash.isReset = true;
                TabelaHash.Instance(codigoClasseX);

                Classificador.Instance().PrintResults();

                try
                {

                    assercao.IsTrue(TabelaHash.Instance(codigoClasseX).isNomeFuncao("metodoA") == false, codigoTodasClasses);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }

            public void TesteOperadoresCondicionaisConectivos(AssercaoSuiteClasse assercao)
            {
                try
                {
                    string codigoExpressao = "((a>1) ||  (b<3) && (a==1))";
                    Classificador.TabelaHash table = TabelaHash.Instance(codigoExpressao);

                    assercao.IsTrue(table.isNomeOperador("&&"), codigoExpressao);
                    assercao.IsTrue(table.isNomeOperador("||"), codigoExpressao);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false,"TESTE FALHOU: "+ ex.Message);
                }
                
            }

            public void TesteWriteReadHashTable(AssercaoSuiteClasse assercao)
            {
                string codigo = "public class classeA { public int propriedadeA = 1;  public classeA(){ int x=1; } ;public int metodoB(double x, ! int[] y){ return 5;} ;};";
                Classificador.TabelaHash table = TabelaHash.Instance(codigo);




                try
                {
                    assercao.IsTrue(table.isNomeClasse("classeA"), codigo);
                    assercao.IsTrue(table.isNomeFuncao("classeA"), codigo);

                    List<ElementToken> results = table.ReadToken("classeA");
                    for (int i = 0; i < results.Count; i++)
                    {
                        Console.WriteLine(results[i].ToString());
                    }
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "Teste falhou: " + ex.Message);
                }

                
            }

  

            public void TesteClassificacaoDeTokenTermoChave(AssercaoSuiteClasse assercao)
            {
                string codigo = "public class classeA { public int propriedadeA = 1;  public classeA(){ int x=1; } ;public int metodoB(double x, ! int[] y){ return 5;} ;};";
                Classificador.TabelaHash.isReset = true;
                Classificador.TabelaHash table = Classificador.TabelaHash.Instance(codigo);
                try
                {
                    assercao.IsTrue(table.isNomeTermoChave("for"));
                    assercao.IsTrue(table.isNomeTermoChave("if"));
                    assercao.IsTrue(table.isNumero("1000"));
                    assercao.IsTrue(table.isNumero("1.0"));
                    assercao.IsTrue(table.isLiteral(aspas + "texto" + aspas));
                    assercao.IsTrue(table.isOperadorTermoChave("("));
                    assercao.IsTrue(table.isOperadorTermoChave("["));

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, ex.Message);
                }


            }


    
    
            public void TesteClassificacaoDeTokens(AssercaoSuiteClasse assercao)
            {
                string codigo = "public class classeA { public int propriedadeA = 1;  public classeA(){ int x=1; } ;public int metodoB(double x, ! int[] y){ return 5;} ;};";
                Classificador.TabelaHash.isReset = true;
                Classificador.TabelaHash table = Classificador.TabelaHash.Instance(codigo);
                try
                {
                    assercao.IsTrue(table.isNomeObjeto("umObjeto"));
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, ex.Message);
                }

            }
           
        }
    }
}
