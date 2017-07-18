using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace GithubDisplay.RequestModels
{
    public class Request
    {
        [JsonIgnore]
        public virtual List<Tuple<string, string>> Headers { get; set; } = new List<Tuple<string, string>>();

        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public string Token { get; set; }
    }
}
