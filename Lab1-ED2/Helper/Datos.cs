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
}