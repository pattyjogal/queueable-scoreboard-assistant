using System;
using System.ComponentModel;

namespace queueable_scoreboard_assistant.Common
{
    public class ScoreboardStateHandler : INotifyPropertyChanged
    {
        public ScoreboardStateHandler()
        {
            ScoreboardState = new ScoreboardState
            {
                LeftScore = 0,
                RightScore = 0,
                LeftPlayerName = "",
                RightPlayerName = "",
            };
        }

        private ScoreboardState _scoreboardState;
        public ScoreboardState ScoreboardState
        {
            get { return _scoreboardState; }
            set { _scoreboardState = value; NotifyPropertyChanged("ScoreboardState"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
