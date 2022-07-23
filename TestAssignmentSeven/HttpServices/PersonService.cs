using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TestAssignmentSeven.Models;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TestAssignmentSeven.HttpServices
{
    public class PersonService: IPersonService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PersonService(IConfiguration configuration, IHttpClientFactory HttpClientFactory)
        {
            this._configuration = configuration;
            this._httpClientFactory = HttpClientFactory;
        }       

        public async Task GetPersons()
        {
            var BaseURL = _configuration["BaseUrl"];
            var ApiEndpoint = _configuration["ApiEndpoint"];

            IList<Person> Persons = new List<Person>();

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, BaseURL + ApiEndpoint);
            requestMessage.Headers.Add("Accept", "application/json");
            
            var client = _httpClientFactory.CreateClient();

            try
            {
                var responseMessage = await client.SendAsync(requestMessage);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var contents =
                       await responseMessage.Content.ReadAsStringAsync();                    

                    Persons = JsonConvert.DeserializeObject<IList<Person>>(contents);                   

                    if (Persons.Count > 0)
                    {
                        bool IsValid = ValidateJson(Persons.ToList());

                        if (!IsValid)
                        {
                            Console.WriteLine("The API endpoint returned Json data is not in valid structure");
                            return;
                        }
                        var personWithId42 = Persons.FirstOrDefault(p => p.id.Equals(42));
                        var allFirstNameswithAge23 = String.Join(",", Persons.Where(p => p.age.Equals(23)).Select(x => x.first));
                        var genderPerAgeQuery = Persons.GroupBy(p => p.age)
                                                    .Select(g =>
                                                        new
                                                        {
                                                            Age = g.Key,
                                                            Male = g.Where(p => p.gender.Equals("M")).Count(),
                                                            Female = g.Where(p => p.gender.Equals("F") || p.gender != "M").Count(),
                                                        }).OrderBy(x => x.Age);

                        if (personWithId42 != null)
                        {
                            Console.WriteLine("Full Name: {0} {1}", personWithId42.first, personWithId42.last);
                        }

                        if (allFirstNameswithAge23 != null)
                        {
                            Console.WriteLine("Persons First Name with Age 23 : {0}", allFirstNameswithAge23);
                        }

                        foreach (var group in genderPerAgeQuery)
                        {
                            Console.WriteLine("Age: {0} Female: {1} Male: {2}", group.Age, group.Female, group.Male);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Response successful, but no data returned by the API endpoint");
                    }
                }
                else
                {
                    Console.WriteLine("Unsuccessful response received with status code: {0}", responseMessage.StatusCode);                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calling API!! Please check if API or it's endpoint is up and listening to requests. Error message: {0}", ex.Message);
            }            
        }

        private bool ValidateJson(List<Person> Persons)
        {
            bool valid = true;
          
            string schemaJson = @"{
                                  'description': 'A person',
                                  'type': 'object',
                                  'properties': {
                                    'id': {'type': 'integer'},
                                    'first': {'type': 'string'},
                                    'last': {'type': 'string'},
                                    'age': {'type': 'integer'},
                                    'gender': {'type': 'string'},
                                  },
                                  'required': [ 'id', 'first', 'last', 'age', 'gender' ]
                                }";

            JSchema schema = JSchema.Parse(schemaJson);

            foreach (var person in Persons)
            {
                JObject user = JObject.Parse(JsonConvert.SerializeObject(person));
                valid = user.IsValid(schema);
            }
            return valid;
        }
    }
}
