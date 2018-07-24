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

        public static int defaultWindowWidth, defaultWindowHeight;

        public static int renderWidth, renderHeight;

        public static FractalType selectedFractalType;
        public static DrawingMode selectedDrawingMode;

        public static int maxIterations;

        public static char csvSeparator;

        public static void Load()
        {

            defaultWindowWidth = Convert.ToInt32(ConfigurationManager.AppSettings.Get("defaultWindowWidth"));
            defaultWindowHeight = Convert.ToInt32(ConfigurationManager.AppSettings.Get("defaultWindowHeight")); 

            renderWidth = defaultWindowWidth;
            renderHeight = defaultWindowHeight - 20;

            selectedFractalType = (string) Properties.Settings.Default["selectedFractalType"] != ""
                ? (FractalType)Convert.ToInt32(Properties.Settings.Default["selectedFractalType"])
                : (FractalType)Convert.ToInt32(ConfigurationManager.AppSettings.Get("defaultSelectedFractalType"));

            selectedDrawingMode = (string)Properties.Settings.Default["selectedDrawingMode"] != ""
                ? (DrawingMode)Convert.ToInt32(Properties.Settings.Default["selectedDrawingMode"])
                : (DrawingMode)Convert.ToInt32(ConfigurationManager.AppSettings.Get("defaultSelectedDrawingMode"));

            maxIterations = Convert.ToInt32(ConfigurationManager.AppSettings.Get("maxIterations"));

            csvSeparator = ConfigurationManager.AppSettings.Get("csvSeparator").Length > 0 
                ? ConfigurationManager.AppSettings.Get("csvSeparator")[0]
                : ';';

            Save();

        }

        public static void Save()
        {

            Properties.Settings.Default["selectedFractalType"] = ((int)selectedFractalType).ToString();
            Properties.Settings.Default["selectedDrawingMode"] = ((int)selectedDrawingMode).ToString();

            Properties.Settings.Default.Save();

        }

    }
}
