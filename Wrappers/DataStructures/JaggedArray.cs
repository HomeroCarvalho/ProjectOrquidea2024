using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using parser;

namespace parser
{
    /// <summary>
    /// vetor multi-dimensional para valores genericos (classe object).
    /// </summary>
    public class JaggedArray: Objeto
    {

        

        // array do JaggedArray.        
        private List<List<object>> array;

        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="sizeInRows">tamanhho inicial do jagged array.</param>
        public JaggedArray(int sizeInRows)
        {
            array = new List<List<object>>();

            // inicia o jagged array com [sizeInRows] linhas.
            for (int i = 0; i < sizeInRows; i++)
                array.Add(new List<object>());

            this.tipo = "JaggedArray";
            this.isWrapperObject = true;
            this.tipoElemento = "object";
        }


        /// <summary>
        /// construtor vazio.
        /// </summary>
        public JaggedArray()
        {
            array = new List<List<object>>();
            for (int i = 0; i < 10; i++)
            {
                array.Add(new List<object>());
            }
            this.tipo = "JaggedArray";
            this.isWrapperObject = true;
            this.tipoElemento = "object";
        }


        /// <summary>
        /// seta o tipo de elemento do jagged.
        /// </summary>
        /// <param name="tipoElemento">novo tipo dos elementos do jagged.</param>
        public void SetTipoElemento(string tipoElemento)
        {
            this.tipoElemento= tipoElemento;
        }


        /// <summary>
        /// faz o casting entre o objeto parametro, para um jagged array.
        /// </summary>
        /// <param name="objToCopy">object para conversao.</param>
        public void Casting(object objToCopy)
        {
            if (objToCopy != null)
            {
                JaggedArray j1 = (JaggedArray)objToCopy;
                if (this.array == null)
                {
                    this.array = new List<List<object>>();
                }
                else
                {
                    this.array.Clear();
                }

                for (int x=0; x< j1.array.Count; x++)
                {

                    this.array.Add(new List<Object>());

                    for (int y = 0; y < j1.array[x].Count; y++)
                    {
                        this.array[x].Add(j1.array[x][y]); 
                    }
                }
            }
            else
            {
                this.array = null;
            }
        }

   
        /// <summary>
        /// faz uma copia do objetoParametro.
        /// </summary>
        /// <param name="objAClonar">jagged array a clonar.</param>
        /// <returns></returns>
        public static JaggedArray Clone(Objeto objAClonar)
        {
            JaggedArray jClonar = (JaggedArray)objAClonar;
            JaggedArray j = new JaggedArray();
            if (jClonar.array != null)
            {
                j.array = new List<List<object>>();
                for (int i = 0; i < jClonar.array.Count; i++)
                {
                    j.array.Add(jClonar.array[i]);
                }
            }
            j.nome = jClonar.nome;
            j.tipoElemento = jClonar.tipoElemento;
            j.tipo = jClonar.tipo;
            j.isWrapperObject = true;
            j.isReadOnly = jClonar.isReadOnly;
            j.valor = jClonar.valor;
            return j;
        }

