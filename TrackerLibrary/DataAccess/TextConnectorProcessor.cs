using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess.TextHelpers
{
    public static class TextConnectorProcessor
    {
        public static string GetFullFilePath(this string fileName)
        {
            return $"{ ConfigurationManager.AppSettings["filePath"] }\\{ fileName }";
        }

        public static List<string> LoadFile(this string file)
        {
            if (!File.Exists(file))
            {
                return new List<string>();
            }

            return File.ReadAllLines(file).ToList();
        }

        public static List<PrizeModel> ConvertToPrizeModels(this List<string> lines)
        {
            List<PrizeModel> output = new List<PrizeModel>();

            foreach(string line in lines)
            {
                string[] cols = line.Split(',');

                PrizeModel p = new PrizeModel
                {
                    Id = int.Parse(cols[0]),
                    PlaceNumber = int.Parse(cols[1]),
                    PlaceName = cols[2],
                    PrizeAmount = decimal.Parse(cols[3]),
                    PrizePercentage = double.Parse(cols[4])
                };
                output.Add(p);
            }
            return output;
        }

        public static List<PersonModel> ConvertToPersonModels(this List<string> lines)
        {
            List<PersonModel> output = new List<PersonModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                PersonModel p = new PersonModel
                {
                    Id = int.Parse(cols[0]),
                    FirstName = cols[1],
                    LastName = cols[2],
                    EmailAddress = cols[3],
                    CellphoneNumber = cols[4]
                };
                output.Add(p);
            }

            return output;
        }

        public static List<TeamModel> ConvertToTeamModels(this List<string> lines)
        {
            List<TeamModel> output = new List<TeamModel>();
            List<PersonModel> persons = GlobalConfig.PersonsFile.GetFullFilePath().LoadFile().ConvertToPersonModels();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TeamModel t = new TeamModel
                {
                    Id = int.Parse(cols[0]),
                    TeamName = cols[1]
                };

                string[] personIds = cols[2].Split('|');

                foreach (var personId in personIds)
                {
                    t.TeamMembers.Add(persons.First(p => p.Id == int.Parse(personId)));
                }

                output.Add(t);
            }

            return output;
        }

        public static List<TournamentModel> ConvertToTournamentModels(this List<string> lines)
        {
            // Format: TournamentId,TournamentName,EntryFee,(EnteredTeams = id|id|id|...),(Prizes = id|id|id|...),(Rounds - id|id|id|...)
            List<TournamentModel> output = new List<TournamentModel>();
            List<TeamModel> teams = GlobalConfig.TeamsFile.GetFullFilePath().LoadFile().ConvertToTeamModels();
            List<PrizeModel> prizes = GlobalConfig.PrizesFile.GetFullFilePath().LoadFile().ConvertToPrizeModels();
            List<RoundModel> rounds = GlobalConfig.RoundsFile.GetFullFilePath().LoadFile().ConvertToRoundModels();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TournamentModel tournament = new TournamentModel
                {
                    Id = int.Parse(cols[0]),
                    TournamentName = cols[1],
                    EntryFee = decimal.Parse(cols[2]),
                };

                string[] teamsIds = cols[3].Split('|');

                foreach (var teamId in teamsIds)
                {
                    tournament.EnteredTeams.Add(teams.First(t => t.Id == int.Parse(teamId)));
                }

                string[] prizeIds= cols[4].Split(new []{'|'}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var prizeId in prizeIds)
                {
                    tournament.Prizes.Add(prizes.First(p => p.Id == int.Parse(prizeId)));
                }

                string[] roundIds = cols[5].Split('|');

                foreach (string roundId in roundIds)
                {
                    tournament.Rounds.Add(rounds.First(r => r.Id == int.Parse(roundId)));
                }

                foreach (var round in tournament.Rounds)
                {
                    foreach (var matchup in round.Matchups)
                    {
                        if (matchup.WinnerId.HasValue)
                        {
                            matchup.Winner = teams.First(t => t.Id == matchup.WinnerId);
                        }

                        foreach (var entry in matchup.Entries)
                        {
                            if (entry.TeamCompetingId.HasValue)
                            {
                                entry.TeamCompeting = teams.First(t => t.Id == entry.TeamCompetingId);
                            }
                        }
                    }
                }

                output.Add(tournament);
            }

            return output;
        }

        public static List<RoundModel> ConvertToRoundModels(this List<string> lines)
        {
            List<RoundModel> output = new List<RoundModel>();

            List<MatchupModel> matchups =
                GlobalConfig.MatchupsFile.GetFullFilePath().LoadFile().ConvertToMatchupModels();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                RoundModel round = new RoundModel();
                round.Id = int.Parse(cols[0]);
                round.Number = int.Parse(cols[1]);
                round.Active = cols[2] == "" ? null : (bool?)bool.Parse(cols[2]);

                string[] matchupIds = cols[3].Split('|');

                foreach (string matchupId in matchupIds)
                {
                    round.Matchups.Add(matchups.First(m => m.Id == int.Parse(matchupId)));
                }

                output.Add(round);
            }

            return output;
        }

        public static List<MatchupModel> ConvertToMatchupModels(this List<string> lines)
        {
            List<MatchupModel> output = new List<MatchupModel>();

            List<MatchupEntryModel> matchupEntries = GlobalConfig.MatchupEntriesFile.GetFullFilePath().LoadFile().ConvertToMatchupEntryModels();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                MatchupModel matchup = new MatchupModel();
                matchup.Id = int.Parse(cols[0]);

                string[] entryIds = cols[1].Split('|');

                foreach (string entryId in entryIds)
                {
                    matchup.Entries.Add(matchupEntries.First(m => m.Id == int.Parse(entryId)));
                }

                matchup.WinnerId = cols[2] == "" ? null : (int?)int.Parse(cols[2]);

                matchup.MatchupRound = int.Parse(cols[3]);

                output.Add(matchup);
            }

            matchupEntries.ForEach(e => e.ParentMatchup = e.ParentMatchupId.HasValue ? output.Find(m => e.ParentMatchupId == m.Id) : null);

            return output;
        }

        public static List<MatchupEntryModel> ConvertToMatchupEntryModels(this List<string> lines)
        {
            List<MatchupEntryModel> output = new List<MatchupEntryModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                MatchupEntryModel entry = new MatchupEntryModel();

                entry.Id = int.Parse(cols[0]);

                entry.TeamCompetingId = cols[1] == "" ? null : (int?)(int.Parse(cols[1]));

                entry.Score = cols[2] == "" ? null : (double?)double.Parse(cols[2]);

                entry.ParentMatchupId = cols[3] == "" ? null : (int?)int.Parse(cols[3]);

                output.Add(entry);
            }

            return output;
        }

        public static List<MatchupEntryModel> GetMatchupEntryModelList(this List<MatchupModel> matchups)
        {
            List<MatchupEntryModel> output = new List<MatchupEntryModel>();

            matchups.ForEach(m => m.Entries.ForEach(e => output.Add(e)));

            return output;
        }

        public static List<MatchupModel> GetMatchupModelList(this List<RoundModel> rounds)
        {
            List<MatchupModel> output = new List<MatchupModel>();

            rounds.ForEach(r => r.Matchups.ForEach(m => output.Add(m)));

            return output;
        }

        public static void SaveToPrizesFile(this List<PrizeModel> models)
        {
            List<string> lines = new List<string>();

            foreach (var p in models)
            {
                lines.Add($"{ p.Id },{ p.PlaceNumber },{ p.PlaceName },{ p.PrizeAmount },{ p.PrizePercentage }");
            }

            File.WriteAllLines(GlobalConfig.PrizesFile.GetFullFilePath(), lines);
        }

        public static void SaveToPersonsFile(this List<PersonModel> models)
        {
            List<string> lines = new List<string>();

            foreach (var p in models)
            {
                lines.Add($"{p.Id},{p.FirstName},{p.LastName},{p.EmailAddress},{p.CellphoneNumber}");
            }

            File.WriteAllLines(GlobalConfig.PersonsFile.GetFullFilePath(), lines);
        }

        public static void SaveToTeamsFile(this List<TeamModel> models)
        {
            List<string> lines = new List<string>();

            foreach (TeamModel t in models)
            {
                lines.Add($"{ t.Id },{ t.TeamName },{ t.TeamMembers.ConvertModelListIdsToString() }");
            }

            File.WriteAllLines(GlobalConfig.TeamsFile.GetFullFilePath(), lines);
        }

        public static void SaveToTournamentsFile(this List<TournamentModel> models)                                                                                                                                                    
        {
            List<string> lines = new List<string>();

            foreach (TournamentModel tm in models)
            {
                lines.Add($"{ tm.Id }," +
                          $"{ tm.TournamentName }," +
                          $"{ tm.EntryFee }," +
                          $"{ tm.EnteredTeams.ConvertModelListIdsToString() }," +
                          $"{ tm.Prizes.ConvertModelListIdsToString() }," +
                          $"{ tm.Rounds.ConvertModelListIdsToString() }");
            }

            File.WriteAllLines(GlobalConfig.TournamentsFile.GetFullFilePath(), lines);
        }

        public static void SaveRoundsToFile(this TournamentModel model)
        {
            List<RoundModel> rounds = GlobalConfig.RoundsFile.GetFullFilePath().LoadFile().ConvertToRoundModels();

            List<MatchupModel> matchups = rounds.GetMatchupModelList().OrderBy(m => m.Id).ToList();

            List<MatchupEntryModel> matchupEntries = matchups.GetMatchupEntryModelList().OrderBy(m => m.Id).ToList();

            // Get the maximum Id for the Rounds list and calculate the next id position
            // Get the maximum Id for the Matchups list and calculate the next id position
            // Get the maximum Id for the MatchupEntry list and calculate the next id position

            int currentRoundId = 1;

            if (rounds.Count > 0)
            {
                currentRoundId = rounds.OrderByDescending(x => x.Id).First().Id + 1;
            }

            int currentMatchupId = 1;

            if (matchups.Count > 0)
            {
                currentMatchupId = matchups.OrderByDescending(x => x.Id).First().Id + 1;
            }

            int currentMatchupEntryId = 1;

            if (matchupEntries.Count > 0)
            {
                currentMatchupEntryId = matchupEntries.OrderByDescending(x => x.Id).First().Id + 1;
            }

            // Track and assign id to each new MatchupModel and add to the MatchupModel list
            // For each new MatchupModel, track and assign id to each MatchupEntryModel in the MatchupModel Entries and add to the MatchupEntry list

            foreach (RoundModel round in model.Rounds)
            {
                foreach (MatchupModel matchup in round.Matchups)
                {
                    foreach (MatchupEntryModel matchupEntry in matchup.Entries)
                    {
                        matchupEntry.Id = currentMatchupEntryId++;
                        matchupEntry.TeamCompetingId = matchupEntry.TeamCompeting?.Id;
                        matchupEntry.ParentMatchupId = matchupEntry.ParentMatchup?.Id;
                        matchupEntries.Add(matchupEntry);
                    }

                    matchup.Id = currentMatchupId++;
                    matchup.WinnerId = matchup.Winner?.Id;
                    matchups.Add(matchup);
                }

                round.Id = currentRoundId++;
                rounds.Add(round);
            }

            // Write both lists to their individual files
            rounds.SaveToRoundsFile();
            matchups.SaveToMatchupsFile();
            matchupEntries.SaveToMatchupEntriesFile();
        }

        public static void SaveToRoundsFile(this List<RoundModel> models)
        {
            List<string> lines = new List<string>();

            foreach (RoundModel m in models)
            {
                lines.Add($"{ m.Id },{ m.Number },{ m.Active },{ m.Matchups.ConvertModelListIdsToString() }");
            }

            File.WriteAllLines(GlobalConfig.RoundsFile.GetFullFilePath(), lines);
        }

        public static void SaveToMatchupsFile(this List<MatchupModel> models)
        {
            List<string> lines = new List<string>();

            foreach (MatchupModel m in models)
            {
                lines.Add($"{ m.Id },{ m.Entries.ConvertModelListIdsToString() },{ m.WinnerId },{ m.MatchupRound }");
            }

            File.WriteAllLines(GlobalConfig.MatchupsFile.GetFullFilePath(), lines);
        }

        public static void SaveToMatchupEntriesFile(this List<MatchupEntryModel> models)
        {
            List<string> lines = new List<string>();

            foreach (MatchupEntryModel m in models)
            {
                lines.Add($"{ m.Id },{ m.TeamCompetingId },{ m.Score },{ m.ParentMatchupId }");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntriesFile.GetFullFilePath(), lines);
        }

        public static void UpdateMatchupToFile(this MatchupModel matchup)
        {
            matchup.WinnerId = matchup.Winner?.Id;
            matchup.Entries.ForEach(e => { 
                e.TeamCompetingId = e.TeamCompeting?.Id;
                e.ParentMatchupId = e.ParentMatchup?.Id;
            });

            List<MatchupModel> matchups =
                GlobalConfig.MatchupsFile.GetFullFilePath().LoadFile().ConvertToMatchupModels();

            MatchupModel oldMatchup = matchups.First(m => m.Id == matchup.Id);
            var index = matchups.IndexOf(oldMatchup);

            if (index != -1)
            {
                matchups[index] = matchup;
            }

            List<MatchupEntryModel> matchupEntries = matchups.GetMatchupEntryModelList();

            matchups.SaveToMatchupsFile();
            matchupEntries.SaveToMatchupEntriesFile();
        }

        public static void UpdateRoundToFile(this RoundModel round)
        {
            List<RoundModel> rounds = GlobalConfig.RoundsFile.GetFullFilePath().LoadFile().ConvertToRoundModels();

            RoundModel oldRound = rounds.First(r => r.Id == round.Id);
            var index = rounds.IndexOf(oldRound);

            if (index != -1)
            {
                rounds[index] = round;
            }

            rounds.SaveToRoundsFile();
        }

        public static void UpdateMatchupEntryToFile(this MatchupEntryModel matchupEntry)
        {
            matchupEntry.TeamCompetingId = matchupEntry.TeamCompeting?.Id;
            matchupEntry.ParentMatchupId = matchupEntry.ParentMatchup?.Id;

            List<MatchupEntryModel> matchupEntries = GlobalConfig
                .MatchupEntriesFile
                .GetFullFilePath()
                .LoadFile()
                .ConvertToMatchupEntryModels();

            MatchupEntryModel oldMatchupEntry = matchupEntries.First(me => me.Id == matchupEntry.Id);
            var index = matchupEntries.IndexOf(oldMatchupEntry);

            if (index != -1)
            {
                matchupEntries[index] = matchupEntry;
            }

            matchupEntries.SaveToMatchupEntriesFile();
        }

        public static void RemoveFromTournamentsFile(this TournamentModel tournament)
        {
            List<TournamentModel> tournaments =
                GlobalConfig.TournamentsFile.GetFullFilePath().LoadFile().ConvertToTournamentModels();
            tournaments.Remove(tournaments.Find(t => t.Id == tournament.Id));

            tournaments.SaveToTournamentsFile();
        }

        private static string ConvertModelListIdsToString<T>(this List<T> models)
        {
            string output = "";

            if (models.Count == 0)
            {
                return string.Empty;
            }

            PropertyInfo propInfo = typeof(T).GetProperty("Id");

            models.ForEach(x => output += $"{propInfo?.GetValue(x)}|");

            output = output.Substring(0, output.Length - 1);

            return output;
        }
    }
}
