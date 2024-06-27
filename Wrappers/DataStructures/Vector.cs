using parser;
using parser.ProgramacaoOrentadaAObjetos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;


namespace parser
{


    /// <summary>
    /// classe de [Array] do orquidea. pode funcionar como Lista,
    /// se aplicarmos vector.Clear(). e para adicionar elementos: vetorr.Append(elemento);
    /// </summary>
    public class Vector : Objeto
    {

        /// <summary>
        /// array contendo os elementos do vetor.
        /// </summary>
        public List<object> VectorObjects;

        /// <summary>
        /// tamanho do array.
        /// </summary>
        public int _size = 0;


        
        /// <summary>
        /// getter do tamanho do array.
        /// </summary>
        public int Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
            
            }
        }

        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="size">tamanho do vector.</param>
        public Vector(int size)
        {

            this.VectorObjects = new List<object>();
            for (int i = 0; i < size; i++)
            {
                this.VectorObjects.Add(new object());
            }

            this.SetTipoElement("Object");
            this.tipo = "Vector";
            this.isWrapperObject = true;
            this._size = size;
            this.valor = this;
        }

        /// <summary>
        /// construtor vazio.
        /// </summary>
        public Vector()
        {
            this.VectorObjects = new List<object>();
            for (int i = 0; i < 10; i++)
            {
                this.VectorObjects.Add(null);
            }
            this.SetTipoElement("Object");
            this.Size = 10;
            this.tipo = "Vector";
            this.isWrapperObject = true;
            this.valor = this;
        }

        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="tipoElemento">tipo do elemento, podendo ser objetos de classes orquidea.</param>
        public Vector(string tipoElemento)
        {
            this.VectorObjects= new List<object>();
            for (int i = 0; i < 10; i++)
            {
                this.VectorObjects.Add(null);
            }
            this.Size= 10;
            this.SetTipoElement(tipoElemento);
            this.tipo = "Vector";
            this.isWrapperObject = true;
            this.valor= this;
        }

        /// <summary>
        /// construtor
        /// </summary>
        /// <param name="tipoElemento">tipo do elemento, podendo ser objetos de classes orquidea.</param>
        /// <param name="size">tamanho do array</param>
        public Vector(string tipoElemento, int size)
        {
            this.VectorObjects = new List<object>();
            for (int i = 0; i < size; i++)
            {
                this.VectorObjects.Add(null);
            }
            this.Size = size;
            this.SetTipoElement(tipoElemento);
            this.tipo = "Vector";
            this.isWrapperObject = true;
            this.valor = this;
        }


        /// <summary>
        /// cria uma copia do objeto parametro. esse Objeto é um Vector, porque um
        /// vetor é um Objeto também.
        /// </summary>
        /// <param name="objClonar">Objeto a clonar.</param>
        /// <returns>retorna uma copia do vetor.</returns>
        public static Vector Clone(Objeto objClonar)
        {
            Vector vClonar = null;
            if (WrapperData.isWrapperData(objClonar.tipo) != null)
            {
                vClonar = (Vector)objClonar.valor;
                vClonar.valor = vClonar;
                return vClonar;
              
            }

            vClonar = (Vector)objClonar;
            Vector v = new Vector(objClonar.tipoElemento, vClonar.size());
            v.tipoElemento= vClonar.tipoElemento;
            v.tipo = vClonar.tipo;
            v.nome = vClonar.nome;
            v.isWrapperObject = true;
            v.valor = vClonar;
            v._size = vClonar.Size;
            v.isReadOnly = vClonar.isReadOnly;
            v.valor = vClonar.valor;
            v.VectorObjects = new List<object>();
            
            // clona os elmeentos do vector.
            if (v.VectorObjects != null)
            {
                for (int x = 0; x < vClonar.VectorObjects.Count; x++) 
                {
                    v.VectorObjects.Add(vClonar.VectorObjects[x]);
                }
            }

            // clona todas propriedades do vector.
            if (vClonar.propriedades != null)
            {
                v.propriedades = new List<Objeto>();

                for (int i = 0; i < vClonar.propriedades.Count; i++)
                {
                    v.propriedades.Add(vClonar.propriedades[i].Clone());    
                }
            }

            return v;
        }
        /// <summary>
        /// obtem o tamanho do array.
        /// </summary>
        /// <returns></returns>
        public int size()
        {
            if (this.valor == null)
            {
                this.valor = this;
            }
            Vector vtAtualizado = (Vector)this.valor;
            return vtAtualizado.VectorObjects.Count;
        }


        /// <summary>
        /// apaga todos elementos, ao redimensionar.
        /// </summary>
        /// <param name="newSize"></param>
        public void reSize(int newSize)
        {
            if (newSize > this._size)
            {
                int oldSize= this._size;
                for (int i = 0; i > newSize - oldSize; i++)
                {
                    this.VectorObjects.Add(null);
                }
                this._size = newSize;
            }
            else
            {
                this._size = newSize;

                this.VectorObjects = new List<object>();
                for (int i = 0; i < newSize; i++)
                {
                    this.VectorObjects.Add(null);
                }
            }

            this.valor = this;
        }


       
        /// <summary>
        /// insere um elemento no começo do vector.
        /// </summary>
        /// <param name="valor"></param>
        public void pushFront(object valor)
        {
            if (this.valor == null)
            {
                this.valor = this;
            }
            Vector vt = (Vector)this.valor;
            vt.VectorObjects.Insert(0, valor);
            this._size++;

            
        }

        /// <summary>
        /// obtem o primeiro do vetor, e retira-o do vetor.
        /// </summary>
        /// <returns></returns>
        public object popFront()
        {
            if (this.valor == null)
            {
                this.valor = this;
            }

            Vector vtAtualizado = (Vector)this.valor;
                      

            object elemento = vtAtualizado.VectorObjects[0];
            vtAtualizado.VectorObjects.RemoveAt(0);
            return elemento;

        }

        /// <summary>
        /// insere um elemento no final do vector.
        /// </summary>
        /// <param name="valorElemento">elemento a inserir.</param>
        public void pushBack(object valorElemento)
        {
            if (this.valor == null)
            {
                this.valor = this;
            }

            Vector vtAtualizado = (Vector)this.valor;
            vtAtualizado.VectorObjects.Add(valorElemento);
            

        }


        /// <summary>
        /// retorna o ultimo elemento setado no indice mais alto. é responsabilidade do
        /// desenvolvedor se houver index of bound exception, como todo código delega, por motivos
        /// de desenpenho se o codigo estiver correto.
        /// </summary>
        public object popBack()
        {
            if (this.valor == null)
            {
                this.valor = this;
            }


            Vector vtAtualizado = (Vector)this.valor;


            object valorElemento = vtAtualizado.VectorObjects[vtAtualizado.VectorObjects.Count - 1];
            vtAtualizado.VectorObjects.RemoveAt(vtAtualizado.VectorObjects.Count - 1);
            vtAtualizado._size--;

            this.valor = this;
            return valorElemento;


        }

        /// <summary>
        /// insere um novo elemento, no indice parametro.
        /// </summary>
        /// <param name="index">indice para a insercao.</param>
        /// <param name="novoValor">valor do elmento a inserir.</param>
        public void insert(int index, object novoValor)
        {

            if (this.valor == null)
            {
                this.valor = this;
            }

            Vector vtAtualizado = (Vector)this.valor;
            vtAtualizado.VectorObjects.Insert(index, novoValor);
            this.valor = this;

        }

        /// <summary>
        /// remove um elemento numa posicao parametro.
        /// </summary>
        /// <param name="index">indice do elemento a remover</param>
        public void remove(int index)
        {
            if (this.valor == null)
            {
                this.valor = this;
            }

            Vector vtAtualizado = (Vector)this.valor;
            if ((index >= 0) && (index < vtAtualizado._size))
            {

                vtAtualizado.VectorObjects.RemoveAt((int)index);
                vtAtualizado._size--;

            }




        }

        /// <summary>
        /// muda o valor de um elemento.
        /// </summary>
        /// <param name="index">indice do elemento.</param>
        /// <param name="novoValor">novo valor do elemento.</param>
        public void Set(int index, object novoValor)
        {
            if (this.valor == null)
            {
                this.valor = this;
            }
            
            this.VectorObjects[index] = novoValor;
            Vector vtAtualizado= (Vector)this.valor;
            vtAtualizado.VectorObjects[index] = novoValor;
            
        }

        /// <summary>
        /// esvazia o conteudo do vetor. o vetor passa a funcionar como UMA LISTA! para aumentar a lista, utilizar
        /// Append() funcao.
        /// </summary>
        public void Clear()
        {
            if (this.valor == null)
            {
                this.valor = this;
            }

            Vector vtAtualizado = (Vector)this.valor;
            if (vtAtualizado.VectorObjects == null)
            {
                vtAtualizado.VectorObjects = new List<object>();
                this.VectorObjects= new List<object>();

                vtAtualizado._size = 0;
                this._size = 0;

            }
            else
            {
                vtAtualizado.VectorObjects.Clear();
                this.VectorObjects.Clear();

                vtAtualizado._size = 0;
                this._size = 0;

            }


            this.valor = this;
        }

        /// <summary>
        /// copia os dados de um vetor.
        /// </summary>
        /// <param name="vt">vetor a ser copiado.</param>
        public void Casting(object vt)
        {
            Vector vtToCopy = (Vector)vt;
            this.VectorObjects.Clear();
            if (vtToCopy.VectorObjects != null)
            {
                this.VectorObjects.AddRange(vtToCopy.VectorObjects);
              
            }

            
        }




        /// <summary>
        /// retorna o tipo do elemento do vector.
        /// </summary>
        /// <returns></returns>
        public new string GetTipoElement()
        {

            return this.tipoElemento;
        }

        /// <summary>
        /// seta o tipo do elemento do vector.
        /// </summary>
        /// <param name="newType"></param>
        public new void SetTipoElement(string newType)
        {
            this.tipoElemento = newType;
        }

       

        /// <summary>
        /// faz o processamento de instanciacao do vector, contido numa expressao chamada de metodo.
        /// </summary>
        /// <param name="exprss">expresssao chamada de metodo da instanciacao.</param>
        /// <param name="escopo">contexto onde a expressao esta.</param>
        public void Create(int size)
        {

            this.VectorObjects = new List<object>();
            for (int i = 0; i < size; i++)
            {
                this.VectorObjects.Add(null);
            }
            this._size= size;
            this.isWrapperObject = true;

        }


        /// <summary>
        /// obtem o i-esimo elemento.
        /// </summary>
        /// <param name="index">indice do elemento.</param>
        /// <returns></returns>
        public object Get(int index)
        {
            
            Vector vtAtualizaado = (Vector)this.valor;
            if ((index>=0) && (index<vtAtualizaado.VectorObjects.Count))
            {
                return vtAtualizaado.VectorObjects[index];
            }
            else
            {
                return null;
            }
            
        
        }



        /// <summary>
        /// obtem o elemento no i-esimo [indice].
        /// </summary>
        /// <param name="indice">indice do elemento.</param>
        /// <returns></returns>
        public object GetElement(int indice)
        {
            if (this.valor == null)
            {
                this.valor = this;
            }
            Vector vAtualizado = (Vector)this.valor;
            if ((indice >= 0) && (indice < vAtualizado.VectorObjects.Count))
            {
                return vAtualizado.VectorObjects[(int)indice];
            }
            else
            {
                return null;
            }
           

        }

        /// <summary>
        /// seta um elemento, que vem de uma expressao, no indice da expressao parametro.
        /// </summary>
        /// <param name="valorElement">valor do elemento.</param>
        /// <param name="indexObject">indice dentro do vetor.</param>
        /// <exception cref="Exception"></exception>
        public void SetElement(object indexObject, object valorElement)
        {

            if (this.valor == null)
            {
                this.valor = this;
            }

            object index = null;
            if (indexObject.GetType() == typeof(Objeto))
            {
                index = (int)(((Objeto)indexObject).valor);
            }
            else
            {
                if (indexObject.GetType() == typeof(string))
                {
                    index=int.Parse(indexObject.ToString());
                }
                else
                {
                    index = (int)indexObject;
                }
                
            }

            if ((this.valor != null) && (valorElement != null)) 
            {
                if (valorElement.GetType()==typeof(Objeto)) {
                
                
                    Objeto objValor= ((Objeto)valorElement).Clone();
                    valorElement = objValor;
                }

                
                Vector vtAtualizado = (Vector)this.valor;
                vtAtualizado.VectorObjects[(int)index] = valorElement;
            
               
            }
        }



        /// <summary>
        /// adiciona um elemento no final do vetor.
        /// </summary>
        /// <param name="element">objeto a adicionar.</param>
        public void Append(object element)
        {
            if (this.valor == null)
            {
                this.valor = this;
            }
            Vector vtAtualizado= (Vector)this.valor;
            vtAtualizado.VectorObjects.Add(element);
            vtAtualizado._size++;

           
        }

        /// <summary>
        /// faz uma ordenacao do vetor, se o tipo de elemento for Objeto.
        /// utiliza o campo hash para ordenar.
        /// </summary>
        public void Sort()
        {
            CmpObjeto cmp = new CmpObjeto();
            List<Objeto> lstObjetos = new List<Objeto>();   
            if (this.tipoElemento == "Objeto")
            {
                if (this.VectorObjects != null)
                {
                    for (int x = 0; x < this.size(); x++)
                    {
                        // adiciona os objetos na lista.
                        if (this.VectorObjects[x] != null)
                        {
                            Objeto obj = (Objeto)this.VectorObjects[x];
                            lstObjetos.Add(obj);
                        }
                    }
                    
                    if (lstObjetos.Count > 0)
                    {
                        // ordena a lista de objetos.
                        lstObjetos.Sort(cmp);
                        // copia os objetos ordenados de volta para o vetor.
                        for (int x = 0; x < lstObjetos.Count; x++)
                        {
                            this.VectorObjects[x] = lstObjetos[x];
                        }
                    }   
                }
                
                

            }
        }


        /// <summary>
        /// atualiza um elemento do vector, com dados de um objeto [actual].
        /// </summary>
        /// <param name="wrapperObject">wrapper vector.</param>
        /// <param name="objetoElemento">objeto actual contendo dados atualizados de processamento.</param>
        /// <param name="index">indices de acesso ao elemento.</param>
        public void UpdateFromActualObject(Objeto objetoElemento, Vector wrapperObject, int index1, int index2)
        {

            if (objetoElemento == null)
            {
                return;
            }
            else
            {
                if ((wrapperObject!=null) && (wrapperObject.GetElement(index1)!=null) && (wrapperObject.GetElement(index1).GetType()== typeof(Objeto)))
                {
                    string nomeElemento = ((Objeto)this.GetElement(index1)).nome;
                    Objeto elementoAtualizar = objetoElemento.Clone();
                    elementoAtualizar.nome = nomeElemento;
                    wrapperObject.SetElement(index1, elementoAtualizar);


                    this.SetElement(index1, elementoAtualizar);

                }
                else
                {
                    Objeto elementoAtualizar = objetoElemento;
                    wrapperObject.SetElement(index1, elementoAtualizar);
                    this.SetElement(index1, elementoAtualizar);
                }
                
            }
            
        }


        /// <summary>
        /// comparador de Objeto, pelo campo hash
        /// </summary>
        internal class CmpObjeto : Comparer<Objeto>
        {
            public override int Compare(Objeto? x, Objeto? y)
            {
                return string.Compare(x.hashText, y.hashText);
                       
            }
        }

        /// <summary>
        /// obtem um texto contendo dados do vector.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "";
            if (this.nome != null)
            {
                str += this.nome;
            }

            if ((this.VectorObjects != null)  && (this.VectorObjects.Count<15))
            {
                for (int x = 0; x < size(); x++)
                {
                    if (VectorObjects[x] != null)
                    {
                        str += VectorObjects[x].ToString()+",";
                    }


                }
                str += "...";
                str=str.Remove(str.Length-1);
            }
            return str;
        }


        /// <summary>
        /// imprime no terminal dados do vector.
        /// </summary>
        /// <param name="message"></param>
        public void Print(string message)
        {
            System.Console.Write(message + ": [");
            if (this.VectorObjects != null)
                for (int x = 0; x < VectorObjects.Count; x++)
                {
                    if (VectorObjects[x] != null)
                    {
                        System.Console.Write(VectorObjects[x].ToString() + " ");
                    }
                        

                }

            System.Console.WriteLine("]");
        }


   
     
        public new class Testes : SuiteClasseTestes
        {
           
            public Testes() : base("testes estrutura de dados Vector")
            {
            }


            public void TesteClasseOrquideaEmVectorEIfElse(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                
                try
                {
                    string class_entity = "public class entity{ public int a; public int b; public entity(int x, int y){a=x; b=y;if (a==0){a=3;};};};";
                    string create = "entity[] v1[20]; for (int i=0;i<5;i++){entity e1= create(i,i); v1[i]=e1;};";
                    ProcessadorDeID compilador = new ProcessadorDeID(class_entity + create);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);

                    Vector vtAtualizado = (Vector)(compilador.escopo.tabela.GetObjeto("v1", compilador.escopo).valor);

                    assercao.IsTrue(vtAtualizado.GetElement(0) != null, class_entity + create);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

            }




            public void TesteClasseOrquideaCombinadoComVectors(AssercaoSuiteClasse assercao)
            {

                try
                {
                    SystemInit.InitSystem();
                    string classe_entity =
                        "public class entity { " +
                        "public entity[] entidades;  " +
                        "public int id;"+
                        "public entity(){ entidades.Clear(); id= 1;};" +
                        "public void metodoB(){entity e= create(); entidades.Append(e); entidades[0].metodoAdicional();};" +
                        "public void metodoAdicional(){id=3;};" +
                        "};";
                    string code_create = "entity e3= create(); e3.metodoB();";

                    ProcessadorDeID compilador = new ProcessadorDeID(classe_entity+ code_create);
                    compilador.Compilar();




                    Classe classeEntity = RepositorioDeClassesOO.Instance().GetClasse("entity");
                    assercao.IsTrue(classeEntity.metodos[1].instrucoesFuncao.Count == 3, classe_entity + "  " + code_create);




                    ProgramaEmVM programa = new ProgramaEmVM(compilador.instrucoes);
                    programa.Run(compilador.escopo);

                    Vector vtResult = (Vector)compilador.escopo.tabela.GetObjeto("e3", compilador.escopo).propriedades[0].valor;

                    Objeto objResult = (Objeto)vtResult.GetElement(0);

                    assercao.IsTrue(objResult.propriedades[1].valor.ToString() == "3", classe_entity + "  " + code_create);

                }
                
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                
                }
                
                
            }
            public void TesteChamadasDeFuncaoAninhados(AssercaoSuiteClasse assercao)
            {

                try
                {
                    SystemInit.InitSystem();
                    string class_entity = "public class entity{ public int a; public int b; public entity(){a=1; b=2;}; public void metodoB(){a=3;};};";
                    string create = "entity[] v1[20]; v1[0]= create(); v1[0].metodoB();";


                    ProcessadorDeID compilador = new ProcessadorDeID(class_entity + create);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.instrucoes);
                    programa.Run(compilador.escopo);

                    Vector vtResult = (Vector)compilador.escopo.tabela.GetObjeto("v1", compilador.escopo).valor;
                    Objeto objResult = (Objeto)vtResult.GetElement(0);

                    assercao.IsTrue(objResult.propriedades[0].valor.ToString() == "3" && objResult.propriedades[1].valor.ToString() == "2", class_entity + "   " + create);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }


            public void TesteAppendFuncao(AssercaoSuiteClasse assercao)
            {
                try
                {

                    SystemInit.InitSystem();
                    string code_0_0 = "double[] v[10]; v.Clear(); v.Append(2.0); v.Append(v[0]+3.0);";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                    compilador.Compilar();


                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    Vector vtResult = (Vector)compilador.escopo.tabela.GetObjeto("v", compilador.escopo).valor;

                    assercao.IsTrue(vtResult.Get(1).ToString() == "5", code_0_0);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }


            public void TesteChamadaDeMetodoComActualObjeto(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                
                    string code_class = "public class Ship{ public int[] bullets; public Ship(){int i=0; bullets.Clear(); bullets.Append(i);};}; Ship ship1= create();";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_class);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.instrucoes);
                    programa.Run(compilador.escopo);

                    Objeto objResultMain = (Objeto)compilador.escopo.tabela.GetObjeto("ship1", compilador.escopo).valor;
                    Vector objVector = (Vector)objResultMain.propriedades[0].valor;
                    object objElemento = objVector.Get(0);
                    assercao.IsTrue(objVector._size == 1 && objElemento.ToString() == "0", code_class);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }


            }


            public void TesteMalhaFor(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string code_0_0 = "int qtdElementos= 10; int[] v[qtdElementos]; for (int i=0;i<qtdElementos;i++){v[i]=i; Prompt.sWrite(Castings.ToString(v[i]));};";
                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                compilador.Compilar();

                ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                programa.Run(compilador.escopo);

                Vector vtResult = (Vector)compilador.escopo.tabela.GetObjeto("v", compilador.escopo).valor;
                assercao.IsTrue(vtResult.size() == 10);
            }

            public void TesteAppenObjetoOrquidea(AssercaoSuiteClasse assercao)
            {
                try
                {
                    SystemInit.InitSystem();
                    string code_class = "public class Ship{ public int id; public Ship(){int x=1;};}; Ship[] ships[10]; ships.Clear(); Ship nave1= create(); ships.Append(nave1);";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_class);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.instrucoes);
                    programa.Run(compilador.escopo);

                    
                    
                    
                    Vector vtResult = (Vector)compilador.escopo.tabela.GetObjeto("ships", compilador.escopo).valor;


                    assercao.IsTrue(vtResult.VectorObjects.Count == 1, code_class);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }


            }

        
     

         

            public void TesteRemoveFuncao(AssercaoSuiteClasse assercao)
            {
                try
                {


                    SystemInit.InitSystem();
                    string code_0_0 = "int[] v[10]; for (int i=0;i<v.size();i++){v[i]=i;}; v.remove(0); for (int j=0;j<v.size();j++){Prompt.sWrite(Castings.ToString(v[j]));};";
                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                    compilador.Compilar();

                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    Vector vtResult = (Vector)compilador.escopo.tabela.GetObjeto("v", compilador.escopo).valor;
                    assercao.IsTrue(vtResult.GetElement(0).ToString() == "1");

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
                
            }


          public void TesteMalhaForElementos(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string code_0_0 = "int[] v[10]; for (int i=0;i<v.size();i++){v[i]=i;};for (int j=0;j<v.size();j++){Prompt.sWrite(Castings.ToString(v[j]));};";
                ProcessadorDeID compilador = new ProcessadorDeID(code_0_0);
                compilador.Compilar();

                ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                program.Run(compilador.escopo);

             
            }


             public void TesteClasseOrquideaEmVector2(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                try
                {
                    string class_entity = "public class entity{ public int a; public int b; public entity(int x, int y){actual.a=x; actual.b=y;};};";
                    string create = "entity[] v1[20]; for (int i=0;i<5;i++){entity e1= create(i,i); v1[i]=e1;};";
                    ProcessadorDeID compilador = new ProcessadorDeID(class_entity + create);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);

                    Vector vtAtualizado = (Vector)(compilador.escopo.tabela.GetObjeto("v1", compilador.escopo).valor);

                    assercao.IsTrue(vtAtualizado.GetElement(0) != null, class_entity + create);

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

            }


            public void TestesVetores(AssercaoSuiteClasse assercao)
            {
                try
                {


                    SystemInit.InitSystem();
                    string classeUnidade = "public class unidade {public int xx; public int yy; public void unidade(int a){actual.xx= a; actual.yy=a;};};";
                    string objetosCreate = "unidade[] m1[20]; unidade t1= create(1); unidade t2= create(2); m1[0]=t1; m1[1]=t2;";


                    ProcessadorDeID compilador = new ProcessadorDeID(classeUnidade + objetosCreate);
                    compilador.Compilar();

                    ProgramaEmVM prog = new ProgramaEmVM(compilador.GetInstrucoes());
                    prog.Run(compilador.escopo);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU:  " + ex.Message);
                }
            }


  
     

            public void TesteClasseOrquideaEmVector(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();

                try
                {
                    string class_entity = "public class entity{ public int a; public int b; public entity(int x, int y){actual.a=x; actual.b=y;};};";
                    string create = "entity[] v1[20]; entity e1= create(1,1); v1[0]=e1; entity e2=create(2,2); v1[1]=e2;";

                    ProcessadorDeID compilador = new ProcessadorDeID(class_entity + create);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);

                    Vector vtResult= compilador.escopo.tabela.GetObjeto("v1", compilador.escopo) as Vector;
                    assercao.IsTrue(((Vector)vtResult.valor).GetElement(0) != null, class_entity + create);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
                
            }



            public void TesteSetElementVector(AssercaoSuiteClasse assercao)
            {
                SystemInit.InitSystem();
                string code_0_0_create = "int[] v1[20]; int x= 1;";
                string code_0_0_program = "v1[0]=0;v1[1]=1;v1[2]=2;v1[3]=3;v1[4]=4; x= v1[1];";

                try
                {
                    ProcessadorDeID compilador = new ProcessadorDeID(code_0_0_create + code_0_0_program);
                    compilador.Compilar();

                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);

                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("x", compilador.escopo).valor.ToString() == "1", code_0_0_create + " " + code_0_0_program);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHO:  " + ex.Message);
                }

            }

            public void TesteInstanciacaoVector(AssercaoSuiteClasse assercao)
            {
                Vector umVetor = new Vector("int", 10);

                umVetor.Set(0, 1);
                object valor = umVetor.Get(0);
                assercao.IsTrue(valor.ToString() == "1", "valor de set-get validacao.");


                umVetor.Print("vetor antes das operacoes");


                umVetor.pushFront(6);
                umVetor.Print("adicao na frente valor 6: elements: "+umVetor.size());
                assercao.IsTrue(umVetor.size() == 11,"operacao push front");




                umVetor.pushBack(4);
                umVetor.Print("adicao na tras valor 4 elements: "+umVetor.size());
                assercao.IsTrue(umVetor.size() == 12, "operacao push back");




                umVetor.insert(2, 3);
                umVetor.Print("inserindo um elemento na posicao 2, valor 3 elements:"+umVetor.size());
                assercao.IsTrue(umVetor.size() == 13, "operacao insert");


                object valor2= umVetor.popBack();
                umVetor.Print("valor retirado operacao pop back.elements: "+umVetor.size());
                assercao.IsTrue(umVetor.size() == 12);


                object valor3=umVetor.popFront();
                umVetor.Print("valor retirado operacao pop front elements: "+umVetor.size());
                assercao.IsTrue(umVetor.size() == 11);



              
            }
        }
    }
   
}
