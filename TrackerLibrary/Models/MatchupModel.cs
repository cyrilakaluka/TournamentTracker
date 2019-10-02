using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary.Models
{
    /// <summary>
    /// Class representing a matchup between competing teams
    /// </summary>
    public class MatchupModel
    {
        /// <summary>
        /// Contains a list of matchup entries for one matchup
        /// </summary>
        public List<MatchupEntryModel> Entries { get; set; } = new List<MatchupEntryModel>();

        /// <summary>
        /// Represents the winner of a particular matchup
        /// </summary>
        public TeamModel Winner { get; set; }

        /// <summary>
        /// Specifies the round in which a matchup was played
        /// </summary>
        public int MatchupRound { get; set; }
    }
}