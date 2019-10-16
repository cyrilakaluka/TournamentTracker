using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary.Models
{
    /// <summary>
    /// Class representing a prize given at a tournament
    /// </summary>
    public class PrizeModel
    {
        /// <summary>
        /// Unique Identification for each model
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Integer representation of the tournament position the prize belongs to
        /// </summary>
        public int PlaceNumber { get; set; }

        /// <summary>
        /// String representation of the tournament position the prize belongs to
        /// </summary>
        public string PlaceName { get; set; }

        /// <summary>
        /// Monetary amount given out for this prize
        /// </summary>
        public decimal PrizeAmount { get; set; }

        /// <summary>
        /// Percentage amount given for this prize
        /// </summary>
        public double PrizePercentage { get; set; }

        public PrizeModel()
        {

        }

        public PrizeModel(string placeName, string placeNumber, string prizeAmount, string prizePercentage)
        {
            PlaceName = placeName;

            int.TryParse(placeNumber, out int placeNumberValue);
            PlaceNumber = placeNumberValue;

            decimal.TryParse(prizeAmount, out decimal prizeAmountValue);
            PrizeAmount = prizeAmountValue;

            double.TryParse(prizePercentage, out double prizePercentageValue);
            PrizePercentage = prizePercentageValue;
        }
    }
}