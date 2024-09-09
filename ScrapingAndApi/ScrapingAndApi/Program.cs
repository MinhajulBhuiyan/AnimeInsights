using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ScrapingAndApi;

namespace AnimeTitleFetcher
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Fetcher.AnimeFetcher();
        }
    }
}
