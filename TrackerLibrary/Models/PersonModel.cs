using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace TrackerLibrary.Models
{
    /// <summary>
    /// Class representation of each individual entered in the tournament
    /// </summary>
    public class PersonModel
    {

        /// <summary>
        /// Unique Identification for each model
        /// </summary>
        public int Id { get; set; }

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

        /// <summary>
        /// Combination of FirstName and LastName of a Person
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";
    }
}
