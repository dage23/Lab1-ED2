using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lab1_ED2.Models;
namespace Lab1_ED2.Helper
{
    public class Datos
    {
       
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
}