using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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

                foreach (var id in personIds)
                {
                    t.TeamMembers.Add(persons.First(p => p.Id == int.Parse(id)));
                }

                output.Add(t);
            }

            return output;
        }

        public static List<TournamentModel> ConvertToTournamentModels(this List<string> lines)
        {
            // Format: TournamentId,TournamentName,EntryFee,(EnteredTeams = id|id|id|...),(Prizes = id|id|id|...),(Rounds - id^id^id^...|id^id^id...|id^id^id...|...)
            List<TournamentModel> output = new List<TournamentModel>();
            List<TeamModel> teams = GlobalConfig.TeamsFile.GetFullFilePath().LoadFile().ConvertToTeamModels();
            List<PrizeModel> prizes = GlobalConfig.PrizesFile.GetFullFilePath().LoadFile().ConvertToPrizeModels();
            List<MatchupModel> matchups = GlobalConfig.MatchupsFile.GetFullFilePath().LoadFile().ConvertToMatchupModels();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TournamentModel tournament = new TournamentModel
                {
                    Id = int.Parse(cols[0]),
                    TournamentName = cols[1],
                    EntryFee = decimal.Parse(cols[2]),
                };

                string[] teamsId = cols[3].Split('|');

                foreach (var id in teamsId)
                {
                    tournament.EnteredTeams.Add(teams.First(t => t.Id == int.Parse(id)));
                }

                string[] prizesId = cols[4].Split(new []{'|'}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var id in prizesId)
                {
                    tournament.Prizes.Add(prizes.First(p => p.Id == int.Parse(id)));
                }

                string[] rounds = cols[5].Split('|');

                foreach (string round in rounds)
                {
                    string[] tournamentMatchups = round.Split('^');
                    List<MatchupModel> tournamentMatchupList = new List<MatchupModel>();

                    foreach (string tournamentMatchup in tournamentMatchups)
                    {
                        tournamentMatchupList.Add(matchups.First(m => m.Id == int.Parse(tournamentMatchup)));
                    }

                    tournament.Rounds.Add(tournamentMatchupList);
                }

                output.Add(tournament);
            }

            return output;
        }

        public static List<MatchupModel> ConvertToMatchupModels(this List<string> lines)
        {
            List<MatchupModel> output = new List<MatchupModel>();
            List<TeamModel> teams = GlobalConfig.TeamsFile.GetFullFilePath().LoadFile()
                .ConvertToTeamModels();
            List<string> matchupEntryLines = GlobalConfig.MatchupEntriesFile.GetFullFilePath().LoadFile();
            List<MatchupEntryModel> matchupEntries = ConvertToMatchupEntryModels();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                MatchupModel matchup = new MatchupModel();
                matchup.Id = int.Parse(cols[0]);

                string[] entries = cols[1].Split('|');

                foreach (string entry in entries)
                {
                    matchup.Entries.Add(matchupEntries.First(m => m.Id == int.Parse(entry)));
                }

                matchup.Winner = string.IsNullOrEmpty(cols[2]) ? null : teams.First(t => t.Id == int.Parse(cols[2]));
                matchup.MatchupRound = int.Parse(cols[3]);

                output.Add(matchup);
            }

            AssignParentIdToEntries();

            return output;

            #region Local functions
            List<MatchupEntryModel> ConvertToMatchupEntryModels()
            {
                List<MatchupEntryModel> matchupEntriesOuptut = new List<MatchupEntryModel>();

                foreach (string line in matchupEntryLines)
                {
                    string[] cols = line.Split(',');

                    MatchupEntryModel entry = new MatchupEntryModel();
                    entry.Id = int.Parse(cols[0]);
                    entry.TeamCompeting =
                        string.IsNullOrEmpty(cols[1]) ? null : teams.First(t => t.Id == int.Parse(cols[1]));
                    entry.Score = string.IsNullOrEmpty(cols[2]) ? null : double.Parse(cols[2]) as double?;

                    matchupEntriesOuptut.Add(entry);
                }

                return matchupEntriesOuptut;
            }

            void AssignParentIdToEntries()
            {
                List<string> parentIdCol = new List<string>();

                foreach (string line in matchupEntryLines)
                {
                    parentIdCol.Add(line.Split(',')[3]);
                }

                foreach (var entry in matchupEntries)
                {
                    string parentId = parentIdCol[matchupEntries.IndexOf(entry)];
                    entry.ParentMatchup = string.IsNullOrEmpty(parentId) ? null : output.First(m => m.Id == int.Parse(parentId));
                }
            } 
            #endregion
        }

        public static List<MatchupEntryModel> GetMatchupEntryModelList(this List<MatchupModel> matchups)
        {
            List<MatchupEntryModel> output = new List<MatchupEntryModel>();

            foreach (MatchupModel matchup in matchups)
            {
                foreach (MatchupEntryModel entry in matchup.Entries)
                {
                    output.Add(entry);
                }
            }

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
                lines.Add($"{ t.Id },{ t.TeamName },{ ConvertPersonListToString(t.TeamMembers) }");
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
                          $"{ ConvertTeamListToString(tm.EnteredTeams) }," +
                          $"{ ConvertPrizeListToString(tm.Prizes) }," +
                          $"{ ConvertRoundListToString(tm.Rounds) }");
            }

            File.WriteAllLines(GlobalConfig.TournamentsFile.GetFullFilePath(), lines);

            string ConvertTeamListToString(List<TeamModel> teams)
            {
                string output = "";

                if (teams.Count == 0)
                {
                    return string.Empty;
                }

                foreach (var t in teams)
                {
                    output += $"{t.Id}|";
                }

                output = output.Substring(0, output.Length - 1);

                return output;
            }

            string ConvertPrizeListToString(List<PrizeModel> prizes)
            {
                string output = "";

                if (prizes.Count == 0)
                {
                    return string.Empty;
                }

                foreach (var p in prizes)
                {
                    output += $"{p.Id}|";
                }

                output = output.Substring(0, output.Length - 1);

                return output;
            }

            string ConvertRoundListToString(List<List<MatchupModel>> rounds)
            {
                string output = "";

                if (rounds.Count == 0)
                {
                    return string.Empty;
                }

                foreach (var round in rounds)
                {
                    output += $"{ ConvertMatchupListToString(round) }|";
                }

                output = output.Substring(0, output.Length - 1);

                return output;
            }

            string ConvertMatchupListToString(List<MatchupModel> matchups)
            {
                string output = "";

                if (matchups.Count == 0)
                {
                    return string.Empty;
                }

                foreach (var m in matchups)
                {
                    output += $"{m.Id}^";
                }

                output = output.Substring(0, output.Length - 1);

                return output;
            }
        }

        public static void SaveRoundsToFile(this TournamentModel model)
        {

            // Load all Matchups from file and store on a MatchupModel list
            // Get all MatchupEntries from the matchups and store on a MatchupEntryModel list

            List<MatchupModel> matchups = GlobalConfig.MatchupsFile
                .GetFullFilePath()
                .LoadFile()
                .ConvertToMatchupModels();
            List<MatchupEntryModel> matchupEntries = matchups.GetMatchupEntryModelList().OrderBy(m => m.Id).ToList();

            // Get the maximum Id for the Matchups list and calculate the next id position
            // Get the maximum Id for the MatchupEntry list and calculate the next id position

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

            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel matchup in round)
                {
                    foreach (MatchupEntryModel matchupEntry in matchup.Entries)
                    {
                        matchupEntry.Id = currentMatchupEntryId++;
                        matchupEntries.Add(matchupEntry);
                    }

                    matchup.Id = currentMatchupId++;
                    matchups.Add(matchup);
                }
            }

            // Write both lists to their individual files
            matchups.SaveToMatchupsFile();
            matchupEntries.SaveToMatchupEntriesFile();
        }

        public static void SaveToMatchupsFile(this List<MatchupModel> models)
        {
            List<string> lines = new List<string>();

            foreach (MatchupModel m in models)
            {
                lines.Add($"{ m.Id },{ ConvertMatchupEntryListToString(m.Entries) },{ m.Winner?.Id },{ m.MatchupRound }");
            }

            File.WriteAllLines(GlobalConfig.MatchupsFile.GetFullFilePath(), lines);
        }

        public static void SaveToMatchupEntriesFile(this List<MatchupEntryModel> models)
        {
            List<string> lines = new List<string>();

            foreach (MatchupEntryModel m in models)
            {
                lines.Add($"{ m.Id },{ m.TeamCompeting?.Id },{ m.Score },{ m.ParentMatchup?.Id }");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntriesFile.GetFullFilePath(), lines);
        }

        public static void UpdateMatchupToFile(this MatchupModel newMatchup)
        {
            List<MatchupModel> matchups =
                GlobalConfig.MatchupsFile.GetFullFilePath().LoadFile().ConvertToMatchupModels();

            MatchupModel oldMatchup = matchups.First(m => m.Id == newMatchup.Id);
            var index = matchups.IndexOf(oldMatchup);

            if (index != -1)
            {
                matchups[index] = newMatchup;
            }

            List<MatchupEntryModel> matchupEntries = matchups.GetMatchupEntryModelList();

            matchups.SaveToMatchupsFile();
            matchupEntries.SaveToMatchupEntriesFile();
        }

        private static object ConvertMatchupEntryListToString(List<MatchupEntryModel> entries)
        {
            string output = "";

            if (entries.Count == 0)
            {
                return string.Empty;
            }

            foreach (var e in entries)
            {
                output += $"{e.Id}|";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertPersonListToString(List<PersonModel> persons)
        {
            string output = "";

            if (persons.Count == 0)
            {
                return string.Empty;
            }

            foreach (var p in persons)
            {
                output += $"{p.Id}|";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }
    }
}
