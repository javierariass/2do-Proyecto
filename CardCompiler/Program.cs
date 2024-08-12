using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CardCompiler
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
           
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new IDE());
           /* List<Token> tokens = new List<Token>();
            SintaxBack process = new SintaxBack();
            // string lineas = "bool var = false;\n bool var2 = true;\nint a = 17;\nint b = 17.3;\ndouble var = 56.3;\n string Raza = 'Elfos negros 2'; \nstring Nombre = 'EL Diego'; \nstring Type= 'Oro'; "; 
            string lineas = "string variable = 'azul' @@ 'Marino';";
            Console.WriteLine(lineas + "\n\n");
            process.VerifcateSintax(lineas);
           */
        }
        
    }
}
