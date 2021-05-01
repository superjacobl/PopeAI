namespace PopeAI
{
    public class ClientRole
    {

        /// <summary>
        /// The unique Id of this role
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// The name of the role
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The position of the role: Lower has more authority
        /// </summary>
        public uint Position { get; set; }

        /// <summary>
        /// The ID of the planet or system this role belongs to
        /// </summary>
        public ulong PlanetId { get; set; }

        /// <summary>
        /// The planet permissions for the role
        /// </summary>
        public ulong Permissions { get; set; }

        // RGB Components for role color
        public byte ColorRed { get; set; }

        public byte ColorGreen { get; set; }

        public byte ColorBlue { get; set; }

        // Formatting options
        public bool Bold { get; set; }

        public bool Italics { get; set; }

        public uint GetAuthority()
        {
            return uint.MaxValue - (1 + Position);
        }
    }
}
