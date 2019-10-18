using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess
{
    public class SqlConnector : IDataConnection
    {
        private const string ConnectionDatabase = "Tournaments";

        /// <summary>
        /// Saves a new prize to the database
        /// </summary>
        /// <param name="model">The prize information</param>
        /// <returns>The prize information, including the unique identifier</returns>
        public void CreatePrize(PrizeModel model)
        {
            using (IDbConnection dbConnection = new System.Data.SqlClient.SqlConnection(GlobalConfig.GetConnectionString(ConnectionDatabase)))
            {
                var p = new DynamicParameters();
                p.Add("@PlaceNumber", model.PlaceNumber);
                p.Add("@PlaceName", model.PlaceName);
                p.Add("@PrizeAmount", model.PrizeAmount);
                p.Add("@PrizePercentage", model.PrizePercentage);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                dbConnection.Execute("dbo.spPrizes_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");
            }
        }

        public void CreatePerson(PersonModel model)
        {
            using (IDbConnection dbConnection = new System.Data.SqlClient.SqlConnection(GlobalConfig.GetConnectionString(ConnectionDatabase)))
            {
                var p = new DynamicParameters();
                p.Add("@FirstName", model.FirstName);
                p.Add("@LastName", model.LastName);
                p.Add("@EmailAddress", model.EmailAddress);
                p.Add("@PhoneNumber", model.CellphoneNumber);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                dbConnection.Execute("dbo.spPeople_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");
            }
        }

        public void CreateTeam(TeamModel model)
        {
            using (IDbConnection dbConnection = new System.Data.SqlClient.SqlConnection(GlobalConfig.GetConnectionString(ConnectionDatabase)))
            {
                var p = new DynamicParameters();
                p.Add("@TeamName", model.TeamName);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                dbConnection.Execute("dbo.spTeams_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");

                foreach (PersonModel member in model.TeamMembers)
                {
                    p = new DynamicParameters();
                    p.Add("@TeamId", model.Id);
                    p.Add("@PersonId", member.Id);
                    p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                    dbConnection.Execute("dbo.spTeamMembers_Insert", p, commandType: CommandType.StoredProcedure);
                }
            }
        }

        public void CreateTournament(TournamentModel model)
        {
            using (IDbConnection dbConnection = new System.Data.SqlClient.SqlConnection(GlobalConfig.GetConnectionString(ConnectionDatabase)))
            {
                SaveTournament(dbConnection);

                SaveTournamentPrizes(dbConnection);

                SaveTournamentEntries(dbConnection);

                SaveTournamentRounds(dbConnection);
            }

            void SaveTournament(IDbConnection dbConnection)
            {
                var p = new DynamicParameters();
                p.Add("@TournamentName", model.TournamentName);
                p.Add("@EntryFee", model.EntryFee);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                dbConnection.Execute("dbo.spTournaments_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");
            }

            void SaveTournamentPrizes(IDbConnection dbConnection)
            {
                foreach (PrizeModel prize in model.Prizes)
                {
                    var p = new DynamicParameters();
                    p.Add("@TournamentId", model.Id);
                    p.Add("@PrizeId", prize.Id);
                    p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                    dbConnection.Execute("dbo.spTournamentPrizes_Insert", p, commandType: CommandType.StoredProcedure);
                }
            }

            void SaveTournamentEntries(IDbConnection dbConnection)
            {
                foreach (TeamModel team in model.EnteredTeams)
                {
                    var p = new DynamicParameters();
                    p.Add("@TournamentId", model.Id);
                    p.Add("@TeamId", team.Id);
                    p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                    dbConnection.Execute("dbo.spTournamentEntries_Insert", p, commandType: CommandType.StoredProcedure);
                }
            }

            void SaveTournamentRounds(IDbConnection dbConnection)
            {
                foreach (RoundModel round in model.Rounds)
                {
                    var p = new DynamicParameters();
                    p.Add("@TournamentId", model.Id);
                    p.Add("@Number", round.Number);
                    p.Add("@Active", round.Active);
                    p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                    dbConnection.Execute("dbo.spRounds_Insert", p, commandType: CommandType.StoredProcedure);

                    round.Id = p.Get<int>("@id");

                    foreach (MatchupModel matchup in round.Matchups)
                    {
                        p = new DynamicParameters();
                        p.Add("@RoundId", round.Id);
                        p.Add("@WinnerId", matchup.Winner?.Id);
                        p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                        dbConnection.Execute("dbo.spMatchups_Insert", p, commandType: CommandType.StoredProcedure);

                        matchup.Id = p.Get<int>("@id");

                        foreach (MatchupEntryModel matchupEntry in matchup.Entries)
                        {
                            p = new DynamicParameters();
                            p.Add("@MatchupId", matchup.Id);
                            p.Add("@ParentMatchupId", matchupEntry.ParentMatchup?.Id);
                            p.Add("@TeamCompetingId", matchupEntry.TeamCompeting?.Id);
                            p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                            dbConnection.Execute("dbo.spMatchupEntries_Insert", p, commandType: CommandType.StoredProcedure);
                        }
                    }
                }
            }
        }

        public void UpdateMatchup(MatchupModel model)
        {
            using (IDbConnection dbConnection = new System.Data.SqlClient.SqlConnection(GlobalConfig.GetConnectionString(ConnectionDatabase)))
            {
                var p = new DynamicParameters();
                p.Add("@id", model.Id);
                p.Add("@WinnerId", model.Winner?.Id);

                dbConnection.Execute("dbo.spMatchups_Update", p, commandType: CommandType.StoredProcedure);
            }

            model.Entries.ForEach(UpdateMatchupEntry);
        }

        public void UpdateMatchupEntry(MatchupEntryModel model)
        {
            using (IDbConnection dbConnection = new System.Data.SqlClient.SqlConnection(GlobalConfig.GetConnectionString(ConnectionDatabase)))
            {
                if (model.TeamCompeting != null)
                {
                    var p = new DynamicParameters();
                    p.Add("@id", model.Id);
                    p.Add("@TeamCompetingId", model.TeamCompeting.Id);
                    if (model.Score.HasValue)
                    {
                        p.Add("@Score", model.Score);
                    }

                    dbConnection.Execute("dbo.spMatchupEntries_Update", p, commandType: CommandType.StoredProcedure); 
                }
            }
        }

        public void DeactivateTournament(TournamentModel model)
        {
            using (IDbConnection dbConnection = new System.Data.SqlClient.SqlConnection(GlobalConfig.GetConnectionString(ConnectionDatabase)))
            {
                var p = new DynamicParameters();
                p.Add("@id", model.Id);

                dbConnection.Execute("dbo.spTournament_Deactivate", p, commandType: CommandType.StoredProcedure);
            }
        }

        public void UpdateRound(RoundModel model)
        {
            using (IDbConnection dbConnection = new System.Data.SqlClient.SqlConnection(GlobalConfig.GetConnectionString(ConnectionDatabase)))
            {
                var p = new DynamicParameters();
                p.Add("@id", model.Id);
                p.Add("@Active", model.Active ?? null);

                dbConnection.Execute("dbo.spRounds_Update", p, commandType: CommandType.StoredProcedure);
            }
        }

        public List<PersonModel> GetPerson_All()
        {
            List<PersonModel> output;

            using (IDbConnection dbConnection = new System.Data.SqlClient.SqlConnection(GlobalConfig.GetConnectionString(ConnectionDatabase)))
            {
                output = dbConnection.Query<PersonModel>("dbo.spPeople_GetAll").ToList();
            }

            return output;
        }

        public List<TournamentModel> GetTournament_All()
        {
            List<TournamentModel> output;

            using (IDbConnection dbConnection = new System.Data.SqlClient.SqlConnection(GlobalConfig.GetConnectionString(ConnectionDatabase)))
            {
                output = dbConnection.Query<TournamentModel>("dbo.spTournaments_GetAll").ToList();

                foreach (TournamentModel tournament in output)
                {
                    // Populate the prizes
                    var p = new DynamicParameters();
                    p.Add("@TournamentId", tournament.Id);

                    tournament.Prizes = dbConnection.Query<PrizeModel>("dbo.spPrizes_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();

                    // Populate the teams
                    tournament.EnteredTeams = dbConnection.Query<TeamModel>("dbo.spTeam_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();

                    // Populate the rounds
                    tournament.Rounds = dbConnection.Query<RoundModel>("dbo.spRounds_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();

                    foreach (TeamModel team in tournament.EnteredTeams)
                    {
                        p = new DynamicParameters();
                        p.Add("@TeamId", team.Id);

                        team.TeamMembers = dbConnection.Query<PersonModel>("dbo.spTeamMembers_GetByTeam", p, commandType: CommandType.StoredProcedure).ToList();
                    }

                    RoundModel previousRound = tournament.Rounds.FirstOrDefault();

                    foreach (RoundModel currentRound in tournament.Rounds)
                    {
                        p = new DynamicParameters();
                        p.Add("@RoundId", currentRound.Id);

                        currentRound.Matchups = dbConnection.Query<MatchupModel>("dbo.spMatchups_GetByRound", p, commandType: CommandType.StoredProcedure).ToList();

                        foreach (MatchupModel matchup in currentRound.Matchups)
                        {
                            matchup.MatchupRound = currentRound.Number;

                            if (matchup.WinnerId.HasValue)
                            {
                                matchup.Winner = tournament.EnteredTeams.Find(t => t.Id == matchup.WinnerId);
                            }

                            p = new DynamicParameters();
                            p.Add("@MatchupId", matchup.Id);

                            matchup.Entries = dbConnection.Query<MatchupEntryModel>("dbo.spMatchupEntries_GetByMatchup", p, commandType: CommandType.StoredProcedure).ToList();

                            foreach (var entry in matchup.Entries)
                            {
                                if (entry.TeamCompetingId.HasValue)
                                {
                                    entry.TeamCompeting = tournament.EnteredTeams.First(t => t.Id == entry.TeamCompetingId);
                                }

                                if (entry.ParentMatchupId.HasValue)
                                {
                                    entry.ParentMatchup = previousRound?.Matchups.First(m => m.Id == entry.ParentMatchupId);
                                }
                            }
                        }
                        previousRound = currentRound;
                    }
                }
            }

            return output;
        }

        public List<TeamModel> GetTeam_All()
        {
            List<TeamModel> output;
            using (IDbConnection dbConnection = new System.Data.SqlClient.SqlConnection(GlobalConfig.GetConnectionString(ConnectionDatabase)))
            {
                output = dbConnection.Query<TeamModel>("dbo.spTeams_GetAll").ToList();

                foreach (TeamModel team in output)
                {
                    var p = new DynamicParameters();
                    p.Add("@TeamId", team.Id);

                    team.TeamMembers = dbConnection.Query<PersonModel>("dbo.spTeamMembers_GetByTeam", p, commandType: CommandType.StoredProcedure).ToList();
                }
            }

            return output;
        }
    }
}
