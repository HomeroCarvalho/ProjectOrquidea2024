using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace parser
{
    /// <summary>
    /// classe para desenho de texto, na tela.
    /// </summary>
    public class Text:Objeto
    {
        /// <summary>
        /// objeto texto para desenho.
        /// </summary>
        public SFML.Graphics.Text texto;

        /// <summary>
        /// index id perante a lista de textos a desenhar na tela.
        /// </summary>
        private int indexText = 0;
   
        /// <summary>
        /// fonte utilizada para desenhos de texto.
        /// </summary>
        private static SFML.Graphics.Font fonte = new Font("arial.ttf");

        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="str_text">texto a ser mostrado.</param>
        /// <param name="x">coordenada x de desenho.</param>
        /// <param name="y">coordenada y de desenho.</param>
        public Text(string str_text, double x, double y)
        {

            this.texto = new SFML.Graphics.Text(str_text, fonte);
            this.texto.Color = Color.White;
            this.texto.Position = new SFML.System.Vector2f((float)x, (float)y);

            LoopGame.textos.Add(this);
            this.indexText = LoopGame.textos.Count - 1;

            SetProps(x, y);
        }


        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="str_text">texto a ser mostrado.</param>
        /// <param name="x">coordenada x de desenho.</param>
        /// <param name="y">coordenada y de desenho.</param>
        public Text(string str_text, double x, double y, int sizeFont)
        {
            
            
            this.texto = new SFML.Graphics.Text(str_text, fonte);
            this.texto.CharacterSize = (uint)sizeFont;
            this.texto.Color= Color.White;
            this.texto.Position = new SFML.System.Vector2f((float)x, (float)y);
            LoopGame.textos.Add(this);
            this.indexText = LoopGame.textos.Count - 1;
            this.SetProps(x, y);
        }


        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="str_text">texto a ser mostrado.</param>
        /// <param name="nameFileFont">nome do arquivo da fonte.</param>
        /// <param name="x">coordenada x para desenho.</param>
        /// <param name="y">coordenada y para desenho.</param>
        public Text(string str_text, string nameFileFont, double x, double y)
        {
            SFML.Graphics.Font fonte= new Font(nameFileFont);
            this.texto = new SFML.Graphics.Text(str_text, fonte);
            this.texto.Position = new SFML.System.Vector2f((float)x, (float)y);
            this.texto.Color = Color.White;
            LoopGame.textos.Add(this);
            this.indexText = LoopGame.textos.Count - 1;
            this.SetProps(x, y);
        }

        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="str_text">texto a ser mostrado.</param>
        /// <param name="nameFileFont">nome do arquivo da fonte.</param>
        /// <param name="corText">cor do texto.</param>
        /// <param name="x">coordenada x para desenho.</param>
        /// <param name="y">coordenada y para desenho.</param>
        public Text(string str_text, string nameFileFont, Color corText, double x , double y)
        {
            SFML.Graphics.Font fonte = new Font(nameFileFont);
            
            this.texto = new SFML.Graphics.Text(str_text, fonte);
            this.texto.Color = corText;
            this.texto.Position = new SFML.System.Vector2f((float)x, (float)y);

            LoopGame.textos.Add(this);
            this.indexText = LoopGame.textos.Count - 1;
            this.SetProps(x, y);
        }

        /// <summary>
        /// seta um novo texto info.
        /// </summary>
        /// <param name="newTexto">novo texto info.</param>
        /// <param name="x">coordenada x para desenho.</param>
        /// <param name="y">coordenada y para desenho.</param>
        public void SetText(string newTexto, double x, double y)
        {
            int index = (int)this.GET("index");
            LoopGame.textos[this.indexText] = new Text(newTexto, x, y);
        }

        /// <summary>
        /// seta propriedades adicionais.
        /// </summary>
        /// <param name="x">coordenada x para desenho.</param>
        /// <param name="y">coordenada y para desenho.</param>
        private void SetProps(double x, double y)
        {
            this.SET("x", "double", x);
            this.SET("y", "double", y);
            this.SET("index", "int", this.indexText);
        }


        public new class Testes: SuiteClasseTestes
        {
            public Testes():base("testes de desenho de textos")
            {

            }

            public void TestesDesenhoTextos(AssercaoSuiteClasse assercao)
            {
                string pathFile = @"programasTestesJogos\programaJogoDesenhosDeTextos.DAT";
                ParserAFile.ExecuteAProgram(pathFile);
            }
        }
    }
}
