using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ScrapingAndApi
{
    public class Fetcher
    {
        public static async Task AnimeFetcher()
        {
            string graphqlEndpoint = "https://graphql.anilist.co";
            int totalTitles = 500; // Desired number of anime titles
            List<string> insertStatements = new List<string>();

            int perPage = 50;
            int currentPage = 1;
            int fetchedTitles = 0;

            while (fetchedTitles < totalTitles)
            {
                string query = @"
                query ($page: Int, $perPage: Int) {
                    Page (page: $page, perPage: $perPage) {
                        media (type: ANIME, sort: POPULARITY_DESC, isAdult: false) {
                            title {
                                romaji
                            }
                            description(asHtml: false)
                            duration
                            startDate {
                                year
                                month
                                day
                            }
                            format
                            episodes
                        }
                    }
                }";

                var variables = new
                {
                    page = currentPage,
                    perPage = perPage
                };

                List<string> statements = await FetchAnimeDataAsync(graphqlEndpoint, query, variables);
                if (statements.Count == 0) break; // No more valid titles to fetch
                insertStatements.AddRange(statements);
                fetchedTitles += statements.Count;

                currentPage++;
            }

            // Limit the number of statements to the desired amount
            insertStatements = insertStatements.GetRange(0, Math.Min(totalTitles, insertStatements.Count));
            insertStatements.Insert(0, "");
            insertStatements.Insert(0, "BEGIN");
            insertStatements.Insert(0, "");
            insertStatements.Insert(0, "SET DEFINE OFF;");

            insertStatements.Insert(0, $"-- Total Animes Fetched: {insertStatements.Count - 4}"); // Adjust for added statements

            insertStatements.Add("");
            insertStatements.Add("COMMIT;");
            insertStatements.Add("END;");

            string filePath = "C:\\Users\\mirsa\\Documents\\temp\\anime_insert_statements1.sql";

            File.WriteAllLines(filePath, insertStatements);

            Console.WriteLine($"Insert statements have been written to {filePath}");
            Console.ReadKey();
        }

        public static async Task<List<string>> FetchAnimeDataAsync(string url, string query, object variables)
        {
            List<string> insertStatements = new List<string>();

            using (HttpClient client = new HttpClient())
            {
                var requestBody = new
                {
                    query = query,
                    variables = variables
                };

                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(responseString);

                    var media = json["data"]["Page"]["media"];

                    foreach (var item in media)
                    {
                        try
                        {
                            string title = item["title"]["romaji"]?.ToString().Replace("'", "''") ?? throw new Exception("Title is null");
                            string description = item["description"]?.ToString().Replace("'", "''") ?? throw new Exception("Description is null");
                            int duration = item["duration"]?.ToObject<int>() ?? throw new Exception("Duration is null");
                            var startDate = item["startDate"];
                            if (startDate["year"] == null || startDate["month"] == null || startDate["day"] == null)
                                throw new Exception("Start date is null");

                            string releaseDate = $"TIMESTAMP '{startDate["year"]}-{startDate["month"]:D2}-{startDate["day"]:D2} 00:00:00'";

                            string format = item["format"]?.ToString().ToUpper() ?? throw new Exception("Format is null");
                            string type = format == "MOVIE" ? "MOVIE" : "SHOW";
                            int episodeCount = item["episodes"]?.ToObject<int>() ?? throw new Exception("Episode count is null");

                            string insertStatement = $"INSERT INTO \"Animes\" (\"Title\", \"Type\", \"EpisodeCount\", \"Description\", \"Duration\", \"ReleaseDate\") " +
                                                     $"VALUES ('{title}', '{type}', {episodeCount}, '{description}', {duration}, {releaseDate});";
                            insertStatements.Add(insertStatement);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}. Breaking process.");
                            return insertStatements;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error fetching data from API");
                }
            }

            return insertStatements;
        }


        public static async Task DirectorFetcher()
        {
            string graphqlEndpoint = "https://graphql.anilist.co";
            int totalDirectors = 500; // Desired number of directors
            List<string> insertStatements = new List<string>();

            int perPage = 50;
            int currentPage = 1;
            int fetchedDirectors = 0;

            while (fetchedDirectors < totalDirectors)
            {
                string query = @"
        query ($page: Int, $perPage: Int) {
            Page (page: $page, perPage: $perPage) {
                media (type: ANIME, sort: POPULARITY_DESC, isAdult: false) {
                    staff {
                        edges {
                            node {
                                name {
                                    full
                                }
                                dateOfBirth {
                                    year
                                    month
                                    day
                                }
                                gender
                            }
                            role
                        }
                    }
                }
            }
        }";

                var variables = new
                {
                    page = currentPage,
                    perPage = perPage
                };

                List<string> statements = await FetchDirectorDataAsync(graphqlEndpoint, query, variables);
                if (statements.Count == 0) break; // No more valid directors to fetch
                insertStatements.AddRange(statements);
                fetchedDirectors += statements.Count;

                currentPage++;
            }

            // Limit the number of statements to the desired amount
            insertStatements = insertStatements.GetRange(0, Math.Min(totalDirectors, insertStatements.Count));

            // Add additional formatting
            insertStatements.Insert(0, "");
            insertStatements.Insert(0, "BEGIN");
            insertStatements.Insert(0, "");
            insertStatements.Insert(0, "SET DEFINE OFF;");
            insertStatements.Insert(0, $"-- Total Directors Fetched: {insertStatements.Count - 4}");


            insertStatements.Add("");
            insertStatements.Add("COMMIT;");
            insertStatements.Add("END;");

            // Write the insert statements to a file
            string filePath = "C:\\Users\\mirsa\\Documents\\temp\\director_insert_statements.sql";
            File.WriteAllLines(filePath, insertStatements);

            Console.WriteLine($"Insert statements have been written to {filePath}");
            Console.ReadKey();
        }

        public static async Task<List<string>> FetchDirectorDataAsync(string url, string query, object variables)
        {
            List<string> insertStatements = new List<string>();

            using (HttpClient client = new HttpClient())
            {
                var requestBody = new
                {
                    query = query,
                    variables = variables
                };

                string jsonString = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(responseString);

                    var media = json["data"]["Page"]["media"];

                    foreach (var anime in media)
                    {
                        var staff = anime["staff"]["edges"];

                        foreach (var edge in staff)
                        {
                            var node = edge["node"];
                            string role = edge["role"]?.ToString();
                            if (role == "Director")
                            {
                                try
                                {
                                    string name = node["name"]["full"]?.ToString().Replace("'", "''") ?? "";
                                    string dob = GetDOB(node);
                                    string gender = node["gender"]?.ToString() ?? "";

                                    if (string.IsNullOrEmpty(name) || dob == "NULL" || string.IsNullOrEmpty(gender)) continue;

                                    string insertStatement = $"INSERT INTO \"Directors\" (\"Name\", \"DOB\", \"Gender\") " +
                                                             $"VALUES ('{name}', {dob}, '{gender}');";
                                    insertStatements.Add(insertStatement);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error: {ex.Message}. Skipping director.");
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error fetching data from API");
                }
            }

            return insertStatements;
        }

        public static async Task VoiceActorFetcher()
        {
            string graphqlEndpoint = "https://graphql.anilist.co";
            int totalVoiceActors = 500; // Desired number of voice actors
            List<string> insertStatements = new List<string>();

            int perPage = 50;
            int currentPage = 1;
            int fetchedVoiceActors = 0;

            while (fetchedVoiceActors < totalVoiceActors)
            {
                string query = @"
        query ($page: Int, $perPage: Int) {
            Page (page: $page, perPage: $perPage) {
                media (type: ANIME, sort: POPULARITY_DESC, isAdult: false) {
                    characters {
                        edges {
                            node {
                                name {
                                    full
                                }
                            }
                            voiceActors {
                                name {
                                    full
                                }
                                dateOfBirth {
                                    year
                                    month
                                    day
                                }
                                gender
                            }
                        }
                    }
                }
            }
        }";

                var variables = new
                {
                    page = currentPage,
                    perPage = perPage
                };

                List<string> statements = await FetchVoiceActorDataAsync(graphqlEndpoint, query, variables);
                if (statements.Count == 0) break; // No more valid voice actors to fetch
                insertStatements.AddRange(statements);
                fetchedVoiceActors += statements.Count;

                currentPage++;
            }

            // Limit the number of statements to the desired amount
            insertStatements = insertStatements.GetRange(0, Math.Min(totalVoiceActors, insertStatements.Count));

            // Add additional formatting
            insertStatements.Insert(0, "");
            insertStatements.Insert(0, "BEGIN");
            insertStatements.Insert(0, "");
            insertStatements.Insert(0, "SET DEFINE OFF;");
            insertStatements.Insert(0, $"-- Total Voice Actors Fetched: {insertStatements.Count-4}");

            insertStatements.Add("");
            insertStatements.Add("COMMIT;");
            insertStatements.Add("END;");

            // Write the insert statements to a file
            string filePath = "C:\\Users\\mirsa\\Documents\\temp\\voice_actor_insert_statements.sql";
            File.WriteAllLines(filePath, insertStatements);

            Console.WriteLine($"Insert statements have been written to {filePath}");
            Console.ReadKey();
        }

        public static async Task<List<string>> FetchVoiceActorDataAsync(string url, string query, object variables)
        {
            List<string> insertStatements = new List<string>();

            using (HttpClient client = new HttpClient())
            {
                var requestBody = new
                {
                    query = query,
                    variables = variables
                };

                string jsonString = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(responseString);

                    var media = json["data"]["Page"]["media"];

                    foreach (var anime in media)
                    {
                        var characters = anime["characters"]["edges"];

                        foreach (var character in characters)
                        {
                            var voiceActors = character["voiceActors"];

                            foreach (var voiceActor in voiceActors)
                            {
                                try
                                {
                                    string name = voiceActor["name"]["full"]?.ToString().Replace("'", "''") ?? "";
                                    string dob = GetDOB(voiceActor);
                                    string gender = voiceActor["gender"]?.ToString() ?? "";

                                    // Skip if any required field is null
                                    if (string.IsNullOrEmpty(name) || dob == "NULL" || string.IsNullOrEmpty(gender)) continue;

                                    string insertStatement = $"INSERT INTO \"VoiceActors\" (\"Name\", \"DOB\", \"Gender\") " +
                                                             $"VALUES ('{name}', {dob}, '{gender}');";
                                    insertStatements.Add(insertStatement);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error: {ex.Message}. Skipping voice actor.");
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error fetching data from API");
                }
            }

            return insertStatements;
        }

        private static string GetDOB(JToken node)
        {
            string dob = "NULL";
            var dobNode = node["dateOfBirth"];
            if (dobNode != null)
            {
                int year = dobNode["year"]?.ToObject<int>() ?? 0;
                int month = dobNode["month"]?.ToObject<int>() ?? 0;
                int day = dobNode["day"]?.ToObject<int>() ?? 0;
                if (year != 0 && month != 0 && day != 0)
                {
                    dob = $"TO_DATE('{year}-{month:D2}-{day:D2}', 'YYYY-MM-DD')";
                }
            }
            return dob;
        }


    }
}
