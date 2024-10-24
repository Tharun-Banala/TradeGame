using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serverportal.Models
{
    public class Properties
    {

        public int PropertyID { get; set; }   // Primary Key
        public string PropertyName { get; set; }   // Property name with a max length of 20 characters
        public int ColorId { get; set; }   // Foreign Key to the colors table
        public int OwnerId { get; set; }   // Foreign Key to the BusinessGamePlayers table
        public double Mortage { get; set; }   // Mortgage value
        public double Rent { get; set; }   // Rent value
        public double RentHouse1 { get; set; }   // Rent for house 1
        public double RentHouse2 { get; set; }   // Rent for house 2
        public double RentHouse3 { get; set; }   // Rent for house 3
        public Properties(){


        }
    }
}
