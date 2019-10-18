using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class TournamentViewerForm : Form
    {
        private TournamentModel tournament;
        private List<MatchupModel> selectedMatchups;
        public TournamentViewerForm(TournamentModel tournament)
        {
            InitializeComponent();

            if (tournament == null)
            {
               // TODO - Handle cases when tournament is null
            }

            this.tournament = tournament;

            tournament.TournamentCompleted += OnTournamentCompleted;

            selectedMatchups = tournament.Rounds.FirstOrDefault()?.Matchups;

            InitializeFormData();
        }

        private void InitializeFormData()
        {
            tournamentName.Text = tournament.TournamentName;

            WireUpRoundsLists();

            ShowFormElements();

            LoadMatchupDetails();
        }

        private void WireUpRoundsLists()
        {
            roundDropDown.DataSource = null;
            roundDropDown.DataSource = tournament.Rounds;
            roundDropDown.DisplayMember = "Number";
        }

        private void WireUpMatchupsLists()
        {
            matchupListBox.DataSource = null;
            matchupListBox.DataSource = selectedMatchups;
            matchupListBox.DisplayMember = "DisplayName";
        }

        private void roundDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRoundMatchups();

            ShowFormElements();

            LoadMatchupDetails();
        }

        private void LoadRoundMatchups()
        {
            if (roundDropDown.SelectedItem == null)
                return;

            RoundModel round = (RoundModel)roundDropDown.SelectedItem;

            selectedMatchups = round.Matchups.Where(m => m.Winner == null || !unplayedOnlyCheckbox.Checked).ToList();

            WireUpMatchupsLists();

            ShowMatchupsInfo();
        }

        private void ShowMatchupsInfo()
        {
            bool isVisible = selectedMatchups.Count > 0;

            teamOneName.Visible = isVisible;
            teamOneScoreValue.Visible = isVisible;
            teamOneScoreLabel.Visible = isVisible;
            teamTwoName.Visible = isVisible;
            teamTwoScoreValue.Visible = isVisible;
            teamTwoScoreLabel.Visible = isVisible;
            versusLabel.Visible = isVisible;
            scoreButton.Visible = isVisible;
        }

        private void matchupListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMatchupDetails();
        }

        private void LoadMatchupDetails()
        {
            if (matchupListBox.SelectedItem == null || roundDropDown.SelectedItem == null)
                return;

            RoundModel round = (RoundModel) roundDropDown.SelectedItem;

            MatchupModel m = (MatchupModel) matchupListBox.SelectedItem;

            if (round.Active.HasValue)
            {
                teamOneName.Text = m.Entries[0].TeamCompeting.TeamName;
                teamOneScoreValue.Text = m.Entries[0].Score.GetValueOrDefault().ToString();

                teamTwoName.Text = "<bye>";
                teamTwoScoreValue.Text = "0";

                if (round.Active.Value == true)
                {
                    teamOneScoreValue.Enabled = false;
                    teamTwoScoreValue.Enabled = false;
                    teamOneScoreValue.BackColor = Color.LightGray;
                    teamTwoScoreValue.BackColor = Color.LightGray;
                    scoreButton.Enabled = false;
                    scoreButton.BackColor = Color.LightGray; 
                }

                if (m.Entries.Count > 1)
                {
                    teamTwoName.Text = m.Entries[1].TeamCompeting.TeamName;
                    teamTwoScoreValue.Text = m.Entries[1].Score.GetValueOrDefault().ToString();

                    if (round.Active.Value == true) 
                    {
                        teamOneScoreValue.Enabled = true;
                        teamTwoScoreValue.Enabled = true;
                        teamOneScoreValue.BackColor = Color.White;
                        teamTwoScoreValue.BackColor = Color.White;
                        scoreButton.Enabled = true;
                        scoreButton.BackColor = Color.White; 
                    }
                }
            }
        }

        private void unplayedOnlyCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            LoadRoundMatchups();
        }

        private void scoreButton_Click(object sender, EventArgs e)
        {
            MatchupModel matchup = (MatchupModel) matchupListBox.SelectedItem;

            string errorMessage = IsValidScoreData(matchup);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show($"Input Error: {errorMessage}");
                return;
            }

            try
            {
                matchup.UpdateMatchupResults(double.Parse(teamOneScoreValue.Text), double.Parse(teamTwoScoreValue.Text));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application error: {ex.Message}");
                return;
            }

            LoadMatchupDetails();
        }

        private string IsValidScoreData(MatchupModel matchup)
        {
            string output = string.Empty;

            bool scoreTeamOneValid = double.TryParse(teamOneScoreValue.Text, out double scoreTeamOne);
            bool scoreTeamTwoValid = double.TryParse(teamTwoScoreValue.Text, out double scoreTeamTwo);

            if (!scoreTeamOneValid)
            {
                output = $"Please enter a valid score for { matchup.Entries[0].TeamCompeting.TeamName }";
            }
            else if (!scoreTeamTwoValid)
            {
                output = $"Please enter a valid score for { matchup.Entries[1].TeamCompeting.TeamName }";
            }
            else if (scoreTeamOne == 0 && scoreTeamTwo == 0)
            {
                output = "You did not enter a score for either team";
            }
            else if (scoreTeamOne == scoreTeamTwo)
            {
                output = "Tie scores are not allowed";
            }

            return output;
        }

        private void concludeButton_Click(object sender, EventArgs e)
        {
            RoundModel round = (RoundModel) roundDropDown.SelectedItem;

            if (round == null) return;

            try
            {
                tournament.UpdateTournamentRound(round);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application error: {ex.Message}");
                return;
            }

            RefreshForm();
        }

        private void ShowFormElements()
        {
            RoundModel round = (RoundModel) roundDropDown.SelectedItem;

            if(round == null) return;

            if (round.Active.HasValue)
            {
                if (!round.Active.Value)
                {
                    concludeButton.Enabled = false;
                    concludeButton.BackColor = Color.LightGray;
                    concludeButton.Text = "Completed";
                    scoreButton.Enabled = false;
                    scoreButton.BackColor = Color.LightGray;
                    scoreButton.Visible = true;
                    teamOneName.Visible = true;
                    teamOneScoreLabel.Visible = true;
                    teamOneScoreValue.Enabled = false;
                    teamOneScoreValue.BackColor = Color.LightGray;
                    teamTwoName.Visible = true;
                    teamTwoScoreLabel.Visible = true;
                    teamTwoScoreValue.Enabled = false;
                    teamTwoScoreValue.BackColor = Color.LightGray;
                }
                else
                {
                    concludeButton.Enabled = true;
                    concludeButton.BackColor = Color.White;
                    concludeButton.Text = "End Round";
                    scoreButton.Enabled = true;
                    scoreButton.BackColor = Color.White;
                    scoreButton.Visible = true;
                    teamOneName.Visible = true;
                    teamOneScoreLabel.Visible = true;
                    teamOneScoreValue.Enabled = true;
                    teamOneScoreValue.BackColor = Color.White;
                    teamTwoName.Visible = true;
                    teamTwoScoreLabel.Visible = true;
                    teamTwoScoreValue.Enabled = true;
                    teamTwoScoreValue.BackColor = Color.White;

                    if (tournament.Rounds.Count == round.Number) // final round
                    {
                        concludeButton.Text = "End Tournament";
                    }
                }
            }
            else
            {
                concludeButton.Enabled = false;
                concludeButton.BackColor = Color.LightGray;
                concludeButton.Text = "Not Yet Active";
                teamOneName.Visible = false;
                teamOneScoreValue.Visible = false;
                teamOneScoreLabel.Visible = false;
                teamTwoName.Visible = false;
                teamTwoScoreValue.Visible = false;
                teamTwoScoreLabel.Visible = false;
                versusLabel.Visible = false;
                scoreButton.Visible = false;
            }
        }

        private void RefreshForm()
        {
            LoadRoundMatchups();

            ShowFormElements();

            LoadMatchupDetails();
        }

        private void OnTournamentCompleted(object source, DateTime dateTime)
        {
            MessageBox.Show("This tournament has been completed");
            this.Close();
        }
    }
}
