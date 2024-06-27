using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ObjectiveC;
using parser.ProgramacaoOrentadaAObjetos;
using ParserLinguagemOrquidea.Wrappers;
using Wrappers;


namespace parser
{




    public class Objeto
    {
        /// <summary>
        /// private, protected, ou public.
        /// </summary>
        public string acessor;

        /// <summary>
        /// classe do objeto.
        /// </summary>
        public string tipo;

        /// <summary>
        /// classe de elementos constituintes desse objeeto.
        /// </summary>
        public string tipoElemento;
        /// <summary>
        /// nome do objeto.
        /// </summary>
        public string nome;

        /// <summary>
        /// valor do objeto.
        /// </summary>
        public object valor;


        /// <summary>
        /// objeto de manipulaçao do objeto principal, dentro da classe a que o objeto principal pertence.
        /// acesso a propriedades do objeto principal.
        /// </summary>
        public Objeto actual;

        /// <summary>
        /// nome do objeto caller a qual o actual representa.
        /// </summary>
        public string nomeObjetoCaller;

       

        /// <summary>
        /// classe a qual o objeto pertence;
        /// </summary>
        public string classePertence;

        /// <summary>
        /// indice do construtor do objeto.
        /// </summary>
        public int indexConstrutor = -1;
 
        
        /// <summary>
        /// se true o objeto é também um metodo.
        /// </summary>
        public bool isMethod = false;

        /// <summary>
        /// se true o objeto é estático.
        /// </summary>
        public bool isStatic = false;

        /// <summary>
        /// se true o objeto é multi-argumento.
        /// </summary>
        public bool isMultArgument = false;

        /// <summary>
        /// se true o objeto é um wrapper data object.
        /// </summary>
        public bool isWrapperObject = false;

        /// <summary>
        /// se tre o objeto é um objeto- metodo.
        /// </summary>
        public bool isFunctionParameter = false;


        /// <summary>
        /// se true o obejto como parâmetro é passado por valor.
        /// </summary>
        public bool isReadOnly = false;


      
        /// <summary>
        /// lista de campos do objeto.
        /// </summary>
        public List<Objeto> propriedades = new List<Objeto>();

        /// <summary>
        /// se [true], instancia em tempo de compilação.
        /// </summary>
        public bool createFirst = false;


        /// <summary>
        /// valor para ordenações de listas de Objeto.
        /// </summary>
        public long hash;

        /// <summary>
        /// valor para ordenações de listas de objetos, com tipo elemento for string;
        /// </summary>
        public string hashText = "";
      
        
        /// <summary>
        /// lista de expressoes do objeto.
        /// </summary>
        private List<Expressao> expressoes = new List<Expressao>();



        /// <summary>
        /// token TRUE, utilizado em instanciacao de objetos booleanos, e em condicionais.
        /// </summary>
        public const string TRUE = "TRUE";
        /// <summary>
        /// token FALSE, utilizado em instanciacao de objetos booleanosl, e em condicionais.
        /// </summary>
        public const string FALSE = "FALSE";

    


        public List<Metodo> construtores = new List<Metodo>();
        public Objeto()
        {
            this.nome = "";
            this.tipo = "";
            this.tipoElemento = "";
            this.valor = null;
            this.propriedades = new List<Objeto>();
            this.isStatic = false;
        }


        
        public Objeto(Objeto objeto)
        {
            this.nome = objeto.nome;
            this.tipo = objeto.tipo;
            this.tipoElemento = objeto.tipoElemento;
            this.propriedades = new List<Objeto>();
            this.valor = objeto.valor;
            this.isStatic = objeto.isStatic;
            if ((objeto.propriedades != null) && (objeto.propriedades.Count > 0))
            {
                this.propriedades = objeto.propriedades.ToList<Objeto>();
            
            }

          
        }


        public Objeto(string nomeAcessor, string nomeClasse, string nomeObjeto, string nomeCampo, object valorCampo)
        {
            InitObjeto(nomeAcessor, nomeClasse, nomeObjeto, null);
            Objeto campoModificar = this.propriedades.Find(k => k.GetNome() == nomeCampo);
            campoModificar.SetValor(valorCampo); // aciona a otimização de cálculo de expressões.
            this.isStatic = false;
            this.isReadOnly = false;
        }

        
        public Objeto(string nomeAcessor, string nomeClasse, string nomeObjeto, object valor)
        {
            InitObjeto(nomeAcessor, nomeClasse, nomeObjeto, valor);
            this.isStatic = false;
            this.isReadOnly = false;
        }// Objeto()

        
        public Objeto(string nomeAcessor, string nomeClasse, string nomeObjeto, object valor, Escopo escopo, bool isStatic)
        {
            InitObjeto(nomeAcessor, nomeClasse, nomeObjeto, valor);
            this.isStatic = isStatic;
            this.isReadOnly = false;
            this.valor = this;
        }// Objeto()

