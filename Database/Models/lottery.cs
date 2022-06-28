using System;
using System.Collections.Generic;
/* namespace PopeAI.Database.Models.Lottery
{
    public class Lottery
    {
        [Key]
        public ulong PlanetId {get; set;}
        public double Jackpot {get; set;}
        public double JackpotIncreasePerMesage {get; set;}
        public DateTime EndDate {get; set;}
        public DateTime StartDate {get; set;}
        public string Type {get; set;}
        public ulong ChannelId {get; set;}

        public async Task AddTickets(ulong UserId, ulong amount, ulong planetid, PopeAIDB Context) {
            string TicketId = $"{planetid}-{UserId}";
            LotteryTicket ticket = await Context.LotteryTickets.FirstOrDefaultAsync(x => x.UserId == UserId && x.PlanetId == planetid);
            if (ticket == null) {
                ticket = new LotteryTicket();
                ticket.Id = TicketId;
                ticket.PlanetId = planetid;
                ticket.UserId = UserId;
                ticket.Tickets = 1;
                await Context.LotteryTickets.AddAsync(ticket);
            }
            else {
                ticket.Tickets += 1;
            }
            await Context.SaveChangesAsync();
        }
    }
}
*/