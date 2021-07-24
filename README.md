# RoleDiscordBot
Discord bot created in Discord .NET for a medium sized discord server to help manage user's roles. The bot also provides additional feature such as deleting messages from channel and also provide an option for blacklisting certain words.   

Technology stack:
.NET CORE 3.0 

Worth to note libraries used:
Discord .NET
Polly
FluentAssertions

The bot uses the prefix  _

Most important commands:

[SETTINGS] 
= In order for the bot to fully function the bot requires some settings first = 

_SetBotCommandsChannel  #channel   sets the channel in which bot gives feedback. 


[ROLES]

_SetRolesChannel  #channel        sets the roles channel   

_AssignRole msgId|@role|:emote: 

_DeleteRoleAssignment @role @role2 @role3 

_DeleteAssignedMessage  msgID


[SLURS]
_AddSlur [word]       adds word to the list of forbidden words. If word is used by the user, bot deletes the messages and sends private message to the user informing them that their msg was unappropriate [msg content]. Mods are also notified of the msg in selected bot spam chanel (if it's set)

_RemoveSlur [word]         removes the word from the list if the word is there 

_ResetSlurs      deletes slurs file content


[DELETING MESSAGES]

_SetDefaultMsgsDeletionCounter [value]  sets the default messages counter for deleting. Value must be a number. 

_deletemsgs                          deletes the messages from the channel the command is typed it. It gets default number of messages. If the default number is not set (the previous command is never used), it removes 20 messages.

_deletemsgs  #channel    deletes messages from given channel (MENTIONED) using the default deletion counter. Same as before, if the counter is never set - removes 20.  It forbids providing more than one channel

_deletemsgs|counter   deletes messages from the channel the command is typed in. The number of messages are given in the counter. example  _deletemsgs |5

_deletemsgs #channel|counter deletes messages from the given, mentioned channel using the given counter



[DISPLAY]
_DisplayBadWords    displays current bad words list
_DisplayCurrentRolesChannel   displays the role channel that is currently set
_DisplayCurrentBotSpamChannel    displays the bot spam channel that is currently set
_DisplayCurrentMsgCounter   displays the current default counter for deleting msgs
_DisplayMsgsInRoleChannel   displays msgs info in current role channel
_DisplayRolesPairedToMsgsAndEmotes  displays current roles assignments 
_DisplayRolesNotPairedYet  displays a list of roles that are not paired with any emote and msg yet




[Stuff to add]
- more test coverage
- make SettingsService and FileHandlingService truly async
- Services registration automatically