        public Objeto(string nomeAcessor, string nomeClasse, string nomeOObjeto, object valor, List<Objeto> campos, Escopo escopo)
        {
            InitObjeto(nomeAcessor, nomeClasse, nomeOObjeto, valor);
            if (campos != null)
            {
                this.propriedades = campos.ToList<Objeto>();
            }
            else
            {
                this.propriedades = new List<Objeto>();
            }
            
            this.isStatic = false;
            this.isReadOnly = false;
            this.valor = this;
        }
        private void InitObjeto(string nomeAcessor, string nomeClasse, string nomeObjeto, object valor)
        {
         

            this.acessor = nomeAcessor;
            this.nome = nomeObjeto;
            this.tipo = nomeClasse;
            this.tipoElemento = this.tipo;
            this.valor = valor;

            if (WrapperData.isWrapperData(nomeObjeto) != null) 
            {
                this.isWrapperObject = true;
                this.propriedades = new List<Objeto>();
                return;
            }


            Classe classe = RepositorioDeClassesOO.Instance().GetClasse(nomeClasse);
            
            if (classe != null)
            {
                if ((classe.GetPropriedades() != null) && (classe.GetPropriedades().Count > 0))
                {
                    for (int i = 0; i < classe.propriedades.Count; i++)
                    {
                        if (WrapperData.isWrapperData(classe.propriedades[i].tipo) != null)
                        {
                            object objWrapper = WrapperData.InstantiateWrapperObject(classe.propriedades[i].tipo);
                            if (objWrapper.GetType() == typeof(Vector))
                            {
                                this.propriedades.Add((Vector)objWrapper);
                            }
                            else
                            if (objWrapper.GetType() == typeof(Matriz))
                            {
                                this.propriedades.Add((Matriz)objWrapper);
                            }
                            else
                            if (objWrapper.GetType() == typeof(JaggedArray))
                            {
                                this.propriedades.Add((JaggedArray)objWrapper);
                            }
                            else
                            if (objWrapper.GetType() == typeof(DictionaryText)) 
                            {
                                this.propriedades.Add((DictionaryText)objWrapper);
                            }

                            this.propriedades[this.propriedades.Count - 1].nome = classe.propriedades[i].nome;
                            this.propriedades[this.propriedades.Count - 1].tipoElemento = classe.propriedades[i].tipoElemento;
                            this.propriedades[this.propriedades.Count - 1].acessor = classe.propriedades[i].acessor;
                        }
                        else
                        {
                            this.propriedades.Add(classe.propriedades[i].Clone());
                        }
                        
                    }
                }
                else
                {
                    this.propriedades = new List<Objeto>();
                }

            }
         
        }


        /// <summary>
        /// obtem um objeto da classe parametro,por definição nos headers.
        /// util quando a classe nao foi compilada.
        /// </summary>
        /// <param name="nomeClasseDoObjeto">nome da classe do objeto.</param>
        /// <returns>retorna um objeto da classe parametro, junto com suas propriedades.</returns>
        public static Objeto GetObjetoByDefinitionInHeaders(string nomeClasseDoObjeto)
        {
            List<Objeto> propriedadesDoObjeto= new List<Objeto>();
            Random rnd = new Random();
            if (Expressao.headers != null)
            {
                HeaderClass header = Expressao.headers.cabecalhoDeClasses.Find(k=>k.nomeClasse==nomeClasseDoObjeto);
                if (header != null)
                {
                    List<HeaderProperty> propriedades = header.properties;
                    if (propriedades != null)
                    {
                        for (int i = 0; i < propriedades.Count; i++)
                        {
                            Objeto umaPropriedade = new Objeto("private", propriedades[i].typeOfProperty, propriedades[i].name, null);
                            propriedadesDoObjeto.Add(umaPropriedade);
                        }
                        
                        Objeto objInstanciado = new Objeto("private", nomeClasseDoObjeto, "name" + rnd.Next(100000).ToString(), null);
                        objInstanciado.propriedades.AddRange(propriedadesDoObjeto);

                        return objInstanciado;
                    }

                }
            }
            return new Objeto("private", nomeClasseDoObjeto, "name" + rnd.Next(100000).ToString(), null);
            
        }


