namespace Quicksilver {
    // The first byte of every packet will contain an "opcode" specifying what the packet is meant to be.
    // This may be a simple plaintext message, a file, or system messages.
    public enum OpCode {
        Close, // Sent to client/server informing them to close the socket.
        InitUserList, // Sent to clients when connecting, initializing the list of users in the chatroom.
        NoticeDisconnect, // Sent from server to client when another client disconnects from the server.
        NoticeConnect, // Sent from server to client when another client connects to the server.
        MessageBegin, // Sent to client/server when a text message begins sending.
        FileBegin, // Sent to client/server when a file begins sending.
    }
}