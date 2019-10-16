                                                                                                                                                           using System;
using System.Collections.Generic;
                                                                                                                                                           using System.Linq;
                                                                                                                                                           using System.Text;

namespace TrackerLibrary.Models
{
    /// <summary>
    /// Class representing a matchup between competing teams
    /// </summary>
    public class MatchupModel
    {
        /// <summary>
        /// Unique Identification for each model
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Contains a list of matchup entries for one matchup
        /// </summary>
        public List<MatchupEntryModel> Entries { get; set; } = new List<MatchupEntryModel>();

        /// <summary>
        /// The ID from the database that will be used to identify the winner.
        /// </summary>
        public int? WinnerId { get; set; }

        /// <summary>
        /// Represents the winner of a particular matchup
        /// </summary>
        public TeamModel Winner { get; set; }

        /// <summary>
        /// Specifies the round in which a matchup was played
        /// </summary>
        public int MatchupRound { get; set; }

        public string DisplayName
        {
            get
            {
                if (Entries.Count > 0)
                {
                    if (Entries.Count > 1)
                    {
                        if (Entries[0].TeamCompeting != null && Entries[1].TeamCompeting != null)
                        {
                            return $"{Entries[0].TeamCompeting.TeamName} vs. {Entries[1].TeamCompeting.TeamName}";
                        }
                    }
                    else
                    {
                        return $"{Entries[0].TeamCompeting.TeamName}";
                    }
                }

                return "Matchup Not Yet Determined";
            }
        }
    }
}