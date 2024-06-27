using System;
using System.Runtime.CompilerServices;

namespace parser.ProgramacaoOrentadaAObjetos
{

    /// <summary>
    /// implementa o gerenciamento do tempo no jogo.
    /// </summary>
    public class TimeGame
    {

        private DateTime dtLastCicle = DateTime.Now;
        private DateTime dtActual;

       
        /// <summary>
        /// obtém o tempo absoluto em milisegundos.
        /// </summary>
        /// <returns></returns>
        public double GetTime()
        {
            dtActual = DateTime.Now;
            TimeSpan tempoDecorrido = dtActual - dtLastCicle;
            dtLastCicle = dtActual;

            return tempoDecorrido.TotalMilliseconds;
        } //GetTime()

    }  // class TimeGame


    /// <summary>
    /// classe que encapsula o tempo de reação para determinada atividade. É semelhante ao [TimeGame.GetElapsedTime()],
    /// mas contém algumas funcionalidade a mais, além de esconder as contas feitas do tempo percorrido.
    /// </summary>
    public class TimeReaction: Objeto
    {
        private double timeTrigger;
        private double timeAcumulated = 0.0f;


        private TimeGame time = new TimeGame();
       
       

        /// <summary>
        /// calcula o tempo em millisegundos, a partir do valor de fps (frames by seconds).
        /// </summary>
        /// <param name="_fps">tempo em millsegundos</param>
        /// <returns></returns>
        public static double GetTimeFromFPS(double  _fps)
        {

            if (_fps != 0)
                return 1000.0 / _fps;
            else
                return 0.0;
        }

        /// <summary>
        /// construtor sem parametros. seta o time de disparo para 20.0 fps;
        /// </summary>
        public TimeReaction()
        {
            this.timeTrigger = 100.0 / 20.0;
        }

        /// <summary>
        /// seta um temporizador.
        /// </summary>
        /// <param name="timeInMillsec">frames por segundo para um ciclo.</param>
        public TimeReaction(double timeInMillsec)
        {
            /// x fps---> 1000 mlsec;
            /// 1 fps----> y mlsec.---> y=1000.0f/fps
            /// 
            this.timeTrigger = timeInMillsec;
        } // TimeRection()

        /// <summary>
        /// retorna [1] se passou o tempo de reação, [0] se não passou o tempo de reação.
        /// conta o tempo restante para o próximo ciclo.
        /// </summary>
        /// <returns>retorna [true] para ocorreu o tempo de reagir, [false] se não.</returns>
        public int IsTimeToAct()
        {
            this.timeAcumulated+= time.GetTime();
            if (this.timeAcumulated>this.timeTrigger)
            {
                this.timeAcumulated = this.timeAcumulated - this.timeTrigger;
                return 1;
            } // if
            return 0;
        }

      
        
       
    

    }



} 
