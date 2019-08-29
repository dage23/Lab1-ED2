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
        public string indice;
    }

}