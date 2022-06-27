using System.Security.Cryptography;
using System.Text;

namespace PopeAI.Database.Models.Messaging
{
    [Index(nameof(Content))]
    public class Message
    {
        /// <summary>
        /// The Id of the message
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// The user's ID
        /// </summary>
        public ulong Author_Id { get; set; }
        public ulong Member_Id { get; set; }
        public ulong? Planet_Index {get; set;}

        /// <summary>
        /// String representation of message
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The time the message was sent (in UTC)
        /// </summary>
        public DateTime TimeSent { get; set; }

        /// <summary>
        /// Id of the channel this message belonged to
        /// </summary>
        public ulong Channel_Id { get; set; }

        /// <summary>
        /// Index of the message
        /// </summary>
        public ulong MessageIndex { get; set; }

        public ulong Planet_Id { get; set; }
        public string EmbedData {get; set;}
        public string MentionsData { get; set; }
        public byte[]? Hash {get; set;}

        public NpgsqlTsVector SearchVector { get; set; }

        /// <summary>
        /// Returns the hash for a message.
        /// </summary>
        public byte[] GetHash()
        {
            using (SHA256 sha = SHA256.Create())
            {
                string conc = $"{Author_Id}{Content}{TimeSent}{Channel_Id}{MessageIndex}{EmbedData}{MentionsData}";

                byte[] buffer = Encoding.Unicode.GetBytes(conc);

                return sha.ComputeHash(buffer);
            }
        }
    }    
}