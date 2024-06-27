using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;


using System.Reflection;
using System;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    /// <summary>
    /// bloco básico para linguagens orientada a objeto, como o orquidea.
    /// </summary>
    public class Classe
    {

        /// <summary>
        /// nome da classe.
        /// </summary>
        public string nome { get; set; }
        public string nomeLongo { get; set; }

        /// <summary>
        /// acessor da classe: public, protected, ou private.
        /// </summary>
        public string acessor { get; set; }

        /// <summary>
        /// se [true], a classe é importada da Linguagem Base.
        /// </summary>
        public bool isImport = false;

        /// <summary>
        /// se [true] a classe é estrutural: string, double, int, char, etc...
        /// </summary>
        public bool isEstructuralClasse = false;

        /// <summary>
        /// escopo do corpo da classe.
        /// </summary>
        public Escopo escopoDaClasse { get; set; }

        /// <summary>
        /// tokens da classe.
        /// </summary>
        public List<string> tokensDaClasse { get; set; }




        public delegate object UmMetodoDaClasse(params object[] parametros);
        public enum tipoBluePrint { EH_CLASSE, EH_INTERFACE };



        /// <summary>
        /// classes herdadas da classe.
        /// </summary>
        public List<Classe> classesHerdadas { get; set; }
        
        /// <summary>
        /// interfaces da classe.
        /// </summary>
        public List<Classe> interfacesHerdadas { get; set; }




        /// <summary>
        /// instrucoes do corpo da classe.
        /// </summary>
        public List<Instrucao> instrucoesDoCorpoDaClasse { get; set; }
     
        /// <summary>
        /// construtores possiveis da classe.
        /// </summary>
        public List<Metodo> construtores { get; set; }


        /// <summary>
        /// lista de proprieades estaticas da classe.
        /// </summary>
        public List <Objeto> propriedadesEstaticas { get; set; }




        /// <summary>
        /// obtem o nome da classe.
        /// </summary>
        /// <returns></returns>
        public string GetNome()
        {
            return nome;
        }
        /// <summary>
        /// retorna uma lista dos metodos da classe.
        /// </summary>
        /// <returns></returns>
        public List<Metodo> GetMetodos()
        {
            return metodos;
        }

        /// <summary>
        /// retorna uma lista de todos metodos com o nome parametro.
        /// </summary>
        /// <param name="nomeMetodo">nome dos metodos.</param>
        /// <returns></returns>
        public List<Metodo> GetMetodo(string nomeMetodo)
        {
            return metodos.FindAll(k => k.nome.Equals(nomeMetodo));
        }

        /// <summary>
        /// retorna uma lista de todos operadores da classe.
        /// </summary>
        /// <returns></returns>
        public List<Operador> GetOperadores()
        {
            return operadores;
        }

        /// <summary>
        /// retorna uma lista de todas propriedades da classe.
        /// </summary>
        /// <returns></returns>
        public List<Objeto> GetPropriedades()
        {
            return propriedades;
        }

        /// <summary>
        /// retorna um [Objeto] de uma das propriedades da classe.
        /// </summary>
        /// <param name="nomeProp">nome da propriedade.</param>
        /// <returns></returns>
        public Objeto GetPropriedade(string nomeProp)
        {
            return propriedades.Find(k => k.GetNome() == nomeProp);
        }

        /// <summary>
        /// retorna um operador da classe.
        /// </summary>
        /// <param name="nomeOperador">nome do operador.</param>
        /// <returns></returns>
        public Operador GetOperador(string nomeOperador)
        {
            return operadores.Find(k => k.nome.Equals(nomeOperador));
        }


        public Objeto actual { get; set; }


        /// <summary>
        /// lista de todos metodos da classe.
        /// </summary>
        public List<Metodo> metodos = new List<Metodo>();
        
        /// <summary>
        /// lista de todas propriedades da classe. propriedades estáticas são armazenadas em outra lista, no escoopoROOT.
        /// </summary>
        public List<Objeto> propriedades = new List<Objeto>();

        /// <summary>
        /// lista de todos operadores da classe.
        /// </summary>
        public List<Operador> operadores = new List<Operador>();

        /// <summary>
        /// se true terminou a compilação.
        /// </summary>
        public bool isBuildMethods = true;

        private static LinguagemOrquidea linguagem;

        /// <summary>
        /// lista de info de metodos importados.
        /// </summary>
        MethodInfo[] metodoReflexao { get; set; }

        /// <summary>
        /// lista de info de classes importadas.
        /// </summary>
        ConstructorInfo[] construtoresReflexao { get; set; }
        
        /// <summary>
        /// construtor vazio.
        /// </summary>
        public Classe()
        {
            if (linguagem == null)
                linguagem = LinguagemOrquidea.Instance();
            this.metodos = new List<Metodo>();
            this.operadores = new List<Operador>();
            this.propriedades = new List<Objeto>();
            this.tokensDaClasse = new List<string>();
            this.construtores = new List<Metodo>();
            this.propriedadesEstaticas = new List<Objeto>();
            this.classesHerdadas = new List<Classe>();
            this.interfacesHerdadas = new List<Classe>();
            this.GetInfoReflexao();
        }

        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="acessor">tipo de acesso da classe: public, protected, ou private.</param>
        /// <param name="name">nome da classe.</param>
        /// <param name="methods">lista de metodos da classe.</param>
        /// <param name="operadores">lista de operadores da classe.</param>
        /// <param name="propriedades">lista de propriedades da classe.</param>
        public Classe(string acessor, string name, List<Metodo> methods, List<Operador> operadores, List<Objeto> propriedades):base()
        {
          
            this.acessor = acessor;
            this.nome = name;
            this.tokensDaClasse = new List<string>();
            this.metodos = new List<Metodo>();
            this.propriedades = new List<Objeto>();
            this.propriedadesEstaticas = new List<Objeto>();
            this.operadores = new List<Operador>();
            this.construtores = new List<Metodo>();
            this.classesHerdadas = new List<Classe>();
            this.interfacesHerdadas = new List<Classe>();
            // adiciona os métodos da classe.
            if (methods != null)
                for (int i = 0; i < methods.Count; i++)
                    this.metodos.Add(methods[i]);

            // adiciona as propriedades da classe.
            if (propriedades != null)
                for (int i = 0; i < propriedades.Count; i++)
                    this.propriedades.Add(propriedades[i]);

            // adiciona os operadores da classe.
            if (operadores != null)
                for (int i = 0; i < operadores.Count; i++)
                {
                    operadores[i].tipoRetorno = this.nome; // seta a classe a qual o operador pertence.
                    this.operadores.Add(operadores[i]);
                }
            this.GetInfoReflexao();
            RepositorioDeClassesOO.Instance().RegistraUmaClasse(this);

       } // constructor



        private void GetInfoReflexao()
        {
            string pathAssembly = Path.GetFullPath(@"ProjectOrquidea2024.dll");
            Assembly assemblyParserOrquidea = Assembly.LoadFrom(pathAssembly);

            List<Type> types = assemblyParserOrquidea.GetTypes().ToList<Type>();
            Type tipoDaClasse = types.Find(k => k.Name.Equals(this.nome));
            
            
            if (tipoDaClasse != null)
            {
                this.metodoReflexao = tipoDaClasse.GetMethods();
                this.construtoresReflexao = tipoDaClasse.GetConstructors();
            }
        }

     
        /// <summary>
        /// faz uma chamada do método constructor de objeto importados, que tem os parâmetros especificados na entrada.
        /// </summary>
        /// <param name="tipoClasse">tipo da classe importada.</param>
        /// <param name="parametrosConstructor">parametros do construtor.</param>
        public object CallConstructor(Type tipoClasse, object[] parametrosConstructor)
        {
            ConstructorInfo[] construtores = tipoClasse.GetConstructors();
            if (construtores.Length > 0)
                return construtores[0].Invoke(parametrosConstructor);
            else
                return null;
        } // GetConstructor()

        
        /// <summary>
        /// faz uma chamada de metodo importado.
        /// </summary>
        /// <param name="objectCaller">objeto que chama o metodo.</param>
        /// <param name="nameMethod">nome do metodo.</param>
        /// <param name="parametersCall">lista de parametros da chamada.</param>
        /// <returns></returns>
        public object CallAMethod(object objectCaller, string nameMethod, object[] parametersCall)
        {
            Metodo metodoInvocado = this.GetMetodos().Find(k => k.nome.Equals(nameMethod));
            return metodoInvocado.InfoMethod.Invoke(objectCaller, parametersCall);
        }

        /// <summary>
        /// Obtem para um arquivo texto, a descrição da classe, com propriedades, metodos, e operadores.
        /// </summary>
        /// <param name="classeAGuardar">nome da classe.</param>
        public void Save(Classe classeAGuardar)
        {
            Stream stream = new FileStream("classe_" + this.nome + ".txt", FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);
          
            SaveClassePropriedadesMetodosOperadores(classeAGuardar, writer);  // escreve recursivamente todas propriedades, metodos, ou operadores, herdados ou nao.

            writer.Close();
            stream.Close();

            writer.Dispose();
            stream.Dispose();
        }  // Save()

        private void SaveClassePropriedadesMetodosOperadores(Classe classeAGuardar, StreamWriter writer)
        {
            string name = "";
            if (RepositorioDeClassesOO.Instance().GetClasse(classeAGuardar.nome) != null)
                name = "classe";
            else
            if (RepositorioDeClassesOO.Instance().ObtemUmaInterface(classeAGuardar.nome) != null)
                name = "interface";
            writer.WriteLine(name + ": " + classeAGuardar.nome + TextoDescritivo(classeAGuardar) + "\n");
            writer.WriteLine();
            if ((classeAGuardar.classesHerdadas != null) && (classeAGuardar.classesHerdadas.Count > 0))
                for (int x = 0; x < classeAGuardar.classesHerdadas.Count; x++)
                {
                    writer.WriteLine("classe Herdada: \n");
                    SaveClassePropriedadesMetodosOperadores(classeAGuardar.classesHerdadas[x], writer); // escrita recursiva, para capturar todos metodos, propriedades, e operadores de todas classes herdadas.
                    writer.WriteLine();
                }
            if ((classeAGuardar.interfacesHerdadas != null) && (classeAGuardar.interfacesHerdadas.Count > 0))
                for (int x = 0; x < classeAGuardar.interfacesHerdadas.Count; x++)
                {
                    writer.WriteLine("interface Herdada: \n");
                    SaveClassePropriedadesMetodosOperadores(classeAGuardar.interfacesHerdadas[x], writer);
                    writer.WriteLine();
                }
        

        }

        /// <summary>
        /// carrega uma classe em um arquivo.
        /// </summary>
        /// <param name="nomeClasse">nome da classe a carregar.</param>
        /// <returns></returns>
        public static Classe Load(string nomeClasse)
        {
            LinguagemOrquidea linguage = LinguagemOrquidea.Instance();
            Stream stream = new FileStream("classe_" + nomeClasse + ".txt", FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);

            // le todos tokens da classe, em uma linha só.
            string todosTokensDaClasse = reader.ReadLine();
            todosTokensDaClasse = todosTokensDaClasse.Trim(' ');

            // cria um extrator de classes, métodos e propriedades.
            ExtratoresOO extratores = new ExtratoresOO(new List<string>() { todosTokensDaClasse }, new Escopo(new List<string>() { todosTokensDaClasse }));
            Classe umaClasse = extratores.ExtaiUmaClasse(Classe.tipoBluePrint.EH_CLASSE);
            if (umaClasse == null)
                return null;
            reader.Close();
            stream.Close();

            reader.Dispose();
            stream.Dispose();
            return umaClasse;
        } // Load()


        public override string ToString()
        {
            return TextoDescritivo(this);
        } // ToString()


        /// <summary>
        /// obtem um texto info, da classe, com nomes e tipos de propriedades, metodos, operadores.
        /// </summary>
        /// <param name="classe">classe do texto.</param>
        /// <returns></returns>
        public static string TextoDescritivo(Classe classe)
        {
            string str = "nome: " + classe.GetNome() + "  \n";

            if ((classe.propriedadesEstaticas != null) && (classe.propriedadesEstaticas.Count > 0))
            {
                str += "propriedades estaticas: \n";
                for (int x = 0; x < classe.propriedadesEstaticas.Count; x++)
                    str += classe.propriedadesEstaticas[x].ToString() + "  \n";
            }
            str += "\n";
            str += "\n";
            if ((classe.propriedades != null) && (classe.propriedades.Count > 0))
            {
                str += "propriedades: \n";
                for (int x = 0; x < classe.propriedades.Count; x++)
                    str += classe.propriedades[x].ToString() + "  \n";
            } // if

            str += "\n";
            str += "\n";
            if ((classe.metodos != null) && (classe.metodos.Count > 0))
            {
                str += "metodos: \n";
                for (int x = 0; x < classe.metodos.Count; x++)
                    str += classe.metodos[x].ToString() + " \n";
            } // if

            str += "\n";
            str += "\n";
            if ((classe.operadores != null) && (classe.operadores.Count > 0))
            {
                str += "operadores:  \n";
                for (int x = 0; x < classe.operadores.Count; x++)
                    str += classe.operadores[x].ToString();
            }
            return str;

        }
    } // class Classe

} // namespace parser
