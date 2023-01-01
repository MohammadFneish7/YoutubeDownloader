using System.IO;
using System.Reflection;

namespace YoutubeDownloader.Models
{
    public class Counter
    {
        public long Count { get; set; } = 125000;
        private string path = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "count.txt");
        public Counter()
        {
            try
            {
                Count = long.Parse(File.ReadAllText(path));
            }
            catch (Exception)
            {

            }
        }
        public long Increment()
        {
            Count++;
            File.WriteAllText(path, Count.ToString());
            return Count;
        }
    }
}
