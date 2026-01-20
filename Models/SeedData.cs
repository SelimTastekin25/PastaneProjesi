using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PastaneProjesi.Models; // other usings
using System;
using System.Linq;

namespace PastaneProjesi.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new PastaneContext(serviceProvider.GetRequiredService<DbContextOptions<PastaneContext>>()))
            {
                // Admin Kullanıcısı Oluşturma
                if (!context.Users.Any(u => u.UserName == "admin"))
                {
                    var adminUser = new User
                    {
                        UserName = "admin",
                        Email = "admin@pastane.com",
                        FullName = "System Admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        IsAdmin = true,
                        CreatedAt = DateTime.Now
                    };
                    context.Users.Add(adminUser);
                    context.SaveChanges();
                }

                // Görsel URL Tanımlamaları
                var pistachioImg = "https://images.unsplash.com/photo-1603379421571-f110a830382a?auto=format&fit=crop&q=80&w=800";
                var chocolateImg = "https://images.unsplash.com/photo-1626263468007-a9e0cf83f1ac?auto=format&fit=crop&q=80&w=800";
                var redVelvetImg = "https://images.unsplash.com/photo-1586788680434-30d324b2d46f?auto=format&fit=crop&q=80&w=800";
                var baklavaImg = "https://images.unsplash.com/photo-1598110750624-207050c4f28c?auto=format&fit=crop&q=80&w=800";
                var coldBaklavaImg = "/img/soguk-baklava.jpg";
                var sekerpareImg = "/img/şekerpare.jpg";
                var cookieImg = "https://images.unsplash.com/photo-1499636136210-6f4ee915583e?auto=format&fit=crop&q=80&w=800";
                var applePieImg = "https://images.unsplash.com/photo-1621743478914-cc8a86d7e7b5?auto=format&fit=crop&q=80&w=800";
                var savoryImg = "https://images.unsplash.com/photo-1560999448-1288f5c62468?auto=format&fit=crop&q=80&w=800";

                // Kategoriler
                var categories = new List<Category>
                {
                    new Category { Name = "Pastalar" },
                    new Category { Name = "Şerbetli Tatlılar" },
                    new Category { Name = "Kurabiyeler" },
                    new Category { Name = "Özel Gün Pastaları" },
                    new Category { Name = "Sütlü Tatlılar" },
                    new Category { Name = "Soğuk İçecekler" }
                };

                foreach (var cat in categories)
                {
                    if (!context.Categories.Any(c => c.Name == cat.Name))
                    {
                        context.Categories.Add(cat);
                    }
                }
                context.SaveChanges();

                // Refresh categories from DB to get IDs
                var dbCategories = context.Categories.ToList();
                var pastalar = dbCategories.First(c => c.Name == "Pastalar");
                var serbetli = dbCategories.First(c => c.Name == "Şerbetli Tatlılar");
                var kurabiyeler = dbCategories.First(c => c.Name == "Kurabiyeler");
                var ozelGun = dbCategories.First(c => c.Name == "Özel Gün Pastaları");
                var sutlu = dbCategories.First(c => c.Name == "Sütlü Tatlılar");
                var icecek = dbCategories.First(c => c.Name == "Soğuk İçecekler");

                // Ürünler Listesi
                var products = new List<Product>
                {
                    // Pastalar
                    new Product { Name = "Fıstıklı Yaş Pasta", Price = 1250.00M, Stock = 10, CategoryId = pastalar.Id, ImageUrl = pistachioImg, Description = "Bol Antep fıstıklı, yumuşacık pandispanya ve özel kremasıyla hazırlanan enfes bir lezzet." },
                    new Product { Name = "Çikolatalı Ganaj", Price = 1350.00M, Stock = 8, CategoryId = pastalar.Id, ImageUrl = chocolateImg, Description = "Yoğun Belçika çikolatası ve taze krema ile hazırlanan, çikolata severlerin vazgeçilmezi." },
                    new Product { Name = "Red Velvet", Price = 1150.00M, Stock = 12, CategoryId = pastalar.Id, ImageUrl = redVelvetImg, Description = "Kadifemsi dokusu ve özel peynir kreması dolgusuyla görsel bir şölen." },

                    // Şerbetli
                    new Product { Name = "Antep Fıstıklı Baklava", Price = 1600.00M, Stock = 20, CategoryId = serbetli.Id, ImageUrl = baklavaImg, Description = "Gaziantep'ten gelen özel fıstıklarla, incecik açılmış 40 kat yufkanın muhteşem buluşması." },
                    new Product { Name = "Soğuk Baklava", Price = 1750.00M, Stock = 15, CategoryId = serbetli.Id, ImageUrl = coldBaklavaImg, Description = "Sütlü şerbeti ve üzerine serpiştirilen kakao ile hafif ve modern bir baklava deneyimi." },
                    new Product { Name = "Şekerpare", Price = 650.00M, Stock = 25, CategoryId = serbetli.Id, ImageUrl = sekerpareImg, Description = "Ağızda dağılan kıvamı ve tam dozunda şerbetiyle klasik bir Türk tatlısı." },

                    // Kurabiyeler
                    new Product { Name = "Damla Çikolatalı", Price = 450.00M, Stock = 30, CategoryId = kurabiyeler.Id, ImageUrl = cookieImg, Description = "Bol tereyağlı ve parça çikolatalı, çay saatlerinin vazgeçilmezi kıtır kurabiye." },
                    new Product { Name = "Elmalı Turta", Price = 480.00M, Stock = 18, CategoryId = kurabiyeler.Id, ImageUrl = applePieImg, Description = "Tarçın ve elmanın muhteşem uyumu, ev yapımı tadında." },
                    new Product { Name = "Tuzlu Kurabiye", Price = 420.00M, Stock = 25, CategoryId = kurabiyeler.Id, ImageUrl = savoryImg, Description = "Çörek otlu ve mahlepli, ağızda dağılan taze tuzlu kurabiyeler." },

                    // Özel Gün Pastaları
                    new Product { Name = "Kutlama Pastası", Price = 2500.00M, Stock = 5, CategoryId = ozelGun.Id, ImageUrl = "https://images.unsplash.com/photo-1535141192574-5d4897c12636?auto=format&fit=crop&q=80&w=800", Description = "Düğün ve nişanlarınız için özel tasarım, çok katlı kutlama pastası." },
                    new Product { Name = "Doğum Günü Pastası", Price = 1800.00M, Stock = 5, CategoryId = ozelGun.Id, ImageUrl = "https://images.unsplash.com/photo-1558301211-0d8c8ddee6ec?auto=format&fit=crop&q=80&w=800", Description = "Rengarenk şeker hamuru kaplamalı, içi sürpriz dolgulu doğum günü pastası." },

                    // Sütlü Tatlılar
                    new Product { Name = "Fırın Sütlaç", Price = 220.00M, Stock = 40, CategoryId = sutlu.Id, ImageUrl = "https://images.unsplash.com/photo-1621303837174-89787a7d4729?auto=format&fit=crop&q=80&w=800", Description = "Toprak kaplarda fırınlanmış, üzeri nar gibi kızarmış geleneksel sütlaç." },
                    new Product { Name = "Magnolia", Price = 250.00M, Stock = 35, CategoryId = sutlu.Id, ImageUrl = "https://images.unsplash.com/photo-1563805042-7684c019e1cb?auto=format&fit=crop&q=80&w=800", Description = "Taze çilekler ve bisküvi kırıntılarıyla hazırlanan hafif ve kremamsı lezzet." },

                    // Soğuk İçecekler
                    new Product { Name = "Ev Yapımı Limonata", Price = 120.00M, Stock = 100, CategoryId = icecek.Id, ImageUrl = "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?auto=format&fit=crop&q=80&w=800", Description = "Taze nanelerle ferahlatılmış, katkısız ev yapımı buz gibi limonata." },
                    new Product { Name = "Soğuk Kahve (Cold Brew)", Price = 150.00M, Stock = 50, CategoryId = icecek.Id, ImageUrl = "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&q=80&w=800", Description = "12 saat demlenmiş, yumuşak içimli soğuk kahve." }
                };

                foreach (var p in products)
                {
                    if (!context.Products.Any(prod => prod.Name == p.Name))
                    {
                        context.Products.Add(p);
                    }
                    else
                    {
                        // Update existing images while we are here, just in case
                         var existing = context.Products.First(prod => prod.Name == p.Name);
                         existing.ImageUrl = p.ImageUrl;
                         // existing.CategoryId = p.CategoryId; // Optional: fix category if we moved items
                    }
                }
                context.SaveChanges();
            }
        }
    }
}