namespace PopeAI.Commands.Banking
{
    public class Banking : CommandModuleBase
    {
        Random rnd = new Random();
        [Group("bank")]
        public class Bank : CommandModuleBase
        {

            [Command("deposit")]
            public async Task deposit(CommandContext ctx)
            {

            }
        }

        [Command("bank")]
        public async Task bankview(CommandContext ctx)
        {
            
        }


    }
}