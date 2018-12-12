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
                    return ALogin(request.Content);
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
        public static HttpResponse ARegister(String data)
        {
            char[] delimiterChars = { '=', '&' };
            string[] words = data.Split(delimiterChars);
            bool status = false;//true - успех 
            //INSERT INTO registred values(default,'myname221','12345',2);
            Console.WriteLine("Login: {0}, Pass: {1}", words[1], words[3]);
            string hash = HashMD5(words[3]);
      
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
             response.Headers.Add("Set-Cookie", "Name=" + Base64Encode(words[1]));
            return response;
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static HttpResponse ALogin(String data)
        {
            char[] delimiterChars = { '=', '&' };
            string[] words = data.Split(delimiterChars);
            bool status = false;//true - успех 
            Console.WriteLine("Login: {0}, Pass: {1}", words[1], words[3]);
            string result="";
            //Работа с  базой
            try
            {
               
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT  password_ FROM registred values WHERE login LIKE '"+ words[1] + "';", conn))
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                        {
                            result = reader.GetString(0);
                        }
                    Console.WriteLine(result);
                    conn.Close();
                }
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (result == "")
                status = false;
            else if(HashMD5(words[3]) == result)
                status = true;
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
            if (status)
                response.Headers.Add("Set-Cookie","Name="+ Base64Encode(words[1]));
            return response;
        }
        public static string HashMD5(String data)
        {
            string hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(data);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                hash = sb.ToString();
            }
            return hash;
        }

    }
}
