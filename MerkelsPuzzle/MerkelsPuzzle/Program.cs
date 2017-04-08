using MerkelsPuzzle.HelperClasses;

namespace MerkelsPuzzle
{
    class Program
    {
        static void Main(string[] args)
        {
            SendingPrincipal sendingPrincipal = new SendingPrincipal();
            ReceivingPrincipal receivingPrincipal = new ReceivingPrincipal();
            AttackerPrincipal attackerPrincipal = new AttackerPrincipal();

            //Alice Sends KeyedPuzzles To Bob
            
            var keyedPuzzles = sendingPrincipal.GetKeyedPuzzles(100000);

            //Eve Steals KeyedPuzzles From Alice 
            attackerPrincipal.StealKeyedPuzzles(keyedPuzzles);

            //Bob Receives KeyedPuzzles From Alice
            receivingPrincipal.ReceiveKeyedPuzzles(keyedPuzzles);

            //Bob Selects Index to be sent to Alice
            var index = receivingPrincipal.GetIndex();

            //Eve Steals Index From Bob
            attackerPrincipal.StealIndex(index);

            //Eve Mines KeyedPuzzles
            var secretKey = attackerPrincipal.MineKeyedPuzzles();
        }
    }
}
