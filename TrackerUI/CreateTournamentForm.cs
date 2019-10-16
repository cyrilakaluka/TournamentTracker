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
    public partial class CreateTournamentForm : Form
    {
        private List<TeamModel> availableTeams = GlobalConfig.Connection.GetTeam_All();
        private List<TeamModel> selectedTeams = new List<TeamModel>();
        private List<PrizeModel> selectedPrizes = new List<PrizeModel>();

        public CreateTournamentForm()
        {
            InitializeComponent();
            WireUpLists();
        }

        private void WireUpLists()
        {
            selectTeamDropDown.DataSource = null;
            selectTeamDropDown.DataSource = availableTeams;
            selectTeamDropDown.DisplayMember = "TeamName";

            tournamentTeamsListBox.DataSource = null;
            tournamentTeamsListBox.DataSource = selectedTeams;
            tournamentTeamsListBox.DisplayMember = "TeamName";

            prizesListBox.DataSource = null;
            prizesListBox.DataSource = selectedPrizes;
            prizesListBox.DisplayMember = "PlaceName";
        }

        private void addTeamButton_Click(object sender, EventArgs e)
        {
            TeamModel t = (TeamModel) selectTeamDropDown.SelectedItem;

            if (t != null)
            {
                availableTeams.Remove(t);
                selectedTeams.Add(t);

                WireUpLists();
            }
        }

        private void createPrizeButton_Click(object sender, EventArgs e)
        {
            // Call the create prize form
            var form = new CreatePrizeForm();
            form.PrizeCreated += OnPrizeCreated;
            form.Show();
        }

        private void OnPrizeCreated(PrizeModel model)
        {
            // return a PrizeModel from the form
            // Take  the PrizeModel and put it into out list of selected prizes
            selectedPrizes.Add(model);
            WireUpLists();
        }

        private void createNewTeamLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var form = new CreateTeamForm();
            form.TeamCreated += OnTeamCreated;
            form.Show();
        }

        private void OnTeamCreated(TeamModel model)
        {
            selectedTeams.Add(model);
            WireUpLists();
        }

        private void removeSelectedTeamButton_Click(object sender, EventArgs e)
        {
            var team = (TeamModel) tournamentTeamsListBox.SelectedItem;

            if (team != null)
            {
                selectedTeams.Remove(team);
                availableTeams.Add(team);

                WireUpLists();
            }
        }

        private void removeSelectedPrizeButton_Click(object sender, EventArgs e)
        {
            var prize = (PrizeModel) prizesListBox.SelectedItem;

            if (prize != null)
            {
                selectedPrizes.Remove(prize);

                WireUpLists();
            }
        }

        private void createTournamentButton_Click(object sender, EventArgs e)
        {
            // Create out tournament model
            if (ValidateForm())
            {
                TournamentModel model = new TournamentModel
                {
                    TournamentName = tournamentNameValue.Text,
                    EntryFee = decimal.Parse(entryFeeValue.Text),
                    Prizes = selectedPrizes,
                    EnteredTeams = selectedTeams
                };
                
                TournamentLogic.CreateRounds(model);

                GlobalConfig.Connection.CreateTournament(model);

                TournamentViewerForm form = new TournamentViewerForm(model);
                form.Show();
                this.Close();
            }
        }

        private bool ValidateForm()
        {
            bool output = true;

            bool feeAcceptable = decimal.TryParse(entryFeeValue.Text, out _);

            if (!feeAcceptable)
            {
                MessageBox.Show("You need to enter a valid Entry Fee.", 
                    "Invalid Fee", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                output = false;
            }

            return output;
        }
    }
}
