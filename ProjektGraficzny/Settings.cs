using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektGraficzny
{
    public static class Settings
    {

        public static int renderWidth, renderHeight;
        public static int fractalChoice;

        public static void Load()
        {

            renderWidth = Convert.ToInt32(ConfigurationManager.AppSettings.Get("renderWidth"));
            renderHeight = Convert.ToInt32(ConfigurationManager.AppSettings.Get("renderHeight"));
            fractalChoice = Convert.ToInt32(ConfigurationManager.AppSettings.Get("fractalChoice"));

        }

        public static void Save()
        {

            ConfigurationManager.AppSettings.Set("renderWidth", "" + renderWidth);
            ConfigurationManager.AppSettings.Set("renderHeight", "" + renderHeight);
            ConfigurationManager.AppSettings.Set("fractalChoice", "" + fractalChoice);

        }

    }
}
