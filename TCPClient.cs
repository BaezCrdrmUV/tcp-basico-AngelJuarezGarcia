using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace tcp_com
{
    public class TCPClient
    {
        TcpClient client;
        string IP;
        int Port;
        string Username;
        bool chatActivo;

        public TCPClient(string ip, int port, string username)
        {
            try
            {
                client = new TcpClient();
                this.IP = ip;
                this.Port = port;
                this.Username = username;
            }
            catch (System.Exception)
            {
                
            }
        }
        public void Chat()
        {
            chatActivo = true;
            int idMensaje = 0;   
            client.Connect(IP, Port);

            Console.WriteLine("Conectado a la IP: " + IP + " en el puerto: " + Port.ToString());
            Console.WriteLine("\nSigue las instrucciones o envia el mensaje que desees");
            Console.WriteLine("1.- Escribe 'historial'  para mostrar el historial de tus mensajes");
            Console.WriteLine("2.- Escribe 'edit' para editar un mensaje");
            Console.WriteLine("3.- Escribe 'borrar' para borrar un mensaje");
            Console.WriteLine("4.- Escribe 'bye' para terminar el programa\n");

            while(chatActivo)
            {
                string opcionSeleccionada = Console.ReadLine();

                try
                {
                    if (opcionSeleccionada.Equals("historial"))
                    {
                            string jsonMessage = seriarlizarMensaje(idMensaje, opcionSeleccionada,Username);

                            var stream = client.GetStream();
                            byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
                            Console.WriteLine("Cargando mensajes...");
                            stream.Write(data, 0, data.Length);

                            byte[] package = new byte[1024];
                            stream.Read(package);
                            var asString = Encoding.Unicode.GetString(package);
                            List<Message> listaDeMensajes = new List<Message>();
                            listaDeMensajes = JsonConvert.DeserializeObject<List<Message>>(asString);
                            foreach (Message mensaje in listaDeMensajes)
                            {
                                Console.WriteLine(mensaje.id +" -> "+ mensaje.MessageString);
                            }
                            Console.WriteLine("Estos fueron tus ultimos mensajes");
                        
                    }else if(opcionSeleccionada.Equals("edit"))
                    {
                        string jsonMessage = seriarlizarMensaje(idMensaje,opcionSeleccionada,Username);
                    
                        var stream = client.GetStream();
                        byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
                        stream.Write(data, 0, data.Length);

                        Console.WriteLine("Cual es el ID del mensaje a editar?");
                        int id = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Ingrese el nuevo texto del mensaje");
                        String mensajeNuevo = Console.ReadLine();

                        string jsonMessageEditado = seriarlizarMensaje(id,mensajeNuevo, Username);

                        var stream2 = client.GetStream();
                        byte[] datae = Encoding.UTF8.GetBytes(jsonMessageEditado);
                        Console.WriteLine("Editando mensaje...");
                        stream2.Write(datae, 0, datae.Length);

                        // Recepción de mensajes
                        byte[] package = new byte[1024];
                        stream2.Read(package);
                        string mensajeDelServidor = Encoding.UTF8.GetString(package);
                        Console.WriteLine(mensajeDelServidor);
                        
                    }else if(opcionSeleccionada.Equals("borrar"))
                    {
                        string jsonMessage = seriarlizarMensaje(idMensaje,opcionSeleccionada,Username);
                        // Envío de datos
                        var stream = client.GetStream();
                        byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
                        stream.Write(data, 0, data.Length);

                        Console.WriteLine("Cual es el ID del mensaje a editar?");
                        int id = Convert.ToInt32(Console.ReadLine());
                        string jsonMessageToDelete = seriarlizarMensaje(id,"", Username);
                        
                        // Envío de datos
                        var streame = client.GetStream();
                        byte[] datae = Encoding.UTF8.GetBytes(jsonMessageToDelete);
                        Console.WriteLine("Eliminando mensaje...");
                        streame.Write(datae, 0, datae.Length);

                        // Recepción de mensajes
                        byte[] package = new byte[1024];
                        streame.Read(package);
                        string mensajeDelServidor = Encoding.UTF8.GetString(package);
                        Console.WriteLine(mensajeDelServidor);
                        
                    }else if(opcionSeleccionada.Equals("bye"))
                    {
                        string jsonMessage = seriarlizarMensaje(idMensaje, opcionSeleccionada,Username);

                        var stream = client.GetStream();
                        byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
                        Console.WriteLine("Saliendo del chat...");
                        stream.Write(data, 0, data.Length);
                        chatActivo = false;
                        Console.WriteLine("Chat finalizado");
                        
                    }else{
                        idMensaje++;
                        string jsonMessage = seriarlizarMensaje(idMensaje, opcionSeleccionada,Username);

                        // Envío de datos
                        var stream = client.GetStream();
                        byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
                        Console.WriteLine("Enviando datos...");
                        stream.Write(data, 0, data.Length);

                        // Recepción de mensajes
                        byte[] package = new byte[1024];
                        stream.Read(package);
                        string serverMessage = Encoding.UTF8.GetString(package);
                        Console.WriteLine(serverMessage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error {0}", ex.Message);
                }
            }
        }

        private string seriarlizarMensaje(int idMensaje, string mensaje, string usuraio){
            Message newMessage = new Message(idMensaje ,mensaje, usuraio);
            return JsonConvert.SerializeObject(newMessage);
        }
    }
}