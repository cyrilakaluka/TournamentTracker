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
        List<int> rounds = new List<int>();
        List<MatchupModel> selectedMatchups = new List<MatchupModel>();
        public TournamentViewerForm(TournamentModel tournament)
        {
            InitializeComponent();

            this.tournament = tournament;

            InitializeFormData();
        }

        private void InitializeFormData()
        {
            tournamentName.Text = tournament.TournamentName;

            for (int i = 1; i <= tournament.Rounds.Count; i++)
            {
                rounds.Add(i);
            }

            WireUpRoundsLists();
        }

        private void WireUpRoundsLists()
        {
            roundDropDown.DataSource = null;
            roundDropDown.DataSource = rounds;
        }

        private void WireUpMatchupsLists()
        {
            matchupListBox.DataSource = null;
            matchupListBox.DataSource = selectedMatchups;
            matchupListBox.DisplayMember = "DisplayName";
        }

        private void roundDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadAllMatchups();
        }

        private void LoadAllMatchups()
        {
            int round = (int) roundDropDown.SelectedItem;

            selectedMatchups = tournament.Rounds[round - 1]
                .Where(m => m.Winner == null || !unplayedOnlyCheckbox.Checked)
                .ToList();

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
            LoadSingleMatchup();
        }

        private void LoadSingleMatchup()
        {
            if (matchupListBox.SelectedItem == null)
                return;

            MatchupModel m = (MatchupModel) matchupListBox.SelectedItem;

            scoreButton.Enabled = false;

            if (m.Entries[0].TeamCompeting != null)
            {
                teamOneName.Text = m.Entries[0].TeamCompeting.TeamName;
                teamOneScoreValue.Text = m.Entries[0].Score.GetValueOrDefault().ToString();
                teamOneScoreValue.Enabled = false;

                teamTwoName.Text = "<bye>";
                teamTwoScoreValue.Text = "0";
                teamTwoScoreValue.Enabled = false;
            }
            else
            {
                teamOneName.Text = "Not Yet Set";
                teamOneScoreValue.Text = "";
                teamOneScoreValue.Enabled = false;
            }

            if (m.Entries.Count > 1)
            {
                if (m.Entries[1].TeamCompeting != null)
                {
                    teamTwoName.Text = m.Entries[1].TeamCompeting.TeamName;
                    teamTwoScoreValue.Text = m.Entries[1].Score.GetValueOrDefault().ToString();
                    teamTwoScoreValue.Enabled = true;

                    teamOneScoreValue.Enabled = true;
                    scoreButton.Enabled = true;
                }
                else
                {
                    teamTwoName.Text = "Not Yet Set";
                    teamTwoScoreValue.Text = "";
                    teamTwoScoreValue.Enabled = false;
                }
            }
        }

        private void unplayedOnlyCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            LoadAllMatchups();
        }

        private void scoreButton_Click(object sender, EventArgs e)
        {
            MatchupModel matchup = (MatchupModel) matchupListBox.SelectedItem;

            string errorMessage = IsValidScoreData(matchup);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show($"Input Error: {errorMessage}");
            }

            try
            {
                tournament.UpdateTournamentResults(matchup, (int)roundDropDown.SelectedItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application error: {ex.Message}");
                return;
            }

            LoadAllMatchups();
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
    }
}
