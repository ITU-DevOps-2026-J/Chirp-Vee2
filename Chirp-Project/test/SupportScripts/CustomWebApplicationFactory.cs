using Core.Model;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Web;

namespace SupportScripts;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
 protected override void ConfigureWebHost(IWebHostBuilder builder)
 {
  builder.UseEnvironment("Testing");

 var connection = new SqliteConnection("DataSource=:memory:");
 connection.Open();

 builder.ConfigureTestServices(services =>
 {
 var dbOptions = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ChatDbContext>));
 if (dbOptions != null) services.Remove(dbOptions);

 services.AddSingleton(connection);
 services.AddDbContext<ChatDbContext>((sp, options) =>
 options.UseSqlite(sp.GetRequiredService<SqliteConnection>()));

 using var scope = services.BuildServiceProvider().CreateScope();
 var context = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
 context.Database.EnsureCreated();

 if (!context.Authors.Any())
 {
 var a1 = new Author() { AuthorId =1, Name = "Helge", Email = "ropf@itu.dk", Cheeps = new List<Cheep>(), Follows = new(), CheepLikes = new() };
 var a2 = new Author { AuthorId =2, Name = "Adrian", Email = "adho@itu.dk", Cheeps = new List<Cheep>(), Follows = new(), CheepLikes = new() };

 var c1 = new Cheep { CheepId =1, AuthorId =1, Author = a1, Text = "Join itu lan now", TimeStamp = DateTime.Parse("2023-08-01 13:14:37"), PeopleLikes = new() };
 var c2 = new Cheep { CheepId =2, AuthorId =2, Author = a2, Text = "test answer", TimeStamp = DateTime.Parse("2023-08-01 13:15:21"), PeopleLikes = new() };
 var c3 = new Cheep { CheepId =3, AuthorId =1, Author = a1, Text = "Madeleine says i make propaganda", TimeStamp = DateTime.Parse("2023-08-01 13:14:58"), PeopleLikes = new() };
 var c4 = new Cheep { CheepId =4, AuthorId =1, Author = a1, Text = "Vee says i make propaganda", TimeStamp = DateTime.Parse("2023-08-01 13:14:58"), PeopleLikes = new() };

 
 a1.Cheeps = new List<Cheep>() { c1, c3, c4 };
 a2.Cheeps = new List<Cheep>() { c2 };
 a1.Follows = new() {2 };
 a1.CheepLikes = new() {4,1,3 };
 a2.CheepLikes = new() {1 };

 c1.PeopleLikes = new() {1,2 };
 c3.PeopleLikes = new() {1 };
 c4.PeopleLikes = new() {1 };

 context.Authors.AddRange(a1, a2);
 context.Cheeps.AddRange(c1, c2, c3, c4);
 context.SaveChanges();
 }
 });
 }
}
