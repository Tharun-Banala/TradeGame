using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;



namespace serverportal.Models
{
    public class Message
    {
        public string MessageType { get; set; }
        public string message { get; set; }
        public List<Point> points { get; set; }
        public Message(string type, string msg)
        {
            MessageType = type;
            message = msg;
            points = new List<Point>();
        }
        public Message()
        {

        }
    }
}
