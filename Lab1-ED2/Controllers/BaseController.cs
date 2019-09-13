using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lab1_ED2.Helper;
using static Lab1_ED2.Helper.Alertas;

namespace Lab1_ED2.Controllers
{
    public class BaseController : Controller
    {
        public void Success(string message, bool dismissable = false)
        {
            AddAlert(AlertasStyles.Success, message, dismissable);
        }
        public void Information(string message, bool dismissable = false)
        {
            AddAlert(AlertasStyles.Information, message, dismissable);
        }
        public void Danger(string message, bool dismissable = false)
        {
            AddAlert(AlertasStyles.Danger, message, dismissable);
        }
        private void AddAlert(string alertStyle, string message, bool dismissable)
        {
            var alerts = TempData.ContainsKey(Alertas.TempDataKey)
                ? (List<Alertas>)TempData[Alertas.TempDataKey]
                : new List<Alertas>();

            alerts.Add(new Alertas
            {
                AlertStyle = alertStyle,
                Message = message,
                Dismissable = dismissable
            });
            TempData[Alertas.TempDataKey] = alerts;
        }

    }
}