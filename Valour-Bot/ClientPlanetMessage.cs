using System;
using System.Threading.Tasks;

namespace PopeAI
{
    class ClientPlanetMessage
    {
        /// <summary>
        /// The Id of the message
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// The user's ID
        /// </summary>
        public ulong Author_Id { get; set; }

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
        public ulong Message_Index { get; set; }

        public ulong Planet_Id { get; set; }

        public async Task<ClientPlanetUser> GetAuthorAsync()
        {
            ClientPlanetUser planetUser = await PlanetUserCache.GetPlanetUserAsync(Author_Id, Planet_Id);

            return planetUser;
        }

    }
}
