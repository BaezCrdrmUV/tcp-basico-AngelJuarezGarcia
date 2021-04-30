using System;
namespace tcp_com
{
    public class Message
    {
        public string MessageString { get; set; }
        public string User { get; set; }
        public string Hora { get; set; }
        public int id { get; set; }

        public Message()
        {
            MessageString = "";
            User = "Default";
        }

        public Message(int id,string messageString, string user)
        {
            this.id = id;
            this.MessageString = messageString;
            this.User = user;
            this.Hora = DateTime.Now.ToString("h:mm tt");
        }
    }
}