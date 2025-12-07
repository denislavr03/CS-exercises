using System;

namespace NewChatSystem
{
    public class Participant
    {
        private static readonly Random _rng = new Random();
        private string _username = string.Empty;

        public string Username
        {
            get => _username;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    _username = $"Guest_{_rng.Next(100, 999)}";
                else
                    _username = value;
            }
        }

        public Participant(string? id = null)
        {
            Username = id;
        }

        public override string ToString() => Username;
    }
}