using parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wrappers;

using MathNet.Numerics.LinearAlgebra;
namespace parser
{
    /// <summary>
    /// classe Matriz, elementos [double]
    /// </summary>
    public class Matriz : Objeto
    {
        /// <summary>
        /// objeto importado de bblioteca especializada em tb matrizes.
        /// </summary>
        private Matrix<double> _mtData;

        /// <summary>
        /// quantidade de linhas da matriz.
        /// </summary>
        private int lines;

        /// <summary>
        /// quantidade de colunas da matriz.
        /// </summary>
        private int colummns;

        
        /// <summary>
        /// construtor vazio.
        /// </summary>
        public Matriz()
        {
            this.lines = 5;
            this.colummns = 6;
            this._mtData = Matrix<double>.Build.Dense(lines, colummns);
            this.tipo = "Matriz";
            this.tipoElemento = "double";
            this.SetTipoElement("double");
        }


        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="lines">quantidade de linhas da matriz instanciada.</param>
        /// <param name="colummns">quantidade de colunas da matriz instanciada.</param>
        public Matriz(int lines, int colummns)
        {
            this.lines = lines;
            this.colummns = colummns;
            _mtData = Matrix<double>.Build.Dense(lines, colummns);
            this.tipo = "Matriz";
            this.tipoElemento= "double";
            this.SetTipoElement("double");
        }



        /// <summary>
        /// copia os dados de uma matriz.
        /// </summary>
        /// <param name="vt">matriz a ser copiado.</param>
        public void Casting(object vt)
        {
            Matriz vtToCopy = (Matriz)vt;
            this._mtData = vtToCopy._mtData;
            this.lines = vtToCopy.lines;
            this.colummns=vtToCopy.colummns;

            if (vtToCopy._mtData != null)
            {
                for (int lin = 0; lin < vtToCopy.lines; lin++)
                {
                    for (int col = 0; col < vtToCopy.colummns; col++)
                    {
                        this._mtData[lin,col]= vtToCopy._mtData[lin,col];
                    }
                }
                
            }

        }


        /// <summary>
        /// faz uma copia da matriz parametro.
        /// </summary>
        /// <param name="mtAClonar">matriz a clonar.</param>
        /// <returns>retorna uma copia por valor da matriz parametro.</returns>
        public static Matriz Clone(Objeto mtAClonar)
        {
            Matriz mClonar=(Matriz)mtAClonar;
            Matriz m = new Matriz(mClonar.lines, mClonar.colummns);
            m.lines = mClonar.lines;
            m.colummns = mClonar.colummns;
            for (int lin = 0; lin < mClonar.lines; lin++)
            {
                for (int col = 0; col < mClonar.colummns; col++)
                {
                    m._mtData[lin, col] = mClonar._mtData[lin, col];
                }
            }

            m.tipoElemento = mtAClonar.tipoElemento;
            m.tipo = mtAClonar.tipo;
            m.nome = mtAClonar.nome;
            m.isWrapperObject = true;
            m.isReadOnly = mtAClonar.isReadOnly;
            m.valor = mtAClonar.valor;
           
           
            return m;
        }

        /// <summary>
        /// imprime um texto contendo informe da matriz.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "[";
            if (_mtData != null)
            {
                for (int lin=0;lin<lines;lin++)
                {
                    for (int col=0;col<colummns;col++)
                    {
                        str += "[" + _mtData[lin, col].ToString("N0") + "]";
                    }
                }
            }
            str+= "]";
            return str;
        }

        /// <summary>
        /// obtem o elemento [lin,col]
        /// </summary>
        /// <param name="lin">linha da matriz.</param>
        /// <param name="col">coluna da matriz onde está o elemento.</param>
        /// <returns>retorna o valor em [lin,col].</returns>
        public object GetElement(int lin, int col)
        {
            if (this.valor != null)
            {
                Matriz matrizAtualizada=(Matriz)this.valor;
                return matrizAtualizada._mtData[lin, col];
            }
            else
            {
                return _mtData[lin, col];
            }
            
        }

        /// <summary>
        /// seta o elemento de [lin,col].
        /// </summary>
        /// <param name="lin">linha da matriz onde o elemento está.</param>
        /// <param name="col">coluna da matriz onde o elemento está.</param>
        /// <param name="valorElemento">novo valor do elemento [lin,col].</param>
        public void SetElement(int lin, int col, object valorElemento)
        {
            if (this.valor != null)
            {
                Matriz matrizAtualizada= (Matriz)this.valor;
                matrizAtualizada._mtData[lin, col] = (double)valorElemento;
                _mtData[lin, col] = double.Parse(valorElemento.ToString());
            }
            else
            {

                _mtData[lin, col] = double.Parse(valorElemento.ToString());
            }
            
        }



        /// <summary>
        /// instancia um objeto wrapper matriz, a partir de uma chamda de metodo. 
        /// </summary>
        /// <param name="mt">objeto matriz instanciado e registrado.</param>
        /// <param name="lines">numero de linhas da matriz.</param>
        /// <param name="columns">numero de colunas da matriz.</param>
        public void Create(int lines, int columns)
        {
            this._mtData = Matrix<double>.Build.Dense(lines, columns);
            this.tipo = "Matriz";
            this.tipoElemento= "double";
            this.isWrapperObject = true;

        }

        /// <summary>
        /// calcula a matriz inversa.
        /// </summary>
        public void Inverse()
        {
            _mtData = _mtData.Inverse();
        }

        /// <summary>
        /// escreve na tela o texto contendo dados da matriz.
        /// </summary>
        public void Print()
        {
            System.Console.WriteLine(_mtData.ToString());
        }

        /// <summary>
        /// atualiza um elemento da matriz, com dados de um objeto [actual].
        /// </summary>
        /// <param name="wrapperObject">wrapper matriz.</param>
        /// <param name="calerl">objeto actual contendo dados atualizados de processamento.</param>
        /// <param name="index">indices de acesso ao elemento.</param>
        public void UpdateFromActualObject(Objeto calerl, Objeto wrapperObject, int index1, int index2)
        {

            Objeto objetoAtual = calerl.Clone();
            objetoAtual.nome = wrapperObject.nome + index1.ToString();
            Matriz matriz = (Matriz)wrapperObject;
            matriz.SetElement(index1, index2, objetoAtual);

        }


        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para wrapper estructure matriz")
            {
            }

            public void testesGetSetElements(AssercaoSuiteClasse assercao)
            {
                Matriz matriz1 = new Matriz(3, 3);


                matriz1.SetElement(0, 0, 1);
                matriz1.SetElement(1, 1, 2);
                matriz1.Print();


                matriz1.SetElement(0, 0, matriz1.GetElement(1, 1));
                matriz1.Print();


                assercao.IsTrue((double)matriz1.GetElement(0, 0) == (double)2);
            }
          
        }
    }
}
