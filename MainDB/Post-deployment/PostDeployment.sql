/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

:r .\ChannelType_Inserts.sql

IF ('$(BuildConfiguration)' = 'Debug')
BEGIN
    PRINT '***** Creating Test Data for Debug configuration *****';
    :r .\PostDeployment\Inserts\Server_Inserts.sql
    :r .\PostDeployment\Inserts\User_Inserts.sql
    :r .\PostDeployment\Inserts\Birthday_Inserts.sql
    :r .\PostDeployment\Inserts\Reminder_Inserts.sql
    :r .\PostDeployment\Inserts\Greeting_Inserts.sql
    :r .\PostDeployment\Inserts\IdolGroup_Inserts.sql
    :r .\PostDeployment\Inserts\Role_Inserts.sql
    :r .\PostDeployment\Inserts\Channel_Inserts.sql
    :r .\PostDeployment\Inserts\ServerSettingChannel_Inserts.sql
    :r .\PostDeployment\Inserts\CustomCommand_Inserts.sql
    :r .\PostDeployment\Inserts\Idol_Inserts.sql
    :r .\PostDeployment\Inserts\IdolAlias_Inserts.sql
    :r .\PostDeployment\Inserts\IdolImage_Inserts.sql
    :r .\PostDeployment\Inserts\Keyword_Inserts.sql
    :r .\PostDeployment\Inserts\TwitchChannel_Inserts.sql
    :r .\PostDeployment\Inserts\UserBias_Inserts.sql
END
