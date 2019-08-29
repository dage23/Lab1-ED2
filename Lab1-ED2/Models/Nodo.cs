using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lab1_ED2.Models
{
    public class Nodo
    {
        public Caracter caracter { get; set; }
        public double probabilidad { get; set; }
        public Nodo NodoPadre;
        public Nodo NodoHijoDcha;
        public Nodo NodoHijoIzq;
        public int indice;
    }
    class Comparar : IComparer<Nodo> // clase para ordenar la lista de nodos
    {
        public int Compare(Nodo N1, Nodo N2)
        {
            double x = N1.probabilidad;
            double y = N2.probabilidad;
            if (x == 0 || y == 0)
            {
                return 0;
            }

            // CompareTo() method 
            return y.CompareTo(x);

        }
    }
}