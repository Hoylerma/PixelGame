using System;

namespace KettleCog.Simulation
{
    /// <summary>The brewery's coins. Refunds restore coins without counting as income.</summary>
    public sealed class Wallet
    {
        public Wallet(long startingCoins)
        {
            if (startingCoins < 0) throw new ArgumentOutOfRangeException(nameof(startingCoins), startingCoins, "Starting coins must not be negative.");
            Coins = startingCoins;
        }

        public long Coins { get; private set; }

        /// <summary>Total income from sales and contracts, for the Ledger and achievements.</summary>
        public long LifetimeEarned { get; private set; }

        public event Action<long> Changed;

        public bool CanAfford(long amount) => amount <= Coins;

        public bool TrySpend(long amount)
        {
            RequireNonNegative(amount);
            if (amount > Coins) return false;
            Coins -= amount;
            Changed?.Invoke(Coins);
            return true;
        }

        public void Earn(long amount)
        {
            RequireNonNegative(amount);
            Coins += amount;
            LifetimeEarned += amount;
            Changed?.Invoke(Coins);
        }

        public void Refund(long amount)
        {
            RequireNonNegative(amount);
            Coins += amount;
            Changed?.Invoke(Coins);
        }

        static void RequireNonNegative(long amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must not be negative.");
        }
    }
}
