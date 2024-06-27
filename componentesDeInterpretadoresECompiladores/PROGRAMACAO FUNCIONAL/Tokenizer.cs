using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Util;
using stringUtilities;
namespace parser
{

    
    /// <summary>
    /// o tokenizeer faz a retirada de tokens dentro de um texto, tendo como baliza os termos-chave da linguagem 
    /// ao qual os tokens pertence.
    /// </summary>
    public class Tokenizer
    {
        /// <summary>
        /// todos termos chave, p.ex., return, for, if, else, while
        /// </summary>
        private static List<string> todosTermosChave = new List<string>();
        /// <summary>
        /// todos operadores da linguagem.
        /// </summary>
        private static List<string> todosOperadoresLinguagem = new List<string>();

        /// <summary>
        /// separa o texto parametro, em tokens da linugagem
        /// </summary>
        /// <param name="textoComTokens">texto com todos tokens que se quer separar</param>
        /// <returns>retorna uma lista de strings, com o tokens.</returns>
        public static List<string> GetTokens(string? textoComTokens)
        {
            if (textoComTokens == null)
            {
                return null;
            }

            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            todosTermosChave = linguagem.GetTodosTermosChave();
            todosOperadoresLinguagem = linguagem.GetTodosOperadores();

            List<TokenComPosicao> tokensNaoOrdenados = new List<TokenComPosicao>();

            // retira dos termos-chave os operadores, que também são termos-chave, mas nao convem.
            for (int x = 0; x < todosOperadoresLinguagem.Count; x++)
                todosTermosChave.RemoveAll(k => k.Contains(todosOperadoresLinguagem[x]));
  
            List<string> termosChavePresentes = new List<string>();




            // ****************************************************************************************************************
            // processamento de literais: strings constantes delimitados por tokens aspas.
            

            List<string> literais = new List<string>(); // lista de literais, strings constantes delimitada por aspas.
            List<int> posicaoLiterais = new List<int>(); // lista de posicao das literais.

            // extrai textos literais (string constantes, delimitado por aspas).
            ExtraiLiterais(textoComTokens, ref literais, ref posicaoLiterais);

            // se houver literais, adiciona a lista de tokens nao ordenados, porem encontrados.
            if ((literais != null) && (literais.Count > 0))
            {
                for (int x = 0; x < literais.Count; x++)
                {
                    
                    // adiciona o literal na lista de tokens não ordenados, porem encontrados.
                    tokensNaoOrdenados.Add(new TokenComPosicao(literais[x], posicaoLiterais[x]));



                    // remove as literais do texto original.
                    textoComTokens = Util.PreencherVazios.PreencheVazio(textoComTokens, literais[x]);


                }

            }

            //*****************************************************************************************************************************8







            for (int x = 0; x < todosTermosChave.Count; x++)
            {

                int posicaoTermoChave = textoComTokens.IndexOf(todosTermosChave[x]);


           
                if (posicaoTermoChave >= 0)
                {
                    // verifica se o token termo-chave é um token polêmico, exemplo: termo-chave "if", e token "Verifica", o token "Verifica" contém o token "if", eñtão o token "if" não deve ser retirado.
                    if (!IsTokenPolemico(todosTermosChave[x], textoComTokens)) 
                    {

                        tokensNaoOrdenados.Add(new TokenComPosicao(todosTermosChave[x], posicaoTermoChave));
                        
                        // retira o token encontrado, mantendo os indices de posicao dos demais tokens.
                        textoComTokens = Util.PreencherVazios.PreencheVazio(textoComTokens, todosTermosChave[x]);
                        x--;
                    } 


                } 
                

            } 
            
            List<string> todosOperadores = linguagem.GetTodosOperadores();
            todosOperadores.Add("(");
            todosOperadores.Add(")");
            todosOperadores.Add("{");
            todosOperadores.Add("}");

            ComparerTexts comparer = new ComparerTexts();
            todosOperadores.Sort(comparer); // ordena os operadores decrescentemente pelo comprimento de seus caracteres, 
                                            // pois há operadores que são a uniao de dois outros operadores, como "<=", "!=",
                                            // e que devem ser reconhecidos antes dos operadores-parte.

            List<string> operadoresPresentes = new List<string>();
            for (int x = 0; x < todosOperadores.Count; x++)
            {
               
                int posicaoOperador = textoComTokens.IndexOf(todosOperadores[x]);

                if (!IsTokenPolemico(todosOperadores[x], textoComTokens)) 
                {
                    if (posicaoOperador != -1)
                    {
                        tokensNaoOrdenados.Add(new TokenComPosicao(todosOperadores[x], posicaoOperador));
                        textoComTokens = Util.PreencherVazios.PreencheVazio(textoComTokens, todosOperadores[x]);
                        x--;
                    }  // if
                } // if
                else
                if (posicaoOperador >= 0)
                {
                    tokensNaoOrdenados.Add(new TokenComPosicao(todosOperadores[x], posicaoOperador));
                    textoComTokens = Util.PreencherVazios.PreencheVazio(textoComTokens, todosOperadores[x]);
                    x--;
                } // if

            } // for x


            
            // compoe a lista de ids: letras,numeros, palavras.
            List<string> idsPresentes = textoComTokens.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();

            

            // calculo das posicoes de cada id.
            if (idsPresentes != null)
            {
                for (int x = 0; x < idsPresentes.Count; x++)
                {


                    int indexID = textoComTokens.IndexOf(idsPresentes[x]);





                    tokensNaoOrdenados.Add(new TokenComPosicao(idsPresentes[x], indexID));
                    textoComTokens = Util.PreencherVazios.PreencheVazio(textoComTokens, idsPresentes[x]);
                }

            }








            ComparerTokensPosicao comparer1 = new ComparerTokensPosicao();
            tokensNaoOrdenados.Sort(comparer1);

            List<string> tokensOrdenados = new List<string>();
            foreach (TokenComPosicao umtokenNaoOrdenado in tokensNaoOrdenados)
                if (!isEmptyWord(umtokenNaoOrdenado.token))
                    tokensOrdenados.Add(umtokenNaoOrdenado.token);

            // une palavras que sao palavras com letras, e a palavra seguinte é um numero.
            if ((tokensOrdenados != null) && (tokensOrdenados.Count >= 2))
            {
                for (int i = 0; i < tokensOrdenados.Count; i++)
                {
                    if ((i + 1 < tokensOrdenados.Count) && (isPalavraPolemicaUniao2PalavrasComNumero(tokensOrdenados[i], tokensOrdenados[i + 1])))
                    {
                        string wordComNumero = tokensOrdenados[i] + tokensOrdenados[i + 1];
                        tokensOrdenados.RemoveRange(i, 2);
                        tokensOrdenados.Insert(i, wordComNumero);
                    }
                }
            }



            tokensOrdenados = ObtemPontosFlutuantes(tokensOrdenados); // fixa erros de obter tokens que são ponto flutuante (1.1, exemplo).





            return tokensOrdenados;

        } // GetTokens()

