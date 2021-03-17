using System;

namespace Moneybox.App.Domain
{
    public class User
    {
        private Guid id;
        private string name;
        private string email;

        public User(Guid userId, string userName, string emailAddress)
        {
            id = userId;
            name = userName;
            email = emailAddress;
        }

        public Guid Id { get => id; }

        public string Name { get => name; }

        public string Email { get => email; }
    }
}
