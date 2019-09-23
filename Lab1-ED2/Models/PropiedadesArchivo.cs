using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Lab1_ED2.Models
{
    public class PropiedadesArchivo
    {
        [Display(Name = "Nombre Archivo Original")]
        public string NombreArchivoOriginal { get; set; }
        [Display(Name = "Tamano Archivo Original")]
        public double TamanoArchivoDescomprimido { get; set; }
        [Display(Name = "Tamano Archivo Comprimido")]
        public double TamanoArchivoComprimido { get; set; }
        [Display(Name = "Razon de Compresion")]
        public double RazonCompresion { get; set; }
        [Display(Name = "Factor de Compresion")]
        public double FactorCompresion { get; set; }
        [Display(Name = "Porcentaje de Reduccion")]
        public string PorcentajeReduccion { get; set; }
        [Display(Name ="Formato de Compresion")]
        public string FormatoCompresion { get; set; }
    }
}