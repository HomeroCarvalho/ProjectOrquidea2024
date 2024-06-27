using MathNet.Numerics.LinearAlgebra.Complex;
using ModuloTESTES;
using parser.ProgramacaoOrentadaAObjetos;
using parser.textoFormatado;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Util;
using static parser.ExpressaoGrupos;
using static System.Formats.Asn1.AsnWriter;

namespace parser
{

    /// <summary>
    /// expressao generica, conteudo de um simbolo.
    /// </summary>
    public class ExpressaoElemento: Expressao
    {
        public string elemento;

        /// <summary>
        /// retorna o token elemento.
        /// </summary>
        /// <returns></returns>
        public new string GetElemento()
        {
            return elemento;
        }

        /// <summary>
        /// retorna o tipo da expressao elemento.
        /// </summary>
        /// <returns></returns>
        public override string GetTipoExpressao()
        {
            return "no type, token of expression not definided";
        }

        /// <summary>
        /// constructor.
        /// </summary>
        /// <param name="caption">elemento da expressao.</param>
        public ExpressaoElemento(string caption)
        {
            this.elemento = caption;
            this.tokens = new List<string>() { this.elemento };
            this.typeExprss = Expressao.typeELEMENT;
        }

        public override string ToString()
        {
            return elemento;
        }
    }

    /// <summary>
    /// expressao contida entre parenteses, prioritariamente avalida antes de outras sub-expressoes.
    /// </summary>
    public class ExpressaoEntreParenteses : ExpressaoObjeto
    {
        /// <summary>
        /// expressao entre parenteses.
        /// </summary>
        public Expressao exprssParenteses;

        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="exprss_entre_parentes">a expressao entre parenteses.</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        public ExpressaoEntreParenteses(Expressao exprss_entre_parentes, Escopo escopo)
        {

            this.typeExprss = Expressao.typeENTRE_PARENTESES;
        
            if ((exprss_entre_parentes==null) || (exprss_entre_parentes.tokens==null) || (exprss_entre_parentes.tokens.Count==0))
            {
                UtilTokens.WriteAErrorMensage("bad formation in ExpressaoEntreParenteses, method: constructor ExpressaoEntreParenteses", escopo.codigo, escopo);
                return;

            }
            if ((exprss_entre_parentes.Elementos != null) && (exprss_entre_parentes.Elementos.Count > 0))
            {
                this.tipoDaExpressao = exprss_entre_parentes.Elementos[0].tipoDaExpressao;
            }
            else
            {
                this.tipoDaExpressao = exprss_entre_parentes.tipoDaExpressao;
            }
            this.exprssParenteses = exprss_entre_parentes;
            this.tokens = new List<string>();
            if ((exprss_entre_parentes.tokens != null) && (exprss_entre_parentes.tokens.Count > 0))
            {
                this.tokens.AddRange(exprss_entre_parentes.tokens);
            }
            
        }

        /// <summary>
        /// retorna o tipo da expressao.
        /// </summary>
        /// <returns></returns>
        public override string GetTipoExpressao()
        {
            return this.exprssParenteses.tipoDaExpressao;
        }

        public override string ToString()
        {
            if (this.exprssParenteses != null)
            {
                return this.exprssParenteses.ToString();
            }
            else
            {
                return "()";
            }
        }
    }






    /// <summary>
    /// expressao contendo um numero, constante.
    /// </summary>
    public class ExpressaoNumero : ExpressaoObjeto
    {
        /// <summary>
        /// numero guardado na expressao.
        /// </summary>
        public object numero;

        /// <summary>
        /// texto contendo o numero.
        /// </summary>
        private string numberText;

        /// <summary>
        /// construtor vazio.
        /// </summary>
        public ExpressaoNumero()
        {
            numero = "";
        }

       
        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="valueNumber">numero contido num texto.</param>
        public ExpressaoNumero(string valueNumber)
        {
            this.typeExprss = Expressao.typeNUMERO;
            this.numero = castingNumero(valueNumber);
            this.numberText = valueNumber;
            this.tipoDaExpressao = GetTypeNumber(valueNumber);
            this.tokens = new List<string>() { valueNumber };
        }

        /// <summary>
        /// retorna o tipo da expressao.
        /// </summary>
        /// <returns></returns>
        public override string GetTipoExpressao()
        {
            return GetTypeNumber(this.numberText);
        }

      
  
        public override string ToString()
        {
            if (this.numero != null)
            {
                return this.numberText;
            }
            else
            {
                return "number is not instantiate.";
            }
                
        }
        /// <summary>
        /// converte o texto parametro, para um numero.
        /// </summary>
        /// <param name="numeroTexto">texto contendo o numero.</param>
        /// <returns></returns>
        public static object castingNumero(string numeroTexto)
        {
            int n_int;
            float n_float;
            Double n_double;

            if (Int32.TryParse(numeroTexto, out n_int))
            {
                return n_int;
            }
            if (Double.TryParse(numeroTexto, out n_double))
            {
                return n_double;
            }
            else
            if (float.TryParse(numeroTexto, out n_float))
            {
                return n_float;
            }
            else
            {
                // um valor default para numero.
                return 0;
            }
                

        }

        /// <summary>
        /// retorna [true] se o token é nome de uma das classes de numero.
        /// </summary>
        /// <param name="token">token de nome de classe, possivelmente.</param>
        /// <returns></returns>
        public static bool isClasseNumero(string token)
        {
            return (token=="int") || (token=="float") || (token=="double"); 
        }


        /// <summary>
        /// retorna [true] se o token é um numero.
        /// </summary>
        /// <param name="token">token possivelmente um numero.</param>
        /// <returns></returns>
        public static bool isNumero(string token)
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
        /// obtem o tipo do numero: int, float, double,...
        /// </summary>
        /// <param name="numero">texto contendo o numero.</param>
        /// <returns></returns>
        public static string GetTypeNumber(string numero)
        {
            int n_int;
            float n_float;
            double n_double;

            if (int.TryParse(numero, out n_int))
                return "int";
            else
            if ((numero.Contains("f")) && (float.TryParse(numero, out n_float)))
                return "float";
            else
            if (double.TryParse(numero, out n_double))
                return "double";

            else
                return "float";

        }
    }

    /// <summary>
    /// expressao para execução de funções ou métodos, p.ex.: a.metodoA(), b.metodoB(1,1), etc..
    /// </summary>
    public partial class ExpressaoChamadaDeMetodo : Expressao
    {


        /// <summary>
        /// funcao, metodo que sera executado.
        /// </summary>
        public Metodo funcao;
        /// <summary>
        /// classe do objeto que faz a chamada, ao metodo desta mesma classe.
        /// </summary>
        public string classeDoMetodo;

        /// <summary>
        /// nome da funcao-metodo.
        /// </summary>
        public string nomeMetodo;


        /// <summary>
        /// nome do objeto que faz a chamada.
        /// </summary>
        public string nomeObjeto;

        
        /// <summary>
        /// nome de um objeto que se comportará como caller.
        /// </summary>
        public string nomeObjetoCaller;

        /// <summary>
        /// tipo do retorno da chamada do metodo.
        /// </summary>
        public string tipoRetornoMetodo;


        /// <summary>
        /// parametros do metodo chamado.
        /// </summary>
        public List<Expressao> parametros;

        /// <summary>
        /// objeto que chama o metodo.
        /// </summary>
        public Objeto objectCaller;


        /// <summary>
        /// se true a funcao da expressao e um metodo-parametro.
        /// </summary>
        public bool isMethodParameter = false;
        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="obj_caller">objeto que chama o metodo.</param>
        /// <param name="metodo">funcao chamada.</param>
        /// <param name="parametros">parametros da chamada, parametros da funcao.</param>
        public ExpressaoChamadaDeMetodo(Objeto obj_caller, Metodo metodo, List<Expressao> parametros)
        {

            this.typeExprss = Expressao.typeCHAMADA_METODO;
            this.objectCaller = obj_caller.Clone();
            this.nomeObjeto = objectCaller.GetNome();
            this.nomeMetodo = metodo.nome;
            this.classeDoMetodo = metodo.nomeClasse;

            this.funcao = metodo.Clone();
            this.parametros = parametros;
            this.tipoRetornoMetodo = metodo.tipoReturn;
            
            if (objectCaller.isWrapperObject)
            {
                this.tipoDaExpressao = objectCaller.tipoElemento;
            }
            else
            {
                this.tipoDaExpressao = metodo.tipoReturn;
            }


            // tokens do objeto, operador dot, parenteses abre.
            this.tokens = new List<string>() { nomeObjeto, ".", nomeMetodo, "(" };

            // obtem os tokens de parametros.
            if ((parametros != null) && (parametros.Count > 0))
            {
                for (int i = 0; i < parametros.Count-1; i++)
                {
                    tokens.AddRange(parametros[i].tokens);
                    tokens.Add(",");

                }
                if ((parametros.Count - 1 >= 0) && (parametros[parametros.Count - 1] != null) && (parametros[parametros.Count - 1].tokens != null)) 
                {
                    
                    tokens.AddRange(parametros[parametros.Count-1].tokens);

                }
            }
            // token do parenteses fecha.
            this.tokens.Add(")");
        }

     

