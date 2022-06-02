namespace Finance.Api.Domain
{
    public class Accout
    {
        private double balance;
        public void Withdraw(double amount)
        {
            if (balance >= amount)
            {
                balance -= amount;
            }
        }

        public void Deposit(double amount)
        {
            balance += amount;
        }
    }
}
