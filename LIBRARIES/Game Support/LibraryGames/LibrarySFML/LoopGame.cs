using SFML.Graphics;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    /// <summary>
    /// classe que contem a tela de desenho, e funcionalidades de renderização em loop.
    /// </summary>
    public class LoopGame: Objeto
    {
        /// <summary>
        /// janela de desenho do jogo.
        /// </summary>
        private RenderWindow wnd;

        /// <summary>
        /// lista de imagens a desenhar na tela.
        /// </summary>
        public static  List<Imagem> imagens = new List<Imagem>();

        /// <summary>
        /// lista de textos a desenhar na tela.
        /// </summary>
        public static List<Text> textos = new List<Text>();

        /// <summary>
        /// fonte utilizada em textos.
        /// </summary>
        private Font fontTexts;

        /// <summary>
        /// cor dos textos.
        /// </summary>
        private Color colorTexts = Color.Yellow;


        /// <summary>
        /// construtor vazio.
        /// </summary>
        public LoopGame()
        {

        }

        /// <summary>
        /// inicializa e mostra  a tela de desenho.
        /// </summary>
        /// <param name="width">dimensão da tela.</param>
        /// <param name="height">dimensão da tela.</param>
        public LoopGame(int width, int height)
        {
            VideoMode mode = VideoMode.DesktopMode;
            mode.Width = (uint)width;
            mode.Height = (uint)height;

            this.wnd = new RenderWindow(mode, "Lets Play!");
            this.fontTexts = new Font("arial.ttf");
            this.valor = this;

            this.InitEventsWindow();
        } 

        /// <summary>
        /// inicializa e mostra a tela de desenho, com um titulo de apresentação.
        /// </summary>
        /// <param name="width">dimensão da tela.</param>
        /// <param name="height">dimensão da tela.</param>
        /// <param name="title">titulo informe, acima da tela.</param>
        public LoopGame(int width, int height, string title)
        {

            VideoMode mode = VideoMode.DesktopMode;
            mode.Width = (uint)width;
            mode.Height = (uint)height;

            this.wnd = new RenderWindow(mode, title);
            this.fontTexts = new Font("arial.ttf");
            this.valor = this;

            this.InitEventsWindow();
        }

        /// <summary>
        /// inicio da area de desenho.
        /// </summary>
        public void BeginDraw()
        {
            this.wnd.Clear(SFML.Graphics.Color.Black);
        }

        /// <summary>
        /// termino da area de desenho.
        /// </summary>
        public void EndDraw()
        {
            
            this.wnd.Display();
        }

        /// <summary>
        /// seta a taxa de frames por segundo.
        /// </summary>
        /// <param name="fps"></param>
        public void SetFps(int fps)
        {
            this.wnd.SetFramerateLimit((uint)fps);

        }

        /// <summary>
        /// fecha a janela de desenho.
        /// </summary>
        public void CloseWindow()
        {
            this.wnd.Close();
        }

        /// <summary>
        /// desenha uma imagem sfml.
        /// </summary>
        /// <param name="image">imagem a desenhar.</param>
        /// <param name="x">coordenada x para desenho.</param>
        /// <param name="y">coodenada y para desenho.</param>
        public void Draw(Imagem image, double x, double y)
        {
            if (image != null)
            {
                image.spr_image.Position = new SFML.System.Vector2f((float)x, (float)y);
                wnd.Draw(image.spr_image);
            }
          
        }



        /// <summary>
        /// desenha uma imagem.
        /// </summary>
        /// <param name="id">id de identificacao da imagem, dentro do loop.</param>
        /// <param name="xx">coordenada x para desenho.</param>
        /// <param name="yy">coordenada y para desenho.</param>
        public void Draw(int id, double xx, double yy)
        {
            if ((id >= 0) && (id < imagens.Count))
            {
                imagens[id].spr_image.Position = new SFML.System.Vector2f((float)xx, (float)yy);
                wnd.Draw(imagens[id].spr_image);
            }
            
        }

        /// <summary>
        /// desenha um texto.
        /// </summary>
        /// <param name="text">texto a ser desenhado.</param>
        /// <param name="x">coordenada x para desenho.</param>
        /// <param name="y">coordenada y para desenho.</param>
        public void DrawText(Text text, double x, double y)
        {
            if (text != null) 
            {
                text.texto.Position = new SFML.System.Vector2f((float)x, (float)y);
                wnd.Draw(text.texto);
            }

            
        }


        /// <summary>
        /// desenha um texto.
        /// </summary>
        /// <param name="id">id de identificacao do texto. geralmente é o indice de sequencia de instanciacao do texto.</param>
        /// <param name="x">coordenada x para desenho.</param>
        /// <param name="y">coordenada y para desenho.</param>
        public void DrawText(int id, double x, double y)
        {
            if ((LoopGame.textos == null) || (LoopGame.textos.Count == 0))
            {
                return;
            }
            else
            {
                if ((id >= 0) && (id < LoopGame.textos.Count))
                {
                    textos[id].texto.Position = new SFML.System.Vector2f((float)x, (float)y);
                    this.wnd.Draw(textos[id].texto);
                }
            }
            
        }

        

  


        /// <summary>
        /// mantem ou fecha a tela.
        /// </summary>
        /// <returns></returns>
        public int isOpen()
        {
            if (this.wnd.IsOpen)
            {
                return 0;
            }
            else
            {
                this.wnd.Close();
                return 1;
            }


        }

        /// <summary>
        /// inicializa o handler de janela para fechamento de janela.
        /// </summary>
        public void InitEventsWindow()
        {
            wnd.Closed += Wnd_Closed;
            wnd.MouseMoved += Wnd_MouseMoved;
            wnd.MouseButtonPressed += Wnd_MouseButtonPressed;
            wnd.MouseButtonReleased += Wnd_MouseButtonReleased;
            wnd.MouseWheelScrolled += Wnd_MouseWheelScrolled;
          
        }

        /// <summary>
        /// handler de evento se a roda do mouse mudou de posicao, e também se o delta da roda do mouse foi modificado.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wnd_MouseWheelScrolled(object? sender, MouseWheelScrollEventArgs e)
        {
            Input.MouseWheelMoved(e.X, e.Y);
            Input.deltaMouseWheel = (double)e.Delta;
        }




        /// <summary>
        /// handler de evento se algum botao do mouse foi despressionado ou nao.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wnd_MouseButtonReleased(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button== Mouse.Button.Left)
            {
                Input.isMouseLeftButtonIsReleased = true;
            }
            else
            if (e.Button == Mouse.Button.Right)
            {
                Input.isMouseRightButtonIsReleased = true;
            }
        }


        /// <summary>
        /// handler de evento se algum botao do mouse foi pressionado.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wnd_MouseButtonPressed(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == Mouse.Button.Left)
            {
                Input.isMouseLeftButtonIsPressed = true;
            }
            else
            if (e.Button != Mouse.Button.Right)
            {
                Input.isMouseRightButtonIsPressed = true;
            }

        }

        /// <summary>
        /// handler de evento mouse moveu-se.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wnd_MouseMoved(object? sender, MouseMoveEventArgs e)
        {
            Input.MouseMove(e.X, e.Y);
        }



        /// <summary>
        /// handler de evento de fechar a janela.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wnd_Closed(object? sender, EventArgs e)
        {
            this.wnd.Close();
        }


        /// <summary>
        /// dispacha eventos de tela.
        /// </summary>
        public void dispachEvents()
        {
            this.wnd.DispatchEvents();
        }


        public new class Testes: SuiteClasseTestes
        {
            public Testes() : base("TESTE DE DESEMPENHO BIBLIOTECA GRAFICA")
            {

            }

            public void TesteCatchGliders(AssercaoSuiteClasse assercao)
            {
                string pathFile = @"programa jogos\CatchMeteors.txt";
                ParserAFile.ExecuteAProgram(pathFile);
            }

            public void TestesTextos(AssercaoSuiteClasse assercao)
            {
                string pathFile = @"programasTestesJogos\programaJogoDesenhosDeTextos.DAT";
                ParserAFile.ExecuteAProgram(pathFile);
            }


            public void TesteInvadersSFMLComImagens(AssercaoSuiteClasse assercao)
            {

                string path = @"programa jogos\SpaceInvadersSFML_ComImagens.txt";
                ParserAFile.ExecuteAProgram(path);

                System.Environment.Exit(0);

            }
          


        }
    }

    
}
