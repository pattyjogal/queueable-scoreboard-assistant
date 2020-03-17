using System;
using System.ComponentModel;

public class NetworkStateHandler : INotifyPropertyChanged
{

    private NetworkState _networkStatus;
    public NetworkState NetworkStatus
    {
        get { return _networkStatus; }
        set { _networkStatus = value; NotifyPropertyChanged("NetworkStatus"); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(String propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}