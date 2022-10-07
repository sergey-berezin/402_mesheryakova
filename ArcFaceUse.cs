using System;
using System.Collections.Generic;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
//using System.Threading.Tasks.Dataflow;

namespace NuGetArcFace
{
    public class ArcFaceUse : IDisposable
    {
        private InferenceSession session;

        public ArcFaceUse()
        {
            using var modelStream = typeof(ArcFaceUse).Assembly.GetManifestResourceStream("NuGetArcFace.arcfaceresnet100-8.onnx");
            using var memoryStream = new MemoryStream();
            modelStream.CopyTo(memoryStream);
            this.session = new InferenceSession(memoryStream.ToArray());
        }

        public async Task<float[]> GetFeatureVector(Image<Rgb24> face, CancellationToken token)
        {
            return await Task<float[]>.Factory.StartNew(() =>
            {
                token.ThrowIfCancellationRequested();
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("data", ImageToTensor(face)) };
                IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results;
                lock (session)
                {
                    results = session.Run(inputs);
                }
                return Normalize(results.First(v => v.Name == "fc1").AsEnumerable<float>().ToArray());
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /*public void GetFeatureVector(Image<Rgb24> face, BufferBlock<float[]> block, CancellationToken token)
        {
            
            var act = new ActionBlock<Image<Rgb24>>(face =>
            {
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("data", ImageToTensor(face)) };
                IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results;
                token.ThrowIfCancellationRequested();
                results = session.Run(inputs);
                block.Post(Normalize(results.First(v => v.Name == "fc1").AsEnumerable<float>().ToArray()));
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = token
            });
        }*/

        public float Distance(float[] v1, float[] v2) => Length(v1.Zip(v2).Select(p => p.First - p.Second).ToArray());

        public async Task<float> Distance(Image<Rgb24> face1, Image<Rgb24> face2, CancellationToken token)
        {
            float[] v1;
            float[] v2;
            try
            {
                v1 = await this.GetFeatureVector(face1, token);
                v2 = await this.GetFeatureVector(face2, token);
            }
            catch (Exception)
            {
                throw;
            }
            return await Task<float>.Factory.StartNew(() =>
            {
                return Distance(v1, v2);
            });
        }

        public float Similarity(float[] v1, float[] v2) => v1.Zip(v2).Select(p => p.First * p.Second).Sum();

        public async Task<float> Similarity(Image<Rgb24> face1, Image<Rgb24> face2, CancellationToken token)
        {
            float[] v1;
            float[] v2;
            try
            {
                v1 = await this.GetFeatureVector(face1, token);
                v2 = await this.GetFeatureVector(face2, token);
            }
            catch (Exception)
            {
                throw;
            }
            return await Task<float>.Factory.StartNew(() =>
            {
                return Similarity(v1, v2);
            });
        }

        private float Length(float[] v) => (float)Math.Sqrt(v.Select(x => x * x).Sum());

        private float[] Normalize(float[] v)
        {
            var len = Length(v);
            return v.Select(x => x / len).ToArray();
        }

        private static DenseTensor<float> ImageToTensor(Image<Rgb24> img)
        {
            var w = img.Width;
            var h = img.Height;
            var t = new DenseTensor<float>(new[] { 1, 3, h, w });

            img.ProcessPixelRows(pa =>
            {
                for (int y = 0; y < h; y++)
                {
                    Span<Rgb24> pixelSpan = pa.GetRowSpan(y);
                    for (int x = 0; x < w; x++)
                    {
                        t[0, 0, y, x] = pixelSpan[x].R;
                        t[0, 1, y, x] = pixelSpan[x].G;
                        t[0, 2, y, x] = pixelSpan[x].B;
                    }
                }
            });

            return t;
        }

        public void Dispose()
        {
            session.Dispose();
        }
    }
}
