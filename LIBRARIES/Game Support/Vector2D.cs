using MathNet.Numerics.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    /// <summary>
    /// classe basica de vetores 2d. design para utilizacao na [GameLibrary]
    /// </summary>
    public class Vector2D: Objeto
    {

        public double xx2 = 0.0;
        public double yy2 = 0.0;    

      
        public Vector2D()
        {
            this.xx2 = 0.0;
            this.yy2 = 0.0;

            this.SET("xx2", "double", 0.0);
            this.SET("yy2", "double", 0.0);

            this.valor = this;

        }

        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="x">coordenada x do vector.</param>
        /// <param name="y">coordenada y do vector.</param>
        public Vector2D(double x, double y)
        {
            this.xx2 = x;
            this.yy2 = y;

            this.SET("xx2", "double", x);
            this.SET("yy2", "double", y);
            this.valor = this;

        }

        /// <summary>
        /// obtem a coordenada X do vetor.
        /// </summary>
        /// <returns></returns>
        public double getX()
        {
 
            return (double)this.GET("xx2");

        }

        



        /// <summary>
        /// retorna a coordenada Y do vetor.
        /// </summary>
        /// <returns></returns>
        public double getY()
        {
            return (double)this.GET("yy2");
        }



        /// <summary>
        /// seta a coordenada x do vetor.
        /// </summary>
        /// <param name="x">nova coordenada x.</param>
        public void setX(double x)
        {
          
            this.SET("xx2", "double", x);
        }

        /// <summary>
        /// seta a coordenada y do vetor.
        /// </summary>
        /// <param name="y"></param>
        public void setY(double y) 
        {
            this.SET("yy2", "double", y);
        }



        /// <summary>
        /// obtem o modulo do vetor.
        /// </summary>
        /// <returns></returns>
        public double Module()
        {
            return Math.Sqrt(xx2 * xx2 + yy2 * yy2);
        }

        /// <summary>
        /// calculo da magnetude do vetor.
        /// </summary>
        /// <returns></returns>
        public double Magnetude()
        {
            return Module();
        }


        /// <summary>
        /// calculo do vetor unitario (vetor direcional)
        /// </summary>
        /// <returns>retorna o vetor unitario com mgnetide=1.</returns>
        public Vector2D VectorUnitario()
        {
            double mod = Module();
            return new Vector2D(xx2 / mod, yy2 / mod);
        }


        /// <summary>
        /// seta uma nova magnetude no vetor, preservando a direção do vetor.
        /// </summary>
        /// <param name="newMagnetude">novo valor de magnetude.</param>
        public void SetMagnetude(double newMagnetude)
        {
            Vector2D v = VectorUnitario();
            v.xx2 *= newMagnetude;
            v.yy2 *= newMagnetude;

            this.SET("xx2", "double", v.xx2);
            this.SET("yy2", "double", v.yy2);
        }

        /// <summary>
        /// faz o escalonamento, a partir de um valor percentual entre 0..x
        /// </summary>
        /// <param name="scale">valor entre 0..1, podendo sr mais de 1, mas ainda em termos de percentual.</param>
        public void Scale(double scale)
        {
            double newX = (double)this.GET("xx2") * scale;
            double newY = (double)this.GET("yy2") * scale;
            this.SET("xx2", "double", newX);
            this.SET("yy2", "double", newY);
        }

        /// <summary>
        /// adiciona um vetor ao vetor que chamou esta funcao.
        /// </summary>
        /// <param name="inc">Vetor2D a adicional.</param>
        public void Translate(Vector2D inc)
        {
            double newX = (double)this.GET("xx2") + inc.xx2;
            double newY = (double)this.GET("yy2") + inc.yy2;
            this.SET("xx2", "double", newX);
            this.SET("yy2", "double", newY);
        }

        /// <summary>
        /// rotaciona o vetor.
        /// </summary>
        /// <param name="angleDegrees">angulo a rotacionar.</param>
        public void Rotate(double angleDegrees)
        {
            /// xf=r*cos(omega+alfa)
            /// yf= r*sin(omega+alfa)
            ///xf= r*(cos(omega*cos(alfa)- sin(omega)*sin(alfa))
            ///xf= xi*cos(alfa)-yi*sin(alfa)
            ///yf= r*(sin(omega)*cos(alfa)+cos(omega)*sin(alfa))
            ///yf=yi*cos(alfa)+xi*sin(alfa)
            ///
            ///xf=xi*cos(alfa)-yi*sin(alfa)
            ///yf=yi*cos(alfa)+xi*sin(alfa)
            double alfa = toRadians(angleDegrees);

            xx2 = (double)this.GET("xx2");
            yy2 = (double)this.GET("yy2");

            double xf = xx2 * Math.Cos(alfa) - yy2 * Math.Sin(alfa);
            double yf = yy2 * Math.Cos(alfa) + xx2 * Math.Sin(alfa);

            this.SET("xx2", "double", xf);
            this.SET("yy2", "double", yf);
        }

        /// <summary>
        /// adiciona o vetor parametro, para o vetor que chamou esta funcao.
        /// </summary>
        /// <param name="add">vetor a adicionar.</param>
        public void Add(Vector2D add)
        {
            xx2 = (double)this.GET("xx2");
            yy2 = (double)this.GET("yy2");

            xx2 += add.xx2;
            yy2 += add.yy2;

            this.SET("xx2", "double", xx2);
            this.SET("yy2", "double", yy2);
        }

        /// <summary>
        /// subtrai com o vetor parametro, do  vetor que chamou esta funcao.
        /// </summary>
        /// <param name="sub"></param>
        public void Subtract(Vector2D sub)
        {

            xx2 = (double)this.GET("xx2");
            yy2 = (double)this.GET("yy2");

            xx2 += sub.xx2;
            yy2 += sub.yy2;

            this.SET("xx2", "double", xx2);
            this.SET("yy2", "double", yy2);
        }

        /// <summary>
        /// calculo do produto escalar.
        /// </summary>
        /// <param name="other">o outro vetor da operaçao.</param>
        /// <returns>retorna o produto escalar entre o vetor que chamou a funcao, e o vetor parametro.</returns>
        public double Dot(Vector2D other)
        {
            xx2 = (double)this.GET("xx2");
            yy2 = (double)this.GET("yy2");

            return xx2 * other.xx2 + yy2 * other.yy2;
        }

        /// <summary>
        /// calculo de um angulo em graus para angulo em radianos.
        /// </summary>
        /// <param name="angleDegrees">angulo em graus.</param>
        /// <returns>angulo convertido para radianos.</returns>
        public double toRadians(double angleDegrees)
        {
            return (Math.PI / 180.0) * angleDegrees;
        }


        public new class Testes: SuiteClasseTestes
        {
            public Testes():base("teste com classe Vector 2D")
            {

            }

            public void TestesVector2DMousePosition(AssercaoSuiteClasse assercao)
            {
                string pathEsboco = @"esbocos testes\programaOrquideaInputMouse.DAT";
                ParserAFile.ExecuteAProgram(pathEsboco);
            }

            public void TestesIteracaoObjetos(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string code_0_0 = "Vector2D v1= create(50.0,50.0);";
                string code_iteracao = "for (int i=0;i<5;i++){ double x = v1.getX();};";

                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0 + code_iteracao);
                compilador.Compilar();

                ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                programa.Run(compilador.escopo);

                try
                {
                    Vector2D v1 = (Vector2D)(compilador.escopo.tabela.GetObjeto("v1", compilador.escopo).valor);
                    assercao.IsTrue(v1.xx2 == 50.0, code_0_0 + "   " + code_iteracao);
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("x", compilador.escopo).valor.ToString() == "50", code_iteracao);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }

            public void TestesGETTER_SETTER_Vetor2D(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_0_0 = "Vector2D v1= create(15.0,17.0); v1.setX(20.0); double a= v1.getX();";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                    compilador.Compilar();

                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("a", compilador.escopo).valor.ToString() == "20");
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:   " + ex.Message);
                }

            }
            public void TestesGETTER_Vetor2D(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_0_0 = "Vector2D v1= create(15.0,17.0); double a= v1.getX();";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                    compilador.Compilar();

                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("a", compilador.escopo).valor.ToString() == "15");
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:   " + ex.Message);
                }

            }


   

            public void TestesInstanciacao(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_0_0 = "Vector2D v1= create(50.0,50.0);";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                    compilador.Compilar();


                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    Objeto vectorCreated = compilador.escopo.tabela.GetObjeto("v1", compilador.escopo);
                    assercao.IsTrue(((Vector2D)vectorCreated.valor).xx2 == 50.0, code_0_0);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU!  " + ex.Message);
                }


            }
        }

    }
}