        /// <summary>
        /// retorna o tipo da expressao.
        /// </summary>
        /// <returns></returns>
        public override string GetTipoExpressao()
        {
            return this.tipoRetornoMetodo;
        }


        public override string ToString()
        {
            string str = "";
            str += nomeObjeto + "." + nomeMetodo;
            if ((parametros != null) || (parametros.Count == 0))
            {
                str += "()";
            }
            else
            {
                str += "(";
                for (int x = 0; x < parametros.Count - 1; x++)
                    str += parametros[x].ToString() + ",";
                str += parametros[parametros.Count - 1].ToString();

                str += ")";
            }

            return str;
        }

    }
    
    /// <summary>
    /// expressao para acesso a propriedades de objetos, p.ex.: a.propriedadeA, a.propriedadeA.propriedadeB, etc..
    /// </summary>
    public class ExpressaoPropriedadesAninhadas : Expressao
    {

        /// <summary>
        /// propriedade 1a.
        /// </summary>
        public string propriedade;
        /// <summary>
        /// nome do objeto caller.
        /// </summary>
        public string nomeObjeto;
        /// <summary>
        /// classe do objeto caller.
        /// </summary>
        public string classeObjeto;

        /// <summary>
        /// objeto caller. contem as propriedades aninhadas da expressao.
        /// </summary>
        public Objeto objectCaller;
     
        /// <summary>
        /// contem as propriedades aninhadas.
        /// </summary>
        public List<Objeto> aninhamento = new List<Objeto>();
        /// <summary>
        /// contem os nomes das propriedades aninhadas.
        /// </summary>
        public List<string> propriedadesAninhadas = new List<string>();

        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="aninhamento">lista de propriedades aninhadas., p.ex. ob1.prop1.propp2.prop3</param>
        /// <param name="propriedadesANINHADAS">nome das propriedades aninhadas.</param>
        public ExpressaoPropriedadesAninhadas(List<Objeto> aninhamento, List<string> propriedadesANINHADAS)
        {
            this.classeObjeto = aninhamento[0].GetTipo();
            this.nomeObjeto = aninhamento[0].GetNome();
            this.objectCaller = aninhamento[0].Clone();
            this.tipoDaExpressao = aninhamento[aninhamento.Count - 1].GetTipo();
            this.typeExprss = Expressao.typePROPRIEDADES_ANINHADADAS;
            this.aninhamento = aninhamento;
            this.propriedadesAninhadas = propriedadesANINHADAS;
            for (int i = 0; i < aninhamento.Count; i++)
            {
                tokens.Add(aninhamento[i].GetNome());

                if (i < propriedadesAninhadas.Count)
                {
                    tokens.Add(".");
                    tokens.Add(propriedadesAninhadas[i]);
                }
                else
                {
                    tokens.Add(propriedadesAninhadas[i]);
                }

            }
        }

        /// <summary>
        /// construtor vazio.
        /// </summary>
        public ExpressaoPropriedadesAninhadas()
        {
            this.classeObjeto = null;
            this.nomeObjeto = null;
            this.propriedade = null;
            this.typeExprss = Expressao.typePROPRIEDADES_ANINHADADAS;
            this.objectCaller = null;
        }

        public override string ToString()
        {
            string str = "";

            if (objectCaller != null && (objectCaller.nome != null))
            {
                str += objectCaller.nome + ".";
            }

            if ((aninhamento != null) && (aninhamento.Count > 0))
            {
                for (int i = 0; i < aninhamento.Count-1; i++)
                {
                    str += aninhamento[i].nome + ".";
                }
                if (aninhamento.Count - 1 >= 0)
                {
                    str += aninhamento[aninhamento.Count - 1].nome;
                }
            }
            

            return str;
        }

    }

    /// <summary>
    /// expressao para operacoes de atribuicao. p.ex.: x=x+1, obj1.propriedade1= 1, etc...
    /// </summary>
    public class ExpressaoAtribuicao : Expressao
    {


        /// <summary>
        /// expressao do valor a atribuir.
        /// </summary>
        public Expressao ATRIBUICAO;

        /// <summary>
        /// expressao que contem o objeto a receber a atribuicao.
        /// </summary>
        public Expressao exprssObjetoAAtribuir;


        /// <summary>
        /// objeto a receber a expressao de atribuição.
        /// </summary>
        public Objeto objetoAtribuir;

        /// <summary>
        /// expressao de atribuicao. contem uma expressao de objeto a atribuir, e uma expressao de atribuicao.
        /// </summary>
        /// <param name="exprssObjeto">expressao que contem o objeto a atribuir, podendo ser ExpressaoObjeto, ou ExpressaoAtribuir.</param>
        /// <param name="exprssAtribuicao">expressao que contem o valor a atribuir.</param>
        /// <param name="escopo">contexto onde as expressoes estão.</param>
        public ExpressaoAtribuicao(Expressao exprssObjeto, Expressao exprssAtribuicao, Escopo escopo)
        {
            this.typeExprss = Expressao.typeATRIBUICAO;

            this.ATRIBUICAO = exprssAtribuicao;
            this.exprssObjetoAAtribuir = exprssObjeto;
         
            this.tipoDaExpressao = exprssAtribuicao.tipoDaExpressao;
            

            // tipo de expressao do objeto a atribuir: EXPRESSAO_OBJETO.
            if (exprssObjeto.typeExprss == Expressao.typeOBJETO)
            {
                ExpressaoObjeto expOBJETO = (ExpressaoObjeto)exprssObjeto;

                // se a expressao for numero ou literal, faz o calculo do valor.
                if (((exprssAtribuicao.Elementos.Count == 1) &&
                    (exprssAtribuicao.Elementos[0]!=null)) &&
                    ((exprssAtribuicao.Elementos[0].typeExprss == Expressao.typeNUMERO) ||
                     (exprssAtribuicao.Elementos[0].typeExprss == Expressao.typeLITERAL)))
                {

                    if (expOBJETO.objectCaller.valor==null)
                    {
                        EvalExpression eval = new EvalExpression();
                        object valorObjetoDaAtribuicao = eval.EvalPosOrdem(exprssAtribuicao, escopo);
                        expOBJETO.objectCaller.valor = valorObjetoDaAtribuicao;

                        objetoAtribuir = expOBJETO.objectCaller;

                    }

                }
                else
                {
                    objetoAtribuir = expOBJETO.objectCaller;
                }
            }
            // tipo de expressao do objeto a atribuir: EXPRESSAO_PROPRIEDADES ANINHADAS.
            if (exprssObjeto.typeExprss == Expressao.typePROPRIEDADES_ANINHADADAS)
            {
                ExpressaoPropriedadesAninhadas exprssAninhada = (ExpressaoPropriedadesAninhadas)exprssObjeto;
                if ((exprssAninhada.aninhamento != null) && (exprssAninhada.aninhamento.Count > 0))
                {
                    objetoAtribuir = exprssAninhada.aninhamento[exprssAninhada.aninhamento.Count - 1];
                }

            }
                        



            this.tokens = new List<string>();
            if (exprssObjetoAAtribuir != null)
            {
                this.tokens = exprssObjetoAAtribuir.tokens.ToList<string>();
            }
            if (exprssAtribuicao != null)
            {
                this.tokens.AddRange(exprssAtribuicao.tokens);
            }
            
        }

        public override string ToString()
        {
            string str = "";
            if (exprssObjetoAAtribuir != null) 
            {
                str += exprssObjetoAAtribuir.ToString() + "=";
            }
            if (ATRIBUICAO != null) 
            {
                str += ATRIBUICAO.ToString();
            }
            return str;
        }
    }


    /// <summary>
    /// expressao utilizado em [ExpressionSearch]. inativado
    /// com advendo de [ExpressaoPorClassificacao], ou [ExpressaoGrupos] (LEGADO).
    /// </summary>
    public class ExpressaoInstanciacao: Expressao
    {
        public string nomeObjeto;
        public string nomeClasseDoObjeto;
       
        public Expressao exprssAtribuicao;
        

        


        public ExpressaoInstanciacao(string nomeObjeto, string nomeClasse, Expressao exprssAtribuicao, Escopo escopo)
        {
            this.typeExprss = Expressao.typeINSTANCIACAO;
            this.nomeClasseDoObjeto = nomeClasse;
            this.nomeObjeto= nomeObjeto;
            this.exprssAtribuicao= exprssAtribuicao;
            this.tipoDaExpressao = nomeClasse;

            // INSTANCIA O OBJETO DE INSTANCIAÇÃO, se não houver instanciado.
            if (escopo.tabela.GetObjeto(nomeObjeto, escopo) == null)
            {
                Objeto obj = new Objeto("public", nomeClasseDoObjeto, nomeObjeto, null);
                escopo.tabela.GetObjetos().Add(obj);
            }
            this.tokens = new List<string>() { nomeObjeto,".","="};
            this.tokens.AddRange(exprssAtribuicao.tokens);

            
        }

        public override string ToString()
        {
            string str = "";
            if (nomeObjeto == null)
            {
                return str;
            }
            str += nomeObjeto + "= create";
            if (exprssAtribuicao == null)
            {
                return str;
            }
            str += "(";
            str += exprssAtribuicao.ToString();
            str+= ")";

            return str;

        }
    }


