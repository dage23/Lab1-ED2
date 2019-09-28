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
        public Nodo cNodoRaiz;
        public Dictionary<string, byte> DiccionarioIndices = new Dictionary<string, byte>();
        public int TotalDeCaracteres = 0;
        public string TextoEnBinario = string.Empty;
        public List<Caracter> ListaCaracteresFinales = new List<Caracter>();
        public List<Nodo> ListaNodosArbol = new List<Nodo>();
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

        public string ArmarArbolHuffman(List<Caracter> ListaCaracteresExistentes)
        {
            ListaCaracteresExistentes[0].Frecuencia = 1;
            ListaCaracteresExistentes[0].CaracterAUsar = true;
            if (ListaCaracteresExistentes.Count > 1)
            {
                for (int i = 0; i < ListaCaracteresExistentes.Count; i++)
                {
                    for (int j = 1; j < ListaCaracteresExistentes.Count; j++)
                    {
                        if (ListaCaracteresExistentes[i].CaracterTexto == ListaCaracteresExistentes[j].CaracterTexto && !ListaCaracteresExistentes[j].CaracterYaRecorrido)
                        {
                            ListaCaracteresExistentes[i].Frecuencia += 1;
                            ListaCaracteresExistentes[i].CaracterAUsar = true;
                            ListaCaracteresExistentes[j].CaracterYaRecorrido = true;
                        }

                    }
                }
            }
            for (int i = 0; i < ListaCaracteresExistentes.Count; i++)
            {
                if (ListaCaracteresExistentes[i].CaracterAUsar)
                {
                    ListaCaracteresFinales.Add(ListaCaracteresExistentes[i]);
                }
            }

            int CantTotalCaracteres = ListaCaracteresFinales.Count;
            for (int i = 0; i < CantTotalCaracteres; i++)
            {
                var NodoAux = new Nodo
                {
                    caracter = ListaCaracteresFinales.ElementAt(i)
                };
                NodoAux.probabilidad = Convert.ToDouble(NodoAux.caracter.Frecuencia) / (TotalDeCaracteres);
                ListaNodosArbol.Add(NodoAux);
            }
            var MetodoCopara = new Comparar();
            //ordena la lista de mayor a menor
            int TamanoLista = ListaNodosArbol.Count;
            while (ListaNodosArbol.Count() > 1)
            {
                ListaNodosArbol.Sort(MetodoCopara);
                var auxPadre = new Nodo();
                Nodo auxIzq = ListaNodosArbol[TamanoLista - 1];
                Nodo auxDcha = ListaNodosArbol[TamanoLista - 2];
                auxPadre.probabilidad = Convert.ToDouble(auxIzq.probabilidad) + Convert.ToDouble(auxDcha.probabilidad);
                auxPadre.NodoHijoDcha = auxDcha;
                auxPadre.NodoHijoIzq = auxIzq;
                auxPadre.NodoHijoIzq.NodoPadre = auxPadre;
                auxPadre.NodoHijoDcha.NodoPadre = auxPadre;

                ListaNodosArbol[TamanoLista - 2] = auxPadre;
                ListaNodosArbol.RemoveAt(TamanoLista - 1);
                TamanoLista = ListaNodosArbol.Count;
            }
            cNodoRaiz = ListaNodosArbol[0];
            cNodoRaiz.enOrden(cNodoRaiz);
            CrearDiccionario(cNodoRaiz);
            for (int i = 0; i < ListaCaracteresExistentes.Count; i++)
            {
                foreach (var item in DiccionarioIndices)
                {
                    if (item.Value == ListaCaracteresExistentes[i].CaracterTexto)
                    {
                        TextoEnBinario += item.Key;
                        ListaCaracteresExistentes[i].binarioText = item.Key;
                    }
                }
            }
            return TextoEnBinario;
        }
        public void CrearDiccionario(Nodo nNodo)
        {
            if (nNodo != null)
            {
                if (nNodo.NodoHijoIzq == null)
                {
                    DiccionarioIndices.Add(nNodo.indice, nNodo.caracter.CaracterTexto);
                }
                CrearDiccionario(nNodo.NodoHijoIzq);
                CrearDiccionario(nNodo.NodoHijoDcha);
            }
        }


    }
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

}