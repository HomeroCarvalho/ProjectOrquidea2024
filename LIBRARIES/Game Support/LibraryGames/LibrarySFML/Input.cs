using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace parser
{
    /// <summary>
    /// classe para entrada de dados: teclas, mouse.
    /// </summary>
    public class Input
    {

        /// <summary>
        /// lista contendo keys relevante no game.
        /// </summary>
        private static List<string> keysControlInput = new List<string>();




   
        /// <summary>
        /// lista de expressoes handlers de teclas down.
        /// </summary>
        private static List<ExpressionBase> handlersKeyDown = new List<ExpressionBase>();

        /// <summary>
        /// lista de expressoes handler de teclas released.
        /// </summary>
        private static List<ExpressionBase> handlersReleased = new List<ExpressionBase>();


        /// <summary>
        /// mapeamento de nome de keys (texto string) e keys de RayLib.
        /// </summary>
        private static Dictionary<string, SFML.Window.Keyboard.Key> keysIndID = null;


        /// <summary>
        /// true de o botao esquerdo do mouse foi pressionado.
        /// </summary>
        public static bool isMouseLeftButtonIsPressed= false;
        /// <summary>
        /// true se o botao direito do mouse foi pressionado.
        /// </summary>
        public static bool isMouseRightButtonIsPressed = false;
        /// <summary>
        /// tre se o botao esquerdo do mouse foi despressionado.
        /// </summary>
        public static bool isMouseLeftButtonIsReleased = false;
        /// <summary>
        /// true se o botao direito do mouse foi despressionado.
        /// </summary>
        public static bool isMouseRightButtonIsReleased = false;


        /// <summary>
        /// coordenadas do ponteiro do mouse.
        /// </summary>
        public static Vector2D mousePosition = new Vector2D(0.0, 0.0);

        /// <summary>
        /// coordenadas relativas a roda do mouse.
        /// </summary>
        public static Vector2D mouseWheelPosition = new Vector2D(0.0, 0.0);

        /// <summary>
        /// valor delta da roda do mouse.
        /// </summary>
        public static double deltaMouseWheel = 0.0;

        /// <summary>
        /// se true, o sistema de input ja foi inicializado.
        /// </summary>
        public static bool isAlreadyInit = false;
        public Input()
        {
            InitInput();
        }

        /// <summary>
        /// inicializa o sistema de input.
        /// </summary>
        public static void InitInput()
        {
            if (isAlreadyInit)
            {
                return;
            }
            else
            {
                isAlreadyInit = true;
                // adiciona as prropriedades de coordenadas, da posicao do mouse.
                mousePosition.SET("xx2", "double", 0.0);
                mousePosition.SET("yy2", "double", 0.0);

                // adiciona as propriedades de coordenadas da roda do mouse.
                mouseWheelPosition.SET("xx2", "double", 0.0);
                mouseWheelPosition.SET("yy2", "double", 0.0);


                if ((keysControlInput == null) || (keysControlInput.Count == 0))
                {
                    keysIndID = new Dictionary<string, Keyboard.Key>();
                    // teclas frequentes em jogos.
                    if (keysIndID.Count == 0)
                    {
                        // teclas de setas
                        keysIndID = new Dictionary<string, SFML.Window.Keyboard.Key>();
                        keysIndID.Add("left", SFML.Window.Keyboard.Key.Left);
                        keysIndID.Add("right", SFML.Window.Keyboard.Key.Right);
                        keysIndID.Add("up", SFML.Window.Keyboard.Key.Up);
                        keysIndID.Add("down", SFML.Window.Keyboard.Key.Down);

                        // teclas muito utilizadas em games.
                        keysIndID.Add("space", SFML.Window.Keyboard.Key.Space);
                        keysIndID.Add("esc", SFML.Window.Keyboard.Key.Escape);
                        keysIndID.Add("enter", SFML.Window.Keyboard.Key.Enter);

                        // botao alternativo/ aditivo para um jogo.
                        keysIndID.Add("z", SFML.Window.Keyboard.Key.Z);

                        // botao alternativo/ aditivo para um jogo.
                        keysIndID.Add("x", SFML.Window.Keyboard.Key.X);

                        // tecla [Quit] eventos.
                        keysIndID.Add("q", SFML.Window.Keyboard.Key.Q);

                        // teclas de numeros.
                        keysIndID.Add("0", SFML.Window.Keyboard.Key.Num0);
                        keysIndID.Add("1", SFML.Window.Keyboard.Key.Num1);
                        keysIndID.Add("2", SFML.Window.Keyboard.Key.Num2);
                        keysIndID.Add("3", SFML.Window.Keyboard.Key.Num3);
                        keysIndID.Add("4", SFML.Window.Keyboard.Key.Num4);
                        keysIndID.Add("5", SFML.Window.Keyboard.Key.Num5);
                        keysIndID.Add("6", SFML.Window.Keyboard.Key.Num6);
                        keysIndID.Add("7", SFML.Window.Keyboard.Key.Num7);
                        keysIndID.Add("8", SFML.Window.Keyboard.Key.Num8);
                        keysIndID.Add("9", SFML.Window.Keyboard.Key.Num9);
                        
                    }
                }
            }
            

        }

        /// <summary>
        /// retorna true se uma tecla monitorada foi pressionada.
        /// </summary>
        /// <param name="key">tecla a verificar</param>
        /// <returns></returns>
        public static bool isKeyDown(string key)
        {
            InitInput();
            return SFML.Window.Keyboard.IsKeyPressed(keysIndID[key]);
        }

        /// <summary>
        /// retorna true se uma tecla monitorada foi despressionada.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool isKeyUP(string key)
        {
            InitInput();
            return SFML.Window.Keyboard.IsKeyPressed(keysIndID[key]);
        }


        /// <summary>
        /// retonra 1 se o botao esquerdo do mouse foi pressionado. retorna 0 se não.
        /// </summary>
        /// <returns></returns>
        public static int MouseleftPressed()
        {
            InitInput();
            if  (Mouse.IsButtonPressed(Mouse.Button.Left))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// retorna 1 se o botao direito do mouse foi pressionado. retorna 0 se não.
        /// </summary>
        /// <returns></returns>
        public static int MouserightPressed()
        {
            InitInput();
            if (Mouse.IsButtonPressed (Mouse.Button.Right)) {
                return 1;
            }
            else 
            {
                return 0;
            }

        }




  
        /// <summary>
        /// obtem as coordenadas do mouse. transfere para o vector 2d mousePosition.
        /// </summary>
        /// <param name="x">coordenada X atual do mouse.</param>
        /// <param name="y">coordenada Y atual do mouse.</param>
        public static void MouseMove(int x, int y)
        {
            InitInput();
            Input.mousePosition.UpdatePropriedade("xx2", "double", (double)x);
            Input.mousePosition.UpdatePropriedade("yy2", "double", (double)y);
        }


        /// <summary>
        /// obtem as coordenada da roda do mouse.
        /// </summary>
        /// <param name="x">coordenada X atual da roda do mouse.</param>
        /// <param name="y">coordenada Y atual da roda do mouse.</param>
        public static void MouseWheelMoved(int x, int y)
        {
            InitInput();
            Input.mouseWheelPosition.UpdatePropriedade("xx2", "double", (double)x);
            Input.mouseWheelPosition.UpdatePropriedade("yy2", "double", (double)y);
            
        }


        /// <summary>
        /// retorna a posicao do mouse.
        /// </summary>
        /// <returns></returns>
        public static Vector2D GetMousePosition()
        {
            InitInput();
            return Input.mousePosition;
        }

        /// <summary>
        /// obtem o delta da roda do mouse
        /// </summary>
        /// <returns>retorna um inteiro contendo o delta da roda do mouse.</returns>
        public static double GetDeltaWheel()
        {
            InitInput();
            double delta= Input.deltaMouseWheel;
            Input.deltaMouseWheel= 0;
            return delta;
        }


        /// <summary>
        /// retorna [true] se o botao esquerdo do mouse foi pressionado ou nao.
        /// </summary>
        /// <returns></returns>
        public static bool MouseButtonLeftPressed()
        {
            InitInput();
            bool isPressed = Input.isMouseLeftButtonIsPressed;
            Input.isMouseLeftButtonIsPressed = false;
            return isPressed;
        }


        /// <summary>
        /// retorna [true] se o botao direito do mouse foi pressionado.
        /// </summary>
        /// <returns></returns>
        public static bool MouseButtonRightPressed()
        {
            InitInput();
            bool isPressed = Input.isMouseRightButtonIsPressed;
            Input.isMouseRightButtonIsPressed = false;
            return isPressed;
        }


        /// <summary>
        /// retorna [true] se o botao esquerdo do mouse foi despressionado ou nao.
        /// </summary>
        /// <returns></returns>
        public static bool MouseButtonLeftReleased()
        {
            InitInput();
            bool isPressed = Input.isMouseLeftButtonIsReleased;
            Input.isMouseLeftButtonIsReleased = false;
            return isPressed;
        }


        /// <summary>
        /// retorna [true] se o botao direito do mouse foi despressionado ou nao.
        /// </summary>
        /// <returns></returns>
        public static bool MouseButtonRightReleased()
        {
            InitInput();
            bool isPressed = Input.isMouseRightButtonIsReleased;
            Input.isMouseRightButtonIsReleased = false;
            return isPressed;
        }



       

    }
}
