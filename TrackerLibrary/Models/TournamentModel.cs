﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrackerLibrary.Models
{
    /// <summary>
    /// Class representation of the tournament
    /// </summary>
    public class TournamentModel
    {
        public event EventHandler<DateTime> TournamentCompleted;

        /// <summary>
        /// Unique Identification for each model
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the tournament
        /// </summary>
        public string TournamentName { get; set; }

        /// <summary>
        /// Amount paid to gain entry to the tournament
        /// </summary>
        public decimal EntryFee { get; set; }

        /// <summary>
        /// List of teams entered in the tournament
        /// </summary>
        public List<TeamModel> EnteredTeams { get; set; } = new List<TeamModel>();

        /// <summary>
        /// List of prizes awarded in the tournament
        /// </summary>
        public List<PrizeModel> Prizes { get; set; } = new List<PrizeModel>();

        /// <summary>
        /// List of rounds that make up the tournament
        /// </summary>
        public List<RoundModel> Rounds { get; set; } = new List<RoundModel>();

        public void NotifyTournamentComplete()
        {
            TournamentCompleted?.Invoke(this, DateTime.Now);
        }
    }
}
