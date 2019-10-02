using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary.Models
{
    /// <summary>
    /// Class representation of each individual entered in the tournament
    /// </summary>
    public class PersonModel
    {
        /// <summary>
        /// First name of the person
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the person
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Email address of the person
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Phone number of the person
        /// </summary>
        public string CellphoneNumber { get; set; }
    }
}