    /// <summary>
    /// expressao formada por um objeto.
    /// </summary>
    public class ExpressaoObjeto : Expressao
    {
        /// <summary>
        /// classe do objeto.
        /// </summary>
        public string classeObjeto;
        /// <summary>
        /// nome do objeto.
        /// </summary>
        public string nomeObjeto;

        /// <summary>
        /// objeto caller, o proprio objeto da expressao.
        /// </summary>
        public Objeto objectCaller;



        public List<string> classesParametroObjeto;


        /// <summary>
        /// se true o objeto é um metodo objeto.
        /// </summary>
        public bool isFunction = false;

        /// <summary>
        /// contador de nomes aleatorios.
        /// </summary>
        public static int contadorNomes = 0;
       
        /// <summary>
        /// construtor vazio.
        /// </summary>
        public ExpressaoObjeto()
        {

        }

        /// <summary>
        /// construtor APENAS PARA METODOS-PARAMETROS.
        /// </summary>
        /// <param name="objetoDaExpressao">objeto metodo-parameto.</param>
        /// <param name="classesDeMetodoParametro">classe em que o metodo-parametro, tem uma entrada.</param>
        public ExpressaoObjeto(Objeto objetoDaExpressao, List<string>classesDeMetodoParametro)
        {
            this.objectCaller = objetoDaExpressao;
            this.isFunction = true;
            this.tipoDaExpressao = objetoDaExpressao.tipo;
            this.classesParametroObjeto = classesDeMetodoParametro;
            this.typeExprss = Expressao.typeOBJETO;
        }
      
        /// <summary>
        /// expressao contendo um objeto. 
        /// </summary>
        /// <param name="objetoDaExpressao">objeto da expressao.</param>
        public ExpressaoObjeto(Objeto objetoDaExpressao)
        {
            objectCaller = objetoDaExpressao;
             
            
            
            // caso que o objeto é um metodo.
            if (objetoDaExpressao is Metodo)
            {
                this.isFunction = true;
                
            }
            else
            {
                this.isFunction = false;
               
            }
            this.nomeObjeto = objetoDaExpressao.nome;
            classeObjeto = objetoDaExpressao.tipo;
            tipoDaExpressao = objetoDaExpressao.tipo;
            this.typeExprss = Expressao.typeOBJETO;
            


            Elementos = new List<Expressao>();
            tokens = new List<string>() { objectCaller.GetNome() };
           
        }



        /// <summary>
        /// retorna o objeto da expressao.
        /// </summary>
        /// <returns></returns>
        public override object GetElemento()
        {
            return objectCaller;
        }

        public override string ToString()
        {
            string str = "";
            if (nomeObjeto == null)
            {
                return "";
            }
            str += nomeObjeto;

            if ((Elementos != null) && (Elementos.Count > 0))
            {
                for (int x = 0; x < Elementos.Count; x++)
                {
                    str += Elementos[x].ToString() + " ";
                }
                    

            }

            return str;

        }

        /// <summary>
        /// retorna o tipo da expressao: o tipo do objeto.
        /// </summary>
        /// <returns></returns>
        public override string GetTipoExpressao()
        {
            return this.tipoDaExpressao;
        }
    }

    /// <summary>
    /// expressao para processamento de sub-expressoes com operadores, ex.: a+b, x+1, etc..
    /// </summary>
    public class ExpressaoOperador : Expressao
    {

        /// <summary>
        ///  classe onde esta o operador.
        /// </summary>
        public string classeOperador;
        /// <summary>
        /// nome do operador.
        /// </summary>
        public string nomeOperador;
  



        /// <summary>
        /// tipo de operando.
        /// </summary>
        public HeaderOperator.typeOperator typeOperandos = HeaderOperator.typeOperator.binary;

        /// <summary>
        /// operador da expressão.
        /// </summary>
        public Operador operador;
        /// <summary>
        /// nome do primeiro operando.
        /// </summary>
        public Expressao operando1;
        /// <summary>
        /// nome do segundo operando.
        /// </summary>
        public Expressao operando2;
        

        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="op">operador da expressao.</param>
        public ExpressaoOperador(Operador op)
        {
            this.typeExprss = Expressao.typeOPERADOR;
            if (op != null)
            {
                
                
                if (op.tipoReturn != null)
                {
                    this.tipoDaExpressao = op.tipoReturn;
                }

                if (op.nomeClasse != null)
                {
                    this.classeOperador = op.nomeClasse;
                }
                if (op.nome != null)
                {
                    this.nomeOperador = op.nome;
                }
                if (op != null)
                {
                    this.operador = op;
                }
                
                if (op.nomeClasse != null)
                {
                    this.tipoDaExpressao = op.nomeClasse;
                }

                if (nomeOperador != null)
                {
                    this.tokens = new List<string>() { nomeOperador };
                }
                
            }
            else
            {
                this.tokens = new List<string>();
            }
        }

        public override string ToString()
        {
            if (nomeOperador == null)
            {
                return "";
            }
            else
            {
                return nomeOperador;
            }
           
        }

        public override string GetTipoExpressao()
        {
            return this.operador.tipoRetorno;
        }
    }


    /// <summary>
    /// expressao para valores nulos resultante em operacoes, chamada de funcoes/metodos.
    /// </summary>
    public class ExpressaoNILL: Expressao
    {
        /// <summary>
        /// literal contendo o tipo elemento da expressao.
        /// </summary>
        public string nill = "NILL";

        /// <summary>
        /// construtor vazio.
        /// </summary>
        public ExpressaoNILL()
        {
            this.typeExprss = Expressao.typeNILL;
            this.tokens = new List<string>() { "NILL" };
        }

        /// <summary>
        /// obtem o tipo da expressao.
        /// </summary>
        /// <returns></returns>
        public override string GetTipoExpressao()
        {
            return "NILL";
        }

        public override string ToString()
        {
            return "NILL";
        }


    }
    /// <summary>
    /// expressao para constantes de texto, ex.: a="textoA". "textoA" é uma expressao literal text.
    /// </summary>
    public class ExpressaoLiteralText: Expressao
    {
        /// <summary>
        /// constante contendo a literal da expressao.
        /// </summary>
        public string literalText;

        /// <summary>
        /// caracter de literais constantes.
        /// </summary>
        public static new char aspas = '\u0022';

        /// <summary>
        /// caracter delimitador para char constantes.
        /// </summary>
        public static char singleQuote = '\'';



        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="literalText">literal da expressao.</param>
        public ExpressaoLiteralText(string literalText)
        {
            if ((literalText!=null) && (literalText.Length > 0))
            {
                if ((literalText[0] == aspas) && (literalText.Length >= 2)) 
                {
                    literalText = literalText.Substring(1, literalText.Length - 2);
                }
            }
            this.typeExprss = Expressao.typeLITERAL;
            this.literalText = literalText;
            this.tipoDaExpressao = "string";
            this.tokens = new List<string>() { literalText };
        }

        public override string GetTipoExpressao()
        {
            return "string";
        }


        /// <summary>
        /// verifica se um token é uma literal.
        /// </summary>
        /// <param name="token">token a verificar.</param>
        /// <returns>[true] se o token é uma literal.</returns>
        public static bool isConstantLiteral(string token)
        {
            if ((token==null) || (token.Length==0)) 
            {
                return false;
            }
            else
            {
                return (token.IndexOf("\"") == 0) && (token.LastIndexOf("\"") == token.Length - 1);
            }
        }



        public override string ToString()
        {
            if (this.literalText != null)
            {
                return this.literalText;
            }
            else
            {
                return "";
            }

        }
    }

    /// <summary>
    /// classe que instancia e processa expressoes. quase tudo no codigo eh expressao:
    /// chamadas de funcao, operações artimeticas, booleanas condicionais, propriedades aninhadas, etc...
    /// </summary>
    public partial class Expressao
    {

        /// <summary>
        /// tipo da expressao.
        /// </summary>
        public int typeExprss = Expressao.typeEXPRESSAO;
        /// <summary>
        ///  indice do token currente no processamento da expressao.
        /// </summary>
        public int indexToken;

        /// <summary>
        /// lista de tokens que formam a expressao.
        /// </summary>
        public List<string> tokens = new List<string>();

        /// <summary>
        /// lista de classes orquidea, na formacao de headers
        /// </summary>
        public static List<Classe> classesDaLinguagem = null;

        /// <summary>
        /// tipo da expressão: int, string, classe,...
        /// </summary>
        public string tipoDaExpressao;


        /// <summary>
        /// utilizado para processamento por expressoes regex.
        /// </summary>
        public Type typeExpressionBySearcherRegex;
        

        /// <summary>
        /// partes da expressao.
        /// </summary>
        public List<Expressao> Elementos = new List<Expressao>();


        /// <summary>
        /// determina se a expressao foi modificada com alguma avaliacao de expressao.
        /// </summary>
        public bool isModify = true;

        /// <summary>
        /// determina se a expressao está em pos-ordem ou não.
        /// </summary>
        public bool isPosOrdem = false;

        /// <summary>
        /// valor apos um processamento em [Eval].
        /// </summary>
        public object oldValue;


        /// <summary>
        /// retorna as sub-expressoes.
        /// </summary>
        /// <returns></returns>
        public virtual object GetElemento() { return this.Elementos; }



