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
        public ulong AuthorId { get; set; }

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
        public ulong ChannelId { get; set; }

        /// <summary>
        /// Index of the message
        /// </summary>
        public ulong MessageIndex { get; set; }

        public ulong PlanetId { get; set; }

        public async Task<ClientPlanetUser> GetAuthorAsync()
        {
            ClientPlanetUser planetUser = await PlanetUserCache.GetPlanetUserAsync(AuthorId, PlanetId);

            return planetUser;
        }

    }
}
