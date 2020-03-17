public enum NetworkState
{
    NoConnection,
    // Client Specific
    ClientConnectedToServer,
    ClientFailure,
    // Server Specific
    HostingIdle,
    HostingClient,
    HostFailure
}