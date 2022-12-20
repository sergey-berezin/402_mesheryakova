using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace WpfApp
{
    //информация про изображение
    public class ImageInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Hash { get; set; }
        public byte[] Embedding { get; set; }

        public ImageDetails Details { get; set; } //reference navigation property
    }

    //само изображение
    public class ImageDetails
    {
        public static string GetHash(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                return string.Concat(sha256.ComputeHash(data).Select(x => x.ToString("X2")));
            }
        }

        public int Id { get; set; }
        public byte[] Data { get; set; }
    }

    public class ImagesContext : DbContext
    {
        public DbSet<ImageInfo> ImagesInfo { get; set; }
        public DbSet<ImageDetails> ImagesDetails { get; set; }

        public ImagesContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder o)
        {
            o.UseSqlite("Data Source=images.db");
        }
    }

}