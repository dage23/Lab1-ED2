using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lab1_ED2.Models;
namespace Lab1_ED2.Helper
{
    public class Datos
    {
        private static Datos _instance = null;
        public static Datos Instance
        {
            get
            {
                if (_instance == null) _instance = new Datos();
                {
                    return _instance;
                }
            }
        }
        //public List<Caracter> ListaCaracteresExistentes = new List<Caracter>();
        public class Comparar : IComparer<Nodo> // clase para ordenar la lista de nodos
        {
            public int Compare(Nodo N1, Nodo N2)
            {
                double x = N1.probabilidad;
                double y = N2.probabilidad;
                if (x == 0 || y == 0)
                {
                    return 0;
                }
                return y.CompareTo(x);

            }
        }
        //public Nodo cNodo;
        //public Nodo cNodoRaiz;
        public void enOrden(Nodo nNodo)
        {
            if (nNodo != null)
            {
                if (nNodo.NodoPadre != null)
                {
                    if (nNodo.NodoPadre.NodoPadre != null)
                    {
                        nNodo.indice = nNodo.NodoPadre.indice + "0";
                    }
                    else nNodo.indice = "0";
                }
                enOrden(nNodo.NodoHijoIzq);

                if (nNodo.NodoPadre != null)
                {
                    if (nNodo.NodoPadre.NodoPadre != null)
                    {
                        nNodo.indice = nNodo.NodoPadre.indice + "1";
                    }
                    else nNodo.indice = "1";
                }
                enOrden(nNodo.NodoHijoDcha);
                
            }
        }
        public Dictionary<string, char> DiccionarioIndices = new Dictionary<string, char>();
    }
}