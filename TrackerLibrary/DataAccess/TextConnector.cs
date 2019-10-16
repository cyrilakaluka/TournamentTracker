using System;
using System.Collections.Generic;
using System.Text;
using TrackerLibrary.Models;
using TrackerLibrary.DataAccess.TextHelpers;
using System.Linq;

namespace TrackerLibrary.DataAccess
{
    public class TextConnector : IDataConnection
    {
        public void CreatePrize(PrizeModel model)
        {
            // Load the text file and convert the text to List<PrizeModel>
            List<PrizeModel> prizes = GlobalConfig.PrizesFile.GetFullFilePath().LoadFile().ConvertToPrizeModels();

            // Find the max ID
            int currentId = 1;

            if (prizes.Count > 0)
            {
                currentId = prizes.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;

            // Add the new record with the new ID
            prizes.Add(model);

            // Convert the prizes to List<string>
            // Save the List<string> to the text file
            prizes.SaveToPrizesFile();
        }

        public void CreatePerson(PersonModel model)
        {
            List<PersonModel> persons = GlobalConfig.PersonsFile.GetFullFilePath().LoadFile().ConvertToPersonModels();

            int currentId = 1;

            if (persons.Count > 0)
            {
                currentId = persons.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;

            persons.Add(model);

            persons.SaveToPersonsFile();
        }

        public void CreateTeam(TeamModel model)
        {
            List<TeamModel> teams = GlobalConfig.TeamsFile.GetFullFilePath().LoadFile().ConvertToTeamModels();

            // Find the max ID
            int currentId = 1;

            if (teams.Count > 0)
            {
                currentId = teams.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;

            teams.Add(model);

            teams.SaveToTeamsFile();
        }

        public void CreateTournament(TournamentModel model)
        {
            List<TournamentModel> tournaments = GlobalConfig.TournamentsFile
                .GetFullFilePath()
                .LoadFile()
                .ConvertToTournamentModels();

            int currentId = 1;

            if (tournaments.Count > 0)
            {
                currentId = tournaments.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;

            model.SaveRoundsToFile();

            tournaments.Add(model);

            tournaments.SaveToTournamentsFile();
        }

        public void UpdateMatchup(MatchupModel model)
        {
            model.UpdateMatchupToFile();
        }

        public List<PersonModel> GetPerson_All()
        {
            return GlobalConfig.PersonsFile.GetFullFilePath().LoadFile().ConvertToPersonModels();
        }

        public List<TournamentModel> GetTournament_All()
        {
            return GlobalConfig.TournamentsFile
                .GetFullFilePath()
                .LoadFile()
                .ConvertToTournamentModels();
        }

        public List<TeamModel> GetTeam_All()
        {
            return GlobalConfig.TeamsFile.GetFullFilePath().LoadFile().ConvertToTeamModels();
        }
    }
}
