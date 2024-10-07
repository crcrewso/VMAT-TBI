using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;



[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script
    {
        public static ScriptContext context = null;
        public Script() { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext c/*, System.Windows.Window w, ScriptEnvironment environment*/)
        {
            context = c;
            string configFile = "";
            //grab the configuration file in the excuting assembly location
            if (File.Exists(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\configuration\\VMAT_TBI_config.ini")) configFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\configuration\\VMAT_TBI_config.ini";
            VMATTBIautoPlan.UI ui = new VMATTBIautoPlan.UI(context, configFile);
            Window wi = new Window();
            wi.Height = 735;
            wi.Width = 608;
            wi.Content = ui;
            wi.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            wi.ShowDialog();
            wi.SizeToContent = SizeToContent.WidthAndHeight;
        }

        public static ScriptContext GetScriptContext()
        { return context; }
    }
}