        /// <summary>
        /// funcao de conversao dos dados em uma string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "";
            if ((array != null) && (array.Count > 0))
            {
                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i] != null)
                    {
                        for (int j=0;j< array[i].Count; j++)
                        {
                            str += array[i][j].ToString()+",";
                        }
                    }
                }
                str = str.Remove(str.Length - 1);
            }
            return str;
        }


        /// <summary>
        /// insere uma linha (sem suporte para avaliacao alem de 2 dimensoes).
        /// </summary>
        /// <param name="arrayToInsert">jagged array parametro.</param>
        /// <param name="rowToInsert">tamanho do array linha.</param>
        public void InsertRow(JaggedArray arrayToInsert, int rowToInsert)
        {
            array[array.Count - 1].Add(arrayToInsert.array[rowToInsert]);
        }

        /// <summary>
        /// insere uma coluna (sem suporte para avaliação alem de 2 dimensoes).
        /// </summary>
        /// <param name="arrayToInsert">jagged array parametro.</param>
        /// <param name="colFrom">inicio do intervalo.</param>
        /// <param name="colTo">tamanho da coluna inserida.</param>
        public void InsertColumn(JaggedArray arrayToInsert, int colFrom, int colTo)
        {
            array.Insert(colTo, arrayToInsert.array[colFrom]);
        }


        /// <summary>
        /// redimensiona o jagged array, na linha parametro, e nova dimensao parametro.
        /// </summary>
        /// <param name="indexRow">indice da linha.</param>
        /// <param name="sizeNew">novo tamanho do jagged array.</param>
        public void ReSize(int indexRow, int sizeNew)
        {
            int sizeOld = array[indexRow].Count;

            int sizeToChange = sizeNew - sizeOld;
            for (int x = 0; x < sizeToChange; x++)
                array[indexRow].Add(0);

        }

        /// <summary>
        /// obtem o tamanho da linha do jagged array.
        /// </summary>
        /// <param name="indexRow">linha parametro.</param>
        /// <returns></returns>
        public int GetSize(int indexRow)
        {
            if (indexRow < array.Count)
                return array[indexRow].Count;
            else
                return 0;
        }

        /// <summary>
        /// retorna o tipo dos elementos constituinte.
        /// </summary>
        /// <returns></returns>
        public new string GetTipoElement()
        {
            return "Object";
        }

        /// <summary>
        /// adiciona um elemento.
        /// </summary>
        /// <param name="indexRow">indice da linha.</param>
        /// <param name="valor">elemento parametro.</param>
        public void AddElement(int indexRow, object valor)
        {
            array[indexRow].Add(valor);                
        }


        /// <summary>
        /// Seta o elemento [index1][index2].
        /// </summary>
        /// <param name="index1">indice.</param>
        /// <param name="index2">indice.</param>
        /// <param name="novoValorElemento">valor do elemento.</param>
        public void SetElement(int index1, int index2, object novoValorElemento)
        {
            if (this.valor != null)
            {
                JaggedArray jAtualizado=(JaggedArray)this.valor;
                jAtualizado.array[index1][index2] = novoValorElemento;
                array[index1][index2] = novoValorElemento;
            }
            else
            {
                array[index1][index2] = novoValorElemento;
            }
            
        }


        /// <summary>
        /// obtem o elemento a [index1][index2].
        /// </summary>
        /// <param name="index1">indice.</param>
        /// <param name="index2">indice</param>
        /// <returns>retorna o elemento nos indices parametros.</returns>
        public object GetElement(int index1, int index2)
        {
            if (this.valor != null)
            {
                JaggedArray jAtualizado=(JaggedArray)this.valor;
                return jAtualizado.array[index1][index2];
            }
            else
            {
                return array[index1][index2];
            }
            
        }



        /// <summary>
        ///  cria um objeto jagged array, a partir de uma chamada de metodo.
        /// </summary>
        /// <param name="obj">objeto currente.</param>
        /// <param name="size">dimensao inicial do objeto.</param>
        public void Create(int size)
        {
            this.array = new List<List<object>>();
            for (int x = 0; x < size; x++)
            {
                this.array.Add(new List<object>());
            }
            this.isWrapperObject = true;
        }



        /// <summary>
        ///  imprime no terminal, os elementos do jagged array.
        /// </summary>
        /// <param name="message">caption info da operacao de escrita.</param>
        public void Print(string message)
        {
            System.Console.WriteLine(message);  

            for (int row=0;row<array.Count;row++)
            {
                if (array[row] != null)
                {
                    for (int col = 0; col < array[row].Count; col++)
                    {
                        if (array[row][col] != null)
                        {
                            System.Console.Write(array[row][col].ToString()+" ");
                        }
                    }
                        
                }
                    
            }
        }

        /// <summary>
        /// atualiza um elemento do jagged array, com dados de um objeto [actual].
        /// </summary>
        /// <param name="wrapperObject">wrapper vector.</param>
        /// <param name="caller">objeto actual contendo dados atualizados de processamento.</param>
        /// <param name="index">indices de acesso ao elemento.</param>
        public void UpdateFromActualObject(Objeto caller, Objeto wrapperObject, int index1, int index2)
        {
            Objeto objetoAtual = caller.Clone();
            objetoAtual.nome = wrapperObject.nome + index1.ToString();


            JaggedArray jagged = (JaggedArray)wrapperObject;
            jagged.SetElement(index1, index2, objetoAtual);

        }


        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para classe wrapper structure JaggedArray")
            {
            }

            public void TestesOperacoesJaggedArray(AssercaoSuiteClasse assercao)
            {
                JaggedArray jaggedArray = new JaggedArray(1);

                jaggedArray.Print("tamanho 1a linha: " + jaggedArray.GetSize(0));
                assercao.IsTrue(jaggedArray.GetSize(0) == 0);

                Random rand = new Random();

                for (int x = 0;  x < 15; x++) 
                jaggedArray.AddElement(0, rand.Next(15));
                jaggedArray.Print("");
                assercao.IsTrue(jaggedArray.GetSize(0) == 15);


                jaggedArray.ReSize(0, 20);
                jaggedArray.Print("row 0: " + jaggedArray.GetSize(0));
                assercao.IsTrue(jaggedArray.GetSize(0) == 20);
                


            }
        }
    }
}
