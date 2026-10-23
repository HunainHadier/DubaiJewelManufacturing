using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DubaiJewelManufacturing
{
    public class DatabaseHelper
    {
        private string _connectionString;

        public DatabaseHelper()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MyDb"].ConnectionString;
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }



    }
}
