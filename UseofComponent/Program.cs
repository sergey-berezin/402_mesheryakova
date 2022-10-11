using NuGetArcFace;
using System;
using System.Threading.Tasks;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace AppUsingArcFace
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (ArcFaceUse component = new())
            {
                using var face1 = Image.Load<Rgb24>("face1.png");
                using var face2 = Image.Load<Rgb24>("face2.png");

                CancellationTokenSource cts = new CancellationTokenSource();
                CancellationToken token = cts.Token;

                var dist = await component.Distance(face1, face2, token);
                var sim = await component.Similarity(face1, face2, token);

                Console.WriteLine($"For face1 and face2:");
                Console.WriteLine($"Distance =  {dist}");
                Console.WriteLine($"Similarity =  {sim}");
            }
        }
    }
}
