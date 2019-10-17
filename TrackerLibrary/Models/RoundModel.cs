using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{
    public class RoundModel
    {
        /// <summary>
        /// Unique Identification for each model
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Indicates the round number for the tournament
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Indicates whether the round is still active (i.e has been concluded or not)
        /// A null value indicates that the round has not yet been activated
        /// </summary>
        public bool? Active { get; set; }

        /// <summary>
        /// List containing matchups for the round
        /// </summary>
        public List<MatchupModel> Matchups = new List<MatchupModel>();
    }
}
