# Tournament Tracker
A tournament tracker application designed on C# .NET with Windows Forms.

The tournament tracker application keeps track of players and teams enrolled in a tournament as well as the scores for matchups.
It also includes a prize entry to be awarded to the eventual winners.

### Requirements

1. Track games palyed and their outocme (who won).
2. Multiple competitors play in the tournament
3. Creates a tournament plan (who plays in what order).
4. Schedules games.
5. A single loss elimnates a player.
6. The last player standing is the winner.

### Questions

1. How many players will the tournament handle? Is it variable?
2. If a tournament has less than the full complement of players, how do we handle it?
3. Should the ordering of who plays each other be random or ordered by input order?
4. Should we schedule the game or are they just played whenever?
5. If the games are scheduled, how does the system know when to schedule games for?
6. If the games are played whenever, can a game from the second round be played before the first round is complete?
7. Does the system need to store a score of some kind or just who won?
8. What type of front-end shold this system have (for, webpage, app, etc.)?
9. Where wil the data be stored?
10. Will this system handle entry fees, prizes or other payouts?
11. What type of reporting is needed?
12. Who can fil in the results of a game?
13. Are there varying levels of access?
14. Should this system contact users about upcoming games?
15. Is each player on their own or can teams use this tournament tracker?

### Answers

1. The application should be able to handle a variable number of players in a tournament.
2. A tournament with less than the perfect number ( multiple of 2, so 4, 8, 16, 32, etc.) should add in "byes". 
   Basically, certain people selected at random get to skip the first round and act as if they won.
3. The ordering of the tournament should be random.
4. The games should be played in whatever order and whenever the players want to play them.
5. They are not scheduled so we do not care.
6. No. Each round should be fully completed before the next round is displayed.
7. Storing a simple score would be nice. Just a number for each player. That way, the tracker can be flexble enough to handle a checkers tournament (the winner would have a 1 and the loser a 0) or a basketball tournament.
8. The system shold be a desktop system for now, but down the road we might want to turn it into an app or a website.
9. Ideally, the data should be stored in a Microsoft SQL database but please put in an option to store to a text file instead.
10. Yes. The tournament should have the option of charging an entry fee. Prizes should also be an option, where the tournament administrator chooses how much money to award a variable number of places. 
    The total cash amount should not exceed the income from the tournament. A percentage-based system would also be nice to specify.
11. A simple report specifying the outcome of the games per round as well as a report that specifies who won and how much they won. These can be just displayed on a form or they can be emailed to tournament competitors and the administrator.
12. Anyone using the application should be able to fill in the game scores.
13. No. The only method of varied access is if the competorors are not allowed into the applicatio and instead, they do everything via email.
14. Yes, the system should email users that they are due to play in a round as ell as who they are scheduled to play.
15. The tournament tracker should be able to handle the addition of other memebers. All members shold be treated as equals in that they all get tournament emails. Teams should also be able to name their team.

### The Big Picture Design

Structure: Windows Forms application and Class Library
Data: SQL and/or Text File
Users: One at a time on one application

### Key Concepts
 - Email
 - SQL
 - Custome Events
 - Error handling
 - Interfaces
 - Random ordering
 - Texting
 
### Data Design
 
#### Team
   - TeamMembers (List<Person>)
   - TeamName (string)
#### Person
   - FirstName (string)
   - LastName (string)
   - EmailAddress (string)
   - CellphoneNumber (string)
#### Tournament
   - TournamentName (string)
   - EntryFee (decimal)
   - EnteredTeams (List<Team>)
   - Prizes (List<Prize>)
   - Rounds (List<List<Matchup>>)
#### Prize
   - PlaceNumber (int)
   - PlaceName (string)
   - PrizeAmount (decimal)
   - PrizePercentage (double)
#### Matchup
   - Entries (List<MatchupEntry>)
   - Winner (Team)
   - MatchupRound (int)
#### MatchupEntry
   - TeamCompeting (Team)
   - Score (double)
   - ParentMatchup (Matchup)