        /// <summary>
        /// extrai literais, textos que não são nomes-chave da linguagem, nem nomes de operadores.
        /// </summary>
        /// <param name="textoComAspas">codigo a tokenizar.</param>
        /// <param name="literais">lista de literais de retorno.</param>
        /// <param name="posicaoLiterais">indices de posicao das literais.</param>
        private static void ExtraiLiterais(string textoComAspas, ref List<string> literais, ref List<int> posicaoLiterais)
        {
            if ((textoComAspas == null) || (textoComAspas == null))
            {
                return;
            }

            char aspas = '\u0022';

            string[] textoAspas = textoComAspas.Split(new string[] { aspas.ToString() }, StringSplitOptions.RemoveEmptyEntries);


            literais = new List<string>();
            posicaoLiterais = new List<int>();
         


            int indexAspasIniciais = textoComAspas.IndexOf(aspas);
            int indexAspasFinais = textoComAspas.IndexOf(aspas, indexAspasIniciais + 1);
         


           
            while ((indexAspasIniciais != -1) && (indexAspasFinais != -1))
            {
                // forma a literal, extraindo-a do texto inicial.
                string umaLiteral = textoComAspas.Substring(indexAspasIniciais, indexAspasFinais - indexAspasIniciais + 1);

                // guarda a literal e indice de começo da literal, no texto inicial.
                literais.Add(umaLiteral);
                posicaoLiterais.Add(indexAspasIniciais);






                indexAspasIniciais = textoComAspas.IndexOf(aspas, indexAspasFinais + 1);  // obtem aspas apos a ultima aspas ser encontrada.
                indexAspasFinais = textoComAspas.IndexOf(aspas, indexAspasIniciais + 1);  // o indice foi atualizado na linha anterior.
   
               
            }





        }
        /// <summary>
        /// verifica se as palavras seguintes, terminam com letra e a outra começa com número.
        /// é em token como: metodo1. (metodo é a 1a.palavra e 1 e a 2a.palavra.
        /// </summary>
        /// <param name="word_1_semNumero">palavra que termina possivelmente com letra.</param>
        /// <param name="word_2_numero">palavra que comeca possivelmente com numero.</param>
        /// <returns></returns>
        private static bool isPalavraPolemicaUniao2PalavrasComNumero(string word_1_semNumero,string word_2_numero)
        {
            if (((word_1_semNumero != null) && (word_1_semNumero.Length > 0))  && ((word_2_numero != null) && (word_2_numero.Length > 0)))
            {
                if (todosTermosChave.FindIndex(k => k.Equals(word_1_semNumero)) != -1)
                {
                    return false;
                }
                else
                if (todosOperadoresLinguagem.FindIndex(k => k.Equals(word_1_semNumero)) != -1)
                {
                    return false;
                }
                else
                {
                    return ((isLetter(word_1_semNumero[word_1_semNumero.Length - 1])) && (isContemNumeroFirst(word_2_numero)));
                }
                
                
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// retorna true se a palavra [word] começa com um número.
        /// </summary>
        /// <param name="word">palavra a verificar.</param>
        /// <returns></returns>
        private static bool isContemNumeroFirst(string word)
        {
            List<char> numeros = new List<char>() { '0', '1', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            if ((word != null) && (word.Length > 0))
            {
                if (numeros.FindIndex(k => k.Equals(word[0])) != -1)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// returna true se a palavra é uma palavra vazio.
        /// </summary>
        /// <param name="word">palavra a verificar.</param>
        /// <returns></returns>
        private static bool isEmptyWord(string word)
        {
            if (word == "")
                return true;
            foreach (char umCaracter in word)
            {
                if (umCaracter != ' ')
                    return false;
            } // foreach
            return true;
        } // isEmptyWord()

        /// <summary>
        /// retira tokens vazios da lista de tokens parametro.
        /// </summary>
        /// <param name="tokens">lista de tokens a verificar.</param>
        internal static void RetiraEmptyWords(ref List<string> tokens)
        {
            List<string> tokensSemVazios = new List<string>();
            for (int x = 0; x < tokens.Count; x++)
            {
                if (!isEmptyWord(tokens[x]))
                    tokensSemVazios.Add(tokens[x]);
            } // for x
            tokens = tokensSemVazios;
        } // RetiraEmptyWords()






        static List<char> caracteresLetras = new List<char> {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','X','Y','W','Z',
                'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','x','y','w','z'};

        /// <summary>
        /// verifica se o token é um token nome de palavra-chave, nome de operador, mas não é nome de palavra-chave ou operador,
        /// como p.ex.: forcada, contendo o nome-chave [for], mas é um nome de objeto.
        /// </summary>
        /// <param name="token">token polemico.</param>
        /// <param name="textoComOsTokens">texto contendo todos tokens.</param>
        /// <returns></returns>
        private static bool IsTokenPolemico(string token, string textoComOsTokens)
        {


            if (IsSomenteLetras(token, caracteresLetras))
            {

                int indexToken = textoComOsTokens.IndexOf(token);
                if (indexToken == -1)
                    return false;
 
               if (((indexToken - 1) >= 0) && (isLetter(textoComOsTokens[indexToken - 1])))
                    return true;
                else
                if (((indexToken + token.Length < textoComOsTokens.Length)) && (isLetter(textoComOsTokens[indexToken + token.Length])))
                    return true;
                else
                    return false;
            }
            else
            if (token == ".") // o token "." é um token polêmico, é utilizado como operador de ponto flutuante, e também como separador de propriedades/metodos de objetos.
                return true;

            return false;
        }

        /// <summary>
        /// retorna true se o caracter é uma caracter valido.
        /// </summary>
        /// <param name="c">caracter a verificar.</param>
        /// <returns></returns>
        private static bool isLetter(char c)
        {
            List<char> caracteresLetras = new List<char> {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','X','Y','W','Z',
                'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','x','y','w','z'};

            return (caracteresLetras.IndexOf(c) != -1);
        }

        /// <summary>
        /// retorna true se o token tem somente letras.
        /// </summary>
        /// <param name="token">tokens a verificar.</param>
        /// <param name="caracteresLetras">lista de caracteres valido.</param>
        /// <returns></returns>
        private static bool IsSomenteLetras(string token, List<char> caracteresLetras)
        {
            for (int indexLetra = 0; indexLetra < token.Length; indexLetra++)
                if (caracteresLetras.FindIndex(k => k.Equals(token[indexLetra])) == -1)
                    return false;
            return true;
        }

        /// <summary>
        /// retorna uma lista de tokens de numeros ponto-flutuante.
        /// </summary>
        /// <param name="todosTokensObtidos">todos tokens a tokenizar.</param>
        /// <returns></returns>
        internal static List<string> ObtemPontosFlutuantes(List<string> todosTokensObtidos)
        {
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            for (int x = 0; x < todosTokensObtidos.Count; x++)
            {
                if ((todosTokensObtidos[x]==".") && ((x-1)>=0) && (linguagem.IsNumero(todosTokensObtidos[x-1])))
                {
                    int indiceDigitosAnteriores = x - 1;
                    int contadorDigitosAnteriores = 1;



                    while ((indiceDigitosAnteriores >= 0) && (linguagem.IsNumero(todosTokensObtidos[indiceDigitosAnteriores]))) 
                    {
                        indiceDigitosAnteriores--;
                        contadorDigitosAnteriores++;
                    }
                    indiceDigitosAnteriores++;




                    int indiceNumero = x + 1;
                    int indiceNumeroInicial = x - 1;
                    contadorDigitosAnteriores++; //+1 para abranger o operador ".".
                    
                    
                    
                    
                    string numeroPontoFlutuante = Util.UtilString.UneLinhasLista(todosTokensObtidos.GetRange(indiceDigitosAnteriores, contadorDigitosAnteriores )); 



                    todosTokensObtidos.RemoveRange(indiceNumeroInicial, contadorDigitosAnteriores); // retira o numero inicial
                    
                    
                    
                    
                    
                    
                    while ((indiceNumero< todosTokensObtidos.Count) && (linguagem.IsNumero(todosTokensObtidos[indiceNumero])))
                    {
                        numeroPontoFlutuante += todosTokensObtidos[indiceNumero].ToString();
                        todosTokensObtidos.RemoveAt(indiceNumero);
                        indiceNumero += +1 - 1; // registro que: a lista de tokens removeu um elemento (-1), e a malha passou para o próximo elemento (+1).
                    }
                    
                    
                    todosTokensObtidos.Insert(indiceNumeroInicial, numeroPontoFlutuante.Replace(" ", ""));
                }
                
            }
         
            return todosTokensObtidos;
        }


        public class Testes : SuiteClasseTestes
        {
            char aspas = '\u0022';
            public Testes() : base("testes para classe ParserUniversal")
            {
            }


            public void TestesNomesMetodosPropriedades(AssercaoSuiteClasse assercao)
            {

                string codigoClasseX = "public class classeX { public int propriedade1;  public classeX() { int y; } public classeA metodo1(int x) { int x; } public classeA metodo1(int x, int y) { int x; }; }";
                List<string> tokens = new Tokens(codigoClasseX).GetTokens();

                assercao.IsTrue(tokens.Contains("metodo1"));
            }

            public void TesteEstudosUniaoLiterais(AssercaoSuiteClasse assercao)
            {
                string textDesenho = "Prompt.sWrite(\"desenho\")";
                List<string> tokens = new Tokens(textDesenho).GetTokens();

                string codeText = "string text=" + aspas + "A Terra eh verde da Amazonia" + aspas + ";";
                List<string> tokens2 = new Tokens(codeText).GetTokens();

                string codigo_0_1 = "string.Contains(" + aspas + "este e um texto literal" + aspas + "," + aspas + "literal" + aspas + " );";
                List<string> tokens3 = new Tokens(codigo_0_1).GetTokens();
            }
            public void Teste3Literais(AssercaoSuiteClasse assercao)
            {
                

                string codigo1 = "string  x = " + aspas + " hello           world!" + aspas;
                string codigo2 = " string y = " + aspas + "hello       world!" + aspas;
                string codigo3 = " string z = " + aspas + "tem mais um  literal!" + aspas;
                List<string> tokens = Tokenizer.GetTokens(codigo1 + codigo2+ codigo3);




                // teste automatizado.
                assercao.IsTrue(tokens != null && tokens.Count == 12);


            }
            public void Teste2Literais(AssercaoSuiteClasse assercao)
            {
                char aspas = '\u0022';

                string codigo1 = "string  x = " + aspas + " hello           world!" + aspas;
                string codigo2 = "string y = " + aspas + "hello       world!" + aspas;

                List<string> tokens = Tokenizer.GetTokens(codigo1+codigo2);




                // teste automatizado.
                assercao.IsTrue(tokens!=null && tokens.Count==8);


            }

            public void TesteLiteral(AssercaoSuiteClasse assercao)
            {
                char aspas = '\u0022';

                
                
                string codigo = "string  x = " + aspas + " hello world!" + aspas;
                List<string> tokens = Tokenizer.GetTokens(codigo);




                // teste automatizado.
                assercao.IsTrue(tokens != null && tokens.Count == 4);


            }
        }

        internal class TokenComPosicao
        {
            public string token { get; set; }
            public int coluna { get; set; }

            public TokenComPosicao(string _token, int _coluna)
            {
                this.token = (string)_token.Clone();
                this.coluna = _coluna;
            } //TokenComPosicao()

            public override string ToString()
            {
                return this.token;
            }
        } // class

        internal class ComparerTokensPosicao : IComparer<TokenComPosicao>
        {
            public int Compare(TokenComPosicao? x, TokenComPosicao? y)
            {
                if ((x == null)|| (y==null))
                {
                    return 0;

                }
                if (x.coluna < y.coluna)
                    return -1;
                if (x.coluna > y.coluna)
                    return +1;
                return 0;
            }
        }

        internal class ComparerTexts : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x.Length < y.Length)
                    return +1;
                if (x.Length > y.Length)
                    return -1;
                return 0;
            } // Compare()
        } // class

    } // class ParserUniversal
} // namespace
