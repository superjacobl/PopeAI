using System;

namespace PopeAI.Commands.Banking;

public class Party
{
    public string Name { get; set; }
    public int Seats { get; set; }
    public string HexColor { get; set; }

    public Party(string name, int seats, string hexColor)
    {
        Name = name;
        Seats = seats;
        HexColor = hexColor;
    }
}

public class Parliament : CommandModuleBase
{
    public static List<Party> Parties = new()
    {
        new Party("New Vooperis People's Party (NVPP)",32 ,"C7ACE0"),
        new Party("New Vooperis Social Democratic Party (NVSDP)",11 , "1E46F7"),
		new Party("Free Democratic Party (FDP)",7 , "FFED00")
	};

    public static double pi = 3.14159265358979;

    public (int, int, double, string) Coords(double r, double b)
    {
        return new(
            (int)(r * Math.Cos(b / r - pi)),
			(int)(r * Math.Sin(b / r - pi)),
            0,
            ""
        );
    }

    public double calculateSeatDistance(int seatCount, int numberOfRings, double r)
    {
        double x = (pi * numberOfRings * r) / (seatCount - numberOfRings);

        double y = 1 + (pi * (numberOfRings - 1) * numberOfRings / 2) / (seatCount - numberOfRings);

        double a = x / y;
        return a;
	}

    public int score(int seatCount, int numberOfRings, double r)
    {
        return (int)Math.Abs(calculateSeatDistance(seatCount, numberOfRings, r) * numberOfRings / r - (5 / 7));
	}

    public int calculateNumberOfRings(int seatCount, double r)
    {
        int n = (int)Math.Floor(Math.Log(seatCount) / Math.Log(2));
        if (n == 0)
            n = 1;
        int distance = score(seatCount, n, r);
        int direction = 0;
        if (score(seatCount, n + 1, r) < distance)
            direction = 1;
        if (score(seatCount, n - 1, r) < distance && n > 1)
            direction = -1;

        while (score(seatCount, n + direction, r) < distance && n > 0)
        {
            distance = score(seatCount, n + direction, r);
            n += direction;
        }

        return n;
	}

    public List<int> SainteLague(List<double> Votes, int seats)
    {
        var representatives = Votes.Select(x => 0).ToList();
        var divisors = new List<int>();
		var weights = new List<List<double>>();
		double minwight = 0;

		for (int i = 0; i < seats; i++)
            divisors.Add(2 * i + 1);

        if (seats > representatives.Sum())
        {
            foreach(var p in Votes)
            {
                var l = new List<double>();
                foreach(var div in divisors)
                {
                    l.Add(p / div);
                }
                weights.Add(l);
            }
            var flatweights = weights.SelectMany(x => x).OrderByDescending(x => x).ToList();
            minwight = flatweights[seats - representatives.Sum() - 1];
            for (int i = 0; i < Votes.Count; i++)
            {
                representatives[i] += weights[i].Count(w => w > minwight);
            }
		}

		if (seats > representatives.Sum())
        {
            for (int i = 0; i < Votes.Count; i++)
            {
                if (representatives.Sum() < seats && weights[i].Contains(minwight))
                    representatives[i] += 1;
            }
        }
        return representatives;
	}

    public int nextRing(List<List<(int, int, double, string)>> rings, List<int> ringProgress)
    {
        double progressQuota = 0;
		double tQuote = 0;

        for (int index = 0; index < rings.Count; index++)
        {
            int ma = rings[index].Count;
            double poss = (double)ringProgress[index];
            if (poss == 0)
                poss = 0;
            poss = Math.Round(poss / ma, 10);
            tQuote = poss;
            if (progressQuota == 0 || tQuote < progressQuota)
                progressQuota = tQuote;
        }

		for (int index = 0; index < rings.Count; index++)
        {
			int ma = rings[index].Count;
			double poss = (double)ringProgress[index];
			if (poss == 0)
				poss = 0;
			poss = Math.Round(poss / ma, 10);
			tQuote = poss;
            if (tQuote == progressQuota)
            {
                while (rings[index].Count == ringProgress[index])
                {
                    index += 1;
                    if (index >= rings.Count)
                    {
                        index -= 1;
                        break;
                    }
                }
                return index;
            }
		}
        return 0;
	}

	[Command("graph1")]
    public async Task ViewGraph(CommandContext ctx)
    {
        int seatCount = 50;
        int r0 = 150;

        int numberofRings = calculateNumberOfRings(seatCount, r0);
        double seatDistance = calculateSeatDistance(seatCount, numberofRings, r0);

        var _rings = new List<double>();
        for (int i = 0; i < numberofRings; i++)
			_rings.Add(r0 - ((i - 1) * seatDistance));

        var rings = SainteLague(_rings, seatCount);

        var points = new List<List<(int, int, double, string)>>();

        for (int i = 0; i < numberofRings; i++)
        {
            var ring = new List<(int, int, double, string)>();

            double r = r0 - ((i - 1) * seatDistance);

            double b = (rings[i] - 1);
            if (b == 0)
                b = 1;

            double a = (pi * r) / b;

            for (int j = 0; j < (int)rings[i]; j++)
            {
                var point = Coords(r, j * a);
                point.Item3 = 0.4 * seatDistance;
                ring.Add(point);
            }
            points.Add(ring);
        }

        int ii = 0;
        var ringProgress = points.Select(x => 0).ToList();

        foreach(var party in Parties)
        {
            for (int i = 0; i < party.Seats; i++)
            {
				if (points[0].Count <= ringProgress[0])
					Console.WriteLine("gdgd");
				var ring = nextRing(points, ringProgress);
                if (points[ring].Count <= ringProgress[ring])
                    Console.WriteLine("gdgd");
				var a = points[ring][ringProgress[ring]];
				points[ring][ringProgress[ring]] = new(a.Item1, a.Item2, a.Item3, party.HexColor);
                ringProgress[ring]++;
            }
        }

        var embed = new EmbedBuilder()
            .AddPage()
                .WithStyles(
                    new Width(new Size(Unit.Pixels, 400)),
                    new Height(new Size(Unit.Pixels, 205))
                 );

        foreach(var ring in points)
        {
            foreach(var point in ring)
            {
                int x = 190+point.Item1;
                int y = 150+point.Item2;
                if (point.Item4 == "")
                    Console.WriteLine("ffeff");
                embed.AddText(".")
                    .WithStyles(
                        new Position(left: new Size(Unit.Pixels, x), top: new Size(Unit.Pixels, y)),
                        new FontSize(new Size(Unit.Pixels, (int)point.Item3*10)),
                        new TextColor(point.Item4)
                    );
            }
        }

        ctx.ReplyAsync(embed);
	}
}