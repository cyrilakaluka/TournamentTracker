using System;
using System.Collections.Generic;
using System.Text;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess
{
    public interface IDataConnection
    {
        void CreatePrize(PrizeModel model);
        void CreatePerson(PersonModel model);
        void CreateTeam(TeamModel model);
        void CreateTournament(TournamentModel model);
        void UpdateRound(RoundModel model);
        void UpdateMatchup(MatchupModel model);
        void UpdateMatchupEntry(MatchupEntryModel model);
        void DeactivateTournament(TournamentModel model);
        List<PersonModel> GetPerson_All();
        List<TournamentModel> GetTournament_All();
        List<TeamModel> GetTeam_All();
    }
}
