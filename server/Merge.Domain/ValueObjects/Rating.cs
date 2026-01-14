namespace Merge.Domain.ValueObjects;

public record Rating
{
    public int Value { get; }

    public Rating(int value)
    {
        if (value < 1 || value > 5)
            throw new ArgumentOutOfRangeException(nameof(value), "Rating must be between 1 and 5");

        Value = value;
    }

    public static Rating Minimum => new(1);
    public static Rating Maximum => new(5);
    public static Rating Average => new(3);

    public static Rating FromStars(int stars) => new(stars);

    public static Rating CalculateAverage(IEnumerable<Rating> ratings)
    {
        var ratingList = ratings.ToList();
        if (!ratingList.Any())
            throw new InvalidOperationException("Cannot calculate average of empty ratings");

        var average = ratingList.Average(r => r.Value);
        return new Rating((int)Math.Round(average));
    }

    public static decimal CalculateAverageDecimal(IEnumerable<Rating> ratings)
    {
        var ratingList = ratings.ToList();
        if (!ratingList.Any())
            throw new InvalidOperationException("Cannot calculate average of empty ratings");

        return Math.Round((decimal)ratingList.Average(r => r.Value), 1);
    }

    public bool IsPositive() => Value >= 4;
    public bool IsNeutral() => Value == 3;
    public bool IsNegative() => Value <= 2;

    public string ToStarString() => new string('★', Value) + new string('☆', 5 - Value);

    public static implicit operator int(Rating rating) => rating.Value;

    public override string ToString() => $"{Value}/5";
}