        /// <summary>
        /// retorna [true] se o objeto é nulo, sem dados.
        /// </summary>
        /// <param name="objeto">objeto a verificar.</param>
        /// <returns></returns>
        public static bool isNILL(Objeto objeto)
        {
            return (objeto == null);
        }


        

  
    

        /// <summary>
        /// uma lista de objetos estáticos gerados na compilação.
        /// </summary>
        public static List<object> POOL_objetosEstaticos = new List<object>();

        /// <summary>
        /// uma lista de tipos de objetos estáticos quando o programa orquidea é compilado.
        /// </summary>
        public static List<Type> POOL_tiposObjetosEstaticos = new List<Type>();



        /// <summary>
        /// obtem o objeto estatico unico, evitando perda de dados quando se utilizar um metodo, propriedade estática,
        /// com um objto estático.
        /// </summary>
        /// <param name="nomeClasse">nome do objeto estático.</param>
        /// <returns></returns>
        public static object GetStaticObject(string nomeClasse)
        {
            Type type1 = Type.GetType(nomeClasse);
            if (type1 != null)
            {
                // VERIFICA SE UM OBJETO ESTÁTICO JÁ ESTÁ NO POOL DE OBJETOS.
                int index= POOL_tiposObjetosEstaticos.FindIndex(k=>k.Name== nomeClasse);
                if (index != -1)
                {
                    return POOL_objetosEstaticos[index];
                }
                else
                {
                    // SE NAO ESTIVER, REGISTRA O OBJETO ESTÁTICO.
                    object objetoEstatico = null;
                    int indexConstrutor = -1;
                    UtilTokens.FindConstructorCompatible(nomeClasse, new List<Expressao>(), ref indexConstrutor);
                    if (indexConstrutor != -1)
                    {
                        Type type = Type.GetType(nomeClasse);
                        objetoEstatico = Activator.CreateInstance(type);

                        POOL_objetosEstaticos.Add(objetoEstatico);
                        POOL_tiposObjetosEstaticos.Add(type);

                        return objetoEstatico;
                    }
                }
            }
            return null;

            
        }



        /// <summary>
        /// constroi um objeto da classe parametro, com todas propriedades da classe.
        /// </summary>
        /// <param name="nameObject">nome do objeto construido.</param>
        /// <param name="nameClass">nome da classes do objeto.</param>
        /// <returns>retorn um Objeto, contendo todos campos da classe parâmetro</returns>
        public static Objeto BuildObject(string nameObject, string nameClass)
        {

            if (RepositorioDeClassesOO.Instance().GetClasses() != null)
            {
                Classe umaClasse = RepositorioDeClassesOO.Instance().GetClasse(nameClass);
                if (umaClasse != null)
                {
                    List<Objeto> campos = umaClasse.GetPropriedades();
                    Objeto obj = new Objeto("private", nameClass, nameObject, null);

                    // obtem as propriedades da classe do objeto.
                    obj.propriedades = campos;
                    // seta o valor default para o objeto.
                    Objeto.SetValueDefault(obj);
                    return obj;
                }
            }
            return null;
        }

        /// <summary>
        /// seta valores default, para objetos com classe int, float, double, string, char.
        /// se nao for dessas classes, seta como um valor object.
        /// </summary>
        /// <param name="obj">objeto a recber o valor default.</param>
        public static void SetValueDefault(Objeto obj)
        {
            string tipoDoObjeto = obj.GetTipo();
            switch (tipoDoObjeto)
            {
                case "int":
                    obj.valor = 0;
                    break;
                case "float":
                    obj.valor = 0.0f;
                    break;
                case "double":
                    obj.valor = 0.0;
                    break;
                case "string":
                    obj.valor = "";
                    break;
                case "char":
                    obj.valor = ' ';
                    break;
                default:
                    obj.valor = null;
                    break;
            }


           
        }

