namespace PopeAI.Database.Managers;

public static class Functions
{
    public static (long muit, double amount, string symbol) GetValues(double num, bool NoK = false)
    {
        long muit = 1;
        string symbol = "";
        if (num > 0)
            if (NoK)
            {
                if (num > 1_000_000)
                {
                    muit *= 1_000_000;
                    num /= 1_000_000;
                    symbol = "m";
                    if (num > 1000)
                    {
                        muit *= 1000;
                        num /= 1000;
                        symbol = "b";
                    }
                    if (num > 1000)
                    {
                        muit *= 1000;
                        num /= 1000;
                        symbol = "t";
                    }
                    if (num > 1000)
                    {
                        muit *= 1000;
                        num /= 1000;
                        symbol = "q";
                    }
                    if (num > 1000)
                    {
                        muit *= 1000;
                        num /= 1000;
                        symbol = "Q";
                    }
                    if (num > 1000)
                    {
                        muit *= 1000;
                        num /= 1000;
                        symbol = "s";
                    }
                    if (num > 1000)
                    {
                        muit *= 1000;
                        num /= 1000;
                        symbol = "S";
                    }
                    if (num > 1000)
                    {
                        muit *= 1000;
                        num /= 1000;
                        symbol = "o";
                    }
                }
            }
            else
            {
                if (num > 1000)
                {
                    muit = 1000;
                    num /= 1000;
                    symbol = "k";
                }
                if (num > 1000)
                {
                    muit *= 1000;
                    num /= 1000;
                    symbol = "m";
                }
                if (num > 1000)
                {
                    muit *= 1000;
                    num /= 1000;
                    symbol = "b";
                }
                if (num > 1000)
                {
                    muit *= 1000;
                    num /= 1000;
                    symbol = "t";
                }
                if (num > 1000)
                {
                    muit *= 1000;
                    num /= 1000;
                    symbol = "q";
                }
                if (num > 1000)
                {
                    muit *= 1000;
                    num /= 1000;
                    symbol = "Q";
                }
                if (num > 1000)
                {
                    muit *= 1000;
                    num /= 1000;
                    symbol = "s";
                }
                if (num > 1000)
                {
                    muit *= 1000;
                    num /= 1000;
                    symbol = "S";
                }
                if (num > 1000)
                {
                    muit *= 1000;
                    num /= 1000;
                    symbol = "o";
                }
            }
        else
        {
            double nn = num * -1;
            if (NoK)
            {
                if (nn > 1_000_000)
                {
                    muit *= 1_000_000;
                    nn /= 1_000_000;
                    symbol = "m";
                    if (nn > 1000)
                    {
                        muit *= 1000;
                        nn /= 1000;
                        symbol = "b";
                    }
                }
            }
            else
            {
                if (nn > 1000)
                {
                    muit = 1000;
                    nn /= 1000;
                    symbol = "k";
                }
                if (nn > 1000)
                {
                    muit *= 1000;
                    nn /= 1000;
                    symbol = "m";
                }
                if (nn > 1000)
                {
                    muit *= 1000;
                    nn /= 1000;
                    symbol = "b";
                }
            }
            nn *= -1;
            return (muit, nn, symbol);
        }
        return (muit, num, symbol);
    }

    public static string Format(decimal Value, bool AddPlusSign = false, bool WholeNum = false, int Rounding = 2, bool NoK = false, string ExtraSymbol = "")
    {
        var data = GetValues((double)Value, NoK);
        if (WholeNum)
        {
            long amount = (long)data.amount;
            if (AddPlusSign)
            {
                if (amount >= 0)
                    return $"+{ExtraSymbol}{amount.ToString("#,##0")}{data.symbol}";
                return $"{ExtraSymbol}{amount.ToString("#,##0")}{data.symbol}";
            }
            return $"{ExtraSymbol}{amount.ToString("#,##0")}{data.symbol}";
        }
        else
        {
            double amount = Math.Round(data.amount, Rounding);
            if (AddPlusSign)
            {
                if (amount >= 0)
                    return $"+{ExtraSymbol}{amount.ToString("#,##0.##")}{data.symbol}";
                return $"{ExtraSymbol}{amount.ToString("#,##0.##")}{data.symbol}";
            }
            return $"{ExtraSymbol}{amount.ToString("#,##0.##")}{data.symbol}";
        }
    }

    public static string Format(double Value, bool AddPlusSign = false, bool WholeNum = false, int Rounding = 2, bool NoK = false, string ExtraSymbol = "", bool Under1KNoDecimals = false)
    {
        var data = GetValues(Value, NoK);
        if (WholeNum)
        {
            long amount = (long)data.amount;
            if (AddPlusSign)
            {
                if (amount >= 0)
                    return $"+{ExtraSymbol}{amount.ToString("#,##0")}{data.symbol}";
                return $"{ExtraSymbol}{amount.ToString("#,##0")}{data.symbol}";
            }
            return $"{ExtraSymbol}{amount.ToString("#,##0")}{data.symbol}";
        }
        else
        {
            double amount = Math.Round(data.amount, Rounding);
            string places = new string('#', Rounding);
            if (Under1KNoDecimals)
                places = "";
            if (AddPlusSign)
            {
                if (amount >= 0)
                    return $"+{ExtraSymbol}{amount.ToString("#,##0.##")}{data.symbol}";
                return $"{ExtraSymbol}{amount.ToString("#,##0.##")}{data.symbol}";
            }
            return $"{ExtraSymbol}{amount.ToString($"#,##0.{places}")}{data.symbol}";
        }
    }

    public static string Format(long Value, bool AddPlusSign = false, bool WholeNum = false, int Rounding = 2, bool NoK = false, string ExtraSymbol = "")
    {
        return Format((decimal)Value, AddPlusSign, WholeNum, Rounding, NoK, ExtraSymbol);
    }
}
