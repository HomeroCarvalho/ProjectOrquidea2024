using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    /// <summary>
    /// classe de conversao de um tipo para outro.
    /// </summary>
    public class Castings
    {

        /// <summary>
        /// converte um numero int para numero double.
        /// </summary>
        /// <param name="numeroInt">numero inteiro</param>
        /// <returns>retorna um numero double.</returns>
        public static double IntToDouble(object numeroInt)
        {
            return double.Parse(numeroInt.ToString());
        }


        /// <summary>
        /// converte um numero para outro.
        /// </summary>
        /// <param name="tipoFROM">tipo de origem.</param>
        /// <param name="tipoTO">tipo de destino.</param>
        /// <param name="valorFROM">valor a converter.</param>
        /// <returns></returns>
        public static object NumberToNumer(string tipoFROM, string tipoTO, object valorFROM)
        {
            if (valorFROM == null)
            {
                return null;
            }    

            tipoFROM = tipoFROM.Replace("System.", "");
            tipoTO = tipoTO.Replace("System.", "");
            if ((tipoFROM == "int") || (tipoFROM == "Int32"))   
            {
                if (tipoTO == "double")
                {
                    return Convert.ToDouble(valorFROM.ToString());
                }
                else
                if (tipoTO == "float")
                {
                    return Convert.ToSingle(valorFROM.ToString());
                }
                else
                if ((tipoTO == "int") || (tipoTO=="Int32"))
                {
                    return Convert.ToUInt32(valorFROM.ToString());
                }
            }
            else
            if (tipoFROM == "double")
            {
                if (tipoTO == "double")
                {
                    return Convert.ToDouble(valorFROM.ToString());
                }
                else
                if (tipoTO == "float")
                {
                    return Convert.ToSingle(valorFROM.ToString());
                }
                else
                if ((tipoTO == "int") || (tipoTO == "Int32"))
                {
                    return Convert.ToUInt32(valorFROM.ToString());
                }
            }
            else
            if (tipoFROM == "float")
            {
                if (tipoTO == "double")
                {
                    return Convert.ToDouble(valorFROM.ToString());
                }
                else
                if (tipoTO == "float")
                {
                    return Convert.ToSingle(valorFROM.ToString());
                }
                else
                if ((tipoTO == "int") || (tipoTO == "Int32"))
                {
                    return Convert.ToUInt32(valorFROM.ToString());
                }
            }
            
            return valorFROM;
           
        }
        /// <summary>
        /// converte um object para int. Se object for do tipo
        /// Objeto, faz a conversão com seu campo valor.
        /// </summary>
        /// <param name="value">objeto a converter.</param>
        /// <returns>retorna um int.</returns>
        public static int ToInt(object value)
        {
            if (value.GetType()== typeof(Objeto))
            {
                Objeto obj=(Objeto)value;
                return (int)obj.valor;
            }
            else
            {
                return (int)value;
            }
            
        }


        /// <summary>
        /// converte um object para double. Se object for do tipo
        /// Objeto, faz a conversão com seu campo valor.
        /// </summary>
        /// <param name="value">object a converter.</param>
        /// <returns>retorna um double.</returns>
        public static double ToDouble(object value)
        {
            if (value.GetType() == typeof(Objeto))
            {
                Objeto obj = (Objeto)value;
                return (double)obj.valor;
            }
            else
            {
                return (double)value;
            }
            
        }

        /// <summary>
        /// converte um object para um float. Se object for do tipo
        /// Objeto, faz a conversão com seu campo valor.
        /// </summary>
        /// <param name="value">object a converter.</param>
        /// <returns>retorna um float.</returns>
        public static float ToFloat(object value)
        {
            if (value.GetType() == typeof(Objeto))
            {
                Objeto obj = (Objeto)value;
                return (float)obj.valor;
            }
            else
            {
                return (float)value;
            }
            
        }

        /// <summary>
        /// converte um object para um string. Se object for do tipo
        /// Objeto, faz a conversão com seu campo valor.
        /// </summary>
        /// <param name="value">object a converter.</param>
        /// <returns>retorna um string.</returns>
        public static string ToString(object value)
        {
            if (value == null)
            {
                return "NULL";
            }

            if (value.GetType() == typeof(Objeto))
            {
                Objeto obj = (Objeto)value;
                return obj.valor.ToString();
            }
            else
            {
                return value.ToString();
            }
            
        }

        /// <summary>
        /// converte um object para um char. Se object for do tipo
        /// Objeto, faz a conversão com seu campo valor.
        /// </summary>
        /// <param name="value">object a converter.</param>
        /// <returns>retorna um char.</returns>
        public static char ToChar(object value)
        {
            if (value.GetType() == typeof(Objeto))
            {
                Objeto obj = (Objeto)value;
                return (char)obj.valor;
            }
            else
            {
                return (char)value;
            }
            
        }

        /// <summary>
        /// converte um object para um bool. Se object for do tipo
        /// Objeto, faz a conversão com seu campo valor.
        /// </summary>
        /// <param name="value">object a converter.</param>
        /// <returns>retorna um bool.</returns>
        public static bool  ToBoolean(object value)
        {
            if (value.GetType() == typeof(Objeto))
            {
                Objeto obj = (Objeto)value;
                return (bool)obj.valor;
            }
            else
            {
                return (bool)value;
            }
            
        }



        /// <summary>
        /// converte um int para um string. Se o value for do tipo Objeto,
        /// convert para string o camppo Objeto.valor.
        /// </summary>
        /// <param name="value">valor a converter.</param>
        /// <returns></returns>
        public static string IntToText(object value)
        {
            if (value is int)
            {
                if (value == null)
                {
                    return null;
                }

                if (value.GetType() == typeof(Objeto))
                {
                    return ((Objeto)value).valor.ToString();
                }
                else
                {
                    return value.ToString();
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// converte um double para um string. Se value for
        /// do tipo [Objeto], converte com o campo [Objeto.valor].
        /// </summary>
        /// <param name="value">valor double a converter.</param>
        /// <returns>retorna um texto, se bem sucedido a conversão, null se não.</returns>
        public static string DoubleToText(object value)
        {
            if (value is double)
            {
                if (value == null)
                {
                    return null;
                }

                if (value.GetType() == typeof(Objeto))
                {
                    return ((Objeto)value).valor.ToString();
                }
                else
                {
                    return value.ToString();
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// converte um float para um string. Se o value for
        /// do tipo [Objeto], converte pelo campo [Objeto.valor].
        /// </summary>
        /// <param name="value">valor float a converter.</param>
        /// <returns>retorna um string se bem sucedido a conversao.</returns>
        public static string FloatToText(object value)
        {
            if (value is float)
            {
                if (value == null)
                {
                    return null;
                }

                if (value.GetType() == typeof(Objeto))
                {
                    return ((Objeto)value).valor.ToString();
                }
                else
                {
                    return value.ToString();
                }
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// converte um char para um string. Se valor for do tipo
        /// [Objeto], faz a conversao pelo campo [Objeto.valor].
        /// </summary>
        /// <param name="value">valor char a converter.</param>
        /// <returns></returns>
        public static string CharToText(object value)
        {
            if (value is char)
            {
                if (value == null)
                {
                    return null;
                }

                if (value.GetType() == typeof(Objeto))
                {
                    return ((Objeto)value).valor.ToString();
                }
                else
                {
                    return value.ToString();
                }
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// converte um bool para um strig. se valro for do tipo Objeto,
        /// converte o campo Objeto.valr.
        /// </summary>
        /// <param name="value">valor bool a converter para um string.</param>
        /// <returns></returns>
        public static string BoolToText(object value)
        {
            if (value is bool)
            {
                if (value == null)
                {
                    return null;
                }

                if (value.GetType() == typeof(Objeto))
                {
                    return ((Objeto)value).valor.ToString();
                }
                else
                {
                    return value.ToString();
                }
            }
            else
            {
                return null;
            }
        }


    }
}
