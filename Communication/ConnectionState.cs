namespace Jamb.Communication
{
	/// <summary>
	/// State of the connection with another instance of application on remote computer.
	/// 
	/// Possible transitions
	/// Open -> Lost (we lost a connection)
	/// Lost -> Open (connection is reestablished), we will continue sending queued messages
	/// Open -> Closed (connection is terminated without sending the remaining messages)
	/// Open -> Closing (connection will be closed after all messages are sent or enough time passes (as set))
	/// Closing -> Closed (connection is closed by either all messages being sent or no more time for closing)
	/// Lost -> Closed (connection is closed immediatelly and no messages can be sent or received)
	/// </summary>
	public enum ConnectionState
	{
		/// <summary>
		/// Conenction is present and can be used to send and receive messages.
		/// </summary>
		Open,
		/// <summary>
		/// Connection is in state of closing. We will send remaining messages and close the connection.
		/// No more messages can be received.
		/// </summary>
		Closing,
		/// <summary>
		/// Connection is closed. We can't send nor receive messages.
		/// </summary>
		Closed,
		/// <summary>
		/// Connection is lost. Messages for sending will be queued and no messages can be received until connection is reestablished.
		/// </summary>
		Lost
	}
}
