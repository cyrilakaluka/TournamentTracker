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
        }

        public static void UpdateTournamentResults(this TournamentModel tournament, MatchupModel matchup, int round)
        {
            bool parseValid = bool.TryParse(ConfigurationManager.AppSettings["greaterWins"], out bool greaterWins);

            if (!parseValid)
            {
                greaterWins = true;
            }

            UpdateWinner(matchup, greaterWins);

            GlobalConfig.Connection.UpdateMatchup(matchup);

            UpdateMatchupEntryTeamCompeting(tournament.Rounds[round], matchup);

            if (round == 1)
            {
                CheckFirstRoundCompleteAndUpdateByes(tournament);
            }
        }

        private static void CreateOtherRounds(TournamentModel model, int rounds)
        {
            int round = 2;
            List<MatchupModel> previousRound = model.Rounds[0];
            List<MatchupModel> currentRound = new List<MatchupModel>();
            MatchupModel matchup = new MatchupModel();

            while (round <= rounds)
            {
                foreach (MatchupModel m in previousRound)
                {
                    matchup.Entries.Add(new MatchupEntryModel{ ParentMatchup = m });

                    if (matchup.Entries.Count > 1)
                    {
                        matchup.MatchupRound = round;
                        currentRound.Add(matchup);
                        matchup = new MatchupModel();
                    }
                }

                model.Rounds.Add(currentRound);
                previousRound = currentRound;
                currentRound = new List<MatchupModel>();
                round++;
            }
        }

        private static List<MatchupModel> CreateFirstRound(int numberOfByes, List<TeamModel> teams)
        {
            List<MatchupModel> output = new List<MatchupModel>();
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
                    output.Add(matchup);
                    matchup = new MatchupModel();
                }
            }

            return output;
        }

        private static void UpdateWinner(MatchupModel matchup, bool greaterWins)
        {
            if (greaterWins)
            {
                if (matchup.Entries[0].Score > matchup.Entries[1].Score)
                {
                    matchup.Winner = matchup.Entries[0].TeamCompeting;
                }
                else if (matchup.Entries[1].Score > matchup.Entries[0].Score)
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
                if (matchup.Entries[0].Score < matchup.Entries[1].Score)
                {
                    matchup.Winner = matchup.Entries[0].TeamCompeting;
                }
                else if (matchup.Entries[1].Score < matchup.Entries[0].Score)
                {
                    matchup.Winner = matchup.Entries[1].TeamCompeting;
                }
                else
                {
                    throw new Exception("No ties are allowed");
                }
            }
        }

        private static void CheckFirstRoundCompleteAndUpdateByes(TournamentModel tournament)
        {
            bool scoringComplete = tournament.Rounds[0].Count(x => x.Winner == null) == 0;

            if (scoringComplete)
            {
                foreach (var m in tournament.Rounds[1])
                {
                    foreach (var entry in m.Entries)
                    {
                        if (entry.TeamCompeting == null)
                        {
                            entry.TeamCompeting = entry.ParentMatchup.Winner;
                        }
                    }
                }
            }
        }

        private static void UpdateMatchupEntryTeamCompeting(List<MatchupModel> roundMatchups, MatchupModel matchup)
        {
            foreach (MatchupModel m in roundMatchups)
            {
                foreach (MatchupEntryModel me in m.Entries)
                {
                    if (me.ParentMatchup?.Id == matchup.Id)
                    {
                        me.TeamCompeting = matchup.Winner;
                        GlobalConfig.Connection.UpdateMatchup(m);
                        var otherEntry = m.Entries.Find(x => x.Id != me.Id);

                        if (!(otherEntry?.ParentMatchup?.Entries.Count > 1))
                        {
                            if (otherEntry?.ParentMatchup != null)
                            {
                                otherEntry.TeamCompeting = otherEntry.ParentMatchup.Entries[0].TeamCompeting;
                            }
                        }

                        return;
                    }
                }
            }
        }
    }
}
