using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPClienteConsola
{
    class Program
    {
        

        static void Main(string[] args)
        {

            string error = "";
            string mensaje = "";
            //string host = "132.1.2.42";
            string host = "132.1.2.42";
            string localIP = "";
            int puerto = 8075;
            bool cifrado  = false;
            int bytesLeidos;
            int bytesALeer = 512;
            //string host = "127.0.0.1";
            //Esta cadena tiene un caracter "0" al inicio
            string hex = "00:44:49:53:4F:30:30:36:30:30:30:30:30:30:30:38:30:30:38:32:32:30:30:30:30:30:30:30:30:30:30:30:30:30:30:34:30:30:30:30:30:30:30:30:30:30:30:30:30:30:31:32:32:37:31:37:30:32:35:33:30:30:30:30:30:31:30:30:31:03";
            string data = "";
            byte[] b = TCPCliente.TCPCliente.HexsToBytes(hex);

            data = TCPCliente.TCPCliente.BytesToHex(b);

            data = Encoding.UTF8.GetString(b);
            //Este es la cadena obtenida
            //"\0DISO0060000000800822000000000000004000000000000001227170253000001001\3"
            
            Console.WriteLine(string.Format("Conectando. version [{0}]...", TCPCliente.TCPCliente.version() ));
            if (TCPCliente.TCPCliente.open( host, puerto, cifrado, localIP, out error) == 0 )
                Console.WriteLine("OK conectado");
            else
                Console.WriteLine(error);

            while( true ){

                Console.Title = " Conectado? " + (TCPCliente.TCPCliente._connected() == 0 ? "SI" : "NO") ;

                bytesLeidos = TCPCliente.TCPCliente.read(bytesALeer, out mensaje, out error);

                if (mensaje != "")
                    Console.WriteLine(DateTime.Now.ToString() + "-" + TCPCliente.TCPCliente.status() + " > " + mensaje);
                else
                    Console.WriteLine(DateTime.Now.ToString() + "-" + TCPCliente.TCPCliente.status() + " <vacio> ");

                //TCPCliente.TCPCliente.write("hola mundo", out error);
                
                if (error != ""){
                    Console.WriteLine(DateTime.Now.ToString() + "-" + TCPCliente.TCPCliente.status() + " ! " + error );
                    TCPCliente.TCPCliente.open(host, puerto, cifrado, localIP, out error);
                }
                
                System.Threading.Thread.Sleep(1000);

                TCPCliente.TCPCliente.keepAlive();
                
            }
            
            Console.Read();

        }
    }
}
