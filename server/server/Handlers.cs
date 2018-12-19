using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;

namespace server
{
    class Handlers
    {
        static Dictionary<string, string> JSON = new Dictionary<string, string>
        {
            { "status", "fail" },
            { "cookie", " " }
        };
        static String connString = "Server=localhost;Port=5432;User=postgres;Password=12345678;Database=Webapp;";
        public static HttpResponse GetRequest(HttpRequest request)
        {
            switch (request.Url)
            {
                case "/api/login":
                    Console.WriteLine("login");
                    return ALogin(request);
                    break;
                case "/api/register":
                    Console.WriteLine("register");
                    return ARegister(request);
                    break;
                case "/api/places":
                    Console.WriteLine("places");
                    return APlaсes(request.Content);
                    break;
                case "/api/placesupdate":
                    Console.WriteLine("places");
                    //return APlaсesupdate(request.Content);
                    break;
                case "/api/events":
                    Console.WriteLine("places");
                    //return APlaсesupdate(request.Content);
                    break;
                /*case "/api/placesupdate":
                    Console.WriteLine("places");
                    return APlaсesupdate(request.Content);
                    break;*/
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
        public static HttpResponse ARegister(HttpRequest data)
        {
            char[] delimiterChars = { '=', '&' };
            string[] words = data.Content.Split(delimiterChars);
            bool status = false;//true - успех 
            Console.WriteLine("Login: {0}, Pass: {1}, name:{2}, e-mail:{3}, type:{4}", words[1], words[3], words[5], words[7], words[9]);
            string hash = HashMD5(words[3]);
            int result = -1;
            string sql_into_organizer = "INSERT INTO organizer values (@login, @password, @name, @email);";
            string sql_into_landlord = "INSERT INTO Landlord values (@login, @password, @name, @email);";
            string sql_check = "SELECT 1 FROM administrator WHERE NOT EXISTS(SELECT 1 FROM administrator WHERE admin_login = @login_t) and NOT EXISTS(SELECT 1 FROM organizer WHERE login_organizers = @login_t) and NOT EXISTS(SELECT 1 FROM Landlord WHERE login_landlords = @login_t);";
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();


                    NpgsqlCommand cmd = new NpgsqlCommand(sql_check, conn);
                    cmd.Parameters.AddWithValue("login_t", words[1]);
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                        {
                            result = reader.GetInt32(0);
                        }
                    Console.WriteLine(result);
                    if (result == 1)
                    {
                        if (words[9] == "0")
                        {
                            cmd = new NpgsqlCommand(sql_into_organizer, conn);
                        }
                        else
                        {
                            cmd = new NpgsqlCommand(sql_into_landlord, conn);
                        }
                        cmd.Parameters.AddWithValue("login", words[1]);
                        cmd.Parameters.AddWithValue("password", HashMD5(words[3]));
                        cmd.Parameters.AddWithValue("name", words[5]);
                        cmd.Parameters.AddWithValue("email", words[7]);
                    }
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        status = true;
                    }
                    conn.Close();
                }
            }
            catch (Npgsql.NpgsqlException ex)
            {
                status = false;
                Console.WriteLine(ex.Message);
            }
            //Формирование запроса 
            string StatusCode;
            string StatusText;
            if (status)
            {
                StatusCode = "200";
                StatusText = "Ok";
                JSON["status"] = "success";
                JSON["cookie"] = Base64Encode(words[9] + "&" + words[1]);
            }
            else
            {
                StatusCode = "400";
                StatusText = "BadRequest";
                JSON["status"] = "fail";
            }
            HttpResponse response = new HttpResponse
            {
                ReasonPhrase = StatusText,
                StatusCode = StatusCode,
                ContentAsUTF8 = JsonConvert.SerializeObject(JSON, Formatting.Indented)


            };
            // if(status)
            // response.Headers.Add("Set-Cookie", "Auth=" + Base64Encode(words[9] + "&" + words[1]) + ";");
            return response;
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static HttpResponse ALogin(HttpRequest data)
        {
            char[] delimiterChars = { '=', '&' };
            string[] words = data.Content.Split(delimiterChars);
            bool status = false;//true - успех 
            Console.WriteLine("Login: {0}, Pass: {1}", words[1], words[3]);
            string result = "";
            int usertype = -1;
            //Работа с  базой
            try
            {

                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(" SELECT  password_landlords FROM Landlord WHERE login_landlords = '" + words[1] + "';", conn))
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                        {
                            result = reader.GetString(0);
                        }
                    if (result == "")
                    {
                        using (var cmd = new NpgsqlCommand(" SELECT  admin_password FROM administrator WHERE admin_login = '" + words[1] + "';", conn))
                        using (var reader = cmd.ExecuteReader())
                            while (reader.Read())
                            {
                                result = reader.GetString(0);
                            }
                        usertype = 2;
                    }
                    else
                    {
                        usertype = 1;
                    }
                    if (result == "")
                    {
                        using (var cmd = new NpgsqlCommand(" SELECT  password_organizers FROM organizerr WHERE login_organizers = '" + words[1] + "';", conn))
                        using (var reader = cmd.ExecuteReader())
                            while (reader.Read())
                            {
                                result = reader.GetString(0);
                            }
                        usertype = 0;
                    }
                    conn.Close();
                }
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine(result);
            if (result == "")
                status = false;
            else if (HashMD5(words[3]) == result)
                status = true;

