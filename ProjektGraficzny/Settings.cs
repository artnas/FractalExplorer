using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fractals;

namespace ProjektGraficzny
{
    public static class Settings
    {

        public static int renderWidth, renderHeight;

        public static FractalType selectedFractalType;
        public static DrawingMode selectedDrawingMode;

        public static void Load()
        {

            renderWidth = Convert.ToInt32(ConfigurationManager.AppSettings.Get("renderWidth"));
            renderHeight = Convert.ToInt32(ConfigurationManager.AppSettings.Get("renderHeight"));
            selectedFractalType = (FractalType)Convert.ToInt32(ConfigurationManager.AppSettings.Get("selectedFractalType"));
            selectedDrawingMode = (DrawingMode)Convert.ToInt32(ConfigurationManager.AppSettings.Get("selectedDrawingMode"));

        }

        public static void Save()
        {

            ConfigurationManager.AppSettings.Set("renderWidth", "" + renderWidth);
            ConfigurationManager.AppSettings.Set("renderHeight", "" + renderHeight);
            ConfigurationManager.AppSettings.Set("selectedFractalType", "" + (int)selectedFractalType);
            ConfigurationManager.AppSettings.Set("selectedDrawingMode", "" + (int)selectedDrawingMode);

        }

    }
}