        /// <summary>
        /// adiciona uma propriedade para uma classe.
        /// </summary>
        /// <param name="nameProperty">nome da propriedade.</param>
        /// <param name="typeProperty">tipo da propriedade.</param>
        /// <param name="nameClass">nome da clase a adicionar a propriedade.</param>
        /// <returns>[true] se houve a adicao.</returns>
        public bool AddProperty(string nameProperty, string typeProperty, string nameClass)
        {
            Classe classe1 = RepositorioDeClassesOO.Instance().GetClasse(nameClass);
            if (classe1 != null)
            {
                Objeto umaPropriedade = BuildObject(nameProperty, typeProperty);
                Objeto.SetValueDefault(umaPropriedade);
                classe1.propriedades.Add(umaPropriedade);
                return true;
            }
            return false;
        }


        /// <summary>
        /// remove uma propriedade de uma classe.
        /// </summary>
        /// <param name="nameProperty">nome  da propriedade a remover.</param>
        /// <param name="nameClass">nome da classe da propriedade.</param>
        /// <returns>[true] se o procedimento foi bem sucedido.</returns>
        public bool RemoveProperty(string nameProperty, string nameClass)
        {
            Classe classe1 = RepositorioDeClassesOO.Instance().GetClasse(nameClass);
            if (classe1 != null)
            {
                int indexProperty = classe1.propriedades.FindIndex(k => k.nome == nameProperty);
                if (indexProperty != -1)
                {
                    classe1.propriedades.RemoveAt(indexProperty);
                    return true;
                }
                
            }
            return false;
        }

