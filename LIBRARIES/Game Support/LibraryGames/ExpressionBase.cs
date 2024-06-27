using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    /// <summary>
    /// classe de expressao disponibilizada como libray, e importada na instanciacao da linguagem orquidea.
    /// </summary>
    public class ExpressionBase
    {
        /// <summary>
        /// expressao wrapped.
        /// </summary>
        public Expressao expressao;

        /// <summary>
        /// codigo da expressao.
        /// </summary>
        public string codigo;

        /// <summary>
        /// valor obtido ao executar a expressao.
        /// </summary>
        public object result;
        public ExpressionBase() {

            this.expressao = new Expressao();
            this.codigo = "";
            this.result = null;
        
        }
        
        /// <summary>
        /// seta uma chamada de metodo, para a expressao base.
        /// </summary>
        /// <param name="exprss"></param>
        public void SetExpresion(ExpressaoChamadaDeMetodo exprss)
        {
            this.expressao = exprss;
        }

        
        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="codigo">codigo de expressao.</param>
        public ExpressionBase(string codigo)
        {
            this.expressao = new Expressao(codigo, Escopo.escopoCURRENT);
            this.result = null;
            this.codigo= codigo;
        }

        /// <summary>
        /// faz a avaliacao da expressao.
        /// </summary>
        /// <returns>retorna o valor da expressao avaliada.</returns>
        public object Run()
        {
            this.result = EvalExpression.UM_EVAL.EvalPosOrdem(this.expressao, Escopo.escopoCURRENT);
            return this.result;
        }

        /// <summary>
        /// compila a expressao.
        /// </summary>
        /// <param name="codigo">codigo contendo os tokens da expressao.</param>
        public void Compile(string codigo)
        {
            this.codigo = codigo;
            this.expressao= new Expressao(this.codigo, Escopo.escopoCURRENT);    
            
        }


        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes unitarios classe ExpressionBase")
            {
            }

            public void TesteCompilacao(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string codigo = "x=1";
                Objeto x = new Objeto("private", "int", "x", "5");
                Escopo.escopoCURRENT.tabela.RegistraObjeto(x);

                ExpressionBase exprssToCompile = new ExpressionBase();
                exprssToCompile.Compile(codigo);

                try
                {
                    assercao.IsTrue(exprssToCompile.expressao.Elementos[0].GetType() == typeof(ExpressaoAtribuicao), codigo);                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }

            public void TesteAvailiacao(AssercaoSuiteClasse assercao)
            {

                SystemInit.InitSystem();
                string codigo = "x=x+1";
                Objeto x = new Objeto("private", "int", "x", "6");
                Escopo.escopoCURRENT.tabela.RegistraObjeto(x);

                ExpressionBase exprssToCompile = new ExpressionBase();


                // transforma o codigo em texto em uma expressao.
                exprssToCompile.Compile(codigo);
                // avalia a expressao.
                exprssToCompile.Run();


                try
                {
                    assercao.IsTrue(Escopo.escopoCURRENT.tabela.GetObjeto("x", Escopo.escopoCURRENT).valor.ToString() == 7.ToString());
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
            }

        }
    }
}
