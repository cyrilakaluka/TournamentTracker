using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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

            bool isFinalRound = tournament.Rounds.Count == round.Number;

            if (!isFinalRound)
            {
                RoundModel nextRound = tournament.Rounds.First(r => r.Number == round.Number + 1);

                nextRound.Active = true;

                nextRound.Matchups.ForEach(m => m.Entries.ForEach(me =>
                {
                    me.TeamCompeting = me.ParentMatchup.Winner;
                    GlobalConfig.Connection.UpdateMatchupEntry(me);
                }));

                GlobalConfig.Connection.UpdateRound(nextRound);

                tournament.NotifyEnteredTeams(nextRound);
            }
            else
            {
                tournament.CompleteTournament();
                tournament.NotifyTournamentComplete();
            }
        }

        private static void CompleteTournament(this TournamentModel tournament)
        {
            GlobalConfig.Connection.DeactivateTournament(tournament);
            MatchupModel finalMatchup = tournament.Rounds.Last().Matchups.First();
            TeamModel winner = finalMatchup.Winner;
            TeamModel runnerUp = finalMatchup.Entries.First(e => e.TeamCompeting != winner).TeamCompeting;

            decimal winnerPrize = 0;
            decimal runnerUpPrize = 0;

            if (tournament.Prizes.Count > 0)
            {
                decimal totalIncome = tournament.EnteredTeams.Count * tournament.EntryFee;

                PrizeModel firstPlacePrize = tournament.Prizes.FirstOrDefault(p => p.PlaceNumber == 1);
                PrizeModel secondPlacePrize = tournament.Prizes.FirstOrDefault(p => p.PlaceNumber == 2);

                winnerPrize = firstPlacePrize?.CalculatePrizePayout(totalIncome) ?? 0;
                runnerUpPrize = secondPlacePrize?.CalculatePrizePayout(totalIncome) ?? 0;
            }

            string subject = $"In {tournament.TournamentName}, {winner.TeamName} has won!";
            StringBuilder body = new StringBuilder();

            body.AppendLine("<h1>We have a Winner!</h1>");
            body.AppendLine("<p>Congratulations to out winner on a great tournament.</p>");
            body.AppendLine("<br />");

            if (winnerPrize > 0)
            {
                body.AppendLine($"<p>{winner.TeamName} will receive ${winnerPrize}</p>");
            }

            if (runnerUpPrize > 0)
            {
                body.AppendLine($"<p>{runnerUp.TeamName} will receive {runnerUpPrize} as runner up</p>");
            }

            body.AppendLine("<p>Thanks to everyone for participating!</p>");
            body.AppendLine("<p>~Tournament Tracker</p>");

            List<string> bcc = new List<string>();
            tournament.EnteredTeams.ForEach(et => et.TeamMembers.Where(mb => !string.IsNullOrEmpty(mb.EmailAddress)).ToList().ForEach(p => bcc.Add(p.EmailAddress)));

            EmailLogic.SendEmail(new List<string>(), bcc, subject, body.ToString());
        }

        private static decimal CalculatePrizePayout(this PrizeModel prize, decimal totalIncome)
        {
            decimal output = 0;
            if (prize.PrizeAmount > 0)
            {
                output = prize.PrizeAmount;
            }
            else
            {
                output = decimal.Multiply(totalIncome, Convert.ToDecimal(prize.PrizePercentage / 100));
            }

            return output;
        }

        public static void NotifyEnteredTeams(this TournamentModel tournament, RoundModel activeRound)
        {
            activeRound.Matchups.ForEach(m => m.NotifyMatchupEntries(tournament.TournamentName));
        }

        private static void NotifyMatchupEntries(this MatchupModel matchup, string tournamentName)
        {
            matchup.Entries.ForEach(e => e.TeamCompeting.TeamMembers
                .ForEach(mb => mb.NotifyPersonToMatchup(
                    tournamentName, 
                    matchup.MatchupRound, 
                    e.TeamCompeting, 
                    matchup.Entries.FirstOrDefault(me => me.TeamCompeting != e.TeamCompeting)?.TeamCompeting)));
        }

        private static void NotifyPersonToMatchup(this PersonModel person, string tournamentName, int round, TeamModel team, TeamModel competitor)
        {
            if (string.IsNullOrEmpty(person.EmailAddress))
            {
                return;
            }

            string toAddress = person.EmailAddress;
            string subject = "";
            StringBuilder body = new StringBuilder();

            if (competitor != null)
            {
                subject = $"You have a new matchup with {competitor.TeamName}";
                body.AppendLine("<h1>You have a new matchup</h1>");
                body.Append("<strong>Competitor: </strong>");
                body.AppendLine(competitor.TeamName);
                body.AppendLine("\n\n");
                body.AppendLine("Have a great time!");
            }
            else
            {
                subject = "You have a bye week for this round";
                body.AppendLine("Enjoy your round off");
            }
            body.AppendLine("~Tournament Tracker");

            EmailLogic.SendEmail(toAddress, subject, body.ToString());
        }
    }
}