        /// <summary>
        /// contem nome de classes, com propriedades, metodos, operadores de cada classe, registrado.
        /// </summary>
        public static FileHeader headers = null;
        /// <summary>
        /// contem os header de classes base da linguagem.
        /// </summary>
        private static List<HeaderClass> headersDaLinguagem = new List<HeaderClass>();


        public const int typeATRIBUICAO = 0;
        public const int typeCHAMADA_METODO = 1;
        public const int typePROPRIEDADES_ANINHADADAS = 2;
        public const int typeNUMERO = 3;
        public const int typeLITERAL = 4;
        public const int typeELEMENT = 5;
        public const int typeOBJETO = 6;
        public const int typeOPERADOR = 7;
        public const int typeENTRE_PARENTESES = 8;
        public const int typeNILL = 9;
        public const int typeINSTANCIACAO = 10;
        public const int typeEXPRESSAO = 11;



        /// <summary>
        /// caracter de literais constantes.
        /// </summary>
        public static char aspas = '\u0022';
     
        /// <summary>
        /// singleton da classe.
        /// </summary>
        private static Expressao singletonExpressao;

        /// <summary>
        /// retorna o singleton.
        /// </summary>
        public static Expressao Instance
        {
            get
            {
                if (singletonExpressao == null)
                    singletonExpressao = new Expressao();

                return singletonExpressao;

            }

        }
 
        /// <summary>
        /// construtor.
        /// </summary>
        public Expressao()
        {
            this.tokens = new List<string>();
            this.indexToken = 0;
            this.tipoDaExpressao = "";
            this.typeExprss = Expressao.typeEXPRESSAO;
        }

        /// <summary>
        /// construtor de expressoes.
        /// </summary>
        /// <param name="codigoExpressao">trecho de codigo contendo os tokens da expressao.</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        public Expressao(string codigoExpressao, Escopo escopo)
        {
            // se nao inicializou os headers, inicializa. headers são essencial
            // na validação de objetos, metodos, operadores, propriedades..
            if (Expressao.headers == null)
            {
                Expressao.InitHeaders("");
            }
   
            List<string> tokensExpressao= new Tokens(codigoExpressao).GetTokens();
            this.InitPROCESSO(tokensExpressao.ToArray(), escopo);



            
        }

        /// <summary>
        /// construtor de expressoes.
        /// </summary>
        /// <param name="tokensExpressao">tokens da expressao.</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        public Expressao(string[] tokensExpressao, Escopo escopo)
        {
            this.typeExprss= Expressao.typeEXPRESSAO;
            // se nao inicializou os headers, inicializa. headers são essencial
            // na validação de objetos, metodos, operadores, propriedades..
            if (Expressao.headers == null)
            {
                Expressao.InitHeaders("");
            }
            InitPROCESSO(tokensExpressao, escopo);
        }


        /// <summary>
        /// inicializa o processamento de expressoes e sub-expressoes.
        /// </summary>
        /// <param name="tokensExpressao">tokens da expressao.</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        private void InitPROCESSO(string[] tokensExpressao, Escopo escopo)
        {
       
            this.tokens = tokensExpressao.ToList<string>();
            string str_tokens = UtilTokens.FormataEntrada(Utils.OneLineTokens(this.tokens));

            // instancia o motor de processamento de expressoes.
            ExpressaoPorClassificacao expressaoClassificacao = new ExpressaoPorClassificacao();

            // encontra expressoes, atraves do extrator de expressoes.
            List<Expressao> expressionsFound = expressaoClassificacao.ExtraiExpressoes(str_tokens, escopo);



            if ((expressionsFound != null) && (expressionsFound.Count > 0))
            {
                // compoe um container de expressao, compatibilidade com codigo de todo o projeto.
                this.Elementos.AddRange(expressionsFound);
                
              
                for (int i = 0; i < expressionsFound.Count; i++)
                {
                    if (expressionsFound[i]!=null)
                     {
                        if (expressionsFound[i].tipoDaExpressao == "bool"){

                            this.tipoDaExpressao = "bool";
                        }

                    }



                }
                

            }



        }

        /// <summary>
        /// clona a expressao.
        /// </summary>
        /// <returns>retorna uma expressao clone.</returns>
        public Expressao Clone()
        {
            Expressao expressaoClonada = new Expressao();
            expressaoClonada.indexToken = this.indexToken;
            expressaoClonada.tokens = this.tokens.ToList<string>();
            expressaoClonada.Elementos = this.Elementos.ToList<Expressao>();
            expressaoClonada.tipoDaExpressao = this.tipoDaExpressao;
            
            return expressaoClonada;
        }


        /// <summary>
        /// extrai sub-expressoes, a partir de uma lista de tokens.
        /// utilizado muito em extração de lista de expressoes - parametros.
        /// </summary>
        /// <param name="tokensDasSubExpressoes">tokens das sub-expressoes.</param>
        /// <param name="escopo">contexto onde a sub-expressao está.</param>
        /// <returns></returns>
        public static List<Expressao> ExtraiExpressoes(List<string> tokensDasSubExpressoes, Escopo escopo)
        {
            string codigo= Util.UtilString.UneLinhasLista(tokensDasSubExpressoes);
            ExpressaoPorClassificacao exprssEngine = new ExpressaoPorClassificacao();
            List<Expressao> listaExpressao = exprssEngine.ExtraiMultipasExpressoesIndependentes(codigo, escopo);

            if (listaExpressao == null)
            {
                UtilTokens.WriteAErrorMensage("error in processing expression, code: " + Utils.OneLineTokens(tokensDasSubExpressoes) + codigo, tokensDasSubExpressoes, escopo);
                return null;
            }
            else
            {
                return listaExpressao;
            }
        }



        /// <summary>
        /// arruma a expressao para pos-ordem, para avaliação de valores da expressao.
        /// </summary>
        public  void PosOrdemExpressao()
        {

            if ((this.Elementos == null) || (this.Elementos.Count == 0) || (Elementos[0] == null))
            {
                return;
            }

            Expressao expressaoRetorno = new Expressao();

            Pilha<Operador> pilha = new Pilha<Operador>("operadores");
            List<Operador> operadoresPresentes = new List<Operador>();
            int index = 0;
    

            while (index < Elementos.Count)
            {
                if (this.Elementos[index] == null)
                {
                    index++;
                    continue;
                }

                if (this.Elementos[index].typeExprss == Expressao.typeEXPRESSAO)
                {
                    this.Elementos[index].PosOrdemExpressao();
                    return;
                }

                if (this.Elementos[index].typeExprss == Expressao.typeATRIBUICAO)
                {
                    ((ExpressaoAtribuicao)this.Elementos[index]).ATRIBUICAO.PosOrdemExpressao();
                    return;
                }
                else
                if (this.Elementos[index].typeExprss == Expressao.typeENTRE_PARENTESES)
                {
                    if (((ExpressaoEntreParenteses)Elementos[index]).exprssParenteses != null)
                    {
                        ((ExpressaoEntreParenteses)Elementos[index]).exprssParenteses.PosOrdemExpressao();
                        expressaoRetorno.Elementos.Add(Elementos[index]);
                    }

                }
                else
                if (RepositorioDeClassesOO.Instance().GetClasse(Elementos[index].ToString()) != null)
                {
                    index++;
                    continue; // definição do tipo da variavel não é avaliado em Expressao.PosOrdem.    
                }
                else
                if (Elementos[index].typeExprss == Expressao.typeNUMERO)
                {
                    expressaoRetorno.Elementos.Add(Elementos[index]);
                }
                else
                if (this.Elementos[index].typeExprss == Expressao.typeNILL)
                {
                    expressaoRetorno.Elementos.Add(this.Elementos[index]);
                }
                else
                if (ExpressaoNumero.isNumero(this.Elementos[index].ToString()))
                {
                    expressaoRetorno.Elementos.Add(this.Elementos[index]);
                }
                else
                if (this.Elementos[index].typeExprss == Expressao.typeOBJETO)
                {
                    expressaoRetorno.Elementos.Add(this.Elementos[index]);
                }
                else
                if (this.Elementos[index].typeExprss == Expressao.typeCHAMADA_METODO)
                {
                    ExpressaoChamadaDeMetodo chamada = (ExpressaoChamadaDeMetodo)this.Elementos[index];
                    if (chamada.parametros != null)
                        // coloca em pos ordem cada expressao que eh  um parametro da chamada de funcao.
                        for (int x = 0; x < chamada.parametros.Count; x++)
                            ((Expressao)chamada.parametros[x]).PosOrdemExpressao();

                    expressaoRetorno.Elementos.Add(chamada);
                }
                else
                if (this.Elementos[index].typeExprss == Expressao.typePROPRIEDADES_ANINHADADAS)
                {
                    expressaoRetorno.Elementos.Add(this.Elementos[index]);
                }
                else
                if (this.Elementos[index].typeExprss== Expressao.typeOPERADOR)
                {

                    Operador op = ((ExpressaoOperador)this.Elementos[index]).operador;

                    // verificar o mecanismo de prioridade.
                    while ((!pilha.Empty()) && (pilha.Peek().prioridade >= op.prioridade))
                    {
                        Operador op_topo = pilha.Pop();
                        expressaoRetorno.Elementos.Add(new ExpressaoOperador(op_topo));
                    }
                    pilha.Push(op);
                }
                else
                if (this.Elementos[index].typeExprss == Expressao.typeLITERAL)
                {
                    expressaoRetorno.Elementos.Add(this.Elementos[index]);
                }
                index++;
            }

            while (!pilha.Empty())
            {
                Operador operador = pilha.Pop();
                ExpressaoOperador expressaoOperador = new ExpressaoOperador(operador);
                expressaoRetorno.Elementos.Add(expressaoOperador);
            }

            this.Elementos = expressaoRetorno.Elementos;
            this.isModify = expressaoRetorno.isModify;

          


        }




