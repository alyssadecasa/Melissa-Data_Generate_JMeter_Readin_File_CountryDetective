/*
 * Generate txt file for JMeter load testing of CountryDetective web API
 * Program.cs
 * 
 * Reads in a list of addresses from a SQL table. For each address, ths program
 * runs it through the CountryDetective web API contained within either a Windows
 * or Linux Docker Container. Stores the request and response fields in a txt file
 * named by the user
 * 
 * @author Alyssa House
 */

using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace GenerateCSVforCountryDetectiveJMeterTesting
{
    class Program
    {
        private static int count = 1;
        static void Main(string[] args)
        {
            // clear txt file
            System.IO.File.WriteAllText(@"", "");   // txt file path 

            List<string> addresses = BuildAddressLines();

            int count = 1;
            foreach(string address in addresses)
            {
                Console.WriteLine(count++);
                string country = RunGETRequestAsync(address).GetAwaiter().GetResult();
                WriteToFile(address, country);
            }

            // keep console open
            Console.WriteLine("Program finished executing successfully.");
            Console.Read();

        }

        public static void WriteToFile(string addressline, string response)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"", true)) // txt file path
            {
                if (String.IsNullOrEmpty(addressline))
                {
                    //skip
                    return;
                }
                file.WriteLine("Entry_Num_" + count++ + "|" + addressline + "|" + response);
                Console.WriteLine("Entry_Num_" + count++ + "|" + addressline + "|" + response);
            }
        }

        public static List<string> BuildAddressLines()
        {
            List<string> addresses = new List<string>();
            using (SqlDataReader reader = ReadInAddressesFromSQL(""))   // SQL table name
            {
                while (reader.Read()) // for each row
                {    
                    string address = reader["ADDRESSLINE1"].ToString().Trim() + " ";
                    address += reader["ADDRESSLINE2"].ToString().Trim() + " ";
                    address += reader["ADDRESSLINE3"].ToString().Trim() + " ";
                    address += reader["ADDRESSLINE4"].ToString().Trim() + " ";
                    address += reader["ADDRESSLINE5"].ToString().Trim() + " ";
                    address += reader["ADDRESSLINE6"].ToString().Trim() + " ";
                    address += reader["ADDRESSLINE7"].ToString().Trim() + " ";
                    address += reader["ADDRESSLINE8"].ToString().Trim() + " ";
                    address += reader["LOCALITY"].ToString().Trim() + " ";
                    address += reader["ADMINAREA"].ToString().Trim() + " ";
                    address += reader["POSTALCODE"].ToString().Trim();

                    address = Regex.Replace(address, @"\s+", " ");
                    address = address.Trim();
                    
                    address = HttpUtility.UrlEncode(address);
                    addresses.Add(address);
                }
            }
            return addresses;
        }


        static async Task<string> RunGETRequestAsync(string addressline)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("");   // uri retrieved from Docker container
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("lookup?ff=" + addressline);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    RootObject deserializedRootObj = JsonConvert.DeserializeObject<RootObject>(json);
                    string country = deserializedRootObj.country;
                    return country;
                }

                return null;
            }
        }

        public static SqlDataReader ReadInAddressesFromSQL(string ReadTableName)
        {
            // create connection string
            SqlConnectionStringBuilder connString = new SqlConnectionStringBuilder();
            BuildConnectionString(connString);

            // create connection
            SqlConnection connection = new SqlConnection(connString.ToString());

            // Set query to be used 
            string query = "SELECT distinct TOP (11000) newid(), ADDRESSLINE1, ADDRESSLINE2, " +
                "ADDRESSLINE3, ADDRESSLINE4, ADDRESSLINE5, ADDRESSLINE6, ADDRESSLINE7, ADDRESSLINE8, " +
                "LOCALITY, ADMINAREA, POSTALCODE " + "FROM " + ReadTableName + " ORDER BY newid()";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Connection.Open();
                // parameterize query?

                return command.ExecuteReader();
            }
        }

        public static SqlConnectionStringBuilder BuildConnectionString(SqlConnectionStringBuilder connString)
        {
            connString.DataSource = "";                     // Server
            connString.InitialCatalog = "";                 // Database
            connString.IntegratedSecurity = true;           // Connection type: Integrated Security

            return connString;
        }
    }
}
