using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary.Models
{
    /// <summary>
    /// Class representing a matchup entry for the tournament
    /// </summary>
    public class MatchupEntryModel
    {
        /// <summary>
        /// Unique Identification for each model
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The ID from the database that will be used to identify the competing team
        /// </summary>
        public int? TeamCompetingId { get; set; }

        /// <summary>
        /// Represents one team in the matchup
        /// </summary>
        public TeamModel TeamCompeting { get; set; }

        /// <summary>
        /// Represents the score for this particular team
        /// </summary>
        public double? Score { get; set; }

        /// <summary>
        /// The ID from the database that will be used to identify the parent matchup
        /// </summary>
        public int? ParentMatchupId { get; set; }

        /// <summary>
        /// Represents the matchup that this team came 
        /// from as the winner
        /// </summary>
        public MatchupModel ParentMatchup { get; set; }
    }
}