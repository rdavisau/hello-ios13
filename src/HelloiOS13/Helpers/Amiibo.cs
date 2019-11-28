using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace HelloiOS13.Helpers
{
    public interface IAmiiboAPI
    {
        [Get("/amiibo/?head={gameIdentifier}&tail={characterIdentifier}")]
        Task<AmiiboResults> GetAmiibo(string gameIdentifier, string characterIdentifier);
    }

    public class Release
    {
        public string au { get; set; }
        public string eu { get; set; }
        public string jp { get; set; }
        public string na { get; set; }
    }

    public class Amiibo
    {
        public string Name { get; set; }
        public string Character { get; set; }
        public string GameSeries { get; set; }
        public string AmiiboSeries { get; set; }
        public string Image { get; set; }
        public string Head { get; set; }
        public string Tail { get; set; }
        public string Type { get; set; }
        private Release release { get; set; }
    }

    public class AmiiboResults
    {
        public List<Amiibo> Amiibo { get; set; }
    }
}
