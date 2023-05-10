using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace ChatGPT
{
    class Program
    {

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            //Define String builder
            StringBuilder Result = new StringBuilder();

            string inputfolderPath = Path.Combine(Environment.CurrentDirectory, "TLDashboard");

            string[] allDirectories = Directory.GetDirectories(inputfolderPath, "*", SearchOption.AllDirectories);

            // Create a list to store all file paths
            List<string> filepaths = new List<string>();

            // Get all files' paths from the specified folder
            foreach (string file in Directory.GetFiles(inputfolderPath))
            {
                filepaths.Add(file);
            }

            // Loop through all directories and add all file paths to the list
            foreach (string directory in allDirectories)
            {
                string[] files = Directory.GetFiles(directory);

                foreach (string file in files.OrderBy(x => x))
                {
                    filepaths.Add(file);
                }
            }
            // Specify the file path where you want to write output text
            string outputfilePath = Path.Combine(Environment.CurrentDirectory, @"ReportChatGPT\Report-" + DateTime.Now.Millisecond);

            Result.Append("<ul>");
            // Loop through all the filepaths and read the content of each file
            foreach (string filepath in filepaths)
            {
                // Read the content of the file
                string fileContent = string.Concat("Write a functional requirement output for the below c# code in paragraph: ", Environment.NewLine, System.IO.File.ReadAllText(filepath));
                try
                {
                    Console.WriteLine(string.Concat("Below is the explaination of the file path code : ", filepath, Environment.NewLine));
                    //Result.Append(string.Concat("<h3>Below is the explaination of the file path code : ", filepath, "</h3>"));
                    string output = await ChatGPTCall(fileContent);
                    if(!string.IsNullOrEmpty(output))
                    {
                        Result.Append(string.Concat("<li>",output, "</br></br></li>"));
                    }
                }
                catch (System.Exception ex)
                {                    
                    //Console.WriteLine(ex.Message);
                    //Result.Append(string.Concat("<p>An error occurred for the above code explaination: ", ex.Message, "</p>"));
                  
                }
            }
            Result.Append("</ul>");
            string inputhtml = System.IO.File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "template.html"));
            inputhtml = inputhtml.Replace("##HtmlBody", Result.ToString());
            //Save html file
            System.IO.File.WriteAllText(outputfilePath + ".html", inputhtml);
            //Save html into word doc
            SaveAsPDF(outputfilePath, inputhtml);
        }

        public async static Task<string> ChatGPTCall(string input)
        {
            //Console.WriteLine("API Request...");
            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine(input);
            //Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Fetching API Response...");
            Console.WriteLine(Environment.NewLine);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "");

            var msg = new message() { content = input, role = "user" };

            var request = new OpenAIRequest
            {
                Model = "gpt-3.5-turbo",
                Temperature = 1,
                messages = new message[1],

            };
            request.messages[0] = msg;

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var resjson = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Received API Response...");
            if (!response.IsSuccessStatusCode)
            {
                //var errorResponse = JsonSerializer.Deserialize<OpenAIErrorResponse>(resjson);                
                Console.WriteLine("Error: " + resjson);
                return string.Empty;
            }
            else
            {
                var data = JsonSerializer.Deserialize<Root>(resjson);
                var output = string.Join(Environment.NewLine, data.choices.Select(x => x.message).Select(x => x.content).ToList());
                Console.WriteLine(output);
                return output;
            }
        }

        public static bool SaveAsPDF(string path, string html)
        {
            //IGeneratePdf _generatePdf = new GeneratePdf();

            //byte[] fileBytes = _generatePdf.GetPDF(html);
            //if (fileBytes != null)
            //{
            //    System.IO.File.WriteAllBytes(path, fileBytes);
            //}
            return true;
        }
    }

    public class Choice
    {       
        public int index { get; set; }        
        public string finish_reason { get; set; }
        public message message { get; set; }
    }
    public class Root
    {
        public string id { get; set; }
        public string @object { get; set; }
        public int created { get; set; }       
        public List<Choice> choices { get; set; }
        public Usage usage { get; set; }
    }
    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    public class OpenAIRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        public message[] messages { get; set; }


        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }
    }
    public class message
    {
        [JsonPropertyName("role")]
        public string role { get; set; }
        public string content { get; set; }
    }

}