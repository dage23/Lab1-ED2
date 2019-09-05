using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lab1_ED2.Models
{
    public class Caracter:IComparable
    {
        public char CaracterTexto { get; set; }
        public int Frecuencia { get; set; }
        public int indice { get; set; }
        public bool Recorrido = false;
        public bool Tomar = false;
        public string binarioText { get; set; }
        public int CompareTo(object obj)
        {
            var vComparador = (Caracter)obj;
            return CaracterTexto.CompareTo(vComparador.CaracterTexto);
        }
    }
}