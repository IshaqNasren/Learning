using System.Collections.Generic;
using Newtonsoft.Json;
using TodoApi.Model;

namespace TodoApi.Data
{
    public class Seed
    {
        private readonly DataContext _Context;
        public Seed(DataContext context)
        {
            _Context = context;
        }

        public void SeedUsers(){
            
            var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
            var users = JsonConvert.DeserializeObject<List<User>>(userData);
            foreach (var user in users)
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash("password", out passwordHash , out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.Username = user.Username.ToLower();

                _Context.Users.Add(user);
            }

            _Context.SaveChanges();

        }

          private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}