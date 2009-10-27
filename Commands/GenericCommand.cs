﻿namespace helpmebot6.Commands
{
   abstract class GenericCommand
    {

      public User.userRights accessLevel = User.userRights.Normal;

       public void run( User source, string destination, string[ ] args )
       {
           string command = this.GetType( ).ToString( );

           GlobalFunctions.Log( "Running command: " +command );

           if( source.AccessLevel < accessLevel )
           {
               Helpmebot6.irc.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "accessDenied" , "" ) );
               string[ ] aDArgs = { source.ToString( ) ,  command };
               Helpmebot6.irc.IrcPrivmsg( Configuration.Singleton( ).retrieveGlobalStringOption( "channelDebug" ) , Configuration.Singleton( ).GetMessage( "accessDeniedDebug" , aDArgs ) );
           }
           else
           {
               execute( source , destination , args );
           }
       }

      protected abstract void execute( User source , string destination , string[ ] args );
    }
}
