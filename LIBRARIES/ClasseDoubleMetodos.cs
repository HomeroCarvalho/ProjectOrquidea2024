using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    /// <summary>
    /// classe com funcoes matematicas, para a classe double.
    /// </summary>
    public class MetodosDouble: ImportaMetodosClassesBasicas
    {
        public static int contadorNomes = 0;


        public MetodosDouble()
        {
            this.LoadMethods("double", typeof(MetodosDouble));

            


        }
        
        /// <summary>
        /// raiz quadrada da entrada.
        /// </summary>
        /// <param name="x">valor para extrair a raiz quadrada.</param>
        /// <returns></returns>
        public double root2(double x)
        {
            double y = Math.Sqrt(x);
            return y;
        }


        /// <summary>
        /// pontenciação da entrada.
        /// </summary>
        /// <param name="x">valor a potenciar.</param>
        /// <param name="expoent">expoente da operacao de potenciacao.</param>
        /// <returns></returns>
        public double power(double x, double expoent)
        {
        
            double y = Math.Pow(x, expoent);
            return y;
            
        }

        /// <summary>
        /// logaritmo da entrada.
        /// </summary>
        /// <param name="x">valor de entrada.</param>
        /// <param name="base_">base da operacao de  logaritmo.</param>
        /// <returns></returns>
        public double log(double x, double base_)
        {
            double y = Math.Log(x, base_);
            return y;
            
        }

        /// <summary>
        /// arredonda a entrada para o inteiro mais próximo possível.
        /// </summary>
        /// <param name="x">entrada a arredondar.</param>
        /// <returns></returns>
        public double round(double x)
        {
            double y = Math.Round(x);
            return y;
        }

        /// <summary>
        /// converte um ponto-flutuante em um texto string.
        /// </summary>
        /// <param name="x">numero a converter.</param>
        /// <returns></returns>
        public string toText(double x)
        {
            string str = x.ToString();
            return str;
        }

        /// <summary>
        /// retorna o valor absoluto do numero parametro.
        /// </summary>
        /// <param name="x">numero parametro da operacao.</param>
        /// <returns></returns>
        public double abs(double x)
        {
            double y = Math.Abs(x);
            return y;
        }

        /// <summary>
        /// retorna um valor, dentro dos limites [begin] a [end].
        /// </summary>
        /// <param name="x">valor parametro.</param>
        /// <param name="begin">inicio do intervalo.</param>
        /// <param name="end">final do intervalo.</param>
        /// <returns></returns>
        public double clamp(double x, double begin, double end)
        {

            if (x < begin)
                return begin;
            else
            if (x > end)
                return end;
            else
                return x;
        }

        /// <summary>
        /// retorna a arco-tangente do valor parametro.
        /// </summary>
        /// <param name="x">valor parametro.</param>
        /// <param name="dx">intervalo dx.</param>
        /// <param name="dy">intervalo dy.</param>
        /// <returns></returns>
        public double atan(double x, double dx, double dy)
        {
            double y = Math.Atan2(dy, dx);
            return y;
        }

        /// <summary>
        /// retorna o arco-seno do valor parametro.
        /// </summary>
        /// <param name="x">valor parametro.</param>
        /// <returns></returns>
        public double asin(double x)
        {
            double y = Math.Asin(x);
            return y;
        }

        /// <summary>
        /// retorna o arco-cosseno do valor parametro.
        /// </summary>
        /// <param name="x">valor parametro.</param>
        /// <returns></returns>
        public double acos(double x)
        {
            double y = Math.Acos(x);
            return y; ;
        }


        /// <summary>
        /// retorna o seno do valor parametro.
        /// </summary>
        /// <param name="x">valor parametro.</param>
        /// <returns></returns>
        public double sin(double x)
        {
            double y = Math.Sin(x);
            return y;
        }

        /// <summary>
        /// retorna o cosseno do valor parametro.
        /// </summary>
        /// <param name="x">valor parametro.</param>
        /// <returns></returns>
        public double cos(double x)
        {
            double y = Math.Cos(x);
            return y;
        }

        /// <summary>
        /// retorna a tangente do valor parametro.
        /// </summary>
        /// <param name="x">valor parametro.</param>
        /// <returns></returns>
        public double tan(double x)
        {
            double y = Math.Tan(x);
            return y;
        }

        /// <summary>
        /// retorna o seno hiperbole do valor parametro.
        /// </summary>
        /// <param name="x">valor parametro.</param>
        /// <returns></returns>
        public double sinh(double x)
        {
            double y = Math.Sinh(x);
            return y;
        }

        /// <summary>
        /// retorna o cosseno hiperbole do valor parâmetro.
        /// </summary>
        /// <param name="x">valor parametro.</param>
        /// <returns></returns>
        public double cosh(double x)
        {
                double y = Math.Cosh(x);
                return y;
        }

        /// <summary>
        /// retorna a tangente hiperbole do valor parametro,
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double tanh(double x)
        {
            double y = Math.Tanh(x);
            return y;
        }

        /// <summary>
        /// inclusão de um double x, para compatibilizar chamadas de metodo como x.E(), na anotação OO.
        /// </summary>
        /// <param name="x">valor parametro,</param>
        /// <returns></returns>
        public double E(double x)
        {
            double y = Math.E;
            return y;
        }


        /// <summary>
        /// inclusão de um double x, para compatibilizar chamadas de metodo como x.E(), na anotação OO.
        /// </summary>
        /// <param name="x">valor parametro.</param>
        /// <returns></returns>
        public double PI(double x)
        {
            double y = Math.PI;
            return y;
        }


        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para metodos da classe double")
            {
            }

            public void TesteLoadMetodosDouble(AssercaoSuiteClasse assercao)
            {
                LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();

                Classe classeDouble = linguagem.GetClasses().Find(k => k.nome == "double");
                List<Metodo> metodoList = new List<Metodo>();

                foreach (Metodo umMetodo in classeDouble.GetMetodos())
                {
                    System.Console.WriteLine("metodo: {0}", umMetodo.nome);
                }

                System.Console.ReadLine();
            }
        }
    }
}