        /// <summary>
        /// remove uma funcao para uma classe.
        /// </summary>
        /// <param name="fnc">funcao a ser removido.</param>
        /// <param name="nameClass">nome da classe a remover a funcao.</param>
        /// <returns>[true] se a operação foi bem-sucedido.</returns>
        public bool RemoveAFunction(Metodo fnc, string nameClass)
        {
            Classe classe1 = RepositorioDeClassesOO.Instance().GetClasse(nameClass);
            if (classe1 != null)
            {
                
                int index = classe1.metodos.FindIndex(k => k.GetNome() == fnc.GetNome());
                if (index != -1)
                {
                    classe1.metodos.RemoveAt(index);    
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// adiciona uma função para uma classe.
        /// </summary>
        /// <param name="fnc">metodo a ser adicionado.</param>
        /// <param name="nameClass">nome da classe a receber a funcao.</param>
        /// <returns>[true] se a operação foi bem sucedida.</returns>
        public bool AddAFunction(Metodo fnc, string nameClass)
        {
            Classe classe1 = RepositorioDeClassesOO.Instance().GetClasse(nameClass);
            if (classe1 != null)
            {
                classe1.metodos.Add(fnc);
                return true;
            }
            return false;
        }

           
        /// <summary>
        /// clona o objeto que chamou o metodo.
        /// </summary>
        /// <returns></returns>
        public Objeto Clone()
        {


            Objeto objClone=new Objeto();
            objClone.valor = this.valor;
            objClone.classePertence = this.classePertence;
            objClone.acessor = this.acessor;
            objClone.nome = this.nome;
            objClone.tipo = this.tipo;
            objClone.tipoElemento = this.tipoElemento;
            objClone.isWrapperObject= this.isWrapperObject;
            objClone.isStatic = this.isStatic;
            objClone.isMultArgument = this.isMultArgument;
            objClone.isMethod = this.isMethod;
            objClone.isReadOnly = this.isReadOnly;
            objClone.indexConstrutor= this.indexConstrutor;
            objClone.nomeObjetoCaller = this.nomeObjetoCaller;
    
            if (this.actual != null)
            {
                objClone.actual = this.actual.Clone();
            }
            


            if (this.isWrapperObject)
            {
                return WrapperData.Clone(this);
            }

            if ((this.propriedades != null) && (this.propriedades.Count > 0))
            {
                for (int i = 0; i < this.propriedades.Count; i++)
                {
                    objClone.propriedades.Add(this.propriedades[i]);
                }
            }
            return objClone;
        }

   
        /// <summary>
        /// copia para o objeto que chamou, os dados do objeto parametro.
        /// </summary>
        /// <param name="objToCopy">objeto a ser copiado.</param>
        public void CopyObjeto(Objeto objToCopy)
        {
            this.classePertence = objToCopy.classePertence;
            this.acessor=objToCopy.acessor;
            this.nome=objToCopy.nome;
            this.tipo=objToCopy.tipo;
            this.valor=objToCopy.valor;
            this.tipoElemento=objToCopy.tipoElemento;
            this.isWrapperObject = objToCopy.isWrapperObject;
            this.isStatic = objToCopy.isStatic;
            this.isMultArgument = objToCopy.isMultArgument;
            this.isMethod = objToCopy.isMethod;
            this.isReadOnly = objToCopy.isReadOnly;
            this.indexConstrutor = objToCopy.indexConstrutor;
            this.nomeObjetoCaller= objToCopy.nomeObjetoCaller;

            if (objToCopy.actual != null)
            {
                this.actual = objToCopy.actual.Clone();
            }
            
            if ((objToCopy.propriedades != null) && (objToCopy.propriedades.Count > 0))
            {
                this.propriedades = new List<Objeto>();
                for (int i = 0;i < objToCopy.propriedades.Count;i++)
                {
                    this.propriedades.Add(objToCopy.propriedades[i]);
                }
            }

        }

        /// <summary>
        /// retorna o tipo dos elementos constituinte do objeto.
        /// </summary>
        /// <returns></returns>
        public string GetTipoElement()
        {
            return this.tipoElemento; 
        }


        /// <summary>
        /// seta o tipo dos elementos constituintes do objeto.
        /// </summary>
        /// <param name="tipo"></param>
        public void SetTipoElement(string tipo)
        {
            this.tipoElemento = tipo;
        }
     
        
        /// <summary>
        /// atualiza o valor de uma propriedade. se a propriedade nao existir cria a propriedade e seta para o novo valor.
        /// </summary>
        /// <param name="nomePropriedade">nome da propriedade.</param>
        /// <param name="classePropriedade">classe da propriedade.</param>
        /// <param name="newValor">novo valor da propriedade.</param>
        public void UpdatePropriedade(string nomePropriedade, string classePropriedade, object newValor)
        {
            int index=this.propriedades.FindIndex(k=>k.nome==nomePropriedade && k.tipo==classePropriedade);
            if (index == -1)
            {
                this.SET(nomePropriedade, classePropriedade, newValor);
            }
            else
            {
                this.propriedades[index].valor= newValor;
            }
        }



        /// <summary>
        /// obtem uma propriedade do objeto.
        /// FUNCAO LEGADO, estando em versoes antigas de classe de Expressao.
        /// </summary>
        /// <param name="nome">nome da propriedade a obter.</param>
        /// <returns>retorna o objeto da propriedade.</returns>
        public Objeto GetProperty(string nome)
        {
            Objeto afield = this.propriedades.Find(k => k.GetNome() == nome);

            if (afield == null)
            {
                
                return null;

            }
            else
                return afield;
        }

        /// <summary>
        /// retorna o tipo de acessor do objeto: public, private, ou protected.
        /// </summary>
        /// <returns></returns>
        public string GetAcessor()
        {
            return this.acessor;
        }
        
        /// <summary>
        /// seta o acessor do objeto: private, protected, ou public.
        /// </summary>
        /// <param name="acessor">novo tipo do acessor.</param>
        public void SetAcessor(string acessor)
        {
            this.acessor = acessor;
        }


        /// <summary>
        /// seta o nome longo do objeto: tipo@nome.
        /// </summary>
        public void SetNomeLongo()
        {
            this.nome = this.tipo + "@" + this.GetNome();
        }
   
        /// <summary>
        /// seta o valor do objeto, com o valor parametro.
        /// </summary>
        /// <param name="newValue">novo valor do objeto.</param>
        public void SetValorObjeto(object newValue)
        {
            this.valor = newValue;
        }

        /// <summary>
        /// seta uma propriedade com um valor.
        /// FUNCAO ATUALIZADO, UTILIZACAO EM classes Orquidea, PARA SETAR PROPRIEDADES ADICIONAIS, OU MODIFICAÇÕES EM PROPRIEDADES.
        /// </summary>
        /// <param name="nomePropriedade">nome da propriedade.</param>
        /// <param name="classePropriedade">classe da propriedade.</param>
        /// <param name="valor">novo valor da propriedade.</param>
        public void SET(string nomePropriedade, string classePropriedade, object valor) 
        {
            if (this.propriedades == null)
            {
                this.propriedades= new List<Objeto>();
            }

            Objeto objProp = new Objeto("private", classePropriedade, nomePropriedade, valor);

            int indexProp = this.propriedades.FindIndex(k => nome == nomePropriedade && k.tipo == classePropriedade);
            if (indexProp == -1)
            {
                
                this.propriedades.Add(objProp);
            }
            else
            {
                this.propriedades[indexProp] = objProp;
            }
        }

        /// <summary>
        /// obtem o valor de uma propriedade.
        /// FUNCAO ATUALIZADO, UTILIZADO COMO GETTER DE UMA CLASSE ORQUIDEA (FEITO COM classe OBJETO).
        /// </summary>
        /// <param name="nomePropriedade">nome da propriedade</param>
        /// <returns>retorna o valor da propriedade, ou null se nao tiver a propriedade.</returns>
        public object GET(string nomePropriedade)
        {
            int indexProp= this.propriedades.FindIndex(k=>k.nome==nomePropriedade);
            if (indexProp != -1)
            {
                return this.propriedades[indexProp].valor;
            }
            else
            {
                return null;
            }
        }



        /// <summary>
        /// seta uma propriedade do objeto, com o valor parametro.
        /// se nao houver a propriedade, cria a propriedade e acrescenta a lista de propriedades do objeto.
        /// </summary>
        /// <param name="novoField">novo objeto da propriedade</param>
        public void SetField(Objeto novoField)
        {
            int index=this.propriedades.FindIndex(k => k.GetNome() == novoField.GetNome());
            if (index == -1)
            {
                // tenta obter as propriedades do objeeto, a partir da sua classe.
               SET(novoField.nome, novoField.tipo, novoField.valor);
               SetField(novoField);
            }
            if (index != -1)
            {
                // a propriedade já existe, seta para o valor-parametro.
                this.propriedades[index] = novoField;
                if (propriedades[index].expressoes != null)
                    for (int x = 0; x < this.propriedades[index].expressoes.Count; x++)
                        this.propriedades[index].expressoes[x].isModify = true;
            }
        }



        /// <summary>
        /// implementa a otimizacao de expressoes. Se uma expressao conter a variavel
        /// que está sendo modificada, a expressao é setada para modificacao=true.
        /// isso auxilia nos calculos de valor da expressao, que é avaliada apenas se 
        /// se alguma variavel-componente da expressao for modificada. Util para
        /// expressoes com variaveis que mudam de valor em tempo de reação humana, ou em tempo-real.
        /// </summary>
        public void SetValorField(string nome, object novoValor)
        {
            if (this.GetProperty(nome) == null)
                return;

            this.GetProperty(nome).valor = novoValor;
            int index = this.propriedades.FindIndex(k => k.tipo == nome);
            if (index != -1)
                for (int x = 0; x < this.propriedades[index].expressoes.Count; x++)
                    if (propriedades[index].expressoes[x] != null)
                        this.propriedades[index].expressoes[x].isModify = true;
    
        } // SetValor()

        /// <summary>
        /// seta o valor do objeto.
        /// </summary>
        public void SetValor(object novoValor)
        {
            this.valor = novoValor;
        } 


        /// <summary>
        /// obtem um [Objeto] de uma propriedade do objeto.
        /// </summary>
        /// <param name="classeObjeto">nome da classe do método.</param>
        /// <param name="nomeCampo">nome da propriedade.</param>
        /// <returns></returns>
        public static Objeto GetCampo(string classeObjeto, string nomeCampo)
        {
            Classe classe = RepositorioDeClassesOO.Instance().GetClasse(classeObjeto);
            if (classe == null)
                return null;
            else
            {
                Objeto objetoCampo = classe.GetPropriedades().Find(k => k.GetNome() == nomeCampo);
                if (objetoCampo == null)
                   return null;
                else
                    return new Objeto(objetoCampo);
            }
        }
     
        /// <summary>
        /// obtem o valor, conteúdo de dados que o Objeto tem. o valor é o real dado do Objeto.
        /// </summary>
        /// <returns></returns>
        public object GetValor()
        {
            return this.valor;
        }


        /// <summary>
        /// obtem o nome do objeto.
        /// </summary>
        /// <returns></returns>
        public string GetNome()
        {
            return this.nome;
        }

        /// <summary>
        /// seta o nome do objeto.
        /// </summary>
        /// <param name="nome">novo nome do objeto.</param>
        public void SetNome(string nome)
        {
            this.nome = nome;
        }
        public string GetTipo()
        {
            return this.tipo;
        }

      

        /// <summary>
        /// obtem as propriedades contidas no objeto.
        /// uso interno, porque não á List no codigo orquidea.
        /// </summary>
        /// <returns>retorna uma lista de objetos.</returns>
        public List<Objeto> GetFields()
        {

            return this.propriedades;
        }


        /// <summary>
        /// constroi um objeto com um valor padrão default.
        /// não utilize se uma classe [Objeto] genérico não tiver um construtor sem parâmetros.
        /// </summary>
        /// <param name="tipo">tipo do objeto.</param>
        /// <returns></returns>
        public static object FactoryObjetos(string tipo)
        {
            if (tipo== null) return null;
            if (tipo=="string")
            {
                return "";
            }
            else
            if (tipo=="double")
            {
                return 0.0;
            }
            else
            if (tipo=="float")
            {
                return 0.0f;
            }
            else
            if (tipo =="int")
            {
                return 0;
            }
            else
            if (tipo=="char")
            {
                return ' ';
            }
            else
            {   // OBTEM O OBJETO A PARTIR DE UM CONSTRUTOR SEM PARAMETROS!
                object resultConstructor = new object();
                object result1 = new object();
                Type classObjectCaller = Type.GetType(tipo);
                ConstructorInfo[] construtores = classObjectCaller.GetConstructors();
                if ((construtores.Length > 0) && (!(resultConstructor is Objeto)))
                {
                    // obtem um objeto da classe que construiu o operador.
                    resultConstructor = classObjectCaller.GetConstructor(new Type[] { }).Invoke(null);
                    return resultConstructor;
                }
                else
                {
                    return new object();
                }
            }
        }

        public override string ToString()
        {

            string str = "";
            if (nome != null)
            {
                str += nome;
            }
            if (tipo != null)
            {
                str = str + ":" + tipo;
            }
            return str;
                
        }


        /// <summary>
        /// obtem o valor hash do Objeto, se seu elemento for uma string;
        /// <params name="qtdChars"> quantidade de caracteres a serem usados para calcular o hash. </params>
        /// </summary>
        public void SetHashWithText(int qtdChars)
        {
            if (this.GetTipo() == "string") 
            {
                string str_completa = this.valor.ToString();
                for (int i=0;i<qtdChars - this.valor.ToString().Length;i++)
                {
                    str_completa = str_completa + "a";
                }

                this.hashText = str_completa.Substring(0, qtdChars);

            }
        }

        public class ComparerObjeto : Comparer<Objeto>
        {
            public override int Compare(Objeto? x, Objeto? y)
            {
                if (x == null || y == null) return 0;
                if (x != null || y == null) return -1;
                if (x == null || y != null) return +1;
                if (x.hash < y.hash) return -1;
                if (x.hash > y.hash) return +1;
                return 0;

            }
        }

        public class Testes:SuiteClasseTestes
        {
            public Testes() : base("testes classe Objeto")
            {

            }

            public void TesteOrdenacaoPorString(AssercaoSuiteClasse assercao)
            {
                // ordenacao de uma lista de Objetos com valor em string.
                List<string> fruits = new List<string>() { "banana", "apple", "orange", "pear" };
                List<Objeto> fruitsObjeto = new List<Objeto>();
                for (int i = 0; i < fruits.Count; i++)
                {
                    Objeto obj = new Objeto("private", "string", fruits[i], fruits[i]);
                    obj.SetHashWithText(8);
                    fruitsObjeto.Add(obj);
                }

                
                try
                {
                    Vector vetorOrd = new Vector(fruits.Count);
                    vetorOrd.tipoElemento = "Objeto";
                    vetorOrd.Clear();
                    for (int i = 0; i < fruitsObjeto.Count; i++)
                    {
                        vetorOrd.Append(fruitsObjeto[i]);
                    }
                    vetorOrd.Sort();

                    assercao.IsTrue(((Objeto)(vetorOrd.GetElement(0))).valor.ToString() == "apple");
                    assercao.IsTrue(((Objeto)(vetorOrd.GetElement(1))).valor.ToString() == "roman");


                }
                catch (Exception e)
                {
                    assercao.IsTrue(false,"TESTE FALHOU: "+e.Message);
                }

            }
         
        }
    } 
} 
