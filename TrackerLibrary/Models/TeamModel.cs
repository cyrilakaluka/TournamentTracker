using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary.Models
{
    /// <summary>
    /// Class representation of a team entered in the tournament
    /// </summary>
    public class TeamModel
    {
        public int Id { get; set; }
        /// <summary>
        /// Name of the team
        /// </summary>
        public string TeamName { get; set; }

        /// <summary>
        /// List of person members of a particular team
        /// </summary>
        public List<PersonModel> TeamMembers { get; set; } = new List<PersonModel>();
    }
}