            //Формирование запроса 
            string StatusCode;
            string StatusText;
            if (status)
            {
                StatusCode = "200";
                StatusText = "Ok";
                JSON["status"] = "success";
                JSON["cookie"] = Base64Encode(usertype + "&" + words[1]);
            }
            else
            {
                StatusCode = "400";
                StatusText = "BadRequest";
                JSON["status"] = "fail";
            }
            HttpResponse response = new HttpResponse
            {
                ReasonPhrase = StatusText,
                StatusCode = StatusCode,
                ContentAsUTF8 = JsonConvert.SerializeObject(JSON, Formatting.Indented)

            };


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
        public static HttpResponse APlaсes(String data)
        {
            bool status = false;//true - успех 
            List<string> costs = new List<string>();
            List<string> landlord = new List<string>();
            List<string> room_name = new List<string>();
            List<string> square = new List<string>();
            List<string> address = new List<string>();
            try
            {

                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(" SELECT r.costs, r.landlord, r.room_name, rc.quare, rc.address FROM room r join room_cost rc on r.costs = rc.costs;", conn))
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                        {
                            costs.Add((reader.GetInt32(0)).ToString());
                            landlord.Add(reader.GetString(1));
                            room_name.Add(reader.GetString(2));
                            square.Add((reader.GetInt32(3)).ToString());
                            address.Add(reader.GetString(4));
                            /*JSON_places["costs"] = (reader.GetInt32(0)).ToString();//costs
                            JSON_places["landlord"] = reader.GetString(1);//landlord
                            JSON_places["room_name"] = reader.GetString(2);//room_name
                            JSON_places["square"] = (reader.GetInt32(3)).ToString();//square
                            JSON_places["address"] = reader.GetString(4);//address*/
                        }
                    status = true;
                    conn.Close();
                }
            }
            catch (Npgsql.NpgsqlException ex)
            {
                status = false;
                Console.WriteLine(ex.Message);
            }
            //Формирование запроса 
            string StatusCode;
            string StatusText;
            if (status)
            {
                StatusCode = "200";
                StatusText = "Ok";
                JSON["status"] = "success";
            }
            else
            {
                StatusCode = "400";
                StatusText = "BadRequest";
                JSON["status"] = "fail";
            }
            List<Dictionary<string, string>> Data = new List<Dictionary<string, string>>();
            ListDictionary list = new ListDictionary();
            for (int i = 0; i < costs.Count(); i++)
            {
                Data.Add(new Dictionary<string, string>());
                Data[i].Add("costs", costs[i]);
                Data[i].Add("landlord", landlord[i]);
                Data[i].Add("room_name", room_name[i]);
                Data[i].Add("square", square[i]);
                Data[i].Add("address", address[i]);
            }
            places place_data = new places
            {
                status = JSON["status"],
                response = Data
            };
            string data_json = JsonConvert.SerializeObject(place_data, Formatting.Indented);
            Console.WriteLine(data_json);
            HttpResponse response = new HttpResponse
            {
                ReasonPhrase = StatusText,
                StatusCode = StatusCode,
                ContentAsUTF8 = JsonConvert.SerializeObject(place_data, Formatting.Indented)


            };
            return response;
        }
        public static HttpResponse APlaсesupdate(String data)
        {
            char[] delimiterChars = { '=', '&' };
            string[] words = data.Split(delimiterChars);
            bool status = false;//true - успех 
            //Формирование запроса 
            string StatusCode;
            string StatusText;
            Dictionary<string, string> JSON_local = new Dictionary<string, string>
        {
            { "status", "fail" },

        };
            if (status)
            {
                StatusCode = "200";
                StatusText = "Ok";
                JSON_local["status"] = "success";
            }
            else
            {
                StatusCode = "400";
                StatusText = "BadRequest";
                JSON_local["status"] = "fail";
            }
            HttpResponse response = new HttpResponse
            {
                ReasonPhrase = StatusText,
                StatusCode = StatusCode,
                ContentAsUTF8 = JsonConvert.SerializeObject(JSON_local, Formatting.Indented)


            };
            return response;
        }
    }
}
