using parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Modulos
{
    /// <summary>
    /// classe de gerenciamento de classes importadas, as bibliotecas de classes.
    /// </summary>
    public class Library
    {

        /// <summary>
        /// construtor vazio.
        /// </summary>
        public Library()
        {
            nameFilesLibrariesRegistred["Prompt"] = typeof(Prompt);
        }



        /// <summary>
        /// classes que implementam uma biblioteca.
        /// </summary>
        private List<Type> bibliotecas = new List<Type>();
        /// <summary>
        /// lista de todas bibliotecas padrão da linguagem.
        /// </summary>
        private static Dictionary<string, Type> nameFilesLibrariesRegistred = new Dictionary<string, Type>();
        /// <summary>
        /// informações de metodos importados.
        /// </summary>
        public List<MethodInfo> metodosBibliotecas = new List<MethodInfo>();



   
        /// <summary>
        /// importa todas classes da linguagem base, de um arquivo .exe, ou .dll.
        /// </summary>
        /// <param name="path">path de arquivo.</param>
        public static void ImportLibrayFromAssembly(string path)
        {
            ImportadorDeClasses importador = new ImportadorDeClasses(path);
        }


        /// <summary>
        /// importa classes da linguagem base, de um arquivo .exe ou .dll.
        /// Libraries devem estar contida numa classe, ou conjunto de classe definidas.
        /// </summary>
        /// <param name="path">path do arquivo Assembly, .exe ou .dll, que contem a library.</param>
        /// <param name="namesLibrary">nomes de classes da library.</param>
        public static void ImportLibrayFromAssembly(string path, string[] namesLibrary)
        {
            ImportadorDeClasses importador = new ImportadorDeClasses(path);
            foreach(string umaBiblioteca in namesLibrary)
            {
                importador.ImportAClassFromAssembly(umaBiblioteca);
            }     
        }




    }
}