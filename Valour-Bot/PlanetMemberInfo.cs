using System.Collections.Generic;

namespace PopeAI
{
    public class PlanetMemberInfo
    {
        public ClientMember Member { get; set; }
        public string State { get; set; }
        public List<ulong> RoleIds { get; set; }
    }
}
