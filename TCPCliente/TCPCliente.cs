using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RGiesecke.DllExport;
using System.Runtime.InteropServices;

using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;

using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace TCPCliente
{
    public class TCPCliente
    {

        static TcpClient tcpClient = null;
        static NetworkStream netStream = null ;
        static SslStream sslStream;
        static string _host = "";
        static int _puerto = 0;
        static bool _cifrado = false;
                
        static int MAJOR_VERSION   = 1;
        static int MINOR_VERSION   = 1;
        static int BUILD_NUMBER    = 0;
        static int REVISION        = 7;

        static string STATUS_UNDEF          = "_UNDEF_";
        static string STATUS_CONECTANDO     = "CONECTANDO";
        static string STATUS_CONECTADO      = "CONECTADO";
        static string STATUS_CERRANDO       = "CERRANDO";
        static string STATUS_DESCONECTADO   = "DESCONECTADO";

        static string _status = STATUS_UNDEF;

        static string CadenaKeepAlive = "00:03";
        
        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        //Manda un byte 0, si existe algun error la propiedad "connected" sera actualizada
        public static void keepAlive()
        {
            try
            {
                byte[] data = HexsToBytes(CadenaKeepAlive);

                if (!_cifrado)
                    netStream.Write(data, 0, data.Length);
                else
                    sslStream.Write(data, 0, data.Length);

            } catch (Exception e)
            {
                
            }

        }

        public static string BytesToHex(byte[] data){
        
            string result = "";

            result = BitConverter.ToString(data);
            
            return result.Replace('-', ':');

        }

        public static byte[] HexsToBytes(string cadenaHexadecimal)
        {

            
            string[] hexs = cadenaHexadecimal.Split(':');
            byte[] result = new byte[hexs.Length];
            int i = 0;
            foreach (string item in hexs)
            {
                result[i] = Convert.ToByte(value: item, fromBase: 16);
                i++;
            }

            return result;                                
        }

        [DllExport("open", CallingConvention = CallingConvention.Cdecl)]
        public static int _open(IntPtr host_P, int puerto, bool cifrado, IntPtr localIP_P, [MarshalAs(UnmanagedType.BStr)] out string error)
        {
            string host = Marshal.PtrToStringAuto(host_P);
            string localIP = Marshal.PtrToStringAuto(localIP_P);

            return open(host, puerto, cifrado, localIP, out error);

        }
        
        public static int open(String host, int puerto, bool cifrado, string localIP,  out string error)
        {
            error = "";
            _status = STATUS_DESCONECTADO;
            int result = -1;
            try
            {

                _cifrado = cifrado;
                
                if (_cifrado)
                {
                    ServicePointManager.SecurityProtocol = 
                                                SecurityProtocolType.Ssl3   |
                                                SecurityProtocolType.Tls    |
                                                SecurityProtocolType.Tls11  |
                                                SecurityProtocolType.Tls12;
                                                                    
                }
                
                _status = STATUS_CONECTANDO;
                _host = host;
                _puerto = puerto;

                //NO se define una IPLocal origen, que decida el SO
                if (localIP =="")
                    tcpClient = new TcpClient(host, puerto);
                else{//Definieron salir por una IP origen en especifico
                    IPEndPoint ipEndPoint =  new IPEndPoint(  IPAddress.Parse(localIP), 0);
                    tcpClient = new TcpClient(ipEndPoint);
                    tcpClient.Connect(host, puerto);

                }

                
                netStream = tcpClient.GetStream();

                if (_cifrado)
                {
                    sslStream = new SslStream(netStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

                    sslStream.AuthenticateAsClient("", null, (SslProtocols)ServicePointManager.SecurityProtocol, false);
                }

                _status = STATUS_CONECTADO ;
                result = 0;
            }
            catch (Exception e)
            {
                error = e.Message;
            }
            return result;

        }

        [DllExport("status", CallingConvention = CallingConvention.Cdecl)]
        public static void status( [MarshalAs(UnmanagedType.BStr)] out string status)
        {
            status = _status;
        }

        public static string status()
        {
            return _status;
        }

        [DllExport("connected", CallingConvention = CallingConvention.Cdecl)]
        public static int _connected()
        {
            int result = -1;
            if (tcpClient == null)
                return result;

            //esta propiedad regresa el ultimo valor obtenido, y no se actualiza al perder la conexion
            result  = tcpClient.Connected? 0 : -1;

            if (result != 0)
                _status = STATUS_DESCONECTADO;

            return result;
        }

        public static bool connected(){
            return _connected() == 0;
        }

        [DllExport("canRead", CallingConvention = CallingConvention.Cdecl)]
        public static bool canRead()
        {
            if (netStream == null)
                return false;

            return netStream.CanRead;
        }

        [DllExport("canWrite", CallingConvention = CallingConvention.Cdecl)]
        public static bool canWrite()
        {
            if (netStream == null)
                return false;

            return netStream.CanWrite;
        }
        
        [DllExport("close", CallingConvention = CallingConvention.Cdecl)]        
        public static void close(){

            _status = STATUS_CERRANDO;
            
            try{

                netStream.Close();
               
            }catch(Exception e){
            
            }

            try{
                tcpClient.Close();
            }catch(Exception e){
                                
            }

            _status = STATUS_DESCONECTADO;
        
        }

        [DllExport("version", CallingConvention = CallingConvention.Cdecl)]
        public static void _version([MarshalAs(UnmanagedType.BStr)] out string result)
        {
            result = version();
        }
        
        public static string version(){
            return string.Format("{0}.{1}.{2}.{3}", MAJOR_VERSION, MINOR_VERSION, BUILD_NUMBER, REVISION);                                     
        }

        [DllExport("read", CallingConvention = CallingConvention.Cdecl)]
        public static int read(int nBytes, [MarshalAs(UnmanagedType.BStr)] out string data, [MarshalAs(UnmanagedType.BStr)]  out string error)
        {

            error = "";
            int bytesLeidos = -1;
            byte[] bytes;
            data = "";
            StringBuilder aData = new StringBuilder();

            try
            {

                bytes = readB(nBytes, out bytesLeidos, out error);

                //aData.AppendFormat("{0}", Encoding.ASCII.GetString(bytes, 0, bytesLeidos));
                //Default es para valores ANSI
                aData.AppendFormat("{0}", Encoding.Default.GetString(bytes, 0, bytesLeidos));

                data = aData.ToString();

            }
            catch (Exception e)
            {
                error = e.Message;
            }

            return bytesLeidos;

        }

        [DllExport("readHex", CallingConvention = CallingConvention.Cdecl)]
        public static int _readHex(int nBytes, [MarshalAs(UnmanagedType.BStr)] out string data, [MarshalAs(UnmanagedType.BStr)]  out string error)
        {

            error = "";
            int bytesLeidos = -1;
            byte[] bytes;
            data = "";
            StringBuilder aData = new StringBuilder();

            try
            {
                bytes = readB(nBytes, out bytesLeidos, out error);

                data = BitConverter.ToString(bytes).Replace('-', ':');
                
            }
            catch (Exception e)
            {
                error = e.Message;
            }

            return bytesLeidos;

        }

        public static byte[] readB(int nBytes, out int bytesLeidos, out string error)
        {

            byte[] result = new byte[nBytes];
            bytesLeidos = 0;
            error = "";
            try
            {
                if (netStream.DataAvailable && tcpClient.Connected)
                    bytesLeidos = (!_cifrado ? netStream.Read(result, 0, result.Length) : sslStream.Read(result, 0, result.Length));

                Array.Resize<byte>(ref result, bytesLeidos);
                                    
            }
            catch (Exception e)
            {
                error = e.Message;
            }

            return result;

        }

        [DllExport("write", CallingConvention = CallingConvention.Cdecl)]
        public static int _write(IntPtr data_P, [MarshalAs(UnmanagedType.BStr)] out string error)
        {
            error = "";
            string data = Marshal.PtrToStringAuto(data_P);
            return write(data, out error);
        }

        //Espero una cadena hexadecimal separada por ":"
        //00:44:49
        [DllExport("writeHex", CallingConvention = CallingConvention.Cdecl)]
        public static int _writeHex(IntPtr dataHex_P, [MarshalAs(UnmanagedType.BStr)] out string error)
        {
            error = "";
            string data = Marshal.PtrToStringAuto(dataHex_P);
            int result = -1;
            try{
                byte[] b = HexsToBytes(data);
                result =  writeB(b, out error);
            }
            catch (Exception e)
            {
                error = e.Message;            
            }

            return result;
            
        }
        
        public static int write(string data, [MarshalAs(UnmanagedType.BStr)] out string error)
        {

            //Byte[] bytes = System.Text.Encoding.ASCII.GetBytes(data);
            //Default es para valores ANSI
            Byte[] bytes = System.Text.Encoding.Default.GetBytes(data);
            return writeB(bytes, out error);
        }

        public static int writeB( byte[] data, out string error){

            error = "";
            int result = -1;
            try{
                if (!_cifrado)
                    netStream.Write(data, 0, data.Length);                    
                else
                    sslStream.Write(data, 0, data.Length);

                result = 0;
               
            }catch(Exception e){
                error = e.Message;
            }

            return result;
            
        }

    }
}
