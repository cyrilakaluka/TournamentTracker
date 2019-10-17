using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    public static class TournamentLogic
    {
        public static void CreateRounds(TournamentModel model)
        {
            var randomizedTeamList = RandomizeTeamOrder(model.EnteredTeams);
            int rounds = FindNumberOfRounds(randomizedTeamList.Count);
            int byes = GetNumberOfByes(rounds, randomizedTeamList.Count);

            model.Rounds.Add(CreateFirstRound(byes, randomizedTeamList));

            CreateOtherRounds(model, rounds);

            #region Local Functions
            int FindNumberOfRounds(int teamCount)
            {
                int powerIndex = 0;

                while (1 << powerIndex < teamCount)
                {
                    powerIndex++;
                }

                return powerIndex;
            }

            int GetNumberOfByes(int numberOfRounds, int numberOfTeams)
            {
                return (1 << numberOfRounds) - numberOfTeams;
            }

            List<TeamModel> RandomizeTeamOrder(List<TeamModel> teams)
            {
                return teams.OrderBy(x => Guid.NewGuid()).ToList();
            } 
            #endregion
        }

        public static void UpdateMatchupResults(this MatchupModel matchup, double scoreTeamOne, double scoreTeamTwo)
        {
            if (matchup.Entries.Count == 1)
            {
                return;
            }

            bool parseValid = bool.TryParse(ConfigurationManager.AppSettings["greaterWins"], out bool greaterWins);

            if (!parseValid)
            {
                greaterWins = true;
            }

            UpdateWinner(matchup, greaterWins, scoreTeamOne, scoreTeamTwo);

            GlobalConfig.Connection.UpdateMatchup(matchup);
        }

        private static void CreateOtherRounds(TournamentModel model, int numRounds)
        {
            RoundModel previousRound = model.Rounds[0];
            RoundModel currentRound = new RoundModel(){ Number = 2, Active = null };
            MatchupModel matchup = new MatchupModel();

            while (currentRound.Number <= numRounds)
            {
                foreach (MatchupModel m in previousRound.Matchups)
                {
                    matchup.Entries.Add(new MatchupEntryModel{ ParentMatchup = m });

                    if (matchup.Entries.Count > 1)
                    {
                        matchup.MatchupRound = currentRound.Number;
                        currentRound.Matchups.Add(matchup);
                        matchup = new MatchupModel();
                    }
                }

                model.Rounds.Add(currentRound);
                previousRound = currentRound;
                currentRound = new RoundModel(){ Number = previousRound.Number + 1, Active = null };
            }
        }

        private static RoundModel CreateFirstRound(int numberOfByes, List<TeamModel> teams)
        {
            RoundModel output = new RoundModel()
            {
                Number = 1,
                Active = true
            };
            List<MatchupModel> matchups = new List<MatchupModel>();
            var matchup = new MatchupModel();

            foreach (TeamModel team in teams)
            {
                matchup.Entries.Add(new MatchupEntryModel { TeamCompeting = team });

                if (numberOfByes > 0 || matchup.Entries.Count > 1)
                {
                    if (numberOfByes > 0)
                    {
                        matchup.Winner = team;
                        numberOfByes--;
                    }
                    matchup.MatchupRound = 1;
                    output.Matchups.Add(matchup);
                    matchup = new MatchupModel();
                }
            }

            return output;
        }

        private static void UpdateWinner(MatchupModel matchup, bool greaterWins, double scoreTeamOne, double scoreTeamTwo)
        {
            if (greaterWins)
            {
                if (scoreTeamOne > scoreTeamTwo)
                {
                    matchup.Winner = matchup.Entries[0].TeamCompeting;
                }
                else if (scoreTeamTwo > scoreTeamOne)
                {
                    matchup.Winner = matchup.Entries[1].TeamCompeting;
                }
                else
                {
                    throw new Exception("No ties are allowed");
                }
            }
            else
            {
                if (scoreTeamOne < scoreTeamTwo)
                {
                    matchup.Winner = matchup.Entries[0].TeamCompeting;
                }
                else if (scoreTeamTwo < scoreTeamOne)
                {
                    matchup.Winner = matchup.Entries[1].TeamCompeting;
                }
                else
                {
                    throw new Exception("No ties are allowed");
                }
            }

            matchup.Entries[0].Score = scoreTeamOne;
            matchup.Entries[1].Score = scoreTeamTwo;
        }

        public static void UpdateTournamentRound(this TournamentModel tournament, RoundModel round)
        {
            bool roundCompleted = round.Matchups.All(m => m.Winner != null);

            if (!roundCompleted)
            {
                throw new Exception("Winners have not been assigned to matchups in this round");
            }

            round.Active = false;

            GlobalConfig.Connection.UpdateRound(round);

            bool finalRound = tournament.Rounds.Count == round.Number;

            if (!finalRound)
            {
                RoundModel nextRound = tournament.Rounds.First(r => r.Number == round.Number + 1);

                nextRound.Active = true;

                nextRound.Matchups.ForEach(m => m.Entries.ForEach(me =>
                {
                    me.TeamCompeting = me.ParentMatchup.Winner;
                    GlobalConfig.Connection.UpdateMatchupEntry(me);
                }));

                GlobalConfig.Connection.UpdateRound(nextRound);
            }
        }
    }
}