        public override string ToString()
        {

            string str = "";
            if ((this.Elementos!=null) && (this.Elementos.Count > 0))
            {
                for (int x=0;x<this.Elementos.Count;x++)
                {
                    if ((this.Elementos[x]!=null) && (this.Elementos[x].tokens!=null) && (this.Elementos[x].tokens.Count > 0))
                    {
                        str+= Util.UtilString.UneLinhasLista(this.Elementos[x].tokens); 

                    }
                }

                return str;
            }
            else
            {
                if ((this.tokens != null) && (this.tokens.Count > 0))
                    str = Utils.OneLineTokens(this.tokens);
                return str;
            }
            
            
            
           
           
        }


        /// <summary>
        /// retorna o tipo da expressao (int, string, double, char, classes orquidea, classes importadas,etc...
        /// </summary>
        /// <returns></returns>
        public virtual string GetTipoExpressao()
        {
            return this.tipoDaExpressao;
        }

        /// <summary>
        /// inicialia o sistema de headers: processamento de classes, sem validacao de objetos, operadores, propriedades, metodos,ect...
        /// </summary>
        /// <param name="codigoClasses">codigo de todos tokens de todas classes a compilar.</param>
        public static void InitHeaders(string codigoClasses)
        {
            List<string> tokensClasses = null;

            if (codigoClasses != "")
                // obtem os tokens de classes do codigo do programa.
                tokensClasses = new Tokens(codigoClasses).GetTokens();
            else
                tokensClasses = new List<string>();
        
            InitHeaders(tokensClasses);

        }


        /// <summary>
        /// inicialia o sistema de headers: processamento de classes, sem validacao de objetos, operadores, propriedades, metodos,ect...
        /// </summary>
        /// <param name="tokensClasses">tokens de todas classes a compilar.</param>
        public static void InitHeaders(List<string> tokensClasses)
        {
            
            if ((tokensClasses != null) && (tokensClasses.Count > 0))
            {
                // REMOVE CLASSE ANTIGAS, COM MESMO NOME DA INSTANCIACAO DE CLASSES ATUAIS.
                if ((SystemInit.isResetFILE_HEADER) && (Expressao.headers != null) && (Expressao.headers.cabecalhoDeClasses != null) && (Expressao.headers.cabecalhoDeClasses.Count > 0)) 
                {
                    SystemInit.isResetFILE_HEADER=false;

                    
                    Expressao.headers.ExtractCabecalhoDeClasses(tokensClasses);
                    

                }


            }

            if (Expressao.headers == null)
            {

                

                // constroi os headers de classes base da linguagem, armazenadas no objeto LinguagemOrquiea.
                Expressao.headers = new FileHeader();
                if ((classesDaLinguagem == null) || (classesDaLinguagem.Count == 0))
                {
                    classesDaLinguagem = LinguagemOrquidea.Instance().GetClasses();


                    Expressao.headers.ExtractHeadersClassFromClassesOrquidea(classesDaLinguagem, headersDaLinguagem);

                }
                else
                {
                    Expressao.headers.cabecalhoDeClasses.AddRange(headersDaLinguagem);
                }

                Expressao.headers.ExtractCabecalhoDeClasses(tokensClasses);



            }
        }

        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes classe [Expressao]")
            {


               
            
            }

            public void TestCreateSetElementGetElementMatriz(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                // typeItem[,] m= [exprss,exprss]
                string code_create_0_0 = "int x=1; int y=5; int a=2; int[,] m1= [20,20];";

                ProcessadorDeID compilador = new ProcessadorDeID(code_create_0_0);
                compilador.Compilar();


                string codigo_atribuicao_02 = "a= m1[x+1,y+5]+a";
                string codigo_atribuicao_03 = "a= m1[x+1,y+5]+1";
                string codigo_atribuicao_01 = "a= m1[1,5]";


                string codigoGet = "m1[x+1,y+5]";
                string codigoSet = "m1[x+1,y+5]=1";



                Expressao exprss_get = new Expressao(codigoGet, compilador.escopo);
                Expressao exprss_set = new Expressao(codigoSet, compilador.escopo);


                Expressao exprss_atribuicao_03 = new Expressao(codigo_atribuicao_03, compilador.escopo);
                Expressao exprss_atribuicao_01 = new Expressao(codigo_atribuicao_01, compilador.escopo);
                Expressao exprss_atribuicao_02 = new Expressao(codigo_atribuicao_02, compilador.escopo);





                assercao.IsTrue(AssertGetElement(exprss_get), codigoGet);
                assercao.IsTrue(AssertSetElement(exprss_set), codigoSet);
                assercao.IsTrue(AssertExpressaoAtribuicao(exprss_atribuicao_03), codigo_atribuicao_03);
                assercao.IsTrue(AssertExpressaoAtribuicao(exprss_atribuicao_01), codigo_atribuicao_01);
                assercao.IsTrue(AssertExpressaoAtribuicao(exprss_atribuicao_02), codigo_atribuicao_02);





            }


