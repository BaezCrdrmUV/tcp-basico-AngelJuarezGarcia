using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using Newtonsoft.Json;

namespace tcp_com
{
    public class TCPServer
    {
        public TcpListener listener { get; set; }
        public bool acceptFlag { get; set; }
        public List<Message> listaDeMensajes { get; set; }
        public List<int> listaDeIdsDeHilos { get; set; }
        public bool hayHilosAbiertos;

        public TCPServer(string ip, int port, bool start = false)
        {
            listaDeMensajes = new List<Message>();
            listaDeIdsDeHilos = new List<int>();
            hayHilosAbiertos = false;

            IPAddress address = IPAddress.Parse(ip);
            this.listener = new TcpListener(address, port);

            if(start == true)
            {
                listener.Start();
                Console.WriteLine("Servidor iniciado en la dirección {0}:{1}",
                    address.MapToIPv4().ToString(), port.ToString());
                acceptFlag = true;
            }
        }

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public void ObtenerHilosAbiertos()
        {
            while(true)
            {
                if(hayHilosAbiertos == true && listaDeIdsDeHilos.Count == 0)
                {
                    Console.WriteLine("Terminado");
                    listener.Stop();
                    listener = null;
                    break;
                }
            }
            Console.WriteLine("Opened messages");
            mostrarMensajes();
            Thread.CurrentThread.Join();
        }

        public void mostrarMensajes()
        {
            Console.WriteLine("Mensajes en servidor");
            foreach (Message mensaje in listaDeMensajes)
            {
                Console.WriteLine("{0} >> {1} : {2}", mensaje.id,  mensaje.User, mensaje.MessageString);
            }
        }

        public void HandleCommunication(Object obj)
        {
            ThreadParams param = (ThreadParams)obj;
            Socket client = param.obj;

            if(client != null)
            {
                Console.WriteLine("Cliente conectado. Esperando datos");
                string comando = "|";
                DateTime now  = DateTime.Now;
                Message mensaje = new Message();
                        

                while(mensaje != null && !mensaje.MessageString.Equals("bye"))
                {
                    try
                    {
                        switch(comando)
                        {
                            case "historial":
                            var asString = JsonConvert.SerializeObject(listaDeMensajes,SerializerSettings);
                            client.Send(Encoding.Unicode.GetBytes(asString));
                            break;

                            case "edit":
                                bool cambio = false;
                                byte[] buffer1 = new byte[1024];
                                client.Receive(buffer1);
                                var uft8Reader1 = new Utf8JsonReader(buffer1);
                                mensaje = System.Text.Json.JsonSerializer.Deserialize<Message>(ref uft8Reader1);

                                foreach(Message mensajeServidor in listaDeMensajes)
                                {
                                    if(mensaje.id == mensajeServidor.id)
                                    {
                                        mensajeServidor.MessageString = mensaje.MessageString;
                                        cambio = true;
                                        break;
                                    }
                                }
                                
                                if(cambio == true)
                                {
                                    byte[] data1 = Encoding.UTF8.GetBytes("Mensaje actualizado correctamente");
                                    client.Send(data1);
                                }
                                else
                                {
                                    byte[] data1 = Encoding.UTF8.GetBytes("No se encontro el mensaje buscado");
                                    client.Send(data1);
                                }
                            break;

                            case "borrar":
                                byte[] buffer2 = new byte[1024];
                                client.Receive(buffer2);
                                var uft8Reader2 = new Utf8JsonReader(buffer2);
                                mensaje = System.Text.Json.JsonSerializer.Deserialize<Message>(ref uft8Reader2);
                                listaDeMensajes.RemoveAt(mensaje.id - 1);
                                byte[] datos = Encoding.UTF8.GetBytes("Mensaje eliminado correctamente");
                                client.Send(datos);
                            break;

                            case "|":
                            break;
                            
                            default:
                                byte[] data = Encoding.UTF8.GetBytes("Mensaje Enviado");
                                client.Send(data);
                            break;
                        }

                        // Escucha por nuevos mensajes
                        byte[] buffer = new byte[1024];
                        client.Receive(buffer);
                        var uft8Reader = new Utf8JsonReader(buffer);
                        mensaje = System.Text.Json.JsonSerializer.Deserialize<Message>(ref uft8Reader);
                        comando = mensaje.MessageString;
                        if(comando != "historial" && comando != "bye" && comando != "edit" && comando != "borrar" )
                        {
                            Console.WriteLine(mensaje.Hora + "-" +mensaje.User+ ": "+ mensaje.MessageString);
                            listaDeMensajes.Add(mensaje);
                        }
                        
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                Console.WriteLine("Cerrando conexión");
                client.Dispose();
                foreach (var item in listaDeIdsDeHilos)
                {
                    Console.WriteLine(item);
                }

                Console.WriteLine("------------");

                listaDeIdsDeHilos.Remove(param.id);

                foreach (var item in listaDeIdsDeHilos)
                {
                    Console.WriteLine(item);
                }

                Thread.CurrentThread.Join();
            }
        }

        public void Listen()
        {
            if(listener != null && acceptFlag == true)
            {
                int id = 0;
                Thread watch = new Thread(new ThreadStart(ObtenerHilosAbiertos));
                watch.Start();

                while(true)
                {
                    Console.WriteLine("Esperando conexión del cliente...");

                    if(hayHilosAbiertos) break;
                    try
                    {
                        var clientSocket = listener.AcceptSocket();
                        Console.WriteLine("Cliente aceptado");

                        Thread thread = new Thread(new ParameterizedThreadStart(HandleCommunication));
                        thread.Start(new ThreadParams(clientSocket, id));
                        listaDeIdsDeHilos.Add(id);
                        id++;
                        hayHilosAbiertos = true;
                    }
                    catch (System.Exception)
                    {
                        
                    }

                }
            }
        }

        public class ThreadParams
        {
            public Socket obj { get; set; }
            public int id { get; set; }

                public ThreadParams(Socket obj, int id)
                {
                    this.obj = obj;
                    this.id = id;
                }
        }
    }
}