using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace server
{
    class Handlers
    {
        static String connString = "Server=localhost;Port=5432;User=postgres;Password=12345678;Database=Webapp;";
        public static void GetRequest(HttpRequest request)
        {
            switch (request.Url)
            {
                case "/api/login":
                    Console.WriteLine("login");
                    //ALogin(request.Content);
                    break;
                case "/api/register":
                    Console.WriteLine("register");
                    ARegister(request.Content);
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }
            
        }
        /*public static void BD_open()
        {
            String connectionString = "Server=localhost;Port=5432;User=postgres;Password=12345678;Database=Webapp;";
            npgSqlConnection = new NpgsqlConnection(connectionString);
            npgSqlConnection.Open();
            Console.WriteLine("Connect BD - ok");
        }*/
        public static void ARegister(String data)
        {
            char[] delimiterChars = { '=', '&' };
            string[] words = data.Split(delimiterChars);

            //INSERT INTO registred values(default,'myname221','12345',2);
            Console.WriteLine("Login: {0}, Pass: {1}", words[1], words[3]);
            string hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(words[3]);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                 hash = sb.ToString();
            }

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO registred values(default,'"+ words[1] +"', '"+ hash +"',2);", conn);
                cmd.ExecuteNonQuery();
            }
               
        }

    }
}
