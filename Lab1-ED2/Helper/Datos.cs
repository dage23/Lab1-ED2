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
        public List<Caracter> ListaCaracteresExistentes = new List<Caracter>();
    }
    public class ArbolH
    {
        private static ArbolH _instance = null;
        public static ArbolH Instance
        {
            get
            {
                if (_instance == null) _instance = new ArbolH();
                {
                    return _instance;
                }
            }
        }
        public List<Nodo> ListaNodosArbol = new List<Nodo>();
    }
    public class Nodo
    {
        public Caracter caracter { get; set; }
        public double probabilidad { get; set; }
        public Nodo NodoPadre;
        public Nodo NodoHijoDcha;
        public Nodo NodoHijoIzq;
        public string indice;
        private static Nodo _instance = null;
        public static Nodo Instance
        {
            get
            {
                if (_instance == null) _instance = new Nodo();
                {
                    return _instance;
                }
            }
        }
        public Nodo NodoRaiz = new Nodo();
        public void enOrden(Nodo nNodo)
        {
            if (nNodo != null)
            {
                enOrden(nNodo.NodoHijoIzq);
                if(nNodo.NodoPadre!=null)
                {
                    if (nNodo.NodoPadre.NodoPadre != null)
                    {
                        nNodo.indice = nNodo.NodoPadre.indice + "0";
                    }
                    else nNodo.indice = "0";
                }
                enOrden(nNodo.NodoHijoDcha);
                if (nNodo.NodoPadre != null)
                {
                    if (nNodo.NodoPadre.NodoPadre != null)
                    {
                        nNodo.indice = nNodo.NodoPadre.indice + "1";
                    }
                    else nNodo.indice = "1";
                }
            }
        }
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
            return y.CompareTo(x);

        }
    }
}