            public void TestesExpressoesMaisDe1ElementoWrapper(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                string codigo_2_wrappers_01 = "a= v1[0]+v2[1]";
                string codigo_2_wrappers_02 = "a= v1[0]+v2[1]+ 1";
                string code_0_0 = "int[] v1[10]; int[] v2[20]; int a=5;";

                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                compilador.Compilar();



                Expressao exprss2_wrappers_01 = new Expressao(codigo_2_wrappers_01, compilador.escopo);
                Expressao exprss2_wrappers_02 = new Expressao(codigo_2_wrappers_02, compilador.escopo);

                try
                {
                    assercao.IsTrue(
                        exprss2_wrappers_01.Elementos[0].GetType() == typeof(ExpressaoAtribuicao) &&
                        ((ExpressaoAtribuicao)exprss2_wrappers_01.Elementos[0]).ATRIBUICAO.Elementos[0].tokens.Contains("GetElement"), codigo_2_wrappers_01);


                    assercao.IsTrue(
                    exprss2_wrappers_02.Elementos[0].GetType() == typeof(ExpressaoAtribuicao) &&
                        ((ExpressaoAtribuicao)exprss2_wrappers_02.Elementos[0]).ATRIBUICAO.Elementos[0].tokens.Contains("GetElement"), codigo_2_wrappers_01);



                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
            }

            
            private bool AssertExpressaoAtribuicao(Expressao exprss)
            {
                string codeError = "";
                try
                {
                    return exprss.Elementos[0].GetType() == typeof(ExpressaoAtribuicao);
                }
                catch (Exception e)
                {

                    codeError = e.Message;
                    return false;
                }
            }
            public void TesteExtracaoExpressoes(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;
                SystemInit.InitSystem();
                string code_0_0_create = "int x=1; int y=5; int t=3";
                string code_exprss_0_0 = "t=x+y;";

                ProcessadorDeID compilador= new ProcessadorDeID(code_0_0_create);
                compilador.Compilar();


                Expressao exprssResult = new Expressao(code_exprss_0_0, compilador.escopo);

                try
                {
                    assercao.IsTrue(
                        exprssResult.Elementos[0].GetType() == typeof(ExpressaoAtribuicao), code_exprss_0_0);
                }
                catch (Exception ex)
                {
                    string codeError = ex.Message;
                    assercao.IsTrue(false, "Falha no teste: " + code_exprss_0_0);
                }
            }


   


            public void TesteUmaChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();



                //preparacao para testes unitarios em massa.
                string codigoClasseA = "public class classeA { public int propriedadeA;  public classeA() { };  public int metodoA(int a, int b ){ int x =1; x=x+1;}; };";





                // testes unitarios em massa.
                string codigoCreate = "classeA objetoA= create(); int x=1; int y=1;";
                string code_chamadaDeMetodo = "objetoA.metodoA(1,5);";
                string code_propriedadeAninhadas = "objetoA.propriedadeA";
                string code_chamadaDeMetodoESTATICO = "double.abs(x)";

                string codigoTotal = codigoClasseA + codigoCreate;

                Escopo escopo = new Escopo(codigoTotal);
                ProcessadorDeID compilador = new ProcessadorDeID(codigoTotal);
                compilador.Compilar();


                Expressao expressao_chamadaDeMetodoEstatico = new Expressao(code_chamadaDeMetodoESTATICO, compilador.escopo);
                Expressao expressao_chamadaDeMetodo = new Expressao(code_chamadaDeMetodo, compilador.escopo);
                Expressao expressao_propriedades = new Expressao(code_propriedadeAninhadas, compilador.escopo);
                

                try
                {
                    assercao.IsTrue(expressao_chamadaDeMetodoEstatico.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                    assercao.IsTrue(expressao_propriedades.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas), code_propriedadeAninhadas);
                    assercao.IsTrue(AssertFuncoes(expressao_chamadaDeMetodo), code_chamadaDeMetodo);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

            }

            public void TestesChamadasDeMetodosEstaticas(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                char aspas = ExpressaoLiteralText.aspas;
                string code_createText = "string text=" + aspas + "Amazonia" + aspas + ";";
                string code_createVector = "Vector separadores_1[20];";

                ProcessadorDeID compilador = new ProcessadorDeID(code_createText + code_createVector);
                compilador.Compilar();

                //compilador.escopo.tabela.GetObjeto("separadores_1", compilador.escopo).isWrapperObject = true;

                string codigo_0_0 = "string.CuttWords(text, separadores_1);";
                Expressao expss_0_0 = new Expressao(codigo_0_0, compilador.escopo);

                try
                {
                    assercao.IsTrue(expss_0_0.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo_0_0);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }

            public void TesteExpressaoPropriedadesAninhadas(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                //preparacao para testes unitarios em massa.
                string codigoClasseA = "public class classeF { public int propriedadeA;  public classeF() { };  public int metodoA(int a, int b ){ int x =1; x=x+1;}; }";
                string codigoClasseB = "public class classeG {public classeF propriedadeB;  public classeG(){ }; }";




                List<string> tokensClasseA = new Tokens(codigoClasseA).GetTokens();
                List<string> tokensClasseB = new Tokens(codigoClasseB).GetTokens();


                // testes unitarios em massa.
                string codigoObjectsCreate = "classeG objetoB= create(); classeG objetoB2= create(); classeF objetoA= create();";
                string codigo0_1 = " objetoB.propriedadeB;";
                string codigo0_2 = " objetoB2.propriedadeB.propriedadeA;";
                string codigo0_3 = "objetoA.propriedadeA=1;";
                string codigo0_5 = "objetoA.propriedadeA + objetoA.propriedadeA;";
                string codigo0_6 = "objetoB.propriedadeB.metodoA( 1, 1);";

                string codigoTotal = codigoClasseA + codigoClasseB + codigoObjectsCreate;

                Escopo escopo = new Escopo(codigoTotal);
                ProcessadorDeID compilador = new ProcessadorDeID(codigoTotal);
                compilador.Compilar();


                Expressao expressoes_1 = new Expressao(codigo0_1, compilador.escopo);
                Expressao expressoes_3 = new Expressao(codigo0_3, compilador.escopo);
                Expressao expressoes_6 = new Expressao(codigo0_6, compilador.escopo);
                Expressao expressoes_5 = new Expressao(codigo0_5, compilador.escopo);
                Expressao expressoes_2 = new Expressao(codigo0_2, compilador.escopo);


                try
                {
                    assercao.IsTrue(expressoes_1.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas), codigo0_1);
                    assercao.IsTrue(expressoes_2.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas), codigo0_2);
                    assercao.IsTrue(expressoes_3.Elementos[0].GetType() == typeof(ExpressaoAtribuicao), codigo0_3);
                    assercao.IsTrue(expressoes_5.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas), codigo0_5);
                    assercao.IsTrue(expressoes_6.Elementos[0].Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo0_6);

                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasse("classeF").GetPropriedade("propriedadeA") != null &&
                                    RepositorioDeClassesOO.Instance().GetClasse("classeG").GetPropriedade("propriedadeB") != null, "instanciacao de classes codigo feito.");
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "FATAL ERROR em validar os resultados." + e.Message);
                }

            }

            public void TesteParametrosMultiArgumentos(AssercaoSuiteClasse assercao)
            {


                SystemInit.InitSystem();

                string codigoClasse = "public class classeT { public int propriedadeA;  public classeT(){ int x=1; } ;public int metodoT(int x, ! Vector y){ int x=2;} ;};";
                string codigoCreate = "classeT objA= create(); double x=1;";
                string codigoChamadaDeMetodo = "objA.metodoT(x,1,1,1);";

                Escopo escopo = new Escopo(codigoClasse + codigoCreate);
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasse + codigoCreate);
                compilador.Compilar();

                Expressao exprss = new Expressao(codigoChamadaDeMetodo, compilador.escopo);

                try
                {
                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasse("classeT") != null &&
                                    compilador.escopo.tabela.GetObjeto("objA", compilador.escopo) != null &&
                                    RepositorioDeClassesOO.Instance().GetClasse("classeT").GetMetodo("metodoT")[0].parametrosDaFuncao[1].isMultArgument == true, codigoClasse);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "falha no teste: " + ex.Message);
                }
            }

            public void TesteExpressaoChamadasDeMetodo(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();


                //preparacao para testes unitarios em massa.
                string codigoClasseA = "public class classeA { public int propriedadeA;  public classeA() { int b=2; };  public int metodoA(int a, int b ){ int x =1; x=x+1;}; };";
                string codigoClasseB = "public class classeB { public int propriedadeB;  public classeB() { int c=2; }; };";





                // testes unitarios em massa.
                string codigoCreate = "classeB objetoB= create(); classeB objetoB2= create(); classeA objetoA= create(); int x=5; int y=1;";
                string codigo0_1 = "objetoA.metodoA(x,1);";
                string codigo0_2 = "objetoA.metodoA(1,5);";
                string codigo0_3 = "objetoA.metodoA((x+1),y+1);";


                string codigoTotal = codigoClasseA + codigoClasseB + codigoCreate;

                Escopo escopo = new Escopo(codigoTotal);
                ProcessadorDeID compilador = new ProcessadorDeID(codigoTotal);
                compilador.Compilar();

                compilador.escopo.tabela.RegistraObjeto(new Objeto("private", "int", "x", "1"));
                compilador.escopo.tabela.RegistraObjeto(new Objeto("private", "int", "y", "5"));




                Expressao expressoes_3 = new Expressao(codigo0_3, compilador.escopo);
                Expressao expressoes_2 = new Expressao(codigo0_2, compilador.escopo);
                Expressao expressoes_1 = new Expressao(codigo0_1, compilador.escopo);




                assercao.IsTrue(AssertFuncoes(expressoes_1), codigo0_1);
                assercao.IsTrue(AssertFuncoes(expressoes_2), codigo0_2);
                assercao.IsTrue(AssertFuncoes(expressoes_3), codigo0_3);

            }



            public void TesteAvaliacaoAtribuicaoPropriedadesAninhadas(AssercaoSuiteClasse assercao)
            {

                SystemInit.InitSystem();

                string code_classe_0_1 = "public class classeY { public int propriedade1; public classeY(){}};";
                string code_create_obj = "classeY obj1= create();";
                string code_expression_0_1 = "obj1.propriedade1= 5;";
                string code_expression_0_2 = "obj1.propriedade1= -1;";
                string code_expression_0_3 = "obj1.propriedade1= obj1.propriedade1+1;";

                ProcessadorDeID compilador = new ProcessadorDeID(code_classe_0_1 + code_create_obj);
                compilador.Compilar();

                Expressao exprssProp_03 = new Expressao(code_expression_0_3, compilador.escopo);
                Expressao exprssProp_02 = new Expressao(code_expression_0_2, compilador.escopo);
                Expressao exprssProp_01 = new Expressao(code_expression_0_1, compilador.escopo);


                try
                {
                    assercao.IsTrue(exprssProp_03.Elementos[0].GetType() == typeof(ExpressaoAtribuicao));
                    assercao.IsTrue(exprssProp_02.Elementos[0].GetType() == typeof(ExpressaoAtribuicao));
                    assercao.IsTrue(exprssProp_01.Elementos[0].GetType() == typeof(ExpressaoAtribuicao));

                }
                catch (Exception ex)
                {
                    string errorCode = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }

            }

   
 
           

     
            private bool AssertFuncoes(Expressao expressoes_1)
            {
                string codigoErro = "";
                try
                {
                    return expressoes_1!=null &&  expressoes_1.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo);
                }
                catch (Exception e)
                {
                    codigoErro = e.Message;
                    return false;
                }

            }

    
            public void TesteListaExpressoes(AssercaoSuiteClasse assercao)
            {
              
                SystemInit.InitSystem();
                // codigo de 3 sub-expressoes.
                string codigo_0_1 = "y= x+(1+y)*3, x+1";
                string codigo0_2 = "(x+3)*1, x*y,1+y";

                List<string> tokens_0_1 = new Tokens(codigo_0_1).GetTokens();
                List<string> tokens_0_2 = new Tokens(codigo0_2).GetTokens();


                Objeto obj1 = new Objeto("private", "int", "x", 1);
                Objeto obj2 = new Objeto("private", "int", "y", 5);
                Escopo escopo = new Escopo(codigo_0_1);
                escopo.tabela.RegistraObjeto(obj1);
                escopo.tabela.RegistraObjeto(obj2);


                List<Expressao> exprss_1 = Expressao.ExtraiExpressoes(tokens_0_1, escopo);
                List<Expressao> exprss_2 = Expressao.ExtraiExpressoes(tokens_0_2, escopo);


                try
                {
                    assercao.IsTrue(true, "extracao de expressoes feito sem erros fatais.");
                    assercao.IsTrue(exprss_1.Count == 2, codigo_0_1);
                    assercao.IsTrue(exprss_2.Count == 3, codigo0_2);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }


            }





            public void TesteExpressaoCondicional(AssercaoSuiteClasse assercao)
            {
                
                SystemInit.InitSystem();

                Escopo escopo = new Escopo("int x=0");
                escopo.tabela.RegistraObjeto(new Objeto("private", "int", "x", 1));
                escopo.tabela.RegistraObjeto(new Objeto("private", "int", "n", 5));

                string exprss_0_0 = "x<n";

                Expressao exprssCondicional = new Expressao(exprss_0_0, escopo);
                try
                {
                    assercao.IsTrue(exprssCondicional.Elementos[1].GetType() == typeof(ExpressaoOperador), exprss_0_0);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE NAO PASSOU: " + ex.Message);
                }

            }

            public void TestesExpressoesOperadores(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;
                SystemInit.InitSystem();

                string code_0_0 = "-2+x*y";


                Escopo escopo = new Escopo(code_0_0);
                escopo.tabela.RegistraObjeto(new Objeto("private", "int", "x", 1));
                escopo.tabela.RegistraObjeto(new Objeto("private", "int", "y", 5));
                Expressao exprssOperador = new Expressao(code_0_0, escopo);
                try
                {
                    assercao.IsTrue(exprssOperador.Elementos.Count > 0, code_0_0);
                    assercao.IsTrue(exprssOperador.Elementos[2].GetType() == typeof(ExpressaoOperador), code_0_0);
                    assercao.IsTrue(exprssOperador.Elementos[4].GetType() == typeof(ExpressaoOperador), code_0_0);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                }
            }




            public void TesteMetodosEstaticos(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                //Expressao.headers = null;
                List<Classe> classes = LinguagemOrquidea.Instance().GetClasses();

                string codigo_0_0 = "double.abs(1)";
                string codgio_0_1 = "double.power(2,2)";
                Escopo escopo = new Escopo(codigo_0_0);

                Expressao exprss_0_0 = new Expressao(codigo_0_0, escopo);
                Expressao exprrs_0_1 = new Expressao(codgio_0_1, escopo);
                try
                {
                    assercao.IsTrue(exprss_0_0.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo_0_0);
                    assercao.IsTrue(exprrs_0_1.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codgio_0_1);
                }
                catch
                {
                    assercao.IsTrue(false, "falha no teste, expressao: " + codigo_0_0);
                }

            }




            public void TesteExpressaoEntreParenteses(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;
                SystemInit.InitSystem();
                string codigo_0_0 = "y=x+1";
                string codigo_0_1 = "y= x+(1+y)*3;";
                string codigo_0_2 = "(x+1)*5;";
                string codigo_0_3 = "x=(x+2)-5;";
                Objeto obj1 = new Objeto("private", "int", "x", 1);
                Objeto obj2 = new Objeto("private", "int", "y", 5);
                Escopo escopo = new Escopo(codigo_0_1 + codigo_0_2 + codigo_0_3);
                escopo.tabela.RegistraObjeto(obj1);
                escopo.tabela.RegistraObjeto(obj2);

                Expressao expressoes_0_1 = new Expressao(codigo_0_1, escopo);
                Expressao expressoes_0_2 = new Expressao(codigo_0_2, escopo);
                Expressao expressoes_0_3 = new Expressao(codigo_0_3, escopo);
                Expressao expressao_0_0 = new Expressao(codigo_0_0, escopo);

                try
                {
                    assercao.IsTrue(expressoes_0_1.Elementos[0].GetType() == typeof(ExpressaoAtribuicao), codigo_0_1);
                    assercao.IsTrue(expressao_0_0.Elementos[0].GetType() == typeof(ExpressaoAtribuicao), codigo_0_0);
                    assercao.IsTrue(expressoes_0_2.Elementos.Count == 3, codigo_0_2);
                    assercao.IsTrue(expressoes_0_3.Elementos[0].GetType() == typeof(ExpressaoAtribuicao), codigo_0_3);

                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "FATAL ERROR em validacao dos resultados" + e.Message);
                }




            }



  
   
            public void TesteExpressaoAtribuicao(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();


                string codigo0_1 = "y=x+1";
                string codigo0_2 = "y=-1";
                string codigo0_3 = "y=x+1*y";
                string codigo0_4 = "y=1";
                string codigo0_5 = "y=x*1";
                string codigo0_6 = "y=++x";
                string codigo0_7 = "y=x++";
                Objeto obj1 = new Objeto("private", "int", "x", 1);
                Objeto obj2 = new Objeto("private", "int", "y", 5);
                Escopo escopo = new Escopo(codigo0_5);
                escopo.tabela.RegistraObjeto(obj1);
                escopo.tabela.RegistraObjeto(obj2);

                ExpressaoPorClassificacao exprssao = new ExpressaoPorClassificacao();

                try
                {
                    
                    Expressao exprssResultCodigo7 = new Expressao(codigo0_7, escopo);
                    Expressao exprssResultCodigo6 = new Expressao(codigo0_6, escopo);


                    Expressao exprssResultCodigo1 = new Expressao(codigo0_1, escopo);
                    Expressao exprssResultCodigo2 = new Expressao(codigo0_2, escopo);
                    Expressao exprssResultCodigo3 = new Expressao(codigo0_3, escopo);
                    Expressao exprssResultCodigo4 = new Expressao(codigo0_4, escopo);
                    Expressao exprssResultCodigo5 = new Expressao(codigo0_5, escopo);


                    assercao.IsTrue(true, "teste feito sem erros fatais.");
                    assercao.IsTrue(AssertAtribuicao(exprssResultCodigo1, 3), codigo0_1);
                    assercao.IsTrue(AssertAtribuicao(exprssResultCodigo2, 2), codigo0_2);
                    assercao.IsTrue(AssertAtribuicao(exprssResultCodigo3, 5), codigo0_3);
                    assercao.IsTrue(AssertAtribuicao(exprssResultCodigo4, 1), codigo0_4);
                    assercao.IsTrue(AssertAtribuicao(exprssResultCodigo5, 3), codigo0_5);
                    assercao.IsTrue(AssertAtribuicao(exprssResultCodigo6, 2), codigo0_6);
                    assercao.IsTrue(AssertAtribuicao(exprssResultCodigo7, 2), codigo0_7);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                }




            }

            private bool AssertAtribuicao(Expressao exprssResult, int qtdElementosExpressaoAtribuicao)
            {

                return (exprssResult.Elementos[0].GetType() == typeof(ExpressaoAtribuicao) &&
                       ((ExpressaoAtribuicao)exprssResult.Elementos[0]).ATRIBUICAO != null);

            }

 



   
         
  


            public void TesteOperadorUnarioEBinarioMasFuncionandoComoBinario(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();



                string code_binary = "x=x+1;";
                string code_unary = "x= -1;";

                Escopo escopo = new Escopo(code_unary + code_binary); ;
                escopo.tabela.RegistraObjeto(new Objeto("private", "int", "x", 1));

                Expressao exprss_unary = new Expressao(code_unary, escopo);
                Expressao exprss_binary = new Expressao(code_binary, escopo);



                assercao.IsTrue(true, "execucao do teste feito sem erros fatais.");
                try
                {
                    assercao.IsTrue(exprss_binary.Elementos[0].GetType() == typeof(ExpressaoAtribuicao));
                    assercao.IsTrue(exprss_unary.Elementos[0].GetType() == typeof(ExpressaoAtribuicao));
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "falha na validacao do teste" + e.Message);
                }
            }

  

            public void TesteExpressaoCondicional2(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string code = "int x=2; int y=5;";
                ProcessadorDeID compilador = new ProcessadorDeID(code);
                compilador.Compilar();

                Expressao exprssCondicional = new Expressao("x>y", compilador.escopo);

                try
                {
                    assercao.IsTrue(exprssCondicional.Elementos.Count == 3 && exprssCondicional.Elementos[1].GetType() == typeof(ExpressaoOperador));
                }
                catch (Exception e)
                {
                    string msgError = e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }


            }

   
 
            public void TesteExpressaoOperadorVariosOperadores(AssercaoSuiteClasse assercao)
            {

                SystemInit.InitSystem();


                string codigoClasseX = "public class classeX { public classeX() { int y; }  public int metodoX(int x, int y) { int x; }; };";
                string codigoClasseE = "public class classeE { public classeE() { int x=1; }  public int metodoE(){ int y= 1; }; };";



                string codigoClasseC = "public class classeC {  public int propriedadeC;  public classeC() { int x =0; } }; ";
                string codigoClasseD = "public class classeD {  public classeC propriedadeD; public classeD() { int y=0; }  };";


                string codigoObjetos1 = "classeE a_1= create(); classeX x_0= create();";
                string codigoObjetos2 = "int objA=1; int objB=4;";
                string codigoObjeto4 = "classeD d= create();";
                string codigoObjeto5 = "int x=1; int y=1; int z=5; int w=5; int b=5; int a=5; string s;";


                string nomeLiteral = "fruta vermelha";

                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseE + codigoClasseX + codigoClasseC + codigoClasseD + codigoObjetos1 + codigoObjetos2 + codigoObjeto4 + codigoObjeto5);
                compilador.Compilar();


                // testes unitarios de expressoes.
                string codigo_expressaoAritimetica = "x+y+z*w";
                string codigo_0_1_chamada = "b + a_1.metodoE();";
                string codigoOperadorUnario = "b++";
                string codigoPropriedadesAninhadas = "d.propriedadeD.propriedadeC;";
                string codigoOperador = "objA+objB;";
                string codigoAtribuicao = "a = a+b;";
                string codigoEntreParenteses = "(a+b)";
                string codigoSomaNumeros = "1+2";

                string codigoLiteral = '\u0022' + nomeLiteral + '\u0022';
                string codigoOperadorLiteral = "s=" + codigoLiteral;

                Expressao exprss_0_1_chamada = new Expressao(codigo_0_1_chamada, compilador.escopo);
                Expressao exprssPropriedadesAninhadas = new Expressao(codigoPropriedadesAninhadas, compilador.escopo);
                Expressao exprssEntreParenteses = new Expressao(codigoEntreParenteses, compilador.escopo);
                Expressao exprssAtribuicao = new Expressao(codigoAtribuicao, compilador.escopo);
                Expressao exprssOperadorSomaLiteral = new Expressao(codigoOperadorLiteral, compilador.escopo);
                Expressao exprssOpUnario = new Expressao(codigoOperadorUnario, compilador.escopo);
                Expressao exprss_Aritimetica = new Expressao(codigo_expressaoAritimetica, compilador.escopo);
                Expressao exprssOperador = new Expressao(codigoOperador, compilador.escopo);
                Expressao exprssOperadorSoma = new Expressao(codigoSomaNumeros, compilador.escopo);
                Expressao exprssLiteral = new Expressao(codigoLiteral, compilador.escopo);


                try
                {
                    assercao.IsTrue(exprss_0_1_chamada.Elementos[2].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo_0_1_chamada);
                    assercao.IsTrue(exprssPropriedadesAninhadas.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas), codigoPropriedadesAninhadas);
                    assercao.IsTrue(exprssEntreParenteses.Elementos[0].GetType() == typeof(ExpressaoEntreParenteses), codigoOperador);
                    assercao.IsTrue(exprssOperador.Elementos[0].GetType() == typeof(ExpressaoObjeto), codigoOperador);
                    assercao.IsTrue(exprss_Aritimetica.Elementos[0].GetType() == typeof(ExpressaoObjeto) && exprss_Aritimetica.Elementos[1].GetType() == typeof(ExpressaoOperador), codigo_expressaoAritimetica);
                    
                    assercao.IsTrue(exprssOpUnario.Elementos[1].GetType() == typeof(ExpressaoOperador), codigoOperadorUnario);
                    assercao.IsTrue(exprssAtribuicao.Elementos[0].GetType() == typeof(ExpressaoAtribuicao), codigoPropriedadesAninhadas);
                    assercao.IsTrue(exprssOperadorSoma.Elementos[1].GetType() == typeof(ExpressaoOperador), codigoSomaNumeros);
                    assercao.IsTrue(exprssLiteral.Elementos[0].GetType() == typeof(ExpressaoLiteralText), codigoLiteral);
                    assercao.IsTrue(exprssOperadorSomaLiteral.Elementos[0].GetType() == typeof(ExpressaoAtribuicao), codigoOperadorLiteral);

                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    assercao.IsTrue(false, "Fail tests");
                }



            }


    

            public void TesteExpressaoProcessamentoObjetoDictionaryText(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                //Expressao.headers = null;
                char aspas = '\u0022';

                string key = aspas + "fruta" + aspas;
                string value = aspas + "maca" + aspas;

                string codigoCreate = "DictionaryText m = { string }";
                string codigoGet = "m{" + key+ "}";
                string codigoSet = "m{" + key + "} = " + value;


                Escopo escopo1 = new Escopo(codigoCreate);

                DictionaryText m = new DictionaryText();
                m.tipoElemento = "string";
                m.tipo = "DictionaryText";
                m.SetNome("m");
                m.isWrapperObject = true;

                escopo1.tabela.RegistraObjeto(m);



                Expressao exprssCreate = new Expressao(codigoCreate, escopo1);
                Expressao exprssSet = new Expressao(codigoSet, escopo1);
                Expressao exprssGet = new Expressao(codigoGet, escopo1);





                try
                {

                    assercao.IsTrue(exprssCreate.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigoCreate);
                    assercao.IsTrue(exprssGet.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigoGet);
                    assercao.IsTrue(exprssSet.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigoSet);
                    assercao.IsTrue(escopo1.tabela.GetObjeto("m", escopo1).isWrapperObject);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "erro na validacao dos resultados: " + e.Message);
                }
            }


  


            public void TestCreatSeElementGetElementVector(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;
                SystemInit.InitSystem();

                string codigoAtribuicao = "x=v5[1];";
                string codigoGetElement = "v5[1];";
                string codigoSetElement = "v5[1]=5;";
                string codigoInstanciacao1 = "int[] v5 [ 6 ];";
                string codigoInstanciacao2 = "int[] v5 [ x+1 ];";


                Escopo escopo = new Escopo(codigoInstanciacao1);
               
                escopo.tabela.GetObjetos().Add(new Objeto("private", "int", "x", 1));
                //escopo.tabela.RegistraObjeto(objetoVetor);

                Expressao exprssCreate1 = new Expressao(codigoInstanciacao1, escopo);
                Expressao exprssCreate2 = new Expressao(codigoInstanciacao2, escopo);

                Expressao epxrssAtribui = new Expressao(codigoAtribuicao, escopo);
                Expressao exprssGET = new Expressao(codigoGetElement, escopo);
                Expressao exprssSET = new Expressao(codigoSetElement, escopo);

                try
                {
                    assercao.IsTrue(
                        epxrssAtribui.Elementos[0].GetType() == typeof(ExpressaoAtribuicao) &&
                        ((ExpressaoAtribuicao)epxrssAtribui.Elementos[0]).ATRIBUICAO.Elementos[0].tokens.Contains("GetElement"), codigoAtribuicao);

                    assercao.IsTrue(AssertCreate(exprssCreate2), codigoInstanciacao2);
                    assercao.IsTrue(AssertCreate(exprssCreate1), codigoInstanciacao1);
                    assercao.IsTrue(AssertGetElement(exprssGET), codigoGetElement);
                    assercao.IsTrue(AssertSetElement(exprssSET), codigoSetElement);
                }
                catch (Exception ex)
                {
                    LoggerTests.AddMessage("Fatal error, classe vector: " + ex.Message);
                }



            }






            
            private bool AssertSetElement(Expressao exprssResult)
            {
                string codeError = "";
                try
                {
                    return (exprssResult.Elementos[0].tokens.Contains("SetElement") &&
                            exprssResult.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                }
                catch (Exception e)
                {
                    codeError = e.Message;
                    return false;
                }
            }
            private bool AssertGetElement(Expressao exprssResult)
            {
                string codeError = "";
                try
                {
                    return (exprssResult.Elementos[0].tokens.Contains("GetElement") &&
                            exprssResult.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                }
                catch (Exception e)
                {
                    codeError = e.Message;
                    return false;
                }
            }
            private bool AssertCreate(Expressao exprssResult)
            {
                string codeError = "";
                try
                {
                    return (exprssResult.Elementos[0].tokens.Contains("Create") &&
                            exprssResult.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                }
                catch (Exception ex)
                {
                    codeError = ex.Message;
                    return false;
                }
            }

    

            public void TesteLiteraisEVariaveisString(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;
                SystemInit.InitSystem();

                // constroi um texto constante.
                string literal = ExpressaoLiteralText.aspas + "Oi, homero " + ExpressaoLiteralText.aspas;

                // constroi  variaveis string.
                Escopo escopo = new Escopo("int x;");
                escopo.tabela.RegistraObjeto(new Objeto("private", "string", "nome", "homero"));
                escopo.tabela.RegistraObjeto(new Objeto("private", "string", "result", null));

                try
                {
                    Expressao exprss = new Expressao(literal+ " +  nome" , escopo);
                }
                catch (Exception e)
                {
                    string errorCode = e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }

                

            }

   
 

    
 

  

            public void TesteExpressaoProcessamentoObjetoJaggedArray(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;
                SystemInit.InitSystem();

                // "JaggedArray id = [ exprss ] [ ];
                string codeCreate = "JaggedArray m=[20][];";
                string codigoGet = "m[1][2]";
                string codigoSet = "m[1][1]=5";
                Escopo escopo1 = new Escopo(codeCreate);

                Expressao exprssCreate = new Expressao(codeCreate, escopo1);
                Expressao exprssGET = new Expressao(codigoGet, escopo1);
                Expressao exprssSET = new Expressao(codigoSet, escopo1);

                try
                {

                    assercao.IsTrue(exprssGET.Elementos.Count == 1, "m[1][2]");
                    assercao.IsTrue(exprssSET.Elementos.Count == 1, "m[1][1]=5");
                    assercao.IsTrue(escopo1.tabela.GetObjeto("m", escopo1).isWrapperObject, "JaggedArray m=[20][]"); ;
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "erro na validacao de resultados." + e.Message);
                }

            }


   
        }
    }
} 
