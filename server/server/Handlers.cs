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
        public static HttpResponse GetRequest(HttpRequest request)
        {
            switch (request.Url)
            {
                case "/api/login":
                    Console.WriteLine("login");
                    //ALogin(request.Content);
                    break;
                case "/api/register":
                    Console.WriteLine("register");
                    return ARegister(request.Content);
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }
            return new HttpResponse()
            {
                ReasonPhrase = "NotFound",
                StatusCode = "404"
            };

        }
        /*public static void BD_open()
        {
            String connectionString = "Server=localhost;Port=5432;User=postgres;Password=12345678;Database=Webapp;";
            npgSqlConnection = new NpgsqlConnection(connectionString);
            npgSqlConnection.Open();
            Console.WriteLine("Connect BD - ok");
        }*/
        public static HttpResponse ARegister(String data)
        {
            char[] delimiterChars = { '=', '&' };
            string[] words = data.Split(delimiterChars);
            bool status = false;//true - успех 
            //INSERT INTO registred values(default,'myname221','12345',2);
            Console.WriteLine("Login: {0}, Pass: {1}", words[1], words[3]);
            string hash;

            // Хеш md5
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
            //Добавление в базу
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO registred values(default,'" + words[1] + "', '" + hash + "',2);", conn);
                    if(cmd.ExecuteNonQuery() == 1)
                    {
                        status = true;
                    }
                    conn.Close();
                }
            }
            catch(Npgsql.NpgsqlException ex)
            {
                if (ex.Code == "23505")
                {
                    status = false; 
                }
                Console.WriteLine(ex.Message);
            }
            //Формирование запроса 
            string StatusCode;
            string StatusText;
            string StatusJson;
            if (status)
            {
                StatusCode = "200";
                StatusText = "Ok";
                StatusJson = "status:success";
            }
            else
            {
                StatusCode = "400";
                StatusText = "BadRequest";
                StatusJson = "status:fail";
            }
            HttpResponse response = new HttpResponse
            {
                ReasonPhrase = StatusText,
                StatusCode = StatusCode,
                ContentAsUTF8 = JsonConvert.SerializeObject(StatusJson)


            };
            if(status)
             response.Headers.Add("Set-Cookie", Base64Encode(words[1]));
            return response;
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

    }
}
