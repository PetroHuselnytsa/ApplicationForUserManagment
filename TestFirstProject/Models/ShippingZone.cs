namespace TestFirstProject.Models
{
    /// <summary>
    /// Simplified shipping zone with flat-rate pricing by weight bracket.
    /// </summary>
    public class ShippingZone
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        /// <summary>Base cost for this zone (Standard shipping).</summary>
        public decimal BaseCost { get; set; }

        /// <summary>Additional cost per kg above 1 kg.</summary>
        public decimal CostPerKg { get; set; }

        /// <summary>Multiplier for Express shipping (e.g. 2.0 = double).</summary>
        public decimal ExpressMultiplier { get; set; } = 2.0m;

        /// <summary>Multiplier for SameDay shipping.</summary>
        public decimal SameDayMultiplier { get; set; } = 3.0m;

        /// <summary>Estimated days for Standard delivery.</summary>
        public int StandardDeliveryDays { get; set; } = 5;

        /// <summary>Estimated days for Express delivery.</summary>
        public int ExpressDeliveryDays { get; set; } = 2;
    }
}
