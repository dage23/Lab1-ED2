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
        public void enOrden(Nodo nNodo)
        {
            if (nNodo != null)
            {
                if (nNodo.NodoPadre != null)
                {
                    var A = "0";
                    if (nNodo == nNodo.NodoPadre.NodoHijoDcha)
                    {
                        A = "1";
                    }
                    if (nNodo.NodoPadre.NodoPadre != null)
                    {
                        nNodo.indice = nNodo.NodoPadre.indice + A;
                    }
                    else nNodo.indice = A;
                }
                enOrden(nNodo.NodoHijoIzq);

                if (nNodo.NodoPadre != null)
                {
                    var A = "0";
                    if (nNodo == nNodo.NodoPadre.NodoHijoDcha)
                    {
                        A = "1";
                    }
                    if (nNodo.NodoPadre.NodoPadre != null)
                    {
                        nNodo.indice = nNodo.NodoPadre.indice + A;
                    }
                    else nNodo.indice = A;
                }
                enOrden(nNodo.NodoHijoDcha);

            }
        }
    }

}